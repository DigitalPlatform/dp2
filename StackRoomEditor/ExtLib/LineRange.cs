//------------------------------------------
// LineRange.cs (c) 2007 by Charles Petzold
//------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public struct LineRange
    {
        Point3D point1;
        Point3D point2;

        public LineRange(Point3D point1, Point3D point2)
        {
            this.point1 = point1;
            this.point2 = point2;
        }

        public Point3D Point1
        {
            set { point1 = value; }
            get { return point1; }
        }

        public Point3D Point2
        {
            set { point2 = value; }
            get { return point2; }
        }

        public Point3D PointFromX(double x)
        {
            double factor = (x - Point1.X) / (Point2.X - Point1.X);
            double y = Point1.Y + factor * (Point2.Y - Point1.Y);
            double z = Point1.Z + factor * (Point2.Z - Point1.Z);
            return new Point3D(x, y, z);
        }

        public Point3D PointFromY(double y)
        {
            double factor = (y - Point1.Y) / (Point2.Y - Point1.Y);
            double x = Point1.X + factor * (Point2.X - Point1.X);
            double z = Point1.Z + factor * (Point2.Z - Point1.Z);
            return new Point3D(x, y, z);
        }

        public Point3D PointFromZ(double z)
        {
            double factor = (z - Point1.Z) / (Point2.Z - Point1.Z);
            double x = Point1.X + factor * (Point2.X - Point1.X);
            double y = Point1.Y + factor * (Point2.Y - Point1.Y);
            return new Point3D(x, y, z);
        }
    }
}
