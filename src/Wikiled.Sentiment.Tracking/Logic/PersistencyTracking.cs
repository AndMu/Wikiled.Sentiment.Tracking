using System;
using System.IO;
using System.Reactive.Disposables;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Extensions;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public class PersistencyTracking : IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();

        private readonly CsvWriter writer;

        private readonly StreamWriter streamWriter;

        private ILogger<PersistencyTracking> logger;

        public PersistencyTracking(ILogger<PersistencyTracking> logger, TrackingConfiguration configuration, IRatingStream stream, IRestorer restorer)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (restorer == null)
            {
                throw new ArgumentNullException(nameof(restorer));
            }

            if (string.IsNullOrEmpty(configuration?.Persistency))
            {
                throw new ArgumentNullException(nameof(configuration.Persistency));
            }

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("Creating persistency {0}", configuration.Persistency);
            configuration.Persistency.EnsureDirectoryExistence();
            restorer.Load(configuration.Persistency);
            var subscription = stream.Stream.Subscribe(item => Process(item.Item1, item.Item2));
            disposable.Add(subscription);
            configuration.Persistency.Backup();
            streamWriter = new StreamWriter(configuration.Persistency, false);
            disposable.Add(streamWriter);
            writer = new CsvWriter(streamWriter);
            disposable.Add(writer);
            writer.WriteField("Date");
            writer.WriteField("Tag");
            writer.WriteField("Type");
            writer.WriteField("Id");
            writer.WriteField("Rating");
            writer.NextRecord();
        }

        private void Process(ITracker tracker, RatingRecord record)
        {
            lock (writer)
            {
                writer.WriteField(record.Date);
                writer.WriteField(tracker.Name);
                writer.WriteField(tracker.Type);
                writer.WriteField(record.Id);
                writer.WriteField(record.Rating);
                writer.NextRecord();
                writer.Flush();
                streamWriter.Flush();
            }
        }

        public void Dispose()
        {
            disposable?.Dispose();
        }
    }
}
