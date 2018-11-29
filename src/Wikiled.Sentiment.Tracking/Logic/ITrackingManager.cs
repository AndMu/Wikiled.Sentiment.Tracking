using System.Collections.Generic;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public interface ITrackingManager
    {
        IEnumerable<ITracker> AllTrackers { get; }

        ITracker Resolve(string key, string type);
    }
}