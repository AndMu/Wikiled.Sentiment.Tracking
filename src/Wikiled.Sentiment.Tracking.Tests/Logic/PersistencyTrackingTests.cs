using System;
using System.IO;
using System.Reactive;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.MachineLearning.Tests.Mathematics.Tracking
{
    [TestFixture]
    public class PersistencyTrackingTests
    {
        private TrackingConfiguration mockTrackingConfiguration;

        private Mock<IRatingStream> mockRatingStream;

        private Mock<IRestorer> mockRestorer;

        private PersistencyTracking instance;

        private TestScheduler scheduler;

        [SetUp]
        public void SetUp()
        {
            scheduler = new TestScheduler();
            mockRestorer = new Mock<IRestorer>();
            mockTrackingConfiguration = new TrackingConfiguration(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), Path.Combine(TestContext.CurrentContext.TestDirectory, "file.csv"));
            if (File.Exists(mockTrackingConfiguration.Persistency))
            {
                File.Delete(mockTrackingConfiguration.Persistency);
            }

            mockRatingStream = new Mock<IRatingStream>();
            var tracker = new Mock<ITracker>();
            var stream = scheduler.CreateHotObservable(
                new Recorded<Notification<(ITracker Tracker, RatingRecord Rating)>>(100, Notification.CreateOnNext((tracker.Object, new RatingRecord("1", DateTime.Now, 2)))));
            mockRatingStream.Setup(item => item.Stream).Returns(stream);
            instance = CreateTrackingPersistency();
        }

        [TearDown]
        public void Teardown()
        {
            instance.Dispose();
        }

       [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new PersistencyTracking(new NullLogger<PersistencyTracking>(), null, mockRatingStream.Object, mockRestorer.Object));
            Assert.Throws<ArgumentNullException>(() => new PersistencyTracking(new NullLogger<PersistencyTracking>(), mockTrackingConfiguration, null, mockRestorer.Object));
            Assert.Throws<ArgumentNullException>(() => new PersistencyTracking(null, mockTrackingConfiguration, mockRatingStream.Object, mockRestorer.Object));
            Assert.Throws<ArgumentNullException>(() => new PersistencyTracking(new NullLogger<PersistencyTracking>(), mockTrackingConfiguration, mockRatingStream.Object, null));
        }

        [Test]
        public void Logic()
        {
            scheduler.AdvanceBy(100);
            instance.Dispose();
            Assert.IsTrue(File.Exists(mockTrackingConfiguration.Persistency));
        }

        private PersistencyTracking CreateTrackingPersistency()
        {
            return new PersistencyTracking(new NullLogger<PersistencyTracking>(), mockTrackingConfiguration, mockRatingStream.Object, mockRestorer.Object);
        }
    }
}
