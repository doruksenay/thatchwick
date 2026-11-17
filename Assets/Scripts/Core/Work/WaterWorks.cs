using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Work
{
    /// <summary>Bir tesisatın (leğen, küvet, hela, çeşme) bugünkü su durumu.</summary>
    public sealed class WaterFixtureState
    {
        public bool Supplied;
        /// <summary>Su gelmiyorsa nedeni (arayüz); geliyorsa null.</summary>
        public string Why;
        public int Use;
    }

    /// <summary>
    /// Su şebekesinin bir anlık hesabı: hangi ağ parçası ve su yolu ıslak, hangi tesisata su geliyor, kaynaklardan
    /// ne kadar çekildi, sarnıçlara ne kadar fazla su gidebilir.
    /// </summary>
    public sealed class WaterReport
    {
        /// <summary>Kaynağa bağlı ağ eşyaları (kanal, kapak, çark, kaynaklar, sarnıçlar).</summary>
        public readonly HashSet<int> ConnectedItems = new HashSet<int>();
        /// <summary>Su taşıyan su yolu karoları (zemin kat).</summary>
        public readonly HashSet<GridPos> WetConduits = new HashSet<GridPos>();
        /// <summary>Tesisat durumu (yalnızca su dağıtımı hesaplandıysa dolu).</summary>
        public readonly Dictionary<int, WaterFixtureState> Fixtures = new Dictionary<int, WaterFixtureState>();
        /// <summary>Kaynak eşyadan (sarnıç dahil) bugün çekilen su.</summary>
        public readonly Dictionary<int, int> Drawn = new Dictionary<int, int>();
        /// <summary>Sarnıca ağdaki artan sudan dolabilecek miktar.</summary>
        public readonly Dictionary<int, int> Refill = new Dictionary<int, int>();
        /// <summary>Kazılan (kaynaksız) göletten bugün çekilen su: gölet bu kadar alçalır.</summary>
        public readonly Dictionary<Pond, int> PondDrawn = new Dictionary<Pond, int>();
        public int TotalSupply, TotalDemand, SuppliedDemand;

        public bool IsWet(GridPos p) => WetConduits.Contains(p.OnLevel(0));
        public bool IsSupplied(ItemInstance it) => it != null && Fixtures.TryGetValue(it.Id, out var f) && f.Supplied;
        public string WhyDry(ItemInstance it) => it != null && Fixtures.TryGetValue(it.Id, out var f) ? f.Why : null;
    }

    /// <summary>
    /// Su şebekesi grafiği (17.11.2026). Düğümler: ağ eşyaları (kaynak, kanal, kapak, çark, sarnıç, su terazisi), su yolu
    /// karoları ve göl/dere su kütleleri (kendiliğinden akan kaynak). Kenarlar: dik komşuluk (eşyanın altındaki su yolu
    /// da değer). Tesisat yaprak düğümdür: suyu geçirmez, yalnızca ister.
    /// Su dağıtımı: her tesisat sırayla (önce zemin kat), boru kapasitesi ve kaynak suyu yetenlerden geçen en kısa yolla
    /// beslenir; yol üstündeki her su yolunun günlük kapasitesinden düşülür. Böylece ince boru darboğaz olur, büyük ağ
    /// birden çok kaynak ister.
    /// </summary>
    public sealed class WaterGraph
    {
        private enum Kind { Item, Conduit, Body }

        private sealed class Node
        {
            public Kind Kind;
            public ItemInstance Item;
            public GridPos Pos;
            public ConduitType Conduit;
            public Pond Pond;                // su kütlesi düğümünde gölet
            public bool Passable = true;     // donmuş açık ark ve kapalı kapak geçirmez
            public bool Leaf;                // su çarkı: su alır, iletmez
            public int Capacity = int.MaxValue;
            public int Supply;               // kaynak suyu (kalan)
            public int Initial;              // kaynak suyu (gün başı)
            public bool Seed;                // bağlantı tohumu (eski kural: kuyu, gölet, su karosuna değen)
            public int Comp = -1;
            public readonly HashSet<int> Adj = new HashSet<int>();
        }

        private readonly WorldState _world;
        private readonly List<Node> _nodes = new List<Node>();
        private readonly Dictionary<GridPos, int> _itemAt = new Dictionary<GridPos, int>();
        private readonly Dictionary<GridPos, int> _conduitAt = new Dictionary<GridPos, int>();
        private readonly Dictionary<Pond, int> _pondNode = new Dictionary<Pond, int>();     // gölet/dere → kaynak düğümü
        private readonly List<ItemInstance> _fixtures = new List<ItemInstance>();

        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        /// <summary>Göl ya da derenin günlük suyu: küçük gölet az, büyük su çok verir.</summary>
        public static int BodyFlow(int tiles) => Math.Min(16, 4 + tiles / 2);

        public WaterGraph(WorldState world)
        {
            _world = world;
            var map = world.Map;

            foreach (var (pos, type) in map.ConduitTiles())
            {
                var n = new Node { Kind = Kind.Conduit, Pos = pos, Conduit = type, Capacity = Conduits.Capacity(type) };
                n.Passable = !(map.WaterFrozen && Conduits.Freezes(type));
                _conduitAt[pos] = Add(n);
            }

            foreach (var it in world.Items)
            {
                if (it.IsDecor) continue;
                var d = it.Def;
                if (IsConductive(d))
                {
                    var n = new Node { Kind = Kind.Item, Item = it, Pos = it.Position, Leaf = d.PowerSource };
                    if (d.WaterGate && it.Toggled) n.Passable = false;
                    n.Supply = d.IsCistern ? Math.Min(Math.Max(0, it.Fill), d.CisternFlow > 0 ? d.CisternFlow : ItemDef.DefaultWaterFlow) : d.EffectiveWaterFlow;
                    n.Seed = d.WaterSource || d.IsCistern && it.Fill > 0;
                    int id = Add(n);
                    foreach (var t in it.OccupiedTiles()) _itemAt[t] = id;
                }
                else if (d.IsFixture) _fixtures.Add(it);
            }
            // Kapalı kapak altındaki su yolunu da keser (kapak arkın üstüne konur).
            foreach (var n in new List<Node>(_nodes))
            {
                if (n.Kind != Kind.Item || n.Passable || !n.Item.Def.WaterGate) continue;
                foreach (var t in n.Item.OccupiedTiles())
                    if (_conduitAt.TryGetValue(t, out int c)) _nodes[c].Passable = false;
            }

            // Kenarlar.
            for (int i = 0; i < _nodes.Count; i++)
            {
                var n = _nodes[i];
                if (n.Kind == Kind.Conduit)
                {
                    foreach (var o in Dirs)
                        if (_conduitAt.TryGetValue(n.Pos + o, out int j)) Link(i, j);
                    TouchWater(i, n.Pos);
                }
                else if (n.Kind == Kind.Item)
                {
                    foreach (var t in n.Item.OccupiedTiles())
                    {
                        foreach (var o in Dirs)
                            if (_itemAt.TryGetValue(t + o, out int j) && j != i) Link(i, j);
                        if (t.Z != 0) continue;
                        // Eşyanın altındaki ya da yanındaki su yolu. Su çarkı yalnızca açık suyla döner.
                        for (int k = -1; k < 4; k++)
                        {
                            var c = k < 0 ? t : t + Dirs[k];
                            if (_conduitAt.TryGetValue(c, out int j) && (!n.Leaf || Conduits.IsOpen(_nodes[j].Conduit))) Link(i, j);
                        }
                    }
                    // Eski kural: göl/dere karosuna değen ağ parçası (çark ve kapalı kapak hariç) beslenir.
                    if (!n.Leaf && n.Passable && WaterNetwork.TouchesWater(map, n.Item))
                        foreach (var t in n.Item.OccupiedTiles()) TouchWater(i, t, includeSelf: true);
                }
            }
        }

        public static bool IsConductive(ItemDef d) => WaterNetwork.IsNode(d);

        private int Add(Node n) { n.Initial = n.Supply; _nodes.Add(n); return _nodes.Count - 1; }

        private void Link(int a, int b) { _nodes[a].Adj.Add(b); _nodes[b].Adj.Add(a); }

        private void TouchWater(int node, GridPos p, bool includeSelf = false)
        {
            var map = _world.Map;
            if (!map.HasWater || p.Z != 0) return;
            for (int k = includeSelf ? -1 : 0; k < 4; k++)
            {
                var w = k < 0 ? p : p + Dirs[k];
                if (!map.InBounds(w) || !map.IsFilled(w)) continue;      // kuru çukurdan su gelmez
                Link(node, BodyNode(map.PondAt(w)));
            }
        }

        // Gölet ya da dere (dik komşu su karoları, GridMap.Pond) başına tek kaynak düğümü. Kaynaklı su (doğal gölet,
        // dere) kendi akışıyla verir; kazılan gölet yağmur suyunu verir ve çekilen su onu alçaltır (WaterWorks.OnNewDay).
        private int BodyNode(Pond pond)
        {
            if (!_pondNode.TryGetValue(pond, out int id))
            {
                int flow = BodyFlow(pond.Tiles.Count);
                if (!pond.Spring) flow = Math.Min(flow, (int)(pond.Level * pond.Tiles.Count * WaterWorks.PondUnitsPerTile));
                id = Add(new Node { Kind = Kind.Body, Pos = pond.Anchor, Pond = pond, Supply = flow, Seed = true });
                _pondNode[pond] = id;
            }
            return id;
        }

        private bool Enterable(Node n) => n.Passable;

        /// <summary>Bağlantı: tohumlardan (kaynak, su kütlesi, dolu sarnıç) ulaşılan düğümler; çark alır ama iletmez.</summary>
        public void Connect(WaterReport report)
        {
            var seen = new bool[_nodes.Count];
            var q = new Queue<int>();
            for (int i = 0; i < _nodes.Count; i++)
                if (_nodes[i].Seed && Enterable(_nodes[i])) { seen[i] = true; q.Enqueue(i); }
            while (q.Count > 0)
            {
                int i = q.Dequeue();
                var n = _nodes[i];
                if (n.Kind == Kind.Item) report.ConnectedItems.Add(n.Item.Id);
                else if (n.Kind == Kind.Conduit) report.WetConduits.Add(n.Pos);
                if (n.Leaf) continue;
                foreach (int j in n.Adj)
                {
                    if (seen[j] || !Enterable(_nodes[j])) continue;
                    seen[j] = true; q.Enqueue(j);
                }
            }

            // Bileşenler (kapasiteye bakmadan): su terazisi ve sarnıç dolumu için.
            int comp = 0;
            for (int s = 0; s < _nodes.Count; s++)
            {
                if (_nodes[s].Comp >= 0 || !Enterable(_nodes[s])) continue;
                _nodes[s].Comp = comp;
                q.Enqueue(s);
                while (q.Count > 0)
                {
                    int i = q.Dequeue();
                    if (_nodes[i].Leaf) continue;
                    foreach (int j in _nodes[i].Adj)
                    {
                        if (_nodes[j].Comp >= 0 || !Enterable(_nodes[j])) continue;
                        _nodes[j].Comp = comp; q.Enqueue(j);
                    }
                }
                comp++;
            }
        }

        /// <summary>Tesisata su dağıtır; sarnıçlara kalan suyu yazar. Önce Connect çağrılmalı.</summary>
        public void Allocate(WaterReport report)
        {
            // Basınçlı bileşenler: bağlı bir su terazisi barındıranlar.
            var headComps = new HashSet<int>();
            foreach (var n in _nodes)
                if (n.Kind == Kind.Item && n.Item.Def.WaterHead && n.Comp >= 0 && report.ConnectedItems.Contains(n.Item.Id)) headComps.Add(n.Comp);
            foreach (var n in _nodes) if (n.Supply > 0 && n.Comp >= 0) report.TotalSupply += n.Supply;

            _fixtures.Sort((a, b) => a.Position.Z != b.Position.Z ? a.Position.Z.CompareTo(b.Position.Z) : a.Id.CompareTo(b.Id));
            foreach (var fx in _fixtures)
            {
                int use = fx.Def.WaterUse;
                report.TotalDemand += use;
                var state = new WaterFixtureState { Use = use };
                report.Fixtures[fx.Id] = state;

                var starts = new List<int>();
                bool frozen = false, needsHead = false;
                foreach (var t in fx.OccupiedTiles())
                {
                    // Aynı kattaki ağ eşyası (sarnıç, kanal, su terazisi...).
                    foreach (var o in Dirs)
                        if (_itemAt.TryGetValue(t + o, out int j) && !_nodes[j].Leaf) starts.Add(j);
                    if (t.Z == 0)
                    {
                        for (int k = -1; k < 4; k++)
                            if (_conduitAt.TryGetValue(k < 0 ? t : t + Dirs[k], out int j)) starts.Add(j);
                    }
                    else if (_conduitAt.TryGetValue(t.OnLevel(0), out int r) && Conduits.IsRiser(_nodes[r].Conduit))
                    {
                        // Üst kat: altındaki kurşun boru kolonundan, yalnızca basınçlı ağda.
                        if (_nodes[r].Comp >= 0 && headComps.Contains(_nodes[r].Comp)) starts.Add(r);
                        else needsHead = true;
                    }
                }
                if (starts.Count == 0)
                {
                    state.Why = needsHead ? "Üst kata su çıkmıyor: ağa su terazisi kur." : "Su yoluna bağlı değil: altından ya da yanından boru geçir.";
                    continue;
                }
                foreach (int s in starts) if (!_nodes[s].Passable && _nodes[s].Kind == Kind.Conduit) frozen = true;

                var path = FindSupply(starts, use);
                if (path == null)
                {
                    bool anySource = false;
                    foreach (int s in starts)
                    {
                        int c = _nodes[s].Comp;
                        if (c < 0) continue;
                        foreach (var n in _nodes) if (n.Comp == c && n.Initial > 0) { anySource = true; break; }
                        if (anySource) break;
                    }
                    state.Why = frozen && !anySource ? "Ark dondu: kışın su boruyla gelir."
                        : !anySource ? "Ağda boruya su veren kaynak yok: göl, dere, sulama dolabı, kova-makara ya da sarnıç gerekir (kuyudan su kovayla çekilir)."
                        : "Su yetmiyor: kaynak az ya da boru dar (künk, kurşun boru, taş ark daha çok taşır).";
                    continue;
                }
                foreach (int i in path)
                {
                    var n = _nodes[i];
                    if (n.Kind == Kind.Conduit) n.Capacity -= use;
                }
                var src = _nodes[path[path.Count - 1]];
                src.Supply -= use;
                Draw(report, src, use);
                state.Supplied = true;
                report.SuppliedDemand += use;
            }

            // Kalan kendiliğinden akan su (göl, dere, kaynak eşyası; sarnıç değil) aynı ağdaki sarnıçları doldurur.
            // Kazılan göletten sarnıca giden su da göleti alçaltır.
            foreach (var c in _nodes)
            {
                if (c.Kind != Kind.Item || !c.Item.Def.IsCistern || c.Comp < 0) continue;
                int room = c.Item.Def.CisternCapacity - c.Item.Fill;
                int added = 0;
                foreach (var n in _nodes)
                {
                    if (room - added <= 0) break;
                    if (n.Comp != c.Comp || n.Supply <= 0 || n.Kind == Kind.Item && n.Item.Def.IsCistern) continue;
                    int take = Math.Min(n.Supply, room - added);
                    n.Supply -= take;
                    added += take;
                    if (n.Kind == Kind.Body) Draw(report, n, take);
                }
                if (added > 0) report.Refill[c.Item.Id] = added;
            }
        }

        private static void Draw(WaterReport report, Node src, int amount)
        {
            if (src.Kind == Kind.Item) report.Drawn[src.Item.Id] = (report.Drawn.TryGetValue(src.Item.Id, out int dr) ? dr : 0) + amount;
            else if (src.Kind == Kind.Body && src.Pond != null && !src.Pond.Spring)
                report.PondDrawn[src.Pond] = (report.PondDrawn.TryGetValue(src.Pond, out int pd) ? pd : 0) + amount;
        }

        // Tesisattan geriye, kapasitesi yeten düğümlerden en yakın yeterli kaynağa (BFS). Yol: başlangıçtan kaynağa.
        private List<int> FindSupply(List<int> starts, int use)
        {
            var parent = new Dictionary<int, int>();
            var q = new Queue<int>();
            foreach (int s in starts)
            {
                if (parent.ContainsKey(s) || !Usable(_nodes[s], use)) continue;
                parent[s] = -1; q.Enqueue(s);
            }
            while (q.Count > 0)
            {
                int i = q.Dequeue();
                var n = _nodes[i];
                if (n.Supply >= use)
                {
                    var path = new List<int>();
                    for (int k = i; k >= 0; k = parent[k]) path.Add(k);
                    path.Reverse();      // başlangıç → kaynak
                    return path;
                }
                if (n.Leaf) continue;
                foreach (int j in n.Adj)
                {
                    if (parent.ContainsKey(j) || !Usable(_nodes[j], use)) continue;
                    parent[j] = i; q.Enqueue(j);
                }
            }
            return null;
        }

        private static bool Usable(Node n, int use) => n.Passable && !n.Leaf && n.Capacity >= use;
    }

    /// <summary>
    /// Su işleri servisi: şebekeyi değişiklikte yeniden hesaplar (arayüz her karede okur), gün başında sarnıçları doldurur
    /// ve boşaltır, akarsulu helayı yıkar. Tesisata su gelirken banyo için kova taşınmaz.
    /// </summary>
    public sealed class WaterWorks
    {
        private readonly WorldState _world;
        private WaterReport _report;

        /// <summary>Dolu bir gölet karosunun tuttuğu su (birim): 3x3 dolu gölet 90 birim. Çekilen su göleti bu oranla alçaltır.</summary>
        public const float PondUnitsPerTile = 10f;

        public WaterWorks(WorldState world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _world.Changed += Invalidate;
            _world.Map.Changed += Invalidate;
            _world.Map.ConduitsChanged += Invalidate;
            _world.Map.RoadsChanged += Invalidate;      // donma da bu olayla gelir
        }

        /// <summary>Kapak açıp kapamak gibi olaysız değişikliklerden sonra çağrılır.</summary>
        public void Invalidate() => _report = null;

        /// <summary>Güncel hesap (gerekirse yeniden).</summary>
        public WaterReport Report => _report ?? (_report = Solve(_world));

        public static WaterReport Solve(WorldState world)
        {
            var report = new WaterReport();
            var g = new WaterGraph(world);
            g.Connect(report);
            g.Allocate(report);
            return report;
        }

        public bool IsSupplied(ItemInstance it) => it != null && it.Def.IsFixture && Report.IsSupplied(it);

        /// <summary>Su gelen çeşme ve lülelerin yanındaki karolar: kova buradan doldurulur.</summary>
        public HashSet<GridPos> TapAccess()
        {
            var goals = new HashSet<GridPos>();
            var map = _world.Map;
            var r = Report;
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || !it.Def.IsTap || !r.IsSupplied(it)) continue;
                foreach (var t in it.OccupiedTiles())
                    foreach (var o in new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) })
                    {
                        var n = t + o;
                        if (map.InBounds(n) && map.IsWalkable(n) && !map.EdgeBlocks(GridMap.EdgeBetween(t, n))) goals.Add(n);
                    }
            }
            return goals;
        }

        /// <summary>Kova doldurulacak bütün yerler: kuyu, gölet, dere kıyısı ve su gelen çeşmeler.</summary>
        public HashSet<GridPos> AllWaterAccess()
        {
            var goals = WaterNetwork.WaterAccess(_world);
            goals.UnionWith(TapAccess());
            return goals;
        }

        /// <summary>Gün sonu özeti.</summary>
        public sealed class DayResult
        {
            public int Flushed, CisternsFilled, DryFixtures, PondDrawn;
        }

        /// <summary>
        /// Gün başı: sarnıçtan çekilen su düşer, yağmurlu günde açıktaki sarnıç dolar, ağda artan su sarnıca gider;
        /// suyu gelen akarsulu hela yıkanır (gübre çıkmaz, boşaltma işi kalmaz).
        /// </summary>
        public DayResult OnNewDay(bool rained)
        {
            var r = Solve(_world);
            var result = new DayResult();
            foreach (var kv in r.PondDrawn)
            {
                kv.Key.Level = Math.Max(0f, kv.Key.Level - kv.Value / (kv.Key.Tiles.Count * PondUnitsPerTile));
                result.PondDrawn += kv.Value;
            }
            var rooms = _world.Map.Rooms;
            foreach (var it in _world.Items)
            {
                if (it.IsDecor) continue;
                var d = it.Def;
                if (d.IsCistern)
                {
                    int before = it.Fill;
                    int fill = it.Fill - (r.Drawn.TryGetValue(it.Id, out int drawn) ? drawn : 0);
                    if (r.Refill.TryGetValue(it.Id, out int add)) fill += add;
                    var room = rooms.RoomAt(it.Position);
                    if (rained && (room == null || !room.IsIndoor)) fill += d.CisternRain;
                    it.Fill = Math.Max(0, Math.Min(d.CisternCapacity, fill));
                    if (it.Fill > before) result.CisternsFilled++;
                }
                else if (d.IsFixture)
                {
                    if (!r.IsSupplied(it)) { result.DryFixtures++; continue; }
                    if (d.FillCapacity > 0 && it.Fill > 0) { it.Fill = 0; result.Flushed++; }
                }
            }
            Invalidate();
            return result;
        }
    }
}
