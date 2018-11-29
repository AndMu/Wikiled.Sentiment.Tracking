using System;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public interface ITracker : IDisposable
    {
        string Name { get; }

        string Type { get; }

        IObservable<RatingRecord> Ratings { get; }

        void TrimOlder(TimeSpan maxTrack);

        void AddRating(RatingRecord rating);

        bool IsTracked(string id);

        RatingRecord[] GetRatings(int lastHours = 24);

        double? CalculateAverageRating(int lastHours = 24);

        int Count(bool withRating = true, int lastHours = 24);
    }
}