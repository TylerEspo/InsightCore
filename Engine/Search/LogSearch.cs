namespace InsightCore.Engine.Search
{
    public class LogSearch
    {
        /// <summary>
        /// Contains meta data of the raw search. 
        /// Such as the raw input, timeframe and index of the search that will be passed to the engine.
        /// </summary>
        /// <param name="input"></param>
        public LogSearch(string? input, bool complexMode = false)
        {
            Query = new Query(input);
            this.ComplexMode = complexMode;
        }

        public LogSearch()
        {
            Query = new Query(string.Empty);
        }

        /// <summary>
        /// Constructed query from raw input.
        /// </summary>
        public Query Query { get; set; }

        /// <summary>
        /// On date search option
        /// </summary>
        public DateTime? OnDate { get; set; }

        /// <summary>
        /// Start date search option
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        /// End date search option
        /// </summary>
        public DateTime? End { get; set; }

        public bool ComplexMode { get; set; } = false;

        /// <summary>
        /// Selected index for search 
        /// </summary>
        public SearchIndex Index
        {
            get
            {
                KeyValuePair<string, string> indexDefinition = Query.Definitions.FirstOrDefault(x => x.Key.ToLower().Equals("index"));
                if (Enum.TryParse(indexDefinition.Value, true, out SearchIndex retVal))
                    return retVal;
                else
                    return SearchIndex.IIS;
            }
        }
    }
}
