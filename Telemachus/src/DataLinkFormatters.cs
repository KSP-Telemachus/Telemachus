//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Telemachus
{
    public interface DataSourceResultFormatter
    {
        /// Prepares an object for serialization e.g. transforms it into any special basic representation
        object prepareForSerialization(object input);
    }

    public abstract class FormatterProvider
    {
        public DataSourceResultFormatter SensorModuleList { get; set; }
        public DataSourceResultFormatter ResourceList { get; set; }
        public DataSourceResultFormatter CurrentResourceList { get; set; }
        public DataSourceResultFormatter ActiveResourceList { get; set; }
        public DataSourceResultFormatter MaxResourceList { get; set; }
        public DataSourceResultFormatter MaxCurrentResourceList { get; set; }
        public DataSourceResultFormatter MechJebSimulation { get; set; }
        public DataSourceResultFormatter APIEntry { get; set; }
        public DataSourceResultFormatter Vector3d { get; set; }
        public DataSourceResultFormatter Default { get; set; }
        public DataSourceResultFormatter StringArray { get; set; }
    }

    public class JSONFormatterProvider : FormatterProvider
    {
        private static JSONFormatterProvider instance;
        public static JSONFormatterProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new JSONFormatterProvider();
                }
                return instance;
            }
        }

        private JSONFormatterProvider()
        {
            SensorModuleList = new SensorModuleListJSONFormatter();
            ResourceList = new ResourceListJSONFormatter();
            MaxResourceList = new MaxResourceListJSONFormatter();
            ActiveResourceList = new ActiveResourceListJSONFormatter();
            CurrentResourceList = new CurrentResourceListJSONFormatter();
            MaxCurrentResourceList = new ActiveResourceTotalListJSONFormatter();
            APIEntry = new APIEntryJSONFormatter();
            MechJebSimulation = new MechJebSimulationJSONFormatter();
            Vector3d = new Vector3dJSONFormatter();
            StringArray = new APIEntryStringArrayFormatter();
            Default = new DefaultJSONFormatter();
        }

        public abstract class JSONFormatter : DataSourceResultFormatter
        {
            public abstract object prepareForSerialization(object input);
        }

        public class DefaultJSONFormatter : JSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                return input;
            }
        }

        public class Vector3dJSONFormatter : JSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                var vec = (Vector3d)input;
                return new[] { vec.x, vec.y, vec.z };
            }
        }

        public class APIEntryJSONFormatter : JSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                var apiList = input as List<APIEntry>;
                var apiData = new List<Dictionary<string, object>>();

                foreach (var api in apiList)
                {
                    var apiDict = new Dictionary<string, object>();
                    apiDict["apistring"] = api.APIString;
                    apiDict["name"] = api.name;
                    apiDict["units"] = api.units.ToString();
                    apiDict["plotable"] = api.plotable;
                    apiData.Add(apiDict);
                }

                return apiData;
            }
        }

        public class APIEntryStringArrayFormatter : JSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                // Looks like this is a list of string... this should be handled fine by serialization
                return input;
            }
        }

        public class ResourceListJSONFormatter : JSONFormatter
        {
            /// Generically sums resources with a particular condition
            /// <typeparam name="T"></typeparam>
            /// <param name="resources">The list of resources to sum</param>
            /// <param name="valueAccessor">A lambda to access the resource value of interest</param>
            /// <param name="conditionForInclusion">A condition that the resource entry has to pass</param>
            /// <returns>The sum of resources, or -1 if there are no resources.</returns>
            protected object SumResources<T>(IList<T> resources, Func<T, double> valueAccessor, Func<T, bool> conditionForInclusion = null)
            {
                if (resources.Count == 0) return -1;
                double result = 0;
                foreach (var entry in resources)
                {
                    if (conditionForInclusion != null && !conditionForInclusion(entry))
                    {
                        // Skip if we don't pass the condition
                        continue;
                    }
                    result += valueAccessor(entry);
                }
                return result;
            }

            public override object prepareForSerialization(object input)
            {
                return SumResources((List<PartResource>)input, x => x.amount);
            }
        }

        public class CurrentResourceListJSONFormatter : ResourceListJSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                return SumResources((List<PartResource>)input,
                    x => x.amount,
                    x => x.part.inStageIndex == Staging.CurrentStage);
            }
        }


        public class ActiveResourceListJSONFormatter : ResourceListJSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                return SumResources((List<Vessel.ActiveResource>)input,
                    x => x.amount);
            }
        }

        public class ActiveResourceTotalListJSONFormatter : ResourceListJSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                return SumResources((List<Vessel.ActiveResource>)input,
                    x => x.maxAmount);
            }
        }

        public class MaxResourceListJSONFormatter : ResourceListJSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                return SumResources((List<PartResource>)input,
                    x => x.maxAmount);
            }
        }

        public class SensorModuleListJSONFormatter : JSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                List<ModuleEnviroSensor> sensors = (List<ModuleEnviroSensor>)input;
                var sensorValues = new List<float>();
                var sensorNames = new List<string>();

                foreach (var sensor in sensors)
                {
                    // Read a value for this sensor
                    if (!sensor.isEnabled)
                    {
                        sensorValues.Add(0);
                    } else
                    {
                        float f = 0;
                        try {
                            // Try to read the sensor as a float by grabbing the start
                            var numberOnly = Regex.Match(sensor.readoutInfo, "^[-+]?[0-9]*\\.?[0-9]*([eE][-+]?[0-9]+)?").Value;
                            float.TryParse(numberOnly, out f);
                        } catch
                        {
                            f = 0;
                        }
                        sensorValues.Add(f);
                    }

                    // Read a partname for the sensor
                    try
                    {
                        var partName = (sensor.isEnabled ? "" : "Inactive ") + sensor.part.parent.name;
                        sensorNames.Add(partName);
                    }
                    catch
                    {
                        sensorNames.Add("Unavailable");
                    }

                }
                if (sensorNames.Count == 0)
                {
                    sensorNames.Add("No Sensors of the Appropriate Type");
                    sensorValues.Add(0);
                }
                return new object[] { sensorNames, sensorValues };
            }
        }

        public class MechJebSimulationJSONFormatter : JSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                var simuluation = input as MechJebDataLinkHandler.MechJebSimulation;

                if(simuluation == null) { return null; }

                var simulationData = new Dictionary<string, object>();

                simulationData["atmo"] = this.convertStageList(simuluation.atmoStats);
                simulationData["vacuum"] = this.convertStageList(simuluation.vacuumStats);
                return simulationData;
            }

            private List<Dictionary<string, object>> convertStageList(List<MechJebDataLinkHandler.MechJebStageSimulationStats> stats)
            {
                var result = new List<Dictionary<string, object>>();

                foreach(var stat in stats)
                {
                    var stage = new Dictionary<string, object>();
                    stage["startMass"] = stat.startMass;
                    stage["endMass"] = stat.endMass;
                    stage["startThrust"] = stat.startThrust;
                    stage["maxAccel"] = stat.maxAccel;
                    stage["deltaTime"] = stat.deltaTime;
                    stage["deltaV"] = stat.deltaV;
                    stage["resourceMass"] = stat.resourceMass;
                    stage["isp"] = stat.isp;
                    stage["stagedMass"] = stat.stagedMass;

                    result.Add(stage);
                }

                return result;
            }
        }
    }
}
