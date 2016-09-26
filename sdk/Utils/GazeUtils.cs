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
using EyeTribe.ClientSdk.Data;

namespace EyeTribe.ClientSdk.Utils
{
    /// <summary>
    /// Utility methods common to gaze control routines.
    /// </summary>
    public class GazeUtils
    {
        protected GazeUtils() 
        {
            //ensure non-instantiability, allowing inheritance
        }

        /// <summary>
        /// Find average pupil center of two eyes.
        /// </summary>
        /// <param name="leftEye"></param>
        /// <param name="rightEye"></param>
        /// <returns>the average center point in normalized values</returns>
        public static Point2D GetEyesCenterNormalized(Eye leftEye, Eye rightEye)
        {
            Point2D eyeCenter = Point2D.Zero;

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

            return Point2D.Zero;
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

            return Point2D.Zero;
        }

        private static double _MinimumEyesDistance = 0.1f;
        private static double _MaximumEyesDistance = 0.3f;

        /// <summary>
        /// Calculates distance between pupil centers based on previously
        /// recorded min and max values
        /// </summary>
        /// <param name="leftEye"></param>
        /// <param name="rightEye"></param>
        /// <returns>a normalized value [0f..1f]</returns>
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
        /// <returns>a normalized value [0f..1f]</returns>
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
        /// Converts a 2d point in relative values to a coordinate within parameter dimensions.
        /// <para/>
        /// Use to map relative values [0f..1f] to screen coordinates.
        /// </summary>
        /// <param name="point">gaze point to base calculation upon</param>
        /// <param name="dimWidthPix">dimension width in pixels to base calculation upon</param>
        /// <param name="dimHeightPix">dimension height in pixels to base calculation upon</param>
        /// <returns>2D point in screen space</returns>
        public static Point2D GetRelativeToScreenSpace(Point2D point, int dimWidthPix, int dimHeightPix)
        {
            return new Point2D(
                point.X * dimWidthPix, 
                point.Y * dimHeightPix
                );
        }

        /// <summary>
        /// Converts a 2d screen point in pixels to normalized relative values based on parameter dimensions.
        /// </summary>
        /// <param name="point">gaze point to base calculation upon</param>
        /// <param name="dimWidthPix">dimension width in pixels to base calculation upon</param>
        /// <param name="dimHeightPix">dimension height in pixels to base calculation upon</param>
        /// <returns>2D point in relative values</returns>
        public static Point2D GetScreenSpaceToRelative(Point2D point, int dimWidthPix, int dimHeightPix)
        {
            return new Point2D(
                    point.X / GazeManager.Instance.ScreenResolutionWidth, 
                    point.Y / GazeManager.Instance.ScreenResolutionHeight
                    );
        }

        /// <summary>
        /// Clamps a gaze points within the limits of the parameter rect.
        /// </summary>
        /// <param name="gaze">gaze point to base calculation upon</param>
        /// <param name="dimWidth">dimension width to base calculation upon</param>
        /// <param name="dimHeight">dimension height to base calculation upon</param>
        /// <returns>clamped gaze point</returns>
        public static Point2D ClampGazeToRect(Point2D gaze, int dimWidth, int dimHeight)
        {
            return ClampGazeToRect(gaze, 0, 0, dimWidth, dimHeight, 0, 0);
        }

        /// <summary>
        /// Clamps a gaze points within the limits of the parameter rect.
        /// </summary>
        /// <param name="gaze">gaze data frame to base calculation upon</param>
        /// <param name="dimX">x coordinate of topleft rect anchor</param>
        /// <param name="dimY">y coordinate of topleft rect anchor</param>
        /// <param name="dimWidth">dimension width to base calculation upon</param>
        /// <param name="dimHeight">dimension height to base calculation upon</param>
        /// <returns>clamped gaze point</returns>
        public static Point2D ClampGazeToRect(Point2D gaze, int dimX, int dimY, int dimWidth, int dimHeight)
        {
            return ClampGazeToRect(gaze, dimX, dimY, dimWidth, dimHeight, 0, 0);
        }

        /// <summary>
        /// Clamps a gaze points within the limits of the parameter rect.
        /// </summary>
        /// <param name="gaze">gaze data frame to base calculation upon</param>
        /// <param name="dimX">x coordinate of topleft rect anchor</param>
        /// <param name="dimY">y coordinate of topleft rect anchor</param>
        /// <param name="dimWidth">dimension width to base calculation upon</param>
        /// <param name="dimHeight">dimension height to base calculation upon</param>
        /// <param name="clampMargin">extra space surrounding rect to be included</param>
        /// <returns>clamped gaze point</returns>
        public static Point2D ClampGazeToRect(Point2D gaze, int dimWidth, int dimHeight, float clampMargin)
        {
            return ClampGazeToRect(gaze, 0, 0, dimWidth, dimHeight, clampMargin, clampMargin);
        }

        /// <summary>
        /// Clamps a gaze points within the limits of the parameter rect.
        /// </summary>
        /// <param name="gaze">gaze data frame to base calculation upon</param>
        /// <param name="dimX">x coordinate of topleft rect anchor</param>
        /// <param name="dimY">y coordinate of topleft rect anchor</param>
        /// <param name="dimWidth">dimension width to base calculation upon</param>
        /// <param name="dimHeight">dimension height to base calculation upon</param>
        /// <param name="clampHorsMargin">extra hors space surrounding rect to be included</param>
        /// <param name="clampVertMargin">extra vert space surrounding rect to be included</param>
        /// <returns>clamped gaze point</returns>
        public static Point2D ClampGazeToRect(Point2D gaze, int dimX, int dimY, int dimWidth, int dimHeight, float clampHorsMargin, float clampVertMargin)
        {
            Point2D clamped = new Point2D(gaze);

            if (null != gaze)
            {
                if (gaze.X < dimX && gaze.X > dimX - clampHorsMargin)
                    clamped.X = dimX + 1;

                if(gaze.X > dimX + dimWidth && gaze.X < (dimX + dimWidth + clampHorsMargin))
                    clamped.X = dimX + dimWidth - 1;

                if (gaze.Y < dimY && gaze.Y > dimY - clampVertMargin)
                    clamped.Y = dimY + 1;

                if (gaze.Y > dimY + dimHeight && gaze.Y < (dimY + dimHeight + clampVertMargin))
                    clamped.Y = dimY + dimHeight - 1;
            }

            return clamped;
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
            return new Point2D(point.X /= screenWidth, point.Y /= screenHeight);
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

            //scale up and shift
            normMap.X *= 2f;
            normMap.X -= 1f;
            normMap.Y *= 2f;
            normMap.Y -= 1f;

            return normMap;
        }
    }
}
