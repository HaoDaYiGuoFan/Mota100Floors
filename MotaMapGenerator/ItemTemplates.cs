namespace MotaMapGenerator;

/// <summary>
/// 道具模板与商店配置（对标原版魔塔资源体系）：
/// 原版命名与数值——红药水 +100 生命、蓝药水 +250~300 生命、红宝石 +2 攻击、蓝宝石 +2 防御；
/// 2~4 档宝石/药水为按层数成长的外推档位（25/51/81 层起投放），命名沿用原版风格。
/// </summary>
public static class ItemTemplates
{
    public static List<ItemTemplate> Build() => new()
    {
        // 生命药水（原版：红药水/蓝药水；后期为大血瓶/圣血瓶外推档）
        new() { Id = 1, Name = "红药水", Desc = "恢复 100 点生命", Type = "Hp", Value = 100 },
        new() { Id = 2, Name = "蓝药水", Desc = "恢复 300 点生命", Type = "Hp", Value = 300 },
        new() { Id = 3, Name = "大血瓶", Desc = "恢复 1000 点生命", Type = "Hp", Value = 1000 },
        new() { Id = 4, Name = "圣血瓶", Desc = "恢复 3000 点生命", Type = "Hp", Value = 3000 },
        // 红宝石：攻击（原版 +2 起步，后段为 100 层外推档）
        new() { Id = 5, Name = "红宝石",     Desc = "攻击 +2",  Type = "Attack", Value = 2 },
        new() { Id = 6, Name = "大红宝石",   Desc = "攻击 +4",  Type = "Attack", Value = 4 },
        new() { Id = 7, Name = "巨红宝石",   Desc = "攻击 +7",  Type = "Attack", Value = 7 },
        new() { Id = 8, Name = "神之红宝石", Desc = "攻击 +16", Type = "Attack", Value = 16 },
        // 蓝宝石：防御（原版 +2 起步，成长略低于攻击，贴近原版攻强防稳的构筑节奏）
        new() { Id = 9,  Name = "蓝宝石",     Desc = "防御 +2", Type = "Defense", Value = 2 },
        new() { Id = 10, Name = "大蓝宝石",   Desc = "防御 +3", Type = "Defense", Value = 3 },
        new() { Id = 11, Name = "巨蓝宝石",   Desc = "防御 +4", Type = "Defense", Value = 4 },
        new() { Id = 12, Name = "神之蓝宝石", Desc = "防御 +6", Type = "Defense", Value = 6 },
        // 钥匙
        new() { Id = 13, Name = "黄钥匙", Desc = "可打开黄色门", Type = "Key", KeyType = "Yellow", Value = 1 },
        new() { Id = 14, Name = "蓝钥匙", Desc = "可打开蓝色门", Type = "Key", KeyType = "Blue", Value = 1 },
        new() { Id = 15, Name = "红钥匙", Desc = "可打开红色门", Type = "Key", KeyType = "Red", Value = 1 },
        // 飞行器（楼层穿梭机）：永久持有道具，1F 左下角拾取，不消耗
        new() { Id = 16, Name = "飞行器", Desc = "楼层穿梭机，在楼梯旁使用，跳转至已探索楼层，无使用次数", Type = "FlyOrb", KeyType = "Yellow", Value = 0 },
    };

