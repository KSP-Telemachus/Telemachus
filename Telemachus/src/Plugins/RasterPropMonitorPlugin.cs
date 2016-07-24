using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Telemachus.Plugins
{
    /// <summary>
    /// Allows reference types to be checked as a dictionary key via reference equality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IdentityEqualityComparer<T> : IEqualityComparer<T>
    where T : class
    {
        public int GetHashCode(T value)
        {
            return RuntimeHelpers.GetHashCode(value);
        }

        public bool Equals(T left, T right)
        {
            return left == right; // Reference identity comparison
        }
    }


    /// <summary>
    /// Handles interfacing with RasterPropMonitor
    /// </summary>
    class RasterPropMonitorPlugin : IMinimalTelemachusPlugin
    {
        protected class RPMVesselComputer
        {
            public VesselModule parent { get; private set; }
            private Type typ;

            public Func<string,object> ProcessVariable { get; private set; }
            //public Action FixedUpdate { get; private set; }
            public Action Update { get; private set; }

            public RPMVesselComputer(VesselModule parent)
            {
                this.parent = parent;
                this.typ = parent.GetType();

                var pV = typ.GetMethod("ProcessVariable");
                ProcessVariable = (x) => pV.Invoke(parent, new object[] { x, null });

                var fU = typ.GetMethod("DoFixedUpdate");
                var up = typ.GetMethod("Update");
                Update = () =>
                {
                    up.Invoke(parent, null);
                    fU.Invoke(parent, new object[] { true });
                };
            }
        }

        public string[] Commands { get { return new[] { "rpm.available", "rpm.*" }; } }

        Dictionary<VesselModule, RPMVesselComputer> rpmComputers = new Dictionary<VesselModule, RPMVesselComputer>(new IdentityEqualityComparer<VesselModule>());

        public Func<Vessel, string[], object> GetAPIHandler(string API)
        {
            if (API == "rpm.available") return (v, a) => { return FindRPMModule(v) != null; };

            return (vessel, args) => {
                var module = FindRPMModule(vessel);
                if (module != null) {
                    module.Update();
                    return module.ProcessVariable(API.Substring(4));
                }
                return null;
            };
        }
        
        /// <summary>
        /// Scans all attached modules to a vessel, and returns the RasterPropMonitor vessel computer.
        /// </summary>
        /// <param name="vessel">The vessel to scan</param>
        /// <returns>The RPMVesselComputer proxy class, or null</returns>
        private RPMVesselComputer FindRPMModule(Vessel vessel)
        {
            if (vessel)
            {
                foreach (var vm in vessel.GetComponents<VesselModule>())
                {
                    if (vm.GetType().Name == "RPMVesselComputer")
                    {
                        RPMVesselComputer cmp;
                        if (rpmComputers.TryGetValue(vm, out cmp))
                        {
                            return cmp;
                        }
                        cmp = new RPMVesselComputer(vm);
                        rpmComputers[vm] = cmp;
                        return cmp;
                    }
                }
            }
            return null;
        }
    }
}
