using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Sentiment.Tracking.Tests.Logic
{
    [TestFixture]
    public class ExpireTrackingTests
    {
        private TestScheduler scheduler;

        private ILogger<ExpireTracking> logger;

        private TrackingConfiguration trackingConfiguration;

        private ExpireTracking instance;

        private Mock<ITracker> tracker;

        [SetUp]
        public void SetUp()
        {
            tracker = new Mock<ITracker>();
            scheduler = new TestScheduler();
            logger = new NullLogger<ExpireTracking>();
            trackingConfiguration = new TrackingConfiguration(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), "Data.csv");
            instance = CreateInstance();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new ExpireTracking(null, logger, trackingConfiguration));
            Assert.Throws<ArgumentNullException>(() => new ExpireTracking(scheduler, null, trackingConfiguration));
            Assert.Throws<ArgumentNullException>(() => new ExpireTracking(scheduler, logger, null));
        }

        [Test]
        public void ExpireArguments()
        {
            Assert.Throws<ArgumentNullException>(() => instance.Register(null));
        }

        [Test]
        public void Expire()
        {
            instance.Register(tracker.Object);
            scheduler.AdvanceBy(TimeSpan.FromMinutes(2).Ticks);
            tracker.Verify(item => item.TrimOlder(TimeSpan.FromMinutes(1)), Times.Exactly(2));
        }

        [Test]
        public void Dispose()
        {
            instance.Register(tracker.Object);
            instance.Dispose();
            scheduler.AdvanceBy(TimeSpan.FromMinutes(2).Ticks);
            tracker.Verify(item => item.TrimOlder(TimeSpan.FromMinutes(1)), Times.Never);
        }

        private ExpireTracking CreateInstance()
        {
            return new ExpireTracking(scheduler, logger, trackingConfiguration);
        }
    }
}
