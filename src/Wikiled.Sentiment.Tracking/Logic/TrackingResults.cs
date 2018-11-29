using System;
using System.Collections.Generic;
using System.Text;

namespace Wikiled.Sentiment.Tracking.Logic
{
    public class TrackingResults
    {
        public TrackingResults()
        {
            Sentiment = new Dictionary<string, TrackingResult>(StringComparer.OrdinalIgnoreCase);
        }

        public string Keyword { get; set; }

        public string Type { get; set; }

        public int Total { get; set; }

        public Dictionary<string, TrackingResult> Sentiment { get; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Tracking Result: [{Keyword}/{Type}]({Total})");
            if (Sentiment != null)
            {
                foreach (var result in Sentiment)
                {
                    builder.Append($" [{result.Key}]:{result.Value}");
                }
            }

            return builder.ToString();
        }
    }
}
