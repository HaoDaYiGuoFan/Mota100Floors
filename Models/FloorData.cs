namespace Mota100Floors.Models;

using System.Text.Json.Serialization;
using Mota100Floors.Helpers;

/// <summary>单层地图数据</summary>
public class FloorData
{
    /// <summary>楼层号 1~100</summary>
    public int FloorNumber { get; set; }

    public int Width { get; set; } = MapConstants.MapWidth;

    public int Height { get; set; } = MapConstants.MapHeight;

    /// <summary>
    /// 紧凑地形网格（每行一个字符串，每字符一种地形码）：
    /// W=墙 . =地板 U=上楼 D=下楼 r/b/y=红/蓝/黄门
    /// </summary>
    public string[] GridRows { get; set; } = Array.Empty<string>();

    /// <summary>该层怪物实例</summary>
    public List<Monster> MonstersOnFloor { get; set; } = new();

    /// <summary>该层道具实例</summary>
    public List<Item> ItemsOnFloor { get; set; } = new();

    /// <summary>该层 NPC / 商店事件</summary>
    public List<GameEvent> Events { get; set; } = new();

    /// <summary>运行时瓦片二维数组 [x, y]（不入档）</summary>
    [JsonIgnore]
    public MapTile[,] Tiles { get; set; } = new MapTile[0, 0];

    /// <summary>入场点坐标（到达本层时玩家的落点）</summary>
    [JsonIgnore]
    public int EntryX { get; set; }

    [JsonIgnore]
    public int EntryY { get; set; }

    public MapTile? GetTile(int x, int y)
        => x >= 0 && y >= 0 && x < Width && y < Height ? Tiles[x, y] : null;
}
