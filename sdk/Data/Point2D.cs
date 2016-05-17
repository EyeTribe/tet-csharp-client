/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;

namespace EyeTribe.ClientSdk.Data
{
    /// <summary>
    /// A 2D coordinate with float precision.
    /// </summary>
    public struct Point2D
    {
        private float x;
        private float y;

        public const float Epsilon = 1e-005f;

        public static readonly Point2D Zero = new Point2D();

        public Point2D(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Point2D(Point2D other)
            : this(other.x, other.y)
        {
        }

        public float X
        {
            get { return x; }
            set { x = value; }
        }

        public float Y
        {
            get { return y; }
            set { y = value; }
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is Point2D))
                return false;

            Point2D other = (Point2D)o;

            return
                Double.Equals(this.x, other.x) &&
                Double.Equals(this.y, other.y);
        }

        public override int GetHashCode()
        {
            int hash = 571;
            hash = hash * 2777 + X.GetHashCode();
            hash = hash * 2777 + Y.GetHashCode();
            return hash;
        }

        public static Point2D operator +(Point2D point1, Point2D point2)
        {
            return new Point2D { x = point1.x + point2.x, y = point1.y + point2.y };
        }

        public static Point2D operator -(Point2D point1, Point2D point2)
        {
            return new Point2D { x = point1.x - point2.x, y = point1.y - point2.y };
        }

        public static Point2D operator -(Point2D point1)
        {
            return new Point2D(-point1.x, -point1.y);
        }

        public static Point2D operator *(Point2D point1, Point2D multi)
        {
            return new Point2D { x = point1.x * multi.x, y = point1.y * multi.y };
        }

        public static Point2D operator *(Point2D point1, double k)
        {
            return new Point2D((float)(k * point1.x), (float)(k * point1.y));
        }

        public static Point2D operator /(Point2D point1, double k)
        {
            return new Point2D((float)(point1.x / k), (float)(point1.y / k));
        }

        public static bool operator ==(Point2D point1, Point2D point2)
        {
            return Double.Equals(point1.x, point2.x) && Double.Equals(point1.y, point2.y);
        }

        public static bool operator !=(Point2D point1, Point2D point2)
        {
            return !(point1 == point2);
        }

        public Point2D Add(Point2D p2)
        {
            return new Point2D(x + p2.x, y + p2.y);
        }

        public Point2D Subtract(Point2D p2)
        {
            return new Point2D(x - p2.x, y - p2.y);
        }

        public Point2D Multiply(int k)
        {
            return new Point2D(x * k, y * k);
        }

        public Point2D Divide(int k)
        {
            return new Point2D(x / k, y / k);
        }

        public Point2D Multiply(double k)
        {
            return new Point2D((float)(k * x), (float)(k * y));
        }

        public Point2D Divide(double k)
        {
            return new Point2D((float)(x / k), (float)(y / k));
        }

        public double Average()
        {
            return (x + y) / 2;
        }

        public override String ToString()
        {
            return "{" + x + ", " + y + "}";
        }
    }
}