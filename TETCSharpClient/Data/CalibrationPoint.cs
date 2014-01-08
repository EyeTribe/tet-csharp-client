using Newtonsoft.Json;

namespace TETCSharpClient.Data
{
    public class CalibrationPoint
    {
        /// <summary>
        /// State defines that no data is available for calibration point
        /// </summary>
        public static readonly int STATE_NO_DATA = 0;

        /// <summary>
        /// State defines that calibration point should be resampled
        /// </summary>
        public static readonly int STATE_RESAMPLE = 1;

        /// <summary>
        /// State defines that calibration point was succesfully sampled
        /// </summary>
        public static readonly int STATE_OK = 2;

        /// <summary>
        /// State of calib point
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_STATE)]
        public int State { get; set; }

        /// <summary>
        /// Coordinate in pixels
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_COORDINATES)]
        public Point2D Coordinates { get; set; }

        /// <summary>
        /// Mean estimated coordinates
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_MEAN_ESTIMATED_COORDINATES)]
        public Point2D MeanEstimatedCoords { get; set; }

        /// <summary>
        /// Accuracy
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_ACCURACIES_DEGREES)]
        public GazeAccuracy Accuracy { get; set; }

        /// <summary>
        /// Mean Error
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_MEAN_ERRORS_PIXELS)]
        public GazeMeanError MeanError { get; set; }

        /// <summary>
        /// Standard Deviation
        /// </summary>
        [JsonProperty(PropertyName = Protocol.CALIBRESULT_KEY_STANDARD_DEVIATIONS_PIXELS)]
        public GazeStandardDeviation StandardDeviation { get; set; }

        public CalibrationPoint()
        {
            Coordinates = new Point2D();
            MeanEstimatedCoords = new Point2D();
            Accuracy = new GazeAccuracy();
            MeanError = new GazeMeanError();
            StandardDeviation = new GazeStandardDeviation();
        }
    }
}