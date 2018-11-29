using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public class TrackingManager : ITrackingManager
    {
        private readonly ITrackerFactory trackerFactory;

        private readonly ConcurrentDictionary<string, Lazy<ITracker>> trackers = new ConcurrentDictionary<string, Lazy<ITracker>>(StringComparer.OrdinalIgnoreCase);

        public TrackingManager(ITrackerFactory trackerFactory)
        {
            this.trackerFactory = trackerFactory ?? throw new ArgumentNullException(nameof(trackerFactory));
        }

        public IEnumerable<ITracker> AllTrackers => trackers.Values.Select(item => item.Value);

        public ITracker Resolve(string key, string type)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var result = trackers.GetOrAdd($"{key}_{type}", new Lazy<ITracker>(() => trackerFactory.Construct(key, type)));
            return result.Value;
        }
    }
}
