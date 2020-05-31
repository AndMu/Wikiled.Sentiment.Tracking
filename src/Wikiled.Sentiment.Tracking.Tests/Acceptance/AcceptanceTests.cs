using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
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
            var file = Path.Combine(TestContext.CurrentContext.TestDirectory, "Test", "result.csv");
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            config = new TrackingConfiguration(TimeSpan.FromMinutes(1), TimeSpan.FromDays(2), file);
            config.Restore = true;
        }

        [Test]
        public void TestCycle()
        {
            using (var container = Create().BuildServiceProvider())
            {
                container.GetRequiredService<IHostedService>();
                var manager = container.GetRequiredService<ITrackingManager>();
                manager.Resolve("Test", "Type1").AddRating(new RatingRecord { Id = "1", Date = DateTime.Now, Rating = 2 });
                manager.Resolve("Test", "Type1").AddRating(new RatingRecord { Id = "2", Date = DateTime.Now});
                manager.Resolve("Test", "Type2").AddRating(new RatingRecord { Id = "1", Date = DateTime.Now, Rating = 2 });
            }

            using (var container = Create().BuildServiceProvider())
            {
                container.GetRequiredService<IHostedService>();
                var manager = container.GetRequiredService<ITrackingManager>();
                var ratings = manager.Resolve("Test", "Type1").GetRatings();
                Assert.AreEqual(1, ratings.Length);
            }
        }

        private ServiceCollection Create()
        {
            var builder = new ServiceCollection();
            builder.RegisterModule(new TrackingModule(config));
            builder.RegisterModule<CommonModule>();
            builder.RegisterModule(new LoggingModule(new LoggerFactory()));
            return builder;
        }
    }
}
