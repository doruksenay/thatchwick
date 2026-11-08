using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Build
{
    /// <summary>Oda şablonunun maliyet dökümü.</summary>
    public sealed class TemplateQuote
    {
        public int Akce;
        public int WallEdges, DoorEdges, WindowEdges, FloorTiles;
        public readonly List<KeyValuePair<ItemDef, int>> Items = new List<KeyValuePair<ItemDef, int>>();
        /// <summary>Envanterde olduğu için ödenmeyecek eşya adedi.</summary>
        public int FromInventory;
        public string Problem;
        public bool Ok => Problem == null;
    }

    /// <summary>
    /// Hazır oda şablonları (Adım 43): duvar, kapı, pencere, zemin ve eşyalar tek tıkla kurulur, tek adımda geri alınır.
    /// Envanterde olan eşya önce kullanılır, eksik olanlar satın alınır. Hepsi yapılabilirse yapılır, değilse hiçbir şey değişmez.
    /// </summary>
    public sealed class RoomTemplateService
    {
        private readonly BuildService _build;
        private readonly WorldState _world;
        private readonly Household _house;
        private readonly ContentDatabase _content;

        public RoomTemplateService(BuildService build, WorldState world, Household house, ContentDatabase content)
        {
            _build = build ?? throw new ArgumentNullException(nameof(build));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public static bool TryParseFloor(string name, out FloorType floor)
        {
            floor = FloorType.Grass;
            if (string.IsNullOrWhiteSpace(name)) return false;
            return Enum.TryParse(name, true, out floor) && Enum.IsDefined(typeof(FloorType), floor);
        }

        /// <summary>Şablonun kenarları (duvar/kapı/pencere), kapı ve pencereler duvarın yerine konur.</summary>
        public static List<KeyValuePair<Edge, EdgeKind>> Edges(RoomTemplateDef t, GridPos origin)
        {
            var edges = new List<KeyValuePair<Edge, EdgeKind>>();
            int w = t.Width, h = t.Height, ox = origin.X, oy = origin.Y;
            for (int i = 0; i < w; i++)
            {
                edges.Add(new KeyValuePair<Edge, EdgeKind>(new Edge(ox + i, oy, EdgeAxis.Horizontal), EdgeKind.Wall));       // kuzey
                edges.Add(new KeyValuePair<Edge, EdgeKind>(new Edge(ox + i, oy + h, EdgeAxis.Horizontal), EdgeKind.Wall));   // güney
            }
            for (int j = 0; j < h; j++)
            {
                edges.Add(new KeyValuePair<Edge, EdgeKind>(new Edge(ox, oy + j, EdgeAxis.Vertical), EdgeKind.Wall));         // batı
                edges.Add(new KeyValuePair<Edge, EdgeKind>(new Edge(ox + w, oy + j, EdgeAxis.Vertical), EdgeKind.Wall));     // doğu
            }

            Edge SideEdge(TemplateSide side, int offset)
            {
                switch (side)
                {
                    case TemplateSide.North: return new Edge(ox + offset, oy, EdgeAxis.Horizontal);
                    case TemplateSide.South: return new Edge(ox + offset, oy + h, EdgeAxis.Horizontal);
                    case TemplateSide.West: return new Edge(ox, oy + offset, EdgeAxis.Vertical);
                    default: return new Edge(ox + w, oy + offset, EdgeAxis.Vertical);
                }
            }
            void Set(Edge e, EdgeKind kind)
            {
                for (int i = 0; i < edges.Count; i++) if (edges[i].Key.Equals(e)) { edges[i] = new KeyValuePair<Edge, EdgeKind>(e, kind); return; }
            }

            Set(SideEdge(t.DoorSide, t.DoorOffset), EdgeKind.Door);
            var opposite = (TemplateSide)(((int)t.DoorSide + 2) % 4);
            foreach (var wOffset in t.Windows) Set(SideEdge(opposite, wOffset), EdgeKind.Window);
            return edges;
        }

        /// <summary>Kenar uzunluğu: kuzey/güney genişlik, doğu/batı yükseklik.</summary>
        public static int SideLength(RoomTemplateDef t, TemplateSide side) =>
            side == TemplateSide.North || side == TemplateSide.South ? t.Width : t.Height;

        private int EdgeCost(EdgeKind kind)
        {
            int wall = BuildService.EdgePrice(EdgeKind.Wall);
            int refund = (int)Math.Floor(wall * _build.SellRatio);
            return kind == EdgeKind.Wall ? wall : wall + Math.Max(0, BuildService.EdgePrice(kind) - refund);
        }

        private int FloorCost(FloorType floor, FloorType old)
        {
            int refund = (int)Math.Floor(BuildService.FloorPrice(old) * _build.SellRatio);
            return Math.Max(0, BuildService.FloorPrice(floor) - refund);
        }

        public TemplateQuote Quote(string templateId, GridPos origin)
        {
            var q = new TemplateQuote();
            if (!_content.Templates.TryGet(templateId, out var t)) { q.Problem = "Böyle bir şablon yok."; return q; }
            var map = _world.Map;

            // Sınır ve arsa.
            for (int y = 0; y < t.Height; y++)
                for (int x = 0; x < t.Width; x++)
                {
                    var tile = new GridPos(origin.X + x, origin.Y + y);
                    if (!map.InBounds(tile)) { q.Problem = "Harita dışına taşıyor."; return q; }
                    if (!map.IsOwned(tile)) { q.Problem = BuildService.NotOwned; return q; }
                }

            foreach (var kv in Edges(t, origin))
            {
                if (!map.EdgeInBounds(kv.Key)) { q.Problem = "Duvarlar harita dışına taşıyor."; return q; }
                var old = map.GetEdge(kv.Key);
                if (old == kv.Value) continue;
                q.Akce += old == EdgeKind.None ? EdgeCost(kv.Value) : Math.Max(0, BuildService.EdgePrice(kv.Value) - (int)Math.Floor(BuildService.EdgePrice(old) * _build.SellRatio));
                if (kv.Value == EdgeKind.Wall) q.WallEdges++; else if (kv.Value == EdgeKind.Door) q.DoorEdges++; else q.WindowEdges++;
            }

            if (!string.IsNullOrWhiteSpace(t.Floor))
            {
                if (!TryParseFloor(t.Floor, out var floor)) { q.Problem = "Şablon zemini geçersiz."; return q; }
                for (int y = 0; y < t.Height; y++)
                    for (int x = 0; x < t.Width; x++)
                    {
                        var old = map.GetFloor(new GridPos(origin.X + x, origin.Y + y));
                        if (old == floor) continue;
                        q.Akce += FloorCost(floor, old);
                        q.FloorTiles++;
                    }
            }

            // Eşyalar: önce envanter, eksik olan satın alınır.
            var needed = new Dictionary<string, int>(StringComparer.Ordinal);
            var occupied = new HashSet<GridPos>();
            foreach (var ti in t.Items)
            {
                if (!_content.Items.TryGet(ti.ItemId, out var def)) { q.Problem = $"Eşya '{ti.ItemId}' bulunamadı."; return q; }
                var pos = new GridPos(origin.X + ti.X, origin.Y + ti.Y);
                var check = _world.CanPlaceFloor(def, pos, ti.Rotation);
                if (!check.Ok) { q.Problem = $"{def.DisplayName}: {check.Reason}"; return q; }
                var (fw, fh) = Footprint.Dimensions(def.Width, def.Height, ti.Rotation);
                for (int dy = 0; dy < fh; dy++)
                    for (int dx = 0; dx < fw; dx++)
                        if (!occupied.Add(new GridPos(pos.X + dx, pos.Y + dy))) { q.Problem = $"{def.DisplayName}: şablon eşyaları çakışıyor."; return q; }
                needed[def.Id] = (needed.TryGetValue(def.Id, out var n) ? n : 0) + 1;
            }
            foreach (var kv in needed)
            {
                var def = _content.Items.Get(kv.Key);
                if (!_build.IsUnlocked(def)) { q.Problem = $"{def.DisplayName}: hesap seviyesi {def.UnlockLevel} gerekli."; return q; }
                int have = Math.Min(kv.Value, _house.Count(kv.Key));
                int buy = kv.Value - have;
                q.FromInventory += have;
                if (buy > 0)
                {
                    if (def.Price <= 0) { q.Problem = $"{def.DisplayName} satılmıyor ve envanterde yok."; return q; }
                    q.Akce += _build.PriceFor(def) * buy;
                }
                q.Items.Add(new KeyValuePair<ItemDef, int>(def, kv.Value));
            }
            return q;
        }

        public string CanBuild(string templateId, GridPos origin)
        {
            var q = Quote(templateId, origin);
            if (!q.Ok) return q.Problem;
            if (_house.Akce < q.Akce) return $"Akçe yetmiyor ({q.Akce} gerekir).";
            return null;
        }

        /// <summary>Şablonu kurar; tek adımda geri alınabilir. Başarısızlıkta dünya olduğu gibi kalır.</summary>
        public PlacementCheck Build(string templateId, GridPos origin, out List<ItemInstance> placed)
        {
            placed = new List<ItemInstance>();
            var problem = CanBuild(templateId, origin);
            if (problem != null) return PlacementCheck.Fail(problem);
            var t = _content.Templates.Get(templateId);
            var map = _world.Map;

            int undoBefore = _build.UndoPushes;
            _build.BeginGroup(t.DisplayName);
            PlacementCheck failure = PlacementCheck.Success;
            try
            {
                foreach (var kv in Edges(t, origin))
                {
                    if (map.GetEdge(kv.Key) == kv.Value) continue;
                    if (kv.Value != EdgeKind.Wall && map.GetEdge(kv.Key) == EdgeKind.None)
                    {
                        var w = _build.SetEdge(kv.Key, EdgeKind.Wall);
                        if (!w.Ok) { failure = w; break; }
                    }
                    var r = _build.SetEdge(kv.Key, kv.Value);
                    if (!r.Ok) { failure = r; break; }
                }

                if (failure.Ok && TryParseFloor(t.Floor, out var floor))
                    for (int y = 0; y < t.Height && failure.Ok; y++)
                        for (int x = 0; x < t.Width; x++)
                        {
                            var tile = new GridPos(origin.X + x, origin.Y + y);
                            if (map.GetFloor(tile) == floor) continue;
                            var r = _build.PaintFloor(tile, floor);
                            if (!r.Ok) { failure = r; break; }
                        }

                if (failure.Ok)
                    foreach (var ti in t.Items)
                    {
                        if (_house.Count(ti.ItemId) <= 0)
                        {
                            var b = _build.Buy(ti.ItemId);
                            if (!b.Ok) { failure = b; break; }
                        }
                        var pos = new GridPos(origin.X + ti.X, origin.Y + ti.Y);
                        var p = _build.PlaceFromInventory(ti.ItemId, pos, ti.Rotation, out var item);
                        if (!p.Ok) { failure = p; break; }
                        placed.Add(item);
                    }
            }
            finally
            {
                _build.EndGroup();
            }

            if (!failure.Ok)
            {
                if (_build.UndoPushes > undoBefore) _build.Undo();   // yapılanları geri al
                placed.Clear();
                return failure;
            }
            return PlacementCheck.Success;
        }
    }
}
