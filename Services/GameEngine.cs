namespace Mota100Floors.Services;

using Mota100Floors.Helpers;
using Mota100Floors.Helpers.Enum;
using Mota100Floors.Models;

public enum EntryMode
{
    /// <summary>初始进入（第 1 层出生点）</summary>
    Start,

    /// <summary>从下一层上楼进入（落在本层 D 处）</summary>
    FromBelow,

    /// <summary>从上一层下楼进入（落在本层 U 处）</summary>
    FromAbove,

    /// <summary>从存档恢复（保留记录中的坐标）</summary>
    Load,

    /// <summary>飞行器穿梭进入（落在目标楼层楼梯处）</summary>
    FlyOrb,
}

/// <summary>核心游戏引擎：状态 + 规则（移动/战斗/拾取/开门/楼梯/商店）</summary>
public class GameEngine
{
    public Player Player { get; private set; } = new();

    public FloorData CurrentFloor { get; private set; } = null!;

    public ShopConfig CurrentShop { get; private set; } = null!;

    public bool IsShopOpen { get; private set; }

    /// <summary>任务进度字典：QuestId → 0 未接取 / 1 进行中 / 2 已完成（随档保存）</summary>
    public Dictionary<int, int> QuestState { get; } = new();

    /// <summary>事件/战斗消息（最新在前）</summary>
    public List<string> Messages { get; } = new();

    private readonly Dictionary<int, FloorData> _floorCache = new();

    /// <summary>进行中的逐回合战斗（null = 无战斗）</summary>
    public ActiveBattleState? ActiveBattle { get; private set; }

    /// <summary>战斗是否已开始且未结束（期间禁止移动/交互）</summary>
    public bool IsBattleOpen => ActiveBattle != null;

    /// <summary>触发战斗的怪物格坐标（战斗胜利后移动到此）</summary>
    private (int X, int Y) _battleTile;

    private const int MaxMessages = 300;

    public event Action? FloorChanged;

    public event Action? StateChanged;

    /// <summary>进入战斗结算（已确认开打）</summary>
    public event Action? BattleStarted;

    /// <summary>战斗结算完成</summary>
    public event Action? BattleFinished;

    /// <summary>逐回合战斗中：每推进一回合触发（用于 UI 刷新回合战况）</summary>
    public event Action? BattleStepAdvanced;

    /// <summary>战斗结束（胜利/失败/撤退/取消），战斗面板可关闭</summary>
    public event Action? BattleEnded;

    /// <summary>成功开门（用于播放开门音效）</summary>
    public event Action? DoorOpened;

    /// <summary>成功拾取道具（携带被拾取的道具实例，用于播放拾取音效/触发特殊道具提示）</summary>
    public event Action<Item>? ItemPickedUp;

    /// <summary>飞行器传送完成（用于播放传送落地音效）</summary>
    public event Action? FlyOrbTeleported;

    public GameEngine() => EnterFloor(1, EntryMode.Start);

    public void Restart()
    {
        ActiveBattle = null;
        Player = new Player();
        _floorCache.Clear();
        Messages.Clear();
        QuestState.Clear();
        EnterFloor(1, EntryMode.Start);
    }

    /// <summary>向消息区追加系统提示并刷新界面</summary>
    public void Notify(string message) => PushMessage(message);

    /// <summary>生成存档快照：玩家属性 + 已访问楼层的增量状态（死亡怪物/已拾取道具/已开门）</summary>
    public GameSaveData CreateSave()
    {
        var save = new GameSaveData
        {
            SavedAt = DateTime.Now,
            Player = ClonePlayer(Player),
            QuestState = QuestState.ToDictionary(kv => kv.Key, kv => kv.Value),
        };

        foreach (var (number, floor) in _floorCache)
        {
            var fs = new FloorState { FloorNumber = number };
            foreach (var m in floor.MonstersOnFloor.Where(m => !m.IsAlive))
                fs.MonsterAlive[m.Id] = false;
            foreach (var it in floor.ItemsOnFloor.Where(i => i.PickedUp))
                fs.ItemPickedUp[it.Id] = true;
            for (int y = 0; y < floor.Height; y++)
                for (int x = 0; x < floor.Width; x++)
                    if (floor.GridRows[y][x] is 'r' or 'b' or 'y'
                        && floor.Tiles[x, y].TileType == TileType.Floor)
                        fs.DoorOpened[$"{x},{y}"] = true;
            save.Floors[number] = fs;
        }

        return save;
    }

