## 项目总览

这是一个基于 Godot 4 + C# 的粒子成长与多阶段 Boss 战游戏框架。核心思路是通过组件化和模式化（策略模式、状态机模式），实现高复用、易扩展的 Boss 系统。

---

## 目录结构

```
res://
├─ script/
│  ├─ Boss/
│  │  ├─ MoveStrategy/
│  │  │  ├─ ChaseSquareStrategy.cs      // 追踪方块策略实现
│  │  │  └─ IMoveStrategy.cs            // 移动策略接口，定义了移动策略的统一接口方法，用于初始化移动相关参数和逐帧更新移动行为，保证策略实现的一致性与可扩展性
│  │  ├─ StateMachine/
│  │  │  ├─ BossStateMachine.cs         // 状态机驱动器
│  │  │  ├─ IBossState.cs               // Boss 状态接口，定义状态切换的统一接口方法，用于进入状态、退出状态和逐帧更新状态逻辑
│  │  │  └─ IdleState.cs                // 初始状态实现，演示基本状态行为
│  │  ├─ PointFormula/                  // 🆕 粒子坐标公式策略
│  │  │  ├─ IPointFormula.cs            // 粒子坐标公式接口
│  │  │  └─ DefaultFormula.cs           // 旧版默认粒子公式实现
│  │  ├─ BossBase.cs                    // 聚合模块的 Boss 基类
│  │  └─ ParticleCloud.cs               // 粒子云生成器模块
│  └─ Scenes/
│     └─ BossBase.tscn                  // BossBase 场景
├─ GameRoot.cs                          // 旧版入口脚本（可删除）
├─ game_root.tscn                       // 旧入口场景（可弃用）
├─ icon.svg                             // 项目图标
├─ project.godot                        // Godot 主配置
└─ YourGame.csproj                      // C# 编译配置
```

---

## 设计模式介绍

* **策略模式（Strategy Pattern）**：将“移动行为”和“粒子坐标计算”抽象为接口，具体算法（如 `ChaseSquareStrategy`、`DefaultFormula` 等）可以在运行时自由替换，增加扩展性。
* **状态机模式（State Machine Pattern）**：通过 `IBossState` 抽象各个作战阶段（Idle、Enraged、Death…），`BossStateMachine` 管理状态注册、切换与逐帧驱动，轻松实现多阶段 Boss 形态与行为。
* **组件化设计（Component-Based Design）**：将粒子云、移动 AI、状态机分别封装成独立 C# 脚本，使逻辑职责单一、模块内聚；通过在 `BossBase` 中组合各组件，简化场景搭建与复用。

---

## 文件介绍

### 1. ParticleCloud.cs

* **路径**：`script/Boss/ParticleCloud.cs`
* **职责**：封装粒子云的生成与更新逻辑；提供 API：

  * `Initialize(MultiMeshInstance2D inst)`：绑定场景 MultiMeshInstance2D 并初始化颜色 / 数量
  * `Advance(double delta)`：每帧推进内部时间并刷新所有粒子
  * `ApplyTransform(Vector2 pos, double rot)`：同步粒子云位置与朝向
  * `GetBBoxLocal99()`：获取用于调试绘制的粒子包围盒
  * `SetColor(int idx, Color c)`：动态修改单粒子颜色
  * `SetFormula(IPointFormula f)`：🆕 注入新的粒子公式策略
* **实现细节**：内部不再硬编码粒子坐标公式，默认使用 `DefaultFormula`，可随时更换其他公式策略。

### 2. PointFormula/ 🆕

* **路径**：`script/Boss/PointFormula/`
* **职责**：粒子坐标计算策略，便于实现不同 Boss 粒子效果。
* **文件说明**：

  * `IPointFormula.cs`：粒子坐标公式接口，定义统一的坐标计算方法。
  * `DefaultFormula.cs`：旧版三角函数云团公式，实现了 `IPointFormula`。

### 3. MoveStrategy/

#### IMoveStrategy.cs

* **职责**：定义移动策略接口

  ```csharp
  void Initialize(Node2D host, Func<Vector2> target, double moveSpeed, double rotateSpeedRad, double posEps);
  void Advance(double delta);
  ```

#### ChaseSquareStrategy.cs

* **职责**：追踪方块的具体策略，实现 `IMoveStrategy`
* **实现细节**：计算宿主与目标的向量距离、理想角度，控制旋转与位移。

### 4. StateMachine/

#### IBossState.cs

* **职责**：定义 Boss 状态接口

  ```csharp
  void Enter();
  void Exit();
  void Tick(double delta);
  ```

#### BossStateMachine.cs

* **职责**：管理状态注册、切换与逐帧调用；支持通过 `Change(string name)` 切换；在 `_UnhandledInput` 中监听 `Tab` 热键循环状态

#### IdleState.cs

* **职责**：初始状态，组合 `ParticleCloud` 与 `IMoveStrategy`：

  * `Enter`：示例性修改末尾 100 粒子颜色
  * `Tick`：调用 `move.Advance` + `cloud.ApplyTransform` + `cloud.Advance`

### 5. BossBase.cs

* **路径**：`script/Boss/BossBase.cs`
* **职责**：作为 Boss 模块的统一入口，聚合粒子云、移动策略、状态机以及示范用的玩家方块
* **调参**：`[Export]` 属性：`MoveSpeed`、`RotateSpeedRad`、`PosEps`
* **调试宏**：`#if DEBUG` 包裹象限/包围盒绘制
* **方法**：在 `_Ready` 中初始化所有组件；在 `_Process` 中处理玩家输入与刷新；在 `_Draw` 中绘制调试 UI。

---

## 场景与 Boss 扩展指南

### A. 根据现有代码生成新场景

1. 复制现有的 `BossBase.tscn`，重命名为新的场景（如 `BossSpiral.tscn`）。
2. 根节点挂载 `BossBase.cs`，在 Inspector 中将 `Formula Id` 修改为 "spiral"；或者在 `_Ready` 中调用 `cloud.SetFormula(new SpiralFormula())`。

### B. 生成新的 Boss

创建新 Boss 继承 `BossBase`，并覆盖 `_Ready` 方法以自定义粒子公式。

```csharp
public partial class Boss2 : BossBase
{
    public override void _Ready()
    {
        base._Ready();
        cloud.SetFormula(new MyCoolFormula());   // 自定义粒子弹幕
    }
}
```

然后新建场景 `Boss2.tscn` 并挂载脚本。

### C. 更改移动策略

实现新的移动策略类并实现 `IMoveStrategy` 接口，然后在 `_Ready` 中替换实例。

### D. 更改 Boss 状态

在 `StateMachine` 目录实现 `IBossState` 接口、注册状态并切换即可。
