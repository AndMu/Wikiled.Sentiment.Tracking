using System;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public class TrackingConfiguration
    {
        public TrackingConfiguration(TimeSpan scanTime, TimeSpan expire, string persistency)
        {
            ScanTime = scanTime;
            Expire = expire;
            Persistency = persistency;
        }

        public TimeSpan ScanTime { get; }

        public TimeSpan Expire { get; }

        public string Persistency { get; }

        public bool Restore { get; set; }
    }
}
