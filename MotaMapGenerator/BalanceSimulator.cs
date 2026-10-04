namespace MotaMapGenerator;

using System.Text.Json;

/// <summary>
/// 通关模拟器：模拟一个「逐层顺序满清 + 商店消费」的参考玩家从 1 层打到 100 层，
/// 校验原版魔塔数值曲线在 100 层体量下可通关：
/// 1. 每层每只怪都能破防（玩家攻击 &gt; 怪物防御，否则游戏中禁止开战=卡死）；
/// 2. 逐层战损不断链（含 BOSS 层）；
/// 3. 战斗公式与游戏 MathHelper 完全一致（原版魔塔公式：先手、max(1,ATK−DEF)）。
/// 输出逐层战况与最终属性；任何失败都会打印详细信息并返回非零退出码。
/// </summary>
public static class BalanceSimulator
{
    private const int StartHp = 500, StartAtk = 10, StartDef = 10; // 与 Player.cs 初始值一致（按前几层怪物战损校准）

    public static int Run(string floorsDir, List<MonsterTemplate> templates, List<ShopConfigJson> shops)
    {
        var tplById = templates.ToDictionary(t => t.Id);
        int hp = StartHp, atk = StartAtk, def = StartDef, gold = 0;
        int yellow = 1, blue = 0, red = 0;
        var fails = new List<string>();
        int worstLossFloor = 0, worstLossPct = 0;

        Console.WriteLine();
        Console.WriteLine("========== 通关模拟（顺序满清参考玩家） ==========");
        Console.WriteLine($"初始：HP {hp} / 攻 {atk} / 防 {def} / 黄钥匙 {yellow}");

        for (int f = 1; f <= 100; f++)
        {
            var floor = JsonSerializer.Deserialize<FloorJson>(
                File.ReadAllText(Path.Combine(floorsDir, $"Floor{f}.json")),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            int hpBefore = hp, atkBefore = atk;

            // 1. 拾取全层道具：药水直接入账（原版无生命上限），宝石加属性，钥匙入袋
            foreach (var it in floor.ItemsOnFloor)
            {
                switch (it.Type)
                {
                    case "Hp": hp += it.Value; break;
                    case "Attack": atk += it.Value; break;
                    case "Defense": def += it.Value; break;
                    case "Key" when it.KeyType == "Yellow": yellow++; break;
                    case "Key" when it.KeyType == "Blue": blue++; break;
                    case "Key" when it.KeyType == "Red": red++; break;
                }
            }

            // 2. 商店消费（按楼层段位定价）：保底黄钥匙 → 攻防交替投资 → 余钱买生命
            if (floor.Events.Any(e => e.ShopId != null))
            {
                int tier = f <= 24 ? 0 : f <= 50 ? 1 : f <= 80 ? 2 : 3;
                var shop = shops.First(s => s.ShopId == tier + 1);
                int Price(string type, string keyType = "") => shop.Items
                    .First(i => i.Type == type && (type != "Key" || i.KeyType == keyType)).Price;
                int yellowPrice = Price("Key", "Yellow");
                int atkPrice = Price("Attack"), defPrice = Price("Defense"), hpPrice = Price("Hp");

                while (yellow < 3 && gold >= yellowPrice) { gold -= yellowPrice; yellow++; }
                bool atkTurn = true;
                while (true)
                {
                    if (atkTurn && gold >= atkPrice) { gold -= atkPrice; atk += shop.Items.First(i => i.Type == "Attack").Value; }
                    else if (!atkTurn && gold >= defPrice) { gold -= defPrice; def += shop.Items.First(i => i.Type == "Defense").Value; }
                    else if (gold >= atkPrice) { gold -= atkPrice; atk += shop.Items.First(i => i.Type == "Attack").Value; }
                    else if (gold >= defPrice) { gold -= defPrice; def += shop.Items.First(i => i.Type == "Defense").Value; }
                    else break;
                    atkTurn = !atkTurn;
                }
                while (gold >= hpPrice) { gold -= hpPrice; hp += shop.Items.First(i => i.Type == "Hp").Value; }
            }

            // 3. 满清全层怪物（含 BOSS），公式与 MathHelper 一致
            int lossTotal = 0;
            foreach (var m in floor.MonstersOnFloor)
            {
                var tpl = tplById[m.TemplateId];
                int pDmg = Math.Max(1, atk - tpl.Defense);
                int mDmg = tpl.IgnoreDefense ? Math.Max(1, tpl.Attack) : Math.Max(1, tpl.Attack - def);

                if (atk <= tpl.Defense)
                {
                    fails.Add($"F{f:D3} 无法破防 {tpl.Name}(防 {tpl.Defense})：玩家攻 {atk}");
                    continue;
                }

                int turns = (int)Math.Ceiling(tpl.Hp / (double)pDmg);
                int loss = (turns - 1) * mDmg;
                lossTotal += loss;
                gold += tpl.GoldReward;
            }

            hp -= lossTotal;
            if (hp <= 0)
                fails.Add($"F{f:D3} 战损断链：层前 HP {hpBefore} + 药水后 {hp + lossTotal}，全层战损 {lossTotal}");

            int lossPct = hpBefore > 0 ? (int)(100L * lossTotal / hpBefore) : 100;
            if (lossPct > worstLossPct && f < 100) { worstLossPct = lossPct; worstLossFloor = f; }

            string bossMark = floor.MonstersOnFloor.Any(m => m.IsBoss) ? " ◆BOSS" : "";
            Console.WriteLine(
                $"F{f:D3} | 入层 HP {hpBefore,6} → 出层 {hp,6} | 战损 {lossTotal,5} ({lossPct,2}%) | " +
                $"攻 {atkBefore,3}→{atk,3} 防 {def,3} | 金币 {gold,4} | 钥 y{yellow} b{blue} r{red}{bossMark}");
        }

        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"通关：最终 HP {hp} / 攻 {atk} / 防 {def} / 金币 {gold}");
        if (worstLossPct > 0)
            Console.WriteLine($"最险层：F{worstLossFloor}（战损 {worstLossPct}% 层前生命）");

        if (fails.Count > 0)
        {
            Console.WriteLine($"\n❌ 平衡校验失败 {fails.Count} 项：");
            foreach (var line in fails.Take(40)) Console.WriteLine("  " + line);
            return 1;
        }

        Console.WriteLine("\n✅ 平衡校验通过：全程可破防、无战损断链，100 层可通关。");
        return 0;
    }
}
