using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TETCSharpClient.Data;

namespace TETCSharpClient
{
    /// <summary>
    /// Utility methods common to gaze control routines.
    /// </summary>
    public class GazeUtils
    {
        /// <summary>
        /// Find average pupil center of two eyes.
        /// </summary>
        /// <param name="leftEye"></param>
        /// <param name="rightEye"></param>
        /// <returns>the average center point in normalized values</returns>
        public static Point2D getEyesCenterNormalized(Eye leftEye, Eye rightEye)
        {
            Point2D eyeCenter = new Point2D();

            if (null != leftEye && null != rightEye)
            {
                eyeCenter = new Point2D(
                        (leftEye.PupilCenterCoordinates.X + rightEye.PupilCenterCoordinates.X) / 2,
                        (leftEye.PupilCenterCoordinates.Y + rightEye.PupilCenterCoordinates.Y) / 2
                        );
            }
            else if (null != leftEye)
            {
                eyeCenter = leftEye.PupilCenterCoordinates;
            }
            else if (null != rightEye)
            {
                eyeCenter = rightEye.PupilCenterCoordinates;
            }

            return eyeCenter;
        }

        /// <summary>
        /// Find average pupil center of two eyes.
        /// </summary>
        /// <param name="gazeData">gaze data frame to base calculation upon</param>
        /// <returns>the average center point in normalized values</returns>
        public static Point2D getEyesCenterNormalized(GazeData gazeData)
        {
            if (null != gazeData)
                return getEyesCenterNormalized(gazeData.LeftEye, gazeData.RightEye);
            else
                return null;
        }

        /// <summary>
        /// Find average pupil center of two eyes.
        /// </summary>
        /// <param name="leftEye"></param>
        /// <param name="rightEye"></param>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        /// <returns>the average center point in pixels</returns>
        public static Point2D getEyesCenterPixels(Eye leftEye, Eye rightEye, int screenWidth, int screenHeight)
        {
            Point2D center = getEyesCenterNormalized(leftEye, rightEye);

            return getRelativeToScreenSpace(center, screenWidth, screenHeight);
        }

        /// <summary>
        /// Find average pupil center of two eyes.
        /// </summary>
        /// <param name="gazeData">gaze data frame to base calculation upon</param>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        /// <returns>the average center point in pixels</returns>
        public static Point2D getEyesCenterPixels(GazeData gazeData, int screenWidth, int screenHeight)
        {
            if (null != gazeData)
                return getEyesCenterPixels(gazeData.LeftEye, gazeData.RightEye, screenWidth, screenHeight);
            else
                return null;
        }

        private static double _MinimumEyesDistance = 0.1f;
        private static double _MaximumEyesDistance = 0.3f;

        /// <summary>
        /// Calculates distance between pupil centers based on previously
        /// recorded min and max values
        /// </summary>
        /// <param name="leftEye"></param>
        /// <param name="rightEye"></param>
        /// <returns>a normalized value [0..1]</returns>
        public static double getEyesDistanceNormalized(Eye leftEye, Eye rightEye)
        {
            double dist = Math.Abs(getDistancePoint2D(leftEye.PupilCenterCoordinates, rightEye.PupilCenterCoordinates));

            if (dist < _MinimumEyesDistance)
                _MinimumEyesDistance = dist;

            if (dist > _MaximumEyesDistance)
                _MaximumEyesDistance = dist;

            //return normalized
            return dist / (_MaximumEyesDistance - _MinimumEyesDistance);
        }

        /// <summary>
        /// Calculates distance between pupil centers based on previously
        /// recorded min and max values
        /// </summary>
        /// <param name="gazeData">gaze data frame to base calculation upon</param>
        /// <returns>a normalized value [0..1]</returns>
        public static double getEyesDistanceNormalized(GazeData gazeData)
        {
            return getEyesDistanceNormalized(gazeData.LeftEye, gazeData.RightEye);
        }

        /// <summary>
        /// Calculates distance between two points.
        /// </summary>
        public static double getDistancePoint2D(Point2D a, Point2D b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// Converts a relative point to screen point in pixels
        /// </summary>
        public static Point2D getRelativeToScreenSpace(Point2D point, int screenWidth, int screenHeight)
        {
            Point2D screenPoint = null;

            if (null != point)
            {
                screenPoint = new Point2D(point);
                screenPoint.X = Math.Round(screenPoint.X * screenWidth);
                screenPoint.Y = Math.Round(screenPoint.Y * screenHeight);
            }

            return screenPoint;
        }

        /// <summary>
        /// Normalizes a pixel point based on screen dims
        /// </summary>
        /// <param name="point">point in pixels to normalize</param>
        /// <param name="screenWidth">the width value to base normalization upon</param>
        /// <param name="screenHeight">the height value to base normalization upon</param>
        /// <returns>normalized 2d point</returns> 
        public static Point2D getNormalizedCoords(Point2D point, int screenWidth, int screenHeight)
        {
            Point2D norm = null;

            if (null != point)
            {
                norm = new Point2D(point);
                norm.X /= screenWidth;
                norm.Y /= screenHeight;
            }

            return norm;
        }

        /// <summary>
        /// Maps a 2d pixel point into normalized space [x: -1:1 , y: -1:1]
        /// </summary>
        /// <param name="point">point in pixels to normalize</param>
        /// <param name="screenWidth">the width value to base normalization upon</param>
        /// <param name="screenHeight">the height value to base normalization upon</param>
        /// <returns>normalized 2d point</returns> 
        public static Point2D getNormalizedMapping(Point2D point, int screenWidth, int screenHeight)
        {
            Point2D normMap = getNormalizedCoords(point, screenWidth, screenHeight);

            if (null != normMap)
            {
                //scale up and shift
                normMap.X *= 2f;
                normMap.X -= 1f;
                normMap.Y *= 2f;
                normMap.Y -= 1f;
            }

            return normMap;
        }
    }
}
