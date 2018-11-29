﻿using System;
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

            var mask = Path.GetFileNameWithoutExtension(file);
            var directory = Path.GetFileName(file);
            var files = Directory.GetFiles(directory, $"{mask}.*");
            foreach (var currentFile in files)
            {
                LoadSingleFile(currentFile);
            }

            if (!File.Exists(file))
            {
                logger.LogInformation("File {0} not found", file);
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