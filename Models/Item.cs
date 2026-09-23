namespace Mota100Floors.Models;

using Mota100Floors.Helpers.Enum;

/// <summary>道具（楼层实例）</summary>
public class Item
{
    /// <summary>楼层内实例 ID</summary>
    public int Id { get; set; }

    /// <summary>道具模板 ID（对应 Items.json）</summary>
    public int TemplateId { get; set; }

    public string Name { get; set; } = "";

    public string Desc { get; set; } = "";

    public ItemType Type { get; set; }

    /// <summary>当 Type == Key 时的钥匙类型</summary>
    public KeyType KeyType { get; set; }

    /// <summary>增益数值</summary>
    public int Value { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    /// <summary>是否已拾取</summary>
    public bool PickedUp { get; set; }
}