    /// <summary>从存档快照恢复游戏状态（玩家属性 + 楼层增量缓存 + 位置）</summary>
    public void LoadFromSave(GameSaveData save)
    {
        if (save?.Player == null) return;

        int floorNumber = save.Player.CurrentFloor is >= 1 and <= MapConstants.TotalFloors
            ? save.Player.CurrentFloor
            : 1;

        Player = save.Player;
        // 上限引入前的存档血量可能远超 MaxHpCap，读档时收敛（HUD 与规则即刻一致）
        Player.MaxHp = Math.Min(Player.MaxHp, Player.MaxHpCap);
        Player.Hp = Math.Min(Player.Hp, Player.MaxHp);
        Player.VisitedFloors ??= new(); // 旧版 v1 存档无该字段，兜底初始化
        _floorCache.Clear();
        Messages.Clear();
        QuestState.Clear();
        foreach (var (questId, state) in save.QuestState ?? new())
            QuestState[questId] = state;
        foreach (var (number, fs) in save.Floors)
        {
            if (number is < 1 or > MapConstants.TotalFloors) continue;
            _floorCache[number] = GameData.LoadFloor(number, fs);
        }

        IsShopOpen = false;
        CurrentShop = GameData.GetShop(1);
        EnterFloor(floorNumber, EntryMode.Load);
    }

    /// <summary>克隆玩家快照，避免后续读档引用保存时仍被修改的对象</summary>
    private static Player ClonePlayer(Player p) => new()
    {
        Hp = p.Hp,
        MaxHp = p.MaxHp,
        Attack = p.Attack,
        Defense = p.Defense,
        Gold = p.Gold,
        RedKey = p.RedKey,
        BlueKey = p.BlueKey,
        YellowKey = p.YellowKey,
        CurrentFloor = p.CurrentFloor,
        PosX = p.PosX,
        PosY = p.PosY,
        IsDead = p.IsDead,
        IsWin = p.IsWin,
        KillByTemplate = new Dictionary<int, int>(p.KillByTemplate ?? new()),
        PickByTemplate = new Dictionary<int, int>(p.PickByTemplate ?? new()),
        IsHasFlyOrb = p.IsHasFlyOrb,
        VisitedFloors = new List<int>(p.VisitedFloors ?? new()),
    };

    /// <summary>尝试向 (dx, dy) 移动一步，返回是否发生了移动/交互</summary>
    public bool TryMove(int dx, int dy)
    {
        if (IsShopOpen || IsBattleOpen) return false;

        int nx = Player.PosX + dx;
        int ny = Player.PosY + dy;
        var tile = CurrentFloor.GetTile(nx, ny);
        if (tile == null || tile.TileType == TileType.Wall) return false;

        switch (tile.TileType)
        {
            case TileType.Monster:
                return Fight(nx, ny);
            case TileType.Item:
                PickUp(nx, ny);
                MoveTo(nx, ny);
                return true;
            case TileType.RedDoor:
            case TileType.BlueDoor:
            case TileType.YellowDoor:
                return OpenDoor(nx, ny);
            case TileType.StairUp:
                GoToFloor(1);
                return true;
            case TileType.StairDown:
                GoToFloor(-1);
                return true;
            case TileType.Shop:
            case TileType.NPC:
                return InteractAndMove(nx, ny);
            default:
                MoveTo(nx, ny);
                return true;
        }
    }

    /// <summary>尝试与 (x, y) 的 NPC/商店交互；守门 NPC 条件不足时不移动、返回 false</summary>
    private bool InteractAndMove(int x, int y)
    {
        if (!Interact(x, y)) return false;
        MoveTo(x, y);
        return true;
    }

