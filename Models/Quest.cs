namespace Mota100Floors.Models;

using System.Text.Json.Serialization;
using Mota100Floors.Helpers.Enum;

/// <summary>任务目标类型</summary>
public enum QuestTargetType
{
    /// <summary>击杀指定模板怪物 TargetId 共 Count 只</summary>
    KillMonster,

    /// <summary>拾取指定模板道具 TargetId 共 Count 个</summary>
    PickupItem,

    /// <summary>抵达第 TargetId 层</summary>
    ReachFloor,
}

/// <summary>任务模板（对应 Data/Quests.json）</summary>
public class Quest
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    /// <summary>接取后展示的提示（含目标）</summary>
    public string Hint { get; set; } = "";

    /// <summary>完成后 NPC 的感谢语</summary>
    public string DoneText { get; set; } = "";

    public QuestTargetType Type { get; set; }

    public int TargetId { get; set; }

    public int Count { get; set; }

    public int RewardGold { get; set; }

    public int RewardAttack { get; set; }

    public int RewardDefense { get; set; }

    /// <summary>奖励生命上限（同时回复等量生命）</summary>
    public int RewardMaxHp { get; set; }

    /// <summary>完成提示（任务面板展示的奖励文案）</summary>
    public string RewardText { get; set; } = "";
}