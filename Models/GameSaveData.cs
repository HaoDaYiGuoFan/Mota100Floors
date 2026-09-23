namespace Mota100Floors.Models;

/// <summary>存档模型：玩家全属性 + 100 层怪物/道具/门状态</summary>
public class GameSaveData
{
    /// <summary>当前存档格式版本（用于数据结构变更时的迁移）</summary>
    /// <remarks>v2：Player 新增 IsHasFlyOrb 与 VisitedFloors（飞行器穿梭列表）。旧 v1 档读取时字段缺省，
    /// 由 GameEngine.LoadFromSave 兜底初始化并记录当前楼层，无需手工迁移。</remarks>
    public const int CurrentVersion = 2;

    public int Version { get; set; } = CurrentVersion;

    public DateTime SavedAt { get; set; }

    public Player Player { get; set; } = new();

    /// <summary>任务进度：QuestId → 0 未接取 / 1 进行中 / 2 已完成</summary>
    public Dictionary<int, int> QuestState { get; set; } = new();

    /// <summary>key = 楼层号 1~100</summary>
    public Dictionary<int, FloorState> Floors { get; set; } = new();
}

/// <summary>单层持久化状态</summary>
public class FloorState
{
    public int FloorNumber { get; set; }

    /// <summary>怪物实例 Id → 是否存活</summary>
    public Dictionary<int, bool> MonsterAlive { get; set; } = new();

    /// <summary>道具实例 Id → 是否已拾取</summary>
    public Dictionary<int, bool> ItemPickedUp { get; set; } = new();

    /// <summary>"x,y" → 门是否已打开</summary>
    public Dictionary<string, bool> DoorOpened { get; set; } = new();
}
