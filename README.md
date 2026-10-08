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

## 🆕 V1.1 · 对标原版魔塔重制

- 🎵 **BGM 全面换血**：探索 / 战斗双 BGM 由阴沉小调改为欢快大调（C 大调 112 BPM ／ A 大调 150 BPM），8-bit 风格无缝循环。
- 👾 **怪物全面对标原版**：60 种怪物换用原版魔塔体系与数值（1~23 号 = 原版 24 层经典版怪物手册原值），每只怪物独立原版风格像素贴图。
- ⚔️ **打怪玩法对齐原版**：触怪即战（取消战斗 / 撤退确认面板，逐回合动画自动推进），法师系无视防御，破防不足视作撞墙。
- 🧪 **数值经济重制**：玩家 500 / 10 / 10 起步（按前几层怪物战损校准）、红蓝宝石 +2 起步、药水与分层商店（4 档定价）对齐原版节奏；生成器新增通关平衡模拟器。
- 🛠 **修复**：地图生成器输出路径错位（数据此前未真正写入游戏 `Data/` 目录）的问题。

---

## ✨ 功能特性

- **100 层地牢**：18 × 11 网格、48 px 瓦片；上楼 / 下楼 / 三色钥匙门 / 回廊互通，可绕路变强后再战。
- **完全对标原版魔塔（V1.1 数值重制）**：60 种怪物全面换用原版体系——1~23 号（绿 / 红 / 黑史莱姆、小 / 大 / 红蝙蝠、骷髅人 / 士兵 / 队长、初级 / 高级 / 麻衣法师、初级 / 中级 / 高级卫兵、兽人、兽人武士、石头人、大乌鸦、白衣武士、双手剑士、僵尸）数值原样取自原版 24 层经典版怪物手册，25~60 号按原版成长曲线外推；每 10 层一座原版风格里程碑 BOSS（骷髅将军 → 冥灵魔王）。
- **原版风格像素贴图**：每只怪物 / 每种道具独立贴图（`monster_1~60.png`、`item_1~12.png`，16×16 像素画 ×3 放大），由 `Assets/generate_monster_sprites.ps1` 程序化生成，预览见 [docs/sprites-preview.png](docs/sprites-preview.png)；门 / 钥匙 / 楼梯 / 商店 / NPC / 玩家沿用经典素材，全部内嵌为 WPF `Resource`。
- **四方向玩家精灵**：`player_up / down / left / right` 按移动方向实时切换贴图，撞墙不转向。
- **触怪即战（对标原版）**：走上怪物格立即开战，逐回合动画推进（玩家先手 → 怪物反击 → 吸血回复），260 ms/回合实时刷新 HP 与战报日志，结束自动结算；无法破防的怪物视作撞墙，杜绝无谓送死。
- **原版伤害公式**：玩家单次伤害 = max(1, 玩家ATK − 怪物DEF)，魔法系怪物无视防御；预估与真实结算共用同一公式（`MathHelper`）。
- **怪物手册（`B` 键）**：60 种怪物的生命 / 攻击 / 防御 / 金币 / 贴图 / 类型，实时计算预估损失与「✓ / ✗ 可战胜」。
- **音效与音乐**：拾取、飞行器拾取 / 启动 / 落地、移动、开门、战斗命中、胜利、阵亡共 8 类音效；欢快向探索 / 战斗双 BGM（C 大调 112 BPM ／ A 大调 150 BPM，8-bit 风格无缝循环）；`M` 键静音；全部由 `generate_audio.ps1` 程序化合成。
- **存档系统**：`F5` 存档、`F9` 读档（`%LOCALAPPDATA%\Mota100Floors\save.json`，临时文件 + 原子覆盖）。
- **经典魔塔系统**：分层商店（按楼层段位 4 档定价，金币购血 / 攻 / 防 / 钥匙）、NPC 任务（5 项）、6 处守门属性门槛、每 10 层 BOSS 掉落红钥匙、100 层击破冥灵魔王通关。
- **飞行器（楼层穿梭机）**：1F 左下角永久道具；`I` 键打开背包，在楼层任意位置使用打开穿梭面板，传送到任意已到达楼层（未探索置灰，落地于目标层楼梯处）。
- **原版数值经济**：初始生命 500 / 攻击 10 / 防御 10 / 黄钥匙 ×1（初始生命按最优顺序全清第 1 层损血 ≈58% 校准），红 / 蓝宝石 +2 起步、红药水 +100；生命上限 100,000（按通关模拟器校准：满清参考玩家最大单层战损约 5.5 千，10 万为其数倍余量，超出上限的回血浪费）；生成器内置**通关模拟器**自动校验 100 层曲线「可破防、无战损断链」。

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

