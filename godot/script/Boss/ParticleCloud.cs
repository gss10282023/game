using Godot;     // 引入 Godot 命名空间，提供 Node2D、Vector2、Color 等类
using System;    // 引入 System 命名空间，提供 Math、Array 等工具
using Boss.PointFormula;   // 🆕 引入粒子公式接口 & 默认实现

// 定义 ParticleCloud 类
// 继承 Node2D
// 用于渲染、更新并管理一大团粒子
public partial class ParticleCloud : Node2D
{
	/* —— 常量 —— */

	// 表示粒子数量
	// 整个云团里一共有多少个粒子
	public const int PointCount = 10_000;

	// 内部用于计算动画推进的增量
	// 每次 Advance 时，t += InternalAdvance
	// 控制动画节奏
	const double InternalAdvance = Math.PI / 240;

	/* —— 状态 —— */

	double t;
	// 内部用于动画公式的累积时间

	int frameCount;
	// 用来记录帧数（递增）
	// 可用于在粒子公式里制造帧依赖的动态效果

	Rect2 bbox99;
	// 存储 1%–99% 的局部坐标范围，用于可视化调试
	// 例如绘制 Bounding Box 之类

	/* —— MultiMesh —— */

	MultiMeshInstance2D cloudInst;
	// 用于实际在场景里显示粒子云的节点
	// 是 MultiMesh 的实例化容器

	MultiMesh cloudMesh;
	// 存放真正的实例化数据：
	// - 每个粒子的 Transform
	// - 每个粒子的颜色
	// - 等

	/* —— 粒子公式策略 —— */
	
	/// <summary>
	/// 当前使用的粒子坐标公式，实现 IPointFormula
	/// 缺省为 DefaultFormula（旧版公式）
	/// </summary>
	IPointFormula _formula = new DefaultFormula();   // 🆕 默认策略

	/// <summary>
	/// 让外部（Boss / 状态机）随时替换粒子公式
	/// </summary>
	public void SetFormula(IPointFormula formula)    // 🆕 注入接口
	{
		_formula = formula ?? throw new ArgumentNullException(nameof(formula));
	}

	/* ---------- 对外接口 ---------- */

	// 初始化方法
	// 用于设置 cloudInst、cloudMesh，并进行基本配置
	//
	// 初始化顺序非常重要：
	//   ① 先设 Format
	//   ② 再设 InstanceCount
	//   ③ 确保有 Mesh
	//   ④ 最后再改颜色
	public void Initialize(MultiMeshInstance2D instance)
	{
		cloudInst = instance;
		cloudMesh = cloudInst.Multimesh;

		/* 1️⃣ 先开格式，再设数量！ */

		// 设置 MultiMesh 的 Transform 格式为 2D
		// 表示我们只需要 2D 平移、缩放、旋转矩阵
		cloudMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform2D;

		// 开启 MultiMesh 支持单独给每个实例设置颜色
		cloudMesh.UseColors = true;

		// 如果想节省显存：
		// cloudMesh.ColorFormat = MultiMesh.ColorFormatEnum.Color8bit;
		// 不过精度会下降

		/* 2️⃣ 现在才设实例数 */

		// 必须在设定格式后，才设置实例数量
		cloudMesh.InstanceCount = PointCount;

		/* 3️⃣ 确保有 Mesh */

		// 如果 MultiMesh 尚未绑定 Mesh，则自动创建一个 QuadMesh
		// QuadMesh = 方形网格
		if (cloudMesh.Mesh == null)
			cloudMesh.Mesh = new QuadMesh
			{
				Size = Vector2.One  // 默认单元大小为 (1,1)
			};

		/* 4️⃣ 安心设置颜色 */

		// 将每个粒子的初始颜色设置好
		// - 大部分粒子 → 白色
		// - 最后 100 个粒子 → 红色
		// 为了让状态机可以用不同颜色做视觉提示
		for (int i = 0; i < PointCount; i++)
			cloudMesh.SetInstanceColor(i,
				i >= PointCount - 100
					? Colors.Red
					: Colors.White);
	}

	/// <summary>
	/// 每帧调用，推动内部时间 t，累加帧数
	/// 然后更新所有粒子的位置
	/// </summary>
	public void Advance(double delta)
	{
		// 累加 t
		t += InternalAdvance;

		// 累加帧数
		frameCount += 1;

		// 重算每个粒子的坐标并更新到 MultiMesh
		UpdateCloudParticles();
	}

	/// <summary>
	/// 把 cloudInst 的场景位置和角度
	/// 应用到 MultiMeshInstance2D
	/// 
	/// 意义：
	/// - cloudInst.Position → 决定 MultiMesh 实例整体位置
	/// - cloudInst.Rotation → 决定整体朝向
	/// 
	/// 如果不调用这一步，
	/// MultiMesh 仍然在原地不动
	/// </summary>
	public void ApplyTransform(Vector2 pos, double rot)
	{
		cloudInst.Position = pos;
		cloudInst.Rotation = (float)rot;
	}

	/// <summary>
	/// 供外部访问 bbox99，用于调试绘制等场景
	/// </summary>
	public Rect2 GetBBoxLocal99() => bbox99;

	/* ---------- 私有实现 ---------- */

	/// <summary>
	/// 修改单个粒子的颜色
	/// 可用于：
	/// - 实现状态机的视觉效果（闪烁、变色）
	/// - 高亮选中的粒子
	/// </summary>
	public void SetColor(int index, Color color)
	{
		// 检查 index 是否越界
		if (index < 0 || index >= PointCount) return;

		cloudMesh.SetInstanceColor(index, color);
	}

	/// <summary>
	/// 更新粒子的位置数据
	/// 
	/// 实现步骤：
	/// 1. 根据公式算出每个粒子的位置
	/// 2. 计算局部质心
	/// 3. 把质心抵消掉
	/// 4. 写回到 MultiMesh 的 Transform2D
	/// </summary>
	void UpdateCloudParticles()
	{
		float[] xs = new float[PointCount];
		float[] ys = new float[PointCount];
		Vector2[] local = new Vector2[PointCount];

		// 计算每个粒子的位置
		for (int i = 0; i < PointCount; i++)
		{
			// 调用当前策略 MapPoint 获得粒子 (px, py) 坐标
			(double px, double py) =              // 🛠 改用 _formula
				_formula.MapPoint(i, i / 235.0, t, frameCount);

			// 转换为 Godot 的 Vector2
			Vector2 v = new((float)px, (float)py);
			local[i] = v;
			xs[i] = v.X;
			ys[i] = v.Y;
		}

		// 统计 1% ~ 99% 区间的 x/y 极值，用于去除离群点
		Array.Sort(xs);
		Array.Sort(ys);
		int lo = (int)(PointCount * 0.01);
		int hi = (int)(PointCount * 0.99);

		float minX = xs[lo], maxX = xs[hi];
		float minY = ys[lo], maxY = ys[hi];

		// 计算 99% 区间的质心
		Vector2 localCenter = new(
			(minX + maxX) * 0.5f,
			(minY + maxY) * 0.5f
		);

		// 将每个粒子的局部坐标减去质心
		// 保证整个云团围绕 (0,0)
		for (int i = 0; i < PointCount; i++)
		{
			cloudMesh.SetInstanceTransform2D(
				i,
				new Transform2D(0, local[i] - localCenter)
			);
		}

		// 保存包围盒信息
		bbox99 = new Rect2(
			minX - localCenter.X,
			minY - localCenter.Y,
			maxX - minX,
			maxY - minY
		);
	}


}
