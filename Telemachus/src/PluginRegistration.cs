using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Telemachus
{
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

        public static Action Register(object toRegister)
        {
            if (Manager == null) throw new NullReferenceException("Telemachus Plugin infrastructure not yet intialised!");

            return Manager.Register(toRegister);
        }
    }

    public interface IMinimalTelemachusPlugin
    {
        /// <summary>
        /// Gets a list of commands that the plugin responds to. This can include wildcards (*).
        /// </summary>
        string[] Commands { get; }
        /// <summary>
        /// Called to retrieve a delegate for calling a particular API, which 
        /// </summary>
        /// <param name="API"></param>
        /// <returns></returns>
        APIDelegate GetAPIHandler(string API);
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
        }

        // List of registered plugin instances
        private List<PluginHandler> registeredPlugins = new List<PluginHandler>();
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
        public Action Register(object toRegister)
        {
            var handler = new PluginHandler() { instance = toRegister };
            // Get a list of commands that this instance handles
            var commands = ReadCommandList(toRegister);
            // Get the plugin handler function
            var apiMethod = toRegister.GetType().GetMethod("GetAPIHandler", new Type[] { typeof(string) });
            if (apiMethod.ReturnType != typeof(APIDelegate))
                throw new ArgumentException("Telemachus could not find the API handler 'Func<Vessel, string[], object> GetAPIHandler(string)' on the provided interface");
            handler.apiHandler = (APIHandler)Delegate.CreateDelegate(typeof(APIHandler), toRegister, apiMethod);
            PluginLogger.print("Got plugin registration call for " + toRegister.GetType());

            // Make a simple hashset of basic commands
            handler.commands = new HashSet<string>(commands.Where(x => !x.Contains("*")));
            // Now, deal with any wildcard plugin strings by building a regex
            handler.regexCommands = commands
                .Where(x => x.Contains("*"))
                .Select(x => new Regex("^" + x.Replace(".", "\\.").Replace("*", ".*") + "$")).ToList();

            lock(_dataLock)
                registeredPlugins.Add(handler);

            // Return a method to call to deregister this from the plugin system.
            return () => { Deregister(handler); };
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

        private static string[] ReadCommandList(object pluginInstance)
        {
            var objType = pluginInstance.GetType();
            object returnValue = null;

            // Attempt to read from either a property, or a field, or a member function, named 'Commands'
            var prop = objType.GetProperty("Commands");
            var field = objType.GetField("Commands");
            var method = objType.GetMethod("Commands");
            if (prop != null)
            {
                returnValue = prop.GetValue(pluginInstance, null);
            }
            else if (field != null)
            {
                returnValue = field.GetValue(pluginInstance);
            }
            else if (method != null && method.ReturnType != typeof(void) && method.GetParameters().Length == 0)
            {
                returnValue = method.Invoke(pluginInstance, null);
            }
            // Did we read anything?
            if (returnValue == null) throw new ArgumentException("Telemachus could not read 'Commands' member from object " + pluginInstance.ToString());

            // Now, determine the type, and return the command list.
            if (returnValue is string)
            {
                return new string[] { (string)returnValue };
            }
            else if (returnValue is IEnumerable)
            {
                return ((IEnumerable)returnValue).OfType<string>().ToArray();
            }
            // Unrecognised format.
            throw new ArgumentException("Telemachus got unknown type '" + returnValue.GetType().ToString() + "' as Command list, expecting string[]");
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
    }
}
