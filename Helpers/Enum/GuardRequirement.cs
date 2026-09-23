namespace Mota100Floors.Helpers.Enum;

/// <summary>守门 NPC 的通行条件类型（魔塔式进度门槛）</summary>
public enum GuardRequirement
{
    /// <summary>无门槛，仅对话</summary>
    None,

    /// <summary>攻击力 ≥ RequirementValue</summary>
    Attack,

    /// <summary>防御力 ≥ RequirementValue</summary>
    Defense,

    /// <summary>金币 ≥ RequirementValue</summary>
    Gold,

    /// <summary>红钥匙 ≥ RequirementValue（只检测不消耗）</summary>
    RedKey,

    /// <summary>蓝钥匙 ≥ RequirementValue（只检测不消耗）</summary>
    BlueKey,

    /// <summary>黄钥匙 ≥ RequirementValue（只检测不消耗）</summary>
    YellowKey,

    /// <summary>已抵达的楼层数 ≥ RequirementValue</summary>
    FloorReached,
}