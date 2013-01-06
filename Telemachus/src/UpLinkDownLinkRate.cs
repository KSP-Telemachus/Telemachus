//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    public class UpLinkDownLinkRate
    {
        #region Fields

        private static TimeSpan TIME_SPAN_5_SECONDS = new TimeSpan(0,0,5);
        private static DateTime TIME_ARBITRARY = System.DateTime.Now;

        private int averageSize = 0;

        LinkedList<KeyValuePair<DateTime, int>> upLinkRate = new LinkedList<KeyValuePair<DateTime, int>>();
        LinkedList<KeyValuePair<DateTime, int>> downLinkRate = new LinkedList<KeyValuePair<DateTime, int>>();

        #endregion

        #region Constructors

        public UpLinkDownLinkRate(int averageSize)
        {
            this.averageSize = averageSize;
        }

        #endregion

        #region Accessors

        public void addUpLinkPoint(DateTime time, int bytes)
        {
            addGuardedPoint(time, bytes, upLinkRate);
        }

        public void addDownLinkPoint(DateTime time, int bytes)
        {
            addGuardedPoint(time, bytes, downLinkRate);
        }

        public double getDownLinkRate()
        {
            return average(downLinkRate);
        }

        public double getUpLinkRate()
        {
            return average(upLinkRate);
        }

        #endregion

        #region Private Methods

        private void addGuardedPoint(DateTime time, int bytes, LinkedList<KeyValuePair<DateTime, int>> rate)
        {
            lock (rate)
            {
                rate.AddFirst(new KeyValuePair<DateTime, int>(time, bytes));

                if (rate.Count >= averageSize)
                {
                    rate.RemoveLast();
                }
            }
        }

        private double average(LinkedList<KeyValuePair<DateTime, int>> rate)
        {
            lock(rate)
            {
                if (rate.Count > 1)
                {
                    DateTime newestTime = TIME_ARBITRARY, lastTime = TIME_ARBITRARY, thresholdTime = System.DateTime.Now.Subtract(TIME_SPAN_5_SECONDS);
                    long totalBytes = 0;

                    foreach (KeyValuePair<DateTime, int> point in rate)
                    {
                        if (totalBytes == 0)
                        {
                            newestTime = point.Key;
                        }

                        totalBytes += point.Value;
                        lastTime = point.Key;

                        if (point.Key < thresholdTime)
                        {    
                            break;
                        }
                    }

                    return ((double)totalBytes) / newestTime.Subtract(lastTime).TotalSeconds;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion
    }
}
