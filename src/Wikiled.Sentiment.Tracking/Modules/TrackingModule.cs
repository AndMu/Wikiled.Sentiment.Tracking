using System;
using Autofac;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Sentiment.Tracking.Modules
{
    public class TrackingModule : Module
    {
        private readonly TrackingConfiguration configuration;

        public TrackingModule(TrackingConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Tracker>().As<ITracker>();
            builder.RegisterType<ExpireTracking>().As<ITrackingRegister>().SingleInstance();
            builder.RegisterType<TrackingStream>().As<ITrackingRegister>().As<IRatingStream>().SingleInstance();

            builder.RegisterType<PersistencyTracking>().SingleInstance();
            builder.RegisterType<TrackingManager>().As<ITrackingManager>().SingleInstance();
            builder.RegisterType<TrackerFactory>().As<ITrackerFactory>().SingleInstance();
            builder.RegisterInstance(configuration);

            if (configuration.Restore)
            {
                builder.RegisterType<Restorer>().As<IRestorer>();
            }
            else
            {
                builder.RegisterType<NullRestorer>().As<IRestorer>();
            }
        }
    }
}
