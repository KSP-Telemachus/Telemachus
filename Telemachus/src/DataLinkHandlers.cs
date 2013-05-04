//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Servers.MinimalHTTPServer;
using System.Threading;
using System.Collections;
using UnityEngine;

namespace Telemachus
{
    public class MechJebDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public MechJebDataLinkHandler()
        {
            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { return reflectOff(dataSources); }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                    return predictFailure(dataSources.vessel);
                },
               "mj.smartassoff", "Smart ASS Off", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.forward, "MANEUVER_NODE"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.node", "Node", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.forward, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.prograde", "Prograde", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.back, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.retrograde", "Retrograde", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.left, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.normalplus", "Normal Plus", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.right, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.normalminus", "Normal Minus", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.up, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.radialplus", "Radial Plus", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { return reflectAttitudeTo(dataSources, Vector3d.down, "ORBIT"); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
               "mj.radialminus", "Radial Minus", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { return surface(dataSources); }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); 
                    return predictFailure(dataSources.vessel);
                },
               "mj.surface", "Surface [float heading, float pitch]", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) =>
                        {
                            return reflectAttitudeTo(dataSources, double.Parse(dataSources.args[0]),
                                double.Parse(dataSources.args[1]), double.Parse(dataSources.args[2])
                                );
                        }), UnityEngine.SendMessageOptions.DontRequireReceiver); 
                    return predictFailure(dataSources.vessel);
                },
               "mj.surface2", "Surface [double heading, double pitch, double roll]", false));
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
                methodInfo.Invoke(attitude, new object [] {});
                return true;
            }

            return false;
        }

        private bool reflectAttitudeTo(DataSources dataSources, Vector3d v, string reference)
        {
            object attitude = null;

            Type attitudeType = getAttitudeType(dataSources, ref attitude);
            if(attitudeType != null)
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

                methodInfo.Invoke(attitude, new object[] { heading, pitch , roll, this });

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
                    PluginLogger.debug(e.Message + " " +  e.StackTrace);
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
    }

    public class FlyByWireDataLinkHandler : DataLinkHandler
    {
        #region Fields

        static float yaw = 0, pitch = 0, roll = 0;
        static bool iyaw = false, ipitch = false, iroll = false;
        static int on = 0;

        #endregion

        #region Initialisation

        public FlyByWireDataLinkHandler()
        {
            registerAPI(new APIEntry(
                dataSources => { yaw = checkFlightStateParameters(float.Parse(dataSources.args[0])); iyaw = true ; return 0; },
                "v.setYaw", "Yaw [float yaw]", false));

            registerAPI(new APIEntry(
                dataSources => { pitch = checkFlightStateParameters(float.Parse(dataSources.args[0])); ipitch = true; return 0; },
                "v.setPitch", "Pitch [float pitch]", false));

            registerAPI(new APIEntry(
                dataSources => { roll = checkFlightStateParameters(float.Parse(dataSources.args[0])); iroll = true; return 0; },
                "v.setRoll", "Roll [float roll]", false));

            registerAPI(new APIEntry(
                dataSources => { on = int.Parse(dataSources.args[0]); iroll = true; return 0; },
                "v.setFbW", "Set Fly by Wire On or Off [bool state]", false));

            registerAPI(new APIEntry(
                dataSources => { 
                    yaw = checkFlightStateParameters(float.Parse(dataSources.args[0]));
                    pitch = checkFlightStateParameters(float.Parse(dataSources.args[1]));
                    roll = checkFlightStateParameters(float.Parse(dataSources.args[2]));
                    iyaw = true;
                    ipitch = true;
                    iroll = true;
                    return 0; 
                },
                "v.setYawPitchRoll", "Roll [float yaw, float pitch, float roll]", false));
        }

        #endregion

        #region Methods

        public static void onFlyByWire(FlightCtrlState fcs)
        {
            if (on > 0)
            {
                fcs.yaw = yaw;
                fcs.pitch = pitch;
                fcs.roll = roll;
            }

        }

        public static void reset()
        {
            yaw = 0;
            pitch = 0;
            roll = 0;
        }

        private float checkFlightStateParameters(float f)
        {
            if(float.IsNaN(f))
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

        public FlightDataLinkHandler()
        {
            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => { Staging.ActivateNextStage(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                    return predictFailure(dataSources.vessel);
                }, "f.stage", "Stage", false));

            registerAPI(new APIEntry(
               dataSources =>
               {
                   TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                       (x) => { setThrottle(x); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                       return predictFailure(dataSources.vessel);
               },
                "f.setThrottle", "Set Throttle [float magnitude]", false));

            registerAPI(new APIEntry(
               dataSources =>
               {
                   float t = FlightInputHandler.state.mainThrottle;
                   return t;
                },
                "f.throttle", "Throttle", true));

            registerAPI(new APIEntry(
               dataSources =>
               {
                   TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                       (x) => { throttleUp(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                       return predictFailure(dataSources.vessel);
               },
                "f.throttleUp", "Throttle Up", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { throttleZero(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                        return predictFailure(dataSources.vessel);
                },
                "f.throttleZero", "Throttle Zero", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { throttleFull(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                        return predictFailure(dataSources.vessel);
                },
                "f.throttleFull", "Throttle Full", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { throttleDown(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver);
                        return predictFailure(dataSources.vessel);
                },
                "f.throttleDown", "Throttle Down", false));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { dataSources.vessel.ActionGroups.ToggleGroup(KSPActionGroup.RCS); return 0d ; }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
                },
                "f.rcs", "RCS", false));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.SAS),
                "f.sas", "SAS", false));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Light),
                "f.light", "Light", false));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Gear),
                "f.gear", "Gear"));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Brakes),
                "f.brake", "Brake", false));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Abort),
                "f.abort", "Abort", false));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom01),
                "f.ag1", "Action Group 1", false));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom02),
                "f.ag2", "Action Group 2", false));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom03),
                "f.ag3", "Action Group 3", false));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom04),
                "f.ag4", "Action Group 4", false));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom05),
                "f.ag5", "Action Group 5", false));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom06),
                "f.ag6", "Action Group 6", false));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom07),
                "f.ag7", "Action Group 7", false));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom08),
                "f.ag8", "Action Group 8", false));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom09),
                "f.ag9", "Action Group 9", false));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom10),
                "f.ag10", "Action Group 10", false));
        }

        private DataLinkHandler.APIDelegate buildActionGroupToggleDelayedLamda(KSPActionGroup actionGroup)
        {
            return dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { dataSources.vessel.ActionGroups.ToggleGroup(actionGroup); return 0d; }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return predictFailure(dataSources.vessel);
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

        public TimeWarpDataLinkHandler()
        {
            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { TimeWarp.SetRate(int.Parse(x.args[0]),false); return 0d; }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return false;
                },
                "t.timeWarp", "Time Warp [int rate]", false));
        }


        #endregion
    }

    public class BodyDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public BodyDataLinkHandler()
        {
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.referenceBody.name; },
                "b.name", "Body Name", new StringJSONFormatter()));
            registerAPI(new APIEntry(
               dataSources => { return dataSources.vessel.orbit.referenceBody.position; },
               "b.position", "Body Position", new Vector3dJSONFormatter()));
        }

        #endregion
    }

    public class VesselDataLinkHandler : DataLinkHandler
    {
        #region Initialisation
        
        public VesselDataLinkHandler()
        {
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.altitude; }, 
                "v.altitude", "Altitude"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.altitude - dataSources.vessel.heightFromTerrain; }, 
                "v.heightFromTerrain", "Height from Terrain"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.heightFromTerrain; },
                "v.terrainHeight", "Terrain Height"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.missionTime; }, 
                "v.missionTime", "Mission Time"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.srf_velocity.magnitude; },
                "v.surfaceVelocity", "Surface Velocity"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.angularVelocity.magnitude; },
                "v.angularVelocity", "Angular Velocity"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.obt_velocity.magnitude; },
                "v.orbitalVelocity", "Orbital Velocity", APIEntry.UnitType.VELOCITY, true));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.horizontalSrfSpeed; },
                "v.surfaceSpeed", "Surface Speed"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.verticalSpeed; },
                "v.verticalSpeed", "Vertical Speed"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.atmDensity; },
                "v.atmosphericDensity", "Atmospheric Density"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.longitude; },
                "v.long", "Longitude"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.latitude; },
                "v.lat", "Latitude"));
            registerAPI(new APIEntry(
                dataSources => { return (dataSources.vessel.atmDensity * 0.5) + Math.Pow(dataSources.vessel.srf_velocity.magnitude, 2); },
                "v.dynamicPressure", "Dynamic Pressure"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.name; },
                "v.name", "Name", new StringJSONFormatter()));
        }

        #endregion
    }

    public class OrbitDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public OrbitDataLinkHandler()
        {
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.PeA; },
                "o.PeA", "Periapsis", APIEntry.UnitType.DISTANCE, true));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.ApA; },
                "o.ApA", "Apoapsis", APIEntry.UnitType.DISTANCE, true));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.timeToAp; },
                "o.timeToAp", "Time to Apoapsis", APIEntry.UnitType.TIME, true));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.timeToPe; },
                "o.timeToPe", "Time to Periapsis", APIEntry.UnitType.TIME, true));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.inclination; },
                "o.inclination", "Inclination", APIEntry.UnitType.DEG, true));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.eccentricity; },
                "o.eccentricity", "Eccentricity", APIEntry.UnitType.UNITLESS, true));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.period; },
                "o.period", "Orbital Period", APIEntry.UnitType.TIME, true));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.argumentOfPeriapsis; },
                "o.argumentOfPeriapsis", "Argument of Periapsis", APIEntry.UnitType.DEG, true));
        }

        #endregion
    }

    public class SensorDataLinkHandler : DataLinkHandler
    {

        #region Fields

        SensorCache sensorCache = new SensorCache();

        #endregion

        #region Initialisation

        public SensorDataLinkHandler(VesselChangeDetector vesselChangeDetector)
        {
            vesselChangeDetector.suscribe(new VesselChangeDetector.VesselChange(vesselChanged));

            registerAPI(new APIEntry(
               dataSources => { return getsSensorValues(dataSources); },
               "s.sensor", "Sensor Information [string sensor type]", new SensorModuleListJSONFormatter()));
        }

        #endregion

        #region Sensors

        protected List<ModuleEnviroSensor> getsSensorValues(DataSources datasources)
        {
            return sensorCache.get(datasources);
        }

        #endregion

        #region VesselChangeDetector

        private void vesselChanged(Vessel vessel)
        {
            
        }

        #endregion
    }


    public class ResourceDataLinkHandler : DataLinkHandler
    {

        #region Fields

        ResourceCache resourceCache = new ResourceCache();

        #endregion

        #region Initialisation

        public ResourceDataLinkHandler(VesselChangeDetector vesselChangeDetector)
        {
            vesselChangeDetector.suscribe(new VesselChangeDetector.VesselChange(vesselChanged));
            registerAPI(new APIEntry(
                dataSources => { return getsResourceValues(dataSources); },
                "r.resource", "Resource Information [string resource type]", new ResourceListJSONFormatter()));
        }

        #endregion

        #region Sensors

        protected List<PartResource> getsResourceValues(DataSources datasources)
        {
            return resourceCache.get(datasources);
        }

        #endregion

        #region VesselChangeDetector

        private void vesselChanged(Vessel vessel)
        {

        }

        #endregion
    }

    public abstract class ModuleCache<T>
    {
        #region Constants

        protected const int ACCESS_REFRESH = 10;

        #endregion

        #region Fields

        protected Dictionary<string, List<T>> partModules = new Dictionary<string, List<T>>();
        protected bool isDirty = true;
        protected int accesses = 0;

        #endregion

        #region Cache

        protected void setDirty(bool value)
        {
            isDirty = value;
        }

        protected bool checkDirty()
        {
            return isDirty;
        }

        public void setDirty()
        {
            setDirty(true);
        }

        public List<T> get(DataSources dataSources)
        {
            string ID = dataSources.args[0];
            List<T> avail = null, ret = null;

            lock (this)
            {
                accesses++;

                if (accesses >= ACCESS_REFRESH)
                {
                    setDirty();
                }

                if (checkDirty())
                {
                    refresh(dataSources.vessel);
                    accesses = 0;
                }

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

    public class ResourceCache : ModuleCache<PartResource>
    {
        #region ModuleCache

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
                            List<PartResource> list = null;
                            partModules.TryGetValue(partResource.resourceName, out list);
                            if (list == null)
                            {
                                list = new List<PartResource>();
                                partModules[partResource.resourceName] = list;
                                
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

        protected override void refresh(Vessel vessel)
        {
            try
            {
                partModules.Clear();

                List<Part> partsWithSensors = vessel.parts.FindAll(p => p.Modules.Contains("ModuleEnviroSensor"));

                foreach (Part part in partsWithSensors)
                {
                    foreach (var module in part.Modules)
                    {
                        if (module.GetType().Equals(typeof(ModuleEnviroSensor)))
                        {
                            List<ModuleEnviroSensor> list = null;
                            partModules.TryGetValue(((ModuleEnviroSensor)module).sensorType, out list);
                            if (list == null)
                            {
                                PluginLogger.debug(((ModuleEnviroSensor)module).sensorType);
                                list = new List<ModuleEnviroSensor>();
                                partModules[((ModuleEnviroSensor)module).sensorType] = list;
                                
                            }

                            list.Add((ModuleEnviroSensor)module);
                        }
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

        public PausedDataLinkHandler()
        {
            registerAPI(new APIEntry(
                dataSources => { return partPaused(); },
                "p.paused", "Paused"));
        }

        #endregion

        #region Methods

        public static int partPaused()
        {
            bool result = FlightDriver.Pause ||
                !TelemachusPowerDrain.isActive ||
                !TelemachusPowerDrain.activeToggle ||
                !VesselChangeDetector.hasTelemachusPart;

            if (result)
            {
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

        public APIDataLinkHandler(DataLinkResponsibility dataLinkResponsibility)
        {
            registerAPI(new APIEntry(
                dataSources =>
                {
                    List<APIEntry> APIList = new List<APIEntry>(); 
                    dataLinkResponsibility.getAPIList(ref APIList); return APIList;},
                "a.api", "API Listing", new APIEntryJSONFormatter()));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    List<APIEntry> APIList = new List<APIEntry>();
                    foreach(string apiRequest in dataSources.args)
                    {
                        dataLinkResponsibility.getAPIEntry(apiRequest, ref APIList);
                    }

                    return APIList;
                },
                "a.apiSubSet", 
                "Subset of the API Listing [string api1, string api2, ... , string apiN]", 
                new APIEntryJSONFormatter()));

            registerAPI(new APIEntry(
                dataSources => { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); },
                "a.version", "Telemachus Version", new StringJSONFormatter()));
        }

        #endregion
    }

    public class CompoundDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public CompoundDataLinkHandler(List<DataLinkHandler> APIHandlers)
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

    public class DefaultDataLinkHandler : DataLinkHandler
    {
        #region DataLinkHandler

        public override bool process(String API, out APIEntry result)
        {
            throw new SoftException("Bad data link reference.");
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

        #endregion

        #region Initialisation

        public DataLinkHandler()
        {
            nullAPI = new APIEntry(
                dataSources =>
                {
                    return pausedHandler();
                },
                "null", "null");
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
                   result =  nullAPI;
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

        public enum UnitType { UNITLESS, VELOCITY, DEG, DISTANCE, TIME};

        #endregion

        #region Fields

        public DataLinkHandler.APIDelegate function { get; set; }
        public string APIString { get; set; }
        public string name { get; set; }
        public UnitType units { get; set; }
        public bool plotable { get; set; }
        public DataSourceResultFormatter formatter { get; set;}

        #endregion

        #region Initialisation

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString, string name)
        {
            this.function = function;
            this.APIString = APIString;
            this.name = name;
            this.formatter = new DefaultJSONFormatter();
            this.units = UnitType.UNITLESS;
            plotable = false;
            
        }

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString, string name, 
            DataSourceResultFormatter formatter)
        {
            this.function = function;
            this.APIString = APIString;
            this.name = name;
            this.formatter = formatter;
            this.units = UnitType.UNITLESS;
            plotable = false;
        }

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString, string name,
           DataSourceResultFormatter formatter, UnitType units, bool plotable)
        {
            this.function = function;
            this.APIString = APIString;
            this.name = name;
            this.formatter = formatter;
            this.units = units;
            this.plotable = plotable;
        }

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString, string name,
            UnitType units, bool plotable)
        {
            this.function = function;
            this.APIString = APIString;
            this.name = name;
            this.formatter = new DefaultJSONFormatter();
            this.units = units;
            this.plotable = plotable;
        }

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString, string name,
           bool plotable)
        {
            this.function = function;
            this.APIString = APIString;
            this.name = name;
            this.formatter = new DefaultJSONFormatter();
            this.units = units;
            this.plotable = plotable;
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
            :base(function, "", "")
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
