using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Haritadaki sabit bir ulu ağaç yeri (belge 52 §2).</summary>
    public sealed class GreatTreeSite
    {
        public string Id;
        public string ItemId;
        /// <summary>Bölge kimliği (Regions); "farm" ise arsanın içinde olmalı.</summary>
        public string RegionId;
        /// <summary>Tercih edilen yer (dünya karosu); boşsa çevresinde en yakın uygun yer aranır.</summary>
        public int X, Y;
    }

    /// <summary>
    /// Ulu ağaçlar (belge 52): kesilemez, dikilemez, taşınamaz; haritaya sabit yerlerinde konur. Arsada bir tane kesin
    /// (karar 0-1), dünyada en az dört. Yerleştirme eksik olanları ekler (yeni oyun ve eski kayıt aynı yoldan).
    /// </summary>
    public static class GreatTrees
    {
        public const string Tag = "great_tree";
        /// <summary>Köşk boyu etiketi: kiosk5 ise 5x5, değilse 4x4 (karar 0-5).</summary>
        public const string BigKioskTag = "kiosk5";
        /// <summary>Hazır aşevi köşkü olan ulu ağaç (kasaba çınarı, karar 0-6).</summary>
        public const string InnTag = "inn_kiosk";

        public const string Oak = "item.great_oak", Linden = "item.great_linden", Walnut = "item.great_walnut",
            Chestnut = "item.great_chestnut", Plane = "item.great_plane";

        public static readonly GreatTreeSite[] Sites =
        {
            // Arsa: güneydoğudaki yabani meşelerin yerinde (karar 0-1).
            new GreatTreeSite { Id = "farm", ItemId = Oak, RegionId = "farm", X = 54, Y = 52 },
            // Köy yolu: yol kenarında ceviz.
            new GreatTreeSite { Id = "road", ItemId = Walnut, RegionId = "road", X = Regions.Road.X + 30, Y = Regions.Road.Y + 21 },
            // Şehir meydanının güneydoğusu: hazır aşevi köşklü çınar.
            new GreatTreeSite { Id = "town", ItemId = Plane, RegionId = "town", X = Regions.Town.X + 40, Y = Regions.Town.Y + 34 },
            // Orman: kestane.
            new GreatTreeSite { Id = "forest", ItemId = Chestnut, RegionId = "forest", X = Regions.Forest.X + 24, Y = Regions.Forest.Y + 26 },
            // Irmak kıyısı: ıhlamur (Pitchford türü).
            new GreatTreeSite { Id = "lake", ItemId = Linden, RegionId = "lake", X = Regions.Lake.X + 12, Y = Regions.Lake.Y + 22 },
        };

        public static bool IsGreatTree(ItemDef def) => def != null && def.HasTag(Tag);
        public static bool IsInn(ItemDef def) => def != null && def.HasTag(InnTag);
        public static int KioskSize(ItemDef def) => def != null && def.HasTag(BigKioskTag) ? 5 : 4;

        /// <summary>Köşkün ip merdiveninin yer karosu (ağacın önü, iki karo güneyi).</summary>
        public static GridPos LadderTile(ItemInstance tree) => new GridPos(tree.Position.X, tree.Position.Y - 2, 0);

        private static Region RegionById(string id)
        {
            foreach (var r in Regions.All) if (r.Id == id) return r;
            return null;
        }

        /// <summary>
        /// Eksik ulu ağaçları yerlerine koyar; konanların listesini döner. Bölgesi bu haritada olmayan yer atlanır (eski
        /// küçük kayıt yalnızca çiftlikteki meşeyi alır). Ayak izindeki ve merdiven karosundaki bitki ve toplama düğümü kalkar.
        /// </summary>
        public static List<ItemInstance> Ensure(WorldState world, ContentDatabase content)
        {
            var placed = new List<ItemInstance>();
            if (world == null || content == null) return placed;
            bool regional = Regions.IsRegional(world.Map);
            foreach (var site in Sites)
            {
                if (!content.Items.TryGet(site.ItemId, out var def)) continue;
                var region = RegionById(site.RegionId);
                if (site.RegionId != "farm" && (!regional || region == null || !Regions.Exists(world.Map, region))) continue;
                if (HasTreeNear(world, def.Id, site)) continue;
                var spot = FindSpot(world, def, site, region, regional);
                if (!spot.HasValue) continue;
                Clear(world, def, spot.Value);
                if (!world.CanPlaceFloor(def, spot.Value, 0).Ok) continue;
                placed.Add(world.PlaceItem(def, spot.Value));
            }
            return placed;
        }

        private static bool HasTreeNear(WorldState world, string defId, GreatTreeSite site)
        {
            foreach (var it in world.Items)
                if (it.Def.Id == defId && Math.Abs(it.Position.X - site.X) <= 16 && Math.Abs(it.Position.Y - site.Y) <= 16) return true;
            return false;
        }

        /// <summary>Spiral arama: ayak izi + merdiven karosu boş (ya da yalnızca kaldırılabilir bitki), su değil, aynı bölgede.</summary>
        private static GridPos? FindSpot(WorldState world, ItemDef def, GreatTreeSite site, Region region, bool regional)
        {
            for (int r = 0; r <= 10; r++)
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;
                        var p = new GridPos(site.X + dx, site.Y + dy, 0);
                        if (Fits(world, def, p, site, region, regional)) return p;
                    }
            return null;
        }

        private static bool Fits(WorldState world, ItemDef def, GridPos p, GreatTreeSite site, Region region, bool regional)
        {
            var map = world.Map;
            var tiles = new List<GridPos>();
            for (int y = 0; y < def.Height; y++) for (int x = 0; x < def.Width; x++) tiles.Add(new GridPos(p.X + x, p.Y + y, 0));
            tiles.Add(new GridPos(p.X, p.Y - 2, 0));   // ip merdiveni
            tiles.Add(new GridPos(p.X, p.Y - 1, 0));
            foreach (var t in tiles)
            {
                if (!map.InBounds(t) || map.IsWater(t)) return false;
                if (regional && region != null && !region.Contains(t)) return false;
                if (site.RegionId == "farm" && !map.IsOwned(t)) return false;
                if (map.GetFloor(t) != FloorType.Grass && map.GetFloor(t) != FloorType.Dirt) return false;
                if (world.IsOccupiedByCharacter(t)) return false;
                var it = world.GetItemAt(t);
                if (it != null && !Removable(it)) return false;
                if (it == null && map.IsBlocked(t)) return false;
                for (int z = 1; z <= GridMap.MaxLevel; z++) if (map.HasTile(new GridPos(t.X, t.Y, z))) return false;
            }
            // Köşk için üstte boş alan (oda + balkon) da gerekir.
            int n = KioskSize(def);
            for (int y = p.Y - 2; y <= p.Y - 2 + n; y++)
                for (int x = p.X - 1; x < p.X - 1 + n; x++)
                {
                    var up = new GridPos(x, y, 1);
                    if (!map.InBounds(up) || map.HasTile(up)) return false;
                    if (regional && region != null && !region.Contains(new GridPos(x, y, 0))) return false;
                }
            return true;
        }

        private static bool Removable(ItemInstance it) => it.IsPlant || it.Def.IsForage || it.Def.Category == "forage" || it.Def.Category == "plant";

        private static void Clear(WorldState world, ItemDef def, GridPos p)
        {
            var tiles = new List<GridPos>();
            for (int y = 0; y < def.Height; y++) for (int x = 0; x < def.Width; x++) tiles.Add(new GridPos(p.X + x, p.Y + y, 0));
            tiles.Add(new GridPos(p.X, p.Y - 2, 0));
            tiles.Add(new GridPos(p.X, p.Y - 1, 0));
            foreach (var t in tiles)
            {
                var it = world.GetItemAt(t);
                if (it != null && Removable(it)) world.RemoveItem(it.Id);
            }
        }

        /// <summary>Ağacın günlük ürünü (Topla): türüne göre, mevsimine göre (JSON'daki gather etkileşimi verir).</summary>
        public static string SpeciesName(ItemDef def) => def?.DisplayName ?? "";
    }
}
