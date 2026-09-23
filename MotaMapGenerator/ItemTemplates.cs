namespace MotaMapGenerator;

/// <summary>道具模板与商店配置</summary>
public static class ItemTemplates
{
    public static List<ItemTemplate> Build() => new()
    {
        // 生命药水
        new() { Id = 1, Name = "生命药水·小", Desc = "恢复 100 点生命", Type = "Hp", Value = 100 },
        new() { Id = 2, Name = "生命药水·大", Desc = "恢复 300 点生命", Type = "Hp", Value = 300 },
        new() { Id = 3, Name = "圣光药水", Desc = "恢复 1000 点生命", Type = "Hp", Value = 1000 },
        // 力量宝石
        new() { Id = 4, Name = "力量宝石·小", Desc = "攻击 +5", Type = "Attack", Value = 5 },
        new() { Id = 5, Name = "力量宝石·大", Desc = "攻击 +15", Type = "Attack", Value = 15 },
        new() { Id = 6, Name = "战神之石", Desc = "攻击 +40", Type = "Attack", Value = 40 },
        // 守护宝石
        new() { Id = 7, Name = "守护宝石·小", Desc = "防御 +5", Type = "Defense", Value = 5 },
        new() { Id = 8, Name = "守护宝石·大", Desc = "防御 +15", Type = "Defense", Value = 15 },
        new() { Id = 9, Name = "泰坦之盾", Desc = "防御 +40", Type = "Defense", Value = 40 },
        // 钥匙
        new() { Id = 10, Name = "黄钥匙", Desc = "可打开黄色门", Type = "Key", KeyType = "Yellow", Value = 1 },
        new() { Id = 11, Name = "蓝钥匙", Desc = "可打开蓝色门", Type = "Key", KeyType = "Blue", Value = 1 },
        new() { Id = 12, Name = "红钥匙", Desc = "可打开红色门", Type = "Key", KeyType = "Red", Value = 1 },
        // 飞行器（楼层穿梭机）：永久持有道具，1F 左下角拾取，不消耗
        new() { Id = 13, Name = "飞行器", Desc = "楼层穿梭机，在楼梯旁使用，跳转至已探索楼层，无使用次数", Type = "FlyOrb", KeyType = "Yellow", Value = 0 },
    };

    public static ShopConfigJson BuildShop() => new()
    {
        ShopId = 1,
        Items = new List<ShopItemConfig>
        {
            new() { Id = 1, Name = "生命 +300", Desc = "立即恢复 300 点生命（受上限约束）", Type = "Hp", Value = 300, Price = 100 },
            new() { Id = 2, Name = "攻击 +10", Desc = "永久增加 10 点攻击", Type = "Attack", Value = 10, Price = 100 },
            new() { Id = 3, Name = "防御 +10", Desc = "永久增加 10 点防御", Type = "Defense", Value = 10, Price = 100 },
            new() { Id = 4, Name = "黄钥匙 +1", Desc = "获得 1 把黄钥匙", Type = "Key", KeyType = "Yellow", Value = 1, Price = 10 },
            new() { Id = 5, Name = "蓝钥匙 +1", Desc = "获得 1 把蓝钥匙", Type = "Key", KeyType = "Blue", Value = 1, Price = 30 },
            new() { Id = 6, Name = "红钥匙 +1", Desc = "获得 1 把红钥匙", Type = "Key", KeyType = "Red", Value = 1, Price = 100 },
        },
    };
}
