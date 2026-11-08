using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Mood
{
    /// <summary>
    /// Oda kaynaklı moodlet'ler: aynı stil setinden 4+ eşya olan iç oda "Uyumlu ev", ışık eşyası
    /// (light etiketli: kandil, meşale, lamba) olan iç oda "Aydınlık oda" verir. Karakter odadayken
    /// moodlet tazelenir, çıkınca süresi bitene kadar kalır. Pahalı olmasın diye aralıklı çalışır.
    /// </summary>
    public sealed class RoomMoodSystem
    {
        public const int HarmonyThreshold = 4;
        public const double IntervalDays = 0.05;

        private readonly WorldState _world;
        private readonly MoodCatalog _catalog;
        private double _accum = IntervalDays;   // ilk Tick hemen değerlendirir

        public RoomMoodSystem(WorldState world, MoodCatalog catalog = null)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _catalog = catalog ?? MoodCatalog.Default;
        }

        public void Tick(double gameDays, IEnumerable<Character> characters)
        {
            _accum += gameDays;
            if (_accum < IntervalDays) return;
            _accum = 0;

            var cache = new Dictionary<int, (string set, bool light)>();
            foreach (var c in characters)
            {
                if (c.IsDead || !c.MoodEnabled) continue;
                var room = _world.Map.Rooms.RoomAt(c.Position);
                if (room == null || !room.IsIndoor) continue;
                if (!cache.TryGetValue(room.Id, out var info))
                {
                    info = (HarmonySet(room, _world), HasLight(room, _world));
                    cache[room.Id] = info;
                }
                if (info.set != null) c.AddMoodlet(MoodCatalog.Harmony, _catalog);
                if (info.light) c.AddMoodlet(MoodCatalog.BrightRoom, _catalog);
            }
        }

        private static IEnumerable<ItemInstance> ItemsIn(Room room, WorldState world)
        {
            var tiles = new HashSet<GridPos>(room.Tiles);
            foreach (var item in world.Items)
                if (tiles.Contains(item.Position)) yield return item;
        }

        /// <summary>Odada eşik kadar eşyası olan stil seti (en çok olan); yoksa null.</summary>
        public static string HarmonySet(Room room, WorldState world, int threshold = HarmonyThreshold)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var item in ItemsIn(room, world))
            {
                var set = item.Def.StyleSet;
                if (string.IsNullOrEmpty(set)) continue;
                counts.TryGetValue(set, out int n);
                counts[set] = n + 1;
            }
            string best = null;
            int bestCount = 0;
            foreach (var kv in counts)
                if (kv.Value >= threshold && (kv.Value > bestCount || (kv.Value == bestCount && string.CompareOrdinal(kv.Key, best) < 0)))
                {
                    best = kv.Key;
                    bestCount = kv.Value;
                }
            return best;
        }

        public static bool HasLight(Room room, WorldState world)
        {
            foreach (var item in ItemsIn(room, world))
                if (item.Def.HasTag("light")) return true;
            return false;
        }
    }

    /// <summary>Aile olaylarının moodlet'leri (evlilik, doğum, ölüm).</summary>
    public static class MoodEvents
    {
        public static void OnMarriage(Character a, Character b)
        {
            a?.AddMoodlet(MoodCatalog.Newlywed);
            b?.AddMoodlet(MoodCatalog.Newlywed);
        }

        public static void OnBirth(Character mother, Character father)
        {
            mother?.AddMoodlet(MoodCatalog.NewBaby);
            father?.AddMoodlet(MoodCatalog.NewBaby);
        }

        /// <summary>Ölenin eşi, anne-babası, çocukları ve kardeşleri yas tutar (yaşayan hane üyeleri).</summary>
        public static int OnDeath(Character dead, IEnumerable<Character> members)
        {
            int n = 0;
            foreach (var c in members)
            {
                if (c == dead || c.IsDead) continue;
                if (IsClose(dead, c) && c.AddMoodlet(MoodCatalog.Grief)) n++;
            }
            return n;
        }

        public static bool IsClose(Character a, Character b)
        {
            if (a.SpouseId == b.Id || b.SpouseId == a.Id) return true;
            if (a.BirthMotherId == b.Id || a.BirthFatherId == b.Id) return true;
            if (b.BirthMotherId == a.Id || b.BirthFatherId == a.Id) return true;
            bool sameMother = a.BirthMotherId.HasValue && a.BirthMotherId == b.BirthMotherId;
            bool sameFather = a.BirthFatherId.HasValue && a.BirthFatherId == b.BirthFatherId;
            return sameMother || sameFather;
        }
    }
}