走上道具格自动拾取；走上怪物格立即开战（触怪即战，逐回合自动战斗，无法破防的怪物视作撞墙）；上楼 / 下楼为地图元素，站上即传送。战斗中全局按键锁定，直至战斗结算完成。

## 🗂 目录结构

```text
Mota100Floors/
├─ Mota100.slnx / Mota100Floors.csproj
├─ App.xaml(.cs) / MainWindow.xaml(.cs)     # 入口与主界面
├─ Assets/
│  ├─ GameBrushes.xaml                      # 全局画刷
│  ├─ Audio/                                # 双 BGM + 8 类音效（generate_audio.ps1 合成）
│  ├─ Tiles/                                # 瓦片经典素材 + 60 怪物 / 12 道具原版风格贴图（generate_monster_sprites.ps1 生成）
│  ├─ icon.ico / generate_icon.ps1          # 程序化生成的应用图标
│  └─ import_classic_assets.ps1             # 经典素材批量导入
├─ Data/
│  ├─ Monsters.json                         # 60 个怪物模板（原版数值体系）
│  ├─ Items.json / Quests.json / ShopConfig.json
│  └─ Floors/Floor1..100.json               # 每层地图与放置数据
├─ Helpers/  MapConstants / MathHelper / TileCollisionHelper
├─ Models/   Monster / Player / Item / MapTile / FloorData / Quest / ...
├─ Services/ GameData / GameEngine / AudioManager / SaveSystem
├─ MotaMapGenerator/                        # 独立数据生成 + 连通性校验 + 通关平衡模拟
├─ docs/                                    # GDD 设计文档
└─ release/                                 # 发布产物（zip + 校验和 + 版本说明）
```

## 🧊 内容与数据

- **怪物模板**（`Data/Monsters.json`）：60 个模板，1~23 号为原版 24 层经典版数值，24~50 号为原版曲线外推，51~60 号为里程碑 BOSS；`SpriteIndex` 对应 `monster_{id}.png` 独立贴图，`IgnoreDefense` 法师系在战斗与手册中均无视玩家防御。
- **楼层数据**（`Data/Floors/`）：由 `MotaMapGenerator` 生成、校验连通性并运行通关平衡模拟（独立控制台项目，主项目通过 `DefaultItemExcludes` 排除编译）。修改后运行：`dotnet run --project MotaMapGenerator`（输出目录自动定位主项目 `Data/`）。

## 🔗 设计文档

- 总览：[docs/index.html](docs/index.html)（story / interaction / combat / content 子页）
- Markdown：[GDD V1.0 怪物与逐回合战斗系统](docs/GDD.md) · [怪物系统实现规格](docs/monster-system-spec.md) · [飞行器（楼层穿梭机）任务文档](docs/MT-TASK-ITEM-FlyOrb.md)
- 贴图预览：[docs/sprites-preview.png](docs/sprites-preview.png)（60 怪物 + 12 道具，`Assets/generate_monster_sprites.ps1` 可重新生成）

## ⚠️ 资源说明

`Assets/` 中的经典魔塔素材源自原版游戏，`import_classic_assets.ps1` 仅作学习与致敬用途的本地导入，**请勿用于商业发布**；音效 / 音乐为程序化合成文件，可自由替换为授权素材。

## 🙏 参考与致谢

本项目参考了开源 pygame 魔塔项目 **MagicTowerGame** 的玩法设计与美术素材，并在其基础上以 .NET 10 / WPF 进行了翻版重构与扩展：

- **项目地址**：[GaoDeBuChou/MagicTowerGame](https://github.com/GaoDeBuChou/MagicTowerGame)（`master` 分支）
- **参考内容**：经典魔塔玩法（攻防成长、钥匙开门、逐层闯关）；经典美术素材经 [`Assets/import_classic_assets.ps1`](Assets/import_classic_assets.ps1) 从该项目的 `img/` 目录导入并重绘。
- **特别致谢**：向 MagicTowerGame 的作者以及所有魔塔开源社区开发者致敬——是你们把童年的魔法塔带到了今天。

---

*由 DeepSeek V4 Flash AI 开发 · 使用愉快！有关问题可先查看 `docs/` GDD 文档，或阅读 `Services/` 中的源码。*