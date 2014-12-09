/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
namespace TETCSharpClient.Data
{
    /// <summary>
    /// A 3D coordinate with double precision.
    /// </summary>
    public class Point3D : Point2D
    {
        protected double z;

        public static readonly Point3D zero = new Point3D();

        public Point3D()
            : base()
        {
            this.z = 0d;
        }

        public Point3D(double x, double y, double z)
            : base(x, y)
        {
            this.z = z;
        }

        public Point3D(Point3D other)
            : base((Point2D)other)
        {
            this.z = other.z;
        }

        public double Z
        {
            get { return z; }
            set { z = value; }
        }

        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is Point3D))
                return false;

            var other = o as Point3D;

            return base.Equals(o) && Double.Equals(z, other.z);
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
            return new Point3D(point1.x * k, point1.y * k, point1.z * k);
        }

        public static Point3D operator /(Point3D point1, double k)
        {
            return new Point3D(point1.x / k, point1.y / k, point1.z / k);
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
            return new Point3D(x * k, y * k, z * k);
        }

        public Point3D Divide(double k)
        {
            return new Point3D(x / k, y / k, z / k);
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
