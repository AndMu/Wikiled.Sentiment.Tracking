using System;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public class RatingRecord
    {
        public string Id { get; set; }

        public DateTime Date { get; set; }

        public double? Rating { get; set; }
    }
}
