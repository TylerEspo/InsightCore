using System.Text.RegularExpressions;

namespace InsightCore.Engine.Search
{
    public class Query
    {
        /// <summary>
        /// Query constructed from raw input containing variations of parameters used for searching.
        /// </summary>
        /// <param name="input"></param>
        public Query(string input)
        {
            Input = input;
        }

        /// <summary>
        /// Raw search query input.
        /// </summary>
        public string Input { get; set; } = string.Empty;

        /// <summary>
        /// List of string params split by spaces.
        /// </summary>
        public string[] Parameters
        {
            get
            {
                return Input.Split(' ');
            }
        }

        /// <summary>
        /// The Search Query is the input up until the termination character '|' where keyword processing begins.
        /// </summary>
        public string SearchQuery
        {
            get
            {
                // index=iis
                // Key=Value
                string[] queryParams = Parameters;

                // index=iis | take 30
                // 10
                int endOfSearch = Input.IndexOf('|');

                // 10, 19
                string searchQuery = endOfSearch == -1 ? Input :
                    Input.Remove(endOfSearch);

                return searchQuery;
            }
        }

        /// <summary>
        /// The Keyword Query is the string after the termination character '|'. These keywords are predefined and allow the user to alter the final result.
        /// </summary>
        public string KeywordQuery
        {
            get
            {

                // index=iis
                // Key=Value
                string[] queryParams = Parameters;

                // index=iis | take 30
                // 10
                int startOfKeywords = Input.IndexOf('|');

                // 10, 19
                string keywordQuery = Input.Remove(0, startOfKeywords);

                return keywordQuery;
            }
        }

        /// <summary>
        /// List of strictly defined key/value pairs from search via format: {key}={value}
        /// </summary>
        public Dictionary<string, string> Definitions
        {
            get
            {
                Dictionary<string, string> retVal = new Dictionary<string, string>();

                if (string.IsNullOrEmpty(Input))
                    return retVal;

                // index=iis
                // Key=Value
                string[] queryParams = Parameters;

                foreach (var param in queryParams)
                {
                    var keyValueArray = param.Split('=');
                    // index=iis
                    if (!(param.Split("=").Length > 1))
                        continue;

                    // index iis
                    retVal.Add(keyValueArray[0], keyValueArray[1]);
                }

                return retVal;
            }
        }

        /// <summary>
        /// Dictionary contaning post-processing keywords parsed from search input.
        /// </summary>
        public Dictionary<string, string> KeyWords
        {
            get
            {
                Dictionary<string, string> retVal = new Dictionary<string, string>();

                if (string.IsNullOrEmpty(Input) || !Input.Contains("|"))
                    return retVal;

                string[] keywords = KeywordQuery.Split('|');

                foreach (var keyword in keywords)
                {
                    string trimmedWord = keyword.TrimStart().TrimEnd();

                    if (string.IsNullOrEmpty(trimmedWord) || trimmedWord.Equals("|"))
                        continue;

                    string[] keyValueArray = Regex.Split(trimmedWord, "(?<=')\\s(?=\\w)");
                    var nArr = trimmedWord.Split(" ");
                    retVal.Add(nArr[0], nArr[1].Split(' ')[0]);
                }

                return retVal;

            }
        }

    }
}
