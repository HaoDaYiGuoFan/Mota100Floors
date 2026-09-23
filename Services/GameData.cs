namespace Mota100Floors.Services;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mota100Floors.Helpers.Enum;
using Mota100Floors.Models;

/// <summary>数据加载：楼层 / 怪物模板 / 道具模板 / 商店配置</summary>
public static class GameData
{
    public static List<Monster> MonsterTemplates { get; } = new();

    public static List<Item> ItemTemplates { get; } = new();

    public static List<Quest> Quests { get; } = new();

    public static ShopConfig Shop { get; } = new();

    private static readonly string DataDir =
        Path.Combine(AppContext.BaseDirectory, "Data");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    static GameData()
    {
        MonsterTemplates = Load<List<Monster>>("Monsters.json") ?? new();
        ItemTemplates = Load<List<Item>>("Items.json") ?? new();
        Quests = Load<List<Quest>>("Quests.json") ?? new();
        Shop = Load<ShopConfig>("ShopConfig.json") ?? new ShopConfig();
    }

    private static T? Load<T>(string file) where T : class
    {
        string path = Path.Combine(DataDir, file);
        return File.Exists(path)
            ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions)
            : null;
    }

    /// <summary>
    /// 加载楼层并构建运行时瓦片（怪物/道具/事件覆盖到网格上）。
    /// 传入 <paramref name="state"/> 时先把存档增量（怪物存活/道具拾取/已开门）应用到实例，
    /// 使读档后的楼层与当时的游戏世界完全一致。
    /// </summary>
    public static FloorData LoadFloor(int floorNumber, FloorState? state = null)
    {
        string path = Path.Combine(DataDir, "Floors", $"Floor{floorNumber}.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"楼层数据缺失: {path}", path);
        var floor = JsonSerializer.Deserialize<FloorData>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException($"楼层数据解析失败: {path}");

        if (state != null)
        {
            foreach (var (id, alive) in state.MonsterAlive)
                if (floor.MonstersOnFloor.FirstOrDefault(m => m.Id == id) is { } m)
                    m.IsAlive = alive;
            foreach (var (id, picked) in state.ItemPickedUp)
                if (floor.ItemsOnFloor.FirstOrDefault(i => i.Id == id) is { } it)
                    it.PickedUp = picked;
        }

        // 楼层 JSON 只内联基础数值；美术序号 / 法师特性 / 特殊类型 / BOSS 标记以模板为准，统一回填，
        // 保证同一 TemplateId 的怪物在任意楼层显示与伤害行为完全一致。
        foreach (var m in floor.MonstersOnFloor)
        {
            if (MonsterTemplates.FirstOrDefault(t => t.Id == m.TemplateId) is { } tpl)
            {
                m.SpriteIndex = tpl.SpriteIndex;
                m.Type = tpl.Type;
                m.IgnoreDefense = tpl.IgnoreDefense || tpl.Type == MonsterType.FixedDamage;
                if (tpl.Type == MonsterType.Boss) m.IsBoss = true;
            }
        }

        floor.Tiles = new MapTile[floor.Width, floor.Height];
        for (int y = 0; y < floor.Height; y++)
            for (int x = 0; x < floor.Width; x++)
                floor.Tiles[x, y] = new MapTile { X = x, Y = y, TileType = CharToTile(floor.GridRows[y][x]) };

        // 已打开的门：网格原始字符仍是门，但运行时瓦片应变为地板
        if (state != null)
        {
            foreach (var (key, opened) in state.DoorOpened)
            {
                if (!opened) continue;
                if (key.Split(',') is { Length: 2 } parts
                    && int.TryParse(parts[0], out int dx) && int.TryParse(parts[1], out int dy)
                    && floor.GetTile(dx, dy) is { } dt)
                    dt.TileType = TileType.Floor;
            }
        }

        foreach (var m in floor.MonstersOnFloor.Where(m => m.IsAlive))
            if (floor.GetTile(m.X, m.Y) is { } t)
            {
                t.TileType = TileType.Monster;
                t.TargetId = m.Id;
            }

        foreach (var it in floor.ItemsOnFloor.Where(i => !i.PickedUp))
            if (floor.GetTile(it.X, it.Y) is { } t)
            {
                t.TileType = TileType.Item;
                t.TargetId = it.Id;
            }

        foreach (var ev in floor.Events)
            if (floor.GetTile(ev.X, ev.Y) is { } t)
                t.TileType = ev.ShopId != null ? TileType.Shop : TileType.NPC;

        return floor;
    }

    private static TileType CharToTile(char c) => c switch
    {
        'W' => TileType.Wall,
        'U' => TileType.StairUp,
        'D' => TileType.StairDown,
        'r' => TileType.RedDoor,
        'b' => TileType.BlueDoor,
        'y' => TileType.YellowDoor,
        _ => TileType.Floor,
    };
}
