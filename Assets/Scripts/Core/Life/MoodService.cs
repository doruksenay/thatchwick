using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
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
        /// <summary>Odanın büyüklüğü: dar oda eksi, ferah oda artı (-3 ile +3 arası).</summary>
        public int Size;
        /// <summary>Pencereler: penceresiz oda eksi, her pencere artı (-2 ile +3 arası; bodrumda 0).</summary>
        public int Windows;
        /// <summary>Konfor puanı (eşya konforu + yapı + büyüklük + pencere). Stil seti uyumu ayrı moodlet'tir (RoomMoodSystem: Uyumlu ev).</summary>
        public int Total => Score + Structure + Size + Windows;

        /// <summary>Oyuncuya gösterilen döküm: "Eşya 14 · Zemin ve duvar 3 · Büyüklük +2 · Pencere -2 (penceresiz)".</summary>
        public string Breakdown()
        {
            string sign(int v) => v > 0 ? "+" + v : v.ToString();
            string win = Windows < 0 ? " (penceresiz)" : "";
            string size = Size < 0 ? " (dar)" : "";
            return $"Eşya {Score} · Zemin ve duvar {Structure} · Büyüklük {sign(Size)}{size} · Pencere {sign(Windows)}{win}";
        }
    }

    /// <summary>
    /// Bir odanın güzellik özeti (Project belgesi 27): eşyaların görünüşü. Konfordan (fiziksel rahatlık) ayrıdır.
    /// </summary>
    public sealed class RoomBeauty
    {
        /// <summary>Eşyaların güzelliği (kategori tavanlı; hasar, kararma ve alttaki halılar düşülür).</summary>
        public int Items;
        /// <summary>Çeşitlilik: farklı katmanlardan güzel eşya (halı, duvar, yüzey, tavan, ışık).</summary>
        public int Variety;
        /// <summary>Eksiler (döküntü, is yapan mum, kırık eşya).</summary>
        public int Penalty;
        /// <summary>Üst kademe (Tüccar, Asil) eşya sayısı: "Göz alıcı" için en az 2.</summary>
        public int FineItems;
        public int Area;
        /// <summary>Evin bakımından gelen artı (duvar kaplaması, ak badana, ferah ot serpmesi; belge 45).</summary>
        public int Home;
        /// <summary>Ham toplam (eşya + çeşitlilik + ev - eksi).</summary>
        public int Raw => Items + Variety + Home - Penalty;
        /// <summary>Alana göre ölçekli puan: 3x3 odada ham toplam; büyük oda aynı eşyayla daha düşük.</summary>
        public int Score => (int)Math.Round(Raw * 3.0 / Math.Sqrt(Math.Max(9, Area)));
        public int Tier => MoodService.BeautyTier(Score, FineItems);

        public string Breakdown()
        {
            return $"Eşya {Items} · Çeşitlilik +{Variety}" + (Home > 0 ? $" · Duvar ve bakım +{Home}" : "") + (Penalty > 0 ? $" · Eksi -{Penalty}" : "") + $" · Alan {Area} karo → {Score}";
        }
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
        /// <summary>Eşyanın güzelliğine ek (hatıra eskidikçe, soy ağacı nesille, solmuş mevsim süsü eksi); null ise yok (D3b).</summary>
        public Func<ItemInstance, double> BeautyBonus { get; set; }
        /// <summary>Odanın bakımından gelen artı ve eksi (duvar kaplaması, badana, is, ot serpmesi; belge 45); null ise yok.</summary>
        public Func<Room, (int plus, int minus)> RoomBeautyExtra { get; set; }

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
            result.Size = SizeComfort(room.Area);
            result.Windows = WindowComfort(room);
            return result;
        }

        /// <summary>
        /// Oda büyüklüğü (karo): 1-3 dar (-3), 4-5 küçük (-1), 6-11 olağan (0), 12-19 geniş (+1),
        /// 20-29 ferah (+2), 30+ çok ferah (+3). Sayılar yer tutucu (Adım 28).
        /// </summary>
        public static int SizeComfort(int area)
        {
            if (area <= 0) return 0;
            if (area <= 3) return -3;
            if (area <= 5) return -1;
            if (area <= 11) return 0;
            if (area <= 19) return 1;
            if (area <= 29) return 2;
            return 3;
        }

        /// <summary>
        /// Pencere: penceresiz oda -2; 1 pencere +1, 2 pencere +2, 3 ve üstü +3.
        /// Bodrum (kat -1 ve altı) pencere beklemez: 0.
        /// </summary>
        public static int WindowComfort(Room room)
        {
            if (room == null || room.Level < 0) return 0;
            int n = room.WindowCount;
            if (n <= 0) return -2;
            return Math.Min(3, n);
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

        // ---- güzellik (Project belgesi 27) ----
        public const string BeautyId = "mood.beauty";
        public const int BeautyCategoryCap = 15;
        public const int BeautyNice = 8, BeautyLovely = 16, BeautyStunning = 28;

        /// <summary>Puan basamağı: 0 sade, 1 hoş, 2 güzel, 3 göz alıcı (üst kademeden en az 2 eşya ister).</summary>
        public static int BeautyTier(int score, int fineItems)
        {
            if (score >= BeautyStunning && fineItems >= 2) return 3;
            if (score >= BeautyLovely) return 2;
            if (score >= BeautyNice) return 1;
            return 0;
        }

        public static void BeautyMood(int tier, out int value, out string text)
        {
            switch (tier)
            {
                case 3: value = 15; text = "Göz alıcı oda"; break;
                case 2: value = 10; text = "Güzel oda"; break;
                case 1: value = 5; text = "Hoş oda"; break;
                default: value = 0; text = null; break;
            }
        }

        /// <summary>Eşyanın odaya kattığı güzellik (durum ve kararma çarpanlı; döküntü eksi).</summary>
        public static double BeautyOfItem(ItemInstance item, ContentDatabase content = null)
        {
            // Doldurulan dekor: dolulukla artar, çürükte eksi, kendi ürünüyle ek (D2).
            double b = item.Def.IsFillDecor && content != null ? DecorSystem.FillBeauty(item, content) : item.Def.Beauty;
            if (b > 0)
            {
                b *= Decor.BeautyFactor(item.Condition);
                if (item.IsTarnished) b *= 0.5;
            }
            return b;
        }

        public RoomBeauty BeautyOf(Room room)
        {
            var result = new RoomBeauty();
            if (room == null || !room.IsIndoor) return result;
            result.Area = room.Area;
            var tiles = new HashSet<GridPos>(room.Tiles);
            var perCategory = new Dictionary<string, double>(StringComparer.Ordinal);
            bool rug = false, wall = false, surface = false, ceiling = false, light = false;
            double penalty = 0;
            foreach (var item in _world.Items)
            {
                if (!tiles.Contains(item.Position)) continue;
                double b = BeautyOfItem(item, Content);
                if (BeautyBonus != null) b += BeautyBonus(item);
                if (item.IsRug && b > 0)
                {
                    // Halılar üst üste serilir: güzelliğe yalnızca en üstte kaldığı karolar sayılır.
                    int top = 0, all = 0;
                    foreach (var t in item.OccupiedTiles()) { all++; if (_world.GetRugAt(t) == item) top++; }
                    b = all > 0 ? b * top / all : 0;
                }
                if (b < 0) { penalty -= b; continue; }
                if (b <= 0) continue;
                string cat = item.Def.Category ?? "";
                perCategory[cat] = (perCategory.TryGetValue(cat, out var c) ? c : 0) + b;
                if (item.IsRug) rug = true;
                else if (item.IsWallMounted) wall = true;
                else if (item.IsDecor) surface = true;
                else if (item.IsCeiling) ceiling = true;
                if (item.GivesLight) light = true;
                if (item.Def.StyleSet == "Tüccar" || item.Def.StyleSet == "Asil") result.FineItems++;
            }
            double sum = 0;
            foreach (var kv in perCategory) sum += Math.Min(BeautyCategoryCap, kv.Value);
            result.Items = (int)Math.Round(sum);
            int kinds = (rug ? 1 : 0) + (wall ? 1 : 0) + (surface ? 1 : 0) + (ceiling ? 1 : 0) + (light ? 1 : 0);
            result.Variety = kinds >= 2 ? 2 * (kinds - 1) : 0;
            result.Penalty = (int)Math.Round(penalty);
            if (RoomBeautyExtra != null)
            {
                var (plus, minus) = RoomBeautyExtra(room);
                result.Home = Math.Max(0, plus);
                result.Penalty += Math.Max(0, minus);
            }
            return result;
        }

        /// <summary>Bu puan ve altı "sıkışık oda" moodlet'i verir (dar ve penceresiz, eşyasız oda).</summary>
        public const int ComfortCramped = -4;

        public static void ComfortMood(int total, out int value, out string text)
        {
            if (total >= ComfortHigh) { value = 12; text = "Rahat ve güzel bir ev"; }
            else if (total >= ComfortMid) { value = 6; text = "Düzenli bir ev"; }
            else if (total > 0) { value = 2; text = "Yaşanır bir ev"; }
            else if (total <= ComfortCramped) { value = -4; text = "Sıkışık, karanlık bir oda"; }
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
                // Güzellik (27): ayrı moodlet; yalnızca iç odada.
                BeautyMood(BeautyOf(room).Tier, out int bv, out string btext);
                if (bv != 0) c.AddMoodlet(BeautyId, btext, bv, RefreshDays); else c.Mood.Remove(BeautyId);

            }
        }

        /// <summary>Ölenin yakınlarına (eş, ebeveyn, çocuk, kardeş) yas moodlet'i verir (MoodEvents.OnDeath); kaç kişi etkilendi döner.</summary>
        public int Mourn(Character dead, IEnumerable<Character> members) => MoodEvents.OnDeath(dead, members);
    }
}
