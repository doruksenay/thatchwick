using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Mining
{
    /// <summary>Bir damar türü: hangi hammadde, kaç damar, karo sertliği, gereken Madencilik seviyesi.</summary>
    public sealed class VeinDef
    {
        public string Item;
        public int Count;
        public RockKind Rock = RockKind.Stone;
        public int MinLevel = 1;
        /// <summary>Damar boyu aralığı (karo); cep türleri (kil) kısa ve yuvarlak.</summary>
        public int MinLength = 4, MaxLength = 9;
        public bool Pocket;

        public VeinDef(string item, int count, RockKind rock, int minLevel, bool pocket = false)
        {
            Item = item; Count = count; Rock = rock; MinLevel = minLevel; Pocket = pocket;
        }
    }

    /// <summary>Bir kat ya da arsa kazısının üretim tablosu (23. belge 3.2, 3.4, 4.2).</summary>
    public sealed class MineTable
    {
        public string Id;
        /// <summary>Ana kaya ağırlıkları: toprak, yumuşak taş, taş, sert taş.</summary>
        public float Soil, Soft, Stone, Hard;
        public bool BedrockBorder;
        public List<VeinDef> Veins = new List<VeinDef>();
        /// <summary>Damar karolarının sulu olma oranı ve damar dışı karolarınki.</summary>
        public double WetOre, WetOther;
        /// <summary>Taş karolarında kalıntı şansı ve kalıntı listesi.</summary>
        public double RelicChance;
        public string[] Relics = new string[0];
        /// <summary>Destek mesafesi ve gevşek karonun günlük çatırdama şansı.</summary>
        public int SupportDistance = 3;
        public double CreakChance = 0.10;
        /// <summary>Kazılabilmesi için gereken Madencilik seviyesi.</summary>
        public int MinLevel = 1;

        public static readonly string[] PlotRelics = { "item.relic_foundation_stone", "item.relic_buried_jar", "item.relic_horseshoe" };
        public static readonly string[] MineRelics = { "item.relic_fossil", "item.relic_miner_lamp", "item.relic_pick_head", "item.relic_coin", "item.relic_bone_comb", "item.relic_crystal" };

        /// <summary>Arsa kazısı (kat -2): toprak ve yumuşak taş ağırlıklı; kil, kireçtaşı, kömür, seyrek demir; derin hammadde yok.</summary>
        public static MineTable Plot() => new MineTable
        {
            Id = "plot", Soil = 0.45f, Soft = 0.33f, Stone = 0.19f, Hard = 0.03f,
            Veins =
            {
                new VeinDef("item.clay", 4, RockKind.Soil, 1, pocket: true),
                new VeinDef("item.limestone", 2, RockKind.Stone, 1),
                new VeinDef("item.coal", 2, RockKind.Soft, 2),
                new VeinDef("item.iron_ore", 1, RockKind.Hard, 2),
            },
            WetOre = 0.08, WetOther = 0.04, RelicChance = 0.005, Relics = PlotRelics,
            SupportDistance = 2, CreakChance = 0.10, MinLevel = 2
        };

        /// <summary>Aile ocağının galeri katı (-1): taş, kireçtaşı, demir, kömür, arduvaz, alçıtaşı, aşı boyası.</summary>
        public static MineTable Gallery() => new MineTable
        {
            Id = "mine-1", Soil = 0.05f, Soft = 0.25f, Stone = 0.5f, Hard = 0.2f, BedrockBorder = true,
            Veins =
            {
                new VeinDef("item.limestone", 4, RockKind.Stone, 1),
                new VeinDef("item.flint", 2, RockKind.Stone, 1),
                new VeinDef("item.clay", 3, RockKind.Soil, 1, pocket: true),
                new VeinDef("item.ochre", 2, RockKind.Soft, 2),
                new VeinDef("item.coal", 3, RockKind.Soft, 2),
                new VeinDef("item.iron_ore", 4, RockKind.Hard, 2),
                new VeinDef("item.gypsum", 2, RockKind.Soft, 3),
                new VeinDef("item.slate_stone", 2, RockKind.Hard, 3),
            },
            WetOre = 0, WetOther = 0, RelicChance = 0.005, Relics = MineRelics,
            SupportDistance = 3, CreakChance = 0.10, MinLevel = 1
        };
    }

    /// <summary>Tohumlu kazı yeri üretimi: aynı tohum aynı katı verir (kayıtta yalnızca kazılan şekil ve değişen karolar tutulur).</summary>
    public static class MineGen
    {
        public static void Generate(MineMap map, MineTable table)
        {
            var rng = new Random(map.Seed);
            float total = table.Soil + table.Soft + table.Stone + table.Hard;
            foreach (var (x, y, t) in map.All())
            {
                if (table.BedrockBorder && (x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1)) { t.Rock = RockKind.Bedrock; continue; }
                double r = rng.NextDouble() * total;
                t.Rock = r < table.Soil ? RockKind.Soil : r < table.Soil + table.Soft ? RockKind.Soft : r < table.Soil + table.Soft + table.Stone ? RockKind.Stone : RockKind.Hard;
                t.Wet = rng.NextDouble() < table.WetOther;
            }
            // Damarlar: tek yönde uzanan 1-2 karo genişliğinde şeritler; cepler kısa ve toplu.
            foreach (var v in table.Veins)
                for (int n = 0; n < v.Count; n++)
                {
                    int x = 1 + rng.Next(Math.Max(1, map.Width - 2)), y = 1 + rng.Next(Math.Max(1, map.Height - 2));
                    var (dx, dy) = MineMap.Eight[rng.Next(8)];
                    int len = v.Pocket ? 2 + rng.Next(3) : v.MinLength + rng.Next(v.MaxLength - v.MinLength + 1);
                    bool wide = v.Pocket || rng.NextDouble() < 0.4;
                    for (int i = 0; i < len; i++)
                    {
                        Paint(map, x, y, v, table, rng);
                        if (wide) Paint(map, x + (dy != 0 ? 1 : 0), y + (dx != 0 ? 1 : 0), v, table, rng);
                        if (rng.NextDouble() < 0.25) { var turn = MineMap.Eight[rng.Next(8)]; if (turn.dx != -dx || turn.dy != -dy) (dx, dy) = turn; }
                        x += dx; y += dy;
                        if (!map.InBounds(x, y)) break;
                    }
                }
        }

        private static void Paint(MineMap map, int x, int y, VeinDef v, MineTable table, Random rng)
        {
            var t = map.Get(x, y);
            if (t == null || t.Rock == RockKind.Bedrock || t.IsOpen) return;
            t.Ore = v.Item;
            t.Rock = v.Rock;
            t.Wet = rng.NextDouble() < table.WetOre;
        }
    }
}
