namespace MotaMapGenerator;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class Program
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static int Main()
    {
        string dataDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Mota100Floors\Data"));
        string floorsDir = Path.Combine(dataDir, "Floors");
        Directory.CreateDirectory(floorsDir);

        Console.WriteLine($"输出目录: {dataDir}");

        var monsters = MonsterTemplates.Build();
        var items = ItemTemplates.Build();
        var shop = ItemTemplates.BuildShop();

        // 模板文件
        File.WriteAllText(Path.Combine(dataDir, "Monsters.json"), JsonSerializer.Serialize(monsters, JsonOpts));
        File.WriteAllText(Path.Combine(dataDir, "Items.json"), JsonSerializer.Serialize(items, JsonOpts));
        File.WriteAllText(Path.Combine(dataDir, "ShopConfig.json"), JsonSerializer.Serialize(shop, JsonOpts));
        Console.WriteLine($"怪物模板 {monsters.Count} 种，道具模板 {items.Count} 种，商店商品 {shop.Items.Count} 种。");

        // 逐层生成
        int prevUpX = 1, prevUpY = 1;
        var stats = new Dictionary<int, (int Monsters, int Items, int Doors)>();
        for (int f = 1; f <= 100; f++)
        {
            int tier = f <= 20 ? 0 : f <= 50 ? 1 : f <= 80 ? 2 : 3;
            var floor = FloorGenerator.Generate(f, tier, prevUpX, prevUpY, monsters, items);
            if (f == 1) FloorGenerator.ApplyFloor1Special(floor); // 1F 左下角飞行器

            // 记录本层 StairUp 坐标，供下一层 StairDown 入场点邻近
            (prevUpX, prevUpY) = FindTile(floor, 'U') ?? (prevUpX, prevUpY);

            string file = Path.Combine(floorsDir, $"Floor{f}.json");
            File.WriteAllText(file, JsonSerializer.Serialize(floor, JsonOpts));

            int doors = floor.GridRows.Sum(r => r.Count(c => c is 'r' or 'b' or 'y'));
            stats[f] = (floor.MonstersOnFloor.Count, floor.ItemsOnFloor.Count, doors);
            Console.WriteLine($"Floor {f:D3} 楼层生成完成 | 怪物 {floor.MonstersOnFloor.Count,2} | 道具 {floor.ItemsOnFloor.Count,2} | 门 {doors} | 事件 {floor.Events.Count}");
        }

        // 验证连通性与可通关性
        int fail = 0;
        for (int f = 1; f <= 100; f++)
        {
            var floor = JsonSerializer.Deserialize<FloorJson>(File.ReadAllText(Path.Combine(floorsDir, $"Floor{f}.json")), JsonOpts)!;
            if (!Validator.Validate(floor))
            {
                fail++;
                Console.WriteLine($"[警告] Floor{f} 校验未通过！");
            }
        }
        Console.WriteLine(fail == 0
            ? "全部 100 层校验通过：entry→StairUp 连通、门钥匙均在近侧可达。"
            : $"有 {fail} 层校验未通过，请检查。");
        Console.WriteLine("完成。");
        return 0;
    }

    private static (int, int)? FindTile(FloorJson floor, char ch)
    {
        for (int y = 0; y < floor.GridRows.Length; y++)
            for (int x = 0; x < floor.GridRows[y].Length; x++)
                if (floor.GridRows[y][x] == ch)
                    return (x, y);
        return null;
    }
}
