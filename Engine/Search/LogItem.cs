namespace InsightCore.Engine.Search
{
    public class LogItem
    {
        /// <summary>
        /// A LogItem is a key (field) / value pair.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public LogItem(string field = "", string value = "")
        {
            Field = field;
            Value = value;
        }

        public LogItem() { }

        /// <summary>
        /// Field within a log.
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Value of the field.
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
}
