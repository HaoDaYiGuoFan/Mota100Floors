namespace MotaMapGenerator;

/// <summary>
/// 楼层地图生成器：
/// 1. 房间 + 走廊（链式连通，保证 entry→StairUp 可达）
/// 2. 走廊中点放置门（门颜色按难度），钥匙放在门的“近侧区域”
/// 3. 怪物按距入口的 BFS 距离分配难度，生成在房间/壁龛内
/// 4. 道具（生命/攻防/钥匙）散布
/// 5. 楼梯：上一层 StairUp 坐标作为本层 StairDown 入场点（邻近）
/// </summary>
public static class FloorGenerator
{
    public const int W = 18;
    public const int H = 11;

    private static readonly Random Rng = new(20260918);

    private static readonly (int, int)[] Dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    private sealed class Room
    {
        public int X0, Y0, X1, Y1;
        public List<(int, int)> Cells = new();
        public (int, int) Center => ((X0 + X1) / 2, (Y0 + Y1) / 2);
    }

    public static FloorJson Generate(int floorNumber, int tier, int prevUpX, int prevUpY, List<MonsterTemplate> monsters, List<ItemTemplate> items)
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            var floor = TryGenerate(floorNumber, tier, prevUpX, prevUpY, monsters, items);
            if (floor != null)
                return floor;
        }
        throw new InvalidOperationException($"楼层 {floorNumber} 生成失败（无法保证可通关连通性）");
    }

    private static FloorJson? TryGenerate(int floorNumber, int tier, int prevUpX, int prevUpY, List<MonsterTemplate> monsters, List<ItemTemplate> items)
    {
        char[,] g = new char[W, H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                g[x, y] = 'W';

        // ---- 1. 房间 ----
        var rooms = new List<Room>();
        // 强制入场房间：覆盖入场点
        int entryX, entryY;
        if (floorNumber == 1) { entryX = 1; entryY = 1; }
        else { entryX = Math.Clamp(prevUpX, 1, W - 2); entryY = Math.Clamp(prevUpY, 1, H - 2); }

        var entryRoom = CarveRoom(g, rooms, entryX, entryY, Rng.Next(3, 5), Rng.Next(2, 4));
        entryRoom.X0 = Math.Max(1, entryX - 1);
        entryRoom.Y0 = Math.Max(1, entryY - 1);
        entryRoom.X1 = Math.Min(W - 2, entryX + 1);
        entryRoom.Y1 = Math.Min(H - 2, entryY + 1);
        entryRoom.Cells = FloorRect(g, entryRoom.X0, entryRoom.Y0, entryRoom.X1, entryRoom.Y1);

        int roomCount = Rng.Next(5, 8);
        for (int i = 0; i < roomCount; i++)
        {
            int rw = Rng.Next(3, 6);
            int rh = Rng.Next(2, 4);
            int rx = Rng.Next(1, W - rw - 1);
            int ry = Rng.Next(1, H - rh - 1);
            if (CanPlace(rooms, rx, ry, rx + rw - 1, ry + rh - 1))
                rooms.Add(CarveRoom(g, rooms, rx + rw / 2, ry + rh / 2, rw, rh));
        }
        if (rooms.Count < 3)
            return null;

        // ---- 2. 链式走廊 ----
        var ordered = new List<Room> { entryRoom };
        var rest = rooms.Where(r => r != entryRoom).ToList();
        rest.Sort((a, b) =>
        {
            double da = Dist(a.Center, entryRoom.Center);
            double db = Dist(b.Center, entryRoom.Center);
            return da.CompareTo(db);
        });
        ordered.AddRange(rest);

        var chainCorridors = new List<List<(int, int)>>(); // 每段走廊的单元格
        for (int i = 0; i < ordered.Count - 1; i++)
        {
            var seg = ConnectRooms(g, ordered[i], ordered[i + 1]);
            if (seg == null) return null;
            chainCorridors.Add(seg);
        }

        // ---- 3. 壁龛（死胡同宝藏/怪物点） ----
        int alcoveCount = Rng.Next(2, 5);
        var alcoveCells = new List<(int, int)>();
        for (int i = 0; i < alcoveCount; i++)
        {
            var room = ordered[Rng.Next(ordered.Count)];
            var start = room.Cells[Rng.Next(room.Cells.Count)];
            var alcove = CarveAlcove(g, start, Rng.Next(2, 4));
            if (alcove.Count > 0) alcoveCells.AddRange(alcove);
        }

        // ---- 4. 楼梯 ----
        bool hasDown = floorNumber > 1;
        bool hasUp = floorNumber < 100;
        if (hasDown) g[entryX, entryY] = 'D';
        else if (g[entryX, entryY] == 'W') g[entryX, entryY] = '.';

        // StairUp：最远房间内距离 entry 最远的自由格
        int upX = entryX, upY = entryY;
        if (hasUp)
        {
            var far = FarthestFreeCell(g, entryX, entryY);
            upX = far.Item1; upY = far.Item2;
            g[upX, upY] = 'U';
        }


        // ---- 5. 门（走廊中点）与钥匙 ----
        var gateCells = new List<(int, int)>();
        var doorColors = new List<char>();
        int nGates = tier == 0 ? 1 : tier == 1 ? (floorNumber % 2 == 0 ? 2 : 1) : tier == 2 ? 2 : Rng.Next(2, 4);
        nGates = Math.Min(nGates, chainCorridors.Count);

        for (int i = 0; i < nGates; i++)
        {
            var seg = chainCorridors[i];
            var mid = seg[seg.Count / 2];
            char color = i switch
            {
                0 => 'y',
                1 => tier >= 1 ? 'b' : 'y',
                _ => tier >= 2 ? 'r' : (tier >= 1 ? 'b' : 'y'),
            };
            // 近侧区域：绕过该门与后续门，从 entry BFS
            var near = RegionBfs(g, entryX, entryY, gateCells.Concat(new[] { mid }).ToList());
            // 需要足够的近侧空间放置钥匙
            if (near.Count < 6 || near.Contains((upX, upY)))
            {
                // 该门太靠近入口或无法放置钥匙则跳过（仍可放置后续门）
                continue;
            }
            gateCells.Add(mid);
            doorColors.Add(color);
            g[mid.Item1, mid.Item2] = color;
        }

        // 钥匙实体生成
        var usedCells = new HashSet<(int, int)> { (entryX, entryY), (upX, upY) };
        var keyChar = new Dictionary<char, string> { ['y'] = "Yellow", ['b'] = "Blue", ['r'] = "Red" };
        var keyTpl = items.Where(t => t.Type == "Key").ToList();
        var keyEntities = new List<(int X, int Y, string KeyType)>();
        for (int i = 0; i < gateCells.Count; i++)
        {
            var blocked = gateCells.Skip(i).ToList();
            var region = RegionBfs(g, entryX, entryY, blocked);
            region.RemoveWhere(usedCells.Contains);
            region.RemoveWhere(c => g[c.Item1, c.Item2] is 'D' or 'U' or 'W');
            if (region.Count == 0) continue;
            var keyCell = region.ElementAt(Rng.Next(region.Count));
            usedCells.Add(keyCell);
            keyEntities.Add((keyCell.Item1, keyCell.Item2, keyChar[doorColors[i]]));
        }


        // ---- 6. 怪物 ----
        var freeCells = FreeCells(g).ToList();
        freeCells.RemoveAll(c => usedCells.Contains(c));
        freeCells.RemoveAll(c => c == (entryX, entryY) || (c.Item1 == upX && c.Item2 == upY));
        // 排除链式走廊（关键通道不放怪）
        var chainSet = chainCorridors.SelectMany(s => s).ToHashSet();
        freeCells.RemoveAll(chainSet.Contains);

        var tierMonsters = monsters.Where(m => m.Tier == tier && !m.IsBoss).ToList();
        var lowerMonsters = monsters.Where(m => m.Tier == Math.Max(0, tier - 1) && !m.IsBoss).ToList();
        var distOrder = freeCells
            .Select(c => (Cell: c, Dist: BfsDist(g, entryX, entryY, c.Item1, c.Item2)))
            .Where(t => t.Dist >= 0)
            .OrderBy(t => t.Dist)
            .ToList();

        int monsterCount = Math.Min(distOrder.Count, Rng.Next(8, 15) + tier * 2);
        var monstersOnFloor = new List<MonsterJson>();
        for (int i = 0; i < monsterCount; i++)
        {
            var cell = distOrder[i].Cell;
            if (usedCells.Contains(cell)) continue;
            var tpl = i < monsterCount / 3 && lowerMonsters.Count > 0
                ? lowerMonsters[Rng.Next(lowerMonsters.Count)]
                : tierMonsters[Rng.Next(tierMonsters.Count)];
            usedCells.Add(cell);
            monstersOnFloor.Add(new MonsterJson
            {
                Id = i + 1,
                TemplateId = tpl.Id,
                Name = tpl.Name,
                Hp = tpl.Hp,
                Attack = tpl.Attack,
                Defense = tpl.Defense,
                GoldReward = tpl.GoldReward,
                X = cell.Item1,
                Y = cell.Item2,
                IsAlive = true,
                IsBoss = false,
                Tier = tpl.Tier,
            });
        }

        // 100 层最终 BOSS
        if (floorNumber == 100)
        {
            var bossTpl = monsters.First(m => m.IsBoss);
            var far = FarthestFreeCell(g, entryX, entryY);
            monstersOnFloor.Add(new MonsterJson
            {
                Id = 999,
                TemplateId = bossTpl.Id,
                Name = bossTpl.Name,
                Hp = bossTpl.Hp,
                Attack = bossTpl.Attack,
                Defense = bossTpl.Defense,
                GoldReward = bossTpl.GoldReward,
                X = far.Item1,
                Y = far.Item2,
                IsAlive = true,
                IsBoss = true,
                Tier = 3,
            });
            usedCells.Add(far);
        }


        // ---- 7. 道具 ----
        var itemsOnFloor = new List<ItemJson>();
        var allFree = FreeCells(g).Where(c => !usedCells.Contains(c)).ToList();
        var hpTpl = PickHpTemplate(items, tier);
        int hpCount = Rng.Next(1, 3);
        for (int i = 0; i < hpCount && allFree.Count > 0; i++)
        {
            var cell = PopRandom(allFree);
            usedCells.Add(cell);
            itemsOnFloor.Add(MakeItem(itemsOnFloor.Count + 1, hpTpl, cell));
        }
        // 攻防宝石
        foreach (var type in new[] { "Attack", "Defense" })
        {
            int count = Rng.Next(1, 3);
            var tpl = PickStatTemplate(items, type, tier);
            for (int i = 0; i < count && allFree.Count > 0; i++)
            {
                var cell = PopRandom(allFree);
                usedCells.Add(cell);
                itemsOnFloor.Add(MakeItem(itemsOnFloor.Count + 1, tpl, cell));
            }
        }
        // 门钥匙
        foreach (var ent in keyEntities)
        {
            var tpl = keyTpl.First(t => t.KeyType == ent.KeyType);
            itemsOnFloor.Add(MakeItem(itemsOnFloor.Count + 1, tpl, (ent.X, ent.Y)));
            usedCells.Add((ent.X, ent.Y));
        }
        // 额外富余钥匙
        int extraYellow = Rng.Next(2, 4) + (tier == 0 ? 1 : 0);
        var yellowTpl = keyTpl.First(t => t.KeyType == "Yellow");
        for (int i = 0; i < extraYellow && allFree.Count > 0; i++)
        {
            var cell = PopRandom(allFree);
            usedCells.Add(cell);
            itemsOnFloor.Add(MakeItem(itemsOnFloor.Count + 1, yellowTpl, cell));
        }
        if (tier >= 1 && Rng.NextDouble() < 0.7 && allFree.Count > 0)
        {
            var blueTpl = keyTpl.First(t => t.KeyType == "Blue");
            var cell = PopRandom(allFree);
            usedCells.Add(cell);
            itemsOnFloor.Add(MakeItem(itemsOnFloor.Count + 1, blueTpl, cell));
        }
        if (tier >= 2 && Rng.NextDouble() < 0.5 && allFree.Count > 0)
        {
            var redTpl = keyTpl.First(t => t.KeyType == "Red");
            var cell = PopRandom(allFree);
            usedCells.Add(cell);
            itemsOnFloor.Add(MakeItem(itemsOnFloor.Count + 1, redTpl, cell));
        }

        // ---- 8. 事件（NPC / 商店） ----
        var events = new List<EventJson>();
        var eventFree = allFree.Where(c => !usedCells.Contains(c)).ToList();
        int eventId = 1;
        if (IsShopFloor(floorNumber))
        {
            if (eventFree.Count > 0)
            {
                var cell = PopRandom(eventFree);
                usedCells.Add(cell);
                events.Add(new EventJson
                {
                    Id = eventId++,
                    Type = "Shop",
                    X = cell.Item1,
                    Y = cell.Item2,
                    Name = "旅行商人",
                    Text = "欢迎光临！金币可以购买生命、攻击、防御与钥匙。",
                    ShopId = 1,
                });
            }
        }
        if (Rng.NextDouble() < 0.4 && eventFree.Count > 0)
        {
            var cell = PopRandom(eventFree);
            usedCells.Add(cell);
            events.Add(new EventJson
            {
                Id = eventId++,
                Type = "TalkNPC",
                X = cell.Item1,
                Y = cell.Item2,
                Name = NpcNames[Rng.Next(NpcNames.Length)],
                Text = NpcTips[Rng.Next(NpcTips.Length)],
            });
        }

        // ---- 9. 输出 ----
        string[] rows = new string[H];
        for (int y = 0; y < H; y++)
        {
            char[] row = new char[W];
            for (int x = 0; x < W; x++)
                row[x] = g[x, y];
            rows[y] = new string(row);
        }

        return new FloorJson
        {
            FloorNumber = floorNumber,
            Width = W,
            Height = H,
            GridRows = rows,
            MonstersOnFloor = monstersOnFloor,
            ItemsOnFloor = itemsOnFloor,
            Events = events,
        };
    }


    // ---------- 基础工具 ----------

    private static Room CarveRoom(char[,] g, List<Room> rooms, int cx, int cy, int w, int h)
    {
        int x0 = Math.Clamp(cx - w / 2, 1, W - w - 1);
        int y0 = Math.Clamp(cy - h / 2, 1, H - h - 1);
        x0 = Math.Min(x0, W - w - 1);
        y0 = Math.Min(y0, H - h - 1);
        var room = new Room { X0 = x0, Y0 = y0, X1 = x0 + w - 1, Y1 = y0 + h - 1 };
        room.Cells = FloorRect(g, room.X0, room.Y0, room.X1, room.Y1);
        rooms.Add(room);
        return room;
    }

    private static List<(int, int)> FloorRect(char[,] g, int x0, int y0, int x1, int y1)
    {
        var cells = new List<(int, int)>();
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                if (x < 0 || y < 0 || x >= W || y >= H) continue;
                g[x, y] = '.';
                cells.Add((x, y));
            }
        return cells;
    }

    private static bool CanPlace(List<Room> rooms, int x0, int y0, int x1, int y1)
    {
        foreach (var r in rooms)
        {
            // 扩展 1 格间隙判断重叠
            if (x0 <= r.X1 + 1 && x1 >= r.X0 - 1 && y0 <= r.Y1 + 1 && y1 >= r.Y0 - 1)
                return false;
        }
        return true;
    }

    private static double Dist((int, int) a, (int, int) b)
        => Math.Abs(a.Item1 - b.Item1) + Math.Abs(a.Item2 - b.Item2);


    private static List<(int, int)>? ConnectRooms(char[,] g, Room a, Room b)
    {
        var (ax, ay) = a.Center;
        var (bx, by) = b.Center;
        // 两种 L 形
        List<(int, int)>[] options =
        {
            PathCells(ax, ay, bx, ay).Concat(PathCells(bx, ay, bx, by)).Distinct().ToList(),
            PathCells(ax, ay, ax, by).Concat(PathCells(ax, by, bx, by)).Distinct().ToList(),
        };
        foreach (var seg in options)
        {
            if (seg.Any(c => OutOfBounds(c) || g[c.Item1, c.Item2] != 'W' && !a.Cells.Contains(c) && !b.Cells.Contains(c)))
                continue;
            // 避免穿过第三方房间
            bool ok = true;
            foreach (var c in seg)
            {
                if (a.Cells.Contains(c) || b.Cells.Contains(c)) continue;
                foreach (var n in Neighbors(c))
                {
                    if (OutOfBounds(n)) continue;
                    if (g[n.Item1, n.Item2] == '.' && !seg.Contains(n) && !a.Cells.Contains(n) && !b.Cells.Contains(n))
                    { ok = false; break; }
                }
                if (!ok) break;
            }
            if (!ok) continue;
            foreach (var c in seg)
                g[c.Item1, c.Item2] = '.';
            return seg;
        }
        // 都失败：强行挖通（可能产生回路，门变为可选捷径，仍保证可通关）
        var fallback = PathCells(ax, ay, bx, by);
        foreach (var c in fallback)
            g[c.Item1, c.Item2] = '.';
        return fallback;
    }

    private static List<(int, int)> PathCells(int x0, int y0, int x1, int y1)
    {
        var cells = new List<(int, int)>();
        if (x0 == x1)
        {
            int s = Math.Min(y0, y1), e = Math.Max(y0, y1);
            for (int y = s; y <= e; y++) cells.Add((x0, y));
        }
        else
        {
            int s = Math.Min(x0, x1), e = Math.Max(x0, x1);
            for (int x = s; x <= e; x++) cells.Add((x, y0));
        }
        return cells;
    }

    private static List<(int, int)> CarveAlcove(char[,] g, (int, int) start, int length)
    {
        var cells = new List<(int, int)>();
        var cur = start;
        var used = new HashSet<(int, int)> { start };
        for (int i = 0; i < length; i++)
        {
            var candidates = Dirs
                .Select(d => (cur.Item1 + d.Item1, cur.Item2 + d.Item2))
                .Where(c => !OutOfBounds(c) && g[c.Item1, c.Item2] == 'W' && !used.Contains(c))
                .Where(c => Neighbors(c).Count(n => !OutOfBounds(n) && g[n.Item1, n.Item2] == '.') <= 1)
                .ToList();
            if (candidates.Count == 0) break;
            var next = candidates[Rng.Next(candidates.Count)];
            g[next.Item1, next.Item2] = '.';
            cells.Add(next);
            used.Add(next);
            cur = next;
        }
        return cells;
    }

    private static IEnumerable<(int, int)> Neighbors((int, int) c)
        => Dirs.Select(d => (c.Item1 + d.Item1, c.Item2 + d.Item2));

    private static bool OutOfBounds((int, int) c)
        => c.Item1 < 0 || c.Item2 < 0 || c.Item1 >= W || c.Item2 >= H;


    /// <summary>BFS 可达区域（绕过 blocked 单元格）</summary>
    private static HashSet<(int, int)> RegionBfs(char[,] g, int sx, int sy, List<(int, int)> blocked)
    {
        var result = new HashSet<(int, int)>();
        var queue = new Queue<(int, int)>();
        var blockedSet = blocked.ToHashSet();
        queue.Enqueue((sx, sy));
        result.Add((sx, sy));
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            foreach (var n in Neighbors(c))
            {
                if (OutOfBounds(n) || result.Contains(n) || blockedSet.Contains(n)) continue;
                if (g[n.Item1, n.Item2] == 'W') continue;
                result.Add(n);
                queue.Enqueue(n);
            }
        }
        return result;
    }

    private static int BfsDist(char[,] g, int sx, int sy, int tx, int ty)
    {
        var seen = new HashSet<(int, int)> { (sx, sy) };
        var queue = new Queue<((int, int), int)>();
        queue.Enqueue(((sx, sy), 0));
        while (queue.Count > 0)
        {
            var (c, d) = queue.Dequeue();
            if (c == (tx, ty)) return d;
            foreach (var n in Neighbors(c))
            {
                if (OutOfBounds(n) || seen.Contains(n)) continue;
                if (g[n.Item1, n.Item2] == 'W') continue;
                seen.Add(n);
                queue.Enqueue((n, d + 1));
            }
        }
        return -1;
    }

    private static (int, int) FarthestFreeCell(char[,] g, int sx, int sy)
    {
        var seen = new HashSet<(int, int)> { (sx, sy) };
        var queue = new Queue<((int, int), int)>();
        queue.Enqueue(((sx, sy), 0));
        (int, int) far = (sx, sy);
        int farDist = 0;
        while (queue.Count > 0)
        {
            var (c, d) = queue.Dequeue();
            if (d > farDist) { farDist = d; far = c; }
            foreach (var n in Neighbors(c))
            {
                if (OutOfBounds(n) || seen.Contains(n)) continue;
                if (g[n.Item1, n.Item2] == 'W') continue;
                seen.Add(n);
                queue.Enqueue((n, d + 1));
            }
        }
        return far;
    }

    private static IEnumerable<(int, int)> FreeCells(char[,] g)
    {
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                if (g[x, y] != 'W')
                    yield return (x, y);
    }

    private static T PopRandom<T>(List<T> list)
    {
        int i = Rng.Next(list.Count);
        var v = list[i];
        list[i] = list[^1];
        list.RemoveAt(list.Count - 1);
        return v;
    }

    private static ItemTemplate PickHpTemplate(List<ItemTemplate> items, int tier)
    {
        var hp = items.Where(t => t.Type == "Hp").ToList();
        return tier switch
        {
            0 => hp[0],
            1 => Rng.NextDouble() < 0.7 ? hp[1] : hp[0],
            2 => Rng.NextDouble() < 0.7 ? hp[2] : hp[1],
            _ => hp[2],
        };
    }

    private static ItemTemplate PickStatTemplate(List<ItemTemplate> items, string type, int tier)
    {
        var pool = items.Where(t => t.Type == type).ToList();
        return tier switch
        {
            0 => pool[0],
            1 => Rng.NextDouble() < 0.7 ? pool[1] : pool[0],
            2 => Rng.NextDouble() < 0.7 ? pool[2] : pool[1],
            _ => pool[2],
        };
    }

    private static ItemJson MakeItem(int id, ItemTemplate tpl, (int, int) cell)
        => new()
        {
            Id = id,
            TemplateId = tpl.Id,
            Name = tpl.Name,
            Desc = tpl.Desc,
            Type = tpl.Type,
            KeyType = tpl.KeyType,
            Value = tpl.Value,
            X = cell.Item1,
            Y = cell.Item2,
            PickedUp = false,
        };

    private static bool IsShopFloor(int f)
    {
        int[] shops =
        {
            2, 5, 10, 15, 20,
            25, 32, 40, 48,
            55, 62, 70, 78,
            85, 90, 95, 99,
        };
        return shops.Contains(f);
    }

    private static readonly string[] NpcNames = { "旅人", "老法师", "守卫", "预言者", "商人助手", "神秘人" };

    private static readonly string[] NpcTips =
    {
        "小心！黄色钥匙通常就在起点附近，先收集钥匙再挑战门后的怪物。",
        "防御比攻击更重要：减少每次受到的伤害，持久战斗更安全。",
        "商店里用金币购买属性，性价比最高的投资是攻击与防御。",
        "打不过的怪物可以先绕开，楼层之间互相连通，变强后再回来。",
        "红门和蓝门后面的怪物通常更强，准备好再来挑战。",
        "楼层的宝藏往往藏在壁龛的死角里，仔细探索每一层。",
        "100 层的魔王守护着最终胜利，集齐最强的力量再去挑战吧。",
        "金币来之不易，战斗前先估算自己的损失，别做无谓的牺牲。",
    };

    /// <summary>
    /// 1F 特殊配置（原版设定）：左下角放置飞行器（楼层穿梭机）。
    /// 将左下入口 (1,7) 的黄门改为地板，形成无门直达的暗格，并在该格放置飞行器道具
    /// （无怪物看守、无需钥匙、开局即可拾取）。
    /// 注意：重新运行生成器会重建全部 Floor 文件，手工附加的 NPC（如老祭司任务）需另行保留。
    /// </summary>
    public static void ApplyFloor1Special(FloorJson floor)
    {
        if (floor.FloorNumber != 1) return;

        char[] row = floor.GridRows[7].ToCharArray();
        row[1] = '.';
        floor.GridRows[7] = new string(row);

        floor.ItemsOnFloor.Add(new ItemJson
        {
            Id = 11,
            TemplateId = 13,
            Name = "飞行器",
            Desc = "楼层穿梭机，在楼梯旁使用，跳转至已探索楼层，无使用次数",
            Type = "FlyOrb",
            KeyType = "Yellow",
            Value = 0,
            X = 1,
            Y = 7,
            PickedUp = false,
        });
    }
}

