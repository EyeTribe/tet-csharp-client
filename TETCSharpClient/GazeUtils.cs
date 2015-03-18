/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

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
        public static Point2D GetEyesCenterNormalized(Eye leftEye, Eye rightEye)
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
        public static Point2D GetEyesCenterNormalized(GazeData gazeData)
        {
            if (null != gazeData)
                return GetEyesCenterNormalized(gazeData.LeftEye, gazeData.RightEye);
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
        public static Point2D GetEyesCenterPixels(Eye leftEye, Eye rightEye, int screenWidth, int screenHeight)
        {
            Point2D center = GetEyesCenterNormalized(leftEye, rightEye);

            return GetRelativeToScreenSpace(center, screenWidth, screenHeight);
        }

        /// <summary>
        /// Find average pupil center of two eyes.
        /// </summary>
        /// <param name="gazeData">gaze data frame to base calculation upon</param>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        /// <returns>the average center point in pixels</returns>
        public static Point2D GetEyesCenterPixels(GazeData gazeData, int screenWidth, int screenHeight)
        {
            if (null != gazeData)
                return GetEyesCenterPixels(gazeData.LeftEye, gazeData.RightEye, screenWidth, screenHeight);
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
        public static double GetEyesDistanceNormalized(Eye leftEye, Eye rightEye)
        {
            double dist = Math.Abs(GetDistancePoint2D(leftEye.PupilCenterCoordinates, rightEye.PupilCenterCoordinates));

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
        public static double GetEyesDistanceNormalized(GazeData gazeData)
        {
            return GetEyesDistanceNormalized(gazeData.LeftEye, gazeData.RightEye);
        }

        /// <summary>
        /// Calculates distance between two points.
        /// </summary>
        public static double GetDistancePoint2D(Point2D a, Point2D b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// Converts a relative point to screen point in pixels
        /// </summary>
        public static Point2D GetRelativeToScreenSpace(Point2D point, int screenWidth, int screenHeight)
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
        public static Point2D GetNormalizedCoords(Point2D point, int screenWidth, int screenHeight)
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
        public static Point2D GetNormalizedMapping(Point2D point, int screenWidth, int screenHeight)
        {
            Point2D normMap = GetNormalizedCoords(point, screenWidth, screenHeight);

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

        /// <summary>
        /// Returns the time difference between GazeData timestamp and current system time in millis
        /// </summary>
        /// <param name="gazeData">gaze data frame to base calculation upon</param>
        /// <returns>time difference in milliseconds</returns> 
        public static long GetTimeDeltaNow(GazeData gazeData)
        {
            return (long)Math.Round(((double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - gazeData.TimeStamp);
        }
    }
}
