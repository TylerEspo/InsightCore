using System.Diagnostics;
using System.Reflection;
using InsightCore.Engine.Search;
using InsightCore.Models;
using Microsoft.AspNetCore.Mvc;
using InsightCore.Util;
using InsightCore.Engine.ML;
using System.Text;
namespace InsightCore.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Log Engine Dependency
        /// </summary>
        private readonly LogEngineService _logEngine;

        /// <summary>
        /// ML Prediction Service
        /// </summary>
        private readonly PredictionEngineService _mlEngine;

        /// <summary>
        /// Primary controller for InsightCore
        /// </summary>
        /// <param name="logEngine"></param>
        public HomeController(LogEngineService logEngine, PredictionEngineService mlEngine)
        {
            this._logEngine = logEngine;
            this._mlEngine = mlEngine;

            try
            {
                if (_mlEngine.IsTrained == false)
                {
                    List<LogLine> trainingLogs = logEngine.GetResults
                    .SelectMany(file => file.LogLines)
                    .ToList();

                    _mlEngine.TrainModel(trainingLogs);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Primary View
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Log Search API
        /// </summary>
        /// <param name="query"></param>
        /// <returns>List of LogLines</returns>
        [HttpPost]
        public IActionResult Search([FromBody] SearchRequest request)
        {
            // create log search from raw query
            LogSearch search = new LogSearch(request.Input, request.ComplexMode);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            // fetch search results from log engine
            SearchResult result = _logEngine.Search(search);

            // if the engine is trained and we have enough data
            // detect Anomalies
            if (this._mlEngine.IsTrained)
            {
                foreach (var log in result.LogLines)
                {
                    var prediction = this._mlEngine.PredictAnomalyDetailed(log); 

                    if (prediction.Prediction)
                    {
                        log.HasAnomaly = true;
                        result.Anomalies.Add(log);
                    }
                }
            }

            timer.Stop();

            result.ProcessingTime = timer.ElapsedMilliseconds;

            return Json(result);
        }

        /// <summary>
        /// Error View
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
