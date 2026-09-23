namespace Mota100Floors.Models;

/// <summary>商店配置（对应 ShopConfig.json）</summary>
public class ShopConfig
{
    public int ShopId { get; set; } = 1;

    public List<ShopItem> Items { get; set; } = new();
}

/// <summary>商店商品（在道具基础上增加价格）</summary>
public class ShopItem : Item
{
    public int Price { get; set; }
}
