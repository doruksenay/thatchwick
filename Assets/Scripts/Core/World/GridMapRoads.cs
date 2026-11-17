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

        public bool IsWater(GridPos p) => p.Z == 0 && GetFloor(p) == FloorType.Water;

        /// <summary>Sular donmuş mu (kışın dondurucu soğukta; oyun katmanı günlük verir).</summary>
        public bool WaterFrozen
        {
            get => _waterFrozen;
            set { if (_waterFrozen == value) return; _waterFrozen = value; RoadsChanged?.Invoke(); }
        }

        /// <summary>Açık (donmamış) su karosu mu: içine girilir, kova doldurulur, balık tutulur, yıkanılır.</summary>
        public bool IsOpenWater(GridPos p) => !_waterFrozen && IsWater(p);

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
}
