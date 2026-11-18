using System;
using System.Collections.Generic;

namespace Thatchwick.Core.World
{
    /// <summary>Yol türü (zemin katın dış karolarında, zeminin üstünde ayrı katman).</summary>
    public enum RoadType : byte
    {
        None = 0,
        /// <summary>Toprak patika: oyuncu döşer ya da sık geçilen çimde kendiliğinden oluşur.</summary>
        Path = 1,
        /// <summary>Çakıl yol.</summary>
        Gravel = 2,
        /// <summary>Kaldırım (taş döşeli insan yolu).</summary>
        Paved = 3
    }

    /// <summary>
    /// Yollar ve patikalar (17.11.2026). Yol insanı da hayvanı da hızlandırır: çim 1, patika x1,6, çakıl x1,8, kaldırım x2.
    /// Üstünden geçtikçe aşınır (ağır ayak daha çok); aşınmış yol daha az hızlandırır, onarılır. Sık geçilen çim
    /// kendiliğinden patikaya döner; patika kullanılmazsa yeniden çimlenir.
    /// </summary>
    public sealed partial class GridMap
    {
        /// <summary>Yol döşendi, kalktı ya da durum bandı değişti (yalnızca zemin görünümü yenilenir; duvar/oda değil).</summary>
        public event Action RoadsChanged;

        public const float PathSpeed = 1.6f, GravelSpeed = 1.8f, PavedSpeed = 2.0f;
        /// <summary>Çimde bu kadar ayak izi birikince kendiliğinden patika olur.</summary>
        public const float NaturalPathThreshold = 40f;
        /// <summary>Durum bu değerin altında "yıpranmış", BrokenBelow altında "bozuk".</summary>
        public const float WornBelow = 0.6f, BrokenBelow = 0.3f;

        private RoadType[] _road;
        private float[] _roadCond;      // 0-1: yolun durumu (doğal patikada belirginliği)
        private bool[] _roadNatural;    // kendiliğinden oluşmuş patika (kullanılmazsa çimlenir)
        private float[] _trample;       // çimdeki ayak izi birikimi
        private int _roadCount;

        private void EnsureRoads()
        {
            if (_road != null) return;
            int n = Width * Height;     // yalnızca zemin kat
            _road = new RoadType[n];
            _roadCond = new float[n];
            _roadNatural = new bool[n];
            _trample = new float[n];
        }

        private int PlaneIndex(GridPos p) => p.Y * Width + p.X;
        private bool RoadPlane(GridPos p) => p.Z == 0 && PlaneInBounds(p);

        /// <summary>Haritada en az bir yol var mı (yol bulucu hızlı yolu kullanır).</summary>
        public bool HasRoads => _roadCount > 0;

        // ---- su karoları (dere, gölet) ----
        private int _waterCount;
        private bool _waterFrozen;

        /// <summary>Haritada su karosu var mı.</summary>
        public bool HasWater => _waterCount > 0;
        /// <summary>Yürüme hızını değiştiren karo var mı (yol ya da su): yol bulucu ağırlıklı aramayı bununla açar.</summary>
        public bool HasTerrainSpeed => _roadCount > 0 || _waterCount > 0;

        /// <summary>Su karosu: içine girilen yavaşlar (x0,5) ve ıslanır; donmuşsa üstünde normal yürünür.</summary>
        public const float WaterSpeed = 0.5f;

        /// <summary>Su (gölet çukuru) karosu mu; dolu olmayabilir (kazılan gölet yağmurla dolar): bkz. IsFilled.</summary>
        public bool IsWater(GridPos p) => p.Z == 0 && GetFloor(p) == FloorType.Water;

        /// <summary>Gölet karosunda kullanılabilir su var mı (kaynak gölet ya da yağmurla dolmuş).</summary>
        public bool IsFilled(GridPos p) => IsWater(p) && (PondAt(p)?.IsFilled ?? false);

        /// <summary>Sular donmuş mu (kışın dondurucu soğukta; oyun katmanı günlük verir).</summary>
        public bool WaterFrozen
        {
            get => _waterFrozen;
            set { if (_waterFrozen == value) return; _waterFrozen = value; RoadsChanged?.Invoke(); }
        }

