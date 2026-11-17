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
        /// <summary>Yapının katkısı: zemin ve duvar kaplamalarının ortalama konforu (en çok 6).</summary>
        public int Structure;
        /// <summary>Konfor puanı (eşya konforu + yapı). Stil seti uyumu ayrı moodlet'tir (RoomMoodSystem: Uyumlu ev).</summary>
        public int Total => Score + Structure;
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

        /// <summary>Yapı parçası kataloğu (zemin ve kaplama konforu için); null ise yapı katkısı yok.</summary>
        public ContentDatabase Content { get; set; }

        /// <summary>İç mekân odasının konforu: eşya konfor toplamı + stil seti bonusu (aynı setten 4+ eşya).</summary>
        /// <summary>Eşyanın odaya ek konforu (yadigâr). Null ise yok.</summary>
        public Func<ItemInstance, int> ExtraComfort { get; set; }

        public RoomComfort ComfortOf(Room room)
        {
            var result = new RoomComfort();
            if (room == null || !room.IsIndoor) return result;
            var tiles = new HashSet<GridPos>(room.Tiles);
            var styleCount = new Dictionary<string, int>();
            foreach (var item in _world.Items)
            {
                if (!tiles.Contains(item.Position)) continue;
                result.Score += item.Def.Comfort + (ExtraComfort != null ? ExtraComfort(item) : 0);
                if (!string.IsNullOrEmpty(item.Def.StyleSet))
                    styleCount[item.Def.StyleSet] = (styleCount.TryGetValue(item.Def.StyleSet, out var n) ? n : 0) + 1;
            }
            foreach (var kv in styleCount)
                if (kv.Value >= StyleSetMinItems) { result.StyleSet = kv.Key; result.StyleBonus = StyleSetBonus; break; }
            result.Structure = StructureComfort(room);
            return result;
        }

        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        // Zemin konforu (karoların ortalaması) + iç duvar yüzü kaplamalarının ortalaması; ikisi de 0-3.
        private int StructureComfort(Room room)
        {
            if (Content == null || room.Tiles.Count == 0) return 0;
            var map = _world.Map;
            double floor = 0;
            foreach (var t in room.Tiles)
            {
                var f = map.GetFloor(t);
                if (f == FloorType.Grass) continue;
                var style = map.GetFloorStyle(t);
                var part = style != null && Content.Parts.TryGet(style, out var p) ? p : BuildParts.DefaultFloor(Content, f);
                if (part != null) floor += part.Comfort;
            }
            floor /= room.Tiles.Count;
            double walls = 0; int faces = 0;
            foreach (var t in room.Tiles)
                foreach (var d in Dirs)
                {
                    var e = GridMap.EdgeBetween(t, t + d);
                    if (!map.EdgeInBounds(e) || map.GetEdge(e) != EdgeKind.Wall) continue;
                    faces++;
                    var fin = map.GetFinish(e, e.SideB == t);
                    if (fin != null && Content.Parts.TryGet(fin, out var fp)) walls += fp.Comfort;
                }
            if (faces > 0) walls /= faces;
            return Math.Min(6, (int)Math.Round(floor + walls));
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
