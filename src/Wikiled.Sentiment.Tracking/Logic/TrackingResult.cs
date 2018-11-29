namespace Wikiled.Sentiment.Tracking.Logic
{
    public class TrackingResult
    {
        public double? Average { get; set; }

        public int TotalMessages { get; set; }

        public override string ToString()
        {
            return $" Average: {Average}({TotalMessages})";
        }
    }
}