        /// <summary>Açık (donmamış) su karosu mu: içine girilir, kova doldurulur, balık tutulur, yıkanılır.</summary>
        public bool IsOpenWater(GridPos p) => !_waterFrozen && IsFilled(p);

        public RoadType GetRoad(GridPos p) => _road != null && RoadPlane(p) ? _road[PlaneIndex(p)] : RoadType.None;
        public float GetRoadCondition(GridPos p) => _road != null && RoadPlane(p) ? _roadCond[PlaneIndex(p)] : 0f;
        public bool IsNaturalRoad(GridPos p) => _road != null && RoadPlane(p) && _roadNatural[PlaneIndex(p)];
        public float GetTrample(GridPos p) => _road != null && RoadPlane(p) ? _trample[PlaneIndex(p)] : 0f;

        /// <summary>Yolun hız çarpanı (yolsuz 1).</summary>
        public static float RoadBaseSpeed(RoadType t)
        {
            switch (t)
            {
                case RoadType.Path: return PathSpeed;
                case RoadType.Gravel: return GravelSpeed;
                case RoadType.Paved: return PavedSpeed;
                default: return 1f;
            }
        }

        /// <summary>Aşınmaya dayanıklılık: bu kadar "ağır ayak" yolu tamamen yıpratır.</summary>
        public static float RoadDurability(RoadType t)
        {
            switch (t)
            {
                case RoadType.Path: return 150f;
                case RoadType.Gravel: return 600f;
                case RoadType.Paved: return 2000f;
                default: return 1f;
            }
        }

        /// <summary>Durum bandı: 0 sağlam, 1 yıpranmış, 2 bozuk.</summary>
        public static int ConditionBand(float cond) => cond >= WornBelow ? 0 : cond >= BrokenBelow ? 1 : 2;

        /// <summary>Karonun yürüme hız çarpanı: yolun türü ve durumu (sağlam tam, yıpranmış %65, bozuk %35 katkı).</summary>
        public float WalkSpeedAt(GridPos p)
        {
            if (!HasTerrainSpeed) return 1f;
            if (IsOpenWater(p)) return WaterSpeed;
            if (_roadCount == 0) return 1f;
            var t = GetRoad(p);
            if (t == RoadType.None) return 1f;
            float cond = GetRoadCondition(p);
            float q = IsNaturalRoad(p) ? 1f : ConditionBand(cond) == 0 ? 1f : ConditionBand(cond) == 1 ? 0.65f : 0.35f;
            return 1f + (RoadBaseSpeed(t) - 1f) * q;
        }

        /// <summary>İki karo arası adımın hız çarpanı (iki ucun ortalaması).</summary>
        public float StepSpeed(GridPos from, GridPos to) => !HasTerrainSpeed ? 1f : (WalkSpeedAt(from) + WalkSpeedAt(to)) * 0.5f;

        /// <summary>Yol döşer ya da kaldırır (yapı modu, kayıt). Durum 1 (yeni).</summary>
        public void SetRoad(GridPos p, RoadType type, float condition = 1f, bool natural = false)
        {
            if (!RoadPlane(p)) throw new ArgumentOutOfRangeException(nameof(p), $"{p}: yol yalnızca zemin katta.");
            EnsureRoads();
            int i = PlaneIndex(p);
            var old = _road[i];
            if (old != RoadType.None) _roadCount--;
            if (type != RoadType.None) _roadCount++;
            _road[i] = type;
            _roadCond[i] = type == RoadType.None ? 0f : Math.Max(0f, Math.Min(1f, condition));
            _roadNatural[i] = type == RoadType.Path && natural;
            _trample[i] = 0f;
            RoadsChanged?.Invoke();
        }

        /// <summary>Yolu onarır (durum 1).</summary>
        public void RepairRoad(GridPos p)
        {
            if (GetRoad(p) == RoadType.None) return;
            int i = PlaneIndex(p);
            int band = ConditionBand(_roadCond[i]);
            _roadCond[i] = 1f;
            if (band != 0) RoadsChanged?.Invoke();
        }

        /// <summary>Çiğnenebilir mi: zemin katta, açık havada (kapalı odada değil), çim ya da yol.</summary>
        private bool Trampleable(GridPos p)
        {
            if (!RoadPlane(p)) return false;
            var room = Rooms.RoomAt(p);
            return room == null || !room.IsIndoor;
        }

