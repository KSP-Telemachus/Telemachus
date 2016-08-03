using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telemachus.CameraSnapshots
{
    class RasterPropMonitorCamera : PartModule
    {
        

        public string fieldValues()
        {
            string val = "";
            foreach(BaseField field in Fields)
            {
                val += field.name + " : " + field.originalValue + " || ";
            }

            return val;
        }

        public string partModules()
        {
            string val = "";
            foreach (PartModule module in part.Modules)
            {
                val += module.moduleName + " ||" ;
            }

            return val;
        }

        public string getRPMFields()
        {
            string val = "";
            PartModule rpmModule = null;

            foreach (PartModule module in part.Modules)
            {
                if(module.moduleName == "JSIExternalCameraSelector")
                {
                    rpmModule = module;
                }
            }

            if(rpmModule == null)
            {
                return "NA";
            }

            foreach (BaseField field in rpmModule.Fields)
            {
                val += field.name + " : " + field.originalValue + " || ";
            }

            return val;
        }

        public void DebugInfo()
        {
            PluginLogger.debug("RPM CAMERA LOADED: " + part.name + " ;" + " ; POS: " + part.transform.position + " RPM FIELDS: " + getRPMFields() );
        }

        public override void OnStart(PartModule.StartState state)
        {
            DebugInfo();
        }

        public void Update()
        {
            DebugInfo();
        }
    }
}
