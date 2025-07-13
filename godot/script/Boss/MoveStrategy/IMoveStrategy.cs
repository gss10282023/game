using Godot;    // 引入 Godot 引擎的命名空间，才能使用 Node2D、Vector2 等 Godot 自带类型
using System;   // 引入 .NET 系统命名空间，才能使用 Func<> 等委托类型

// 定义一个“移动策略”接口，用于描述不同的移动行为算法。
// 所有想实现具体移动逻辑的类，都需要实现这个接口。
public interface IMoveStrategy
{
	/// <summary>
	/// 在策略生效前调用，一次性注入依赖。
	/// 用来初始化策略需要的各种数据，例如：
	/// - 移动的对象是谁（host）
	/// - 移动目标的位置（target）
	/// - 移动速度、旋转速度、精度要求等参数
	void Initialize(
		Node2D host,
		// host：需要被移动的 Godot 节点。
		Func<Vector2> target,
		// target：一个返回 Vector2 的函数（没有参数）。
		// 意义：
		//   - 不用写死目标位置
		//   - 可以动态获取目标

		double moveSpeed,
		// moveSpeed：移动速度，单位一般是像素/秒

		double rotateSpeedRad,
		// rotateSpeedRad：旋转速度，单位是弧度/秒。
		// 举例：Math.PI 表示每秒旋转 180 度。

		double posEps);
		// posEps：到目标点的容许误差，单位是像素。
		// 用于避免浮点误差导致“永远靠不近目标”的问题。
		// 举例：posEps = 1 表示距离目标 1 像素以内就算到达。

	/// <summary>
	/// 逐帧调用。
	/// 
	/// 用于驱动移动逻辑，在游戏每帧都执行一次。
	/// 类似 Godot 的 _Process 或 _PhysicsProcess。
	/// 
	/// 参数 delta：
	///   - 本帧经过的时间（秒）
	///   - 举例：60FPS 下，delta 大约是 0.016666...
	/// 
	/// 用途：
	///   - 实现帧率无关的移动
	///     例如：
	///       移动距离 = 速度 * delta
	/// </summary>
	void Advance(double delta);
}
