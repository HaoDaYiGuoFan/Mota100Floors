namespace Mota100Floors.Models;

using Mota100Floors.Helpers.Enum;

/// <summary>怪物（楼层实例）</summary>
public class Monster
{
    /// <summary>楼层内实例 ID（用于存档状态映射）</summary>
    public int Id { get; set; }

    /// <summary>怪物模板 ID（对应 Monsters.json）</summary>
    public int TemplateId { get; set; }

    public string Name { get; set; } = "";

    public int Hp { get; set; }

    public int Attack { get; set; }

    public int Defense { get; set; }

    /// <summary>击杀奖励金币</summary>
    public int GoldReward { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    /// <summary>是否存活</summary>
    public bool IsAlive { get; set; } = true;

    /// <summary>是否最终 BOSS</summary>
    public bool IsBoss { get; set; }

    /// <summary>难度档次（0~3）</summary>
    public int Tier { get; set; }

    /// <summary>美术素材序号（1~6），对应 Assets/Tiles/monster_classic_{n}.png（换皮后按此显示怪物）</summary>
    public int SpriteIndex { get; set; } = 1;

    /// <summary>魔法系怪物：攻击无视玩家防御（伤害 = 攻击力）</summary>
    public bool IgnoreDefense { get; set; }

    /// <summary>
    /// 怪物特殊类型（Normal / FixedDamage / DrainBlood / Boss）。
    /// 由怪物模板（Monsters.json）回填到楼层实例；影响伤害公式、吸血与撤退规则。
    /// </summary>
    public MonsterType Type { get; set; } = MonsterType.Normal;

    /// <summary>是否为 BOSS（禁止撤退；击败后额外掉落红钥匙）。由楼层数据或模板 Boss 类型标记。</summary>
    public bool IsBossTag => IsBoss || Type == MonsterType.Boss;
}
