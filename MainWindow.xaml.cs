namespace Mota100Floors;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Mota100Floors.Helpers;
using Mota100Floors.Helpers.Enum;
using Mota100Floors.Models;
using Mota100Floors.Services;

/// <summary>主窗口：地图渲染 + HUD + 键盘控制 + 商店弹窗</summary>
public partial class MainWindow : Window
{
    private readonly GameEngine _engine = new();

    private readonly AudioManager _audio = new();

    private readonly DispatcherTimer _battleMusicTimer;

    /// <summary>逐回合战斗推进计时器（自动战斗，260ms/回合，对标原版触怪即战节奏）</summary>
    private readonly DispatcherTimer _battleStepTimer;

    /// <summary>战斗结果展示计时器（短暂展示胜负后自动结算关闭）</summary>
    private readonly DispatcherTimer _battleResultTimer;

    /// <summary>战斗面板是否打开</summary>
    private bool _battlePanelOpen;

    /// <summary>战斗结算结果面板已显示（防止 StateChanged / BattleEnded 误关面板）</summary>
    private bool _battleResultHandled;

    private Rectangle? _playerRect;

    /// <summary>玩家朝向（四方向动画）</summary>
    private enum PlayerFacing { Up, Down, Left, Right }

    private PlayerFacing _facing = PlayerFacing.Down;

    /// <summary>怪物手册弹窗是否打开</summary>
    private bool _bestiaryOpen;

    /// <summary>背包弹窗是否打开</summary>
    private bool _inventoryOpen;

    /// <summary>楼层穿梭飞行器面板是否打开</summary>
    private bool _flyOrbOpen;

    /// <summary>穿梭面板当前选中的目标楼层（null = 未选中）</summary>
    private int? _selectedFlyOrbFloor;

    private static readonly double C = MapConstants.CellSize;

