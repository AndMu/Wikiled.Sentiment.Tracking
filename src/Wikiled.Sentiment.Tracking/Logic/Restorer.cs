using System;
using System.IO;
using CsvHelper;
using Microsoft.Extensions.Logging;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public class Restorer : IRestorer
    {
        private readonly ILogger<Restorer> logger;

        private readonly ITrackingManager manager;

        public Restorer(ILogger<Restorer> logger, ITrackingManager manager)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public void Load(string file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            logger.LogInformation("Load: {0}", file);
            var mask = Path.GetFileNameWithoutExtension(file);
            var directory = Path.GetDirectoryName(file);
            if (!Directory.Exists(directory))
            {
                logger.LogWarning("Directory does not exist: {0}", file);
                return;
            }

            var files = Directory.GetFiles(directory, $"{mask}.*");
            DateTime cutoff = DateTime.UtcNow.AddDays(-10);
            foreach (var currentFile in files)
            {
                var info = new FileInfo(currentFile);
                if (info.CreationTime < cutoff ||
                    info.Length <= 0)
                {
                    continue;
                }

                try
                {
                    LoadSingleFile(currentFile);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to load file " + file);
                }
            }
        }

        private void LoadSingleFile(string file)
        {
            logger.LogInformation("Loading {0}", file);
            using (var streamRead = new StreamReader(file))
            using (var csvData = new CsvReader(streamRead))
            {
                csvData.Read();
                csvData.ReadHeader();
                while (csvData.Read())
                {
                    var date = csvData.GetField<DateTime>("Date");
                    var tag = csvData.GetField<string>("Tag");
                    var type = csvData.GetField<string>("Type");
                    var id = csvData.GetField<string>("Id");
                    var rating = csvData.GetField<double?>("Rating");
                    var tracker = manager.Resolve(tag, type);
                    tracker.AddRating(new RatingRecord(id, date, rating));
                }
            }
        }
    }
}
