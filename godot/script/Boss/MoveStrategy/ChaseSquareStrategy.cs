using Godot;    // 引入 Godot 命名空间，提供 Node2D、Vector2、Mathf 等类型
using System;   // 引入 System 命名空间，提供 Func<>、Math 等类型

// 定义一个名为 ChaseSquareStrategy 的类，
// 实现 IMoveStrategy 接口，用于实现“追逐”目标的移动策略。
public class ChaseSquareStrategy : IMoveStrategy
{
	// 用于存储策略所需要的依赖和参数

	Node2D host;
	// host：被移动的 Godot 节点（宿主对象）
	// 例如：敌人节点、玩家节点等

	Func<Vector2> getTarget;
	// getTarget：一个返回 Vector2 的函数，用来动态获取目标位置。
	// 好处：
	//   - 不写死目标
	//   - 可以实时跟踪目标位置

	double moveSpd;
	// moveSpd：移动速度，单位像素/秒

	double rotSpd;
	// rotSpd：旋转速度，单位弧度/秒

	double posEps;
	// posEps：与目标之间的容许误差，单位像素
	// 用于避免因浮点误差导致永远无法“到达”目标

	// 初始化方法，在策略生效前被调用
	// 用于把外部依赖和参数注入进来
	public void Initialize(
		Node2D host,
		Func<Vector2> target,
		double moveSpeed,
		double rotateSpeedRad,
		double posEps)
	{
		this.host      = host;          // 保存宿主节点引用
		this.getTarget = target;        // 保存获取目标位置的委托
		this.moveSpd   = moveSpeed;     // 保存移动速度
		this.rotSpd    = rotateSpeedRad;// 保存旋转速度
		this.posEps    = posEps;        // 保存容许误差
	}

	// 每帧调用一次，推动移动逻辑
	public void Advance(double delta)
	{
		// 先取得宿主节点的当前全局位置（在 2D 场景中的位置）
		Vector2 scenePos = host.Position;

		// 获取目标位置（调用 getTarget 委托）
		Vector2 target = getTarget();

		// 计算向量：
		// 从宿主位置指向目标位置
		Vector2 toTarget = target - scenePos;

		// 计算到目标的距离
		double dist = toTarget.Length();

		/* ------------------------------
		 * 以下部分用于处理“旋转朝向目标”
		 * ------------------------------ */

		// 计算理想角度 desiredRot：
		// - 注意 Godot 的角度定义：
		//   角度 = 0 时朝向上（负 Y 轴）
		//   正角度顺时针旋转
		// Math.Atan2(y, x)：
		//   - 一般写法是 Atan2(deltaY, deltaX)
		//   - 这里为了配合 Godot 的坐标系，写成 Atan2(toTarget.X, -toTarget.Y)
		//     因为在 Godot 中，Y 向下是正方向
		double desiredRot = Math.Atan2(toTarget.X, -toTarget.Y);

		// 将 desiredRot 限制在 [-π, π] 区间内
		desiredRot = Mathf.Wrap((float)desiredRot, -Mathf.Pi, Mathf.Pi);

		// 计算角度差 angDiff：
		// = desiredRot - host 当前朝向
		// 同样需要 wrap，防止出现超过 π 的差值
		double angDiff = Mathf.Wrap(
			(float)(desiredRot - host.Rotation),
			-Mathf.Pi,
			Mathf.Pi
		);

		// 计算本帧允许的最大旋转角度
		double maxStep = rotSpd * delta;

		// Clamp angDiff 到 [-maxStep, maxStep]，防止本帧转太多
		// 最终更新 host 的 Rotation
		host.Rotation = Mathf.Wrap(
			(float)(host.Rotation + Math.Clamp(angDiff, -maxStep, maxStep)),
			-Mathf.Pi,
			Mathf.Pi
		);

		/* ------------------------------
		 * 以下部分用于处理“位移向目标”
		 * ------------------------------ */

		// 如果距离大于误差阈值，就移动向目标
		// 否则不移动
		host.Position += dist > posEps
			? toTarget.Normalized() * (float)Math.Min(moveSpd * delta, dist)
			: Vector2.Zero;

		// 上面这行代码解释：
		// - toTarget.Normalized()：获得方向向量（长度为 1）
		// - moveSpd * delta：本帧最多能走的距离
		// - Math.Min(...)：保证不会超出目标距离
		//   → 防止移过头导致“抖动”
		// - 如果已经靠近目标 (dist <= posEps)，则不再移动
	}
}