        /// <summary>
        /// Biri ya da bir hayvan bu karoya bastı. weight: insan 0,2, koyun/keçi 1, inek/at 3. Yolda durumu düşürür (doğal
        /// patikada belirginleştirir); çimde iz biriktirir, eşik aşılınca kendiliğinden patika olur.
        /// </summary>
        public void Trample(GridPos p, float weight)
        {
            if (weight <= 0f || !Trampleable(p)) return;
            weight *= WearMultiplier;
            EnsureRoads();
            int i = PlaneIndex(p);
            var t = _road[i];
            if (t == RoadType.None)
            {
                if (_floor[Index(p)] != FloorType.Grass) return;      // ekili toprak, taş avlu patikaya dönmez
                _trample[i] += weight;
                if (_trample[i] >= NaturalPathThreshold) SetRoad(p, RoadType.Path, 0.5f, natural: true);
                return;
            }
            int before = ConditionBand(_roadCond[i]);
            if (_roadNatural[i]) _roadCond[i] = Math.Min(1f, _roadCond[i] + weight / NaturalPathThreshold);
            else _roadCond[i] = Math.Max(0f, _roadCond[i] - weight / RoadDurability(t));
            if (ConditionBand(_roadCond[i]) != before) RoadsChanged?.Invoke();
        }

        /// <summary>Aşınma çarpanı (yağmurlu gün 1,5); oyun katmanı günlük verir.</summary>
        public float WearMultiplier { get; set; } = 1f;

        /// <summary>Kendiliğinden patika günde bu kadar silikleşir (kullanılmazsa ~10 günde çimlenir).</summary>
        public const float NaturalPathFade = 0.05f;

        /// <summary>Yeni gün: çimdeki izler azalır, kullanılmayan doğal patika silikleşir ve sonunda çimlenir.</summary>
        public void OnNewDayRoads()
        {
            if (_road == null) return;
            bool changed = false;
            for (int i = 0; i < _road.Length; i++)
            {
                if (_road[i] == RoadType.None) { if (_trample[i] > 0f) _trample[i] *= 0.7f; continue; }
                if (!_roadNatural[i]) continue;
                _roadCond[i] -= NaturalPathFade;
                if (_roadCond[i] <= 0f)
                {
                    _road[i] = RoadType.None; _roadCond[i] = 0f; _roadNatural[i] = false; _roadCount--;
                    changed = true;
                }
            }
            if (changed) RoadsChanged?.Invoke();
        }

        /// <summary>Kayıt: yollu ya da izli karolar.</summary>
        public IEnumerable<(GridPos pos, RoadType type, float cond, bool natural, float trample)> RoadTiles()
        {
            if (_road == null) yield break;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    int i = y * Width + x;
                    if (_road[i] != RoadType.None || _trample[i] >= 1f)
                        yield return (new GridPos(x, y), _road[i], _roadCond[i], _roadNatural[i], _trample[i]);
                }
        }

