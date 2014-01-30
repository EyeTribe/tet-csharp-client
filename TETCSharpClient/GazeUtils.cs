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
        /// <param name="gazeData"/>gaze data frame to base calculation upon</param>
        /// <returns>the average center point in normalized values</returns>
        public static Point2D getEyesCenterNormalized(GazeData gazeData)
        {
            Point2D eyeCenter = new Point2D();

            Point2D left = gazeData.LeftEye.PupilCenterCoordinates;
            Point2D right = gazeData.RightEye.PupilCenterCoordinates;

            if (null != left && null != right)
            {
                eyeCenter.X = (left.X + right.X) / 2;
                eyeCenter.Y = (left.Y + right.Y) / 2;
           } 
            else
            if (null != left)
            {
                eyeCenter = left;
            }
            else
            if (null != right)
            {
                eyeCenter = right;
            }

            return eyeCenter;
        }

        /// <summary>
        /// Find average pupil center of two eyes.
        /// </summary>
        /// <param name="gazeData"/>gaze data frame to base calculation upon</param>
        /// <returns>the average center point in pixels</returns>
        public static Point2D getEyesCenterPixels(GazeData gazeData, int screenWidth, int screenHeight)
        {
            Point2D center = getEyesCenterNormalized(gazeData);

            return getRelativeToScreenSpace(center, screenWidth, screenHeight);
        }

        private static double _MinimumEyesDistance = 0.1f;
        private static double _MaximumEyesDistance = 0.3f;

        /// <summary>
        /// Calculates distance between pupil centers based on previously
        /// recorded min and max values
        /// </summary>
        /// <param name="gazeData"/>gaze data frame to base calculation upon</param>
        /// <returns>a normalized value [0..1]</returns>
        public static double getEyesDistanceNormalized(GazeData gazeData)
        {
            double dist = Math.Abs(getDistancePoint2D(gazeData.LeftEye.PupilCenterCoordinates, gazeData.RightEye.PupilCenterCoordinates));

            if (dist < _MinimumEyesDistance)
                _MinimumEyesDistance = dist;

            if (dist > _MaximumEyesDistance)
                _MaximumEyesDistance = dist;

            //return normalized
            return dist / (_MaximumEyesDistance - _MinimumEyesDistance);
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
            Point2D screen = new Point2D(point);
            screen.X = Math.Round(screen.X * screenWidth);
            screen.Y = Math.Round(screen.Y * screenHeight);

            return screen;
        }

        /// <summary>
        /// Normalizes a pixel point based on screen dims
        /// </summary>
        /// <param name="point"/>point in pixels to normalize</param>
        /// <param name="screenWidth"/>the width value to base normalization upon</param>
        /// <param name="screenHeight"/>the height value to base normalization upon</param>
        /// <returns>normalized 2d point</returns> 
        public static Point2D getNormalizedCoords(Point2D point, int screenWidth, int screenHeight)
        {
            Point2D norm = new Point2D(point);
            norm.X /= screenWidth;
            norm.Y /= screenHeight;
            return norm;
        }

        /// <summary>
        /// Maps a 2d pixel point into normalized space [x: -1:1 , y: -1:1]
        /// </summary>
        /// <param name="point"/>point in pixels to normalize</param>
        /// <param name="screenWidth"/>the width value to base normalization upon</param>
        /// <param name="screenHeight"/>the height value to base normalization upon</param>
        /// <returns>normalized 2d point</returns> 
        public static Point2D getNormalizedMapping(Point2D point, int screenWidth, int screenHeight)
        {
            Point2D normMap = getNormalizedCoords(point, screenWidth, screenHeight);

            //scale up and shift
            normMap.X *= 2f;
            normMap.X -= 1f;
            normMap.Y *= 2f;
            normMap.Y -= 1f;

            return normMap;
        }
    }
}
