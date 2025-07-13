using Godot;    // 引入 Godot 命名空间，用于 Node、Vector2、Color、GD.Print 等类型

// 定义 IdleState 类
// 实现 IBossState 接口，代表 Boss 的“待机”状态
//
// 在状态机模式中，IdleState 主要做：
// - 设置粒子颜色作为视觉提示
// - 让 Boss 保持基本移动（虽然是 idle，也可能在场上漂移）
// - 更新粒子云位置和动画
public class IdleState : IBossState
{
	// cloud：保存对 ParticleCloud 的引用
	// ParticleCloud 是自定义类，通常管理一组粒子（例如 MultiMeshInstance2D）
	readonly ParticleCloud cloud;

	// move：保存移动策略对象
	// 实现 IMoveStrategy，用于驱动 Boss 在场景中的移动逻辑
	readonly IMoveStrategy move;

	// IdleState 构造函数
	// 接收外部注入的 cloud 与 move
	// 好处：
	//   - 解耦依赖
	//   - 状态类不需要自己 new，而是由外部创建并注入
	public IdleState(ParticleCloud cloud, IMoveStrategy move)
	{
		this.cloud = cloud;
		this.move  = move;
	}

	// 当 Boss 进入 IdleState 时会调用此方法
	public void Enter()
	{
		// 以下逻辑：
		// - 把末尾 100 粒子染成绿色
		// - 作为“Boss 处于 Idle 状态”的可视化提示
		//
		// 此处的 Colors.Lime 是亮绿色
		// 主要是用于测试 State Pattern 是否正常运作
		for (int i = ParticleCloud.PointCount - 100; i < ParticleCloud.PointCount; i++)
		{
			cloud.SetColor(i, Colors.Lime);
		}

		// 在 Godot 控制台打印日志
		GD.Print("Enter Idle");
	}

	// 当 Boss 离开 IdleState 时会调用此方法
	public void Exit()
	{
		// 在 Godot 控制台打印日志
		GD.Print("Exit Idle");
	}

	// 每帧调用，用于更新 IdleState 的逻辑
	public void Tick(double delta)
	{
		// 调用移动策略的 Advance 方法
		// 让 Boss 在场景里移动（即使是 idle，也可能漂移或悬停）
		move.Advance(delta);

		// 保证 cloud 的 MultiMesh 位置和角度跟随 cloud 的 Position / Rotation
		//
		// 意义：
		//   - cloud.Position：粒子云的全局位置
		//   - cloud.Rotation：粒子云的角度
		//
		// ApplyTransform() 会把这些数值应用到 MultiMeshInstance2D
		cloud.ApplyTransform(cloud.Position, cloud.Rotation);

		// 更新 cloud 内部的粒子动画
		// 举例：
		//   - 粒子位置震动
		//   - 粒子颜色渐变
		//   - 粒子旋转
		cloud.Advance(delta);
	}
}
