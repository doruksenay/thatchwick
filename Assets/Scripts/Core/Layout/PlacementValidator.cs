using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Layout
{
    /// <summary>Kuralın ağırlığı: hata yerleşimi geçersiz kılar; uyarı (oyuncu yapı modu) kırmızı vurgu verir ama engellemez.</summary>
    public enum Severity { Error = 0, Warning = 1 }

    /// <summary>Bir kural ihlali: kural kodu, okunur neden, konum ve eşya.</summary>
    public sealed class Violation
    {
        public string Code;
        public string Reason;
        public GridPos At;
        /// <summary>İhlale yol açan eşya (yoksa 0).</summary>
        public int ItemId;
        public string ItemDefId;
        public Severity Severity;

        public override string ToString() => $"{Code} {At} {ItemDefId ?? "-"}: {Reason}";
    }

    /// <summary>Kural kodları (rapor ve testlerde sabit).</summary>
    public static class Rules
    {
        public const string Overlap = "cakisma";          // iki eşya aynı karoda ya da eşya geçilmez karoda
        public const string Bounds = "sinir";             // ayak izi duvar/çit kenarını ya da oda/arsa sınırını aşıyor
        public const string DoorFront = "kapi-onu";       // kapının içi (1) ya da dışı (2 karo) dolu
        public const string Interaction = "etkilesim";    // etkileşim hücresi dolu ya da kapıdan ulaşılamıyor
        public const string Facing = "yon";               // oturma eşyası masaya/odağa bakmıyor, duvara bakıyor
        public const string WrongRoom = "yanlis-oda";     // eşya odanın ya da dükkânın türüne ait değil
        public const string Site = "ic-dis";              // dış mekân eşyası içeride ya da tersi
        public const string NoView = "gorsel-yon";        // bu yönün görseli (ya da geçerli aynası) yok
        public const string Relation = "iliski";          // komodin yatak başında değil, ocak eşyası ocaktan uzak, uzun eşya duvardan uzak
        // Dış mekân (Aşama D)
        public const string WellSite = "kuyu-yeri";
        public const string BenchFacing = "bank-yonu";
        public const string Barrel = "fici-odun";
        public const string TreeClear = "agac-mesafe";
        public const string TreeFront = "agac-cephe";
        public const string TradeYard = "meslek-avlu";
        public const string Sign = "tabela";
        public const string Path = "patika";
        public const string RoadNet = "yol-agi";

        public static string Name(string code)
        {
            switch (code)
            {
                case Overlap: return "çakışma";
                case Bounds: return "sınır";
                case DoorFront: return "kapı önü kapalı";
                case Interaction: return "etkileşim noktası kapalı";
                case Facing: return "yanlış yön";
                case WrongRoom: return "yanlış yer";
                case Site: return "iç/dış mekân";
                case NoView: return "yön görseli yok";
                case Relation: return "ilişki";
                case WellSite: return "kuyu yeri";
                case BenchFacing: return "bank yönü";
                case Barrel: return "fıçı/odun yeri";
                case TreeClear: return "ağaç mesafesi";
                case TreeFront: return "ağaç cepheyi örtüyor";
                case TradeYard: return "meslek eşyası dükkândan uzak";
                case Sign: return "tabela yeri";
                case Path: return "patika";
                case RoadNet: return "yol ağı";
                default: return code;
            }
        }
    }

    /// <summary>
    /// Yerleşim doğrulayıcısı (saf C#): bir dünyayı (ve isteğe bağlı yerleşim planını) alıp ihlal listesi döndürür.
    /// Aşama A: çakışma, sınır, kapı önü. Diğer kurallar (yön, etkileşim, oda türü, dış mekân) aşamalarla eklenir;
    /// hepsi aynı listeye yazar. Kapsam verilirse yalnızca o karolardaki eşya ve kapılar denetlenir.
    /// </summary>
    public sealed class PlacementValidator
    {
        /// <summary>Kapının iç yüzünde boş kalacak derinlik (karo) ve dış yüzünde.</summary>
        public const int DoorInside = 1, DoorOutside = 2;

        private readonly WorldState _world;
        private readonly GridMap _map;
        private readonly LayoutPlan _plan;
        private readonly Func<GridPos, bool> _scope;
        private readonly List<Violation> _out = new List<Violation>();
        private Dictionary<GridPos, ItemInstance> _occ;

        /// <summary>Etkileşim kuralının ulaşılabilirlik kümesi (verilmezse bütün yollardan ve kapılardan hesaplanır). Oda döşeyici
        /// odanın girişlerinden hesaplananı verir (hızlı, yerel).</summary>
        public Func<HashSet<GridPos>> ReachProvider { get; set; }

        /// <summary>Oyuncunun elle yaptığı yerleşimde uyarıya düşen kurallar (açık karar: uyarı ve kırmızı vurgu, engel değil).</summary>
        public bool PlayerMode { get; set; }

        public PlacementValidator(WorldState world, LayoutPlan plan = null, Func<GridPos, bool> scope = null)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _map = world.Map;
            _plan = plan;
            _scope = scope;
        }

        public static List<Violation> Run(WorldState world, LayoutPlan plan = null, Func<GridPos, bool> scope = null) =>
            new PlacementValidator(world, plan, scope).Validate();

        private bool InScope(GridPos p) => _scope == null || _scope(p);

        public List<Violation> Validate()
        {
            _out.Clear();
            BuildOccupancy();
            CheckOverlapAndBounds();
            CheckDoors();
            foreach (var r in ExtraRules) r(this);
            return new List<Violation>(_out);
        }

        /// <summary>Sonraki aşamaların kuralları (B, D) buraya eklenir; her biri Add ile ihlal yazar.</summary>
        internal readonly List<Action<PlacementValidator>> ExtraRules = new List<Action<PlacementValidator>>();

        internal WorldState World => _world;
        internal GridMap Map => _map;
        internal LayoutPlan Plan => _plan;
        internal bool Scoped(GridPos p) => InScope(p);

        internal void Add(string code, GridPos at, ItemInstance item, string reason, Severity severity = Severity.Error)
        {
            _out.Add(new Violation
            {
                Code = code, At = at, ItemId = item?.Id ?? 0, ItemDefId = item?.Def.Id, Reason = reason,
                Severity = PlayerMode && severity == Severity.Error && IsSoft(code) ? Severity.Warning : severity
            });
        }

        // Oyuncu yerleşiminde kapı önü, etkileşim ve yön ihlali uyarıdır; çakışma ve sınır kesin engeldir.
        private static bool IsSoft(string code) => code != Rules.Overlap && code != Rules.Bounds;

        // ---------- doluluk ----------

        /// <summary>Karoyu kaplayan yer eşyası (halı, duvar, tavan ve dekor kaplamaz).</summary>
        internal ItemInstance Occupant(GridPos p) => _occ != null && _occ.TryGetValue(p, out var it) ? it : null;

        /// <summary>Yürünebilir ve boş (eşya, su, kapalı karo yok).</summary>
        internal bool IsFree(GridPos p) => _map.HasTile(p) && !_map.IsWater(p) && Occupant(p) == null && !(_map.IsBlocked(p));

        private readonly List<(GridPos tile, ItemInstance a, ItemInstance b)> _overlaps = new List<(GridPos, ItemInstance, ItemInstance)>();

        private void BuildOccupancy()
        {
            _occ = new Dictionary<GridPos, ItemInstance>();
            _overlaps.Clear();
            foreach (var it in _world.Items)
            {
                if (!Occupies(it)) continue;
                foreach (var t in it.OccupiedTiles())
                {
                    if (_occ.TryGetValue(t, out var other)) { _overlaps.Add((t, other, it)); continue; }
                    _occ[t] = it;
                }
            }
        }

        /// <summary>Karo kaplayan yer eşyası mı (halı ve dekor değil).</summary>
        public static bool Occupies(ItemInstance it) => !it.IsDecor && !it.IsWallMounted && !it.IsCeiling && !it.IsRug;

        // ---------- çakışma ve sınır ----------

        private void CheckOverlapAndBounds()
        {
            foreach (var (t, a, b) in _overlaps)
                if (InScope(t)) Add(Rules.Overlap, t, b, $"{b.Def.DisplayName} ile {a.Def.DisplayName} aynı karoda.");

            foreach (var it in _world.Items)
            {
                if (!Occupies(it) || !InScope(it.Position)) continue;
                var tiles = new List<GridPos>(it.OccupiedTiles());
                foreach (var t in tiles)
                {
                    if (!_map.HasTile(t)) { Add(Rules.Bounds, t, it, $"{it.Def.DisplayName} döşemesi olmayan karoya taşıyor."); break; }
                    if (_map.IsWater(t)) { Add(Rules.Overlap, t, it, $"{it.Def.DisplayName} suyun üstünde."); break; }
                }
                // Çok karolu eşyanın kendi karoları arasında duvar, kapı, pencere ya da çit olmamalı (ayak izi sınırı aşıyor).
                bool crossed = false;
                foreach (var a in tiles)
                {
                    foreach (var b in tiles)
                    {
                        if (!Facing.Between(a, b, out _)) continue;
                        var k = _map.GetEdge(GridMap.EdgeBetween(a, b));
                        if (k == EdgeKind.None) continue;
                        Add(Rules.Bounds, a, it, $"{it.Def.DisplayName} {EdgeName(k)} üstünden taşıyor.");
                        crossed = true; break;
                    }
                    if (crossed) break;
                }
                // Planlı arsa ya da oda: eşya ait olduğu dikdörtgenin dışına taşmasın.
                if (_plan != null && !crossed)
                {
                    var site = _plan.SiteAt(it.Position);
                    if (site != null)
                        foreach (var t in tiles)
                            if (!site.Contains(t)) { Add(Rules.Bounds, t, it, $"{it.Def.DisplayName} {site.Name} sınırının dışına taşıyor."); break; }
                }
            }
        }

        internal static string EdgeName(EdgeKind k)
        {
            switch (k)
            {
                case EdgeKind.Wall: return "duvar";
                case EdgeKind.Door: return "kapı";
                case EdgeKind.Window: return "pencere";
                case EdgeKind.Fence: return "çit";
                case EdgeKind.Gate: return "bahçe kapısı";
                case EdgeKind.HalfWall: return "alçak duvar";
                case EdgeKind.Railing: return "korkuluk";
                default: return "kenar";
            }
        }

        // ---------- kapı önü ----------

        /// <summary>Haritadaki kapılar ve iki yüzü: iç (kapalı oda) ve dış. İki yüz de iç odaysa (oda arası kapı) ikisi de iç sayılır.</summary>
        public static IEnumerable<DoorInfo> Doors(GridMap map)
        {
            var rooms = map.Rooms;
            foreach (var kv in map.Edges)
            {
                if (kv.Value != EdgeKind.Door && kv.Value != EdgeKind.Gate) continue;
                var e = kv.Key;
                var a = e.SideA; var b = e.SideB;
                if (!map.HasTile(a) && !map.HasTile(b)) continue;
                var ra = rooms.RoomAt(a); var rb = rooms.RoomAt(b);
                bool aIn = ra != null && ra.IsIndoor, bIn = rb != null && rb.IsIndoor;
                Facing.Between(a, b, out var aToB);
                yield return new DoorInfo
                {
                    Edge = e, Kind = kv.Value,
                    // İç yüz: kapalı odadaki taraf. Bahçe kapısı ve oda arası kapıda iki taraf da "iç" (1 karo).
                    Inside = bIn && !aIn ? b : a,
                    Outside = bIn && !aIn ? a : b,
                    OutDir = bIn && !aIn ? Facing.Opposite(aToB) : aToB,
                    Exterior = aIn != bIn,
                };
            }
        }

        private void CheckDoors()
        {
            foreach (var d in Doors(_map))
            {
                int outDepth = d.Exterior ? DoorOutside : DoorInside;
                // Kapsamlı denetimde (bahçe, bitki örtüsü) kapının 2. dış karosu da sayılır: eskiden yalnızca ilk iç/dış karoya
                // bakılıyordu, ikinci karoya konan çalı ya da ağaç öbeği kapsam dışında kalıp silinmiyordu (tester raporu 66).
                bool scoped = InScope(d.Inside) || InScope(d.Outside);
                for (int i = 1; i < outDepth && !scoped; i++) scoped = InScope(Facing.Step(d.Outside, d.OutDir, i));
                if (!scoped) continue;
                // İç yüz: 1 karo
                CheckClear(d, d.Inside, Facing.Opposite(d.OutDir), DoorInside, "içi");
                CheckClear(d, d.Outside, d.OutDir, outDepth, "önü");
            }
        }

        private void CheckClear(DoorInfo d, GridPos first, Dir4 dir, int depth, string side)
        {
            var p = first;
            for (int i = 0; i < depth; i++)
            {
                if (i > 0)
                {
                    var prev = p;
                    p = Facing.Step(p, dir);
                    var k = _map.GetEdge(GridMap.EdgeBetween(prev, p));
                    if (EdgeKinds.BlocksMovement(k))
                    {
                        Add(Rules.DoorFront, p, null, $"Kapı {side} {i + 1}. karoda {EdgeName(k)} var ({d.Edge}).");
                        return;
                    }
                }
                if (!_map.HasTile(p)) return;                 // harita ya da kat kenarı: denetlenecek karo yok
                var occ = Occupant(p);
                if (occ != null) { Add(Rules.DoorFront, p, occ, $"Kapı {side} dolu: {occ.Def.DisplayName} ({d.Edge})."); return; }
                if (_map.IsWater(p)) { Add(Rules.DoorFront, p, null, $"Kapı {side} suya açılıyor ({d.Edge})."); return; }
                if (_map.IsBlocked(p)) { Add(Rules.DoorFront, p, null, $"Kapı {side} geçilmez karo ({d.Edge})."); return; }
            }
        }
    }

    /// <summary>Bir kapı ya da bahçe kapısı: iç ve dış ilk karoları, dışarı yönü.</summary>
    public struct DoorInfo
    {
        public Edge Edge;
        public EdgeKind Kind;
        public GridPos Inside, Outside;
        public Dir4 OutDir;
        /// <summary>Bina dış kapısı (bir yüz kapalı oda, öbürü dış mekân).</summary>
        public bool Exterior;
    }
}
