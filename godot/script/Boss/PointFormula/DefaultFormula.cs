// res://script/Boss/PointFormula/DefaultFormula.cs
using System;
using Boss.PointFormula;

public class DefaultFormula : IPointFormula
{
	public (double x, double y) MapPoint(double x, double y, double t, int frame)
	{
		double k = (4 + Math.Sin(y * 2 - t) * 3) * Math.Cos(x / 29);
		double e = y / 8 - 13;
		double d = Math.Sqrt(k * k + e * e);
		double q = 3 * Math.Sin(k * 2) + 0.3 / k
				   + Math.Sin(y / 25) * k * (9 + 4 * Math.Sin(e * 9 - d * 3 + t * 2));
		double c = d - t;

		return (
			q + 30 * Math.Cos(c) + 200 + frame,
			q * Math.Sin(c) + d * 39 - 220
		);
	}
}
