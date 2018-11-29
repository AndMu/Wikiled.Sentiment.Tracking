using System;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public interface IRatingStream
    {
        IObservable<(ITracker Tracker, RatingRecord Rating)> Stream { get; }
    }
}
