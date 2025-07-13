using Godot;        // 引入 Godot 引擎命名空间
using System;       // 引入 System 命名空间，用于 Math 等工具
using Boss.PointFormula;   // 🆕 引入粒子公式接口/实现

// 定义 BossBase 类
// 继承 Node2D
//
// 这是 Boss 的场景脚本，作为 Boss 的总体逻辑入口
// 主要负责：
// - 初始化粒子云
// - 初始化移动策略
// - 初始化状态机
// - 处理玩家方块的示例移动
// - Debug 绘制
public partial class BossBase : Node2D
{
	/* —— 调参 —— */

	// Boss 的移动速度（像素/秒）
	[Export]
	public double MoveSpeed = 60;

	// Boss 的旋转速度（弧度/秒）
	// 50° × (π/180) → 转成弧度
	[Export]
	public double RotateSpeedRad = 50 * Math.PI / 180.0;

	// Boss 到目标位置的容许误差
	[Export]
	public double PosEps = 0.5;

	/* —— 粒子公式选择 —— */

	// 让关卡设计师在 Inspector 里选公式
	[Export] public string FormulaId = "default";   // 🆕

	/* —— 组件 —— */

	ParticleCloud cloud;
	// 管理粒子云对象
	// 会在 _Ready() 中创建并添加到场景

	IMoveStrategy move;
	// 移动策略
	// 这里使用 ChaseSquareStrategy，让 Boss 追逐玩家方块

	BossStateMachine fsm;
	// 状态机
	// 管理 Boss 所处的不同状态

	/* —— 玩家方块（示例用）—— */

	// 用一个矩形代替“玩家”
	// 后续可替换为真实玩家节点
	Rect2 dragRect = new(new Vector2(20, 20), new Vector2(24, 24));

	// 玩家方块的移动速度
	const double KeyMoveSpeed = 100;

	/* ------------- _Ready ------------- */

	public override void _Ready()
	{
		/* 1. 将场景内的 MultiMeshInstance2D 设为粒子云 */

		// 创建 ParticleCloud
		cloud = new ParticleCloud();

		// 将 ParticleCloud 加到场景里
		AddChild(cloud);

		// 初始化 ParticleCloud：
		// 传入场景里的 MultiMeshInstance2D 节点
		// (例如场景里放了一个名叫 "Cloud" 的 MultiMeshInstance2D)
		cloud.Initialize(GetNode<MultiMeshInstance2D>("Cloud"));

		// 🆕 1.1 根据 FormulaId 选择并注入粒子公式
		// 目前只有 DefaultFormula；后续可替换为工厂/字典
		IPointFormula formula = FormulaId switch
		{
			// 新公式在此继续补充
			"default" or _ => new DefaultFormula()
		};
		cloud.SetFormula(formula);

		/* 2. MoveStrategy：追踪方块 */

		// 创建追踪策略
		move = new ChaseSquareStrategy();

		// 初始化移动策略
		// host   → 粒子云 cloud
		// target → 玩家方块的中心
		move.Initialize(
			host: cloud,
			target: () => dragRect.Position + dragRect.Size / 2,
			moveSpeed: MoveSpeed,
			rotateSpeedRad: RotateSpeedRad,
			posEps: PosEps
		);

		/* 3. 状态机 */

		// 创建 Boss 状态机
		fsm = new BossStateMachine();

		// 加入场景树
		AddChild(fsm);

		// 注册 Idle 状态
		fsm.Register("Idle", new IdleState(cloud, move));

		// 切换到 Idle 状态作为初始状态
		fsm.Change("Idle");

		/* 4. 输入映射（确保项目里有）*/

		// 若 Godot 项目设置里没有这些动作，要先配置：
		//   move_left
		//   move_right
		//   move_up
		//   move_down
		// 否则输入检测不到
	}

	/* ------------- _Process ------------- */

	public override void _Process(double delta)
	{
		/* 玩家方块示例移动
		 *
		 * 此处仅是临时示范
		 * 后续可以换成真正的 Player 节点或 AI 目标
		 */

		Vector2 d = Vector2.Zero;

		// 根据输入方向累积位移
		if (Input.IsActionPressed("move_left"))
			d.X -= (float)(KeyMoveSpeed * delta);

		if (Input.IsActionPressed("move_right"))
			d.X += (float)(KeyMoveSpeed * delta);

		if (Input.IsActionPressed("move_up"))
			d.Y -= (float)(KeyMoveSpeed * delta);

		if (Input.IsActionPressed("move_down"))
			d.Y += (float)(KeyMoveSpeed * delta);

		// 更新玩家方块的位置
		dragRect.Position += d;

		/* 其他逻辑已由状态机驱动 */
		QueueRedraw();    // 请求重绘，用于触发 _Draw
	}

	/* ------------- _Draw：调试 UI ------------- */

	public override void _Draw()
	{
		
		
		
		// 以下代码仅在 DEBUG 模式下绘制
#if DEBUG

		/* 背景 */
		// 绘制整张画布深灰色
		DrawRect(GetViewport().GetVisibleRect(), new Color(0.05f, 0.05f, 0.05f));

		/* 玩家方块 */
		// 填充绘制 cyan（青色）
		DrawRect(dragRect, Colors.Cyan, true);

		// 白色描边
		DrawRect(dragRect, Colors.White, false, 1);

		/* 云团象限 & 包围盒 */

		//// 将绘制坐标系转换到云团位置
		//DrawSetTransformMatrix(
			//Transform2D.Identity
				//.Translated(-cloud.Position)
				//.Rotated(cloud.Rotation)
				//.Translated(cloud.Position)
		//);
//
		//// 从 cloud 获取云团局部 99% 包围盒
		//// 并稍微放大 0.5 像素用于视觉美观
		//Rect2 rLocal = cloud.GetBBoxLocal99().Grow(0.5f);
//
		//// 转换到全局坐标
		//Rect2 r = new Rect2(
			//rLocal.Position + cloud.Position,
			//rLocal.Size
		//);
//
		//// 计算云团中心点
		//float cx = r.Position.X + r.Size.X * 0.5f;
		//float cy = r.Position.Y + r.Size.Y * 0.5f;
//
		//// 定义 4 个象限的半透明颜色
		//Color qRU = new Color(1, 0, 0, 0.15f);  // 红色（右上）
		//Color qLU = new Color(0, 1, 0, 0.15f);  // 绿色（左上）
		//Color qLL = new Color(0, 0, 1, 0.15f);  // 蓝色（左下）
		//Color qRL = new Color(1, 1, 0, 0.15f);  // 黄色（右下）
//
		//// 绘制 4 个象限矩形
		//DrawRect(new Rect2(cx, r.Position.Y, r.Size.X / 2, r.Size.Y / 2), qRU);
		//DrawRect(new Rect2(r.Position.X, r.Position.Y, r.Size.X / 2, r.Size.Y / 2), qLU);
		//DrawRect(new Rect2(r.Position.X, cy, r.Size.X / 2, r.Size.Y / 2), qLL);
		//DrawRect(new Rect2(cx, cy, r.Size.X / 2, r.Size.Y / 2), qRL);
//
		//// 绘制红色包围盒边框
		//DrawRect(r, Colors.Red, false, 1);
//
		//// 恢复绘制矩阵
		//DrawSetTransformMatrix(Transform2D.Identity);
#endif
	}
}
