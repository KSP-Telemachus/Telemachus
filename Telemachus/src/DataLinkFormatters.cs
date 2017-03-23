//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KSP.UI.Screens;

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
        public DataSourceResultFormatter OrbitPatchList { get; set; }
        public DataSourceResultFormatter ManeuverNode { get; set; }
        public DataSourceResultFormatter ManeuverNodeList { get; set; }
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
            ManeuverNode = new ManeuverNodeJSONFormatter();
            ManeuverNodeList = new ManeuverNodeListJSONFormatter();
            OrbitPatchList = new OrbitPatchListJSONFormatter();
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
                if(vec == null) { return null; }
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
                    x => x.part.inStageIndex == x.part.vessel.currentStage);
            }
        }


        public class ActiveResourceListJSONFormatter : ResourceListJSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                return SumResources((List<SimplifiedResource>)input,
                    x => x.amount);
            }
        }

        public class ActiveResourceTotalListJSONFormatter : ResourceListJSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                return SumResources((List<SimplifiedResource>)input,
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

        // the formatter that can convert a single maneuver node to JSON
        public class ManeuverNodeJSONFormatter : JSONFormatter
        {
            private Vector3dJSONFormatter vectorFormatter = new Vector3dJSONFormatter();
            private OrbitPatchListJSONFormatter orbitPatchesFormatter = new OrbitPatchListJSONFormatter();
            public override object prepareForSerialization(object input)
            {
                var maneuverNode = input as ManeuverNode;

                if (maneuverNode == null) { return null; }

                var maneuverNodeData = new Dictionary<string, object>();

                Orbit orbit = maneuverNode.nextPatch;

                maneuverNodeData["UT"] = maneuverNode.UT;
                maneuverNodeData["deltaV"] = vectorFormatter.prepareForSerialization(maneuverNode.DeltaV);
                maneuverNodeData["PeA"] = orbit.PeA;
                maneuverNodeData["ApA"] = orbit.ApA;
                maneuverNodeData["inclination"] = orbit.inclination;
                maneuverNodeData["eccentricity"] = orbit.eccentricity;
                maneuverNodeData["epoch"] = orbit.epoch;
                maneuverNodeData["period"] = orbit.period;
                maneuverNodeData["argumentOfPeriapsis"] = orbit.argumentOfPeriapsis;
                maneuverNodeData["sma"] = orbit.semiMajorAxis;
                maneuverNodeData["lan"] = orbit.LAN;
                maneuverNodeData["maae"] = orbit.meanAnomalyAtEpoch;
                maneuverNodeData["referenceBody"] = orbit.referenceBody.name;
                if (orbit.closestEncounterBody != null)
                {
                    maneuverNodeData["closestEncounterBody"] = orbit.closestEncounterBody.name;
                }
                else
                {
                    maneuverNodeData["closestEncounterBody"] = null;
                }

                List<Orbit> orbitPatches = OrbitPatches.getPatchesForOrbit(orbit);

                if(orbitPatches != null)
                {
                    maneuverNodeData["orbitPatches"] = orbitPatchesFormatter.prepareForSerialization(orbitPatches);
                }
                else
                {
                    maneuverNodeData["orbitPatches"] = null;
                }

                return maneuverNodeData;
            }
        }

        //the formatter that can convert a list of maneuver nodes to JSON
        public class ManeuverNodeListJSONFormatter : JSONFormatter
        {
            private ManeuverNodeJSONFormatter nodeFormatter = new ManeuverNodeJSONFormatter();
            public override object prepareForSerialization(object input)
            {
                var maneuverNodeList = input as List<ManeuverNode>;

                var maneuverNodeListData = new List<Dictionary<string, object>>();
                
                foreach(ManeuverNode node in maneuverNodeList)
                {
                    maneuverNodeListData.Add((Dictionary<string, object>)nodeFormatter.prepareForSerialization(node));
                }

                return maneuverNodeListData;
            }
        }

        // the formatter that can convert a single maneuver node to JSON
        public class OrbitPatchJSONFormatter : JSONFormatter
        {
            public override object prepareForSerialization(object input)
            {
                Orbit orbit = input as Orbit;
                if (orbit == null) { return null; }

                var orbitPatchData = new Dictionary<string, object>();
                
                orbitPatchData["startUT"] = orbit.StartUT;
                orbitPatchData["endUT"] = orbit.EndUT;
                orbitPatchData["patchStartTransition"] = orbit.patchStartTransition.ToString();
                orbitPatchData["patchEndTransition"] = orbit.patchEndTransition.ToString();
                orbitPatchData["PeA"] = orbit.PeA;
                orbitPatchData["ApA"] = orbit.ApA;
                orbitPatchData["inclination"] = orbit.inclination;
                orbitPatchData["eccentricAnomaly"] = orbit.eccentricity;
                orbitPatchData["eccentricity"] = orbit.eccentricity;
                orbitPatchData["epoch"] = orbit.epoch;
                orbitPatchData["period"] = orbit.period;
                orbitPatchData["argumentOfPeriapsis"] = orbit.argumentOfPeriapsis;
                orbitPatchData["sma"] = orbit.semiMajorAxis;
                orbitPatchData["lan"] = orbit.LAN;
                orbitPatchData["maae"] = orbit.meanAnomalyAtEpoch;
                orbitPatchData["referenceBody"] = orbit.referenceBody.name;
                orbitPatchData["semiLatusRectum"] = orbit.semiLatusRectum;
                orbitPatchData["semiMinorAxis"] = orbit.semiMinorAxis;
                if (orbit.closestEncounterBody != null)
                {
                    orbitPatchData["closestEncounterBody"] = orbit.closestEncounterBody.name;
                }
                else
                {
                    orbitPatchData["closestEncounterBody"] = null;
                }

                return orbitPatchData;
            }
        }

        //the formatter that can convert a list of maneuver nodes to JSON
        public class OrbitPatchListJSONFormatter : JSONFormatter
        {
            private OrbitPatchJSONFormatter orbitFormatter = new OrbitPatchJSONFormatter();
            public override object prepareForSerialization(object input)
            {
                var orbitPatchList = input as List<Orbit>;

                var orbitPatchListData = new List<Dictionary<string, object>>();

                if(orbitPatchList == null) { return orbitPatchListData; }

                foreach (Orbit orbit in orbitPatchList)
                {
                    orbitPatchListData.Add((Dictionary<string, object>)orbitFormatter.prepareForSerialization(orbit));
                }

                return orbitPatchListData;
            }
        }
    }
}
