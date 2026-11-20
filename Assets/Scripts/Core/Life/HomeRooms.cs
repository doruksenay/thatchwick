using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Odanın türü: içindeki eşyalardan anlaşılır (belge 45 §7).</summary>
    public enum RoomKind
    {
        None = 0,
        Living = 1,
        Kitchen = 2,
        Bedroom = 3,
        Workshop = 4,
        Pantry = 5,
        /// <summary>Sera: pencereli, ışıklı zemin kat odası (PlantSystem.IsGreenhouse) içinde bitki ya da seraya ait eşya; yatak ve ocak yok.</summary>
        Greenhouse = 6,
        /// <summary>Ağaç köşkü (belge 52): ulu ağaca bağlı oda; konforlu eşyaya manzara katkısı, içindekilere manzara moodlet'i.</summary>
        TreeKiosk = 7,
        /// <summary>Mezbaha (belge 60a §2): içinde kesim kancası olan kapalı oda. Kesim burada, görünmeden yapılır.</summary>
        Slaughterhouse = 8
    }

    public sealed partial class HomeLife
    {
        /// <summary>Düzenli odanın işi hızlandırması (mutfakta yemek, atölyede zanaat; yatak odasında dinlenme).</summary>
        public const double TidySpeed = 1.1;

        // ---- sera (belge 45c ek) ----
        /// <summary>Seraya ait eşya etiketi (saksılar, çiçek tarhı, cam bahçe, fidan rafı...).</summary>
        public const string GreenhouseTag = "greenhouse", GreenhouseWorkTag = "greenhouse_work";
        /// <summary>Bakımlı sera: en az 3 seraya ait eşya, biri fidan rafı ya da saksı tezgâhı.</summary>
        public const int TidyGreenhouseItems = 3;
        /// <summary>Seraya ait her eşya güzelliğe +1 (en çok +5).</summary>
        public const int GreenhouseBeautyCap = 5;

        /// <summary>Oda sera koşulunu sağlıyor mu (PlantSystem.IsGreenhouse); null ise sera odası tanınmaz.</summary>
        public Func<Room, bool> IsGreenhouseRoom { get; set; }
        /// <summary>Oda ağaç köşkü mü (TreehouseSystem.IsKioskRoom); null ise tanınmaz.</summary>
        public Func<Room, bool> IsKioskRoom { get; set; }

        private readonly Dictionary<int, (double at, double factor)> _greenhouseGrowth = new Dictionary<int, (double, double)>();

        /// <summary>Seradaki büyüme çarpanı: bakımlı serada ×1,1. Bir oyun saatine yakın önbellekli (her bitkide oda taranmasın).</summary>
        public double GreenhouseGrowth(Room room)
        {
            if (room == null) return 1.0;
            if (_greenhouseGrowth.TryGetValue(room.Id, out var c) && Now - c.at < 0.05) return c.factor;
            var (kind, tidy) = Classify(room);
            double f = kind == RoomKind.Greenhouse && tidy ? TidySpeed : 1.0;
            _greenhouseGrowth[room.Id] = (Now, f);
            return f;
        }

        /// <summary>Seraya ait eşyaların güzellik katkısı (sera değilse 0).</summary>
        public int GreenhouseBeauty(Room room)
        {
            var info = Inspect(room);
            return info.Kind == RoomKind.Greenhouse ? Math.Min(GreenhouseBeautyCap, info.GreenhouseItems) : 0;
        }

        public static string RoomKindName(RoomKind k)
        {
            switch (k)
            {
                case RoomKind.Kitchen: return "Mutfak";
                case RoomKind.Bedroom: return "Yatak odası";
                case RoomKind.Workshop: return "Atölye";
                case RoomKind.Pantry: return "Kiler";
                case RoomKind.Greenhouse: return "Sera";
                case RoomKind.TreeKiosk: return "Ağaç köşkü";
                case RoomKind.Slaughterhouse: return "Mezbaha";
                case RoomKind.Living: return "Oturma odası";
                default: return "Oda";
            }
        }

        /// <summary>Düzenli odanın adı ve etkisi: "Düzenli mutfak: yemek %10 hızlı".</summary>
        public static string TidyName(RoomKind k)
        {
            switch (k)
            {
                case RoomKind.Kitchen: return "Düzenli mutfak: yemek %10 hızlı ve öğretici";
                case RoomKind.Bedroom: return "Huzurlu yatak odası: uyku %10 dinlendirici";
                case RoomKind.Workshop: return "Düzenli atölye: zanaat %10 hızlı";
                case RoomKind.Greenhouse: return "Bakımlı sera: ekin ve fidan %10 hızlı büyür";
                case RoomKind.TreeKiosk: return "Aydınlık köşk: akşam manzarası";
                default: return null;
            }
        }

        /// <summary>Düzenli olmak için eksik olan (oyuncuya ipucu); düzenliyse ya da kural yoksa null.</summary>
        public string TidyHint(Room room)
        {
            var info = Inspect(room);
            switch (info.Kind)
            {
                case RoomKind.Kitchen:
                    if (info.Shelves == 0) return "Düzenli mutfak için duvara raf, tabak rafı ya da kancalı çıta as.";
                    if (info.SurfaceDecor < 2) return "Düzenli mutfak için raflara ve masaya en az 2 kap kacak koy.";
                    if (info.Light == 0) return "Düzenli mutfak için bir ışık koy.";
                    return null;
                case RoomKind.Workshop:
                    if (info.WallItems < 2 && !(info.WallItems >= 1 && info.Storage >= 1)) return "Düzenli atölye için alet askısı ve raf (ya da sandık) koy.";
                    if (info.Light == 0) return "Düzenli atölye için bir ışık koy.";
                    return null;
                case RoomKind.Greenhouse:
                    if (info.GreenhouseWork == 0) return "Bakımlı sera için bir fidan rafı ya da saksı tezgâhı koy.";
                    if (info.GreenhouseItems < TidyGreenhouseItems) return $"Bakımlı sera için seraya ait en az {TidyGreenhouseItems} eşya koy (saksı, çiçek tarhı, cam bahçe).";
                    return null;
                case RoomKind.TreeKiosk:
                    return info.Light == 0 ? "Aydınlık köşk için bir fener ya da mum koy." : null;
                case RoomKind.Bedroom:
                    if (info.Storage == 0) return "Huzurlu yatak odası için bir sandık ya da dolap koy.";
                    if (!info.SoftFloorOrCurtain) return "Huzurlu yatak odası için halı ser ya da pencereye perde as.";
                    return null;
            }
            return null;
        }

        private sealed class RoomInfo
        {
            public RoomKind Kind;
            public int Beds, Stoves, Stations, Pantries, Shelves, WallItems, Storage, SurfaceDecor, Light;
            public int Plants, GreenhouseItems, GreenhouseWork;
            public bool SoftFloorOrCurtain;
            public int Hooks, MeatStores;
        }

        private static bool IsBed(ItemDef d)
        {
            foreach (var i in d.Interactions)
                if (i.Need == NeedType.Energy && i.Restore >= 40f && i.AllConsumed().Count == 0) return true;
            return false;
        }

        private static bool IsStove(ItemDef d)
        {
            foreach (var i in d.Interactions) if (i.Skill == SkillType.Cooking) return true;
            return false;
        }

        private static bool IsStation(ItemDef d)
        {
            foreach (var i in d.Interactions) if (i.IsCraft && i.Skill != SkillType.Cooking && i.HomeAction == null) return true;
            return false;
        }

        private RoomInfo Inspect(Room room)
        {
            var info = new RoomInfo();
            if (room == null || !room.IsIndoor) return info;
            foreach (var it in _world.Items)
            {
                if (RoomOf(it) != room) continue;
                var d = it.Def;
                if (it.GivesLight) info.Light++;
                if (d.HasTag(Cooking.Slaughterhouse.HookTag)) info.Hooks++;
                if (d.HasTag(Cooking.Slaughterhouse.StoreTag)) info.MeatStores++;
                if (d.HasTag(GreenhouseTag)) info.GreenhouseItems++;
                if (d.HasTag(GreenhouseWorkTag)) info.GreenhouseWork++;
                if (it.IsPlant) { info.Plants++; continue; }
                if (it.IsDecor) { info.SurfaceDecor++; continue; }
                if (it.IsWallMounted)
                {
                    info.WallItems++;
                    if (d.SurfaceSlots.Count > 0) info.Shelves++;
                    if (d.WindowMount && d.Insulation > 0f) info.SoftFloorOrCurtain = true;
                    continue;
                }
                if (it.IsRug) { if (d.Id != Thatchwick.Core.Content.Decor.Debris) info.SoftFloorOrCurtain = true; continue; }
                if (IsBed(d)) info.Beds++;
                else if (IsStove(d)) info.Stoves++;
                else if (IsStation(d)) info.Stations++;
                if (d.Preserves > 0f) info.Pantries++;
                if (d.Storage > 0) info.Storage++;
            }
            if (IsKioskRoom != null && IsKioskRoom(room)) info.Kind = RoomKind.TreeKiosk;
            else if (info.Hooks > 0) info.Kind = RoomKind.Slaughterhouse;
            else if (info.Beds > 0) info.Kind = RoomKind.Bedroom;
            else if (info.Stoves > 0) info.Kind = RoomKind.Kitchen;
            else if ((info.Plants > 0 || info.GreenhouseItems > 0) && IsGreenhouseRoom != null && IsGreenhouseRoom(room)) info.Kind = RoomKind.Greenhouse;
            else if (info.Stations > 0) info.Kind = RoomKind.Workshop;
            else if (info.Pantries > 0) info.Kind = RoomKind.Pantry;
            else info.Kind = RoomKind.Living;
            return info;
        }

        /// <summary>Odanın türü ve düzenli olup olmadığı.</summary>
        public (RoomKind kind, bool tidy) Classify(Room room)
        {
            var info = Inspect(room);
            bool tidy = false;
            switch (info.Kind)
            {
                case RoomKind.Kitchen: tidy = info.Shelves >= 1 && info.SurfaceDecor >= 2 && info.Light >= 1; break;
                case RoomKind.Workshop: tidy = (info.WallItems >= 2 || (info.WallItems >= 1 && info.Storage >= 1)) && info.Light >= 1; break;
                case RoomKind.Bedroom: tidy = info.Storage >= 1 && info.SoftFloorOrCurtain; break;
                case RoomKind.Greenhouse: tidy = info.GreenhouseWork >= 1 && info.GreenhouseItems >= TidyGreenhouseItems; break;
                case RoomKind.TreeKiosk: tidy = info.Light >= 1; break;
                case RoomKind.Slaughterhouse: tidy = info.MeatStores >= 1; break;
            }
            return (info.Kind, tidy);
        }

        /// <summary>Oda paneli satırı: "Mutfak · düzenli · Loş · is 3".</summary>
        public string RoomSummary(Room room)
        {
            if (room == null || !room.IsIndoor) return null;
            var (kind, tidy) = Classify(room);
            var parts = new List<string> { RoomKindName(kind) };
            if (tidy) parts.Add("düzenli");
            parts.Add($"ışık {LightName(LightLevel(room)).ToLowerInvariant()} ({LightPoints(room)})");
            double soot = SootOf(room);
            if (soot >= 1) parts.Add($"is {soot:0}");
            if (IsWhitewashed(room)) parts.Add("ak badana");
            return string.Join(" · ", parts);
        }

        private string RoomName(Room room)
        {
            var (kind, _) = Classify(room);
            switch (kind)
            {
                case RoomKind.Kitchen: return "Mutfağın";
                case RoomKind.Bedroom: return "Yatak odasının";
                case RoomKind.Workshop: return "Atölyenin";
                case RoomKind.Pantry: return "Kilerin";
                case RoomKind.Greenhouse: return "Seranın";
                default: return "Odanın";
            }
        }
    }
}
