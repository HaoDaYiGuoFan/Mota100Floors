# MT-TASK-ITEM-FlyOrb · 飞行器（楼层穿梭机）

> 状态：**已完成** ｜ 编号：MT-ITEM-007 ｜ 类型：永久道具 + 楼层传送系统
> 涉及版本：存档 v2（v1 旧档自动兼容）

---

## 1. 功能需求

1F 左下暗角放置永久道具「飞行器（楼层穿梭机）」；拾取后加入背包（`I` 键），在**楼梯格上**使用时打开穿梭面板，可传送到任何**已经到达过**的楼层（未探索楼层置灰不可选）。传送保留目标层已消耗的怪物 / 物品 / 门状态。整塔共 **100 层**（以 `MapConstants.TotalFloors` 为准，任务文档提及的 1~50 与本作 100F BOSS 设定不符，采用后者）。

## 2. 数据模型

| 文件 | 改动 |
|---|---|
| `Helpers/Enum/ItemType.cs` | 新增 `FlyOrb` |
| `Models/Player.cs` | 新增 `IsHasFlyOrb`（是否持有）与 `VisitedFloors: List<int>`（已到达楼层集合，null 兜底为 `new()`） |
| `Models/GameSaveData.cs` | 存档版本升至 **v2**，新增两字段；v1 旧档加载时 null 合并兜底，当前楼层会在下一次 `EnterFloor` 时补记 |

## 3. 引擎逻辑（`Services/GameEngine.cs`）

- `EntryMode.FlyOrb`：进入目标层后优先落在 **StairDown**，其次 **StairUp**，都没有才回退出生点（隐藏层 0F / 44F 同样适用）。
- `EnterFloor`：每次进入楼层把 `floorNumber` 记入 `VisitedFloors`（新游戏 / 读旧档首次进入时补记）。
- `TryUseFlyOrb(out string reason)`：要求①持有飞行器 ②站在楼梯格（StairUp / StairDown）③不在商店 ④不在战斗中，否则返回失败原因。
- `FlyTo(floorNumber)`：目标必须 ∈ `VisitedFloors`，否则拒绝；成功后触发 `FloorChanged` + `StateChanged` + 新事件 `FlyOrbTeleported`。
- `PickUp`：拾取 FlyOrb 时写入 `IsHasFlyOrb` 并触发 `ItemPickedUp(item)`（事件签名由 `Action` 改为 `Action<Item>`，携带 `Item`）。
- `FindTile(TileType, out int, out int)` 改为返回 `bool`（供 FlyOrb 落点查找复用）。
- 1F 教士对话在持有飞行器时追加一行提示「在楼梯旁使用飞行器可以穿梭楼层」。
- 传送使用 `_floors[floor]` 原有对象，怪物 / 物品 / 门消耗状态天然保留；死亡重置仅重建 `Player`（`VisitedFloors` 清空），读档恢复。

## 4. 1F 放置与生成器

- `Data/Floors/Floor1.json`：行 7 列 1 改为 `.`，新增物品 Id 11（FlyOrb）坐标 `X=1, Y=7`（左下暗角，无门 / 无怪，开局可达）。JSON 已校验。
- `MotaMapGenerator/ItemTemplates.cs`：新增 Id 13 模板（FlyOrb）。
- `MotaMapGenerator/FloorGenerator.cs`：`ApplyFloor1Special` 同步放置飞行器。
- `MotaMapGenerator/Program.cs`：接入上述特殊楼层处理，重跑 `dotnet run --project MotaMapGenerator` 可重建全部 Floor JSON（手加 NPC 需人工保留）。

## 5. UI（`MainWindow.xaml` / `.cs`）

- **背包弹窗**（`InventoryOverlay`，`I` 键开关，Esc 关闭）：黑底金边复古风格，标题「背 包」；持有飞行器时显示道具卡（图标 + 名称 + 说明 + 「使用」按钮），未持有显示「背包空空如也。」。
- **穿梭面板**（`FlyOrbOverlay`）：标题「【楼层穿梭飞行器】」，`ItemsControl` 列出 1~100 层；已到达显示「已到达」（金色可点），未探索显示「未探索」（灰色禁用）；选中楼层后「传 送」按钮激活，「取 消」关闭。
- 打开面板时锁定移动 / F5 / F9 / M / B，Esc 按 穿梭→背包→手册→商店 顺序逐级关闭。
- 地图渲染：`TileType.Item` 的 brush switch 增加 `ItemType.FlyOrb => "ItemFlyOrbBrush"`。
- 事件接线：`ItemPickedUp` 订阅签名改为 `(Item item)`；新增 `_engine.FlyOrbTeleported += OnFlyOrbTeleported`（播落地音 + 刷新）。

## 6. 音频

`Assets/Audio/generate_audio.ps1` 扩展三个 8-bit SFX 并已生成：

| 文件 | 用途 | 触发点 |
|---|---|---|
| `flyorb_pickup.wav` | 拾取 | `OnItemPickedUp` FlyOrb 分支 |
| `flyorb_launch.wav` | 启动 | 确认传送时 |
| `flyorb_arrive.wav` | 落地 | `OnFlyOrbTeleported` |

`AudioManager` 新增 `_flyOrbPickup / _flyOrbLaunch / _flyOrbArrive` 三个播放器与 `PlayFlyOrbPickup / PlayFlyOrbLaunch / PlayFlyOrbArrive` 方法。

## 7. 素材

- `Assets/generate_flyorb_tile.ps1`（新脚本）：生成 `Assets/Tiles/item_flyorb.png`（地图瓦片）与 `icon_flyorb.png`（背包图标）。
- `Assets/GameBrushes.xaml`：新增 `ItemFlyOrbBrush`。

## 8. 构建验证

```powershell
dotnet build Mota100Floors.csproj -c Debug
dotnet build Mota100Floors.csproj -c Release
```

结果：**0 警告 0 错误**。

## 9. 手动测试清单

- [ ] 开局 1F 左下角拾取飞行器（播放拾取音 + 说明弹窗）。
- [ ] 背包 `I` 键开关；未持有时显示「背包空空如也」。
- [ ] 非楼梯格点「使用」→ 失败提示（不打开面板）。
- [ ] 楼梯格点「使用」→ 穿梭面板打开；未探索楼层置灰不可点。
- [ ] 选中已到达楼层 → 「传 送」可用 → 播放启动 / 落地音，落地于目标层楼梯（优先下楼楼梯）。
- [ ] 传送后目标层已击杀怪物 / 已拾取物品 / 已开门状态保留。
- [ ] `F5` 存档 `F9` 读档：飞行器持有与 `VisitedFloors` 均恢复（v1 旧档不报错）。
- [ ] 阵亡重开：飞行器与已探索楼层清空。
- [ ] 隐藏层（0F / 44F）可被访问 / 传送，落地楼梯回退逻辑正常。
- [ ] 面板打开期间方向键 / F5 / F9 / M / B 锁定，Esc 逐级关闭。

## 10. 已知说明

- 任务文档原述楼层范围 1~50，本作实际 100 层（`MapConstants.TotalFloors = 100`，100F BOSS 通关），列表与校验均按 100 层实现。
- 重跑 `MotaMapGenerator` 会重建所有 Floor JSON，`ApplyFloor1Special` 已同步飞行器放置；其它手工改动需人工合并。
