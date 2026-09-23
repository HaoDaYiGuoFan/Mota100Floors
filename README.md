# 🗼 魔塔 100 层 · Mota 100 Floors

🌐 **语言切换 / Language Switch：** [🇨🇳 **中文**](README.md) · [🇬🇧 **English**](README.en.md)

> 🧒 **童年的游戏 · 翻版致敬**
>
> 小时候，没有精致的画面，也没有复杂的系统——一台老电脑、几个方向键，就装下了整个童年。
> 魔塔，是许多人游戏生涯的启蒙：攒钥匙、算伤害、步步为营，只为登上塔顶的那一天。
> 这份翻版之作，把那份纯真与热血原样带回：**同样的 100 层挑战，一场全新的冒险。**

> **由 DeepSeek V4 Flash AI 开发**  
> 经典魔塔式回合制地牢 RPG，基于 .NET 10 + WPF

![.NET 10](https://img.shields.io/badge/.NET-10.0-blue)
![C#](https://img.shields.io/badge/C%23-13.0-brightgreen)
![WPF](https://img.shields.io/badge/WPF-Windows-purple)
![AI](https://img.shields.io/badge/AI-DeepSeek_V4_Flash-orange)
![Platform](https://img.shields.io/badge/Platform-Windows%20x64-informational)

---

## 🤖 AI 开发声明

本游戏**完全由 DeepSeek V4 Flash AI 开发**：从游戏设计（GDD）、C# 代码、XAML 界面、100 层地图数据生成器，到图标 / 音效等全部内容均由 AI 生成与迭代完成，供学习、娱乐与 AI 协作开发研究之用。

---

## 📋 技术栈

| 技术 | 版本 |
|---|---|
| 平台 | Windows (WPF) |
| 框架 | .NET 10 (`net10.0-windows`) |
| 语言 | C#（隐式 using、可空启用） |
| MVVM | CommunityToolkit.Mvvm 8.4 |
| 发布形态 | 自包含单文件 exe（win-x64，免装运行时） |

---

## ✨ 功能特性

- **100 层地牢**：18 × 11 网格、48 px 瓦片；上楼 / 下楼 / 三色钥匙门 / 回廊互通，可绕路变强后再战。
- **经典素材换皮**：原版魔塔素材重绘为 `monster_classic_1~6` 六套怪物贴图、门 / 钥匙 / 楼梯 / 商店 / NPC / 药水 / 力量与守护宝石，全部内嵌为 WPF `Resource`。
- **四方向玩家精灵**：`player_up / down / left / right` 按移动方向实时切换贴图，撞墙不转向。
- **逐回合战斗（GDD V1.0）**：走上怪物格弹出战斗面板——先展示怪物卡 / 勇士卡与预估战果，选择「⚔ 战斗 / 撤退」；确认后逐回合推进（玩家先手 → 怪物反击 → 吸血回复），450 ms/回合实时刷新 HP 与战报日志。预估与真实结算共用同一公式（`MathHelper`）。
- **破防门槛**：玩家攻击 ≤ 怪物防御时禁止开战；预估必死时给出红色预警，仍可选择以命相搏。
- **4 类怪物**：`Normal` 普通 ／ `FixedDamage` 魔法·固定伤害（无视防御）／ `DrainBlood` 吸血 ／ `Boss`（禁止撤退、胜利掉落红钥匙，100 层 BOSS 击破即通关）。
- **怪物手册（`B` 键）**：41 种怪物的生命 / 攻击 / 防御 / 金币 / 贴图 / 类型，实时计算预估损失与「✓ / ✗ 可战胜」。
- **音效与音乐**：拾取、飞行器拾取 / 启动 / 落地、移动、开门、战斗命中、胜利、阵亡共 8 类音效；探索 / 战斗双 BGM；`M` 键静音；全部由 `generate_audio.ps1` 程序化合成。
- **存档系统**：`F5` 存档、`F9` 读档（`%LOCALAPPDATA%\Mota100Floors\save.json`，临时文件 + 原子覆盖）。
- **经典魔塔系统**：15 座商店（金币购血 / 攻 / 防 / 钥匙）、NPC 任务（5 项）、6 处守门属性门槛、Boss 战与 100 层通关判定。
- **飞行器（楼层穿梭机）**：1F 左下角永久道具；`I` 键打开背包，站在楼梯格上使用打开穿梭面板，传送到任意已到达楼层（未探索置灰）。
- **数值与图标**：初始生命 100 / 攻击 10 / 防御 10；金色塔标应用图标由 `generate_icon.ps1` 程序化生成（exe + 窗口双用）。

---

## 🚀 快速开始

前置要求：**[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**（Windows；仅源码构建需要，直接玩用下方 Release 包）。

```powershell
# 构建（Debug）
dotnet build

# 直接运行
dotnet run --project Mota100Floors.csproj

# 发布 Windows x64 自包含单文件 exe（免装 .NET 运行时）
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -o publish\win-x64
```

> 发布产物位于 `publish\win-x64\`，双击 `Mota100Floors.exe` 即可游玩，目标机器无需安装 .NET。单文件 exe 已内嵌 PNG 资源与运行时；`Data\*.json` 与 `Assets\Audio\*.wav|*.mp3` 按设计以旁置目录随程序发布（csproj 中 `ExcludeFromSingleFile`），分发时整个目录一起拷贝。

## ⬇️ 下载 Release

最新版本可从 **GitHub Releases** 获取（`release/` 目录提供本地副本）：

- `Mota100Floors-win-x64-v1.0.0.zip` — 自包含单文件 exe + 数据 / 音频资源（解压即玩，免装 .NET）
- `SHA256SUMS.txt` — 文件完整性校验和
- `ReleaseNotes.md` — 版本说明（中英）

---

## 🎮 操作说明

| 按键 | 功能 |
|---|---|
| `↑` `↓` `←` `→` / `W` `A` `S` `D` | 移动一格（实时切换朝向贴图） |
| `B` | 打开 / 关闭怪物手册 |
| `I` | 打开 / 关闭背包（拾取飞行器后可见「使用」入口） |
| `Esc` | 关闭穿梭面板 / 背包 / 手册 / 商店 / 对话 |
| `F5` | 存档 |
| `F9` | 读档 |
| `M` | 音乐与音效静音开关 |

走上道具格自动拾取；走上怪物格弹出战斗面板（BOSS 战强制开战）；上楼 / 下楼为地图元素，站上即传送。战斗中全局按键锁定，直至点击「继续」完成结算。

## 🗂 目录结构

```text
Mota100Floors/
├─ Mota100.slnx / Mota100Floors.csproj
├─ App.xaml(.cs) / MainWindow.xaml(.cs)     # 入口与主界面
├─ Assets/
│  ├─ GameBrushes.xaml                      # 全局画刷
│  ├─ Audio/                                # 双 BGM + 8 类音效（generate_audio.ps1 合成）
│  ├─ Tiles/                                # 瓦片与经典素材 PNG
│  ├─ icon.ico / generate_icon.ps1          # 程序化生成的应用图标
│  └─ import_classic_assets.ps1             # 经典素材批量导入
├─ Data/
│  ├─ Monsters.json                         # 41 个怪物模板
│  ├─ Items.json / Quests.json / ShopConfig.json
│  └─ Floors/Floor1..100.json               # 每层地图与放置数据
├─ Helpers/  MapConstants / MathHelper / TileCollisionHelper
├─ Models/   Monster / Player / Item / MapTile / FloorData / Quest / ...
├─ Services/ GameData / GameEngine / AudioManager / SaveSystem
├─ MotaMapGenerator/                        # 独立数据生成与校验工具
├─ docs/                                    # GDD 设计文档
└─ release/                                 # 发布产物（zip + 校验和 + 版本说明）
```

## 🧊 内容与数据

- **怪物模板**（`Data/Monsters.json`）：41 个模板，`SpriteIndex = 1~6` 对应 `monster_classic_1~6` 贴图动态映射；`IgnoreDefense` 法师模板在战斗与手册中均无视玩家防御。
- **楼层数据**（`Data/Floors/`）：由 `MotaMapGenerator` 生成与校验（独立控制台项目，主项目通过 `DefaultItemExcludes` 排除编译）。修改后运行：`dotnet run --project MotaMapGenerator`。

## 🔗 设计文档

- 总览：[docs/index.html](docs/index.html)（story / interaction / combat / content 子页）
- Markdown：[GDD V1.0 怪物与逐回合战斗系统](docs/GDD.md) · [怪物系统实现规格](docs/monster-system-spec.md) · [飞行器（楼层穿梭机）任务文档](docs/MT-TASK-ITEM-FlyOrb.md)

## ⚠️ 资源说明

`Assets/` 中的经典魔塔素材源自原版游戏，`import_classic_assets.ps1` 仅作学习与致敬用途的本地导入，**请勿用于商业发布**；音效 / 音乐为程序化合成文件，可自由替换为授权素材。

## 🙏 参考与致谢

本项目参考了开源 pygame 魔塔项目 **MagicTowerGame** 的玩法设计与美术素材，并在其基础上以 .NET 10 / WPF 进行了翻版重构与扩展：

- **项目地址**：[GaoDeBuChou/MagicTowerGame](https://github.com/GaoDeBuChou/MagicTowerGame)（`master` 分支）
- **参考内容**：经典魔塔玩法（攻防成长、钥匙开门、逐层闯关）；经典美术素材经 [`Assets/import_classic_assets.ps1`](Assets/import_classic_assets.ps1) 从该项目的 `img/` 目录导入并重绘。
- **特别致谢**：向 MagicTowerGame 的作者以及所有魔塔开源社区开发者致敬——是你们把童年的魔法塔带到了今天。

---

*由 DeepSeek V4 Flash AI 开发 · 使用愉快！有关问题可先查看 `docs/` GDD 文档，或阅读 `Services/` 中的源码。*