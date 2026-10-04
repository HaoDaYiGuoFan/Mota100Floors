namespace MotaMapGenerator;

/// <summary>
/// 怪物模板池：完全对标原版魔塔（24 层经典版 V1.12）的怪物体系与数值。
/// 1~23 号为原版经典怪物，数值取自原版怪物手册（生命/攻击/防御/金币原样保留）；
/// 24~50 号为按原版成长曲线外推的同风格怪物（25~99 层）；
/// 51~60 号为原版风格的里程碑 BOSS（每 10 层一座，100 层最终 BOSS 冥灵魔王即原版 21 层版最终 BOSS 名）。
/// SpriteIndex = 贴图编号（Assets/Tiles/monster_{id}.png，原版风格像素画）。
/// Magic = 法师系：魔法攻击无视玩家防御（原版「法师系魔法攻击」规则）。
/// </summary>
public static class MonsterTemplates
{
    public static List<MonsterTemplate> Build() => new()
    {
        // ================= 原版经典怪物（数值 = 24 层经典版怪物手册） =================
        new() { Id = 1,  Name = "绿色史莱姆", Hp = 35,  Attack = 18,  Defense = 1,   GoldReward = 2,   MinFloor = 1,  MaxFloor = 12, SpriteIndex = 1 },
        new() { Id = 2,  Name = "红色史莱姆", Hp = 45,  Attack = 20,  Defense = 2,   GoldReward = 3,   MinFloor = 1,  MaxFloor = 14, SpriteIndex = 2 },
        new() { Id = 3,  Name = "小蝙蝠",     Hp = 50,  Attack = 18,  Defense = 4,   GoldReward = 4,   MinFloor = 1,  MaxFloor = 14, SpriteIndex = 3 },
        new() { Id = 4,  Name = "黑色史莱姆", Hp = 60,  Attack = 24,  Defense = 4,   GoldReward = 6,   MinFloor = 3,  MaxFloor = 16, SpriteIndex = 4 },
        new() { Id = 5,  Name = "初级法师",   Hp = 40,  Attack = 28,  Defense = 2,   GoldReward = 8,   MinFloor = 5,  MaxFloor = 18, SpriteIndex = 5, IgnoreDefense = true, Type = "FixedDamage" },
        new() { Id = 6,  Name = "骷髅人",     Hp = 110, Attack = 26,  Defense = 6,   GoldReward = 10,  MinFloor = 4,  MaxFloor = 18, SpriteIndex = 6 },
        new() { Id = 7,  Name = "大蝙蝠",     Hp = 120, Attack = 33,  Defense = 8,   GoldReward = 15,  MinFloor = 7,  MaxFloor = 20, SpriteIndex = 7 },
        new() { Id = 8,  Name = "大史莱姆",   Hp = 120, Attack = 28,  Defense = 12,  GoldReward = 20,  MinFloor = 9,  MaxFloor = 22, SpriteIndex = 8 },
        new() { Id = 9,  Name = "骷髅士兵",   Hp = 150, Attack = 35,  Defense = 10,  GoldReward = 20,  MinFloor = 10, MaxFloor = 22, SpriteIndex = 9 },
        new() { Id = 10, Name = "兽人",       Hp = 220, Attack = 45,  Defense = 12,  GoldReward = 35,  MinFloor = 12, MaxFloor = 24, SpriteIndex = 10 },
        new() { Id = 11, Name = "初级卫兵",   Hp = 180, Attack = 40,  Defense = 14,  GoldReward = 30,  MinFloor = 13, MaxFloor = 24, SpriteIndex = 11 },
        new() { Id = 12, Name = "红蝙蝠",     Hp = 200, Attack = 45,  Defense = 20,  GoldReward = 35,  MinFloor = 14, MaxFloor = 26, SpriteIndex = 12 },
        new() { Id = 13, Name = "骷髅队长",   Hp = 240, Attack = 55,  Defense = 16,  GoldReward = 40,  MinFloor = 15, MaxFloor = 27, SpriteIndex = 13 },
        new() { Id = 14, Name = "麻衣法师",   Hp = 125, Attack = 45,  Defense = 15,  GoldReward = 40,  MinFloor = 16, MaxFloor = 28, SpriteIndex = 14, IgnoreDefense = true, Type = "FixedDamage" },
        new() { Id = 15, Name = "高级法师",   Hp = 80,  Attack = 48,  Defense = 8,   GoldReward = 28,  MinFloor = 17, MaxFloor = 28, SpriteIndex = 15, IgnoreDefense = true, Type = "FixedDamage" },
        new() { Id = 16, Name = "中级卫兵",   Hp = 300, Attack = 65,  Defense = 25,  GoldReward = 60,  MinFloor = 18, MaxFloor = 30, SpriteIndex = 16 },
        new() { Id = 17, Name = "大乌鸦",     Hp = 150, Attack = 55,  Defense = 20,  GoldReward = 55,  MinFloor = 19, MaxFloor = 30, SpriteIndex = 17 },
        new() { Id = 18, Name = "石头人",     Hp = 300, Attack = 60,  Defense = 25,  GoldReward = 60,  MinFloor = 20, MaxFloor = 32, SpriteIndex = 18 },
        new() { Id = 19, Name = "白衣武士",   Hp = 100, Attack = 60,  Defense = 20,  GoldReward = 40,  MinFloor = 21, MaxFloor = 32, SpriteIndex = 19 },
        new() { Id = 20, Name = "兽人武士",   Hp = 400, Attack = 70,  Defense = 30,  GoldReward = 90,  MinFloor = 22, MaxFloor = 34, SpriteIndex = 20 },
        new() { Id = 21, Name = "双手剑士",   Hp = 200, Attack = 65,  Defense = 30,  GoldReward = 80,  MinFloor = 23, MaxFloor = 34, SpriteIndex = 21 },
        new() { Id = 22, Name = "僵尸",       Hp = 300, Attack = 55,  Defense = 15,  GoldReward = 45,  MinFloor = 23, MaxFloor = 34, SpriteIndex = 22 },
        new() { Id = 23, Name = "高级卫兵",   Hp = 450, Attack = 90,  Defense = 35,  GoldReward = 100, MinFloor = 24, MaxFloor = 36, SpriteIndex = 23 },

        // ================= 中段外推怪物（25~50 层，原版风格） =================
        new() { Id = 24, Name = "骷髅勇士",   Hp = 500,  Attack = 95,  Defense = 30,  GoldReward = 120, MinFloor = 25, MaxFloor = 42, SpriteIndex = 24 },
        new() { Id = 25, Name = "血翼蝙蝠",   Hp = 450,  Attack = 100, Defense = 25,  GoldReward = 110, MinFloor = 25, MaxFloor = 42, SpriteIndex = 25 },
        new() { Id = 26, Name = "石头巨人",   Hp = 600,  Attack = 105, Defense = 45,  GoldReward = 140, MinFloor = 27, MaxFloor = 46, SpriteIndex = 26 },
        new() { Id = 27, Name = "邪恶法师",   Hp = 350,  Attack = 110, Defense = 20,  GoldReward = 130, MinFloor = 28, MaxFloor = 46, SpriteIndex = 27, IgnoreDefense = true, Type = "FixedDamage" },
        new() { Id = 28, Name = "食人魔",     Hp = 900,  Attack = 125, Defense = 40,  GoldReward = 180, MinFloor = 29, MaxFloor = 48, SpriteIndex = 28 },
        new() { Id = 29, Name = "白袍法师",   Hp = 420,  Attack = 130, Defense = 30,  GoldReward = 160, MinFloor = 30, MaxFloor = 48, SpriteIndex = 29, IgnoreDefense = true, Type = "FixedDamage" },
        new() { Id = 30, Name = "青铜卫兵",   Hp = 650,  Attack = 120, Defense = 50,  GoldReward = 160, MinFloor = 31, MaxFloor = 50, SpriteIndex = 30 },
        new() { Id = 31, Name = "大剑士",     Hp = 700,  Attack = 130, Defense = 55,  GoldReward = 180, MinFloor = 33, MaxFloor = 50, SpriteIndex = 31 },
        new() { Id = 32, Name = "红衣法师",   Hp = 380,  Attack = 145, Defense = 25,  GoldReward = 190, MinFloor = 35, MaxFloor = 48, SpriteIndex = 32, IgnoreDefense = true, Type = "FixedDamage" },
        new() { Id = 33, Name = "银甲卫兵",   Hp = 800,  Attack = 140, Defense = 60,  GoldReward = 200, MinFloor = 37, MaxFloor = 50, SpriteIndex = 33 },

        // ================= 高段外推怪物（51~80 层，原版风格） =================
        new() { Id = 34, Name = "暗黑骑士",   Hp = 1500, Attack = 240, Defense = 110, GoldReward = 150, MinFloor = 51, MaxFloor = 66, SpriteIndex = 34 },
        new() { Id = 35, Name = "烈焰法师",   Hp = 1300, Attack = 200, Defense = 90,  GoldReward = 160, MinFloor = 51, MaxFloor = 66, SpriteIndex = 35, IgnoreDefense = true, Type = "FixedDamage" },
        new() { Id = 36, Name = "黄金卫兵",   Hp = 1900, Attack = 280, Defense = 130, GoldReward = 180, MinFloor = 53, MaxFloor = 70, SpriteIndex = 36 },
        new() { Id = 37, Name = "岩石巨人",   Hp = 2200, Attack = 290, Defense = 160, GoldReward = 200, MinFloor = 55, MaxFloor = 72, SpriteIndex = 37 },
        new() { Id = 38, Name = "骷髅王",     Hp = 2000, Attack = 310, Defense = 120, GoldReward = 200, MinFloor = 57, MaxFloor = 74, SpriteIndex = 38 },
        new() { Id = 39, Name = "冰霜法师",   Hp = 1400, Attack = 260, Defense = 100, GoldReward = 220, MinFloor = 59, MaxFloor = 76, SpriteIndex = 39, IgnoreDefense = true, Type = "FixedDamage" },
        new() { Id = 40, Name = "魔王卫队",   Hp = 2500, Attack = 340, Defense = 150, GoldReward = 230, MinFloor = 61, MaxFloor = 78, SpriteIndex = 40 },
        new() { Id = 41, Name = "双手剑圣",   Hp = 3000, Attack = 360, Defense = 170, GoldReward = 250, MinFloor = 63, MaxFloor = 80, SpriteIndex = 41 },
        new() { Id = 42, Name = "黑骑士团长", Hp = 3500, Attack = 390, Defense = 190, GoldReward = 280, MinFloor = 65, MaxFloor = 80, SpriteIndex = 42 },

        // ================= 终局外推怪物（81~99 层，原版风格） =================
        new() { Id = 43, Name = "冥灵武士",   Hp = 4000, Attack = 600, Defense = 250, GoldReward = 250, MinFloor = 81, MaxFloor = 94, SpriteIndex = 43 },
        new() { Id = 44, Name = "大法师",     Hp = 3800, Attack = 480, Defense = 200, GoldReward = 260, MinFloor = 81, MaxFloor = 94, SpriteIndex = 44, IgnoreDefense = true, Type = "FixedDamage" },
        new() { Id = 45, Name = "圣殿卫士",   Hp = 4800, Attack = 680, Defense = 300, GoldReward = 280, MinFloor = 83, MaxFloor = 96, SpriteIndex = 45 },
        new() { Id = 46, Name = "冥灵队长",   Hp = 5500, Attack = 730, Defense = 320, GoldReward = 300, MinFloor = 85, MaxFloor = 98, SpriteIndex = 46 },
        new() { Id = 47, Name = "骨龙",       Hp = 7000, Attack = 780, Defense = 350, GoldReward = 340, MinFloor = 87, MaxFloor = 98, SpriteIndex = 47 },
        new() { Id = 48, Name = "灭世法师",   Hp = 6000, Attack = 600, Defense = 270, GoldReward = 350, MinFloor = 89, MaxFloor = 99, SpriteIndex = 48, IgnoreDefense = true, Type = "FixedDamage" },
        new() { Id = 49, Name = "魔王亲卫",   Hp = 8000, Attack = 840, Defense = 380, GoldReward = 380, MinFloor = 91, MaxFloor = 99, SpriteIndex = 49 },
        new() { Id = 50, Name = "混沌魔将",   Hp = 9500, Attack = 920, Defense = 400, GoldReward = 410, MinFloor = 93, MaxFloor = 99, SpriteIndex = 50 },

        // ================= 里程碑 BOSS（每 10 层，原版风格） =================
        new() { Id = 51, Name = "骷髅将军",   Hp = 300,  Attack = 60,  Defense = 25,  GoldReward = 80,  MinFloor = 10, MaxFloor = 10, SpriteIndex = 51, Type = "Boss", IsBoss = true },
        new() { Id = 52, Name = "兽人酋长",   Hp = 800,  Attack = 110, Defense = 45,  GoldReward = 200, MinFloor = 20, MaxFloor = 20, SpriteIndex = 52, Type = "Boss", IsBoss = true },
        new() { Id = 53, Name = "魔王",       Hp = 1200, Attack = 150, Defense = 70,  GoldReward = 300, MinFloor = 30, MaxFloor = 30, SpriteIndex = 53, Type = "Boss", IsBoss = true },
        new() { Id = 54, Name = "黄金卫士",   Hp = 2400, Attack = 230, Defense = 120, GoldReward = 380, MinFloor = 40, MaxFloor = 40, SpriteIndex = 54, Type = "Boss", IsBoss = true },
        new() { Id = 55, Name = "魔龙",       Hp = 3200, Attack = 280, Defense = 140, GoldReward = 250, MinFloor = 50, MaxFloor = 50, SpriteIndex = 55, Type = "Boss", IsBoss = true },
        new() { Id = 56, Name = "骨龙王",     Hp = 4200, Attack = 340, Defense = 160, GoldReward = 300, MinFloor = 60, MaxFloor = 60, SpriteIndex = 56, Type = "Boss", IsBoss = true },
        new() { Id = 57, Name = "炎魔",       Hp = 5000, Attack = 390, Defense = 190, GoldReward = 350, MinFloor = 70, MaxFloor = 70, SpriteIndex = 57, Type = "Boss", IsBoss = true },
        new() { Id = 58, Name = "暗黑魔龙",   Hp = 6000, Attack = 450, Defense = 220, GoldReward = 400, MinFloor = 80, MaxFloor = 80, SpriteIndex = 58, Type = "Boss", IsBoss = true },
        new() { Id = 59, Name = "魔神",       Hp = 7500, Attack = 520, Defense = 260, GoldReward = 500, MinFloor = 90, MaxFloor = 90, SpriteIndex = 59, Type = "Boss", IsBoss = true },
        new() { Id = 60, Name = "冥灵魔王",   Hp = 12000, Attack = 600, Defense = 320, GoldReward = 0,  MinFloor = 100, MaxFloor = 100, SpriteIndex = 60, Type = "Boss", IsBoss = true },
    };
}
