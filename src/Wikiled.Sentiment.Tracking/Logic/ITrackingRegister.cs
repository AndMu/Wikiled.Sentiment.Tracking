using System;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public interface ITrackingRegister : IDisposable
    {
        void Register(ITracker tracker);
    }
}