using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Mood;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Bir odanın konfor özeti.</summary>
    public sealed class RoomComfort
    {
        public int Score;
        public string StyleSet;      // uyumlu oda veren set; yoksa null
        public int StyleBonus;
        /// <summary>Konfor puanı (eşya konforu). Stil seti uyumu ayrı moodlet'tir (RoomMoodSystem: Uyumlu ev).</summary>
        public int Total => Score;
    }

    /// <summary>
    /// Ruh hali kaynakları (Adım 32): oda konforu (eşya konfor puanı toplamı) ve yas. Soğuk, açlık, kirlilik gibi
    /// ihtiyaç moodlet'leri Character.Tick'te, stil uyumu/ışık RoomMoodSystem'de, yemek-sohbet-eğlence ilgili
    /// sistemlerde verilir (Core.Mood).
    /// </summary>
    public sealed class MoodService
    {
        public const string ComfortId = "mood.comfort";

        public const int StyleSetMinItems = 4;
        public const int StyleSetBonus = 5;
        public const double RefreshDays = 0.15;
        public const int ComfortHigh = 25, ComfortMid = 12;

        private readonly WorldState _world;

        public MoodService(WorldState world) { _world = world ?? throw new ArgumentNullException(nameof(world)); }

        /// <summary>İç mekân odasının konforu: eşya konfor toplamı + stil seti bonusu (aynı setten 4+ eşya).</summary>
        public RoomComfort ComfortOf(Room room)
        {
            var result = new RoomComfort();
            if (room == null || !room.IsIndoor) return result;
            var tiles = new HashSet<GridPos>(room.Tiles);
            var styleCount = new Dictionary<string, int>();
            foreach (var item in _world.Items)
            {
                if (!tiles.Contains(item.Position)) continue;
                result.Score += item.Def.Comfort;
                if (!string.IsNullOrEmpty(item.Def.StyleSet))
                    styleCount[item.Def.StyleSet] = (styleCount.TryGetValue(item.Def.StyleSet, out var n) ? n : 0) + 1;
            }
            foreach (var kv in styleCount)
                if (kv.Value >= StyleSetMinItems) { result.StyleSet = kv.Key; result.StyleBonus = StyleSetBonus; break; }
            return result;
        }

        public static void ComfortMood(int total, out int value, out string text)
        {
            if (total >= ComfortHigh) { value = 12; text = "Rahat ve güzel bir ev"; }
            else if (total >= ComfortMid) { value = 6; text = "Düzenli bir ev"; }
            else if (total > 0) { value = 2; text = "Yaşanır bir ev"; }
            else { value = 0; text = null; }
        }

        /// <summary>Tüm yaşayanların konfor ve soğuk moodlet'lerini yeniler.</summary>
        public void Update(IEnumerable<Character> characters)
        {
            var rooms = _world.Map.Rooms;
            foreach (var c in characters)
            {
                if (c.IsDead) continue;
                var room = rooms.RoomAt(c.Position);
                var comfort = ComfortOf(room);
                ComfortMood(comfort.Total, out int v, out string text);
                if (v != 0) c.AddMoodlet(ComfortId, text, v, RefreshDays); else c.Mood.Remove(ComfortId);

            }
        }

        /// <summary>Ölenin yakınlarına (eş, ebeveyn, çocuk, kardeş) yas moodlet'i verir (MoodEvents.OnDeath); kaç kişi etkilendi döner.</summary>
        public int Mourn(Character dead, IEnumerable<Character> members) => MoodEvents.OnDeath(dead, members);
    }
}