    /// <summary>
    /// 商店（原版风格定价，按楼层段位分 4 档：ShopId 1~4 对应 1~24 / 25~50 / 51~80 / 81~100 层）。
    /// 金币产出随楼层增长，价格同步上调，保证「每层金币 ≈ 1~3 次属性投资」的原版节奏。
    /// </summary>
    public static List<ShopConfigJson> BuildShops() => new()
    {
        new() // ShopId 1：新手段（1~24 层）
        {
            ShopId = 1,
            Items = new List<ShopItemConfig>
            {
                new() { Id = 1, Name = "生命 +200", Desc = "立即恢复 200 点生命", Type = "Hp", Value = 200, Price = 30 },
                new() { Id = 2, Name = "攻击 +2",   Desc = "永久增加 2 点攻击", Type = "Attack", Value = 2, Price = 50 },
                new() { Id = 3, Name = "防御 +2",   Desc = "永久增加 2 点防御", Type = "Defense", Value = 2, Price = 60 },
                new() { Id = 4, Name = "黄钥匙 +1", Desc = "获得 1 把黄钥匙", Type = "Key", KeyType = "Yellow", Value = 1, Price = 25 },
                new() { Id = 5, Name = "蓝钥匙 +1", Desc = "获得 1 把蓝钥匙", Type = "Key", KeyType = "Blue", Value = 1, Price = 80 },
                new() { Id = 6, Name = "红钥匙 +1", Desc = "获得 1 把红钥匙", Type = "Key", KeyType = "Red", Value = 1, Price = 160 },
            },
        },
        new() // ShopId 2：中层段（25~50 层）
        {
            ShopId = 2,
            Items = new List<ShopItemConfig>
            {
                new() { Id = 1, Name = "生命 +500", Desc = "立即恢复 500 点生命", Type = "Hp", Value = 500, Price = 80 },
                new() { Id = 2, Name = "攻击 +4",   Desc = "永久增加 4 点攻击", Type = "Attack", Value = 4, Price = 250 },
                new() { Id = 3, Name = "防御 +3",   Desc = "永久增加 3 点防御", Type = "Defense", Value = 3, Price = 300 },
                new() { Id = 4, Name = "黄钥匙 +1", Desc = "获得 1 把黄钥匙", Type = "Key", KeyType = "Yellow", Value = 1, Price = 60 },
                new() { Id = 5, Name = "蓝钥匙 +1", Desc = "获得 1 把蓝钥匙", Type = "Key", KeyType = "Blue", Value = 1, Price = 180 },
                new() { Id = 6, Name = "红钥匙 +1", Desc = "获得 1 把红钥匙", Type = "Key", KeyType = "Red", Value = 1, Price = 360 },
            },
        },
        new() // ShopId 3：高段（51~80 层）
        {
            ShopId = 3,
            Items = new List<ShopItemConfig>
            {
                new() { Id = 1, Name = "生命 +2000", Desc = "立即恢复 2000 点生命", Type = "Hp", Value = 2000, Price = 300 },
                new() { Id = 2, Name = "攻击 +7",    Desc = "永久增加 7 点攻击", Type = "Attack", Value = 7, Price = 900 },
                new() { Id = 3, Name = "防御 +5",    Desc = "永久增加 5 点防御", Type = "Defense", Value = 5, Price = 1100 },
                new() { Id = 4, Name = "黄钥匙 +1",  Desc = "获得 1 把黄钥匙", Type = "Key", KeyType = "Yellow", Value = 1, Price = 250 },
                new() { Id = 5, Name = "蓝钥匙 +1",  Desc = "获得 1 把蓝钥匙", Type = "Key", KeyType = "Blue", Value = 1, Price = 700 },
                new() { Id = 6, Name = "红钥匙 +1",  Desc = "获得 1 把红钥匙", Type = "Key", KeyType = "Red", Value = 1, Price = 1400 },
            },
        },
        new() // ShopId 4：终局段（81~100 层）
        {
            ShopId = 4,
            Items = new List<ShopItemConfig>
            {
                new() { Id = 1, Name = "生命 +8000", Desc = "立即恢复 8000 点生命", Type = "Hp", Value = 8000, Price = 1200 },
                new() { Id = 2, Name = "攻击 +16",   Desc = "永久增加 16 点攻击", Type = "Attack", Value = 16, Price = 3200 },
                new() { Id = 3, Name = "防御 +8",    Desc = "永久增加 8 点防御", Type = "Defense", Value = 8, Price = 4000 },
                new() { Id = 4, Name = "黄钥匙 +1",  Desc = "获得 1 把黄钥匙", Type = "Key", KeyType = "Yellow", Value = 1, Price = 900 },
                new() { Id = 5, Name = "蓝钥匙 +1",  Desc = "获得 1 把蓝钥匙", Type = "Key", KeyType = "Blue", Value = 1, Price = 2600 },
                new() { Id = 6, Name = "红钥匙 +1",  Desc = "获得 1 把红钥匙", Type = "Key", KeyType = "Red", Value = 1, Price = 5200 },
            },
        },
    };
}
