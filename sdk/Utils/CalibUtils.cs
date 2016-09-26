using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EyeTribe.ClientSdk.Data;

namespace EyeTribe.ClientSdk.Utils
{
    public class CalibUtils
    {
        public enum CalibQuality
        {
            NONE = 0,
            PERFECT = 4,
            GOOD = 3,
            MODERATE = 2,
            POOR = 1
        }

        protected CalibUtils() 
        {
            //ensure non-instantiability, allowing inheritance
        }

        /**
         * Return CalibQuality based on CalibrationResult object.
         *
         * @param result
         * @return CalibQuality according to CalibrationResult paramenter
         */
        public static CalibQuality GetCalibQuality(CalibrationResult result)
        {
            if (result != null && !float.IsNaN((float)result.AverageErrorDegree))
            {
                if (result.AverageErrorDegree < 0.5f)
                {
                    return CalibQuality.PERFECT;
                }
                else if (result.AverageErrorDegree < 0.7f)
                {
                    return CalibQuality.GOOD;
                }
                else if (result.AverageErrorDegree < 1f)
                {
                    return CalibQuality.MODERATE;
                }
                else if (result.AverageErrorDegree < 1.5f)
                {
                    return CalibQuality.POOR;
                }
            }

            return CalibQuality.NONE;
        }

