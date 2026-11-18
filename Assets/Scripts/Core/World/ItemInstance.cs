using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.World
{
    /// <summary>
    /// Haritaya yerleştirilmiş bir eşya (ItemDef'in örneği). Yer eşyası bir kapladığı alan
    /// (Position = sol-üst/en küçük köşe, Rotation = 90° adımlarla saat yönü) ile tanımlanır.
    /// Dekor eşyası bir başka eşyanın yuvasında durur (ParentId + SlotId) ve karo kaplamaz.
    /// </summary>
    public sealed class ItemInstance
    {
        public int Id { get; }
        public ItemDef Def { get; }
        public GridPos Position { get; internal set; }
        /// <summary>0-3: 90° adımlarla saat yönünde dönüş.</summary>
        public int Rotation { get; internal set; }
        /// <summary>Dekor ise üzerinde durduğu eşyanın kimliği.</summary>
        public int? ParentId { get; internal set; }
        public string SlotId { get; internal set; }

        public bool IsDecor => ParentId.HasValue;

        /// <summary>Duvara asılı eşya: Position = önündeki karo, Rotation = asıldığı kenar (WallSide).</summary>
        public bool IsWallMounted => Def.Placement == ItemPlacement.Wall && !IsDecor;
        /// <summary>Duvar eşyasının asıldığı (ilk) kenar.</summary>
        public Edge WallEdge => WallSides.EdgeOf(Position, Rotation);
        /// <summary>Tavana asılı eşya (kiriş kancası, asma fener): karo kaplamaz.</summary>
        public bool IsCeiling => Def.Placement == ItemPlacement.Ceiling && !IsDecor;
        /// <summary>Duvar eşyasının bandı (Decor.BandLow, BandMid, BandHigh).</summary>
        public int WallBand { get; internal set; } = Decor.BandMid;
        /// <summary>Duvar eşyasının kapladığı kenar sayısı (geniş duvar halısı 2-3).</summary>
        public int WallSpan => IsWallMounted ? Math.Max(1, Def.WallWidth) : 0;

        /// <summary>Geniş duvar eşyasının bütün kenarları ve önlerindeki karolar (kenar boyunca + yönde).</summary>
        public IEnumerable<(GridPos tile, Edge edge)> WallSegments()
        {
            if (!IsWallMounted) yield break;
            foreach (var seg in WallSides.Segments(Position, Rotation, WallSpan)) yield return seg;
        }

        public bool HangsOn(Edge edge)
        {
            foreach (var seg in WallSegments()) if (seg.edge.Equals(edge)) return true;
            return false;
        }

        // ---- ev dekorasyonu (Project belgesi 27) ----
        /// <summary>Durum: çöken raftan düşen eşya hasar alır; onarılan eşya "Onarılmış" kalır.</summary>
        public ItemCondition Condition { get; set; }
        /// <summary>Halı sırası: büyük olan üstte (halılar sınırsız üst üste serilir).</summary>
        public int LayerOrder { get; internal set; }
        /// <summary>Metal aynanın son cilasından beri geçen gün.</summary>
        public double TarnishDays { get; set; }
        public bool IsTarnished => Def.TarnishDays > 0f && TarnishDays >= Def.TarnishDays;
        private List<StoredItem> _contents;
        /// <summary>İçindekiler (döküntü yığınındaki eşyalar ve kırık parçalar; doldurulan dekordaki gerçek ürünler).</summary>
        public List<StoredItem> Contents => _contents ?? (_contents = new List<StoredItem>());
        public bool HasContents => _contents != null && _contents.Count > 0;

        // ---- barınak (Adım: hayvanlar) ----
        /// <summary>Barınaktaki yem stoğu (birim).</summary>
        public double FeedStock { get; internal set; }
        /// <summary>Mobilya kaplaması (renk/malzeme örneği, yapı parçası kimliği); null = varsayılan.</summary>
        public string FinishId { get; internal set; }
        /// <summary>Açılıp kapanan düzenek (su kapağı) kapalı mı.</summary>
        public bool Toggled { get; set; }
        /// <summary>Lazımlık/hela dolumu (kullanım sayısı).</summary>
        public int Fill { get; set; }
        public bool IsRug => Def.IsRug;
        private Dictionary<string, int> _stock;
        /// <summary>Barınakta toplanmayı bekleyen ürünler (yumurta, süt, yün).</summary>
        public IReadOnlyDictionary<string, int> Stock => _stock ?? (_stock = new Dictionary<string, int>());
        public int StockTotal { get { int n = 0; if (_stock != null) foreach (var v in _stock.Values) n += v; return n; } }

        internal void AddStock(string itemId, int count)
        {
            if (_stock == null) _stock = new Dictionary<string, int>();
            _stock.TryGetValue(itemId, out int n);
            _stock[itemId] = n + count;
        }

        internal void SetStock(string itemId, int count)
        {
            if (_stock == null) _stock = new Dictionary<string, int>();
            if (count > 0) _stock[itemId] = count; else _stock.Remove(itemId);
        }

        /// <summary>Isı kaynağının kalan yanma süresi (oyun günü). 0 = sönük.</summary>
        public double FuelDays { get; internal set; }
        public bool IsLit => FuelDays > 0;

        // ---- çiftçilik durumu (PlantSystem yönetir) ----
        /// <summary>Ekin bugün sulandı (her yeni gün sıfırlanır; yağmur ve hendek de sular).</summary>
        public bool Watered { get; internal set; }
        /// <summary>Ekinin büyüme koşullarında geçirdiği toplam süre ve bunun sulu geçen kısmı (kalite için).</summary>
        public double AgeDays { get; internal set; }
        public double WetDays { get; internal set; }
        /// <summary>0-1 sulu geçen süre oranı.</summary>
        public float WetRatio => AgeDays <= 0 ? 0f : (float)System.Math.Min(1.0, WetDays / AgeDays);
        /// <summary>Meyve ağacında meyve olgun mu ve olgunlaşma ilerlemesi (gün).</summary>
        public bool FruitReady { get; internal set; }
        public double FruitDays { get; internal set; }

        internal void SetPlantState(bool watered, double ageDays, double wetDays, bool fruitReady, double fruitDays)
        {
            Watered = watered; AgeDays = ageDays; WetDays = wetDays; FruitReady = fruitReady; FruitDays = fruitDays;
        }

        /// <summary>Bitkinin büyüme ilerlemesi (oyun günü).</summary>
        public double GrowthDays { get; internal set; }
        public bool IsPlant => Def.IsPlant;
        /// <summary>Bitki değilse ya da olgunlaştıysa true.</summary>
        public bool IsGrown => !IsPlant || GrowthDays >= Def.GrowDays;
        /// <summary>0-1 büyüme oranı (bitki değilse 1).</summary>
        public float GrowthFraction => IsPlant ? (float)System.Math.Min(1.0, GrowthDays / Def.GrowDays) : 1f;

        /// <summary>Dönüş uygulanmış genişlik/yükseklik.</summary>
        public int Width => IsDecor || IsWallMounted || IsCeiling ? 1 : Footprint.Dimensions(Def.Width, Def.Height, Rotation).w;
        public int Height => IsDecor || IsWallMounted || IsCeiling ? 1 : Footprint.Dimensions(Def.Width, Def.Height, Rotation).h;

        public ItemInstance(int id, ItemDef def, GridPos position, int rotation = 0)
        {
            Id = id;
            Def = def;
            Position = position;
            Rotation = rotation;
        }

        /// <summary>Kapladığı karolar (dekor ve duvar eşyası için boş).</summary>
        public IEnumerable<GridPos> OccupiedTiles()
        {
            if (IsDecor || IsWallMounted || IsCeiling) yield break;
            int w = Width, h = Height;
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    yield return new GridPos(Position.X + dx, Position.Y + dy, Position.Z);
        }

        /// <summary>Bir yüzey yuvasının ızgara koordinatındaki (sürekli, karo köşesi 0) konumu.</summary>
        public bool TryGetSlotPosition(string slotId, out float gx, out float gy)
        {
            gx = gy = 0f;
            var slot = Def.FindSlot(slotId);
            if (slot == null) return false;
            if (IsWallMounted)
            {
                // Duvar rafı: yuvalar duvar boyunca dizilir (U, geniş rafta bütün genişliğe yayılır), duvarın hemen önünde.
                int span = WallSpan;
                float along = slot.U * span;
                int segIndex = Math.Min(span - 1, (int)Math.Floor(along));
                var (ox, oy) = WallSides.SegmentOffset(Rotation, segIndex);
                var (ax, ay) = WallSides.AlongWall(Rotation, along - segIndex);
                gx = Position.X + ox + ax;
                gy = Position.Y + oy + ay;
                return true;
            }
            if (IsCeiling)
            {
                gx = Position.X + slot.U;
                gy = Position.Y + slot.V;
                return true;
            }
            var p = Footprint.RotatePoint(slot.U, slot.V, Def.Width, Def.Height, Rotation);
            gx = Position.X + p.x;
            gy = Position.Y + p.y;
            return true;
        }
    }

    /// <summary>
    /// Bir karonun dört kenarı: 0 = -y (Horizontal(x,y)), 1 = +x (Vertical(x+1,y)),
    /// 2 = +y (Horizontal(x,y+1)), 3 = -x (Vertical(x,y)). Duvar eşyası bu kenara asılır.
    /// </summary>
    public static class WallSides
    {
        public static Edge EdgeOf(GridPos tile, int side)
        {
            switch (Footprint.Normalize(side))
            {
                case 0: return new Edge(tile.X, tile.Y, EdgeAxis.Horizontal, tile.Z);
                case 1: return new Edge(tile.X + 1, tile.Y, EdgeAxis.Vertical, tile.Z);
                case 2: return new Edge(tile.X, tile.Y + 1, EdgeAxis.Horizontal, tile.Z);
                default: return new Edge(tile.X, tile.Y, EdgeAxis.Vertical, tile.Z);
            }
        }

        /// <summary>Geniş duvar eşyasında i. kenarın önündeki karonun ilk karoya göre kayması (kenar boyunca + yön).</summary>
        public static (int dx, int dy) SegmentOffset(int side, int i)
        {
            // Yatay kenarlar (0, 2) x boyunca, dikey kenarlar (1, 3) y boyunca uzanır.
            return Footprint.Normalize(side) % 2 == 0 ? (i, 0) : (0, i);
        }

        /// <summary>Geniş duvar eşyasının kenarları ve önlerindeki karolar.</summary>
        public static IEnumerable<(GridPos tile, Edge edge)> Segments(GridPos first, int side, int span)
        {
            for (int i = 0; i < Math.Max(1, span); i++)
            {
                var (dx, dy) = SegmentOffset(side, i);
                var t = new GridPos(first.X + dx, first.Y + dy, first.Z);
                yield return (t, EdgeOf(t, side));
            }
        }

        /// <summary>Kenar üzerindeki yerel nokta (u: kenar boyunca 0-1), karo köşesine göre, duvardan biraz içeride.</summary>
        public static (float x, float y) AlongWall(int side, float u)
        {
            const float inset = 0.12f;
            switch (Footprint.Normalize(side))
            {
                case 0: return (u, inset);
                case 1: return (1f - inset, u);
                case 2: return (u, 1f - inset);
                default: return (inset, u);
            }
        }
    }

    /// <summary>Bir kaptaki eşya (döküntü yığını, meyve sepeti): kimlik, durum, yaş (bozulma günü) ve hanenin kendi ürünü mü.</summary>
    public struct StoredItem
    {
        public string ItemId;
        public ItemCondition Condition;
        /// <summary>Yiyecek içerikte bozulma yaşı (oyun günü, sıcaklık çarpanlı).</summary>
        public double Age;
        /// <summary>Hanenin kendi ürünü ("Kendi emeğimiz" güzellik eki); pazardan alınmışsa false.</summary>
        public bool Own;
        public StoredItem(string id, ItemCondition c) { ItemId = id; Condition = c; Age = 0; Own = false; }
        public StoredItem(string id, double age, bool own) { ItemId = id; Condition = ItemCondition.Sound; Age = age; Own = own; }
    }

    /// <summary>Kaplama alanı ve dönüş hesapları.</summary>
    public static class Footprint
    {
        public static (int w, int h) Dimensions(int w, int h, int rotation)
        {
            int r = Normalize(rotation);
            return (r % 2 == 0) ? (w, h) : (h, w);
        }

        public static int Normalize(int rotation) => ((rotation % 4) + 4) % 4;

        /// <summary>
        /// Dönmemiş w x h alanı içindeki (u,v) (0-1) yerel noktayı, 90° adımla saat yönünde
        /// döndürülmüş alandaki sürekli koordinata çevirir.
        /// </summary>
        public static (float x, float y) RotatePoint(float u, float v, int w, int h, int rotation)
        {
            switch (Normalize(rotation))
            {
                case 0: return (u * w, v * h);
                case 1: return ((1f - v) * h, u * w);
                case 2: return ((1f - u) * w, (1f - v) * h);
                default: return (v * h, (1f - u) * w);
            }
        }
    }
}