    /// <summary>商店购买</summary>
    public bool BuyShopItem(ShopItem item)
    {
        if (!IsShopOpen) return false;
        if (Player.Gold < item.Price)
        {
            PushMessage("金币不足，无法购买！");
            StateChanged?.Invoke();
            return false;
        }

        Player.Gold -= item.Price;
        switch (item.Type)
        {
            case ItemType.Hp:
                Heal(item.Value);
                PushMessage($"购买{item.Name}，恢复 {item.Value} 生命。");
                break;
            case ItemType.Attack:
                Player.Attack += item.Value;
                PushMessage($"购买{item.Name}，攻击 +{item.Value}。");
                break;
            case ItemType.Defense:
                Player.Defense += item.Value;
                PushMessage($"购买{item.Name}，防御 +{item.Value}。");
                break;
            case ItemType.Key:
                AddKey(item.KeyType, item.Value);
                PushMessage($"购买{item.Name}。");
                break;
        }
        StateChanged?.Invoke();
        return true;
    }

    public void CloseShop()
    {
        if (!IsShopOpen) return;
        IsShopOpen = false;
        StateChanged?.Invoke();
    }

    // ---------- 核心逻辑 ----------

    private void EnterFloor(int floorNumber, EntryMode mode)
    {
        if (!_floorCache.TryGetValue(floorNumber, out var floor))
        {
            floor = GameData.LoadFloor(floorNumber);
            _floorCache[floorNumber] = floor;
        }

        CurrentFloor = floor;
        Player.CurrentFloor = floorNumber;
        Player.VisitedFloors ??= new();
        // 飞行器穿梭列表：只要踏入过该楼层即记录（含传送进入）
        if (!Player.VisitedFloors.Contains(floorNumber))
            Player.VisitedFloors.Add(floorNumber);

        switch (mode)
        {
            case EntryMode.Start:
                FindStartPoint(out int sx, out int sy);
                Player.PosX = sx;
                Player.PosY = sy;
                break;
            case EntryMode.FromBelow:
                FindTile(TileType.StairDown, out int dx, out int dy);
                Player.PosX = dx;
                Player.PosY = dy;
                break;
            case EntryMode.FromAbove:
                FindTile(TileType.StairUp, out int ux, out int uy);
                Player.PosX = ux;
                Player.PosY = uy;
                break;
            case EntryMode.Load:
                Player.PosX = Math.Clamp(Player.PosX, 0, floor.Width - 1);
                Player.PosY = Math.Clamp(Player.PosY, 0, floor.Height - 1);
                break;
            case EntryMode.FlyOrb:
                // 飞行器传送落点：目标楼层楼梯处（优先下楼楼梯 = 常规入口，其次上楼楼梯）
                if (!FindTile(TileType.StairDown, out int fx, out int fy)
                    && !FindTile(TileType.StairUp, out fx, out fy))
                    FindStartPoint(out fx, out fy);
                Player.PosX = fx;
                Player.PosY = fy;
                break;
        }

        PushMessage($"进入第 {floorNumber} 层。");
        FloorChanged?.Invoke();
    }

    private void MoveTo(int x, int y)
    {
        Player.PosX = x;
        Player.PosY = y;
        StateChanged?.Invoke();
    }

