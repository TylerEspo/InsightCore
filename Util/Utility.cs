using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;

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


        /// <summary>
        /// Compute hash for file monitoring
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ComputeFileHash(string filePath)
        {
            using (var sha1 = SHA1.Create()) // Use SHA256.Create() for a stronger hash
            using (var stream = new BufferedStream(File.OpenRead(filePath), 1024 * 32)) // 32KB buffer
            {
                byte[] hash = sha1.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }


        public static bool MatchWildcard(string fieldValue, string pattern, bool startsWith, bool endsWith)
        {
            if (startsWith && endsWith)
                return fieldValue.Contains(pattern, StringComparison.OrdinalIgnoreCase);
            if (startsWith)
                return fieldValue.EndsWith(pattern, StringComparison.OrdinalIgnoreCase);
            if (endsWith)
                return fieldValue.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);

            return fieldValue.Equals(pattern, StringComparison.OrdinalIgnoreCase);
        }
    }
}
