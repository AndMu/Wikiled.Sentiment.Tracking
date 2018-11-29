using System;
using System.IO;
using Autofac;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Wikiled.Common.Utilities.Helpers;
using Wikiled.Common.Utilities.Modules;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Sentiment.Tracking.Modules;

namespace Wikiled.Sentiment.Tracking.Tests.Acceptance
{
    [TestFixture]
    public class AcceptanceTests
    {
        private TrackingConfiguration config;

        [SetUp]
        public void Setup()
        {
            config = new TrackingConfiguration(TimeSpan.FromMinutes(1),
                                               TimeSpan.FromDays(2),
                                               Path.Combine(TestContext.CurrentContext.TestDirectory, "result.csv"));
            config.Restore = true;
        }

        [Test]
        public void TestCycle()
        {
            using (var container = Create().Build())
            {
                var manager = container.Resolve<ITrackingManager>();
                manager.Resolve("Test", "Type1").AddRating(new RatingRecord("1", DateTime.Now, 2));
                manager.Resolve("Test", "Type1").AddRating(new RatingRecord("1", DateTime.Now, null));
                manager.Resolve("Test", "Type2").AddRating(new RatingRecord("1", DateTime.Now, 2));
            }

            using (var container = Create().Build())
            {
                var manager = container.Resolve<ITrackingManager>();
                var ratings = manager.Resolve("Test", "Type1").GetRatings();
                Assert.AreEqual(1, ratings.Length);
            }
        }

        private ContainerBuilder Create()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new TrackingModule(config));
            builder.RegisterModule<CommonModule>();
            builder.RegisterModule(new LoggingModule(new LoggerFactory()));
            return builder;
        }
    }
}
