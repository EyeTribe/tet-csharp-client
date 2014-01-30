namespace TETCSharpClient.Data
{
    public class Point3D : Point2D
    {
        private double z;

        public Point3D(double x, double y, double z)
        {
            base.X = x;
            base.Y = y;
            this.z = z;
        }

        public double Z
        {
            get { return z; }
            set { z = value; }
        }
    }
}
