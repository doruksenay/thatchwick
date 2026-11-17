using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Content
{
    /// <summary>Yapı parçasının türü: hangi araçla kullanıldığını belirler.</summary>
    public enum BuildPartKind
    {
        /// <summary>Duvar ve yarım duvar malzemesi (dal-örgü, ahşap çatkı, taş...).</summary>
        WallMaterial = 0,
        /// <summary>Duvarın bir yüzünün kaplaması (badana, sıva, lambri, duvar halısı).</summary>
        WallFinish = 1,
        Door = 2,
        Window = 3,
        Fence = 4,
        /// <summary>Çit kapısı: çit hattında geçilebilir parça.</summary>
        Gate = 5,
        Railing = 6,
        /// <summary>Zemin kaplaması; Base hangi temel türe (çim, toprak, ahşap, taş) sayıldığını söyler.</summary>
        Floor = 7,
        Roof = 8,
        /// <summary>Mobilya kaplaması (renk/malzeme örneği); Group eşyanın finishGroup'uyla eşleşir.</summary>
        ItemFinish = 9
    }

    /// <summary>
    /// Yapı modunun malzeme ve parça kataloğu (items.json "buildParts"): Sims'teki gibi her duvar, kapı, pencere,
    /// zemin, çatı ve mobilya kaplaması seçilebilir bir parçadır. Görsel gelene kadar Color ile çizilir.
    /// </summary>
    public sealed class BuildPartDef : IContentDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public BuildPartKind Kind { get; set; }
        /// <summary>Parça başına akçe (duvar/kapı/pencere/çit kenar başına, zemin ve çatı karo başına).</summary>
        public int Price { get; set; }
        public int UnlockLevel { get; set; } = 1;
        /// <summary>Yer tutucu renk (#RRGGBB).</summary>
        public string Color { get; set; } = "#C8B490";
        /// <summary>Zemin: temel tür (Grass, Dirt, Wood, Stone).</summary>
        public World.FloorType Base { get; set; } = World.FloorType.Wood;
        /// <summary>Isı kaybı çarpanı (duvar malzemesi, kapı, pencere, çatı); 1 = varsayılan.</summary>
        public float HeatLoss { get; set; } = 1f;
        /// <summary>Konfor katkısı (kaplama, zemin): odanın konfor puanına eklenir.</summary>
        public int Comfort { get; set; }
        /// <summary>Mobilya kaplaması: grup (wood, cloth, metal, clay) ve fiyat çarpanı.</summary>
        public string Group { get; set; }
        public float PriceFactor { get; set; } = 1f;
        /// <summary>Konabileceği katlar; boşsa her kat (bodrum penceresi yalnızca -1).</summary>
        public List<int> Levels { get; set; } = new List<int>();
        /// <summary>Türünün varsayılanı (eski kayıtlar ve stil seçilmemiş araç bunu kullanır).</summary>
        public bool IsDefault { get; set; }
        /// <summary>Malzeme (tuğla, kiremit, yontma taş, boya): envanterde varsa harcanır ve akçe yarıya iner; yoksa tam akçe.</summary>
        public string MaterialItem { get; set; }
        public int MaterialCount { get; set; } = 1;

        public bool AllowsLevel(int level) => Levels.Count == 0 || Levels.Contains(level);

        /// <summary>Renk metnini 0-255 kanallarına çevirir; geçersizse false.</summary>
        public bool TryGetRgb(out byte r, out byte g, out byte b)
        {
            r = g = b = 0;
            var c = Color;
            if (string.IsNullOrEmpty(c) || c.Length != 7 || c[0] != '#') return false;
            try
            {
                r = Convert.ToByte(c.Substring(1, 2), 16);
                g = Convert.ToByte(c.Substring(3, 2), 16);
                b = Convert.ToByte(c.Substring(5, 2), 16);
                return true;
            }
            catch (FormatException) { return false; }
        }
    }

    /// <summary>Yapı parçası kataloğu üzerinde kısa sorgular.</summary>
    public static class BuildParts
    {
        public static List<BuildPartDef> OfKind(ContentDatabase db, BuildPartKind kind, int level = int.MinValue)
        {
            var list = new List<BuildPartDef>();
            foreach (var p in db.Parts.All)
                if (p.Kind == kind && (level == int.MinValue || p.AllowsLevel(level))) list.Add(p);
            list.Sort((a, b) => a.UnlockLevel != b.UnlockLevel ? a.UnlockLevel.CompareTo(b.UnlockLevel) : a.Price.CompareTo(b.Price));
            return list;
        }

        /// <summary>Türün varsayılan parçası (IsDefault ya da ilk); yoksa null.</summary>
        public static BuildPartDef Default(ContentDatabase db, BuildPartKind kind)
        {
            BuildPartDef first = null;
            foreach (var p in db.Parts.All)
            {
                if (p.Kind != kind) continue;
                if (p.IsDefault) return p;
                if (first == null) first = p;
            }
            return first;
        }

        /// <summary>Kimliği verilen türde bir parça ise onu, değilse türün varsayılanını döner.</summary>
        public static BuildPartDef Resolve(ContentDatabase db, BuildPartKind kind, string id)
        {
            if (!string.IsNullOrEmpty(id) && db.Parts.TryGet(id, out var p) && p.Kind == kind) return p;
            return Default(db, kind);
        }

        /// <summary>Zemin türü için varsayılan zemin parçası.</summary>
        public static BuildPartDef DefaultFloor(ContentDatabase db, World.FloorType floor)
        {
            BuildPartDef first = null;
            foreach (var p in db.Parts.All)
            {
                if (p.Kind != BuildPartKind.Floor || p.Base != floor) continue;
                if (p.IsDefault) return p;
                if (first == null) first = p;
            }
            return first;
        }

        /// <summary>Eşyanın kaplama grubundaki seçenekler (grup yoksa boş).</summary>
        public static List<BuildPartDef> FinishesFor(ContentDatabase db, ItemDef def)
        {
            var list = new List<BuildPartDef>();
            if (def == null || string.IsNullOrEmpty(def.FinishGroup)) return list;
            foreach (var p in db.Parts.All)
                if (p.Kind == BuildPartKind.ItemFinish && p.Group == def.FinishGroup) list.Add(p);
            return list;
        }
    }
}
