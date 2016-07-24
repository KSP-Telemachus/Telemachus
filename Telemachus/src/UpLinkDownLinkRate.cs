//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    public class UpLinkDownLinkRate
    {
        #region Constants

        public const int DEFAULT_AVERAGE_SIZE = 20;

        #endregion

        #region Fields

        private static TimeSpan TIME_SPAN_5_SECONDS = new TimeSpan(0, 0, 5);
        private static DateTime TIME_ARBITRARY = System.DateTime.Now;

        private int averageSize = DEFAULT_AVERAGE_SIZE;

        private LinkedList<KeyValuePair<DateTime, int>> upLinkRate = new LinkedList<KeyValuePair<DateTime, int>>();
        private LinkedList<KeyValuePair<DateTime, int>> downLinkRate = new LinkedList<KeyValuePair<DateTime, int>>();

        #endregion

        #region Constructors

        public UpLinkDownLinkRate()
        {

        }

        public UpLinkDownLinkRate(int averageSize)
        {
            this.averageSize = averageSize;
        }

        #endregion

        #region Accessors

        /// <summary>Add a data point for bytes recieved from the client (UPlink)</summary>
        public void RecieveDataFromClient(int bytes)
        {
            // Convert to bits for the data rate
            addGuardedPoint(DateTime.Now, bytes*8, upLinkRate);
        }

        /// <summary>Add a data point for bytes sent to the client (DOWNlink)</summary>
        public void SendDataToClient(int bytes)
        {
            // Convert to bits for the data rate
            addGuardedPoint(DateTime.Now, bytes*8, downLinkRate);
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
            lock (rate)
            {
                if (rate.Count > 1)
                {
                    DateTime newestTime = TIME_ARBITRARY, lastTime = TIME_ARBITRARY, thresholdTime = System.DateTime.Now.Subtract(TIME_SPAN_5_SECONDS);
                    long totalBytes = 0;
                    int irel = 0;

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

                        irel++;
                    }

                    if (irel > 0)
                    {
                        double delta = newestTime.Subtract(lastTime).TotalSeconds;

                        if (delta == 0)
                        {
                            return (double)totalBytes;
                        }
                        else
                        {
                            return ((double)totalBytes) / delta;
                        }
                    }
                    else
                    {
                        return 0;
                    }
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
