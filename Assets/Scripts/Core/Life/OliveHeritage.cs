using System;
using System.Collections.Generic;
using System.Globalization;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Yadigâr zeytin ağacı (belge 32-7): zeytin çok uzun yaşar. Dikildiği gün ve diken kişi kaydedilir;
    /// <see cref="HeirloomDays"/> gün (≈ 20 yıl, iki kuşak) sonra ağaç "yadigâr" olur, kronikte anılır
    /// ve hasadı yaşla artar (her <see cref="DaysPerBonus"/> günde +1 zeytin, en çok <see cref="MaxBonus"/>).
    /// </summary>
    public sealed class OliveHeritage
    {
        public const string TreeId = "item.tree_olive";
        public const int HeirloomDays = 40;
        public const int DaysPerBonus = 20;
        public const int MaxBonus = 3;

        private sealed class Tree { public int PlantedDay; public string Planter; public bool Heirloom; }
        private readonly Dictionary<int, Tree> _trees = new Dictionary<int, Tree>();

        public int Count => _trees.Count;

        public bool IsHeirloom(int itemId) => _trees.TryGetValue(itemId, out var t) && t.Heirloom;
        public string PlanterOf(int itemId) => _trees.TryGetValue(itemId, out var t) ? t.Planter : null;
        public int AgeOf(int itemId, int today) => _trees.TryGetValue(itemId, out var t) ? Math.Max(0, today - t.PlantedDay) : 0;

        /// <summary>Hasatta yaşa göre ek zeytin (yadigâr olmayan ağaçta 0).</summary>
        public int BonusOlives(int itemId, int today)
        {
            if (!_trees.TryGetValue(itemId, out var t) || !t.Heirloom) return 0;
            return Math.Min(MaxBonus, 1 + (today - t.PlantedDay - HeirloomDays) / DaysPerBonus);
        }

        /// <summary>
        /// Günlük: yeni dikilen zeytinleri kaydeder (dikeni hane reisi sayar), kesilenleri siler,
        /// yaşı dolanı yadigâr yapar. Yadigâr olan ağaçların açıklamalarını döner.
        /// </summary>
        /// <summary>Son "yadigâr ağaç" haberi ve dikenin adı (kroniğe anahtarlı yazmak için; SavedText).</summary>
        public string LastHeirloomNote { get; private set; }
        public string LastHeirloomPlanter { get; private set; }

        public List<string> OnNewDay(int today, WorldState world, string headName)
        {
            var news = new List<string>();
            var seen = new HashSet<int>();
            foreach (var it in world.Items)
            {
                if (it.Def.Id != TreeId) continue;
                seen.Add(it.Id);
                if (!_trees.TryGetValue(it.Id, out var t))
                {
                    _trees[it.Id] = new Tree { PlantedDay = today, Planter = headName ?? "Hane" };
                    continue;
                }
                if (!t.Heirloom && today - t.PlantedDay >= HeirloomDays)
                {
                    t.Heirloom = true;
                    LastHeirloomPlanter = t.Planter;
                    LastHeirloomNote = $"{t.Planter}'in diktiği zeytin artık yadigâr ağaç; yaşlandıkça daha çok verir.";
                    news.Add(LastHeirloomNote);
                }
            }
            var gone = new List<int>();
            foreach (var id in _trees.Keys) if (!seen.Contains(id)) gone.Add(id);
            foreach (var id in gone) _trees.Remove(id);
            return news;
        }

        // itemId:dikimGünü:yadigâr(0/1):diken
        public string[] Export()
        {
            var l = new List<string>();
            foreach (var kv in _trees)
                l.Add(string.Join(":", kv.Key.ToString(CultureInfo.InvariantCulture), kv.Value.PlantedDay.ToString(CultureInfo.InvariantCulture), kv.Value.Heirloom ? "1" : "0", kv.Value.Planter ?? ""));
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _trees.Clear();
            if (data == null) return;
            foreach (var e in data)
            {
                var p = (e ?? "").Split(new[] { ':' }, 4);
                if (p.Length < 4 || !int.TryParse(p[0], out int id) || !int.TryParse(p[1], out int day)) continue;
                _trees[id] = new Tree { PlantedDay = day, Heirloom = p[2] == "1", Planter = p[3] };
            }
        }
    }

    /// <summary>Petekli bal (32-8.2): kovandaki baldan petek kesilir; iki bal yerine bir petek.</summary>
    public static class Honeycomb
    {
        public const string HoneyId = "item.honey";
        public const string CombId = "item.honeycomb";
        public const int HoneyPerComb = 2;

        public static bool Cut(ItemInstance hive, Household house)
        {
            if (hive == null || house == null) return false;
            var stock = AnimalSystem.TakeStock(hive);
            int honey = 0;
            foreach (var kv in stock) if (kv.Key == HoneyId) honey += kv.Value;
            bool ok = honey >= HoneyPerComb;
            foreach (var kv in stock)
            {
                int back = kv.Value;
                if (ok && kv.Key == HoneyId) back -= HoneyPerComb;
                AnimalSystem.PutStock(hive, kv.Key, back);
            }
            if (ok) house.AddItem(CombId, 1);
            return ok;
        }
    }
}
