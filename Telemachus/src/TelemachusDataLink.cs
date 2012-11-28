using System;
using UnityEngine;
using KSP.IO;
using System.Net;
using System.Text;
using RedCorona.Net;
using System.Collections.Generic;
using System.Collections;

namespace Telemachus
{
    public class TelemachusDataLink : Part
    {
        List<SensorBuffer> sensorBuffers = new List<SensorBuffer>();
        static HttpServer http = null;

        protected override void onFlightStart()
        {
            startDataLink();
            base.onFlightStart();
        }

        protected override void onDisconnect()
        {
            stopDataLink();
            base.onDisconnect();
        }

        protected override void  onPartDestroy()
        {   
            stopDataLink();
            base.onPartDestroy();
        }

        private void startDataLink()
        {
            if (http == null)
            {
                UnityEngine.Debug.Log("Telemachus data link starting");

                UnityEngine.Debug.Log("Telemachus data link creating sensor buffers");

                sensorBuffers.Add(new Altitude(this.vessel, 300, "altitude"));
                sensorBuffers.Add(new Density(this.vessel, 300, "density"));
                sensorBuffers.Add(new GeeForce(this.vessel, 300, "geeforce"));

                http = new HttpServer(new Server(8080));
                http.Handlers.Add(new DataLocator(this));

                UnityEngine.Debug.Log("Telemachus data link listening for requests");
            }
        }

        private void stopDataLink()
        {
            if (http != null)
            {
                UnityEngine.Debug.Log("Telemachus data link shutting down.");
                http.Close();
                http = null;
            }
        }

        public SensorBuffer ContainsBuffer(string name)
        {
            foreach (SensorBuffer sb in sensorBuffers)
            {
                if (sb.getName().Equals(name))
                {
                    return sb;
                }
            }

            return null;
        }
    }

    public abstract class SensorBuffer
    {
        protected Queue<double> x = null;
        protected Queue<double> y = null;
        protected Vessel vessel = null;
        protected int clamp = 100;
        protected String name = "";


        public String getName()
        {
            return name;
        }

        public Queue<double> getX()
        {
            return x;
        }

        public Queue<double> getY()
        {
            return y;
        }

        public SensorBuffer(Vessel vessel, int clamp, string name)
        {
            x = new Queue<double>();
            y = new Queue<double>();
            this.vessel = vessel;
            this.clamp = clamp;
            this.name = name;
        }

        abstract public void Update();

        protected void clampDataSet(Queue<double> list, int clamp)
        {
            if (list.Count > clamp)
            {
                list.Dequeue();
            }
        }

        override public String ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[x:");
            foreach (double d in x)
            {
                sb.Append(d.ToString());
                sb.Append(",");
            }
            sb.Remove(sb.Length-1,1);
            sb.Append("]");


            
            return sb.ToString();
        }

        public String ToJavaScript()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[['x','y'],");

            IEnumerator xenum = x.GetEnumerator();
            IEnumerator yenum = y.GetEnumerator();
            xenum.Reset();
            yenum.Reset();

            while (xenum.MoveNext())
            {

                yenum.MoveNext();
                sb.Append("[" + xenum.Current + "," + yenum.Current + "]");
                sb.Append(",");
            }
           
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");

            return sb.ToString();
        }
    }

    public class Altitude : SensorBuffer
    {
        public Altitude(Vessel vessel, int clamp, String name) : base(vessel, clamp, name)
        {
        }

        override public void Update()
        {
            y.Enqueue(vessel.altitude);
            clampDataSet(y, clamp);

            x.Enqueue(vessel.missionTime);
            clampDataSet(x, clamp);
        }
    }

    public class Density : SensorBuffer
    {
        public Density(Vessel vessel, int clamp, String name)
            : base(vessel, clamp, name)
        {
        }

        override public void Update()
        {
            y.Enqueue(vessel.atmDensity);
            clampDataSet(y, clamp);

            x.Enqueue(vessel.missionTime);
            clampDataSet(x, clamp);
        }
    }

    public class GeeForce : SensorBuffer
    {
        public GeeForce(Vessel vessel, int clamp, String name)
            : base(vessel, clamp, name)
        {
        }

        override public void Update()
        {
            y.Enqueue(vessel.geeForce);
            clampDataSet(y, clamp);

            x.Enqueue(vessel.missionTime);
            clampDataSet(x, clamp);
        }
    }
}

