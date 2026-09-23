namespace Mota100Floors.Helpers.Enum;

/// <summary>
/// 怪物特殊类型（对齐《魔塔100层 GDD V1.0》第 4 节「怪物特殊机制说明」）。
/// 类型决定战斗公式分支与交互规则。
/// </summary>
public enum MonsterType
{
    /// <summary>普通怪：无特殊效果，使用基础战斗公式。</summary>
    Normal = 0,

    /// <summary>魔法怪（固定伤害）：攻击无视玩家 DEF，伤害 = 怪物 ATK。</summary>
    FixedDamage = 1,

    /// <summary>吸血怪：每一轮攻击命中玩家后，恢复本次造成伤害等量的 HP，不超过怪物最大 HP。</summary>
    DrainBlood = 2,

    /// <summary>BOSS：禁止撤退，击败后额外掉落红钥匙。</summary>
    Boss = 3,
}
