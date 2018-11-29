using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Utilities.Config;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public class Tracker : ITracker
    {
        private readonly ILogger<Tracker> logger;

        private readonly IApplicationConfiguration config;

        private readonly List<RatingRecord> ratings = new List<RatingRecord>();

        private readonly Dictionary<string, RatingRecord> idTable = new Dictionary<string, RatingRecord>();

        private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();

        private readonly Subject<RatingRecord> stream = new Subject<RatingRecord>();

        public Tracker(string name, string type, ILogger<Tracker> logger, IApplicationConfiguration config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string Name { get; }

        public string Type { get; }

        public IObservable<RatingRecord> Ratings => stream;

        public void TrimOlder(TimeSpan maxTrack)
        {
            DateTime cutOff = config.Now.Subtract(maxTrack);
            @lock.EnterWriteLock();
            try
            {
                foreach (RatingRecord item in ratings.Where(item => item.Date < cutOff).ToArray())
                {
                    ratings.Remove(item);
                    idTable.Remove(item.Id);
                }
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }

        public void AddRating(RatingRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            stream.OnNext(record);
            @lock.EnterWriteLock();
            try
            {
                ratings.Add(record);
                if (idTable.ContainsKey(record.Id))
                {
                    logger.LogWarning("<{0}> key already present. Replacing", record.Id);
                    ratings.Remove(idTable[record.Id]);
                }

                idTable[record.Id] = record;
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }

        public bool IsTracked(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            @lock.EnterReadLock();
            try
            {
                return idTable.ContainsKey(id);
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }

        public RatingRecord[] GetRatings(int lastHours = 24)
        {
            @lock.EnterReadLock();
            try
            {
                return GetValues(true, lastHours).Where(item => item?.Rating != null).ToArray();
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }

        public double? CalculateAverageRating(int lastHours = 24)
        {
            @lock.EnterReadLock();
            try
            {
                double[] sentiment = GetValues(true, lastHours).Where(item => item?.Rating != null).Select(item => item.Rating.Value).ToArray();
                if (sentiment.Length == 0)
                {
                    return null;
                }

                return sentiment.Average();
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }

        public int Count(bool withRating = true, int lastHours = 24)
        {
            @lock.EnterReadLock();
            try
            {
                return GetValues(withRating, lastHours).Count();
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            stream.OnCompleted();
            @lock?.Dispose();
            stream?.Dispose();
        }

        private IEnumerable<RatingRecord> GetValues(bool withSentiment, int lastHours = 24)
        {
            DateTime time = config.Now;
            time = time.AddHours(-lastHours);
            return ratings.Where(item => (!withSentiment || item.Rating.HasValue) && item.Date > time);
        }
    }
}
