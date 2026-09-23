namespace MotaMapGenerator;

/// <summary>楼层 JSON 输出模型（与游戏 FloorData 字段对应）</summary>
public class FloorJson
{
    public int FloorNumber { get; set; }
    public int Width { get; set; } = 18;
    public int Height { get; set; } = 11;

    /// <summary>W=墙 . =地板 U=上楼 D=下楼 r/b/y=红/蓝/黄门</summary>
    public string[] GridRows { get; set; } = Array.Empty<string>();

    public List<MonsterJson> MonstersOnFloor { get; set; } = new();
    public List<ItemJson> ItemsOnFloor { get; set; } = new();
    public List<EventJson> Events { get; set; } = new();
}

public class MonsterJson
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string Name { get; set; } = "";
    public int Hp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int GoldReward { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsAlive { get; set; } = true;
    public bool IsBoss { get; set; }
    public int Tier { get; set; }
}

public class ItemJson
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string Name { get; set; } = "";
    public string Desc { get; set; } = "";
    public string Type { get; set; } = "Hp";   // Hp/Attack/Defense/Key
    public string KeyType { get; set; } = "Yellow";
    public int Value { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool PickedUp { get; set; }
}

public class EventJson
{
    public int Id { get; set; }
    public string Type { get; set; } = "TalkNPC"; // TalkNPC / Shop
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; } = "";
    public string Text { get; set; } = "";
    public int? ShopId { get; set; }
}

/// <summary>怪物模板（写入 Monsters.json）</summary>
public class MonsterTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Hp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int GoldReward { get; set; }
    public int Tier { get; set; }
    public bool IsBoss { get; set; }
}

/// <summary>道具模板（写入 Items.json）</summary>
public class ItemTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Desc { get; set; } = "";
    public string Type { get; set; } = "Hp";
    public string KeyType { get; set; } = "Yellow";
    public int Value { get; set; }
}

/// <summary>商店商品配置（写入 ShopConfig.json）</summary>
public class ShopItemConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Desc { get; set; } = "";
    public string Type { get; set; } = "Hp";
    public string KeyType { get; set; } = "Yellow";
    public int Value { get; set; }
    public int Price { get; set; }
}

public class ShopConfigJson
{
    public int ShopId { get; set; } = 1;
    public List<ShopItemConfig> Items { get; set; } = new();
}
