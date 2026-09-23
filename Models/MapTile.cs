namespace Mota100Floors.Models;

using Mota100Floors.Helpers.Enum;

/// <summary>地图瓦片</summary>
public class MapTile
{
    public TileType TileType { get; set; }

    /// <summary>怪物/道具实例 ID</summary>
    public int TargetId { get; set; }

    public bool Visible { get; set; } = true;

    public int X { get; set; }

    public int Y { get; set; }
}