        /**
         * Return an int representation of calibration quality based on CalibrationResult object.
         * <p/>
         * Useful for setting 'star rating' UI components explaining the current calibration quality.
         *
         * @param result
         * @return int value from 0-4, higher number equals better calibration
         */
        public static int GetCalibRating(CalibrationResult result)
        {
            CalibQuality cq = GetCalibQuality(result);

            if (cq.Equals(CalibQuality.PERFECT))
            {
                return 4;
            }
            else if (cq.Equals(CalibQuality.GOOD))
            {
                return 3;
            }
            else if (cq.Equals(CalibQuality.MODERATE))
            {
                return 2;
            }
            else if (cq.Equals(CalibQuality.POOR))
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Helper method that generates geometric calibration points based on desired rect area.
        /// <para/>
        /// This is useful when implementing a custom calibration UI.
        /// </summary>
        /// <param name="rows">the number of rows in calibration point grid</param>
        /// <param name="columns">the number of columns in calibration point grid</param>
        /// <param name="width">width of the rect area to spread the calibration points in</param>
        /// <param name="height">height of the rect area to spread the calibration points in</param>
        /// <returns>list of calibration points</returns> 
        public static List<Point2D> InitCalibrationPoints(int rows, int columns, int width, int height)
        {
            return InitCalibrationPoints(rows, columns, width, height, 0, 0, true);
        }

        /// <summary>
        /// Helper method that generates geometric calibration points based on desired rect area.
        /// <para/>
        /// This is useful when implementing a custom calibration UI.
        /// </summary>
        /// <param name="rows">the number of rows in calibration point grid</param>
        /// <param name="columns">the number of columns in calibration point grid</param>
        /// <param name="width">width of the rect area to spread the calibration points in</param>
        /// <param name="height">height of the rect area to spread the calibration points in</param>
        /// <param name="shuffle">should the returned calibration point be shuffled</param>
        /// <returns>list of calibration points</returns> 
        public static List<Point2D> InitCalibrationPoints(int rows, int columns, int width, int height, Boolean shuffle)
        {
            return InitCalibrationPoints(rows, columns, width, height, 0, 0, shuffle);
        }

        /// <summary>
        /// Helper method that generates geometric calibration points based on desired rect area.
        /// <para/>
        /// This is useful when implementing a custom calibration UI.
        /// </summary>
        /// <param name="rows">the number of rows in calibration point grid</param>
        /// <param name="columns">the number of columns in calibration point grid</param>
        /// <param name="width">width of the rect area to spread the calibration points in</param>
        /// <param name="height">height of the rect area to spread the calibration points in</param>
        /// <returns>list of calibration points</returns> 
        public static List<Point2D> InitCalibrationPoints(int rows, int columns, double width, double height)
        {
            return InitCalibrationPoints(rows, columns, (int)Math.Round(width), (int)Math.Round(height), 0, 0, true);
        }

        /// <summary>
        /// Helper method that generates geometric calibration points based on desired rect area.
        /// <para/>
        /// This is useful when implementing a custom calibration UI.
        /// </summary>
        /// <param name="rows">the number of rows in calibration point grid</param>
        /// <param name="columns">the number of columns in calibration point grid</param>
        /// <param name="width">width of the rect area to spread the calibration points in</param>
        /// <param name="height">height of the rect area to spread the calibration points in</param>
        /// <param name="shuffle">should the returned calibration point be shuffled</param>
        /// <returns>list of calibration points</returns> 
        public static List<Point2D> InitCalibrationPoints(int rows, int columns, double width, double height, Boolean shuffle)
        {
            return InitCalibrationPoints(rows, columns, (int)Math.Round(width), (int)Math.Round(height), 0, 0, shuffle);
        }

        /// <summary>
        /// Helper method that generates geometric calibration points based on desired rect area.
        /// <para/>
        /// This is useful when implementing a custom calibration UI.
        /// </summary>
        /// <param name="rows">the number of rows in calibration point grid</param>
        /// <param name="columns">the number of columns in calibration point grid</param>
        /// <param name="width">width of the rect area to spread the calibration points in</param>
        /// <param name="height">height of the rect area to spread the calibration points in</param>
        /// <param name="paddingHors">optional horizontal padding in rect area</param>
        /// <param name="paddingVert">optional vertical padding in rect area</param>
        /// <returns>list of calibration points</returns> 
        public static List<Point2D> InitCalibrationPoints(int rows, int columns, double width, double height, double paddingHors, double paddingVert)
        {
            return InitCalibrationPoints(rows, columns, (int)Math.Round(width), (int)Math.Round(height), (int)Math.Round(paddingHors), (int)Math.Round(paddingVert), true);
        }

        /// <summary>
        /// Helper method that generates geometric calibration points based on desired rect area.
        /// <para/>
        /// This is useful when implementing a custom calibration UI.
        /// </summary>
        /// <param name="rows">the number of rows in calibration point grid</param>
        /// <param name="columns">the number of columns in calibration point grid</param>
        /// <param name="width">width of the rect area to spread the calibration points in</param>
        /// <param name="height">height of the rect area to spread the calibration points in</param>
        /// <param name="paddingHors">optional horizontal padding in rect area</param>
        /// <param name="paddingVert">optional vertical padding in rect area</param>
        /// <param name="shuffle">should the returned calibration point be shuffled</param>
        /// <returns>list of calibration points</returns> 
        public static List<Point2D> InitCalibrationPoints(int rows, int columns, double width, double height, double paddingHors, double paddingVert, Boolean shuffle)
        {
            return InitCalibrationPoints(rows, columns, (int)Math.Round(width), (int)Math.Round(height), (int)Math.Round(paddingHors), (int)Math.Round(paddingVert), shuffle);
        }

        /// <summary>
        /// Helper method that generates geometric calibration points based on desired rect area.
        /// <para/>
        /// This is useful when implementing a custom calibration UI.
        /// </summary>
        /// <param name="rows">the number of rows in calibration point grid</param>
        /// <param name="columns">the number of columns in calibration point grid</param>
        /// <param name="width">width of the rect area to spread the calibration points in</param>
        /// <param name="height">height of the rect area to spread the calibration points in</param>
        /// <param name="paddingHors">optional horizontal padding in rect area</param>
        /// <param name="paddingVert">optional vertical padding in rect area</param>
        /// <returns>list of calibration points</returns> 
        public static List<Point2D> InitCalibrationPoints(int rows, int columns, int width, int height, int paddingHors, int paddingVert)
        {
            return InitCalibrationPoints(rows, columns, width, height, paddingHors, paddingVert, true);
        }

        /// <summary>
        /// Helper method that generates geometric calibration points based on desired rect area.
        /// <para/>
        /// This is useful when implementing a custom calibration UI.
        /// </summary>
        /// <param name="rows">the number of rows in calibration point grid</param>
        /// <param name="columns">the number of columns in calibration point grid</param>
        /// <param name="width">width of the rect area to spread the calibration points in</param>
        /// <param name="height">height of the rect area to spread the calibration points in</param>
        /// <param name="paddingHors">optional horizontal padding in rect area</param>
        /// <param name="paddingVert">optional vertical padding in rect area</param>
        /// <param name="shuffle">should the returned calibration point be shuffled</param>
        /// <returns>list of calibration points</returns> 
        public static List<Point2D> InitCalibrationPoints(int rows, int columns, int width, int height, int paddingHors, int paddingVert, Boolean shuffle)
        {
            List<Point2D> anchors = new List<Point2D>();

            float x = 0,y = 0;
            float horsSlice = (float)(width - paddingHors - paddingHors) / (columns - 1);
            float vertSlice = (float)(height - paddingVert - paddingVert) / (rows - 1);
            for(int i = 0; i < columns; i++)
            {
                x = horsSlice * i;

                for(int j = 0; j < rows; j++)
                {
                    y = vertSlice * j;

                    Point2D p = new Point2D(paddingHors + x, paddingVert + y);

                    anchors.Add(p);
                }
            }

            //randomly shuffle points
            if(shuffle)
                Shuffle<Point2D>(anchors);

            return anchors;
        }

        protected static void Shuffle<T>(IList<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
