namespace InsightCore.Engine.Search
{
    public class LogFile
    {
        /// <summary>
        /// Log File in W3C standards. Uses FileInfo for extensive file information.
        /// </summary>
        /// <param name="fileInfo"></param>
        public LogFile(FileInfo? fileInfo)
        {
            this.FileInfo = fileInfo;
        }

        /// <summary>
        /// File Information
        /// </summary>
        public FileInfo? FileInfo { get; set; }

        /// <summary>
        /// Array of log fields defined within the log 
        /// </summary>
        public List<string> Fields { get; set; } = new List<string>();

        /// <summary>
        /// List of log lines within the current log file.
        /// </summary>
        public List<LogLine> LogLines { get; set; } = new List<LogLine>();

        /// <summary>
        /// Index in which the log exists in.
        /// </summary>
        public SearchIndex Index { get; set; }

        /// <summary>
        /// Byte hash of file
        /// </summary>
        public string? Hash { get; set; }

    }

}
