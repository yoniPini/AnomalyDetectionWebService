using System;
using System.Collections.Generic;
using System.Linq; // float[].Average();
using System.Text;
using System.Threading.Tasks;

namespace AnomalyDetectionWebService.Models.Utils
{

	public class Line
	{
		public float a { get; set; }
		public float b { get; set; }
		public float f(float x)
		{
			return a * x + b;
		}
	}

	public class Point
	{
		public float x { get; set; }
		public float y { get; set; }
	}
	public class Circle
	{
		public Point center { get; set; }
		public float radius { get; set; }
	}
	public class MathUtil
	{
		public static float Var(float[] arr)
		{
			float sum = 0;
			foreach (float f in arr) sum += (f * f);
			return sum / arr.Length - (float)Math.Pow(arr.Average(), 2);
		}
		public static float Cov(float[] x, float[] y)
		{
			float sum = 0;
			for (int i = 0; i < x.Length; i++) sum += (x[i] * y[i]);
			return sum / x.Length - x.Average() * y.Average();
		}
		public static float Pearson(float[] x, float[] y)
		{
			return Cov(x, y) / (float)Math.Sqrt(Var(x) * Var(y));
		}
		public static float AbsPearson(float[] x, float[] y)
		{
			var p = Pearson(x, y);
			return p > 0 ? p : -p;
		}

		public static Line Reg(float[] x, float[] y)
		{
			float _a = Cov(x, y) / Var(x);
			float _b = y.Average() - _a * x.Average();
			return new Line() { a = _a, b = _b };
		}

		public static float Dev(Point p, float[] x, float[] y) { return Dev(p, Reg(x, y)); }
		public static float Dev(Point p, Line l) { return (float)Math.Abs(p.y - l.f(p.x)); }

		// find minimal ecnclosing circle which contains the given points
		public static Circle findMinCircle(List<Point> points)
		{
			return MinimalCircle.findMinCircle(points);
		}
		public static Circle findMinCircle(float[] x, float[] y)
		{
			List<Point> list = new List<Point>();
			for (int i = 0; i < x.Length; i++)
				list.Add(new Point() { x = x[i], y = y[i] });
			return findMinCircle(list);
		}
	}

	public class MinimalCircle
	{
		// trivial minimal ecnclosing circle which contains the given points
		private static Circle from2points(Point a, Point b)
		{
			float _x = (a.x + b.x) / 2;
			float _y = (a.y + b.y) / 2;
			float _r = dist(a, b) / 2;
			return new Circle() { center = new Point() { x = _x, y = _y }, radius = _r };
		}

		public static float dist(Point a, Point b)
		{
			float x2 = (a.x - b.x) * (a.x - b.x);
			float y2 = (a.y - b.y) * (a.y - b.y);
			return (float)Math.Sqrt(x2 + y2);
		}
		// trivial minimal ecnclosing circle which contains the given points
		private static Circle from3Points(Point a, Point b, Point c)
		{
			Point mAB = new Point() { x = (a.x + b.x) / 2, y = (a.y + b.y) / 2 }; // mid point of line AB
			float slopAB = (b.y - a.y) / (b.x - a.x);
			float pSlopAB = -1 / slopAB;

			Point mBC = new Point() { x = (b.x + c.x) / 2, y = (b.y + c.y) / 2 };
			float slopBC = (c.y - b.y) / (c.x - b.x);
			float pSlopBC = -1 / slopBC;



			float x = (-pSlopBC * mBC.x + mBC.y + pSlopAB * mAB.x - mAB.y) / (pSlopAB - pSlopBC);
			float y = pSlopAB * (x - mAB.x) + mAB.y;
			Point center = new Point() { x = x, y = y };
			float R = dist(center, a);

			return new Circle() { center = center, radius = R };
		}

		// trivial minimal ecnclosing circle which contains the given points
		private static Circle trivial(List<Point> P)
		{
			if (P.Count == 0)
				return new Circle() { center = new Point() { x = 0, y = 0 }, radius = 0 };
			else if (P.Count == 1)
				return new Circle() { center = P[0], radius = 0 };
			else if (P.Count == 2)
				return from2points(P[0], P[1]);

			// maybe 2 of the points define a small circle that contains the 3ed point
			Circle c = from2points(P[0], P[1]);
			if (dist(P[2], c.center) <= c.radius)
				return c;
			c = from2points(P[0], P[2]);
			if (dist(P[1], c.center) <= c.radius)
				return c;
			c = from2points(P[1], P[2]);
			if (dist(P[0], c.center) <= c.radius)
				return c;
			// else find the unique circle from 3 points
			return from3Points(P[0], P[1], P[2]);
		}

		// welzl algorithm to find minimal enclosing circle, P is points, R is points that have to be on the circumference of a circle, n is |P|
		private static Circle welzl(List<Point> P, List<Point> R, int n)
		{
			// we must take the trivial choich if we have limits |R| = 3 or |P| = 0
			if (n == 0 || R.Count == 3)
			{
				return trivial(new List<Point>(R));
			}

			// let P = P - {p}
			Point p = new Point() { x = P[n - 1].x, y = P[n - 1].y };

			// otherwise, maybe the enclosing circle doesn't have the 'last' point = p, on its circumference
			Circle c = welzl(P, new List<Point>(R), n - 1);

			if (dist(p, c.center) <= c.radius)
				return c;

			// if we here, it's means the 'last' point = p, has to be on the circumference of the enclosing circle
			R.Add(p);

			return welzl(P, new List<Point>(R), n - 1);
		}

		// find minimal ecnclosing circle which contains the given points, using welzl algorithm
		public static Circle findMinCircle(List<Point> points)
		{
			return welzl(points, new List<Point>(), points.Count);
		}


	}
}
