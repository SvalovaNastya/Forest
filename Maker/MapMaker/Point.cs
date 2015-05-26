using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
	public class Point
	{
		protected bool Equals(Point other)
		{
			return Y == other.Y && X == other.X;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Y << 10) + X;
			}
		}

		public int Y { get; private set; }
		public int X { get; private set; }

		public Point(int y, int x)
		{
			Y = y;
			X = x;
		}

		public Point Add(Point point)
		{
			return new Point(Y + point.Y, X + point.X);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Point) obj);
		}
	}
}
