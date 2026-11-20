using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>
    /// Eşya yönü ile görsel arasındaki tek bağ (yerleşim sistemi, belge 44 B).
    /// Ön yüzü olan eşyanın dönüş 0'daki yönü ayak izinden gelir: yatay (W&gt;H) ve kare eşya N'ye, dikey (H&gt;W) eşya W'ye bakar;
    /// her dönüş saat yönünde bir adım çevirir (N → E → S → W). Bir yön ancak görseli (Art.Faces) ya da sol/sağ simetrik
    /// eşyada aynasının görseli varsa geçerlidir; arkası dönük (E, S) görünüm aynayla elde edilmez.
    /// Ön yüzü olmayan eşya eksen görselleriyle çizilir (AlongX çift, AlongY tek dönüş; eksik eksen yalnızca simetrik
    /// eşyada aynayla). Görseli hiç olmayan eşyada her dönüş geçerli (yer tutucu kutu yönsüzdür).
    /// </summary>
    public static class ItemFacing
    {
        /// <summary>
        /// Dönüş 0'daki yön. Çok karolu eşyada ayak izinden (uzun kenar ön yüzdür); tek karoda kendi görselinin baktığı yön
        /// (N, yoksa W, E, S): eski kayıtlarda dönüş 0 hep aynı görselle çizilmişti, görünüş değişmesin.
        /// </summary>
        public static Dir4 BaseFacing(ItemDef def)
        {
            if (def.Width != def.Height) return def.Height > def.Width ? Dir4.W : Dir4.N;
            var art = def.Art;
            if (art != null && art.HasFaces)
                foreach (var d in BaseOrder) if (art.Faces[(int)d] != null) return d;
            return Dir4.N;
        }

        private static readonly Dir4[] BaseOrder = { Dir4.N, Dir4.W, Dir4.E, Dir4.S };

        /// <summary>Dönüşe göre yön (ön yüzü olmayan eşyada da hesaplanır; yön kuralı yalnızca ön yüzlüde işler).</summary>
        public static Dir4 FacingFor(ItemDef def, int rotation) => Facing.Rotate(BaseFacing(def), Footprint.Normalize(rotation));

        /// <summary>İstenen yön için dönüş.</summary>
        public static int RotationFor(ItemDef def, Dir4 facing) => (((int)facing - (int)BaseFacing(def)) % 4 + 4) % 4;

        /// <summary>Yerleşmiş yer eşyasının ön yüzünün yönü; ön yüzü yoksa (ya da duvar/dekor/halı ise) null.</summary>
        public static Dir4? Of(ItemInstance item)
        {
            if (item == null || !item.Def.HasFront || !PlacementValidator.Occupies(item)) return null;
            return FacingFor(item.Def, item.Rotation);
        }

        /// <summary>
        /// Dönüş için çizilecek görsel ve ayna. Görsel yoksa ya da bu yön geçersizse false (sprite null).
        /// lit: ısı kaynağı yanıyorsa Art.Lit (aynı ayna ile).
        /// </summary>
        public static bool TryView(ItemDef def, int rotation, bool lit, out string sprite, out bool flip)
        {
            sprite = null; flip = false;
            var art = def?.Art;
            if (art == null) return false;
            int r = Footprint.Normalize(rotation);
            if (art.Rotations.Count > 0 && !art.Rotations.Contains(r)) return false;
            if (def.HasFront && art.HasFaces)
            {
                var f = FacingFor(def, r);
                if (art.Faces[(int)f] != null) sprite = art.Faces[(int)f];
                else if (art.Mirror && art.Faces[(int)Facing.Mirror(f)] != null) { sprite = art.Faces[(int)Facing.Mirror(f)]; flip = true; }
                if (sprite == null) return false;
                if (lit && art.Lit != null) sprite = art.Lit;
                return true;
            }
            bool even = r % 2 == 0;
            if (def.Width == def.Height)
            {
                sprite = art.AlongX ?? art.AlongY;
                if (sprite == null) return false;
                flip = art.Mirror && !even;   // kare simetrik eşya: döndürme görünsün diye tek dönüşte ayna
            }
            else
            {
                // Dönüşten sonra uzun kenar grid x boyunca mı: 2x1 eşya çift dönüşte, 1x2 eşya (merdiven, ahır bölmesi) tek dönüşte.
                bool alongX = even == (def.Width > def.Height);
                var want = alongX ? art.AlongX : art.AlongY;
                var other = alongX ? art.AlongY : art.AlongX;
                if (want != null) sprite = want;
                else if (art.Mirror && other != null) { sprite = other; flip = true; }
                else return false;
            }
            if (lit && art.Lit != null) sprite = art.Lit;
            return true;
        }

        /// <summary>Bu dönüşün görseli var mı (ya da eşyanın hiç görseli yok mu: yer tutucu her yöne döner).</summary>
        public static bool IsValid(ItemDef def, int rotation)
        {
            var a = def?.Art;
            if (a == null || (!a.HasFaces && a.AlongX == null && a.AlongY == null)) return true;   // görsel yok ya da yalnızca bitki evresi
            return TryView(def, rotation, false, out _, out _);
        }

        public static List<int> ValidRotations(ItemDef def)
        {
            var list = new List<int>();
            for (int r = 0; r < 4; r++) if (IsValid(def, r)) list.Add(r);
            return list;
        }

        /// <summary>Döndür düğmesi: yalnızca geçerli yönler arasında döner (hiç geçerli yön yoksa bir adım).</summary>
        public static int NextRotation(ItemDef def, int current)
        {
            for (int i = 1; i <= 4; i++)
            {
                int r = (Footprint.Normalize(current) + i) % 4;
                if (IsValid(def, r)) return r;
            }
            return (Footprint.Normalize(current) + 1) % 4;
        }

        /// <summary>Geçerli bir dönüş: istenen geçerliyse o, değilse ilk geçerli (yoksa istenen).</summary>
        public static int Snap(ItemDef def, int rotation)
        {
            if (IsValid(def, rotation)) return Footprint.Normalize(rotation);
            var v = ValidRotations(def);
            return v.Count > 0 ? v[0] : Footprint.Normalize(rotation);
        }

        /// <summary>Ön yüzlü eşyanın önündeki karolar (FrontDepth derinliğinde, ayak izinin ön kenarı boyunca).</summary>
        public static IEnumerable<GridPos> InteractionCells(ItemInstance item)
        {
            var f = Of(item);
            if (!f.HasValue) yield break;
            foreach (var c in FrontCells(item.Def, item.Position, item.Rotation, item.Def.FrontDepth)) yield return c;
        }

        /// <summary>Taslak yerleşim için ön karolar (eşya henüz konmamışken).</summary>
        public static IEnumerable<GridPos> FrontCells(ItemDef def, GridPos pos, int rotation, int depth = 1)
        {
            if (!def.HasFront) yield break;
            var f = FacingFor(def, rotation);
            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rotation);
            var foot = new HashSet<GridPos>();
            for (int dy = 0; dy < h; dy++) for (int dx = 0; dx < w; dx++) foot.Add(new GridPos(pos.X + dx, pos.Y + dy, pos.Z));
            foreach (var t in foot)
            {
                var n = Facing.Step(t, f);
                if (foot.Contains(n)) continue;
                for (int d = 0; d < System.Math.Max(1, depth); d++) yield return Facing.Step(t, f, d + 1);
            }
        }

        /// <summary>Ayak izinin arka kenarındaki karolar ve arkalarındaki kenar (uzun eşya duvara sırt verir).</summary>
        public static IEnumerable<(GridPos tile, Edge back)> BackEdges(ItemDef def, GridPos pos, int rotation, Dir4 facing)
        {
            var back = Facing.Opposite(facing);
            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rotation);
            var foot = new HashSet<GridPos>();
            for (int dy = 0; dy < h; dy++) for (int dx = 0; dx < w; dx++) foot.Add(new GridPos(pos.X + dx, pos.Y + dy, pos.Z));
            foreach (var t in foot)
                if (!foot.Contains(Facing.Step(t, back))) yield return (t, Facing.EdgeOf(t, back));
        }
    }
}
