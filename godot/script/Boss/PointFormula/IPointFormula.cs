// res://script/Boss/PointFormula/IPointFormula.cs
namespace Boss.PointFormula
{
	/// <summary>
	/// 粒子坐标公式接口
	/// </summary>
	public interface IPointFormula
	{
		/// <param name="xArg">当前粒子索引 i</param>
		/// <param name="yArg">i / 235.0 —— 你现在用的比例</param>
		/// <param name="t">内部时间增量</param>
		/// <param name="frame">帧计数</param>
		/// <returns>粒子局部 (x,y)</returns>
		(double x, double y) MapPoint(double xArg, double yArg, double t, int frame);
	}
}
