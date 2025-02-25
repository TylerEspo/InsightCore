using System.Linq.Expressions;
using System.Reflection;

namespace InsightCore.Util
{
    public static class Utility
    {
        /// <summary>
        /// List of file extensions 
        /// </summary>
        public static List<string> LogExtensions = new List<string>() { ".txt", ".log", ".w3c", ".json" };

        /// <summary>
        /// Sets the first character of string to uppercase and the remaining to lowercase.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };


    }
}
