//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections;
using UnityEngine;
using KSP.UI.Screens;
using KSP.UI.Util;

namespace Telemachus
{
    public class MechJebDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public MechJebDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return reflectOff(dataSources); }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                    return predictFailure(dataSources.vessel);
                },
               "mj.smartassoff", "Smart ASS Off", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.forward, "MANEUVER_NODE"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.node", "Node", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.forward, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.prograde", "Prograde", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.back, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.retrograde", "Retrograde", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.left, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.normalplus", "Normal Plus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.right, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.normalminus", "Normal Minus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.up, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.radialplus", "Radial Plus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.down, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.radialminus", "Radial Minus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return (FlightGlobals.fetch.VesselTarget != null ? reflectAttitudeTo(dataSources, Vector3d.forward, "TARGET") : false); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.targetplus", "Target Plus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return (FlightGlobals.fetch.VesselTarget != null ? reflectAttitudeTo(dataSources, Vector3d.back, "TARGET") : false); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.targetminus", "Target Minus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return (FlightGlobals.fetch.VesselTarget != null ? reflectAttitudeTo(dataSources, Vector3d.forward, "RELATIVE_VELOCITY") : false); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.relativeplus", "Relative Plus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return (FlightGlobals.fetch.VesselTarget != null ? reflectAttitudeTo(dataSources, Vector3d.back, "RELATIVE_VELOCITY") : false); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.relativeminus", "Relative Minus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return (FlightGlobals.fetch.VesselTarget != null ? reflectAttitudeTo(dataSources, Vector3d.forward, "TARGET_ORIENTATION") : false); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.parallelplus", "Parallel Plus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return (FlightGlobals.fetch.VesselTarget != null ? reflectAttitudeTo(dataSources, Vector3d.back, "TARGET_ORIENTATION") : false); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.parallelminus", "Parallel Minus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { return surface(dataSources); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver);
                    return predictFailure(dataSources.vessel);
                },
               "mj.surface", "Surface [float heading, float pitch]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) =>
                        {
                            return reflectAttitudeTo(dataSources, double.Parse(dataSources.args[0]),
                                double.Parse(dataSources.args[1]), double.Parse(dataSources.args[2])
                                );
                        }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                    return predictFailure(dataSources.vessel);
                },
               "mj.surface2", "Surface [double heading, double pitch, double roll]", formatters.Default));

            registerAPI(new APIEntry(
                dataSources => {
                    PluginLogger.debug("Start GET");
                    return getStagingInfo(dataSources);
                },
                "mj.stagingInfo", "Staging Info [object stagingInfo]",
                formatters.MechJebSimulation, APIEntry.UnitType.UNITLESS));
        }

        #endregion

        #region Flight Control

        private bool surface(DataSources dataSources)
        {
            Quaternion r = Quaternion.AngleAxis(float.Parse(dataSources.args[0]), Vector3.up) * Quaternion.AngleAxis(-float.Parse(dataSources.args[1]), Vector3.right);
            return reflectAttitudeTo(dataSources, r * Vector3d.forward, "SURFACE_NORTH");
        }

        private bool reflectOff(DataSources dataSources)
        {
            object attitude = null;
            Type attitudeType = getAttitudeType(dataSources, ref attitude);
            if (attitudeType != null)
            {
                MethodInfo methodInfo = attitudeType.GetMethod("attitudeDeactivate");
                methodInfo.Invoke(attitude, new object[] { });
                return true;
            }

            return false;
        }

        private bool reflectAttitudeTo(DataSources dataSources, Vector3d v, string reference)
        {
            object attitude = null;

            Type attitudeType = getAttitudeType(dataSources, ref attitude);
            if (attitudeType != null)
            {
                Type attitudeReferenceType = attitude.GetType().GetProperty("attitudeReference",
                    BindingFlags.Public | BindingFlags.Instance).GetValue(attitude, null).GetType();


                MethodInfo methodInfo = attitudeType.GetMethod("attitudeTo", new[] { typeof(Vector3d), 
                            attitudeReferenceType, typeof(object) });

                methodInfo.Invoke(attitude, new object[] { v, Enum.Parse(attitudeReferenceType, reference), this });

                return true;
            }

            return false;
        }

        private bool reflectAttitudeTo(DataSources dataSources, double heading, double pitch, double roll)
        {
            object attitude = null;

            Type attitudeType = getAttitudeType(dataSources, ref attitude);
            if (attitudeType != null)
            {
                MethodInfo methodInfo = attitudeType.GetMethod("attitudeTo", new[] { typeof(double),
                    typeof(double),typeof(double), typeof(object) });

                methodInfo.Invoke(attitude, new object[] { heading, pitch, roll, this });

                return true;
            }

            return false;
        }

        private Type getAttitudeType(DataSources dataSources, ref object attitude)
        {
            PartModule mechJebCore = findMechJeb(dataSources.vessel);

            if (mechJebCore == null)
            {
                PluginLogger.debug("No Mechjeb part installed.");
                return null;
            }
            else
            {
                try
                {
                    PluginLogger.debug("Mechjeb part installed, reflecting.");
                    Type mechJebCoreType = mechJebCore.GetType();
                    FieldInfo attitudeField = mechJebCoreType.GetField("attitude", BindingFlags.Public | BindingFlags.Instance);
                    attitude = attitudeField.GetValue(mechJebCore);

                    Type attitudeReferenceType = attitude.GetType().GetProperty("attitudeReference",
                        BindingFlags.Public | BindingFlags.Instance).GetValue(attitude, null).GetType();

                    return attitude.GetType();
                }
                catch (Exception e)
                {
                    PluginLogger.debug(e.Message + " " + e.StackTrace);
                }

                return null;
            }
        }

        private MechJebSimulation getStagingInfo(DataSources dataSources)
        {
            object stagingInfo = null;

            PluginLogger.debug("Grabbing staging info");
            Type stagingInfoType = getStagingInfoType(dataSources, ref stagingInfo);

            MechJebSimulation stats = new MechJebSimulation();

            if (stagingInfo != null)
            {
                PluginLogger.debug("Found the staging type, getting the staging info");

                stats.convertMechJebData(stagingInfoType, stagingInfo);

                return stats;
            }

            PluginLogger.debug("Could not get staging info");
            return null;
        }

        private Type getStagingInfoType(DataSources dataSources, ref object stagingInfo)
        {
            PluginLogger.debug("Finding part for staging");
            PartModule mechJebCore = findMechJeb(dataSources.vessel);

            if (mechJebCore == null)
            {
                PluginLogger.debug("No Mechjeb part installed.");
                return null;
            }
            else
            {
                try
                {
                    PluginLogger.debug("Mechjeb part installed, reflecting.");
                    Type mechJebCoreType = mechJebCore.GetType();
                    PluginLogger.debug("Trying to get computermodule Method info");
                    MethodInfo mechJebCoreGetComputerModuleMethodInfo = mechJebCoreType.GetMethod("GetComputerModule", new[] {typeof(string) } );
                    PluginLogger.debug("Trying to get calling computer method");
                    stagingInfo = mechJebCoreGetComputerModuleMethodInfo.Invoke(mechJebCore, new object[] { "MechJebModuleStageStats" });

                    PluginLogger.debug("Staging Info Type: " + stagingInfo.GetType().Name);
                    PluginLogger.print("Staging Info Type: " + stagingInfo.GetType().Name);


                    return stagingInfo.GetType();
                }
                catch (Exception e)
                {
                    PluginLogger.debug(e.Message + " " + e.StackTrace);
                }

                return null;
            }
        }

        private static int predictFailure(Vessel vessel)
        {

            int pause = PausedDataLinkHandler.partPaused();

            if (pause > 0)
            {
                return pause;
            }

            if (findMechJeb(vessel) == null)
            {
                return 5;
            }

            return 0;
        }

        private static PartModule findMechJeb(Vessel vessel)
        {
            try
            {
                List<Part> pl = vessel.parts.FindAll(p => p.Modules.Contains("MechJebCore"));

                foreach (PartModule m in pl[0].Modules)
                {
                    if (m.GetType().Name.Equals("MechJebCore"))
                    {
                        return m;
                    }
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message + " " + e.StackTrace);
            }

            return null;
        }

        #endregion

        #region DataLinkHandler

        protected override int pausedHandler()
        {
            return PausedDataLinkHandler.partPaused();
        }

        #endregion

        #region Data Structures
        public class MechJebSimulation
        {
            public List<MechJebStageSimulationStats> vacuumStats = new List<MechJebStageSimulationStats>();
            public List<MechJebStageSimulationStats> atmoStats = new List<MechJebStageSimulationStats>();

            public void convertMechJebData(Type stagingInfoType, object stagingInfo)
            {
                FieldInfo atmoStatsField = stagingInfoType.GetField("atmoStats", BindingFlags.Public | BindingFlags.Instance);
                PluginLogger.debug("Getting Atmo Stats");
                Array atmoStats = (Array)atmoStatsField.GetValue(stagingInfo);

                this.populateStats(atmoStats, this.atmoStats);

                FieldInfo vacStatsField = stagingInfoType.GetField("vacStats", BindingFlags.Public | BindingFlags.Instance);
                PluginLogger.debug("Getting Vac Stats");
                Array vacStats = (Array)vacStatsField.GetValue(stagingInfo);

                this.populateStats(vacStats, this.vacuumStats);

            }

            private void populateStats(Array mechJebStatsArray, List<MechJebStageSimulationStats> destinationStats) {
                PluginLogger.debug("Building stats object. Size: " + mechJebStatsArray.Length);
                foreach (object mechJebStat in mechJebStatsArray)
                {
                    MechJebStageSimulationStats stat = new MechJebStageSimulationStats();

                    //PluginLogger.debug("Getting Type");
                    Type statType = mechJebStat.GetType();
                    //PluginLogger.debug("Get start mass");
                    stat.startMass = (float)statType.GetField("startMass").GetValue(mechJebStat);

                    //PluginLogger.debug("Get end mass");
                    stat.endMass = (float)statType.GetField("endMass").GetValue(mechJebStat);

                    //PluginLogger.debug("Get start Thrust");
                    stat.startThrust = (float)statType.GetField("startThrust").GetValue(mechJebStat);
                    //PluginLogger.debug("startThrust: " + statType.GetField("startThrust").GetValue(mechJebStat).ToString());

                    //PluginLogger.debug("Get max Accel");
                    stat.maxAccel = (float)statType.GetField("maxAccel").GetValue(mechJebStat);

                    //PluginLogger.debug("Get deltaTime");
                    stat.deltaTime = (float)statType.GetField("deltaTime").GetValue(mechJebStat);

                    //PluginLogger.debug("Get deltaV");
                    stat.deltaV = (float)statType.GetField("deltaV").GetValue(mechJebStat);

                    //PluginLogger.debug("Get resourceMass");
                    stat.resourceMass = (float)statType.GetField("resourceMass").GetValue(mechJebStat);

                    //PluginLogger.debug("Get isp");
                    stat.isp = (float)statType.GetField("isp").GetValue(mechJebStat);
                    //PluginLogger.debug("ISP: " + stat.stagedMass.ToString());

                    //PluginLogger.debug("Get stagedMass");
                    stat.stagedMass = (float)statType.GetField("stagedMass").GetValue(mechJebStat);
                    //PluginLogger.debug("stagedMass: " + stat.stagedMass.ToString());

                    destinationStats.AddUnique(stat);

                    //PluginLogger.debug("Start Mass: " + stat.startMass.ToString());
                }
            }
        }

        public class MechJebStageSimulationStats
        {
            public float startMass;
            public float endMass;
            public float startThrust;
            public float maxAccel;
            public float deltaTime;
            public float deltaV;

            public float resourceMass;
            public float isp;
            public float stagedMass;
        }
        #endregion
    }

    public class FlyByWireDataLinkHandler : DataLinkHandler
    {
        #region Fields

        static float yaw = 0, pitch = 0, roll = 0, x = 0, y = 0, z = 0;
        static int on_attitude = 0;

        #endregion

        #region Initialisation

        public FlyByWireDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new ActionAPIEntry(
                dataSources => { yaw = checkFlightStateParameters(float.Parse(dataSources.args[0])); return 0; },
                "v.setYaw", "Yaw [float yaw]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources => { pitch = checkFlightStateParameters(float.Parse(dataSources.args[0])); return 0; },
                "v.setPitch", "Pitch [float pitch]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources => { roll = checkFlightStateParameters(float.Parse(dataSources.args[0])); return 0; },
                "v.setRoll", "Roll [float roll]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources => { on_attitude = int.Parse(dataSources.args[0]); return 0; },
                "v.setFbW", "Set Fly by Wire On or Off [int state]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    pitch = checkFlightStateParameters(float.Parse(dataSources.args[0]));
                    yaw = checkFlightStateParameters(float.Parse(dataSources.args[1]));
                    roll = checkFlightStateParameters(float.Parse(dataSources.args[2]));
                    x = checkFlightStateParameters(float.Parse(dataSources.args[3]));
                    y = checkFlightStateParameters(float.Parse(dataSources.args[4]));
                    z = checkFlightStateParameters(float.Parse(dataSources.args[5]));

                    return 0;
                },
                "v.setPitchYawRollXYZ", "Set pitch, yaw, roll, X, Y and Z [float pitch, yaw, roll, x, y, z]", formatters.Default));
                
            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    pitch = checkFlightStateParameters(float.Parse(dataSources.args[0]));
                    yaw = checkFlightStateParameters(float.Parse(dataSources.args[1]));
                    roll = checkFlightStateParameters(float.Parse(dataSources.args[2]));

                    return 0;
                },
                "v.setAttitude", "Set pitch, yaw, roll [float pitch, yaw, roll]", formatters.Default));
            
            registerAPI(new ActionAPIEntry(
                dataSources =>
                {   
                    x = checkFlightStateParameters(float.Parse(dataSources.args[0]));
                    y = checkFlightStateParameters(float.Parse(dataSources.args[1]));
                    z = checkFlightStateParameters(float.Parse(dataSources.args[2]));

                    return 0;
                },
                "v.setTranslation", "Set X, Y and Z [float x, y, z]", formatters.Default));
        }

        #endregion

        #region Methods

        public static void onFlyByWire(FlightCtrlState fcs)
        {
            if (on_attitude > 0)
            {
                fcs.yaw = yaw;
                fcs.pitch = pitch;
                fcs.roll = roll;
                fcs.X = x < 0 ? -1 : (x > 0 ? 1 : 0);
                fcs.Y = y < 0 ? -1 : (y > 0 ? 1 : 0);
                fcs.Z = z < 0 ? -1 : (z > 0 ? 1 : 0);
            }
        }

        public static void reset()
        {
            yaw = 0;
            pitch = 0;
            roll = 0;
            x = 0;
            y = 0;
            z = 0;
            on_attitude = 0;
        }

        private float checkFlightStateParameters(float f)
        {
            if (float.IsNaN(f))
            {
                f = 0;
            }

            return Mathf.Clamp(f, -1f, 1f);
        }

        #endregion

        #region DataLinkHandler

        protected override int pausedHandler()
        {
            return PausedDataLinkHandler.partPaused();
        }

        #endregion
    }

    public class FlightDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public FlightDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
						(x) => { StageManager.ActivateNextStage(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                    return predictFailure(dataSources.vessel);
                }, "f.stage", "Stage", formatters.Default));

            registerAPI(new ActionAPIEntry(
               dataSources =>
               {
                   TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                       (x) => { setThrottle(x); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                   return predictFailure(dataSources.vessel);
               },
                "f.setThrottle", "Set Throttle [float magnitude]", formatters.Default));

            registerAPI(new PlotableAPIEntry(
               dataSources =>
               {
                   float t = dataSources.vessel.ctrlState.mainThrottle;
                   return t;
               },
                "f.throttle", "Throttle", formatters.Default, APIEntry.UnitType.UNITLESS));

            registerAPI(new ActionAPIEntry(
               dataSources =>
               {
                   TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                       (x) => { throttleUp(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                   return predictFailure(dataSources.vessel);
               },
                "f.throttleUp", "Throttle Up", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { throttleZero(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                    return predictFailure(dataSources.vessel);
                },
                "f.throttleZero", "Throttle Zero", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { throttleFull(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                    return predictFailure(dataSources.vessel);
                },
                "f.throttleFull", "Throttle Full", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { throttleDown(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                    return predictFailure(dataSources.vessel);
                },
                "f.throttleDown", "Throttle Down", formatters.Default));

            registerAPI(new ActionAPIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.RCS),
                "f.rcs", "RCS [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.SAS),
                "f.sas", "SAS [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Light),
                "f.light", "Light [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Gear),
                "f.gear", "Gear [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Brakes),
                "f.brake", "Brake [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Abort),
                "f.abort", "Abort [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources => 
                {
                    bool state = dataSources.args.Count > 0 ? bool.Parse(dataSources.args[0]) : !FlightInputHandler.fetch.precisionMode;
                    
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => {

                            FlightInputHandler.fetch.precisionMode = state;
                            // Update the UI.
                            // MOARdV: In 1.1, this only affects the normal flight display,
                            // not the docking mode display.
                            var gauges = UnityEngine.Object.FindObjectOfType<KSP.UI.Screens.Flight.LinearControlGauges>();
                            if (gauges != null)
                            {
                                //JUtil.LogMessage(this, "{0} input gauge images", gauges.inputGaugeImages.Count);
                                for (int i = 0; i < gauges.inputGaugeImages.Count; ++i)
                                {
                                    gauges.inputGaugeImages[i].color = (state) ? XKCDColors.BrightCyan : XKCDColors.Orange;
                                }
                            }
                            return Convert.ToInt32(state);
                        }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver);
                        return predictFailure(dataSources.vessel);
                },
                "f.precisionControl", "Precision controls [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom01),
                "f.ag1", "Action Group 1 [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom02),
                "f.ag2", "Action Group 2 [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom03),
                "f.ag3", "Action Group 3 [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom04),
                "f.ag4", "Action Group 4 [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom05),
                "f.ag5", "Action Group 5 [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom06),
                "f.ag6", "Action Group 6 [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom07),
                "f.ag7", "Action Group 7 [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom08),
                "f.ag8", "Action Group 8 [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom09),
                "f.ag9", "Action Group 9 [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom10),
                "f.ag10", "Action Group 10 [optional bool on/off]", formatters.Default));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.ActionGroups[KSPActionGroup.RCS]; },
                "v.rcsValue", "Query RCS value", formatters.Default, APIEntry.UnitType.UNITLESS));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.ActionGroups[KSPActionGroup.SAS]; },
                "v.sasValue", "Query SAS value", formatters.Default, APIEntry.UnitType.UNITLESS));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.ActionGroups[KSPActionGroup.Light]; },
                "v.lightValue", "Query light value", formatters.Default, APIEntry.UnitType.UNITLESS));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.ActionGroups[KSPActionGroup.Brakes]; },
                "v.brakeValue", "Query brake value", formatters.Default, APIEntry.UnitType.UNITLESS));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.ActionGroups[KSPActionGroup.Gear]; },
                "v.gearValue", "Query gear value", formatters.Default, APIEntry.UnitType.UNITLESS));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightInputHandler.fetch.precisionMode; },
                "v.precisionControlValue", "Query precision controls value", formatters.Default, APIEntry.UnitType.UNITLESS));
        }

        private DataLinkHandler.APIDelegate buildActionGroupToggleDelayedLamda(KSPActionGroup actionGroup)
        {
            return dataSources =>
                {
                    if (dataSources.args.Count == 0)
                    {
                        TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                            (x) => { dataSources.vessel.ActionGroups.ToggleGroup(actionGroup); return 0d; }),
                            UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                    }
                    else
                    {
                        TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                            (x) => { dataSources.vessel.ActionGroups.SetGroup(actionGroup, bool.Parse(dataSources.args[0])); return 0d; }),
                            UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                    }
                };
        }

        #endregion

        #region DataLinkHandler

        protected override int pausedHandler()
        {
            return PausedDataLinkHandler.partPaused();
        }

        #endregion

        #region Flight Control

        private void throttleUp()
        {
            FlightInputHandler.state.mainThrottle += 0.1f;

            if (FlightInputHandler.state.mainThrottle > 1)
            {
                FlightInputHandler.state.mainThrottle = 1f;
            }
        }

        private void throttleDown()
        {
            FlightInputHandler.state.mainThrottle -= 0.1f;

            if (FlightInputHandler.state.mainThrottle < 0)
            {
                FlightInputHandler.state.mainThrottle = 0f;
            }
        }

        private void throttleZero()
        {
            FlightInputHandler.state.mainThrottle = 0f;
        }

        private void throttleFull()
        {
            FlightInputHandler.state.mainThrottle = 1f;
        }

        private void setThrottle(DataSources dataSources)
        {
            FlightInputHandler.state.mainThrottle = float.Parse(dataSources.args[0]);
        }

        private static int predictFailure(Vessel vessel)
        {
            return PausedDataLinkHandler.partPaused();
        }

        #endregion
    }

    public class TimeWarpDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public TimeWarpDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { TimeWarp.SetRate(int.Parse(x.args[0]), false); return 0d; }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return false;
                },
                "t.timeWarp", "Time Warp [int rate]", formatters.Default));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return Planetarium.GetUniversalTime(); },
                "t.universalTime", "Universal Time", formatters.Default, APIEntry.UnitType.DATE, true));

            registerAPI(new ActionAPIEntry(
               dataSources =>
               {
                   TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                       (x) => { FlightDriver.SetPause(true); return 0d; }),
                       UnityEngine.SendMessageOptions.DontRequireReceiver); return false;
               },
               "t.pause", "Pause game", formatters.Default));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) => { FlightDriver.SetPause(false); return 0d; }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return false;
                },
                "t.unpause", "Unpause game", formatters.Default));
        }

        #endregion
    }

    public class TargetDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public TargetDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetName() : "No Target Selected.";
                },
                "tar.name", "Target Name", formatters.Default, APIEntry.UnitType.STRING));

            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetType().ToString() : "";
                },
                "tar.type", "Target Type", formatters.Default, APIEntry.UnitType.STRING));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? Vector3.Distance(FlightGlobals.fetch.VesselTarget.GetTransform().position, dataSources.vessel.GetTransform().position) : 0; },
                "tar.distance", "Target Distance", formatters.Default, APIEntry.UnitType.DISTANCE));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? (FlightGlobals.fetch.VesselTarget.GetOrbit().GetVel() - dataSources.vessel.orbit.GetVel()).magnitude : 0; },
                "tar.o.relativeVelocity", "Target Relative Velocity", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().vel.magnitude : 0; },
                "tar.o.velocity", "Target Velocity", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().PeA : 0; },
                "tar.o.PeA", "Target Periapsis", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().ApA : 0; },
                "tar.o.ApA", "Target Apoapsis", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToAp : 0; },
                "tar.o.timeToAp", "Target Time to Apoapsis", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToPe : 0; },
                "tar.o.timeToPe", "Target Time to Periapsis", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().inclination : 0; },
                "tar.o.inclination", "Target Inclination", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().eccentricity : 0; },
                "tar.o.eccentricity", "Target Eccentricity", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().period : 0; },
                "tar.o.period", "Target Orbital Period", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().argumentOfPeriapsis : 0; },
                "tar.o.argumentOfPeriapsis", "Target Argument of Periapsis", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToTransition1 : 0; },
                "tar.o.timeToTransition1", "Target Time to Transition 1", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToTransition1 : 0; },
                "tar.o.timeToTransition2", "Target Time to Transition 2", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
               dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().semiMajorAxis : 0; },
               "tar.o.sma", "Target Semimajor Axis", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
               dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().LAN : 0; },
               "tar.o.lan", "Target Longitude of Ascending Node", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    if (FlightGlobals.fetch.VesselTarget == null) { return 0; }
                    Orbit orbit = FlightGlobals.fetch.VesselTarget.GetOrbit();
                    return orbit.getObtAtUT(0) / orbit.period * (2.0 * Math.PI);
                },
               "tar.o.maae", "Target Mean Anomaly at Epoch", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? Planetarium.GetUniversalTime() - FlightGlobals.fetch.VesselTarget.GetOrbit().ObT : 0; },
                "tar.o.timeOfPeriapsisPassage", "Target Time of Periapsis Passage", formatters.Default, APIEntry.UnitType.DATE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().TrueAnomalyAtUT(Planetarium.GetUniversalTime()) * (180.0 / Math.PI) : double.NaN; },
                "tar.o.trueAnomaly", "Target True Anomaly", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
               dataSources => { return FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().referenceBody.name : ""; },
               "tar.o.orbitingBody", "Target Orbiting Body", formatters.Default, APIEntry.UnitType.STRING));
            registerAPI(new APIEntry(
                dataSources =>
                {
                    if (FlightGlobals.fetch.VesselTarget == null)
                    {
                        return null;
                    }
                    return OrbitPatches.getPatchesForOrbit(FlightGlobals.fetch.VesselTarget.GetOrbit());
                },
                "tar.o.orbitPatches", "Detailed Orbit Patches Info [object orbitPatchInfo]",
                formatters.OrbitPatchList, APIEntry.UnitType.UNITLESS));

            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    if (FlightGlobals.fetch.VesselTarget == null) { return null; }
                    int index = int.Parse(dataSources.args[0]);
                    float ut = float.Parse(dataSources.args[1]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(FlightGlobals.fetch.VesselTarget.GetOrbit(), index);
                    if (orbitPatch == null) { return null; }
                    return orbitPatch.TrueAnomalyAtUT(ut);
                },
                "tar.o.trueAnomalyAtUTForOrbitPatch", "The orbit patch's True Anomaly at Universal Time [orbit patch index, universal time]", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    if (FlightGlobals.fetch.VesselTarget == null) { return null; }
                    int index = int.Parse(dataSources.args[0]);
                    float trueAnomaly = float.Parse(dataSources.args[1]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(FlightGlobals.fetch.VesselTarget.GetOrbit(), index);
                    if (orbitPatch == null) { return null; }

                    double now = Planetarium.GetUniversalTime();
                    return orbitPatch.GetUTforTrueAnomaly(trueAnomaly, now);
                },
                "tar.o.UTForTrueAnomalyForOrbitPatch", "The orbit patch's True Anomaly at Universal Time [orbit patch index, universal time]", formatters.Default, APIEntry.UnitType.DATE));
            registerAPI(new APIEntry(
                dataSources => {
                    if (FlightGlobals.fetch.VesselTarget == null) { return null; }
                    int index = int.Parse(dataSources.args[0]);
                    float trueAnomaly = float.Parse(dataSources.args[1]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(FlightGlobals.fetch.VesselTarget.GetOrbit(), index);
                    if (orbitPatch == null) { return null; }
                    return orbitPatch.getRelativePositionFromTrueAnomaly(trueAnomaly);
                },
                "tar.o.relativePositionAtTrueAnomalyForOrbitPatch", "The orbit patch's predicted displacement from the center of the main body at the given true anomaly [orbit patch index, true anomaly]", formatters.Vector3d, APIEntry.UnitType.UNITLESS));

            registerAPI(new APIEntry(
                dataSources => {
                    if (FlightGlobals.fetch.VesselTarget == null) { return null; }
                    int index = int.Parse(dataSources.args[0]);
                    double ut = double.Parse(dataSources.args[1]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(FlightGlobals.fetch.VesselTarget.GetOrbit(), index);
                    if (orbitPatch == null) { return null; }
                    return orbitPatch.getRelativePositionAtUT(ut);
                },
                "tar.o.relativePositionAtUTForOrbitPatch", "The orbit patch's predicted displacement from the center of the main body at the given universal time [orbit patch index, universal time]", formatters.Vector3d, APIEntry.UnitType.UNITLESS));
        }

        #endregion

        #region DataLinkHandler



        public override bool process(String API, out APIEntry result)
        {
            if (!base.process(API, out result))
            {
                if (API.StartsWith("tar."))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        #endregion
    }

    public class DockingDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        private static Vector3 orientationDeviation = new Vector3();
        private static Vector2 translationDeviation = new Vector3();

        public DockingDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    if (FlightGlobals.fetch.VesselTarget != null)
                    {
                        update();
                        return orientationDeviation.x;
                    }
                    else
                    {
                        return 0;
                    }
                },
                "dock.ax", "Docking x Angle", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    if (FlightGlobals.fetch.VesselTarget != null)
                    {
                        update();
                        return orientationDeviation.y;
                    }
                    else
                    {
                        return 0;
                    }
                },
                "dock.ay", "Relative Pitch Angle", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
               dataSources =>
               {
                   if (FlightGlobals.fetch.VesselTarget != null)
                   {
                       update();
                       return orientationDeviation.z;
                   }
                   else
                   {
                       return 0;
                   }
               },
               "dock.az", "Docking z Angle", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? (FlightGlobals.fetch.VesselTarget.GetTransform().position - dataSources.vessel.GetTransform().position).x : 0; },
                "dock.x", "Target x Distance", formatters.Default, APIEntry.UnitType.DISTANCE));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.fetch.VesselTarget != null ? (FlightGlobals.fetch.VesselTarget.GetTransform().position - dataSources.vessel.GetTransform().position).y : 0; },
                "dock.y", "Target y Distance", formatters.Default, APIEntry.UnitType.DISTANCE));
        }

        #endregion

        #region Methods

        //Borrowed from Docking Port Alignment Indicator by NavyFish
        private void update()
        {
            ModuleDockingNode targetPort = null;
            Transform selfTransform = FlightGlobals.ActiveVessel.ReferenceTransform;
            
            targetPort = FlightGlobals.fetch.VesselTarget as ModuleDockingNode;
            
            if (targetPort == null)
            {
                return;
            }
            
            Transform targetTransform = targetPort.transform;
            Vector3 targetPortOutVector;
            Vector3 targetPortRollReferenceVector;

            if (targetPort.part.name == "dockingPortLateral")
            {
                targetPortOutVector = -targetTransform.forward.normalized;
                targetPortRollReferenceVector = -targetTransform.up;
            }
            else
            {
                targetPortOutVector = targetTransform.up.normalized;
                targetPortRollReferenceVector = targetTransform.forward;
            }

            orientationDeviation.x = AngleAroundNormal(-targetPortOutVector, selfTransform.up, selfTransform.forward);
            orientationDeviation.y = AngleAroundNormal(-targetPortOutVector, selfTransform.up, -selfTransform.right);
            orientationDeviation.z = AngleAroundNormal(targetPortRollReferenceVector, selfTransform.forward, selfTransform.up);
            orientationDeviation.z = (orientationDeviation.z + 360) % 360;

            Vector3 targetToOwnship = selfTransform.position - targetTransform.position;

            translationDeviation.x = AngleAroundNormal(targetToOwnship, targetPortOutVector, selfTransform.forward);
            translationDeviation.y = AngleAroundNormal(targetToOwnship, targetPortOutVector, -selfTransform.right);
        }

        //return signed angle in relation to normal's 2d plane
        private float AngleAroundNormal(Vector3 a, Vector3 b, Vector3 up)
        {
            return AngleSigned(Vector3.Cross(up, a), Vector3.Cross(up, b), up);
        }

        //-180 to 180 angle
        private float AngleSigned(Vector3 v1, Vector3 v2, Vector3 up)
        {
            if (Vector3.Dot(Vector3.Cross(v1, v2), up) < 0) //greater than 90 i.e v1 left of v2
                return -Vector3.Angle(v1, v2);
            return Vector3.Angle(v1, v2);
        }

        #endregion
    }

    public class MapViewDataLinkHandler : DataLinkHandler
    {

        static float ut = 0, x = 0, y = 0, z = 0;
        static int maneuver_node_id = 0;
        #region Initialisation

        public MapViewDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                        (x) =>
                        {
                            if (MapView.MapIsEnabled)
                            { MapView.ExitMapView(); }
                            else { MapView.EnterMapView(); } return 0d;
                        }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return false;
                },
                "m.toggleMapView", " Toggle Map View", formatters.Default));

            registerAPI(new ActionAPIEntry(
               dataSources =>
               {
                   TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                       (x) =>
                       {
                           MapView.EnterMapView(); return 0d;
                       }),
                       UnityEngine.SendMessageOptions.DontRequireReceiver); return false;
               },
               "m.enterMapView", " Enter Map View", formatters.Default));

            registerAPI(new ActionAPIEntry(
              dataSources =>
              {
                  TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources.Clone(),
                      (x) =>
                      {
                          MapView.ExitMapView(); return 0d;
                      }),
                      UnityEngine.SendMessageOptions.DontRequireReceiver); return false;
              },
              "m.exitMapView", " Exit Map View", formatters.Default));

            registerAPI(new APIEntry(
                dataSources => {
                    PluginLogger.debug("Start GET");
                    return dataSources.vessel.patchedConicSolver.maneuverNodes;
                },
                "o.maneuverNodes", "Maneuver Nodes  [object maneuverNodes]",
                formatters.ManeuverNodeList, APIEntry.UnitType.UNITLESS));

            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    ManeuverNode node = getManueverNode(dataSources, int.Parse(dataSources.args[0]));
                    if (node == null) { return null; }

                    int index = int.Parse(dataSources.args[1]);
                    float ut = float.Parse(dataSources.args[2]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(node.nextPatch, index);
                    if (orbitPatch == null) { return null; }
                    return orbitPatch.TrueAnomalyAtUT(ut);
                },
                "o.maneuverNodes.trueAnomalyAtUTForManeuverNodesOrbitPatch", "For a maneuver node, The orbit patch's True Anomaly at Universal Time [int id, orbit patch index, universal time]", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    ManeuverNode node = getManueverNode(dataSources, int.Parse(dataSources.args[0]));
                    if (node == null) { return null; }

                    int index = int.Parse(dataSources.args[1]);
                    float trueAnomaly = float.Parse(dataSources.args[2]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(node.nextPatch, index);
                    if (orbitPatch == null) { return null; }

                    double now = Planetarium.GetUniversalTime();
                    return orbitPatch.GetUTforTrueAnomaly(trueAnomaly, now);
                },
                "o.maneuverNodes.UTForTrueAnomalyForManeuverNodesOrbitPatch", "For a maneuver node, The orbit patch's True Anomaly at Universal Time [int id, orbit patch index, universal time]", formatters.Default, APIEntry.UnitType.DATE));
            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    ManeuverNode node = getManueverNode(dataSources, int.Parse(dataSources.args[0]));
                    if (node == null) { return null; }

                    int index = int.Parse(dataSources.args[1]);
                    float trueAnomaly = float.Parse(dataSources.args[2]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(node.nextPatch, index);
                    if (orbitPatch == null) { return null; }
                    return orbitPatch.getRelativePositionFromTrueAnomaly(trueAnomaly);
                },
                "o.maneuverNodes.relativePositionAtTrueAnomalyForManeuverNodesOrbitPatch", "For a maneuver node, The orbit patch's predicted displacement from the center of the main body at the given true anomaly [int id, orbit patch index, true anomaly]", formatters.Vector3d, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    ManeuverNode node = getManueverNode(dataSources, int.Parse(dataSources.args[0]));
                    if (node == null) { return null; }

                    int index = int.Parse(dataSources.args[1]);
                    double ut = double.Parse(dataSources.args[2]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(node.nextPatch, index);
                    if (orbitPatch == null) { return null; }

                    return orbitPatch.getRelativePositionAtUT(ut);
                },
                "o.maneuverNodes.relativePositionAtUTForManeuverNodesOrbitPatch", "For a maneuver node, The orbit patch's predicted displacement from the center of the main body at the given universal time [int id, orbit patch index, universal time]", formatters.Vector3d, APIEntry.UnitType.UNITLESS));


            registerAPI(new ActionAPIEntry(
                dataSources =>
                {

                    ut = float.Parse(dataSources.args[0]);
                    ManeuverNode node = dataSources.vessel.patchedConicSolver.AddManeuverNode(ut);

                    x = float.Parse(dataSources.args[1]);
                    y = float.Parse(dataSources.args[2]);
                    z = float.Parse(dataSources.args[3]);

                    PluginLogger.debug("x: " + x + "y: " + y + "z: " + z);

                    Vector3d deltaV = new Vector3d(x,y,z);
                    node.OnGizmoUpdated(deltaV, ut);

                    return node;
                },
                "o.addManeuverNode", "Add a manuever based on a UT and DeltaV X, Y and Z [float ut, float x, y, z]", formatters.ManeuverNode));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    ManeuverNode node = getManueverNode(dataSources, int.Parse(dataSources.args[0]));
                    if (node == null) { return null; }

                            
                    ut = float.Parse(dataSources.args[1]);

                    x = float.Parse(dataSources.args[2]);
                    y = float.Parse(dataSources.args[3]);
                    z = float.Parse(dataSources.args[4]);

                    Vector3d deltaV = new Vector3d(x, y, z);
                    node.OnGizmoUpdated(deltaV, ut);
                    return node;
                        
                },
                "o.updateManeuverNode", "Set a manuever node's UT and DeltaV X, Y and Z [int id, float ut, float x, y, z]", formatters.ManeuverNode));

            registerAPI(new ActionAPIEntry(
                dataSources =>
                {
                    ManeuverNode node = getManueverNode(dataSources, int.Parse(dataSources.args[0]));
                    if (node == null) { return false; }

                    dataSources.vessel.patchedConicSolver.RemoveManeuverNode(node);
                    return true;
                },
                "o.removeManeuverNode", "Remove a manuever node [int id]", formatters.Default));
        }



        private ManeuverNode getManueverNode(DataSources datasources, int id)
        {
            PluginLogger.debug("GETTING NODE");
            //return null if the count is less than the ID or the ID is negative
            if(datasources.vessel.patchedConicSolver.maneuverNodes.Count <= id || id < 0)
            {
                return null;
            }

            PluginLogger.debug("FINDING THE RIGHT NODE. ID: " + id);
            ManeuverNode[] nodes = datasources.vessel.patchedConicSolver.maneuverNodes.ToArray();
            return (ManeuverNode) nodes.GetValue(id);
        }

        private float checkFlightStateParameters(float f)
        {
            if (float.IsNaN(f))
            {
                f = 0;
            }

            return Mathf.Clamp(f, -1f, 1f);
        }

        #endregion
    }

    public class BodyDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public BodyDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].name; },
                "b.name", "Body Name [body id]", formatters.Default, APIEntry.UnitType.STRING));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].atmosphereDepth; },
                "b.maxAtmosphere", "Body Atmosphere Depth [body id]", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].Radius; },
                "b.radius", "Body Radius [body id]", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].atmosphereContainsOxygen; },
                "b.atmosphereContainsOxygen", "Atmosphere contains oxygen [body id]", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].sphereOfInfluence; },
                "b.soi", "Body Sphere of Influence [body id]", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].rotationPeriod; },
                "b.rotationPeriod", "Rotation Period [body id]", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].tidallyLocked; },
                "b.tidallyLocked", "Tidally Locked [body id]", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies.Count; },
                "b.number", "Number of Bodies", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].gravParameter; },
                "b.o.gravParameter", "Body Gravitational Parameter [body id]", formatters.Default, APIEntry.UnitType.GRAV));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.GetRelativeVel().magnitude; },
                "b.o.relativeVelocity", "Relative Velocity [body id]", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.PeA; },
                "b.o.PeA", "Periapsis [body id]", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.ApA; },
                "b.o.ApA", "Apoapsis [body id]", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.timeToAp; },
                "b.o.timeToAp", "Time to Apoapsis [body id]", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.timeToPe; },
                "b.o.timeToPe", "Time to Periapsis [body id]", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.inclination; },
                "b.o.inclination", "Inclination [body id]", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.eccentricity; },
                "b.o.eccentricity", "Eccentricity [body id]", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.period; },
                "b.o.period", "Orbital Period [body id]", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.argumentOfPeriapsis; },
                "b.o.argumentOfPeriapsis", "Argument of Periapsis [body id]", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.timeToTransition1; },
                "b.o.timeToTransition1", "Time to Transition 1 [body id]", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.timeToTransition1; },
                "b.o.timeToTransition2", "Time to Transition 2 [body id]", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
               dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.semiMajorAxis; },
               "b.o.sma", "Semimajor Axis [body id]", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
               dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.LAN; },
               "b.o.lan", "Longitude of Ascending Node [body id]", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
               dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.meanAnomalyAtEpoch; },
               "b.o.maae", "Mean Anomaly at Epoch [body id]", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return Planetarium.GetUniversalTime() - FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.ObT; },
                "b.o.timeOfPeriapsisPassage", "Time of Periapsis Passage [body id]", formatters.Default, APIEntry.UnitType.DATE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.Bodies[int.Parse(dataSources.args[0])].orbit.TrueAnomalyAtUT(Planetarium.GetUniversalTime()) * (180.0 / Math.PI); },
                "b.o.trueAnomaly", "True Anomaly [body id]", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    CelestialBody body = FlightGlobals.Bodies[int.Parse(dataSources.args[0])];

                    // Find a common reference body between vessel and body
                    List<CelestialBody> parentBodies = new List<CelestialBody>();
                    CelestialBody parentBody = dataSources.vessel.mainBody;
                    while (true)
                    {
                        if (parentBody == body)
                        {
                            return double.NaN;
                        }
                        parentBodies.Add(parentBody);
                        if (parentBody == Planetarium.fetch.Sun)
                        {
                            break;
                        }
                        else
                        {
                            parentBody = parentBody.referenceBody;
                        }
                    }

                    while (!parentBodies.Contains(body.referenceBody))
                    {
                        body = body.referenceBody;
                    }

                    Orbit orbit = dataSources.vessel.orbit;
                    while (orbit.referenceBody != body.referenceBody)
                    {
                        orbit = orbit.referenceBody.orbit;
                    }

                    // Calculate the phase angle
                    double ut = Planetarium.GetUniversalTime();
                    Vector3d vesselPos = orbit.getRelativePositionAtUT(ut);
                    Vector3d bodyPos = body.orbit.getRelativePositionAtUT(ut);
                    double phaseAngle = (Math.Atan2(bodyPos.y, bodyPos.x) - Math.Atan2(vesselPos.y, vesselPos.x)) * (180.0 / Math.PI);
                    return (phaseAngle < 0) ? phaseAngle + 360 : phaseAngle;
                },
                "b.o.phaseAngle", "Phase Angle [body id]", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new APIEntry(
                dataSources => {
                    int bodyId = int.Parse(dataSources.args[0]);
                    float universalTime = float.Parse(dataSources.args[1]);
                    return FlightGlobals.Bodies[bodyId].getTruePositionAtUT(universalTime);
                },
                "b.o.truePositionAtUT", "True Position at the given UT [body id, universal time]", formatters.Vector3d, APIEntry.UnitType.UNITLESS));
        }

        #endregion
    }

    public class NavBallDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public NavBallDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.CoM);
                    return result.eulerAngles.y;
                },
                "n.heading2", "Heading", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
               dataSources =>
               {
                   Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.CoM);
                   return (result.eulerAngles.x > 180) ? (360.0 - result.eulerAngles.x) : -result.eulerAngles.x;
               },
               "n.pitch2", "Pitch", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
               dataSources =>
               {
                   Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.CoM);
                   return (result.eulerAngles.z > 180) ?
                       (result.eulerAngles.z - 360.0) : result.eulerAngles.z;
               },
               "n.roll2", "Roll", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.CoM);
                    return result.eulerAngles.y;
                },
                "n.rawheading2", "Raw Heading", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
               dataSources =>
               {
                   Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.CoM);
                   return result.eulerAngles.x;
               },
               "n.rawpitch2", "Raw Pitch", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
               dataSources =>
               {
                   Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.CoM);
                   return result.eulerAngles.z;
               },
               "n.rawroll2", "Raw Roll", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.rootPart.transform.position);
                    return result.eulerAngles.y;
                },
                "n.heading", "Heading calculated using the position of the vessels root part", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
               dataSources =>
               {
                   Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.rootPart.transform.position);
                   return (result.eulerAngles.x > 180) ? (360.0 - result.eulerAngles.x) : -result.eulerAngles.x;
               },
               "n.pitch", "Pitch calculated using the position of the vessels root part", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
               dataSources =>
               {
                   Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.rootPart.transform.position);
                   return (result.eulerAngles.z > 180) ?
                       (result.eulerAngles.z - 360.0) : result.eulerAngles.z;
               },
               "n.roll", "Roll calculated using the position of the vessels root part", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.rootPart.transform.position);
                    return result.eulerAngles.y;
                },
                "n.rawheading", "Raw Heading calculated using the position of the vessels root part", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
               dataSources =>
               {
                   Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.rootPart.transform.position);
                   return result.eulerAngles.x;
               },
               "n.rawpitch", "Raw Pitch calculated using the position of the vessels root part", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
               dataSources =>
               {
                   Quaternion result = updateHeadingPitchRollField(dataSources.vessel, dataSources.vessel.rootPart.transform.position);
                   return result.eulerAngles.z;
               },
               "n.rawroll", "Raw Roll calculated using the position of the vessels root part", formatters.Default, APIEntry.UnitType.DEG));
        }

        #endregion

        #region Methods

        //Borrowed from MechJeb2
        private Quaternion updateHeadingPitchRollField(Vessel v, Vector3d CoM)
        {
            Vector3d north, up;
            Quaternion rotationSurface;
         
            up = (CoM - v.mainBody.position).normalized;

            north = Vector3d.Exclude(up, (v.mainBody.position + v.mainBody.transform.up *
                (float)v.mainBody.Radius) - CoM).normalized;

            rotationSurface = Quaternion.LookRotation(north, up);
            return Quaternion.Inverse(Quaternion.Euler(90, 0, 0) *
                Quaternion.Inverse(v.GetTransform().rotation) * rotationSurface);
        }

        #endregion
    }

    public class VesselDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public VesselDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.altitude; },
                "v.altitude", "Altitude", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.heightFromTerrain; },
                "v.heightFromTerrain", "Height from Terrain", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.altitude - dataSources.vessel.heightFromTerrain; },
                "v.terrainHeight", "Terrain Height", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.missionTime; },
                "v.missionTime", "Mission Time", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.srf_velocity.magnitude; },
                "v.surfaceVelocity", "Surface Velocity", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.srf_velocity.x; },
                "v.surfaceVelocityx", "Surface Velocity x", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.srf_velocity.y; },
                "v.surfaceVelocityy", "Surface Velocity y", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.srf_velocity.z; },
                "v.surfaceVelocityz", "Surface Velocity z", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.angularVelocity.magnitude; },
                "v.angularVelocity", "Angular Velocity", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.obt_velocity.magnitude; },
                "v.orbitalVelocity", "Orbital Velocity", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.horizontalSrfSpeed; },
                "v.surfaceSpeed", "Surface Speed", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.verticalSpeed; },
                "v.verticalSpeed", "Vertical Speed", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.geeForce; },
                "v.geeForce", "G-Force", formatters.Default, APIEntry.UnitType.G));
            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    double atmosphericPressure = FlightGlobals.getStaticPressure(dataSources.vessel.altitude, dataSources.vessel.mainBody);
                    double temperature = FlightGlobals.getExternalTemperature(dataSources.vessel.altitude, dataSources.vessel.mainBody);
                    double atmosphericDensityinKilograms = FlightGlobals.getAtmDensity(atmosphericPressure, temperature);
                    return atmosphericDensityinKilograms * 1000;
                    //return dataSources.vessel;
                },
                "v.atmosphericDensity", "Atmospheric Density", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.longitude > 180 ? dataSources.vessel.longitude - 360.0 : dataSources.vessel.longitude; },
                "v.long", "Longitude", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.latitude; },
                "v.lat", "Latitude", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources => {return (dataSources.vessel.atmDensity * 0.5) * Math.Pow(dataSources.vessel.srf_velocity.magnitude, 2); },
                "v.dynamicPressure", "Dynamic Pressure", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.getStaticPressure(dataSources.vessel.altitude, dataSources.vessel.mainBody) * 1000; },
                "v.atmosphericPressurePa", "Atmospheric Pressure (Pa)", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return FlightGlobals.getStaticPressure(dataSources.vessel.altitude, dataSources.vessel.mainBody) * PhysicsGlobals.KpaToAtmospheres; },
                "v.atmosphericPressure", "Atmospheric Pressure", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.name; },
                "v.name", "Name", formatters.Default, APIEntry.UnitType.STRING));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.referenceBody.name; },
                "v.body", "Body Name", formatters.Default, APIEntry.UnitType.STRING));
            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    if (dataSources.vessel.mainBody == Planetarium.fetch.Sun)
                    {
                        return double.NaN;
                    }
                    else
                    {
                        double ut = Planetarium.GetUniversalTime();
                        CelestialBody body = dataSources.vessel.mainBody;
                        Vector3d bodyPrograde = body.orbit.getOrbitalVelocityAtUT(ut);
                        Vector3d bodyNormal = body.orbit.GetOrbitNormal();
                        Vector3d vesselPos = dataSources.vessel.orbit.getRelativePositionAtUT(ut);
                        Vector3d vesselPosInPlane = Vector3d.Exclude(bodyNormal, vesselPos); // Project the vessel position into the body's orbital plane
                        double angle = Vector3d.Angle(vesselPosInPlane, bodyPrograde);
                        if (Vector3d.Dot(Vector3d.Cross(vesselPosInPlane, bodyPrograde), bodyNormal) < 0)
                        { // Correct for angles > 180 degrees
                            angle = 360 - angle;
                        }
                        if (dataSources.vessel.orbit.GetOrbitNormal().z < 0)
                        { // Check for retrograde orbit
                            angle = 360 - angle;
                        }
                        return angle;
                    }
                },
                "v.angleToPrograde", "Angle to Prograde", formatters.Default, APIEntry.UnitType.DEG));
        }

        #endregion
    }

    public class OrbitDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public OrbitDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.GetRelativeVel().magnitude; },
                "o.relativeVelocity", "Relative Velocity", formatters.Default, APIEntry.UnitType.VELOCITY));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.PeA; },
                "o.PeA", "Periapsis", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.ApA; },
                "o.ApA", "Apoapsis", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.timeToAp; },
                "o.timeToAp", "Time to Apoapsis", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.timeToPe; },
                "o.timeToPe", "Time to Periapsis", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.inclination; },
                "o.inclination", "Inclination", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.eccentricity; },
                "o.eccentricity", "Eccentricity", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.epoch; },
                "o.epoch", "Epoch", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.period; },
                "o.period", "Orbital Period", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.argumentOfPeriapsis; },
                "o.argumentOfPeriapsis", "Argument of Periapsis", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.timeToTransition1; },
                "o.timeToTransition1", "Time to Transition 1", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.timeToTransition1; },
                "o.timeToTransition2", "Time to Transition 2", formatters.Default, APIEntry.UnitType.TIME));
            registerAPI(new PlotableAPIEntry(
               dataSources => { return dataSources.vessel.orbit.semiMajorAxis; },
               "o.sma", "Semimajor Axis", formatters.Default, APIEntry.UnitType.DISTANCE));
            registerAPI(new PlotableAPIEntry(
               dataSources => { return dataSources.vessel.orbit.LAN; },
               "o.lan", "Longitude of Ascending Node", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new PlotableAPIEntry(
                dataSources =>
                {
                    Orbit orbit = dataSources.vessel.orbit;
                    return orbit.getObtAtUT(0) / orbit.period * (2.0 * Math.PI);
                },
               "o.maae", "Mean Anomaly at Epoch", formatters.Default, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return Planetarium.GetUniversalTime() - dataSources.vessel.orbit.ObT; },
                "o.timeOfPeriapsisPassage", "Time of Periapsis Passage", formatters.Default, APIEntry.UnitType.DATE));
            registerAPI(new PlotableAPIEntry(
                dataSources => { return dataSources.vessel.orbit.TrueAnomalyAtUT(Planetarium.GetUniversalTime()) * (180.0 / Math.PI); },
                "o.trueAnomaly", "True Anomaly", formatters.Default, APIEntry.UnitType.DEG));
            registerAPI(new APIEntry(
                dataSources => {
                    return OrbitPatches.getPatchesForOrbit(dataSources.vessel.orbit);
                },
                "o.orbitPatches", "Detailed Orbit Patches Info [object orbitPatchInfo]",
                formatters.OrbitPatchList, APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    int index = int.Parse(dataSources.args[0]);
                    float ut = float.Parse(dataSources.args[1]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(dataSources.vessel.orbit, index);
                    if(orbitPatch == null) { return null; }
                    return orbitPatch.TrueAnomalyAtUT(ut);
                },
                "o.trueAnomalyAtUTForOrbitPatch", "The orbit patch's True Anomaly at Universal Time [orbit patch index, universal time]", formatters.Default, APIEntry.UnitType.DEG));

            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    int index = int.Parse(dataSources.args[0]);
                    float trueAnomaly = float.Parse(dataSources.args[1]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(dataSources.vessel.orbit, index);
                    if (orbitPatch == null) { return null; }

                    double now = Planetarium.GetUniversalTime();
                    return orbitPatch.GetUTforTrueAnomaly(trueAnomaly, now);
                },
                "o.UTForTrueAnomalyForOrbitPatch", "The orbit patch's True Anomaly at Universal Time [orbit patch index, universal time]", formatters.Default, APIEntry.UnitType.DATE));
            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    int index = int.Parse(dataSources.args[0]);
                    float trueAnomaly = float.Parse(dataSources.args[1]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(dataSources.vessel.orbit, index);
                    if (orbitPatch == null) { return null; }
                    return orbitPatch.getRelativePositionFromTrueAnomaly(trueAnomaly);
                },
                "o.relativePositionAtTrueAnomalyForOrbitPatch", "The orbit patch's predicted displacement from the center of the main body at the given true anomaly [orbit patch index, true anomaly]", formatters.Vector3d, APIEntry.UnitType.UNITLESS));

            registerAPI(new PlotableAPIEntry(
                dataSources => {
                    int index = int.Parse(dataSources.args[0]);
                    double ut = double.Parse(dataSources.args[1]);

                    Orbit orbitPatch = OrbitPatches.getOrbitPatch(dataSources.vessel.orbit, index);
                    if (orbitPatch == null) { return null; }

                    return orbitPatch.getRelativePositionAtUT(ut);
                },
                "o.relativePositionAtUTForOrbitPatch", "The orbit patch's predicted displacement from the center of the main body at the given universal time [orbit patch index, universal time]", formatters.Vector3d, APIEntry.UnitType.UNITLESS));

        }

        #endregion
    }

    public class SensorDataLinkHandler : DataLinkHandler
    {
        #region Fields

        SensorCache sensorCache = null;

        #endregion

        #region Initialisation

        public SensorDataLinkHandler(VesselChangeDetector vesselChangeDetector, FormatterProvider formatters)
            : base(formatters)
        {
            sensorCache = new SensorCache(vesselChangeDetector);

            registerAPI(new PlotableAPIEntry(
               dataSources => { return getsSensorValues(dataSources); },
               "s.sensor", "Sensor Information [string sensor type]", formatters.SensorModuleList,
               APIEntry.UnitType.UNITLESS));
            registerAPI(new PlotableAPIEntry(
               dataSources => { dataSources.args.Add("TEMP"); return getsSensorValues(dataSources); },
               "s.sensor.temp", "Temperature sensor information", formatters.SensorModuleList,
               APIEntry.UnitType.TEMP));
            registerAPI(new PlotableAPIEntry(
               dataSources => { dataSources.args.Add("PRES"); return getsSensorValues(dataSources); },
               "s.sensor.pres", "Pressure sensor information", formatters.SensorModuleList,
               APIEntry.UnitType.PRES));
            registerAPI(new PlotableAPIEntry(
               dataSources => { dataSources.args.Add("GRAV"); return getsSensorValues(dataSources); },
               "s.sensor.grav", "Gravity sensor information", formatters.SensorModuleList,
               APIEntry.UnitType.GRAV));
            registerAPI(new PlotableAPIEntry(
               dataSources => { dataSources.args.Add("ACC"); return getsSensorValues(dataSources); },
               "s.sensor.acc", "Acceleration sensor information", formatters.SensorModuleList,
               APIEntry.UnitType.ACC));
        }

        #endregion

        #region Sensors

        protected List<ModuleEnviroSensor> getsSensorValues(DataSources datasources)
        {
            sensorCache.vessel = datasources.vessel;
            return sensorCache.get(datasources);
        }

        #endregion
    }


    public class ResourceDataLinkHandler : DataLinkHandler
    {

        #region Fields

        ResourceCache resourceCache = null;
        ActiveResourceCache activeResourceCache = null;

        #endregion

        #region Initialisation

        public ResourceDataLinkHandler(VesselChangeDetector vesselChangeDetector, FormatterProvider formatters)
            : base(formatters)
        {

            resourceCache = new ResourceCache(vesselChangeDetector);
            activeResourceCache = new ActiveResourceCache(vesselChangeDetector);

            registerAPI(new APIEntry(
                dataSources => { return getsResourceValues(dataSources); },
                "r.resource", "Resource Information [string resource type]",
                formatters.ResourceList, APIEntry.UnitType.UNITLESS));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    return getsActiveResourceValues(dataSources);
                },
                "r.resourceCurrent", "Resource Information for Current Stage [string resource type]",
                formatters.ActiveResourceList, APIEntry.UnitType.UNITLESS));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    return getsActiveResourceValues(dataSources);
                },
                "r.resourceCurrentMax", "Max Resource Information for Current Stage [string resource type]",
                formatters.MaxCurrentResourceList, APIEntry.UnitType.UNITLESS));

            registerAPI(new APIEntry(
                dataSources => { return getsResourceValues(dataSources); },
                "r.resourceMax", "Max Resource Information [string resource type]",
                formatters.MaxResourceList, APIEntry.UnitType.UNITLESS));
            registerAPI(new APIEntry(
                dataSources => {
                    List<String> names = new List<String>();
                    PartResourceDefinitionList resourceDefinitionList = PartResourceLibrary.Instance.resourceDefinitions;
                    foreach (PartResourceDefinition resourceDefinition in resourceDefinitionList)
                    {
                        names.Add(resourceDefinition.name);
                    }

                    return names;
                },
                "r.resourceNameList", "List of resource names",
                formatters.StringArray, APIEntry.UnitType.UNITLESS));
        }

        #endregion

        #region Resources

        protected List<PartResource> getsResourceValues(DataSources datasources)
        {
            resourceCache.vessel = datasources.vessel;
            return resourceCache.get(datasources);
        }

        protected List<SimplifiedResource> getsActiveResourceValues(DataSources datasources)
        {
            activeResourceCache.vessel = datasources.vessel;
            return activeResourceCache.get(datasources);
        }

        #endregion
    }

    public abstract class ModuleCache<T>
    {
        #region Constants

        protected const int ACCESS_REFRESH = 10;

        #endregion

        #region Fields

        protected event EventHandler<EventArgs> VesselPropertyChanged;

        private Vessel theVessel = null;
        public Vessel vessel
        {
            get { return theVessel; }
            set {
                if (theVessel == value) return;
                theVessel = value;
                if (VesselPropertyChanged != null) VesselPropertyChanged(this, EventArgs.Empty);
            }
        }

        protected Dictionary<string, List<T>> partModules = new Dictionary<string, List<T>>();

        #endregion

        #region Lock

        readonly protected System.Object cacheLock = new System.Object();

        #endregion

        #region Cache

        public List<T> get(DataSources dataSources)
        {
            string ID = dataSources.args[0].ToLowerInvariant();
            List<T> avail = null, ret = null;

            lock (cacheLock)
            {
                partModules.TryGetValue(ID, out avail);
            }

            if (avail != null)
            {
                ret = new List<T>(avail);
            }
            else
            {
                ret = new List<T>();
            }

            return ret;
        }

        protected abstract void refresh(Vessel vessel);

        #endregion
    }

    public class SimplifiedResource
    {
        public double amount { get; set; }
        public double maxAmount { get; set; }

        public SimplifiedResource(double amount, double maxAmount)
        {
            this.amount = amount;
            this.maxAmount = maxAmount;
        }
    }

    public class ActiveResourceCache : ModuleCache<SimplifiedResource>
    {
        #region ModuleCache

        public ActiveResourceCache(VesselChangeDetector vesselChangeDetector)
        {
            vesselChangeDetector.UpdateNotify += update;
        }

        private void update(object sender, EventArgs eventArgs)
        {
            if (vessel != null)
            {
                lock (cacheLock)
                {
                    refresh(vessel);
                }
            }
        }

        protected override void refresh(Vessel vessel)
        {
            try
            {
                partModules.Clear();
                HashSet<Part> activeParts = new HashSet<Part>();
                foreach(Part part in vessel.GetActiveParts())
                {
                    if (part.inverseStage == vessel.currentStage)
                    {
                        activeParts.Add(part);
                        activeParts.UnionWith(part.crossfeedPartSet.GetParts());
                    }
                }

                PartSet activePartSet = new PartSet(activeParts);
                PartResourceDefinitionList resourceDefinitionList = PartResourceLibrary.Instance.resourceDefinitions;

                foreach(PartResourceDefinition resourceDefinition in resourceDefinitionList)
                {
                    String key = resourceDefinition.name.ToString().ToLowerInvariant();
                    double amount = 0;
                    double maxAmount = 0;
                    bool pulling = true;

                    activePartSet.GetConnectedResourceTotals(resourceDefinition.id, out amount, out maxAmount, pulling);

                    if(!partModules.ContainsKey(key)){
                        partModules[key] = new List<SimplifiedResource>();
                    }

                    partModules[key].Add(new SimplifiedResource(amount, maxAmount));
                    PluginLogger.debug("SIZE OF " + key + " " + partModules[key].Count + " " + amount );
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message);
            }
        }

        #endregion
    }

    public class ResourceCache : ModuleCache<PartResource>
    {
        #region ModuleCache

        public ResourceCache(VesselChangeDetector vesselChangeDetector)
        {
            vesselChangeDetector.UpdateNotify += update;
        }

        private void update(object sender, EventArgs eventArgs)
        {
            if (vessel != null)
            {
                lock (cacheLock)
                {
                    refresh(vessel);
                }
            }
        }
        
        protected override void refresh(Vessel vessel)
        {
            try
            {
                partModules.Clear();

                foreach (Part part in vessel.parts)
                {
                    if (part.Resources.Count > 0)
                    {

                        foreach (PartResource partResource in part.Resources)
                        {
                            String key = partResource.resourceName.ToLowerInvariant();
                            List<PartResource> list = null;
                            partModules.TryGetValue(key, out list);
                            if (list == null)
                            {
                                list = new List<PartResource>();
                                partModules[key] = list;

                            }

                            list.Add(partResource);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message);
            }
        }

        #endregion
    }

    public class SensorCache : ModuleCache<ModuleEnviroSensor>
    {
        #region ModuleCache

        public SensorCache(VesselChangeDetector vesselChangeDetector)
        {
            vesselChangeDetector.UpdateNotify += update;
            VesselPropertyChanged += update;
        }

        private void update(object sender, EventArgs eventArgs)
        {
            if (vessel != null)
            {
                lock (cacheLock)
                {
                    refresh(vessel);
                }
            }
        }

        protected override void refresh(Vessel vessel)
        {
            try
            {
                partModules.Clear();

                List<Part> partsWithSensors = vessel.parts.FindAll(p => p.Modules.Contains("ModuleEnviroSensor"));
                foreach (Part part in partsWithSensors)
                {
                    foreach (var module in part.Modules.OfType<ModuleEnviroSensor>())
                    {
                        if (!partModules.ContainsKey(module.sensorType.ToString()))
                        {
                            partModules[module.sensorType.ToString()] = new List<ModuleEnviroSensor>();
                        }
                        partModules[module.sensorType.ToString()].Add(module);
                    }
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message + " " + e.StackTrace);
            }
        }

        #endregion
    }

    public class PausedDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public PausedDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new PlotableAPIEntry(
                dataSources => { return partPaused(); },
                "p.paused", 
                "Returns an integer indicating the state of antenna. 0 - Flight scene; 1 - Paused; 2 - No power; 3 - Off; 4 - Not found.", 
                formatters.Default, APIEntry.UnitType.UNITLESS, true));
        }

        #endregion

        #region Methods

        public static int partPaused()
        {
            bool result = FlightDriver.Pause ||
                !TelemachusPowerDrain.isActive ||
                !TelemachusPowerDrain.activeToggle ||
                !VesselChangeDetector.hasTelemachusPart ||
                !HighLogic.LoadedSceneIsFlight;
            
            if (result)
            {
                // If we aren't even in the flight scene, say so
                if (!HighLogic.LoadedSceneIsFlight)
                {
                    return 5;
                }

                if (FlightDriver.Pause)
                {
                    return 1;
                }

                if (!TelemachusPowerDrain.isActive)
                {
                    return 2;
                }

                if (!TelemachusPowerDrain.activeToggle)
                {
                    return 3;
                }

                if (!VesselChangeDetector.hasTelemachusPart)
                {
                    return 4;
                }
            }
            else
            {
                return 0;
            }

            return 5;
        }

        #endregion
    }

    public class APIDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public APIDataLinkHandler(IKSPAPI kspAPI, FormatterProvider formatters,
            ServerConfiguration serverConfiguration)
            : base(formatters)
        {
            registerAPI(new APIEntry(
                dataSources =>
                {
                    List<APIEntry> APIList = new List<APIEntry>();
                    kspAPI.getAPIList(ref APIList); return APIList;
                },
                "a.api", "API Listing", formatters.APIEntry, APIEntry.UnitType.UNITLESS, true));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    List<String> IPList = new List<String>();

                    foreach (System.Net.IPAddress a in serverConfiguration.ValidIpAddresses)
                    {
                        IPList.Add(a.ToString());
                    }

                    return IPList;
                },
                "a.ip", "IP Addresses", formatters.StringArray, APIEntry.UnitType.UNITLESS, true));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    List<APIEntry> APIList = new List<APIEntry>();
                    foreach (string apiRequest in dataSources.args)
                    {
                        kspAPI.getAPIEntry(apiRequest, ref APIList);
                    }

                    return APIList;
                },
                "a.apiSubSet",
                "Subset of the API Listing [string api1, string api2, ... , string apiN]",
                formatters.APIEntry, APIEntry.UnitType.STRING, true));

            registerAPI(new PlotableAPIEntry(
                dataSources => { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); },
                "a.version", "Telemachus Version", formatters.Default, APIEntry.UnitType.UNITLESS, true));
        }

        #endregion
    }

    public class CompoundDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public CompoundDataLinkHandler(List<DataLinkHandler> APIHandlers, FormatterProvider formatters)
            : base(formatters)
        {
            foreach (DataLinkHandler dlh in APIHandlers)
            {
                foreach (KeyValuePair<string, APIEntry> entry in dlh.API)
                {
                    registerAPI(entry.Value);
                }
            }
        }

        #endregion
    }

    public abstract class DataLinkHandler
    {
        #region API Delegates

        public delegate object APIDelegate(DataSources datasources);

        #endregion

        #region API Fields

        private Dictionary<string, APIEntry> APIEntries =
           new Dictionary<string, APIEntry>();
        APIEntry nullAPI = null;
        protected FormatterProvider formatters = null;

        #endregion

        #region Initialisation

        public DataLinkHandler(FormatterProvider formatters)
        {
            this.formatters = formatters;
            nullAPI = new APIEntry(
                dataSources =>
                {
                    return pausedHandler();
                },
                "", "", formatters.Default, APIEntry.UnitType.UNITLESS);
        }

        #endregion

        #region DataLinkHandler

        public IEnumerable<KeyValuePair<string, APIEntry>> API
        {
            get
            {
                return APIEntries;
            }
        }

        public virtual bool process(String API, out APIEntry result)
        {
            APIEntry entry = null;

            APIEntries.TryGetValue(API, out entry);

            if (entry == null)
            {
                result = null;
                return false;
            }
            else
            {
                if (pausedHandler() == 0)
                {
                    result = entry;
                }
                else
                {
                    result = nullAPI;
                }

                return true;
            }
        }

        public void appendAPIList(ref List<APIEntry> APIList)
        {
            foreach (KeyValuePair<String, APIEntry> entry in APIEntries)
            {
                APIList.Add(entry.Value);
            }
        }

        protected void registerAPI(APIEntry entry)
        {
            APIEntries.Add(entry.APIString, entry);
        }

        protected virtual int pausedHandler()
        {
            return 0;
        }

        #endregion
    }

    public class APIEntry
    {
        #region Enumeration

        public enum UnitType { UNITLESS, VELOCITY, DEG, DISTANCE, TIME, STRING, TEMP, PRES, GRAV, ACC, DENSITY, DYNAMICPRESSURE, G, DATE };

        #endregion

        #region Fields

        public DataLinkHandler.APIDelegate function { get; set; }
        public string APIString { get; set; }
        public string name { get; set; }
        public UnitType units { get; set; }
        public bool plotable { get; set; }
        public DataSourceResultFormatter formatter { get; set; }
        public bool alwaysEvaluable { get; set; }
        #endregion

        #region Initialisation

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString,
            string name, DataSourceResultFormatter formatter, UnitType units, bool alwaysEvaluable = false)
        {
            this.function = function;
            this.APIString = APIString;
            this.name = name;
            this.formatter = formatter;
            this.units = units;
            this.alwaysEvaluable = alwaysEvaluable;
        }

        #endregion
    }

    public class OrbitPatches
    {
        public static List<Orbit> getPatchesForOrbit(Orbit orbit)
        {
            List<Orbit> orbitPatches = new List<Orbit>();
            //the "next" orbit patch is the root patch, to make the method cleaner
            var nextOrbitPatch = orbit;

            while(nextOrbitPatch != null && nextOrbitPatch.activePatch)
            {
                orbitPatches.Add(nextOrbitPatch);
                if (nextOrbitPatch.patchEndTransition == Orbit.PatchTransitionType.MANEUVER)
                {
                    break;
                }
                else
                {
                    nextOrbitPatch = nextOrbitPatch.nextPatch;
                }
            }

            return orbitPatches;
        }

        public static Orbit getOrbitPatch(Orbit orbit, int index)
        {
            List<Orbit> orbitPatches = getPatchesForOrbit(orbit);
            if(index >= orbitPatches.Count) { return null; }
            return orbitPatches[index];
        }
    }

    public class ActionAPIEntry : APIEntry
    {
        #region Initialisation

        public ActionAPIEntry(DataLinkHandler.APIDelegate function,
            string APIString, string name, DataSourceResultFormatter formatter)
            : base(function, APIString, name, formatter, APIEntry.UnitType.UNITLESS)
        {
            plotable = false;
        }

        #endregion
    }

    public class PlotableAPIEntry : APIEntry
    {
        #region Initialisation

        public PlotableAPIEntry(DataLinkHandler.APIDelegate function, string APIString, string name,
           DataSourceResultFormatter formatter, UnitType units, bool alwaysEvaluable = false)
            : base(function, APIString, name, formatter, units, alwaysEvaluable)
        {
            this.plotable = true;
        }

        #endregion
    }

    public class DelayedAPIEntry : APIEntry
    {
        #region Fields

        private DataSources dataSources = null;

        #endregion

        #region Initialisation

        public DelayedAPIEntry(DataSources dataSources, DataLinkHandler.APIDelegate function)
            : base(function, "", "", null, UnitType.UNITLESS)
        {
            this.dataSources = dataSources;
        }

        #endregion

        #region Methods

        public void call()
        {
            try
            {
                function(dataSources);
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message);
            }
        }

        #endregion
    }
}
