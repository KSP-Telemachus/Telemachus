using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Telemachus
{
    using System.Reflection;
    using System.Text.RegularExpressions;
    // Aliases for the important return types. This is so that the code is actually readable.
    using APIDelegate = Func<Vessel, string[], object>;
    using APIHandler = Func<string, Func<Vessel, string[], object>>;

    /// <summary>
    /// The public-facing, easy-to-access static face of the Telemachus plugin system.
    /// </summary>
    static class PluginRegistration
    {
        public static PluginManager Manager { get; set; }

        public static void Register(object toRegister)
        {
            if (Manager == null) throw new NullReferenceException("Telemachus Plugin infrastructure not yet intialised!");
            Manager.Register(toRegister);
        }
    }

    /// <summary>A minimal interface that any instances being registered must match</summary>
    public interface IMinimalTelemachusPlugin
    {
        /// <summary>
        /// Gets a list of commands that the plugin responds to. This can include wildcards (*).
        /// </summary>
        string[] Commands { get; }
        /// <summary>
        /// Called to retrieve a delegate for calling a particular API
        /// </summary>
        /// <param name="API">The API string, excluding parameters</param>
        /// <returns>null if the API string is not handled, otherwise a delegate to evaluate the API</returns>
        Func<Vessel, string[], object> GetAPIHandler(string API);
    }

    /// <summary>An optional interface, which if plugin instances match, will be able to deregister themselves</summary>
    public interface IDeregisterableTelemachusPlugin : IMinimalTelemachusPlugin
    {
        /// <summary>An action delegate to deregister the instance from the Telemachus API</summary>
        Action Deregister { set; }
    }

    public class PluginManager
    {
        /// <summary>
        /// Records information about a particular plugin instance
        /// </summary>
        private class PluginHandler
        {
            public object instance = null;
            public HashSet<string> commands = new HashSet<string>();
            public List<Regex> regexCommands = new List<Regex>();
            public APIHandler apiHandler = null;
            // Set of handlers that we have already evaluated as coming from this plugin.
            // Used in case we want to remove the plugin and clear out API entries.
            public HashSet<string> known_handlers = new HashSet<string>();
            /// <summary>Has this interface been given the ability to deregister itself?</summary>
            public bool is_deregisterable = false;
        }

        // List of registered plugin instances
        private List<PluginHandler> registeredPlugins = new List<PluginHandler>();
        // List of API handlers that we already asked for
        private Dictionary<string, APIDelegate> handlers = new Dictionary<string, APIDelegate>();
        private object _dataLock = new object();

        public PluginManager()
        {
            // Register ourselves if there is no other manager
            if (PluginRegistration.Manager == null) PluginRegistration.Manager = this;
        }

        /// <summary>
        /// Registers a plugin API with Telemachus
        /// </summary>
        /// <param name="toRegister">An instance of a Plugin object, that conforms to the TelemachusPlugin interface. 
        /// NOTE: Does NOT have to be a physical instance of the interface.</param>
        /// <returns>An Action, calling of which Deregisters the plugin. This is disposeable.</returns>
        public void Register(object toRegister)
        {
            var pluginType = toRegister.GetType();
            // Must conform at least to the minimal interface
            if (!typeof(IMinimalTelemachusPlugin).IsAssignableFrom(pluginType) && !pluginType.DoesMatchInterfaceOf(typeof(IMinimalTelemachusPlugin)))
            {
                throw new ArgumentException("Object " + toRegister.GetType().ToString() + " does not conform to the minimal interface");
            }

            var handler = new PluginHandler() { instance = toRegister };
            // Get a list of commands that this instance handles
            var commands = ReadCommandList(toRegister);
            // Get the plugin handler function
            var apiMethod = toRegister.GetType().GetMethod("GetAPIHandler", new Type[] { typeof(string) });
            handler.apiHandler = (APIHandler)Delegate.CreateDelegate(typeof(APIHandler), toRegister, apiMethod);

            // Does it match the Deregistration? If so, pass it the deregistration method
            if (toRegister is IDeregisterableTelemachusPlugin || pluginType.DoesMatchInterfaceOf(typeof(IDeregisterableTelemachusPlugin)))
            {
                Action deregistration = () => Deregister(handler);
                pluginType.GetProperty("Deregister").SetValue(toRegister, deregistration, null);
                handler.is_deregisterable = true;
            }

            var optional_interfaces = new List<string>();
            if (handler.is_deregisterable) optional_interfaces.Add("Deregister");
            PluginLogger.print("Got plugin registration call for " + toRegister.GetType() + ".\n  Optional interfaces enabled: " + (optional_interfaces.Count == 0 ? "None" : string.Join(", ", optional_interfaces.ToArray())));
            
            // Make a simple hashset of basic commands
            handler.commands = new HashSet<string>(commands.Where(x => !x.Contains("*")));
            // Now, deal with any wildcard plugin strings by building a regex
            handler.regexCommands = commands
                .Where(x => x.Contains("*"))
                .Select(x => new Regex("^" + x.Replace(".", "\\.").Replace("*", ".*") + "$")).ToList();

            lock(_dataLock)
                registeredPlugins.Add(handler);
        }

        /// <summary>
        /// Deregisters an API plugin handler instance via handler reference
        /// </summary>
        /// <param name="handler">The plugin handler for the plugin</param>
        private void Deregister(PluginHandler handler)
        {
            PluginLogger.print("Removing registered plugin " + handler.instance.ToString());
            // Remove the handler caches
            lock(_dataLock)
            {
                foreach (var api in handler.known_handlers)
                {
                    handlers.Remove(api);
                }
                registeredPlugins.Remove(handler);
            }
        }

        /// <summary>
        /// Scans the registered plugins for one that handles the named API string
        /// </summary>
        /// <param name="APIname">The API string to handle, excluding any parameters</param>
        /// <returns>An API-processing delegate, or null if none were found</returns>
        public APIDelegate GetAPIDelegate(string APIname)
        {
            lock(_dataLock)
            {
                // Check our handler cache first.
                if (handlers.ContainsKey(APIname))
                {
                    return handlers[APIname];
                }
                foreach (var plugin in registeredPlugins)
                {
                    // Does this match the api at all?
                    if (plugin.commands.Contains(APIname) || plugin.regexCommands.Any(x => x.IsMatch(APIname)))
                    {
                        try
                        {
                            var del = plugin.apiHandler(APIname);
                            if (del != null)
                            {
                                // Add to the handling cache and the plugin response list
                                handlers[APIname] = del;
                                plugin.known_handlers.Add(APIname);
                                return del;
                            }
                        }
                        catch (Exception ex)
                        {
                            PluginLogger.print("Got Exception processing plugin " + plugin.instance.ToString() + ", " + ex.ToString());
                        }
                    }
                }
            }
            return null;
        }
        
        private static string[] ReadCommandList(object pluginInstance)
        {
            var objType = pluginInstance.GetType();
            // Attempt to read from either a property, or a field, or a member function, named 'Commands'
            var prop = objType.GetProperty("Commands");
            var returnValue = (string[])prop.GetValue(pluginInstance, null);
            // Did we read anything?
            if (returnValue == null) throw new NullReferenceException("Telemachus could not read 'Commands' member from object " + pluginInstance.ToString());
            return returnValue;
        }
    }

    /// <summary>Contains extension methods for checking interface equivalence</summary>
    internal static class TypeCheckingExtensions
    {
        /// <summary>
        /// Tests two lists of parameters for length and type equivalence
        /// </summary>
        public static bool IsEquivalentTo(this ParameterInfo[] first, ParameterInfo[] second)
        {
            if (first.Length != second.Length) return false;
            for (int i = 0; i < first.Length; ++i)
            {
                if (first[i].ParameterType != second[i].ParameterType) return false;
            }
            return true;
        }

        /// <summary>
        /// Tests two MethodInfo's for return, public and parameter list equivalence
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool IsEquivalentTo(this MethodInfo first, MethodInfo second)
        {
            if (first.ReturnType != second.ReturnType) return false;
            if (first.IsPublic != second.IsPublic) return false;
            if (!first.GetParameters().IsEquivalentTo(second.GetParameters())) return false;
            return true;
        }

        /// <summary>Tests if a property is the superset of another one e.g. the same type and at least the same accessors</summary>
        /// <param name="target">The property to test. This can have more accessible accessors than the baseline.</param>
        /// <param name="baseline">The baseline, which specifies the type and minimum access levels.</param>
        public static bool IsSupersetOf(this PropertyInfo target, PropertyInfo baseline)
        {
            // Make sure the return value is the same
            if (target.PropertyType != baseline.PropertyType) return false;
            // Make sure that if there is a get/set method on the baseline, the target posesses it
            var canRead = baseline.GetGetMethod() != null;
            var canWrite = baseline.GetSetMethod() != null;
            if (canRead && target.GetGetMethod() == null) return false;
            if (canWrite && target.GetSetMethod() == null) return false;
            return true;
        }

        /// <summary>
        /// Tests all members of the target against a supplied interface to determine if it implements it 'In spirit' if not physically.
        /// </summary>
        /// <param name="target">The type to test</param>
        /// <param name="thisInterface">The interface to test against</param>
        /// <returns></returns>
        public static bool DoesMatchInterfaceOf(this Type target, Type thisInterface)
        {
            foreach (var ifcMember in thisInterface.GetMembers())
            {
                bool found = false;

                // Look for this member on the type. Check all with the same name.
                foreach (var tgtMember in target.GetMember(ifcMember.Name))
                {
                    if (tgtMember.MemberType != ifcMember.MemberType) continue;

                    if (tgtMember.MemberType == MemberTypes.Property)
                    {
                        var tgtProp = (PropertyInfo)tgtMember;
                        var ifcProp = (PropertyInfo)ifcMember;
                        found = tgtProp.IsSupersetOf(ifcProp);
                    }
                    else if (tgtMember.MemberType == MemberTypes.Method)
                    {
                        var tgtMethod = (MethodInfo)tgtMember;
                        var ifcMethod = (MethodInfo)ifcMember;
                        found = tgtMethod.IsEquivalentTo(ifcMethod);
                    }
                    else
                    {
                        throw new NotImplementedException("Cannot handle non-member/non-property interface compatability");
                    }
                    if (found) break;
                }
                if (!found) return false;
            }
            // If here, we passed all the tests. We found a match.
            return true;
        }
    }

}
