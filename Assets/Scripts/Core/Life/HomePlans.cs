using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Bir planın ya da mevsim setinin eşya kaydı. Kind: 0 yer (halı, tavan dahil), 1 duvar, 2 dekor.</summary>
    public sealed class PlanEntry
    {
        public string Item;
        public int Kind;
        public int X, Y, Z, Rot, Band;
        /// <summary>Dekor: planda üstünde durduğu eşyanın sırası; mevsim setinde -1 (ParentItem ve PX/PY ile bulunur).</summary>
        public int Parent = -1;
        public string Slot;
        public string ParentItem;
        public int PX, PY, PZ;

        public string Encode() => string.Join("|", Item, Kind, X, Y, Z, Rot, Band, Parent, Slot ?? "", ParentItem ?? "", PX, PY, PZ);
        public static PlanEntry Decode(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            var p = s.Split('|');
            if (p.Length < 13) return null;
            int I(int k) { int.TryParse(p[k], out int v); return v; }
            return new PlanEntry
            {
                Item = p[0], Kind = I(1), X = I(2), Y = I(3), Z = I(4), Rot = I(5), Band = I(6), Parent = I(7),
                Slot = p[8].Length > 0 ? p[8] : null, ParentItem = p[9].Length > 0 ? p[9] : null, PX = I(10), PY = I(11), PZ = I(12)
            };
        }
    }

    /// <summary>Kaydedilmiş oda planı: eşyalar odanın sol üst köşesine göre.</summary>
    public sealed class RoomPlan
    {
        public string Name;
        public int Width, Height;
        public List<PlanEntry> Entries = new List<PlanEntry>();
    }

    public sealed class PlanResult
    {
        public int Placed, Skipped, Ordered, Bought;
        public readonly List<string> Notes = new List<string>();
        public string Text => $"{Placed} eşya yerleşti" + (Bought > 0 ? $", {Bought} satın alındı" : "") + (Ordered > 0 ? $", {Ordered} sipariş edildi (yarın sabah)" : "")
            + (Skipped > 0 ? $", {Skipped} sığmadı" : "") + ".";
    }

    public sealed partial class HomeLife
    {
        public const int MaxPlans = 10;
        private readonly List<RoomPlan> _plans = new List<RoomPlan>();
        private readonly Dictionary<Season, List<PlanEntry>> _seasonSets = new Dictionary<Season, List<PlanEntry>>();
        public IReadOnlyList<RoomPlan> Plans => _plans;
        /// <summary>Mevsim dönünce kayıtlı set kendiliğinden asılsın mı (ev görevlisi işi).</summary>
        public bool AutoSeasonSwap { get; set; } = true;

        private static bool Skippable(ItemInstance it) =>
            it.Def.IsKeepsake || it.Def.IsPlant || it.Def.IsStairs || it.Def.IsForage || it.Def.Id == Decor.Debris || it.Def.IsHousing;

        private static (int x, int y, int z) Corner(Room room)
        {
            int x = int.MaxValue, y = int.MaxValue, z = 0;
            foreach (var t in room.Tiles) { if (t.X < x) x = t.X; if (t.Y < y) y = t.Y; z = t.Z; }
            return (x, y, z);
        }

        /// <summary>Odanın döşemesini plan olarak kaydeder (hatıra, bitki, merdiven alınmaz). Hata metni ya da null.</summary>
        public string SavePlan(Room room, string name)
        {
            if (room == null || !room.IsIndoor) return "Plan yalnızca kapalı bir odadan alınır.";
            if (_plans.Count >= MaxPlans) return $"En çok {MaxPlans} plan saklanır; birini sil.";
            var (ox, oy, _) = Corner(room);
            int mx = 0, my = 0;
            foreach (var t in room.Tiles) { mx = Math.Max(mx, t.X - ox + 1); my = Math.Max(my, t.Y - oy + 1); }
            var plan = new RoomPlan { Name = string.IsNullOrWhiteSpace(name) ? $"Plan {_plans.Count + 1}" : name.Trim(), Width = mx, Height = my };
            var index = new Dictionary<int, int>();
            var inRoom = new List<ItemInstance>();
            foreach (var it in _world.Items) if (RoomOf(it) == room && !Skippable(it)) inRoom.Add(it);
            foreach (var pass in new[] { 0, 1 })
                foreach (var it in inRoom)
                {
                    if (it.IsDecor) continue;
                    bool wall = it.IsWallMounted;
                    if ((pass == 1) != wall) continue;
                    index[it.Id] = plan.Entries.Count;
                    plan.Entries.Add(new PlanEntry { Item = it.Def.Id, Kind = wall ? 1 : 0, X = it.Position.X - ox, Y = it.Position.Y - oy, Rot = it.Rotation, Band = it.WallBand });
                }
            foreach (var it in inRoom)
            {
                if (!it.IsDecor || !it.ParentId.HasValue || !index.TryGetValue(it.ParentId.Value, out int parent)) continue;
                plan.Entries.Add(new PlanEntry { Item = it.Def.Id, Kind = 2, Parent = parent, Slot = it.SlotId });
            }
            if (plan.Entries.Count == 0) return "Odada kaydedilecek eşya yok.";
            _plans.Add(plan);
            return null;
        }

        public bool DeletePlan(int index)
        {
            if (index < 0 || index >= _plans.Count) return false;
            _plans.RemoveAt(index);
            return true;
        }

        /// <summary>Planın eksikleri: envanterde olmayan eşyalar ve satın alma bedeli.</summary>
        public (Dictionary<string, int> missing, int cost) PlanNeeds(RoomPlan plan)
        {
            var need = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var e in plan.Entries) need[e.Item] = (need.TryGetValue(e.Item, out var n) ? n : 0) + 1;
            var missing = new Dictionary<string, int>(StringComparer.Ordinal);
            int cost = 0;
            foreach (var kv in need)
            {
                int lack = kv.Value - _house.Count(kv.Key);
                if (lack <= 0) continue;
                missing[kv.Key] = lack;
                if (_content.Items.TryGet(kv.Key, out var d)) cost += (Build != null ? Build.PriceFor(d) : d.Price) * lack;
            }
            return (missing, cost);
        }

        /// <summary>
        /// Planı başka bir odaya uygular (tek geri alma adımı). Önce envanter; eksik eşya buy=true ise satın alınır,
        /// order=true ise sipariş edilir (yarın sabah gelir, yerine oyuncu koyar). Sığmayan eşya atlanır.
        /// </summary>
        public PlanResult ApplyPlan(RoomPlan plan, Room room, bool buy, bool order)
        {
            var r = new PlanResult();
            if (plan == null || room == null || !room.IsIndoor || Build == null) { r.Notes.Add("Plan uygulanamaz."); return r; }
            var (ox, oy, oz) = Corner(room);
            var placed = new Dictionary<int, ItemInstance>();
            Build.BeginGroup("Oda planı: " + plan.Name);
            try
            {
                for (int k = 0; k < plan.Entries.Count; k++)
                {
                    var e = plan.Entries[k];
                    if (!_content.Items.TryGet(e.Item, out var def)) { r.Skipped++; continue; }
                    var tile = new GridPos(ox + e.X, oy + e.Y, oz);
                    ItemInstance parent = null;
                    PlacementCheck fits;
                    if (e.Kind == 2)
                    {
                        if (!placed.TryGetValue(e.Parent, out parent)) { r.Skipped++; continue; }
                        fits = _world.CanPlaceOnSurface(def, parent.Id, e.Slot);
                    }
                    else if (e.Kind == 1) fits = _world.CanPlaceOnWall(def, tile, e.Rot, e.Band, null);
                    else fits = def.IsCeiling ? _world.CanPlaceCeiling(def, tile) : def.IsRug ? _world.CanPlaceRug(def, tile, e.Rot) : _world.CanPlaceFloor(def, tile, e.Rot);
                    if (!fits.Ok || !_world.Map.IsOwned(tile) && e.Kind != 2) { r.Skipped++; continue; }
                    if (_house.Count(e.Item) <= 0)
                    {
                        if (order && def.Price > 0 && !def.IsRetired) { if (Build.Order(e.Item).Ok) r.Ordered++; else r.Skipped++; continue; }
                        if (!buy || !Build.Buy(e.Item).Ok) { r.Skipped++; continue; }
                        r.Bought++;
                    }
                    ItemInstance got;
                    PlacementCheck done = e.Kind == 2 ? Build.PlaceDecorFromInventory(e.Item, parent.Id, e.Slot, out got)
                        : e.Kind == 1 ? Build.PlaceWallFromInventory(e.Item, tile, e.Rot, out got, e.Band)
                        : Build.PlaceFromInventory(e.Item, tile, e.Rot, out got);
                    if (done.Ok && got != null) { placed[k] = got; r.Placed++; }
                    else r.Skipped++;
                }
            }
            finally { Build.EndGroup(); }
            return r;
        }

        // ---- mevsim setleri ----

        public bool HasSeasonSet(Season s) => _seasonSets.TryGetValue(s, out var l) && l.Count > 0;
        public int SeasonSetSize(Season s) => _seasonSets.TryGetValue(s, out var l) ? l.Count : 0;

        /// <summary>Şu an asılı olan bu mevsimin süslerini set olarak kaydeder. Kaydedilen adet.</summary>
        public int SaveSeasonSet(Season season)
        {
            var list = new List<PlanEntry>();
            foreach (var it in _world.Items)
            {
                if (!it.Def.IsSeasonal || !it.Def.InSeason(season)) continue;
                var e = new PlanEntry { Item = it.Def.Id, X = it.Position.X, Y = it.Position.Y, Z = it.Position.Z, Rot = it.Rotation, Band = it.WallBand };
                if (it.IsDecor && it.ParentId.HasValue)
                {
                    var p = _world.GetItem(it.ParentId.Value);
                    if (p == null) continue;
                    e.Kind = 2; e.ParentItem = p.Def.Id; e.PX = p.Position.X; e.PY = p.Position.Y; e.PZ = p.Position.Z; e.Slot = it.SlotId;
                }
                else e.Kind = it.IsWallMounted ? 1 : 0;
                list.Add(e);
            }
            _seasonSets[season] = list;
            return list.Count;
        }

        public void ClearSeasonSet(Season season) => _seasonSets.Remove(season);

        /// <summary>
        /// Mevsim döndü: kayıtlı set varsa solan süsler envantere kalkar, yeni mevsimin seti envanterden asılır (satın alınmaz).
        /// (asılan, eksik) döner.
        /// </summary>
        public (int hung, int missing) OnSeasonChanged(Season season)
        {
            if (!AutoSeasonSwap || !HasSeasonSet(season)) return (0, 0);
            foreach (var it in new List<ItemInstance>(_world.Items))
                if (DecorSystem.IsWilted(it, season) && _world.GetItem(it.Id) != null)
                    foreach (var gone in _world.RemoveItemInstances(it.Id)) _house.AddItem(gone.Def.Id, 1, gone.Condition);
            int hung = 0, missing = 0;
            foreach (var e in _seasonSets[season])
            {
                if (!_content.Items.TryGet(e.Item, out var def) || _house.Count(e.Item) <= 0) { missing++; continue; }
                var tile = new GridPos(e.X, e.Y, e.Z);
                ItemInstance placed = null;
                if (e.Kind == 2)
                {
                    ItemInstance parent = null;
                    foreach (var it in _world.Items)
                        if (!it.IsDecor && it.Def.Id == e.ParentItem && it.Position.X == e.PX && it.Position.Y == e.PY && it.Position.Z == e.PZ) { parent = it; break; }
                    if (parent != null && _world.CanPlaceOnSurface(def, parent.Id, e.Slot).Ok && _house.TryTakeItem(e.Item, out var cond))
                    { placed = _world.PlaceOnSurface(def, parent.Id, e.Slot); placed.Condition = cond; }
                }
                else if (e.Kind == 1)
                {
                    if (_world.CanPlaceOnWall(def, tile, e.Rot, e.Band, null).Ok && _house.TryTakeItem(e.Item, out var cond))
                    { placed = _world.PlaceOnWall(def, tile, e.Rot, e.Band); placed.Condition = cond; }
                }
                else if (_world.CanPlaceFloor(def, tile, e.Rot).Ok && _house.TryTakeItem(e.Item, out var cond))
                { placed = _world.PlaceItem(def, tile, e.Rot); placed.Condition = cond; }
                if (placed != null) hung++; else missing++;
            }
            if (hung > 0 || missing > 0)
            {
                _world.RaiseChanged();
                Say($"Mevsim seti asıldı: {hung} süs" + (missing > 0 ? $" ({missing} eksik: envanterde yok ya da yeri dolu)" : "") + ".");
            }
            return (hung, missing);
        }
    }
}
