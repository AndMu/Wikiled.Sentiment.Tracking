using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public class ExpireTracking : ITrackingRegister
    {
        private readonly ILogger<ExpireTracking> logger;

        private readonly IDisposable subscription;

        private readonly ConcurrentBag<ITracker> trackers = new ConcurrentBag<ITracker>();

        private readonly TrackingConfiguration config;

        public ExpireTracking(IScheduler scheduler, ILogger<ExpireTracking> logger, TrackingConfiguration config)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }
            
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            logger.LogDebug("Constructing. Expire: {0} Scan: {0}", config.Expire, config.ScanTime);
            subscription = Observable.Interval(config.ScanTime, scheduler).Subscribe(item => Scan());
        }

        public void Register(ITracker tracker)
        {
            logger.LogDebug("Register");
            if (tracker == null)
            {
                throw new ArgumentNullException(nameof(tracker));
            }

            trackers.Add(tracker);
        }

        public void Dispose()
        {
            logger.LogDebug("Dispose");
            subscription?.Dispose();
        }

        private void Scan()
        {
            logger.LogDebug("Scan");
            Parallel.ForEach(trackers, item => item.TrimOlder(config.Expire));
        }
    }
}
