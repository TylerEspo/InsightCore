using Microsoft.ML.Data;

namespace InsightCore.Engine.Search
{
    /// <summary>
    /// Log Lines parsed within a log file.
    /// </summary>
    public class LogLine
    {
        /// <summary>
        /// Raw content of the log line.
        /// </summary>
        public string Raw { get; set; } = string.Empty;

        /// <summary>
        /// Parsed list of Log Items found during parsing.
        /// </summary>
        public List<LogItem> LogItems { get; set; } = new List<LogItem>();

        /// <summary>
        /// TimeStamp of Log Line.
        /// </summary>
        public DateTime TimeStamp { get; set; } = DateTime.MinValue;

        public string HttpResponseCode => LogItems.FirstOrDefault(x => x.Field.Equals("sc-status", StringComparison.OrdinalIgnoreCase))?.Value ?? "Unknown";

        public string Uri => LogItems.FirstOrDefault(x => x.Field.Equals("cs-uri-stem", StringComparison.OrdinalIgnoreCase))?.Value ?? "Unknown";

        public float ResponseTime
        {
            get
            {
                var value = LogItems.FirstOrDefault(x => x.Field.Equals("time-taken", StringComparison.OrdinalIgnoreCase))?.Value;
                return float.TryParse(value, out float result) ? result : 0f;
            }
            set
            {
                this.ResponseTime = value;
            }
        }

        public float BytesSent
        {
            get
            {
                string? value = LogItems.FirstOrDefault(x => x.Field.Equals("sc-bytes", StringComparison.OrdinalIgnoreCase))?.Value;
                return float.TryParse(value, out float result) ? result : 0f;
            }
            set
            {
                this.BytesSent = value;
            }
        }

        public bool HasAnomaly { get; set; }

        public string QueryStringParams
        {
            get
            {
                string? value = LogItems.FirstOrDefault(x => x.Field.Equals("cs-uri-query", StringComparison.OrdinalIgnoreCase))?.Value;

                return value;
            }
            set
            {
                this.QueryStringParams = value;
            }
        }
    }
}
