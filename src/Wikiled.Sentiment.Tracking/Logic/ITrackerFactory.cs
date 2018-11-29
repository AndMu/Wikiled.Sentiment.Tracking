namespace Wikiled.Sentiment.Tracking.Logic
{
    public interface ITrackerFactory
    {
        ITracker Construct(string name, string type);
    }
}