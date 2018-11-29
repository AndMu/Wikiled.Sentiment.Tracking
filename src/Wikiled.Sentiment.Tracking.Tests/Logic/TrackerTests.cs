using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using System;
using Wikiled.Common.Utilities.Config;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.MachineLearning.Tests.Mathematics.Tracking
{
    [TestFixture]
    public class TrackerTests : ReactiveTest
    {
        private ILogger<Tracker> logger;

        private Mock<IApplicationConfiguration> mockApplicationConfiguration;

        private Tracker instance;

        private TestScheduler scheduler;

        [SetUp]
        public void SetUp()
        {
            scheduler = new TestScheduler();
            logger = new NullLogger<Tracker>();
            mockApplicationConfiguration = new Mock<IApplicationConfiguration>();
            instance = CreateTracker();
            mockApplicationConfiguration.Setup(item => item.Now).Returns(new DateTime(2016, 01, 10));
        }

        [Test]
        public void AddRatingArguments()
        {
            Assert.Throws<ArgumentNullException>(() => instance.AddRating(null));
        }

        [Test]
        public void Stream()
        {
            ITestableObserver<RatingRecord> observer = scheduler.CreateObserver<RatingRecord>();
            instance.Ratings.Subscribe(observer);
            instance.AddRating(new RatingRecord("1", new DateTime(2016, 01, 10), 10));
            scheduler.AdvanceBy(100);
            instance.Dispose();
            observer.Messages.AssertEqual(
                OnNext<RatingRecord>(0, item => true),
                OnCompleted<RatingRecord>(100));
        }

        [Test]
        public void TrimOlder()
        {
            double? result = instance.CalculateAverageRating();
            Assert.IsNull(result);
            instance.AddRating(new RatingRecord("1", new DateTime(2016, 01, 10), 10));
            instance.AddRating(new RatingRecord("2", new DateTime(2016, 01, 10), 10));

            mockApplicationConfiguration.Setup(item => item.Now).Returns(new DateTime(2016, 01, 11));
            Assert.AreEqual(0, instance.Count());
            Assert.AreEqual(2, instance.Count(lastHours: 48));
            Assert.IsTrue(instance.IsTracked("1"));
            instance.TrimOlder(TimeSpan.FromHours(1));
            Assert.AreEqual(0, instance.Count(lastHours: 48));
            Assert.IsFalse(instance.IsTracked("1"));
        }

        [Test]
        public void AddSameId()
        {
            Assert.IsFalse(instance.IsTracked("1"));
            instance.AddRating(new RatingRecord("1", new DateTime(2016, 01, 10), 10));
            Assert.IsTrue(instance.IsTracked("1"));
            instance.AddRating(new RatingRecord("1", new DateTime(2016, 01, 10), 10));
            double? result = instance.CalculateAverageRating();
            Assert.AreEqual(1, instance.Count());
            Assert.AreEqual(10, result);
        }

        [Test]
        public void AverageSentiment()
        {
            double? result = instance.CalculateAverageRating();
            Assert.IsNull(result);
            instance.AddRating(new RatingRecord("1", new DateTime(2016, 01, 10), 10));
            instance.AddRating(new RatingRecord("2", new DateTime(2016, 01, 10), null));
            result = instance.CalculateAverageRating();
            Assert.AreEqual(1, instance.Count());
            Assert.AreEqual(2, instance.Count(false));
            Assert.AreEqual(10, result);

            instance.AddRating(new RatingRecord("3", new DateTime(2016, 01, 11), -10));
            result = instance.CalculateAverageRating();
            var ratings = instance.GetRatings();
            Assert.AreEqual(2, ratings.Length);
            Assert.AreEqual(2, instance.Count());
            Assert.AreEqual(0, result);

            mockApplicationConfiguration.Setup(item => item.Now).Returns(new DateTime(2016, 01, 11));
            result = instance.CalculateAverageRating();
            Assert.AreEqual(1, instance.Count());
            Assert.AreEqual(-10, result);
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new Tracker(null, "Type", logger, mockApplicationConfiguration.Object));
            Assert.Throws<ArgumentNullException>(() => new Tracker("Test", "Type", logger, null));
            Assert.Throws<ArgumentNullException>(() => new Tracker("Test", "Type", null, mockApplicationConfiguration.Object));
            Assert.Throws<ArgumentNullException>(() => new Tracker("Test", null, logger, mockApplicationConfiguration.Object));
            Assert.AreEqual(0, instance.Count());
        }

        private Tracker CreateTracker()
        {
            return new Tracker("Test", "Type", logger, mockApplicationConfiguration.Object);
        }
    }
}
