using System;

namespace ForestSolver
{
    public class Point
    {
        public readonly int X;
        public readonly int Y;
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        static public Point ConvertFromNetPoint(ForestSolverPackages.Point p)
        {
            var t = new Point(p.X, p.Y);
            return t;
        }

        public Point Add(Point other)
        {
            return new Point(this.X + other.X, this.Y + other.Y); 
        }

        public Point Add(DeltaPoint other)
        {
            return new Point(X + other.Deltax, Y + other.Deltay);
        }

        public Point Reverse()
        {
            return new Point(Y, X);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Point))
                throw new InvalidCastException();
            var o = (Point) obj;
            return X == o.X && Y == o.Y;
        }

        static public bool operator ==(Point one, Point two)
        {
            return one.Equals(two);
        }

        static public bool operator !=(Point one, Point two)
        {
            return !(one == two);
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        public ForestSolverPackages.Point ConvertToNetPoint()
        {
            return new ForestSolverPackages.Point(X, Y);
        }
    }

    public class DeltaPoint
    {
        public readonly int Deltax;
        public readonly int Deltay;

        private DeltaPoint(int x, int y)
        {
            Deltax = x;
            Deltay = y;
        }

        public static DeltaPoint GoLeft()
        {
            return new DeltaPoint(-1, 0);
        }

        public static DeltaPoint GoRight()
        {
            return new DeltaPoint(1, 0);
        }

        public static DeltaPoint GoDown()
        {
            return new DeltaPoint(0, 1);
        }

        public static DeltaPoint GoUp()
        {
            return new DeltaPoint(0, -1);
        }

        public DeltaPoint Reverse()
        {
            return new DeltaPoint(-Deltax, -Deltay);
        }

        public override bool Equals(object obj)
        {
            var oth = (DeltaPoint) obj;
            return Deltax == oth.Deltax && Deltay == oth.Deltay;
        }

        public override int GetHashCode()
        {
            return Deltax*2 + Deltay*3;
        }
    }
}