    private bool Fight(int x, int y)
    {
        if (IsBattleOpen) return false; // 已有战斗未结束，不再触发新战斗

        var tile = CurrentFloor.Tiles[x, y];
        var monster = CurrentFloor.MonstersOnFloor.FirstOrDefault(m => m.Id == tile.TargetId);
        if (monster == null || !monster.IsAlive)
        {
            tile.TileType = TileType.Floor;
            MoveTo(x, y);
            return true;
        }

        // 原版魔塔规则：无法破防（攻击 ≤ 怪物防御）视作撞墙，不进入战斗。
        if (!MathHelper.CanBreakDefense(monster, Player))
        {
            PushMessage($"无法破防 {monster.Name}：你的攻击 {Player.Attack} ≤ 怪物防御 {monster.Defense}，先提升攻击再来。");
            StateChanged?.Invoke();
            return false;
        }

        // 原版魔塔规则：触怪即战。创建战斗状态并立即开打（UI 逐回合推进动画）。
        _battleTile = (x, y);
        var sim = MathHelper.SimulateBattle(monster, Player);
        ActiveBattle = new ActiveBattleState
        {
            Monster = monster,
            MonsterHp = monster.Hp,
            PlayerHp = Player.Hp,
            PlayerDamagePerHit = MathHelper.PlayerDamagePerTurn(monster, Player),
            MonsterDamagePerHit = MathHelper.MonsterDamagePerTurn(monster, Player),
            Preview = sim,
            CanBreakDefense = true,
            IsBoss = MathHelper.IsBoss(monster),
            IsFixed = monster.Type == MonsterType.FixedDamage || monster.IgnoreDefense,
            IsDrain = monster.Type == MonsterType.DrainBlood,
        };
        BattleStarted?.Invoke();
        StateChanged?.Invoke();
        return false;
    }

    private void PickUp(int x, int y)
    {
        var tile = CurrentFloor.Tiles[x, y];
        var item = CurrentFloor.ItemsOnFloor.FirstOrDefault(i => i.Id == tile.TargetId);
        if (item == null) return;

        item.PickedUp = true;
        tile.TileType = TileType.Floor;
        Player.PickByTemplate.TryGetValue(item.TemplateId, out int picks);
        Player.PickByTemplate[item.TemplateId] = picks + 1;
        switch (item.Type)
        {
            case ItemType.Hp:
                Heal(item.Value);
                PushMessage($"拾取 {item.Name}，生命 +{item.Value}。");
                break;
            case ItemType.Attack:
                Player.Attack += item.Value;
                PushMessage($"拾取 {item.Name}，攻击 +{item.Value}。");
                break;
            case ItemType.Defense:
                Player.Defense += item.Value;
                PushMessage($"拾取 {item.Name}，防御 +{item.Value}。");
                break;
            case ItemType.Key:
                AddKey(item.KeyType, item.Value);
                PushMessage($"拾取 {item.Name}。");
                break;
            case ItemType.FlyOrb:
                // 飞行器：永久持有道具，拾取后从地图移除并进入背包（无消耗、不改属性）
                Player.IsHasFlyOrb = true;
                PushMessage("【捡到飞行器】这是一台古老的楼层穿梭飞行器，可以在塔内楼层之间快速穿梭。提示：可在楼层任意位置启动，只能前往你已经到达过的楼层。");
                break;
        }

        ItemPickedUp?.Invoke(item);
    }

    private bool OpenDoor(int x, int y)
    {
        var tile = CurrentFloor.Tiles[x, y];
        KeyType key = tile.TileType switch
        {
            TileType.RedDoor => KeyType.Red,
            TileType.BlueDoor => KeyType.Blue,
            TileType.YellowDoor => KeyType.Yellow,
            _ => throw new InvalidOperationException(),
        };

        if (KeyCount(key) <= 0)
        {
            PushMessage($"没有{key}钥匙，无法开门。");
            return false;
        }

        AddKey(key, -1);
        tile.TileType = TileType.Floor;
        PushMessage($"使用 {key}钥匙 打开门。");
        DoorOpened?.Invoke();
        MoveTo(x, y);
        return true;
    }

    private void GoToFloor(int delta)
    {
        int next = CurrentFloor.FloorNumber + delta;
        if (next < 1 || next > MapConstants.TotalFloors) return;
        EnterFloor(next, delta > 0 ? EntryMode.FromBelow : EntryMode.FromAbove);
    }

    // ---------- 飞行器（楼层穿梭机） ----------

