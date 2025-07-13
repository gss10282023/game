using Godot;                         // 引入 Godot 命名空间，提供 Node 等类型
using System.Collections.Generic;    // 引入字典、集合等类型

// BossStateMachine 类
// 继承 Node
//
// 用于管理 Boss 的状态机，负责：
// - 注册所有状态
// - 切换状态
// - 驱动当前状态 Tick
// - 提供调试热键方便测试
public partial class BossStateMachine : Node
{
	IBossState _current;
	// 当前正在运行的状态对象
	// 在每帧里被调用 Tick(delta)

	readonly Dictionary<string, IBossState> _states = new();
	// 用于存放所有已注册的状态
	// key   = 状态名称（字符串）
	// value = 状态对象（IBossState 实现类）

	string _currentTag;
	// 用于记录当前状态的名字
	// 方便调试以及热键切换

	/// <summary>
	/// 注册所有状态
	/// 建议在 Godot 的 _Ready() 里调用。
	/// 
	/// 例如：
	/// 
	///   stateMachine.Register("Idle", new IdleState(...));
	///   stateMachine.Register("Attack", new AttackState(...));
	/// 
	/// </summary>
	public void Register(string name, IBossState state)
	{
		// 将状态存入字典
		_states[name] = state;
	}

	/// <summary>
	/// 切换状态到 name
	/// 
	/// 做了三件事：
	/// 1. 调用旧状态的 Exit()
	/// 2. 切换 _current 指向新状态
	/// 3. 调用新状态的 Enter()
	/// 
	/// 并打印日志提示切换结果
	/// </summary>
	public void Change(string name)
	{
		// 如果字典里没有该状态，什么都不做
		if (!_states.TryGetValue(name, out var next))
			return;

		// 调用当前状态的 Exit
		_current?.Exit();

		// 切换 _current
		_current = next;

		// 调用新状态的 Enter
		_current.Enter();

		// ★ 关键：同步当前状态名称
		_currentTag = name;

		// 在控制台打印彩色日志
		GD.PrintRich($"[color=yellow]>> State → {name}[/color]");
	}

	/// <summary>
	/// Godot 的 _Process，每帧都会调用
	/// 
	/// 用于驱动当前状态的 Tick(delta)
	/// </summary>
	public override void _Process(double delta)
	{
		// 如果当前状态不为空，就调用它的 Tick
		_current?.Tick(delta);
	}

	/* ----------- Debug 热键：Tab 切到下一个 ----------- */
	// 此方法用于测试 State Pattern 是否运作正常
	// 按下 Tab 键即可轮流切换到下一个已注册状态
	public override void _UnhandledInput(InputEvent e)
	{
		// 如果玩家按下 Tab
		if (e.IsActionPressed("ui_tab"))
		{
			// 获取字典所有 key 的迭代器
			var iter = _states.Keys.GetEnumerator();

			// 遍历所有状态名
			while (iter.MoveNext())
			{
				// 如果当前遍历到的状态名 == 当前状态
				if (iter.Current == _currentTag)
				{
					// 如果还有下一个，就切到下一个
					// 否则回到第一个
					_currentTag = iter.MoveNext()
						? iter.Current
						: System.Linq.Enumerable.First(_states.Keys);

					// 执行切换
					Change(_currentTag);
					break;
				}
			}
		}
	}
}
