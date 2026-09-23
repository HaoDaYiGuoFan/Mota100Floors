namespace Mota100Floors.Helpers;

using Mota100Floors.Helpers.Enum;
using Mota100Floors.Models;

/// <summary>碰撞/边界判定辅助</summary>
public static class TileCollisionHelper
{
    /// <summary>坐标是否在地图边界内</summary>
    public static bool IsInBounds(FloorData floor, int x, int y)
        => x >= 0 && y >= 0 && x < floor.Width && y < floor.Height;

    /// <summary>该瓦片是否允许玩家站上去（怪物/门等在交互后也会变为可站立）</summary>
    public static bool IsTerrainWalkable(TileType type)
        => type is TileType.Floor
            or TileType.StairUp or TileType.StairDown
            or TileType.Item or TileType.NPC or TileType.Shop
            or TileType.Monster
            or TileType.RedDoor or TileType.BlueDoor or TileType.YellowDoor;
}
