using InsightCore.Configuration;
using InsightCore.Engine.Search;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.ML;
using Microsoft.ML.Data;
using Newtonsoft.Json;
using Tensorboard;

namespace InsightCore.Engine.ML
{
    public class PredictionEngineService
    {
        /// <summary>
        /// ML.NET Context
        /// </summary>
        private readonly MLContext _mlContext;


        /// <summary>
        /// Trained Model from Pipeline
        /// </summary>
        private ITransformer? _trainedModel;

        /// <summary>
        /// Predoction Engine
        /// </summary>
        private PredictionEngine<LogEntry, AnomalyPrediction>? _predictionEngine;

        /// <summary>
        /// Service that interfaces with ML.NET for anomaly prediction 
        /// </summary>
        public PredictionEngineService(MLContext context)
        {
            _mlContext = context;
        }

        /// <summary>
        /// Returns true if prediction engine has trained dataset.
        /// This is set to true within 'TrainModel' after building the pipline and creating prediction engine instance.
        /// </summary>
        public bool IsTrained { get; set; }

        /// <summary>
        /// Rank used for training which reduces or increases sensitivity towards anomaly detection.
        /// </summary>
        public int RankModifier { get; set; } = 5; 

        /// <summary>
        /// Predicts anomaly from trained data (i.e W3C logs)
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public AnomalyPrediction PredictAnomalyDetailed(LogLine log)
        {
            if (_predictionEngine == null)
                return new AnomalyPrediction();

            var entry = new LogEntry(log);
            return _predictionEngine.Predict(entry);
        }

        /// <summary>
        /// Trains an anomaly detection model using the provided log lines.
        /// </summary>
        public void TrainModel(List<LogLine> logLines)
        {
            if (logLines == null || logLines.Count < 2)
                throw new ArgumentException("At least two log entries are required for training.");

            // Convert LogLine objects into ML.NET input data (LogEntry)
            var logEntries = logLines.Select(log => new LogEntry(log)).ToList();
            IDataView dataView = _mlContext.Data.LoadFromEnumerable(logEntries);

            // Preview the data to check column names
            var preview = dataView.Preview();
            Console.WriteLine(preview);

            // Build a pipeline:
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("HttpResponseCodeKey", nameof(LogLine.HttpResponseCode))
                .Append(_mlContext.Transforms.Conversion.MapKeyToVector("HttpResponseCodeVector", "HttpResponseCodeKey")) // Convert key back to vector
                .Append(_mlContext.Transforms.Text.FeaturizeText("UriFeatures", nameof(LogLine.Uri)))
                .Append(_mlContext.Transforms.Concatenate("Features", "ResponseTime", "HttpResponseCodeVector", "UriFeatures"))
                .Append(_mlContext.AnomalyDetection.Trainers.RandomizedPca(
                    featureColumnName: "Features",
                    rank: this.RankModifier
                ));

            _trainedModel = pipeline.Fit(dataView);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<LogEntry, AnomalyPrediction>(_trainedModel);
            this.IsTrained = true;
        }

        /// <summary>
        /// Predicts whether a given log line is anomalous.
        /// </summary>
        public bool PredictAnomaly(LogLine log)
        {
            if (_predictionEngine == null)
                throw new InvalidOperationException("Model has not been trained.");

            return _predictionEngine.Predict(new LogEntry(log)).Prediction;
        }
    }

    public class LogEntry
    {
        /// <summary>
        /// The ML.NET input model for anomaly detection.
        /// </summary>
        public LogEntry() { }

        /// <summary>
        /// The ML.NET input model for anomaly detection.
        /// </summary>
        public LogEntry(LogLine log)
        {
            ResponseTime = log.ResponseTime;
            Uri = log.Uri;
            HttpResponseCode = log.HttpResponseCode;
            HasAnomaly = log.HasAnomaly;
        }

        /// <summary>
        /// Response time of service call
        /// </summary>
        public float ResponseTime { get; set; }

        /// <summary>
        /// Uri / Url of service call
        /// </summary>
        public string Uri { get; set; } = string.Empty;

        /// <summary>
        /// Http Response Code of service call
        /// </summary>
        public string HttpResponseCode { get; set; } = string.Empty;

        /// <summary>
        /// Anomaly label for prediction engine
        /// </summary>
        [ColumnName("Label")]
        public bool HasAnomaly { get; set; }

    }

    public class AnomalyPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Score { get; set; }
    }

}