using System;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Utilities.Config;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public class TrackerFactory : ITrackerFactory
    {
        private readonly ILogger<Tracker> logger;

        private readonly IApplicationConfiguration config;

        private readonly ITrackingRegister[] register;

        public TrackerFactory(ILogger<Tracker> logger, IApplicationConfiguration config, ITrackingRegister[] register)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.register = register ?? throw new ArgumentNullException(nameof(register));

            if (register.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(register));
            }
        }

        public ITracker Construct(string name, string type)
        {
            var tracker = new Tracker(name, type, logger, config);
            foreach (var item in register)
            {
                item.Register(tracker);
            }
            
            return tracker;
        }
    }
}
