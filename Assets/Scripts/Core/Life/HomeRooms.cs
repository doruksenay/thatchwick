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
        Pantry = 5
    }

    public sealed partial class HomeLife
    {
        /// <summary>Düzenli odanın işi hızlandırması (mutfakta yemek, atölyede zanaat; yatak odasında dinlenme).</summary>
        public const double TidySpeed = 1.1;

        public static string RoomKindName(RoomKind k)
        {
            switch (k)
            {
                case RoomKind.Kitchen: return "Mutfak";
                case RoomKind.Bedroom: return "Yatak odası";
                case RoomKind.Workshop: return "Atölye";
                case RoomKind.Pantry: return "Kiler";
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
            public bool SoftFloorOrCurtain;
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
            if (info.Beds > 0) info.Kind = RoomKind.Bedroom;
            else if (info.Stoves > 0) info.Kind = RoomKind.Kitchen;
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
                default: return "Odanın";
            }
        }
    }
}
