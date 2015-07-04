using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;

namespace Telemachus
{
    public class UpdateTimer
    {
        public event EventHandler<UpdateTimerEventArgs> Elapsed;
        private float lastEventTime = 0.0f;

        private float interval = 0.0f;
        public float Interval
        {
            get { return interval; }
            set { interval = value; }
        }

        private bool enabled = false;
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public void update(object sender, UpdateTimerEventArgs timeElapsedEventArgs)
        {
            if ((timeElapsedEventArgs.Time - lastEventTime > interval) && Enabled)
            {
                if (Elapsed != null)
                {
                    Elapsed(this, timeElapsedEventArgs);
                }

                lastEventTime = timeElapsedEventArgs.Time;
            }
        }
    }

    public class UpdateTimerEventArgs : EventArgs
    {
        private float time = 0.0f;

        public float Time 
        {
            get { return time; }
        }

        public UpdateTimerEventArgs(float time)
        {
            this.time = time;
        }
    }
}
