//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MinimalHTTPServer;
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
            buildAPI();
        }

        protected void buildAPI()
        {
            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => { return reflectOff(dataSources); }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
               "mj.smartassoff", "Smart ASS Off"));

            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => { return reflectAttitudeTo(dataSources, Vector3d.forward, "MANEUVER_NODE"); }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
               "mj.node", "Node"));

            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => { return reflectAttitudeTo(dataSources, Vector3d.forward, "ORBIT"); }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
               "mj.prograde", "Prograde"));

            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => { return reflectAttitudeTo(dataSources, Vector3d.back, "ORBIT"); }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
               "mj.retrograde", "Retrograde"));

            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => {  return reflectAttitudeTo(dataSources, Vector3d.left, "ORBIT"); }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
               "mj.normalplus", "Normal Plus"));

            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => {  return reflectAttitudeTo(dataSources, Vector3d.right, "ORBIT"); }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
               "mj.normalminus", "Normal Minus"));

            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => {  return reflectAttitudeTo(dataSources, Vector3d.up, "ORBIT"); }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
               "mj.radialplus", "Radial Plus"));

            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => {  return reflectAttitudeTo(dataSources, Vector3d.down, "ORBIT"); }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
               "mj.radialminus", "Radial Minus"));

            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => {  return surface(dataSources); }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
               "mj.surface", "Surface"));
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

        private Type getAttitudeType(DataSources dataSources, ref object attitude)
        {
            PartModule mechJebCore = findMechJeb(dataSources.vessel);

            if (mechJebCore == null)
            {
                PluginLogger.Log("[Telemachus] No Mechjeb part installed.");
                return null;
            }
            else
            {
                try
                {   
                    PluginLogger.Log("[Telemachus] Mechjeb part installed, reflecting.");
                    Type mechJebCoreType = mechJebCore.GetType();
                    FieldInfo attitudeField = mechJebCoreType.GetField("attitude", BindingFlags.Public | BindingFlags.Instance);
                    attitude = attitudeField.GetValue(mechJebCore);

                    Type attitudeReferenceType = attitude.GetType().GetProperty("attitudeReference",
                        BindingFlags.Public | BindingFlags.Instance).GetValue(attitude, null).GetType();

                    return attitude.GetType();
                }
                catch (Exception e)
                {
                    PluginLogger.Log("[Telemachus]" + e.Message + " " +  e.StackTrace);
                }

                return null;
            }
        }

        private static PartModule findMechJeb(Vessel vessel)
        {
            List<Part> pl = vessel.parts.FindAll(p => p.Modules.Contains("MechJebCore"));

            foreach (PartModule m in pl[0].Modules)
            {
                if (m.GetType().Name.Equals("MechJebCore"))
                {
                    return m;
                }
            }

            return null;
        }
  
        #endregion
    }

    public class FlightDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public FlightDataLinkHandler()
        {
            buildAPI();
        }

        protected void buildAPI()
        {
            registerAPI(new APIEntry(
                dataSources => { TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                 (x) => { Staging.ActivateNextStage(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                }, "f.stage", "Stage"));

            registerAPI(new APIEntry(
               dataSources =>
               {
                   TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                       (x) => { throttleUp(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
               },
                "f.throttleUp", "Throttle Up"));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { throttleZero(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
                "f.throttleZero", "Throttle Zero"));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { throttleFull(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
                "f.throttleFull", "Throttle Full"));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { throttleDown(); return 0d; }), UnityEngine.SendMessageOptions.DontRequireReceiver); return 0d;
                },
                "f.throttleDown", "Throttle Down"));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { dataSources.vessel.ActionGroups.ToggleGroup(KSPActionGroup.RCS); return 0d; }), 
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return false;
                },
                "f.rcs", "RCS"));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.SAS),
                "f.sas", "SAS"));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Light),
                "f.light", "Light"));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Gear),
                "f.gear", "Gear"));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Brakes),
                "f.brake", "Brake"));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Abort),
                "f.abort", "Abort"));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom01),
                "f.ag1", "AG1"));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom02),
                "f.ag2", "AG2"));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom03),
                "f.ag3", "AG3"));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom04),
                "f.ag4", "AG4"));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom05),
                "f.ag5", "AG5"));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom06),
                "f.ag6", "AG6"));

            registerAPI(new APIEntry(
               buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom07),
                "f.ag7", "AG7"));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom08),
                "f.ag8", "AG8"));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom09),
                "f.ag9", "AG9"));

            registerAPI(new APIEntry(
                buildActionGroupToggleDelayedLamda(KSPActionGroup.Custom10),
                "f.ag10", "AG10"));
        }

        private DataLinkHandler.APIDelegate buildActionGroupToggleDelayedLamda(KSPActionGroup actionGroup)
        {
            return dataSources =>
                {
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI", new DelayedAPIEntry(dataSources,
                        (x) => { dataSources.vessel.ActionGroups.ToggleGroup(actionGroup); return 0d; }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver); return false;
                };
        }

        #endregion

        #region DataLinkHandler
        
        protected override bool pausedHandler()
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

        #endregion
    }

    public class VesselDataLinkHandler : DataLinkHandler
    {
        #region Initialisation
        
        public VesselDataLinkHandler()
        {
            buildAPI();
        }
        
        protected void buildAPI()
        {
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.altitude; }, 
                "v.altitude", "Altitude"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.heightFromTerrain; }, 
                "v.heightFromTerrain", "Height from Terrain"));
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
                "v.orbitalVelocity", "Orbital Velocity"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.horizontalSrfSpeed; },
                "v.surfaceSpeed", "Surface Speed"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.verticalSpeed; },
                "v.verticalSpeed", "Vertical Speed"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.atmDensity; },
                "v.atmosphericDensity", "Atmospheric Density"));
        }

        #endregion
    }

    public class OrbitDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public OrbitDataLinkHandler()
        {
            buildAPI();
        }

        protected void buildAPI()
        {
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.PeA; },
                "o.PeA", "PeA"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.ApA; },
                "o.ApA", "ApA"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.timeToAp; },
                "o.timeToAp", "Time to Ap"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.timeToPe; },
                "o.timeToPe", "Time to Pe"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.inclination; },
                "o.inclination", "Inclination"));
            registerAPI(new APIEntry(
                dataSources => { return dataSources.vessel.orbit.argumentOfPeriapsis; },
                "o.argumentOfPeriapsis", "Argument of Periapsis"));
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
            buildAPI();
            vesselChangeDetector.suscribe(new VesselChangeDetector.VesselChange(vesselChanged));
        }

        protected void buildAPI()
        {
            registerAPI(new APIEntry(
                dataSources => { return getsSensorValues(dataSources, "TEMP"); },
                "s.temperature", "Temperature"));
            registerAPI(new APIEntry(
                dataSources => { return getsSensorValues(dataSources, "ACC"); },
                "s.acceleration", "Acceleration"));
            registerAPI(new APIEntry(
                dataSources => { return getsSensorValues(dataSources, "GRAV"); },
                "s.gravity", "Gravity"));
            registerAPI(new APIEntry(
                dataSources => { return getsSensorValues(dataSources, "PRES"); },
                "s.pressure", "Pressure"));
        }

        #endregion

        #region Sensors

        protected List<ModuleEnviroSensor> getsSensorValues(DataSources datasources, string ID)
        {
            return sensorCache.get(ID, datasources);
        }

        #endregion

        #region VesselChangeDetector

        private void vesselChanged(Vessel vessel)
        {
            sensorCache.setDirty();
        }

        #endregion
    }

    public class SensorCache
    {
        #region Fields

        ReaderWriterLock updateLock = new ReaderWriterLock(), dirtyLock = new ReaderWriterLock();
        Dictionary<string, List<ModuleEnviroSensor>> sensors = new Dictionary<string, List<ModuleEnviroSensor>>();
        bool isDirty = true;

        #endregion

        #region Cache

        private void setDirty(bool value)
        {
            dirtyLock.AcquireWriterLock(Timeout.Infinite);
            isDirty = value;
            dirtyLock.ReleaseWriterLock();
        }

        private bool checkDirty()
        {
            dirtyLock.AcquireReaderLock(Timeout.Infinite);
            bool ret = isDirty;
            dirtyLock.ReleaseReaderLock();

            return ret;
        }

        public void setDirty()
        {
            setDirty(true);
        }

        public void refresh(Vessel vessel, ReaderWriterLock inputLock)
        {
            LockCookie lockCookie = inputLock.UpgradeToWriterLock(Timeout.Infinite);

            try
            {
                sensors.Clear();

                List<Part> partsWithSensors = vessel.parts.FindAll(p => p.Modules.Contains("ModuleEnviroSensor"));

                foreach (Part part in partsWithSensors)
                {
                    foreach (var module in part.Modules)
                    {
                        if (module.GetType().Equals(typeof(ModuleEnviroSensor)))
                        {
                            List<ModuleEnviroSensor> list = null;
                            sensors.TryGetValue(((ModuleEnviroSensor)module).sensorType, out list);
                            if (list == null)
                            {
                                list = new List<ModuleEnviroSensor>();
                                sensors[((ModuleEnviroSensor)module).sensorType] = list;
                            }

                            list.Add((ModuleEnviroSensor)module);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                PluginLogger.Log(e.Message);
            }

            updateLock.DowngradeFromWriterLock(ref lockCookie);
        }

        public List<ModuleEnviroSensor> get(string ID, DataSources dataSources)
        {
            List<ModuleEnviroSensor> avail = null, ret = null;

            updateLock.AcquireReaderLock(Timeout.Infinite);

            if (checkDirty())
            {
                refresh(dataSources.vessel, updateLock);
            }

            sensors.TryGetValue(ID, out avail);

            if (avail != null)
            {
                ret = new List<ModuleEnviroSensor>(avail);
            }
            else
            {
                ret = new List<ModuleEnviroSensor>();
            }
 
            updateLock.ReleaseReaderLock();
            
            return ret;
        }

        #endregion
    }

    public class ResourceDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public ResourceDataLinkHandler()
        {
            buildAPI();
        }

        protected void buildAPI()
        {
            
        }

        #endregion
    }

    public class PausedDataLinkHandler : DataLinkHandler
    {
        #region Initialisation

        public PausedDataLinkHandler()
        {
            buildAPI();
        }

        protected void buildAPI()
        {
            registerAPI(new APIEntry(
                dataSources => { return partPaused(); },
                "p.paused", "Paused"));
        }

        #endregion

        #region Methods

        public static bool partPaused()
        {
            return FlightDriver.Pause ||  
                !TelemachusPowerDrain.isActive || 
                !TelemachusPowerDrain.activeToggle || 
                !VesselChangeDetector.hasTelemachusPart;
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
        APIEntry nullAPI = new APIEntry(
                dataSources =>
                {
                    return false;
                },
                "null", "null");

        #endregion

        #region DataLinkHandler

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
                if (!pausedHandler())
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

        public void appendAPIList(ref List<KeyValuePair<String, String>> APIList)
        {
           
            foreach (KeyValuePair<String, APIEntry> entry in APIEntries)
            {

                    APIList.Add(new KeyValuePair<string, string>(
                        entry.Key, entry.Value.name));
            }
        }

        protected void registerAPI(APIEntry entry)
        {
            APIEntries.Add(entry.APIString, entry);
        }

        protected virtual bool pausedHandler()
        {
            return false;
        }

        #endregion
    }

    public class APIEntry
    {
        #region Fields

        public DataLinkHandler.APIDelegate function { get; set; }
        public string APIString { get; set; }
        public string name { get; set; }

        #endregion

        #region Initialisation

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString, string name)
        {
            this.function = function;
            this.APIString = APIString;
            this.name = name;
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
                PluginLogger.Log(e.Message);
            }
        }

        #endregion
    }
}
