using InsightCore.Configuration;
using InsightCore.Util;

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
        /// Exclude search param
        /// </summary>
        public string Exclude => "NOT";

        /// <summary>
        /// Wildcard search param
        /// </summary>
        public string Wildcard => "*";

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
                // fetch all in-memory data where the log file is under IIS
                var dataSet = GetResults.Where(x => x.Index == search.Index).ToList();

                List<string> additionalFields = new List<string>();

                // iterate log sources (files) that are in memory
                foreach (var logFile in dataSet)
                {
                    // append unique fields found in log file
                    foreach (var field in logFile.Fields)
                    {
                        if (!result.UniqueFields.Any(x => x.Equals(field, StringComparison.InvariantCultureIgnoreCase)))
                            result.UniqueFields.Add(field);
                    }

                    // log lines that matched param logic
                    List<LogLine> logsFound = new List<LogLine>();

                    // iterate through each log line within the current file
                    foreach (var logLine in logFile.LogLines)
                    {
                        // search for text
                        bool hasAllParams = false;

                        // prep complex search w/ additional field values parsed from query string params
                        if (search.ComplexMode)
                        {
                            // append additional field/value pairs from query string
                            string[] queryStringParamDefinitions = logLine.QueryStringParams.Split('&');

                            // process query string definitions if not empty
                            if (queryStringParamDefinitions.Length > 0)
                            {
                                // iterate through all query string params 
                                // i.e somedata=test&username=test
                                foreach (string qsParam in queryStringParamDefinitions)
                                {
                                    // if the param is empty, ignore
                                    if (string.IsNullOrEmpty(qsParam))
                                        continue;

                                    // split for field/value pair
                                    string[] qsDefinitions = qsParam.Split("=");

                                    if (qsDefinitions.Length > 1)
                                    {
                                        // ignore if field or value is empty
                                        if (string.IsNullOrEmpty(qsDefinitions[0]) || string.IsNullOrEmpty(qsDefinitions[1]))
                                            continue;

                                        // add field/value pair to log items for processing later 
                                        logLine.LogItems.Add(new LogItem(qsDefinitions[0], qsDefinitions[1]));

                                        // add to complex search items to append to unique fields after
                                        if (!logFile.Fields.Contains(qsDefinitions[0]) && !additionalFields.Contains(qsDefinitions[0]))
                                            additionalFields.Add(qsDefinitions[0]);
                                    }
                                }
                            }

                            foreach (var newField in additionalFields)
                            {
                                if (!result.UniqueFields.Any(x => x.Contains(newField)))
                                    result.UniqueFields.Add(newField);
                            }
                        }

                        // iterate all search params in query
                        // ensure each param is met to return a search result
                        // check if param is conditional too
                        bool isConditionalParam = false;
                        foreach (var param in searchParams)
                        {
                            if (string.IsNullOrEmpty(param) || param.StartsWith("index="))
                                continue;

                            // conditional statement (i.e NOT)
                            if (param.Equals(this.Exclude))
                            {
                                isConditionalParam = true; // next statement will exclude keyword or definition
                                continue;
                            }

                            var strictDefinitions = param.Split('=');
                            bool isStrictDefinition = strictDefinitions.Length > 1;

                            if (isStrictDefinition)
                            {
                                bool containsValue;

                                string searchPattern = strictDefinitions[1].Replace("*", ""); // Remove wildcard for base search
                                bool startsWithWildcard = strictDefinitions[1].StartsWith(this.Wildcard);
                                bool endsWithWildcard = strictDefinitions[1].EndsWith(this.Wildcard);

                                containsValue = (isConditionalParam)
                                    ? !logLine.LogItems.Any(x =>
                                        x.Field.Equals(strictDefinitions[0], StringComparison.OrdinalIgnoreCase) &&
                                        Utility.MatchWildcard(x.Value, searchPattern, startsWithWildcard, endsWithWildcard))
                                    : logLine.LogItems.Any(x =>
                                        x.Field.Equals(strictDefinitions[0], StringComparison.OrdinalIgnoreCase) &&
                                        Utility.MatchWildcard(x.Value, searchPattern, startsWithWildcard, endsWithWildcard));



                                hasAllParams = containsValue;

                                if (!containsValue)
                                    break;
                            }
                            else
                            {

                                string cleanParam = param;

                                // loose searches already are wildcarded (log-> contains -> word)
                                if (param.EndsWith(this.Wildcard))
                                {
                                    cleanParam = param.Replace("*", "");
                                }

                                bool containsWord = (isConditionalParam) ? !logLine.Raw.Contains(cleanParam, StringComparison.OrdinalIgnoreCase)
                                    : logLine.Raw.Contains(cleanParam, StringComparison.OrdinalIgnoreCase);
                                
                                hasAllParams = containsWord;

                                if (!containsWord)
                                    break;
                            }

                            // set conditional check
                            isConditionalParam = false;

                            // end param iteration 
                        }

                        if (hasAllParams)
                        {
                            logsFound.Add(logLine);
                        }

                        result.TotalLinesSearched++;
                        // end log line iteration
                    }

                    // add found log lines to search result
                    if (logsFound.Count > 0)
                    {
                        result.LogLines.AddRange(logsFound);
                    }


                }
            }

            // post processing (key words)
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

            List<string> files = Directory.GetFiles(this.LogDirectory)
                .Where(file => Utility.LogExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .ToList();

            if (files.Count == 0)
                return;

            foreach (var file in files)
                this._logFiles.Add(new FileInfo(file));

            // reorder to process files so the latest content is displayed on the first page
            this._logFiles = this._logFiles.OrderByDescending(x => x.LastWriteTimeUtc).ToList();

            this.Parse();
        }

        /// <summary>
        /// Parses raw W3C log files and converts to LogFile objects
        /// </summary>
        private void Parse()
        {
            // (log) file iteration 
            foreach (FileInfo fileInfo in _logFiles)
            {
                // continue if file is empty or doesn't exist
                if (!fileInfo.Exists || fileInfo.Length == 0)
                    continue;

                // create log file to be added to our parsed list of logs
                LogFile logFile = new LogFile(fileInfo);
                logFile.Index = SearchIndex.IIS; // temp iis by default
                logFile.Hash = Utility.ComputeFileHash(fileInfo.FullName);

                // read file
                using (StreamReader reader = new StreamReader(fileInfo.FullName))
                {
                    string? line;
                    while ((line = reader?.ReadLine()) != null)
                    {
                        // ignore comments unless field def
                        if (line.StartsWith(CommentChar))
                        {
                            // ignore if its not the field start comment or if we already found fields in this file
                            if (!line.StartsWith(FieldStart) || logFile.Fields.Count > 0)
                                continue;

                            // we found the fields comment, lets set it as the format for this file 
                            logFile.Fields = line.Replace(FieldStart, string.Empty).Split(Delimiter).ToList();
                            continue;
                        }

                        // start of new log line
                        LogLine logLine = new LogLine();
                        logLine.Raw = line; // assign with raw text

                        // we *should* have fields here if it was defined in the file header
                        if (logFile.Fields.Count > 0)
                        {
                            // storing timestamp to be parsed later
                            string tempTimestamp = string.Empty;

                            // iterate through all values identified in the log line
                            string[] fieldValues = line.Split(Delimiter);
                            for (int i = 0; i < fieldValues.Length; i++)
                            {
                                string? fieldName = i > logFile.Fields.Count ? string.Empty : logFile.Fields[i];
                                string fieldValue = fieldValues[i];

                                // create log item to store field name and value pair within the log line
                                LogItem logItem = new LogItem(fieldName, fieldValue);

                                // extract timestamp to parse later
                                if (logFile.Fields[i].Equals("date"))
                                {
                                    tempTimestamp += fieldValue + " ";
                                }
                                else if (logFile.Fields[i].Equals("time"))
                                {
                                    tempTimestamp += fieldValue;
                                }
                                // add log item (key value pair) to log line
                                logLine.LogItems.Add(logItem);
                            }

                            // Todo: Consider using DateTime.TryParse for safety, and adjust for user timezone if needed
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

                        // add log lines to log file
                        logFile.LogLines.Add(logLine);
                    }
                }

                // add to parsed log list
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
