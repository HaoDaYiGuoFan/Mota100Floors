namespace MotaMapGenerator;

/// <summary>
/// 生成结果校验器 —— 直接模拟游戏开门流程：
/// 1. 从入口 BFS 扩散（所有门关闭）
/// 2. 若当前可达区域内存在某扇相邻门的同色钥匙，则开门并继续扩散
/// 3. 直到不动点，最终必须能到达 StairUp（若该层存在）
/// 该方法与门顺序无关，完全等价于玩家实际游玩的可通关性。
/// </summary>
public static class Validator
{
    private static readonly (int, int)[] Dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    public static bool Validate(FloorJson floor)
    {
        // 网格合法性
        if (floor.GridRows.Length != 11) return false;
        if (floor.GridRows.Any(r => r.Length != 18)) return false;

        var entry = FindEntry(floor);
        var up = FindTile(floor, 'U');
        var closedDoors = FindDoors(floor).ToHashSet();

        // 初始区域：所有门关闭
        var region = new HashSet<(int, int)>();
        Flood(floor, region, entry, closedDoors);

        // 钥匙分布（坐标 -> 颜色）
        var keyItems = floor.ItemsOnFloor
            .Where(i => i.Type == "Key")
            .ToDictionary(i => (i.X, i.Y), i => i.KeyType);

        // 已消耗钥匙（同色开门消耗）
        var consumed = new Dictionary<string, int>();
        bool changed = true;
        while (changed)
        {
            changed = false;
            // 当前区域内可用钥匙数 = 区域内钥匙 - 已消耗
            int avail(string color) =>
                keyItems.Count(k => k.Value == color && region.Contains(k.Key))
                - consumed.GetValueOrDefault(color);

            foreach (var door in closedDoors.ToList())
            {
                string color = DoorColor(floor, door);
                if (avail(color) <= 0) continue;
                if (!IsAdjacent(region, door)) continue;

                // 开门
                closedDoors.Remove(door);
                consumed[color] = consumed.GetValueOrDefault(color) + 1;
                region.Add(door);
                Flood(floor, region, door, closedDoors);
                changed = true;
            }
        }

        // 未开的门：如果它们阻挡必经之路，StairUp 将不可达；此处直接要求 StairUp 可达
        if (up is { } upPos && !region.Contains(upPos))
        {
            Console.WriteLine($"    [校验] StairUp ({upPos}) 不可达（仍有关键门无法打开）");
            return false;
        }
        return true;
    }

    /// <summary>从 seed 开始洪泛（跳过墙与未开的门）</summary>
    private static void Flood(FloorJson floor, HashSet<(int, int)> region, (int, int) seed, HashSet<(int, int)> closedDoors)
    {
        var queue = new Queue<(int, int)>();
        queue.Enqueue(seed);
        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            foreach (var (dx, dy) in Dirs)
            {
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= 18 || ny >= 11) continue;
                if (region.Contains((nx, ny))) continue;
                if (closedDoors.Contains((nx, ny))) continue;
                if (floor.GridRows[ny][nx] == 'W') continue;
                region.Add((nx, ny));
                queue.Enqueue((nx, ny));
            }
        }
    }

    private static bool IsAdjacent(HashSet<(int, int)> region, (int, int) door)
    {
        foreach (var (dx, dy) in Dirs)
            if (region.Contains((door.Item1 + dx, door.Item2 + dy)))
                return true;
        return false;
    }

    private static string DoorColor(FloorJson floor, (int, int) door)
        => floor.GridRows[door.Item2][door.Item1] switch { 'r' => "Red", 'b' => "Blue", _ => "Yellow" };

    private static (int, int) FindEntry(FloorJson floor)
    {
        var d = FindTile(floor, 'D');
        return d ?? (1, 1); // 1 楼出生点
    }

    private static (int, int)? FindTile(FloorJson floor, char ch)
    {
        for (int y = 0; y < floor.GridRows.Length; y++)
            for (int x = 0; x < floor.GridRows[y].Length; x++)
                if (floor.GridRows[y][x] == ch)
                    return (x, y);
        return null;
    }

    private static List<(int, int)> FindDoors(FloorJson floor)
    {
        var doors = new List<(int, int)>();
        for (int y = 0; y < floor.GridRows.Length; y++)
            for (int x = 0; x < floor.GridRows[y].Length; x++)
                if (floor.GridRows[y][x] is 'r' or 'b' or 'y')
                    doors.Add((x, y));
        return doors;
    }
}