        /// <summary>Kayıttan iz birikimini geri koyar.</summary>
        public void SetTrample(GridPos p, float trample)
        {
            if (!RoadPlane(p)) return;
            EnsureRoads();
            _trample[PlaneIndex(p)] = Math.Max(0f, Math.Min(NaturalPathThreshold, trample));
        }
    }

    /// <summary>
    /// Bir gölet: birbirine dik komşu su karoları. Kazılan gölet boş başlar ve yalnızca yağmurla dolar; yazın yağmur
    /// yağmazsa buharlaşır. Kaynaklı (haritadaki doğal) gölet hep doludur. Dolu kaldıkça böcekler gelir; balıklar yalnızca
    /// böcek varken ve aynı türden en az iki balık varsa ürer. Kuruyan gölette balıklar ölür, böcekler gider.
    /// </summary>
    public sealed class Pond
    {
        /// <summary>Bu seviyeden sonra kullanılabilir: içine girilir, yıkanılır, kova doldurulur, balık yaşar.</summary>
        public const float FilledLevel = 0.4f;
        /// <summary>Dolu kaldıktan bu kadar gün sonra böcekler gelir.</summary>
        public const double InsectDays = 2.0;
        /// <summary>Karo başına en çok balık.</summary>
        public const int FishPerTile = 3;

        internal readonly List<GridPos> TilesList = new List<GridPos>();
        public IReadOnlyList<GridPos> Tiles => TilesList;
        public GridPos Anchor => TilesList[0];
        /// <summary>Su seviyesi 0 (kuru çukur) - 1 (ağzına kadar).</summary>
        public float Level { get; internal set; }
        /// <summary>Kaynaklı gölet: hep dolu, kurumaz.</summary>
        public bool Spring { get; internal set; }
        public bool Insects { get; internal set; }
        /// <summary>Kesintisiz dolu kalınan gün.</summary>
        public double FilledDays { get; internal set; }
        /// <summary>Tür (eşya kimliği, ör. item.fish_carp) başına balık sayısı.</summary>
        public Dictionary<string, int> Fish { get; } = new Dictionary<string, int>();

        public bool IsFilled => Spring || Level >= FilledLevel;
        public int FishCapacity => TilesList.Count * FishPerTile;
        public int FishTotal { get { int n = 0; foreach (var v in Fish.Values) n += v; return n; } }

        public void AddFish(string species, int count = 1)
        {
            if (count <= 0) return;
            Fish.TryGetValue(species, out int n);
            Fish[species] = n + count;
        }

        public bool TakeFish(string species)
        {
            if (!Fish.TryGetValue(species, out int n) || n <= 0) return false;
            if (n == 1) Fish.Remove(species); else Fish[species] = n - 1;
            return true;
        }
    }

    public sealed partial class GridMap
    {
        private bool _pondsDirty = true;
        private List<Pond> _ponds = new List<Pond>();
        private int[] _pondIndex;           // düzlem karosu -> _ponds sırası (-1 yok)

        /// <summary>Gölet başına yağmur: yağmurlu gün, fırtınalı gün.</summary>
        public const float RainFill = 0.3f, StormFill = 0.55f;
        /// <summary>Sıcakta yağmursuz gün buharlaşma (15°C üstü her 10°C için bir kat daha).</summary>
        public const float EvaporationPerDay = 0.04f;

        /// <summary>Haritadaki göletler (karo kümeleri).</summary>
        public IReadOnlyList<Pond> Ponds { get { RebuildPonds(); return _ponds; } }

        public Pond PondAt(GridPos p)
        {
            if (p.Z != 0 || !PlaneInBounds(p) || GetFloor(p) != FloorType.Water) return null;
            RebuildPonds();
            int k = _pondIndex[PlaneIndex(p)];
            return k >= 0 ? _ponds[k] : null;
        }

        /// <summary>Göleti kaynaklı yapar (harita üretimi: doğal gölet, dere): hep dolu.</summary>
        public void SetSpring(GridPos p, bool spring = true)
        {
            var pond = PondAt(p);
            if (pond == null) return;
            pond.Spring = spring;
            if (spring) pond.Level = 1f;
            RoadsChanged?.Invoke();
        }

        /// <summary>Kayıttan: bir karonun göletinin durumu.</summary>
        public void RestorePond(GridPos anchor, float level, bool spring, bool insects, double filledDays, IEnumerable<KeyValuePair<string, int>> fish)
        {
            var pond = PondAt(anchor);
            if (pond == null) return;
            pond.Level = Math.Max(0f, Math.Min(1f, level));
            pond.Spring = spring;
            pond.Insects = insects;
            pond.FilledDays = Math.Max(0, filledDays);
            pond.Fish.Clear();
            if (fish != null) foreach (var kv in fish) if (kv.Value > 0) pond.AddFish(kv.Key, kv.Value);
        }

        // Su karoları değişince (kazıldı, dolduruldu) gölet kümeleri yeniden çıkarılır. Eski göletin suyu (hacim),
        // balıkları ve böcekleri, yeni kümeyle örtüşen karo oranında aktarılır; yeni kazılan karo kuru gelir.
        private void RebuildPonds()
        {
            if (!_pondsDirty && _pondIndex != null) return;
            _pondsDirty = false;
            int n = Width * Height;
            var oldIndex = _pondIndex;
            var oldPonds = _ponds;
            var index = new int[n];
            for (int i = 0; i < n; i++) index[i] = -1;
            var ponds = new List<Pond>();
            var queue = new Queue<GridPos>();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var start = new GridPos(x, y);
                    if (index[PlaneIndex(start)] >= 0 || GetFloor(start) != FloorType.Water) continue;
                    var pond = new Pond();
                    int k = ponds.Count;
                    ponds.Add(pond);
                    index[PlaneIndex(start)] = k;
                    queue.Enqueue(start);
                    while (queue.Count > 0)
                    {
                        var cur = queue.Dequeue();
                        pond.TilesList.Add(cur);
                        foreach (var o in new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) })
                        {
                            var nb = cur + o;
                            if (!PlaneInBounds(nb) || GetFloor(nb) != FloorType.Water || index[PlaneIndex(nb)] >= 0) continue;
                            index[PlaneIndex(nb)] = k;
                            queue.Enqueue(nb);
                        }
                    }
                }

            if (oldIndex != null && oldPonds.Count > 0)
                foreach (var pond in ponds)
                {
                    var overlap = new Dictionary<int, int>();
                    foreach (var t in pond.TilesList)
                    {
                        int o = oldIndex[PlaneIndex(t)];
                        if (o < 0) continue;
                        overlap.TryGetValue(o, out int c);
                        overlap[o] = c + 1;
                    }
                    double volume = 0;
                    foreach (var kv in overlap)
                    {
                        var old = oldPonds[kv.Key];
                        double share = (double)kv.Value / old.TilesList.Count;
                        volume += old.Level * old.TilesList.Count * share;
                        pond.Spring |= old.Spring;
                        pond.Insects |= old.Insects;
                        pond.FilledDays = Math.Max(pond.FilledDays, old.FilledDays);
                        foreach (var f in old.Fish) pond.AddFish(f.Key, (int)Math.Round(f.Value * share));
                    }
                    pond.Level = pond.Spring ? 1f : (float)Math.Min(1.0, volume / pond.TilesList.Count);
                    if (!pond.IsFilled) { pond.Insects = false; pond.FilledDays = 0; pond.Fish.Clear(); }
                }
            _ponds = ponds;
            _pondIndex = index;
        }

        /// <summary>
        /// Göletlerin günü: yağmur doldurur (rain: 0 kuru, RainFill, StormFill), sıcakta buharlaşır. Dolu kalan gölete
        /// böcek gelir; böcek varken ve kış değilken aynı türden her çift %30 olasılıkla bir yavru verir (karo başına
        /// 3 balık sınırı). Kuruyan gölette balıklar ölür. Olayları (bildirim metni) döner.
        /// </summary>
        public List<string> OnNewDayPonds(float rain, float outdoorTemp, bool winter, Random rng)
        {
            var notes = new List<string>();
            foreach (var pond in Ponds)
            {
                bool wasFilled = pond.IsFilled;
                if (!pond.Spring)
                {
                    if (rain > 0f) pond.Level = Math.Min(1f, pond.Level + rain);
                    else if (outdoorTemp > 15f && !_waterFrozen) pond.Level = Math.Max(0f, pond.Level - EvaporationPerDay * (1f + (outdoorTemp - 15f) / 10f));
                }
                if (pond.IsFilled && !wasFilled) notes.Add("Gölet yağmurla doldu.");
                if (!pond.IsFilled)
                {
                    int dead = pond.FishTotal;
                    if (wasFilled) notes.Add(dead > 0 ? $"Gölet kurudu: {dead} balık öldü." : "Gölet kurudu.");
                    pond.Fish.Clear();
                    pond.Insects = false;
                    pond.FilledDays = 0;
                    continue;
                }
                pond.FilledDays += 1;
                if (!pond.Insects && pond.FilledDays >= Pond.InsectDays && !_waterFrozen)
                {
                    pond.Insects = true;
                    notes.Add("Gölete böcekler geldi: balıklar artık beslenip üreyebilir.");
                    continue;                                   // üreme ertesi günden
                }
                if (!pond.Insects || winter || _waterFrozen) continue;
                int born = 0;
                foreach (var species in new List<string>(pond.Fish.Keys))
                {
                    int pairs = pond.Fish[species] / 2;
                    for (int i = 0; i < pairs && pond.FishTotal < pond.FishCapacity; i++)
                        if (rng.NextDouble() < 0.3) { pond.AddFish(species); born++; }
                }
                if (born > 0) notes.Add($"Gölette {born} yavru balık oldu.");
            }
            if (notes.Count > 0) RoadsChanged?.Invoke();
            return notes;
        }
    }
}
