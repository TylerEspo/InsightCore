namespace InsightCore.Engine.Search
{
    /// <summary>
    /// Search Result model returned to frontend after executing a log search.
    /// </summary>
    public class SearchResult
    {
        public List<LogLine> LogLines { get; set; } = new List<LogLine>();

        public List<string> UniqueFields { get; set; } = new List<string>();

        public List<LogLine> Anomalies { get; set; } = new List<LogLine>();

        public long ProcessingTime { get; set; }

        public int TotalLinesSearched { get; set; }
    }
}
