using System;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public class RatingRecord
    {
        public RatingRecord(string id, DateTime date, double? rating)
        {
            Date = date;
            Rating = rating;
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public string Id { get; }

        public DateTime Date { get; }

        public double? Rating { get; }
    }
}
