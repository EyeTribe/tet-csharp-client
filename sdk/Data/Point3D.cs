/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections;
namespace EyeTribe.ClientSdk.Data
{
    /// <summary>
    /// A 3D coordinate with float precision.
    /// </summary>
    public struct Point3D
    {
        private float x;
        private float y;
        private float z;

        public const float Epsilon = 1e-005f;

        public static readonly Point3D Zero = new Point3D();

        public Point3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Point3D(Point3D other)
            :  this(other.x, other.y,other.z)
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

        public float Z
        {
            get { return z; }
            set { z = value; }
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is Point3D))
                return false;

            Point3D other = (Point3D)o;

            return
                Double.Equals(this.x, other.x) &&
                Double.Equals(this.y, other.y) &&
                Double.Equals(this.z, other.z);
        }

        public override int GetHashCode()
        {
            int hash = 571;
            hash = hash * 2777 + X.GetHashCode();
            hash = hash * 2777 + Y.GetHashCode();
            hash = hash * 2777 + Z.GetHashCode();
            return hash;
        }

        public static Point3D operator +(Point3D point1, Point3D point2)
        {
            return new Point3D { x = point1.x + point2.x, y = point1.y + point2.y, z = point1.z + point2.z };
        }

        public static Point3D operator -(Point3D point1, Point3D point2)
        {
            return new Point3D { x = point1.x - point2.x, y = point1.y - point2.y, z = point1.z * point2.z };
        }

        public static Point3D operator -(Point3D point1)
        {
            return new Point3D(-point1.x, -point1.y, -point1.z);
        }

        public static Point3D operator *(Point3D point1, Point3D multi)
        {
            return new Point3D { x = point1.x * multi.x, y = point1.y * multi.y, z = point1.z * multi.z };
        }

        public static Point3D operator *(Point3D point1, double k)
        {
            return new Point3D((float)(k * point1.x), (float)(k * point1.y), (float)(k * point1.z));
        }

        public static Point3D operator /(Point3D point1, double k)
        {
            return new Point3D((float)(point1.x / k), (float)(point1.y / k), (float)(point1.z / k));
        }

        public static bool operator ==(Point3D point1, Point3D point2)
        {
            return Double.Equals(point1.x, point2.x) && Double.Equals(point1.y, point2.y) && Double.Equals(point1.z, point2.z);
        }

        public static bool operator !=(Point3D point1, Point3D point2)
        {
            return !(point1 == point2);
        }

        public Point3D Add(Point3D p2)
        {
            return new Point3D(x + p2.x, y + p2.y, z + p2.z);
        }

        public Point3D Subtract(Point3D p2)
        {
            return new Point3D(x - p2.x, y - p2.y, z - p2.z);
        }

        public Point3D Multiply(double k)
        {
            return new Point3D((float)(k * x), (float)(k * y), (float)(k * z));
        }

        public Point3D Divide(double k)
        {
            return new Point3D((float)(x / k), (float)(y / k), (float)(z / k));
        }

        public double Average()
        {
            return (x + y + z) / 3;
        }

        public override String ToString()
        {
            return "{" + x + ", " + y + ", " + z + "}";
        }
    }
}
