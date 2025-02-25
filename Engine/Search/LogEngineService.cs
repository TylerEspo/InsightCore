using InsightCore.Configuration;
using InsightCore.Util;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace InsightCore.Engine.Search
{
    public class LogEngineService
    {
        /// <summary>
        /// App Settings
        /// </summary>
        private readonly ApplicationSettings _settings;

        /// <summary>
        /// Log Files (W3C)
        /// </summary>
        private List<FileInfo> _logFiles = new List<FileInfo>();

        /// <summary>
        /// Log files that are parsed to list of LogFile objects
        /// </summary>
        private List<LogFile> _parsedLogs = new List<LogFile>();

        /// <summary>
        /// Log Engine used for searching logs from various sources
        /// </summary>
        /// <param name="settings"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LogEngineService(ApplicationSettings settings)
        {
            this._settings = settings;

            if (string.IsNullOrEmpty(this._settings.GetDirectory()))
                throw new ArgumentNullException("Log Directory not configured.");

            this.Initialize();
        }

        /// <summary>
        /// Returns list of parsed logs
        /// </summary>
        public List<LogFile> GetResults { get { return _parsedLogs; } }

        /// <summary>
        /// Comment Char to skip unless a field
        /// </summary>
        public char CommentChar => '#';

        /// <summary>
        /// Field Comment (IIS W3C Standards)
        /// </summary>
        public string FieldStart => "#Fields: ";

        /// <summary>
        /// Primary Log Delimiter
        /// </summary>
        public char Delimiter => ' ';

        /// <summary>
        /// Exclude search keyword
        /// </summary>
        public string Exclude => "NOT";

        /// <summary>
        /// Location of W3C Logs
        /// </summary>
        public string LogDirectory
        {
            get
            {
                return this._settings.GetDirectory() ?? string.Empty;
            }

        }

        /// <summary>
        /// Performs log search against in-memory data set
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public SearchResult Search(LogSearch search)
        {
            SearchResult result = new SearchResult();

            if (string.IsNullOrEmpty(search.Query.Input))
                return result;

            string[] searchParams = search.Query.SearchQuery.Split(' ');

            // temp until implemented
            if (search.Index != SearchIndex.IIS)
                return result;

            if (search.Index == SearchIndex.IIS)
            {
                // query logs
                var dataSet = GetResults.Where(x => x.Index == search.Index).ToList();

                // iterate log sources (files) that are in memory
                foreach (var logFile in dataSet)
                {
                    foreach (var field in logFile.Fields)
                    {
                        if (!result.UniqueFields.Any(x => x.Equals(field, StringComparison.InvariantCultureIgnoreCase)))
                            result.UniqueFields.Add(field);
                    }

                    List<LogLine> wordDetected = new List<LogLine>();

                    foreach (var logLine in logFile.LogLines)
                    {
                        // search for text
                        bool hasAllParams = false;
                        foreach (var param in searchParams)
                        {
                            if (string.IsNullOrEmpty(param))
                                continue;

                            bool isDefinition = param.Split('=').Length > 1;

                            if (isDefinition)
                            {
                                //todo: do
                                continue;
                                bool containsKeyword = logLine.Raw.Contains(param, StringComparison.OrdinalIgnoreCase);
                                hasAllParams = containsKeyword;
                            }
                            else
                            {
                                bool containsKeyword = logLine.Raw.Contains(param, StringComparison.OrdinalIgnoreCase);
                                hasAllParams = containsKeyword;

                                if (!containsKeyword)
                                    break;

                            }
                        }

                        if (hasAllParams)
                        {
                            wordDetected.Add(logLine);
                        }
                    }

                    if (wordDetected.Count > 0)
                    {
                        result.LogLines.AddRange(wordDetected);
                    }
                }
            }

            // - LogLines
            // - Unique Fields
            // - Fields, Values


            result.LogLines = this.ProcessKeywords(search, result.LogLines.AsQueryable()).ToList();
            return result;
        }

        /// <summary>
        /// Initializes Engine by fetching logs from configured source(s) and stores them in memory for fast use
        /// </summary>
        private void Initialize()
        {
            if (string.IsNullOrEmpty(this.LogDirectory))
                throw new ArgumentNullException("Log Directory not configured.");

            string[] files = Directory.GetFiles(this.LogDirectory)
                .Where(file => Utility.LogExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .ToArray();

            if (files.Length == 0)
                return;

            foreach (var file in files)
                this._logFiles.Add(new FileInfo(file));

            this.Parse();
        }

        /// <summary>
        /// Parses raw W3C log files and converts to LogFile objects
        /// </summary>
        private void Parse()
        {
            foreach (FileInfo fileInfo in _logFiles)
            {
                if (!fileInfo.Exists || fileInfo.Length == 0)
                    continue;

                LogFile logFile = new LogFile(fileInfo);
                logFile.Index = SearchIndex.IIS;
                using (StreamReader reader = new StreamReader(fileInfo.FullName))
                {
                    string? line;
                    while ((line = reader?.ReadLine()) != null)
                    {
                        // ignore comments unless fields
                        if (line.StartsWith(CommentChar))
                        {
                            if (!line.StartsWith(FieldStart) || logFile.Fields.Length > 0)
                                continue;

                            logFile.Fields = line.Replace(FieldStart, string.Empty).Split(Delimiter);
                            continue;
                        }

                        LogLine logLine = new LogLine();
                        logLine.Raw = line;

                        // key/v pairs
                        if (logFile.Fields.Length > 0)
                        {
                            string tempTimestamp = string.Empty;
                            string[] fieldValues = line.Split(Delimiter);
                            for (int i = 0; i < fieldValues.Length; i++)
                            {
                                LogItem logItem = new LogItem();
                                string? fieldName = i > logFile.Fields.Length ? string.Empty : logFile.Fields[i];
                                string fieldValue = fieldValues[i];
                                logItem.Field = fieldName;
                                logItem.Value = fieldValue;

                                if (logFile.Fields[i].Equals("date"))
                                {
                                    tempTimestamp += fieldValue + " ";
                                }
                                else if (logFile.Fields[i].Equals("time"))
                                {
                                    tempTimestamp += fieldValue;
                                }

                                logLine.LogItems.Add(logItem);
                            }

                            // Consider using DateTime.TryParse for safety, and adjust for user timezone if needed
                            if (DateTime.TryParse(tempTimestamp.Trim(), out DateTime parsedTime))
                            {
                                logLine.TimeStamp = parsedTime;
                            }
                            else
                            {
                                // Handle the parsing error appropriately (log error, set default, etc.)
                                logLine.TimeStamp = DateTime.MinValue;
                            }
                        }

                        logFile.LogLines.Add(logLine);
                    }
                }
                _parsedLogs.Add(logFile);
            }
        }

        /// <summary>
        /// Processes keywords (Take, Order by, etc.) post search results.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="results"></param>
        /// <returns>IQueryable LogLines</returns>
        private IQueryable<LogLine> ProcessKeywords(LogSearch search, IQueryable<LogLine> results)
        {
            // keyword post-processing
            foreach (var keyword in search.Query.KeyWords)
            {
                // skip if invalid keyword value
                if (keyword.Value == null)
                    continue;

                try
                {
                    // == keyword logic == 
                    // take <value> = Takes x results from search
                    // order <desc/asc> = Orders results desc/asc from TimeStamp
                    switch (keyword.Key.ToLower())
                    {
                        case "take":
                            results = results.Take(int.Parse(keyword.Value));
                            break;
                        case "order":
                            results = (keyword.Value.ToLower() == "desc") ? results = results.OrderByDescending(x => x.TimeStamp)
                                : results = results.OrderBy(x => x.TimeStamp);
                            break;
                        default: break;
                    }
                }
                catch (Exception e)
                {
                    // possible parsing exception, etc. skip. maybe display to frontend
                    continue;
                }
            }

            return results;
        }
    }
}
