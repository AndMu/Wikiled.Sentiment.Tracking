using System;
using Microsoft.Extensions.DependencyInjection;
using Wikiled.Common.Utilities.Modules;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Sentiment.Tracking.Modules
{
    public class TrackingModule : IModule
    {
        private readonly TrackingConfiguration configuration;

        public TrackingModule(TrackingConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ITracker, Tracker>();
            services.AddSingleton<ITrackingRegister, ExpireTracking>();
            services.AddSingleton<TrackingStream>().As<ITrackingRegister, TrackingStream>().As<IRatingStream, TrackingStream>();

            services.AddSingleton<ITrackerFactory, TrackerFactory>();
            services.AddSingleton<ITrackingManager, TrackingManager>();
            services.AddSingleton(configuration); 

            if (!string.IsNullOrEmpty(configuration.Persistency))
            {
                services.AddHostedService<PersistencyTracking>();
                if (configuration.Restore)
                {
                    services.AddTransient<IRestorer, Restorer>();
                }
                else
                {
                    services.AddTransient<IRestorer, NullRestorer>();
                }
            }

            return services;
        }
    }
}