    public MainWindow()
    {
        InitializeComponent();
        _battleMusicTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
        _battleMusicTimer.Tick += (_, _) =>
        {
            _battleMusicTimer.Stop();
            _audio.PlayExplore();
        };
        _engine.FloorChanged += OnFloorChanged;
        _engine.StateChanged += OnStateChanged;
        _engine.BattleStarted += OnBattleStarted;
        _engine.BattleEnded += OnBattleEnded;
        _engine.DoorOpened += OnDoorOpened;
        _engine.ItemPickedUp += OnItemPickedUp;
        _engine.FlyOrbTeleported += OnFlyOrbTeleported;
        _battleStepTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(260) };
        _battleStepTimer.Tick += OnBattleStepTick;
        _battleResultTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1150) };
        _battleResultTimer.Tick += (_, _) =>
        {
            _battleResultTimer.Stop();
            AutoFinishBattle();
        };
        Loaded += OnLoaded;
    }

    /// <summary>玩家确认开战：切入战斗 BGM 并播放开场命中音效（战斗全程保持）。</summary>
    private void OnBattleStarted()
    {
        _battleMusicTimer.Stop();
        _audio.PlayBattle();
        _audio.PlayHit();
    }

    /// <summary>成功开门：播放开门音效。</summary>
    private void OnDoorOpened() => _audio.PlayDoor();

    /// <summary>战斗结束（胜利/失败/撤退/取消）：未显示结果面板时自动关闭战斗面板。</summary>
    private void OnBattleEnded()
    {
        _battleStepTimer.Stop();
        if (!_battleResultHandled && _battlePanelOpen)
            CloseBattlePanel();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RefreshAll();
        _audio.PlayExplore();
        Focus();
    }

    private void OnFloorChanged()
    {
        _battleMusicTimer.Stop();
        _audio.PlayExplore();
        RefreshMap();
    }

    private void OnStateChanged()
    {
        RefreshMap();
        RefreshHud();
        RefreshShop();
        RefreshMessages();
        RefreshBestiaryIfOpen();
        RefreshBattlePanel();
    }

    /// <summary>拾取物品：飞行器有独立音效与说明弹窗，其余道具播放普通拾取音。</summary>
    private void OnItemPickedUp(Item item)
    {
        if (item.Type == ItemType.FlyOrb)
        {
            _audio.PlayFlyOrbPickup();
            MessageBox.Show("【捡到飞行器】这是一台古老的楼层穿梭飞行器，可在塔内楼层之间穿梭。\n提示：可在楼层任意位置启动，只能前往已经到达过的楼层。",
                "魔塔 100 层", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            _audio.PlayPickup();
        }
    }

    private void RefreshBestiaryIfOpen()
    {
        if (_bestiaryOpen) RefreshBestiary();
    }

    // ---------- 触怪即战：逐回合自动战斗（对标原版魔塔） ----------

    /// <summary>根据引擎状态同步战斗面板开关/内容（由 StateChanged 驱动；战斗自动开打）。</summary>
    private void RefreshBattlePanel()
    {
        var b = _engine.ActiveBattle;
        if (b != null)
        {
            if (!_battlePanelOpen) OpenBattlePanel(b);
            else RefreshBattleStatus(b);
        }
        else if (_battlePanelOpen && !_battleResultHandled)
        {
            CloseBattlePanel();
        }
    }

    private void OpenBattlePanel(ActiveBattleState b)
    {
        _battlePanelOpen = true;
        _battleResultHandled = false;
        BattleOverlay.Visibility = Visibility.Visible;

        var m = b.Monster;
        BattleSprite.Source = LoadMonsterImage(m.TemplateId, m.SpriteIndex);
        BattleMonsterName.Text = m.Name;
        BattleKindText.Text = b.IsBoss ? "BOSS"
            : b.IsFixed ? "魔法怪 · 固定伤害 · 无视防御"
            : b.IsDrain ? "吸血怪 · 反击后回复生命"
            : "普通怪";
        BattleMonsterStats.Text = $"HP {m.Hp}　攻击 {m.Attack}　防御 {m.Defense}　金币 {m.GoldReward}";
        BattleMonsterHp.Text = $"HP：{b.MonsterHp}";
        BattleIntro.Text = b.IntroText;

        BattlePlayerHp.Text = $"HP：{b.PlayerHp}";
        BattlePlayerAttack.Text = $"攻击 {_engine.Player.Attack}";
        BattlePlayerDefense.Text = $"防御 {_engine.Player.Defense}";

        BattleLog.Text = "";

        // 原版规则：触怪即战，无需确认，直接推进回合
        _battleStepTimer.Start();
        OnBattleStepTick(this, EventArgs.Empty);
        Focus();
    }

    /// <summary>刷新战斗中双方实时 HP 与最近战报。</summary>
    private void RefreshBattleStatus(ActiveBattleState b)
    {
        BattlePlayerHp.Text = $"HP：{b.PlayerHp}";
        BattleMonsterHp.Text = $"HP：{Math.Max(0, b.MonsterHp)}";
        BattleLog.Text = string.Join("\n", b.Steps.TakeLast(6).Select(s => s.Text));
    }

    private void OnBattleStepTick(object? sender, EventArgs e)
    {
        if (_engine.ActiveBattle is not { } b)
        {
            _battleStepTimer.Stop();
            return;
        }
        bool ended = _engine.StepBattle();
        RefreshBattleStatus(b);
        if (ended)
        {
            _battleStepTimer.Stop();
            HandleBattleEnd(b);
        }
    }

    /// <summary>战斗结束：短暂展示胜利/阵亡结果后自动结算（原版无确认步骤）。</summary>
    private void HandleBattleEnd(ActiveBattleState b)
    {
        _battleResultHandled = true; // 防止 StateChanged / BattleEnded 关闭面板

        if (b.Won)
        {
            _audio.PlayVictory();
            BattleResultTitle.Text = "🎉 战斗胜利！";
            BattleResultDetail.Text = $"击杀 {b.Monster.Name}，历时 {b.Turn} 回合。";
        }
        else
        {
            _audio.PlayDeath();
            BattleResultTitle.Text = "💀 你被击败了……";
            BattleResultDetail.Text = $"{b.Monster.Name} 夺走了你的生命。";
        }

        _battleMusicTimer.Stop();
        _battleMusicTimer.Start(); // 结果音效播放后切回探索 BGM
        BattleResultPanel.Visibility = Visibility.Visible;
        _battleResultTimer.Start(); // 短暂展示后自动结算
    }

    /// <summary>结果展示结束：执行战斗结算（奖励/阵亡/移动），并按需处理死亡/通关。</summary>
    private void AutoFinishBattle()
    {
        _engine.FinishBattle(); // 结算奖励/阵亡/移动
        RefreshAll();
        CloseBattlePanel();

        if (_engine.Player.IsDead)
        {
            MessageBox.Show("你阵亡了！点击确定重新开始。", "魔塔 100 层");
            _engine.Restart();
            RefreshAll();
        }
        else if (_engine.Player.IsWin)
        {
            MessageBox.Show("恭喜通关魔塔 100 层！点击确定重新开始。", "魔塔 100 层");
            _engine.Restart();
            RefreshAll();
        }
        Focus();
    }

    private void CloseBattlePanel()
    {
        _battleStepTimer.Stop();
        _battleResultTimer.Stop();
        _battlePanelOpen = false;
        _battleResultHandled = false;
        BattleOverlay.Visibility = Visibility.Collapsed;
        BattleResultPanel.Visibility = Visibility.Collapsed;
    }

    private void RefreshAll()
    {
        RefreshMap();
        OnStateChanged();
    }

    // ---------- 地图渲染 ----------

    private void RefreshMap()
    {
        MapCanvas.Children.Clear();
        _playerRect = null;

        var floor = _engine.CurrentFloor;
        MapCanvas.Width = floor.Width * C;
        MapCanvas.Height = floor.Height * C;

        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                var tile = floor.Tiles[x, y];
                Brush fill;
                string tooltip = "";

                switch (tile.TileType)
                {
                    case TileType.Wall:
                        fill = (Brush)FindResource("TileWallBrush");
                        break;
                    case TileType.StairUp:
                        fill = (Brush)FindResource("TileStairUpBrush");
                        tooltip = "上楼（通往上一层）";
                        break;
                    case TileType.StairDown:
                        fill = (Brush)FindResource("TileStairDownBrush");
                        tooltip = "下楼（通往下一层）";
                        break;
                    case TileType.RedDoor:
                        fill = (Brush)FindResource("DoorRedBrush");
                        tooltip = "红门（需要红钥匙）";
                        break;
                    case TileType.BlueDoor:
                        fill = (Brush)FindResource("DoorBlueBrush");
                        tooltip = "蓝门（需要蓝钥匙）";
                        break;
                    case TileType.YellowDoor:
                        fill = (Brush)FindResource("DoorYellowBrush");
                        tooltip = "黄门（需要黄钥匙）";
                        break;
                    case TileType.Monster:
                    {
                        var m = floor.MonstersOnFloor.FirstOrDefault(mm => mm.Id == tile.TargetId);
                        fill = MonsterBrush(m);
                        if (m != null)
                        {
                            tooltip = $"{m.Name}  HP:{m.Hp} 攻:{m.Attack} 防:{m.Defense} 金币:{m.GoldReward}";
                            if (m.IgnoreDefense) tooltip += "【魔法·无视防御】";
                        }
                        break;
                    }
                    case TileType.Item:
                    {
                        var it = floor.ItemsOnFloor.FirstOrDefault(ii => ii.Id == tile.TargetId);
                        fill = ItemBrush(it);
                        if (it != null) tooltip = $"{it.Name}  {it.Desc}";
                        break;
                    }
                    case TileType.NPC:
                        fill = (Brush)FindResource("NpcBrush");
                        var npc = floor.Events.FirstOrDefault(ev => ev.X == x && ev.Y == y);
                        if (npc != null)
                        {
                            tooltip = $"【{npc.Name}】{npc.Text}";
                            if (npc.IsGuard)
                                tooltip += $"\n守门条件：需要{GuardRequirementLabel(npc)}";
                            if (npc.QuestId is { } questId
                                && Mota100Floors.Services.GameData.Quests.FirstOrDefault(q => q.Id == questId) is { } quest)
                                tooltip += $"\n任务：「{quest.Name}」";
                        }
                        break;
                    case TileType.Shop:
                        fill = (Brush)FindResource("ShopBrush");
                        var shopEv = floor.Events.FirstOrDefault(ev => ev.X == x && ev.Y == y);
                        if (shopEv != null) tooltip = $"商店「{shopEv.Name}」";
                        break;
                    default:
                        fill = (Brush)FindResource("TileFloorBrush");
                        break;
                }

                var rect = new Rectangle
                {
                    Width = C,
                    Height = C,
                    Fill = fill,
                    ToolTip = string.IsNullOrEmpty(tooltip) ? null : tooltip,
                };
                // 像素贴图（32/48px）在非整数缩放（如 125% DPI）下双线性会发糊，逐元素强制最近邻
                RenderOptions.SetBitmapScalingMode(rect, BitmapScalingMode.NearestNeighbor);
                Canvas.SetLeft(rect, x * C);
                Canvas.SetTop(rect, y * C);
                MapCanvas.Children.Add(rect);
            }
        }

        _playerRect = new Rectangle
        {
            Width = C,
            Height = C,
            Fill = (Brush)FindResource(FacingBrushKey(_facing)),
        };
        RenderOptions.SetBitmapScalingMode(_playerRect, BitmapScalingMode.NearestNeighbor);
        MapCanvas.Children.Add(_playerRect);
        Canvas.SetZIndex(_playerRect, 2);
        UpdatePlayerPosition();
    }




    private void UpdatePlayerPosition()
    {
        if (_playerRect == null) return;
        double px = _engine.Player.PosX * C;
        double py = _engine.Player.PosY * C;
        Canvas.SetLeft(_playerRect, px);
        Canvas.SetTop(_playerRect, py);
    }

    // ---------- 素材映射：每只怪物 / 每种道具独立贴图，缺失时回退经典素材 ----------

    /// <summary>怪物格贴图：优先 monster_{TemplateId}.png（原版风格），回退 monster_classic_{1~6}。</summary>
    private Brush MonsterBrush(Monster? m)
    {
        int fallback = Math.Clamp(m?.SpriteIndex ?? 1, 1, 6);
        if (m != null && TryFindResource($"Monster{m.TemplateId}Brush") is Brush b) return b;
        return (Brush)FindResource($"MonsterClassic{fallback}Brush");
    }

    /// <summary>道具格贴图：优先 item_{TemplateId}.png（原版风格），回退按类型选取。</summary>
    private Brush ItemBrush(Item? it)
    {
        if (it != null && TryFindResource($"Item{it.TemplateId}Brush") is Brush b) return b;
        string itemKey = it?.Type switch
        {
            ItemType.Attack => "ItemAttackBrush",
            ItemType.Defense => "ItemDefenseBrush",
            ItemType.Key when it.KeyType == KeyType.Red => "ItemKeyRedBrush",
            ItemType.Key when it.KeyType == KeyType.Blue => "ItemKeyBlueBrush",
            ItemType.Key when it.KeyType == KeyType.Yellow => "ItemKeyYellowBrush",
            ItemType.FlyOrb => "ItemFlyOrbBrush",
            _ => "ItemHpBrush",
        };
        return (Brush)FindResource(itemKey);
    }

    /// <summary>战斗面板怪物立绘：优先 monster_{TemplateId}.png，回退经典贴图。</summary>
    private static BitmapImage LoadMonsterImage(int templateId, int spriteIndex)
    {
        string path = $"/Assets/Tiles/monster_{templateId}.png";
        try
        {
            return new BitmapImage(new Uri(path, UriKind.Relative));
        }
        catch
        {
            return new BitmapImage(new Uri($"/Assets/Tiles/monster_classic_{Math.Clamp(spriteIndex, 1, 6)}.png", UriKind.Relative));
        }
    }

    /// <summary>守门条件的可读描述（用于地图 Tooltip）</summary>
    private static string GuardRequirementLabel(GameEvent ev) => ev.Requirement switch
    {
        GuardRequirement.Attack => $"攻击力 ≥ {ev.RequirementValue}",
        GuardRequirement.Defense => $"防御力 ≥ {ev.RequirementValue}",
        GuardRequirement.Gold => $"金币 ≥ {ev.RequirementValue}",
        GuardRequirement.RedKey => $"红钥匙 ≥ {ev.RequirementValue}",
        GuardRequirement.BlueKey => $"蓝钥匙 ≥ {ev.RequirementValue}",
        GuardRequirement.YellowKey => $"黄钥匙 ≥ {ev.RequirementValue}",
        GuardRequirement.FloorReached => $"抵达第 {ev.RequirementValue} 层",
        _ => "",
    };

    // ---------- HUD / 消息 / 商店 ----------

    private void RefreshHud()
    {
        var p = _engine.Player;
        HudHp.Text = $"{p.Hp} / {p.MaxHp}";
        HudAttack.Text = p.Attack.ToString();
        HudDefense.Text = p.Defense.ToString();
        HudGold.Text = p.Gold.ToString();
        HudRedKey.Text = p.RedKey.ToString();
        HudBlueKey.Text = p.BlueKey.ToString();
        HudYellowKey.Text = p.YellowKey.ToString();
        HudFloor.Text = $"{p.CurrentFloor} / {MapConstants.TotalFloors}";
        ShopGold.Text = $"当前金币：{p.Gold}";
    }

    private void RefreshMessages()
    {
        MessageList.ItemsSource = null;
        MessageList.ItemsSource = _engine.Messages;
        MsgScroller.ScrollToTop();
    }

    private void RefreshShop()
    {
        bool open = _engine.IsShopOpen;
        ShopOverlay.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
        if (open)
        {
            ShopList.ItemsSource = null;
            ShopList.ItemsSource = _engine.CurrentShop.Items;
            var ev = _engine.CurrentFloor.Events.FirstOrDefault(e => e.ShopId != null);
            ShopTitle.Text = ev != null ? $"商店「{ev.Name}」" : "商店";
        }
    }

    // ---------- 背包（I 键）与楼层穿梭飞行器 ----------

    /// <summary>背包弹窗开关（穿梭面板打开时不切换）。</summary>
    private void ToggleInventory()
    {
        if (_flyOrbOpen) return;
        _inventoryOpen = !_inventoryOpen;
        InventoryOverlay.Visibility = _inventoryOpen ? Visibility.Visible : Visibility.Collapsed;
        if (_inventoryOpen) RefreshInventory();
        Focus();
    }

    private void OnInventoryClick(object sender, RoutedEventArgs e) => ToggleInventory();

    private void OnInventoryCloseClick(object sender, RoutedEventArgs e) => ToggleInventory();

    /// <summary>关闭背包弹窗（不翻转开关状态，用于打开穿梭面板前）。</summary>
    private void CloseInventory()
    {
        _inventoryOpen = false;
        InventoryOverlay.Visibility = Visibility.Collapsed;
    }

    /// <summary>根据持有物刷新背包内容（目前仅飞行器）。</summary>
    private void RefreshInventory()
    {
        bool has = _engine.Player.IsHasFlyOrb;
        InventoryFlyOrbSection.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
        InventoryEmptyText.Visibility = has ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>背包内点击「使用」：必须持有飞行器且不在商店/战斗中，否则提示失败原因。</summary>
    private void OnFlyOrbUseClick(object sender, RoutedEventArgs e)
    {
        if (!_engine.TryUseFlyOrb(out string reason))
        {
            _engine.Notify(reason);
            return;
        }
        CloseInventory();
        OpenFlyOrbPanel();
    }

    /// <summary>打开穿梭面板：刷新楼层列表并聚焦。</summary>
    private void OpenFlyOrbPanel()
    {
        _flyOrbOpen = true;
        _selectedFlyOrbFloor = null;
        RefreshFlyOrbPanel();
        FlyOrbOverlay.Visibility = Visibility.Visible;
        Focus();
    }

    /// <summary>关闭穿梭面板。</summary>
    private void CloseFlyOrbPanel()
    {
        _flyOrbOpen = false;
        _selectedFlyOrbFloor = null;
        FlyOrbOverlay.Visibility = Visibility.Collapsed;
    }

    private void OnFlyOrbCancelClick(object sender, RoutedEventArgs e)
    {
        CloseFlyOrbPanel();
        Focus();
    }

    /// <summary>刷新穿梭面板楼层列表：已探索楼层可点选，未探索置灰。</summary>
    private void RefreshFlyOrbPanel()
    {
        var visited = _engine.Player.VisitedFloors;
        var entries = Enumerable.Range(1, MapConstants.TotalFloors).Select(n =>
        {
            bool v = visited != null && visited.Contains(n);
            return new FlyOrbFloorEntry
            {
                FloorNumber = n,
                IsVisited = v,
                FloorLabel = $"第 {n} 层",
                StateText = v ? "已到达" : "未探索",
                StateBrush = v ? Brushes.Goldenrod : Brushes.Gray,
                HintText = v ? "点击选中，传送到此楼层" : "尚未抵达此楼层",
            };
        }).ToList();
        FlyOrbFloorList.ItemsSource = entries;
        FlyOrbTeleportBtn.IsEnabled = false;
    }

    /// <summary>点击楼层条目：选中目标楼层并启用传送按钮。</summary>
    private void OnFlyOrbFloorClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: int floor })
        {
            _selectedFlyOrbFloor = floor;
            FlyOrbTeleportBtn.IsEnabled = true;
        }
    }

    /// <summary>确认传送：播放启动音 → 引擎传送（保留各层怪物/物品/门状态，落地到楼梯格）。</summary>
    private void OnFlyOrbTeleportClick(object sender, RoutedEventArgs e)
    {
        if (_selectedFlyOrbFloor is not { } floor) return;
        _audio.PlayFlyOrbLaunch();
        CloseFlyOrbPanel();
        _engine.FlyTo(floor);
        Focus();
    }

    /// <summary>飞行器传送完成：播放落地音效并刷新界面。</summary>
    private void OnFlyOrbTeleported()
    {
        _audio.PlayFlyOrbArrive();
        RefreshAll();
    }

    private sealed class FlyOrbFloorEntry
    {
        public int FloorNumber { get; init; }
        public bool IsVisited { get; init; }
        public string FloorLabel { get; init; } = "";
        public string StateText { get; init; } = "";
        public Brush StateBrush { get; init; } = Brushes.Gray;
        public string HintText { get; init; } = "";
    }

    // ---------- 交互 ----------

    private void OnShopBuyClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ShopItem item })
        {
            _engine.BuyShopItem(item);
            Focus();
        }
    }

    private void OnShopCloseClick(object sender, RoutedEventArgs e)
    {
        _engine.CloseShop();
        Focus();
    }

    private void OnRestartClick(object sender, RoutedEventArgs e)
    {
        _engine.Restart();
        RefreshAll();
        Focus();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e) => SaveGame();

    private void OnLoadClick(object sender, RoutedEventArgs e) => LoadGame();

    /// <summary>保存当前游戏进度到磁盘（%LOCALAPPDATA%\Mota100Floors\save.json）</summary>
    private void SaveGame()
    {
        if (_engine.IsShopOpen)
        {
            _engine.Notify("商店中无法存档，请先关闭商店（Esc）。");
            return;
        }

        _engine.Notify(SaveSystem.Write(_engine.CreateSave())
            ? $"存档成功：{SaveSystem.SaveFilePath}"
            : "存档失败：无法写入磁盘。");
    }

    /// <summary>从磁盘读取存档并恢复游戏状态</summary>
    private void LoadGame()
    {
        var save = SaveSystem.Read();
        if (save == null)
        {
            _engine.Notify("未找到有效存档，请先按 F5 保存。");
            return;
        }

        _battleMusicTimer.Stop();
        _audio.PlayExplore();
        _engine.LoadFromSave(save);
        RefreshAll();
        Focus();
    }

    // ---------- 怪物手册（B 键）与朝向辅助 ----------

    private string FacingBrushKey(PlayerFacing f) => f switch
    {
        PlayerFacing.Up => "PlayerUpBrush",
        PlayerFacing.Left => "PlayerLeftBrush",
        PlayerFacing.Right => "PlayerRightBrush",
        _ => "PlayerDownBrush",
    };

    private void SetFacing(int dx, int dy)
    {
        _facing = dx > 0 ? PlayerFacing.Right
            : dx < 0 ? PlayerFacing.Left
            : dy > 0 ? PlayerFacing.Down
            : PlayerFacing.Up;
        if (_playerRect != null)
            _playerRect.Fill = (Brush)FindResource(FacingBrushKey(_facing));
    }

    private void ToggleBestiary()
    {
        _bestiaryOpen = !_bestiaryOpen;
        BestiaryOverlay.Visibility = _bestiaryOpen ? Visibility.Visible : Visibility.Collapsed;
        if (_bestiaryOpen) RefreshBestiary();
        Focus();
    }

    private void OnBestiaryClick(object sender, RoutedEventArgs e) => ToggleBestiary();

    private void OnBestiaryCloseClick(object sender, RoutedEventArgs e) => ToggleBestiary();

    /// <summary>按当前玩家属性刷新手册（预估伤害与战斗预估逻辑保持一致，含法师无视防御）。</summary>
    private void RefreshBestiary()
    {
        var p = _engine.Player;
        var entries = GameData.MonsterTemplates.Select(t =>
        {
            int damage = MathHelper.EstimatedPlayerDamage(t, p);
            bool canKill = MathHelper.CanKill(t, p);
            var color = canKill ? Color.FromRgb(0x2E, 0x7D, 0x32) : Color.FromRgb(0xC6, 0x3B, 0x3B);
            string sprite = t.Type == MonsterType.Boss || t.IsBoss ? "BOSS" : t.IgnoreDefense ? "魔法·无视防御" : "";
            return new BestiaryEntry
            {
                SpritePath = $"/Assets/Tiles/monster_{t.Id}.png",
                Name = t.Name,
                Hp = t.Hp,
                Attack = t.Attack,
                Defense = t.Defense,
                Gold = t.GoldReward,
                KindText = sprite,
                DamageText = $"预估损失 {damage}",
                CanKillText = canKill ? "✓ 可战胜" : "✗ 暂不可战胜",
                CanKillBrush = new SolidColorBrush(color),
            };
        }).ToList();
        BestiaryList.ItemsSource = entries;
    }

    private sealed class BestiaryEntry
    {
        public string SpritePath { get; init; } = "";
        public string Name { get; init; } = "";
        public int Hp { get; init; }
        public int Attack { get; init; }
        public int Defense { get; init; }
        public int Gold { get; init; }
        public string KindText { get; init; } = "";
        public string DamageText { get; init; } = "";
        public string CanKillText { get; init; } = "";
        public Brush CanKillBrush { get; init; } = Brushes.Gray;
    }

    private void ToggleMute()
    {
        bool muted = _audio.ToggleMute();
        _engine.Messages.Insert(0, muted ? "已静音（按 M 恢复音乐）。" : "音乐已开启（按 M 静音）。");
        RefreshMessages();
    }

    protected override void OnClosed(EventArgs e)
    {
        _audio.Dispose();
        base.OnClosed(e);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        int dx = 0, dy = 0;
        bool anyPanelOpen = _flyOrbOpen || _inventoryOpen;
        switch (e.Key)
        {
            case Key.F5:
                if (!_engine.IsBattleOpen && !anyPanelOpen) SaveGame();
                e.Handled = true;
                return;
            case Key.F9:
                if (!_engine.IsBattleOpen && !anyPanelOpen) LoadGame();
                e.Handled = true;
                return;
            case Key.M:
                if (!_engine.IsBattleOpen && !anyPanelOpen) ToggleMute();
                e.Handled = true;
                return;
            case Key.Up:
            case Key.W:
                dy = -1;
                break;
            case Key.Down:
            case Key.S:
                dy = 1;
                break;
            case Key.Left:
            case Key.A:
                dx = -1;
                break;
            case Key.Right:
            case Key.D:
                dx = 1;
                break;
            case Key.Escape:
                if (_engine.IsBattleOpen) return; // 战斗中不可关闭面板
                if (_flyOrbOpen) { CloseFlyOrbPanel(); Focus(); return; }
                if (_inventoryOpen) { ToggleInventory(); return; }
                if (_bestiaryOpen) { ToggleBestiary(); return; }
                if (_engine.IsShopOpen) _engine.CloseShop();
                return;
            case Key.I:
                if (!_engine.IsBattleOpen && !_engine.IsShopOpen && !_flyOrbOpen) ToggleInventory();
                e.Handled = true;
                return;
            case Key.B:
                if (!_engine.IsBattleOpen && !anyPanelOpen) ToggleBestiary();
                e.Handled = true;
                return;
            default:
                return;
        }

        if (dx != 0 || dy != 0)
        {
            SetFacing(dx, dy);
            if (!_engine.IsBattleOpen && !anyPanelOpen) // 战斗/面板打开时禁止移动（引擎 TryMove 会拒绝战斗移动）
            {
                bool moved = _engine.TryMove(dx, dy);
                UpdatePlayerPosition();
                if (moved) _audio.PlayStep();
                if (_engine.Player.IsDead)
                {
                    MessageBox.Show("你阵亡了！点击确定重新开始。", "魔塔 100 层");
                    _engine.Restart();
                    RefreshAll();
                }
                else if (_engine.Player.IsWin)
                {
                    MessageBox.Show("恭喜通关魔塔 100 层！点击确定重新开始。", "魔塔 100 层");
                    _engine.Restart();
                    RefreshAll();
                }
            }
            e.Handled = true;
        }
    }
}
