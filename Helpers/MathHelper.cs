namespace Mota100Floors.Helpers;

using Mota100Floors.Helpers.Enum;
using Mota100Floors.Models;

/// <summary>
/// 魔塔战斗伤害计算工具（对齐《魔塔100层 GDD V1.0》第 2 节）。
/// 公式：
///   玩家单次伤害 = max(1, 玩家ATK − 怪物DEF)；玩家 ATK ≤ 怪物 DEF 时无法破防、禁止开战；
///   怪物单次伤害 = max(1, 怪物ATK − 玩家DEF)；魔法怪（FixedDamage / IgnoreDefense）无视防御 → max(1, 怪物ATK)；
///   所需回合 = ceil(怪物HP ÷ 玩家单次伤害)；
///   玩家总损血 = (回合数 − 1) × 怪物单次伤害（玩家先手，怪物少反击一次）。
/// 吸血怪（DrainBlood）每轮反击命中后恢复等量 HP，需逐回合模拟求真实战果。
/// </summary>
public static class MathHelper
{
    /// <summary>怪物对玩家单回合伤害（魔法怪无视玩家防御）</summary>
    public static int MonsterDamagePerTurn(Monster m, Player p)
        => m.Type == MonsterType.FixedDamage || m.IgnoreDefense
            ? Math.Max(1, m.Attack)
            : Math.Max(1, m.Attack - p.Defense);

    /// <summary>玩家对怪物单回合伤害</summary>
    public static int PlayerDamagePerTurn(Monster m, Player p)
        => Math.Max(1, p.Attack - m.Defense);

    /// <summary>能否破防：玩家攻击必须高于怪物防御（玩家伤害 ≥ 1）</summary>
    public static bool CanBreakDefense(Monster m, Player p)
        => p.Attack > m.Defense;

    /// <summary>击杀所需回合（吸血怪回血不影响轮次：怪物反击回的血在下一轮玩家攻击时先被打掉）</summary>
    public static int TurnsToKill(Monster m, Player p)
        => (int)Math.Ceiling(m.Hp / (double)Math.Max(1, PlayerDamagePerTurn(m, p)));

    /// <summary>预估玩家本场损失（吸血怪逐回合模拟；普通怪等价于 (回合数−1) × 单次伤害）</summary>
    public static int EstimatedPlayerDamage(Monster m, Player p)
        => SimulateBattle(m, p).PlayerDamageTaken;

    /// <summary>能否无伤/不死击杀（战果为胜利）</summary>
    public static bool CanKill(Monster m, Player p)
        => SimulateBattle(m, p).IsVictory;

    /// <summary>
    /// 逐回合模拟战斗（玩家先手）。用于战斗预估与必死判定，与真实回合流程保持一致：
    /// 每轮：玩家攻击 → 怪物死亡则胜利；否则怪物反击 → 玩家死亡则失败 → 吸血怪回复等量 HP。
    /// </summary>
    public static BattleSimulation SimulateBattle(Monster m, Player p)
    {
        var sim = new BattleSimulation();
        if (!CanBreakDefense(m, p)) return sim; // 无法破防：必败

        int mHp = m.Hp;
        int pHp = p.Hp;
        int pDmg = PlayerDamagePerTurn(m, p);

        for (int turn = 1; turn <= 200000; turn++)
        {
            sim.Turns = turn;
            mHp -= pDmg;
            if (mHp <= 0)
            {
                sim.IsVictory = true;
                return sim;
            }

            int mDmg = MonsterDamagePerTurn(m, p);
            pHp -= mDmg;
            sim.PlayerDamageTaken += mDmg;
            if (pHp <= 0) return sim; // 玩家死亡，战斗失败

            if (m.Type == MonsterType.DrainBlood && mDmg > 0)
            {
                int heal = Math.Min(mDmg, m.Hp - mHp);
                mHp += heal;
                sim.MonsterHealed += heal;
            }
        }

        // 循环上限兜底：吸血抵消大于玩家输出时无法击杀
        return sim;
    }

    /// <summary>该怪物是否禁止撤退（BOSS）</summary>
    public static bool IsBoss(Monster m) => m.IsBoss || m.Type == MonsterType.Boss;
}

/// <summary>一次战斗的预演结果</summary>
public sealed class BattleSimulation
{
    /// <summary>玩家是否最终击杀怪物（未死亡且已破防）</summary>
    public bool IsVictory { get; set; }

    /// <summary>玩家全程受到的伤害合计</summary>
    public int PlayerDamageTaken { get; set; }

    /// <summary>吸血怪全程回复的 HP 合计</summary>
    public int MonsterHealed { get; set; }

    /// <summary>战斗轮次（玩家攻击次数）</summary>
    public int Turns { get; set; }
}

