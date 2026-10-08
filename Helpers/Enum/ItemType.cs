namespace Mota100Floors.Helpers.Enum;

/// <summary>道具类型</summary>
public enum ItemType
{
    /// <summary>加生命</summary>
    Hp,

    /// <summary>加攻击</summary>
    Attack,

    /// <summary>加防御</summary>
    Defense,

    /// <summary>钥匙</summary>
    Key,

    /// <summary>飞行器（永久持有道具，不消耗；拾取后进入背包，任意位置可传送至已探索楼层）</summary>
    FlyOrb,
}
