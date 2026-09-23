# Mota 100 Floors · v1.0.0 Release Notes（中英 · EN/ZH）

> **由 DeepSeek V4 Flash AI 开发 · Developed by DeepSeek V4 Flash AI**

## 🤖 AI 开发声明 · AI Development Notice

本游戏**完全由 DeepSeek V4 Flash AI 开发**：游戏设计（GDD）、C# 代码、XAML 界面、100 层地图数据生成器、图标与音效等全部由 AI 生成与迭代完成。

This game was **entirely developed by the DeepSeek V4 Flash AI** — GDD, C# logic, XAML UI, the 100-floor map generator, icon and audio assets were all authored and iterated by AI.

---

## 🆕 本版内容 · What's New

- **中英双语 GitHub 文档**：README 全面中英对照，含 AI 开发声明、技术栈、功能、操作、目录、下载与资源说明。Bilingual GitHub documentation with an AI development notice.
- **程序化应用图标**：金色塔标 `icon.ico`（16–256 px 七尺寸），同时用于 exe 文件图标与主窗口标题栏，由 `Assets/generate_icon.ps1` 生成。Procedural golden-tower `icon.ico` (7 sizes) used for both the exe and the main window.
- **初始生命值 100**：新游戏初始 HP / MaxHP = 100（原 500）。Starting HP / MaxHP set to 100.
- **启动崩溃修复**：修复 1F 地图 `GridRows[7]` 缺字符（17 字符）导致的 `IndexOutOfRangeException`，已校验全部 100 层网格为 18×11。Fixed the F1 map missing-char startup crash; all 100 floors validated as 18×11.
- **飞行器（楼层穿梭机）**：1F 永久道具，楼梯上使用即可穿梭至已到达楼层，含专属音效与地图图标。Fly Orb floor-shuttle with dedicated SFX and map icon.
- **逐回合战斗（GDD V1.0）**：战斗面板 + 450 ms/回合逐回合结算；4 类怪物（普通 / 魔法无视防御 / 吸血 / Boss）；`B` 键怪物手册实时胜负判定。Turn-based combat panel, 4 monster types and a live monster book.

---

## 📦 发布文件 · Release Files

| 文件 File | 说明 Description |
|---|---|
| `Mota100Floors-win-x64-v1.0.0.zip`（60.7 MB） | 自包含单文件 exe + Data / Audio 资源，解压即玩，免装 .NET · self-contained single-file exe + data/audio, extract and play |
| `SHA256SUMS.txt` | 文件完整性校验和 · integrity checksums |
| `ReleaseNotes.md` | 本版本说明 · this document |

**SHA256**

```
8FE72754A9E9995A72697C15A633DE3E99401223A910C18C01E692A78C4283F5  Mota100Floors-win-x64-v1.0.0.zip
```

---

## ✅ 运行验证 · Verified

- Debug / Release 构建：0 警告 0 错误。Build: 0 warnings / 0 errors.
- 自包含发布 exe（125.8 MB）在 Windows x64 上启动并持续运行正常，无异常退出。Self-contained exe launches and runs normally on Windows x64.
- 发布 zip 共 122 个条目（exe + 100 层数据 + 音频资源）。Release zip contains 122 entries.

---

## ⚠️ 注意 · Notes

- `Assets/` 中的经典魔塔素材源自原版游戏，仅供学习与致敬，**请勿商用**。Classic Mota assets are for learning/homage only — do not use commercially.
- 读取旧存档沿用存档内数值；新游戏 / 重置后才应用初始生命 100。Loading an old save keeps its values; new games apply HP 100.
