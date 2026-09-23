namespace Mota100Floors.Helpers.Enum;

/// <summary>瓦片类型</summary>
public enum TileType
{
    /// <summary>墙，不可通行</summary>
    Wall,

    /// <summary>地板，可通行</summary>
    Floor,

    /// <summary>上楼楼梯</summary>
    StairUp,

    /// <summary>下楼楼梯</summary>
    StairDown,

    /// <summary>红门</summary>
    RedDoor,

    /// <summary>蓝门</summary>
    BlueDoor,

    /// <summary>黄门</summary>
    YellowDoor,

    /// <summary>怪物</summary>
    Monster,

    /// <summary>道具</summary>
    Item,

    /// <summary>NPC 人物</summary>
    NPC,

    /// <summary>商店</summary>
    Shop,
}
