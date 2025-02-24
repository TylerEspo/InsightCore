using Microsoft.Extensions.Configuration;

namespace InsightCore.Configuration
{

    public class ApplicationSettings
    {
        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Application Settings via appsettings.json
        /// </summary>
        public ApplicationSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Directory contaning W3C logs
        /// </summary>
        /// <returns>Directory Path</returns>
        public string? GetDirectory()
        {
            return _configuration.GetValue<string>("LogEngine:LogDirectory");
        }
    }
}
