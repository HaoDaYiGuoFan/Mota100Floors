namespace Mota100Floors.Models;

using Mota100Floors.Helpers.Enum;

/// <summary>事件模型（NPC 对话 / 守门 NPC / 商店 / 任务 NPC 等）</summary>
public class GameEvent
{
    public int Id { get; set; }

    public GameEventType Type { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    /// <summary>NPC 名称</summary>
    public string Name { get; set; } = "";

    /// <summary>NPC 对话文本（守门 NPC 为拦下时的台词）</summary>
    public string Text { get; set; } = "";

    /// <summary>商店配置 ID（对应 ShopConfig.json 的 Id）</summary>
    public int? ShopId { get; set; }

    /// <summary>
    /// 是否守门 NPC（魔塔式进度门槛）：
    /// 为 true 时，玩家条件不足会被拦下（无法移动到此格），条件满足后方可通过。
    /// </summary>
    public bool IsGuard { get; set; }

    /// <summary>通行条件类型（配合 IsGuard 使用）</summary>
    public GuardRequirement Requirement { get; set; }

    /// <summary>通行条件数值</summary>
    public int RequirementValue { get; set; }

    /// <summary>条件满足后放行时的台词</summary>
    public string PassText { get; set; } = "";

    /// <summary>关联的任务 ID（对应 Data/Quests.json）</summary>
    public int? QuestId { get; set; }
}
