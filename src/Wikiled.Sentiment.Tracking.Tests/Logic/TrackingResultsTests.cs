using NUnit.Framework;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.MachineLearning.Tests.Mathematics.Tracking
{
    [TestFixture]
    public class TrackingResultsTests
    {
        [Test]
        public void String()
        {
            var results = new TrackingResults();
            Assert.AreEqual("Tracking Result: [/](0)", results.ToString());
            results.Keyword = "Test";
            results.Type = "Test";
            results.Total = 2;
            results.Sentiment["2H"] = new TrackingResult
            {
                Average = 2,
                TotalMessages = 2
            };

            results.Sentiment["24H"] = new TrackingResult();
            Assert.AreEqual("Tracking Result: [Test/Test](2) [2H]: Average: 2(2) [24H]: Average: (0)", results.ToString());
        }
    }
}