    /// <summary>
    /// 使用飞行器前的校验：
    /// 1. 必须已持有飞行器；
    /// 2. 楼层任意位置均可启动（不要求站在楼梯格）；
    /// 3. 非商店/战斗状态。
    /// 通过返回 true；否则返回 false 并给出提示文案。
    /// </summary>
    public bool TryUseFlyOrb(out string reason)
    {
        reason = "";
        if (!Player.IsHasFlyOrb)
        {
            reason = "你还没有飞行器。1 层左下角可以找到它。";
            return false;
        }
        if (IsShopOpen || IsBattleOpen)
        {
            reason = "当前状态无法使用飞行器。";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 传送到已访问过的目标楼层：
    /// - 玩家直接出现在目标楼层楼梯处；
    /// - 不消耗 HP / 金币 / 钥匙，不刷新怪物、不改变道具与门状态；
    /// - 目标层必须是已访问楼层（VisitedFloors），否则拒绝。
    /// </summary>
    public bool FlyTo(int floorNumber)
    {
        if (!Player.IsHasFlyOrb) return false;
        if (floorNumber is < 1 or > MapConstants.TotalFloors) return false;
        if (!Player.VisitedFloors.Contains(floorNumber)) return false;
        if (IsShopOpen || IsBattleOpen) return false;

        PushMessage($"启动飞行器，穿梭至第 {floorNumber} 层……");
        EnterFloor(floorNumber, EntryMode.FlyOrb);
        FlyOrbTeleported?.Invoke();
        return true;
    }

    private bool Interact(int x, int y)
    {
        var ev = CurrentFloor.Events.FirstOrDefault(e => e.X == x && e.Y == y);
        if (ev == null) return true;

        // 守门 NPC（魔塔式进度门槛）：条件不足时拦下，不允许移动到此格。
        if (ev.IsGuard && !IsGuardRequirementMet(ev))
        {
            PushMessage($"{ev.Name}拦住你：{ev.Text}（需要 {RequirementText(ev)}）");
            StateChanged?.Invoke();
            return false;
        }

        if (ev.ShopId != null)
        {
            CurrentShop = GameData.GetShop(ev.ShopId);
            IsShopOpen = true;
            PushMessage($"进入商店「{ev.Name}」。{ev.Text}");
        }
        else
        {
            if (ev.IsGuard) PushMessage($"{ev.Name}让开道路：{ev.PassText}");
            PushMessage($"{ev.Name}：{ev.Text}");
            if (ev.QuestId is { } questId && !ev.IsGuard)
                HandleQuestNpc(questId);
            // 1F 老祭司：持有飞行器后追加穿梭机台词（原版设定）
            if (Player.IsHasFlyOrb && CurrentFloor.FloorNumber == 1)
                PushMessage($"{ev.Name}：这个飞行器是早年塔内建造者留下的穿梭装置，可以在楼层任意位置启动，但不能传送到未曾探索的区域。");
        }
        StateChanged?.Invoke();
        return true;
    }

    /// <summary>守门 NPC 通行条件是否满足</summary>
    private bool IsGuardRequirementMet(GameEvent ev) => ev.Requirement switch
    {
        GuardRequirement.Attack => Player.Attack >= ev.RequirementValue,
        GuardRequirement.Defense => Player.Defense >= ev.RequirementValue,
        GuardRequirement.Gold => Player.Gold >= ev.RequirementValue,
        GuardRequirement.RedKey => Player.RedKey >= ev.RequirementValue,
        GuardRequirement.BlueKey => Player.BlueKey >= ev.RequirementValue,
        GuardRequirement.YellowKey => Player.YellowKey >= ev.RequirementValue,
        GuardRequirement.FloorReached => Player.CurrentFloor >= ev.RequirementValue,
        _ => true,
    };

    /// <summary>守门条件的可读描述（用于消息提示）</summary>
    private static string RequirementText(GameEvent ev) => ev.Requirement switch
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

    /// <summary>
    /// 任务 NPC 交互逻辑：
    /// 未接取 → 自动接取；进行中 → 满足目标即交任务领奖励，否则提示进度；已完成 → 感谢语。
    /// </summary>
    private void HandleQuestNpc(int questId)
    {
        var quest = GameData.Quests.FirstOrDefault(q => q.Id == questId);
        if (quest == null) return;

        int state = QuestState.GetValueOrDefault(questId, 0);
        if (state == 2)
        {
            PushMessage($"任务「{quest.Name}」已完成：{quest.DoneText}");
            return;
        }

        if (state == 0)
        {
            QuestState[questId] = 1;
            PushMessage($"接取任务「{quest.Name}」：{quest.Hint}（当前进度 {QuestProgressText(quest)}）。去完成它再回来说一声吧。");
            return;
        }

        if (IsQuestComplete(quest))
        {
            CompleteQuest(quest);
        }
        else
        {
            PushMessage($"任务「{quest.Name}」进行中：{quest.Hint}（当前进度 {QuestProgressText(quest)}）。");
        }
    }

    /// <summary>任务目标是否达成</summary>
    private bool IsQuestComplete(Quest quest) => quest.Type switch
    {
        QuestTargetType.KillMonster => Player.KillByTemplate.TryGetValue(quest.TargetId, out int k) && k >= quest.Count,
        QuestTargetType.PickupItem => Player.PickByTemplate.TryGetValue(quest.TargetId, out int p) && p >= quest.Count,
        QuestTargetType.ReachFloor => Player.CurrentFloor >= quest.TargetId,
        _ => false,
    };

    /// <summary>任务进度的可读文本</summary>
    private string QuestProgressText(Quest quest) => quest.Type switch
    {
        QuestTargetType.KillMonster => $"{Player.KillByTemplate.GetValueOrDefault(quest.TargetId)}/{quest.Count}",
        QuestTargetType.PickupItem => $"{Player.PickByTemplate.GetValueOrDefault(quest.TargetId)}/{quest.Count}",
        QuestTargetType.ReachFloor => $"已至第 {Player.CurrentFloor} 层 / 目标第 {quest.TargetId} 层",
        _ => "",
    };

    /// <summary>完成任务并发放奖励</summary>
    private void CompleteQuest(Quest quest)
    {
        Player.Gold += quest.RewardGold;
        Player.Attack += quest.RewardAttack;
        Player.Defense += quest.RewardDefense;
        if (quest.RewardMaxHp > 0)
        {
            Player.MaxHp = Math.Min(Player.MaxHp + quest.RewardMaxHp, Player.MaxHpCap);
            Player.Hp = Math.Min(Player.MaxHp, Player.Hp + quest.RewardMaxHp);
        }

        QuestState[quest.Id] = 2;
        PushMessage($"任务「{quest.Name}」完成！{quest.RewardText}。");
    }

    /// <summary>
    /// 恢复生命：上限随当前生命同步抬高（HUD 显示 "当前 / 峰值"），
    /// 但峰值封顶于 MaxHpCap——超出部分浪费，不再无限累计。
    /// </summary>
    private void Heal(int amount)
    {
        Player.Hp += amount;
        Player.MaxHp = Math.Min(Math.Max(Player.MaxHp, Player.Hp), Player.MaxHpCap);
        Player.Hp = Math.Min(Player.Hp, Player.MaxHp);
    }

    private int KeyCount(KeyType t) => t switch
    {
        KeyType.Red => Player.RedKey,
        KeyType.Blue => Player.BlueKey,
        KeyType.Yellow => Player.YellowKey,
        _ => 0,
    };

    private void AddKey(KeyType t, int delta)
    {
        switch (t)
        {
            case KeyType.Red: Player.RedKey += delta; break;
            case KeyType.Blue: Player.BlueKey += delta; break;
            case KeyType.Yellow: Player.YellowKey += delta; break;
        }
    }

    /// <summary>初始进入时寻找出生点：优先楼层中部的地板瓦片</summary>
    private void FindStartPoint(out int x, out int y)
    {
        int cx = CurrentFloor.Width / 2;
        int cy = CurrentFloor.Height / 2;
        for (int r = 0; r <= Math.Max(cx, cy); r++)
        {
            for (int yy = Math.Max(0, cy - r); yy <= Math.Min(CurrentFloor.Height - 1, cy + r); yy++)
                for (int xx = Math.Max(0, cx - r); xx <= Math.Min(CurrentFloor.Width - 1, cx + r); xx++)
                    if (CurrentFloor.Tiles[xx, yy].TileType == TileType.Floor)
                    {
                        x = xx;
                        y = yy;
                        return;
                    }
        }

        FindTile(TileType.Floor, out x, out y);
    }

    /// <summary>查找指定类型瓦片的坐标（首个匹配）。找到返回 true；未找到时返回 false 并回退到 (1,1)。</summary>
    private bool FindTile(TileType type, out int x, out int y)
    {
        for (int yy = 0; yy < CurrentFloor.Height; yy++)
            for (int xx = 0; xx < CurrentFloor.Width; xx++)
                if (CurrentFloor.Tiles[xx, yy].TileType == type)
                {
                    x = xx;
                    y = yy;
                    return true;
                }
        x = 1;
        y = 1;
        return false;
    }

    private void PushMessage(string message)
    {
        Messages.Insert(0, message);
        if (Messages.Count > MaxMessages)
            Messages.RemoveRange(MaxMessages, Messages.Count - MaxMessages);
        StateChanged?.Invoke();
    }

    // ---------- 逐回合战斗状态机 ----------

    /// <summary>推进一个回合（玩家攻击 → 怪物反击 → 吸血回复），返回本回合后战斗是否已结束。</summary>
    public bool StepBattle()
    {
        var b = ActiveBattle;
        if (b == null || b.Ended) return true;

        b.Turn++;
        int playerHit = b.PlayerDamagePerHit;
        b.MonsterHp -= playerHit;

        if (b.MonsterHp <= 0)
        {
            b.Won = true;
            b.Ended = true;
            b.Steps.Add(new BattleStep
            {
                Turn = b.Turn,
                PlayerHit = playerHit,
                Text = $"第 {b.Turn} 回合：你造成 {playerHit} 伤害，击杀了 {b.Monster.Name}！",
            });
            BattleStepAdvanced?.Invoke();
            return true;
        }

        int monsterHit = b.MonsterDamagePerHit;
        b.PlayerHp -= monsterHit;
        int drained = 0;
        if (b.IsDrain && monsterHit > 0 && b.MonsterHp < b.Monster.Hp)
        {
            drained = Math.Min(monsterHit, b.Monster.Hp - b.MonsterHp);
            b.MonsterHp += drained;
        }

        string drainText = drained > 0 ? $"，{b.Monster.Name} 吸取 {drained} 生命" : "";
        b.Steps.Add(new BattleStep
        {
            Turn = b.Turn,
            PlayerHit = playerHit,
            MonsterHit = monsterHit,
            Drained = drained,
            Text = $"第 {b.Turn} 回合：你造成 {playerHit} 伤害{drainText}；{b.Monster.Name} 反击 {monsterHit} 伤害。怪 HP {b.MonsterHp} ／ 你 HP {b.PlayerHp}。",
        });

        if (b.PlayerHp <= 0)
        {
            b.Won = false;
            b.Ended = true;
        }
        BattleStepAdvanced?.Invoke();
        return b.Ended;
    }

    /// <summary>取消战斗面板（不结算、不移动；触怪即战流程下仅在异常兜底时使用）</summary>
    public void CancelBattle()
    {
        if (ActiveBattle == null) return;
        ActiveBattle = null;
        StateChanged?.Invoke();
        BattleEnded?.Invoke();
    }

    /// <summary>战斗结束结算：胜利 → 奖励 + 击杀 + 移动；失败 → 阵亡；撤退 → 无奖励。</summary>
    public void FinishBattle()
    {
        var b = ActiveBattle;
        if (b == null) return;
        ActiveBattle = null;

        if (b.Retreated)
        {
            StateChanged?.Invoke();
            BattleFinished?.Invoke();
            BattleEnded?.Invoke();
            return;
        }

        var tile = CurrentFloor.Tiles[_battleTile.X, _battleTile.Y];

        if (b.Won)
        {
            var monster = b.Monster;
            int actualLoss = Math.Max(0, Player.Hp - b.PlayerHp);
            Player.Hp = b.PlayerHp;
            Player.Gold += monster.GoldReward;
            Player.KillByTemplate.TryGetValue(monster.TemplateId, out int kills);
            Player.KillByTemplate[monster.TemplateId] = kills + 1;

            if (b.IsBoss)
            {
                Player.RedKey += 1; // GDD：BOSS 掉落红钥匙 ×1
                PushMessage($"击败 BOSS「{monster.Name}」！额外获得红钥匙 ×1。");
            }

            monster.IsAlive = false;
            // 恢复该格原始地形而非一律地板：历史数据中怪物可能站在楼梯格上，
            // 直接置为 Floor 会永久摧毁楼梯（如 80F BOSS 曾压在上楼楼梯上）
            tile.TileType = CurrentFloor.GridRows[_battleTile.Y][_battleTile.X] switch
            {
                'U' => TileType.StairUp,
                'D' => TileType.StairDown,
                _ => TileType.Floor,
            };
            PushMessage($"击败 {monster.Name}！耗时 {b.Turn} 回合，损失 {actualLoss} 生命，获得 {monster.GoldReward} 金币。");

            if (b.IsBoss && CurrentFloor.FloorNumber == MapConstants.TotalFloors)
                Player.IsWin = true; // 100F 魔王本体 → 通关

            BattleFinished?.Invoke();
            MoveTo(_battleTile.X, _battleTile.Y);
        }
        else
        {
            Player.Hp = 0;
            Player.IsDead = true;
            PushMessage($"你被 {b.Monster.Name} 击败了……");
            BattleFinished?.Invoke();
        }

        StateChanged?.Invoke();
        BattleEnded?.Invoke();
    }
}


/// <summary>进行中的逐回合战斗状态（由 GameEngine 维护）</summary>
public sealed class ActiveBattleState
{
    public required Monster Monster { get; init; }

    /// <summary>开战前预演战果（预估损血 / 是否可胜）</summary>
    public required BattleSimulation Preview { get; init; }

    /// <summary>怪物当前 HP（随回合推进变化）</summary>
    public int MonsterHp { get; set; }

    /// <summary>玩家当前 HP（随回合推进变化，最终结算写入 Player）</summary>
    public int PlayerHp { get; set; }

    /// <summary>玩家每回合伤害 = max(1, 玩家ATK − 怪物DEF)</summary>
    public int PlayerDamagePerHit { get; init; }

    /// <summary>怪物每回合反击伤害 = max(1, 怪物ATK − 玩家DEF)（魔法怪无视防御）</summary>
    public int MonsterDamagePerHit { get; init; }

    public int Turn { get; set; }

    /// <summary>玩家攻击力能否破防（≤ 怪物防御则禁止开战）</summary>
    public bool CanBreakDefense { get; init; }

    /// <summary>BOSS 战：禁止撤退</summary>
    public bool IsBoss { get; init; }

    /// <summary>魔法怪（固定伤害、无视防御）</summary>
    public bool IsFixed { get; init; }

    /// <summary>吸血怪（每轮反击后回复等量 HP）</summary>
    public bool IsDrain { get; init; }

    public bool Ended { get; set; }

    public bool Won { get; set; }

    public bool Retreated { get; set; }

    /// <summary>逐回合战报（最新在后）</summary>
    public List<BattleStep> Steps { get; } = new();

    /// <summary>战斗面板开场白（BOSS 附加提示）</summary>
    public string IntroText => IsBoss
        ? $"{Monster.Name} 挡住了去路！这是 BOSS 战，胜利将掉落红钥匙！"
        : $"遭遇 {Monster.Name}！";
}

/// <summary>一个战斗回合的战报</summary>
public sealed class BattleStep
{
    public int Turn { get; init; }

    /// <summary>玩家本回合造成伤害</summary>
    public int PlayerHit { get; init; }

    /// <summary>怪物本回合反击伤害（怪物被击杀的回合为 0）</summary>
    public int MonsterHit { get; init; }

    /// <summary>吸血怪本回合回复量</summary>
    public int Drained { get; init; }

    public string Text { get; init; } = "";
}

