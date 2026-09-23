# 怪物系统实现规格（Monster System Spec · V1.0）

> 与 `docs/GDD.md` 对应。本文档描述**代码级**实现：数据、数值、状态机、事件、UI 与音频，便于维护与扩展。

---

## 1. 数据层

### 1.1 类型枚举

`Helpers/Enum/MonsterType.cs`

```csharp
public enum MonsterType { Normal, FixedDamage, DrainBlood, Boss }
```

### 1.2 怪物模型

`Models/Monster.cs`：新增 `Type`（`MonsterType`，默认 `Normal`）与 `IsBossTag` 辅助属性。`Type == Boss` ⇔ `IsBossTag`。

### 1.3 数据标注 `Data/Monsters.json`

41 个模板全部带 `Type` 字段（默认 `Normal`）：

| 类型 | 模板 Id（自定义名） |
|---|---|
| `FixedDamage` | 14 巫妖 · 18 闪电法师 · 24 水晶灵体 · 25 邪眼魔 · 27 暗影魔王 |
| `DrainBlood` | 11 暗影刺客 · 22 地狱犬 |
| `Boss` | 41 魔王·加坦杰厄（第 100 层） |
| `Normal` | 其余全部 |

> 注：`Type == FixedDamage` 与旧字段 `IgnoreDefense`（6 个模板）是**两套维度**。战斗中判定"魔法怪"取并集：`IsFixed = Type == FixedDamage || IgnoreDefense`。两者重合的模板在手册中仍标注「魔法·无视防御」。

## 2. 数值层 `Helpers/MathHelper.cs`

| 方法 | 说明 |
|---|---|
| `PlayerDamagePerTurn(m, p)` | `max(1, 玩家攻击 − 怪物防御)` |
| `MonsterDamagePerTurn(m, p)` | 普通/吸血：`max(1, 怪物攻击 − 玩家防御)`；魔法怪：`max(1, 怪物攻击)` |
| `EstimatedPlayerDamage(m, p)` | 击杀回合数 − 1，再 × 每回合承伤（最后一回合不反击） |
| `CanBreakDefense(m, p)` | `玩家攻击 > 怪物防御`（破防门槛） |
| `CanKill(m, p)` | 预估损失 < 当前生命 |
| `SimulateBattle(m, p)` | 完整预演 → `BattleSimulation{ IsVictory, PlayerDamageTaken, MonsterHealed, Turns }` |
| `IsBoss(m)` | `m.Type == MonsterType.Boss` |

吸血预演：按回合推进累计回复量（上限 `怪物初始生命`），并在 `PlayerDamageTaken` 中扣除吸血导致的回合数增加。

## 3. 战斗状态机 `Services/GameEngine.cs`

### 3.1 状态对象 `ActiveBattleState`

```csharp
Monster Monster            // 对手模板
BattleSimulation Preview   // 开战前预演战果（UI 展示）
int MonsterHp / PlayerHp   // 逐回合实时 HP
int PlayerDamagePerHit / MonsterDamagePerHit
int Turn                   // 已推进回合数
bool CanBreakDefense       // 破防判定（禁战依据）
bool IsBoss / IsFixed / IsDrain
bool Ended / Won / Retreated
List<BattleStep> Steps     // 回合战报（最新在后）
string IntroText           // 开场白（BOSS 附加禁撤退/红钥匙警告）
```

`BattleStep { Turn, PlayerHit, MonsterHit, Drained, Text }` 为单回合战报。

### 3.2 驱动方法

| 方法 | 触发者 | 行为 |
|---|---|---|
| `Fight(x,y)`（TryMove 内部） | 走上怪物格 | 构造 `ActiveBattleState`，`StateChanged` 通知 UI 开面板；**不移动** |
| `ConfirmBattle()` | 玩家点「战斗」 | 破防失败 ⇒ 提示 + `CancelBattle()`；成功 ⇒ `BattleStarted` 事件 |
| `StepBattle()` → bool | 面板 450ms 计时器 | 玩家先手 → 反击 → 吸血；记录战报；触发 `BattleStepAdvanced`；返回是否结束 |
| `RetreatBattle()` | 玩家点「撤退」 | 非 BOSS ⇒ 标记 `Retreated/Ended` → `FinishBattle()` |
| `CancelBattle()` | 破防失败 | 清空 `ActiveBattle` → `BattleEnded` |
| `FinishBattle()` | 结果面板「继续」 | 胜利：金币 + 击杀计数 + 怪格变地板 + 玩家移动；BOSS 额外红钥匙 ×1（100F ⇒ 通关）；失败：`Hp=0, IsDead`；撤退：无奖励 |

### 3.3 事件

| 事件 | 时机 | 消费方 |
|---|---|---|
| `StateChanged` | 一切状态变更 | UI 全局刷新 |
| `BattleStarted` | 确认开战 | 切入战斗 BGM + 命中音效 |
| `BattleStepAdvanced` | 每推进一回合 | （UI 直接调用 `StepBattle` 后自刷，事件备用） |
| `BattleFinished` | 结算完成 | 预留（击杀/阵亡统计钩子） |
| `BattleEnded` | 胜利/失败/撤退/取消 | 关闭战斗面板 / 切回 BGM |
| `DoorOpened` | 成功开门 | 开门音效 |
| `ItemPickedUp` | 成功拾取 | 拾取音效 |

### 3.4 时序（胜利路径）

```
Fight → 面板弹出 → 点「战斗」→ ConfirmBattle（BattleStarted: BGM+命中）
  → StepBattle ×N（450ms/回合，战报+HP 实时刷新）
  → 结束 → HandleBattleEnd（胜利音效+结果面板，锁定"继续"）
  → 点「继续」→ FinishBattle（结算奖励/移动/StateChanged/BattleEnded）→ 关面板
```

## 4. UI 层 `MainWindow.xaml(.cs)`

- `BattleOverlay` 半透明全屏弹窗：怪物卡 / ⚔ / 勇士卡三栏 + 警告行 + 战报日志 + 操作区。
- 警告行三种状态：**无法破防**（红色，战斗按钮禁用）/ **必死预警**（红色，仍可搏命）/ **预估战果**（绿色基调提示）。
- 战斗中锁定：方向键移动、`F5/F9/M/B/Esc` 全部忽略，直到结算完成。
- 结算后死亡/通关沿用既有 `MessageBox + Restart` 流程。

## 5. 音频层

`Services/AudioManager.cs` 新增 `PlayStep / PlayDoor / PlayVictory / PlayDeath`（每次播放 `Open+Play` 从零开始，不打断 BGM）；`Assets/Audio/generate_audio.ps1` 内新增 `BuildStep/BuildDoor/BuildVictory/BuildDeath` 合成函数，产物：

```
step.wav(0.09s) door_open.wav(0.30s) victory.wav(0.62s) player_death.wav(0.78s)
```

## 6. 验证清单

- [x] `dotnet build`（Debug / Release）0 错误 0 警告
- [x] 41 模板 `Type` 标注校验（脚本读取 JSON 断言）
- [x] 音效产物存在且时长符合设计
- [x] 发布产物 smoke test（`dotnet publish -r win-x64 --self-contained true`）
