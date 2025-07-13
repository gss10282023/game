//using Godot;
//using System;
//
//public partial class GameRoot : Node2D
//{
	///* —— 运行参数 —— */
	//const double RotateSpeedRad = 50 * Math.PI / 180.0;
	//const double MoveSpeed      = 60;
	//const double PosEps         = 0.5;
	//const double KeyMoveSpeed   = 100;
//
	///* —— 玩家方块 —— */
	//Rect2 dragRect = new(new Vector2(20, 20), new Vector2(24, 24));
//
	///* —— 组件 —— */
	//ParticleCloud   cloud;
	//IMoveStrategy   moveStrategy;
	//BossStateMachine fsm;
//
	///* ------------- Ready ------------- */
	//public override void _Ready()
	//{
		///* 1. 粒子云 */
		//cloud = new ParticleCloud();
		//AddChild(cloud);
		//cloud.Initialize(GetNode<MultiMeshInstance2D>("Cloud"));
//
		///* 2. MoveStrategy（追踪方块）*/
		//moveStrategy = new ChaseSquareStrategy();
		//moveStrategy.Initialize(
			//host: cloud,
			//target: () => dragRect.Position + dragRect.Size / 2,
			//moveSpeed: MoveSpeed,
			//rotateSpeedRad: RotateSpeedRad,
			//posEps: PosEps);
//
		///* 3. Boss StateMachine */
		//fsm = new BossStateMachine();
		//AddChild(fsm);
//
		//var idle = new IdleState(cloud, moveStrategy);   // 目前只有 IdleState
		//fsm.Register("Idle", idle);
		//fsm.Change("Idle");                              // 设为初始状态
	//}
//
	///* ------------- 每帧逻辑 ------------- */
	//public override void _Process(double delta)
	//{
		///* A. 玩家方块移动 */
		//Vector2 d = Vector2.Zero;
		//if (Input.IsActionPressed("move_left"))  d.X -= (float)(KeyMoveSpeed * delta);
		//if (Input.IsActionPressed("move_right")) d.X += (float)(KeyMoveSpeed * delta);
		//if (Input.IsActionPressed("move_up"))    d.Y -= (float)(KeyMoveSpeed * delta);
		//if (Input.IsActionPressed("move_down"))  d.Y += (float)(KeyMoveSpeed * delta);
		//dragRect.Position += d;
//
		///* B. 其余逻辑（MoveStrategy + 粒子更新）已在 IdleState 内部处理 */
		///*    这里只需负责玩家输入和绘制刷新 */
		//QueueRedraw();
	//}
//
	///* ------------- _Draw：红框 / 象限 / 方块 ------------- */
	//public override void _Draw()
	//{
		///* 背景 */
		//DrawRect(GetViewport().GetVisibleRect(), new Color(0.04f, 0.04f, 0.04f));
//
		///* 方块 */
		//DrawRect(dragRect, Colors.Cyan,  true);
		//DrawRect(dragRect, Colors.White, false, 1);
//
		///* 变换到云团局部 */
		//DrawSetTransformMatrix(
			//Transform2D.Identity
					   //.Translated(-cloud.Position)
					   //.Rotated(cloud.Rotation)
					   //.Translated(cloud.Position));
//
		///* —— 世界坐标包围盒 —— */
		//Rect2 rLocal = cloud.GetBBoxLocal99().Grow(0.5f);
		//Rect2 r      = new Rect2(rLocal.Position + cloud.Position, rLocal.Size);
//
		//float cx = r.Position.X + r.Size.X * 0.5f;
		//float cy = r.Position.Y + r.Size.Y * 0.5f;
//
		//Color qRU = new Color(1, 0, 0, 0.15f);
		//Color qLU = new Color(0, 1, 0, 0.15f);
		//Color qLL = new Color(0, 0, 1, 0.15f);
		//Color qRL = new Color(1, 1, 0, 0.15f);
//
		//DrawRect(new Rect2(cx, r.Position.Y,        r.Size.X/2, r.Size.Y/2), qRU);
		//DrawRect(new Rect2(r.Position.X, r.Position.Y, r.Size.X/2, r.Size.Y/2), qLU);
		//DrawRect(new Rect2(r.Position.X, cy,          r.Size.X/2, r.Size.Y/2), qLL);
		//DrawRect(new Rect2(cx,          cy,          r.Size.X/2, r.Size.Y/2), qRL);
		//DrawRect(r, Colors.Red, false, 1);
//
		///* 字体工具 */
		//Font font = ThemeDB.FallbackFont;
		//int  fsz  = 14;
		//static Color Solid(Color c) => new Color(c.R, c.G, c.B, 1f);
//
		//DrawString(font, new Vector2(cx + r.Size.X/4,  r.Position.Y + r.Size.Y/4),
				   //"I",  HorizontalAlignment.Center, -1, fsz, Solid(qRU));
		//DrawString(font, new Vector2(r.Position.X + r.Size.X/4, r.Position.Y + r.Size.Y/4),
				   //"II", HorizontalAlignment.Center, -1, fsz, Solid(qLU));
		//DrawString(font, new Vector2(r.Position.X + r.Size.X/4, cy + r.Size.Y/4),
				   //"III",HorizontalAlignment.Center, -1, fsz, Solid(qLL));
		//DrawString(font, new Vector2(cx + r.Size.X/4,  cy + r.Size.Y/4),
				   //"IV", HorizontalAlignment.Center, -1, fsz, Solid(qRL));
//
		//DrawSetTransformMatrix(Transform2D.Identity);
	//}
//}
