using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telemachus.Plugins
{
    /// <summary>
    /// Handles interfacing with RasterPropMonitor
    /// </summary>
    class RasterPropMonitorPlugin : IMinimalTelemachusPlugin
    {
        public string[] Commands { get { return new[] { "rpm.available", "rpm.*" }; } }

        public Func<Vessel, string[], object> GetAPIHandler(string API)
        {
            if (API == "rpm.available") return (v, a) => { return true; };

            return (vessel, args) => {
                var module = FindRPMModule(vessel);
                if (module) { return ReadRPMString(module, API.Substring(4)); }
                return null;
            };
        }
        
        /// <summary>
        /// Scans all attached modules to a vessel, and returns the RasterPropMonitor vessel computer
        /// </summary>
        /// <param name="vessel">The vessel to scan</param>
        /// <returns>The RPMVesselComputer VesselModule, or null</returns>
        private VesselModule FindRPMModule(Vessel vessel)
        {
            if (vessel)
            {
                foreach (var vm in vessel.GetComponents<VesselModule>())
                {
                    if (vm.GetType().Name == "RPMVesselComputer")
                    {
                        return vm;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Evaluates an API string using the VesselModule representing the RPMVesselComputer
        /// </summary>
        /// <param name="rpmComputer">The RPMVesselComputer VesselModule</param>
        /// <param name="apiString">The RPM API-string to evaluate</param>
        /// <returns>Whatever the RPM API returns</returns>
        private object ReadRPMString(VesselModule rpmComputer, string apiString)
        {
            var methInfo = rpmComputer.GetType().GetMethod("ProcessVariable");
            if (methInfo == null) {
                throw new FieldAccessException("Could not access variable processing field on RPM. Wrong version?");
            }
            var ret = methInfo.Invoke(rpmComputer, new object[] { apiString, null });
            return ret;
        }

    }
}
