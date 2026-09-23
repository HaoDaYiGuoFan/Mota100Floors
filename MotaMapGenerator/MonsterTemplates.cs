namespace MotaMapGenerator;

/// <summary>
/// 怪物模板池：按 4 段难度曲线递进（0=1~20 层，1=21~50 层，2=51~80 层，3=81~99 层），
/// 第 41 号为 100 层最终 BOSS。
/// </summary>
public static class MonsterTemplates
{
    public static List<MonsterTemplate> Build() => new()
    {
        // ---- Tier 0：新手层（1~20 层） ----
        new() { Id = 1, Name = "小蝙蝠", Hp = 15, Attack = 5, Defense = 0, GoldReward = 2, Tier = 0 },
        new() { Id = 2, Name = "史莱姆", Hp = 30, Attack = 8, Defense = 1, GoldReward = 4, Tier = 0 },
        new() { Id = 3, Name = "骷髅兵", Hp = 50, Attack = 12, Defense = 3, GoldReward = 8, Tier = 0 },
        new() { Id = 4, Name = "幽灵", Hp = 40, Attack = 15, Defense = 5, GoldReward = 10, Tier = 0 },
        new() { Id = 5, Name = "巨型蜘蛛", Hp = 100, Attack = 18, Defense = 6, GoldReward = 12, Tier = 0 },
        new() { Id = 6, Name = "兽人步兵", Hp = 80, Attack = 20, Defense = 8, GoldReward = 15, Tier = 0 },
        new() { Id = 7, Name = "骷髅队长", Hp = 120, Attack = 25, Defense = 10, GoldReward = 20, Tier = 0 },
        new() { Id = 8, Name = "石像鬼", Hp = 150, Attack = 30, Defense = 15, GoldReward = 25, Tier = 0 },
        new() { Id = 9, Name = "蝙蝠王", Hp = 180, Attack = 35, Defense = 18, GoldReward = 30, Tier = 0 },
        new() { Id = 10, Name = "铁甲骑士", Hp = 220, Attack = 40, Defense = 22, GoldReward = 40, Tier = 0 },

        // ---- Tier 1：中层（21~50 层） ----
        new() { Id = 11, Name = "暗影刺客", Hp = 200, Attack = 45, Defense = 20, GoldReward = 50, Tier = 1 },
        new() { Id = 12, Name = "沼泽巨蟒", Hp = 300, Attack = 55, Defense = 28, GoldReward = 65, Tier = 1 },
        new() { Id = 13, Name = "狼人", Hp = 260, Attack = 60, Defense = 30, GoldReward = 70, Tier = 1 },
        new() { Id = 14, Name = "巫妖", Hp = 350, Attack = 70, Defense = 35, GoldReward = 85, Tier = 1 },
        new() { Id = 15, Name = "石魔像", Hp = 450, Attack = 65, Defense = 50, GoldReward = 90, Tier = 1 },
        new() { Id = 16, Name = "火焰恶魔", Hp = 400, Attack = 80, Defense = 40, GoldReward = 110, Tier = 1 },
        new() { Id = 17, Name = "暗黑骑士", Hp = 550, Attack = 90, Defense = 55, GoldReward = 130, Tier = 1 },
        new() { Id = 18, Name = "闪电法师", Hp = 500, Attack = 95, Defense = 45, GoldReward = 140, Tier = 1 },
        new() { Id = 19, Name = "深渊守卫", Hp = 700, Attack = 105, Defense = 60, GoldReward = 160, Tier = 1 },
        new() { Id = 20, Name = "龙族幼崽", Hp = 800, Attack = 115, Defense = 65, GoldReward = 180, Tier = 1 },

        // ---- Tier 2：高层（51~80 层） ----
        new() { Id = 21, Name = "岩石巨魔", Hp = 900, Attack = 120, Defense = 70, GoldReward = 190, Tier = 2 },
        new() { Id = 22, Name = "地狱犬", Hp = 850, Attack = 130, Defense = 65, GoldReward = 200, Tier = 2 },
        new() { Id = 23, Name = "狂暴兽人", Hp = 1000, Attack = 145, Defense = 80, GoldReward = 220, Tier = 2 },
        new() { Id = 24, Name = "水晶灵体", Hp = 1100, Attack = 135, Defense = 90, GoldReward = 230, Tier = 2 },
        new() { Id = 25, Name = "邪眼魔", Hp = 1300, Attack = 155, Defense = 95, GoldReward = 260, Tier = 2 },
        new() { Id = 26, Name = "钢铁傀儡", Hp = 1500, Attack = 150, Defense = 120, GoldReward = 270, Tier = 2 },
        new() { Id = 27, Name = "暗影魔王", Hp = 1700, Attack = 175, Defense = 110, GoldReward = 300, Tier = 2 },
        new() { Id = 28, Name = "冰霜巨龙", Hp = 2000, Attack = 190, Defense = 130, GoldReward = 340, Tier = 2 },
        new() { Id = 29, Name = "炼狱领主", Hp = 2400, Attack = 210, Defense = 140, GoldReward = 380, Tier = 2 },
        new() { Id = 30, Name = "虚空行者", Hp = 2800, Attack = 230, Defense = 150, GoldReward = 420, Tier = 2 },

        // ---- Tier 3：终局层（81~99 层） ----
        new() { Id = 31, Name = "深渊恶魔", Hp = 3000, Attack = 245, Defense = 160, GoldReward = 450, Tier = 3 },
        new() { Id = 32, Name = "混沌魔裔", Hp = 3500, Attack = 270, Defense = 175, GoldReward = 500, Tier = 3 },
        new() { Id = 33, Name = "末日使者", Hp = 4000, Attack = 300, Defense = 190, GoldReward = 560, Tier = 3 },
        new() { Id = 34, Name = "上古石像", Hp = 4800, Attack = 290, Defense = 230, GoldReward = 600, Tier = 3 },
        new() { Id = 35, Name = "暗影主宰", Hp = 5500, Attack = 330, Defense = 210, GoldReward = 660, Tier = 3 },
        new() { Id = 36, Name = "灭世邪龙", Hp = 6500, Attack = 360, Defense = 240, GoldReward = 720, Tier = 3 },
        new() { Id = 37, Name = "冥府判官", Hp = 7500, Attack = 400, Defense = 260, GoldReward = 800, Tier = 3 },
        new() { Id = 38, Name = "混沌之主", Hp = 9000, Attack = 440, Defense = 280, GoldReward = 900, Tier = 3 },
        new() { Id = 39, Name = "深渊之主", Hp = 11000, Attack = 490, Defense = 310, GoldReward = 1000, Tier = 3 },
        new() { Id = 40, Name = "魔神", Hp = 13000, Attack = 540, Defense = 340, GoldReward = 1100, Tier = 3 },

        // ---- 100 层最终 BOSS ----
        new() { Id = 41, Name = "魔王·加坦杰厄", Hp = 60000, Attack = 800, Defense = 400, GoldReward = 0, Tier = 3, IsBoss = true },
    };
}
