using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Build
{
    /// <summary>
    /// Yapı modu işlemleri: satın al, sat, envanterden yerleştir, envantere al, taşı.
    /// Kurallar burada, arayüz yalnızca çağırır ve nedeni gösterir.
    /// </summary>
    public sealed partial class BuildService
    {
        public const double DefaultSellRatio = 0.5;

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly ContentDatabase _content;

        public double SellRatio { get; }

        // ---- geri al (Adım 43) ----

        private sealed class UndoStep
        {
            public string Label;
            public Func<PlacementCheck> Run;
        }

        public const int UndoCapacity = 30;
        private readonly List<UndoStep> _undo = new List<UndoStep>();
        private List<UndoStep> _group;
        private string _groupLabel;
        private bool _undoing;
        // Bir kez geri alınamayan adım: yığında kalır (sebep geçici olabilir: akçe yetmedi, biri duruyor). Hemen ardından
        // yine geri alınamazsa atılır; böylece kalıcı olarak geri alınamayan bir adım altındakileri kilitlemez.
        private UndoStep _failedOnce;

        public bool CanUndo => _undo.Count > 0;
        public string UndoLabel => _undo.Count > 0 ? _undo[_undo.Count - 1].Label : null;
        public int UndoCount => _undo.Count;
        /// <summary>Şimdiye dek kaydedilen toplam geri alma adımı (kapasite taşsa da artar).</summary>
        public int UndoPushes { get; private set; }
        public void ClearUndo() { _undo.Clear(); _group = null; _failedOnce = null; }

        private void Record(string label, Func<PlacementCheck> undo)
        {
            if (_undoing) return;
            var step = new UndoStep { Label = label, Run = undo };
            if (_group != null) { _group.Add(step); return; }
            _failedOnce = null;
            _undo.Add(step);
            UndoPushes++;
            if (_undo.Count > UndoCapacity) _undo.RemoveAt(0);
        }

        /// <summary>Birden çok işlemi tek geri alma adımında toplar (oda şablonu).</summary>
        public void BeginGroup(string label)
        {
            if (_group != null) return;
            _group = new List<UndoStep>();
            _groupLabel = label;
        }

        public void EndGroup()
        {
            if (_group == null) return;
            var steps = _group;
            _group = null;
            if (steps.Count == 0) return;
            Record(_groupLabel, () =>
            {
                // Geri alınan alt adım listeden çıkar: bir alt adım takılırsa yeniden deneme kaldığı yerden sürer,
                // önceden geri alınmış adımları ikinci kez çalıştırmaz.
                for (int i = steps.Count - 1; i >= 0; i--)
                {
                    var r = steps[i].Run();
                    if (!r.Ok) return r;
                    steps.RemoveAt(i);
                }
                return PlacementCheck.Success;
            });
        }

        /// <summary>Son yapı işlemini geri alır (akçe de geri döner).</summary>
        public PlacementCheck Undo()
        {
            if (_undo.Count == 0) return PlacementCheck.Fail("Geri alınacak bir şey yok.");
            var step = _undo[_undo.Count - 1];
            _undoing = true;
            PlacementCheck result;
            try { result = step.Run(); }
            finally { _undoing = false; }
            if (result.Ok)
            {
                _undo.RemoveAt(_undo.Count - 1);
                _failedOnce = null;
                return result;
            }
            if (_failedOnce == step)
            {
                // İkinci deneme de olmadı: adım atılır, altındaki adımlar geri alınabilir kalır.
                _undo.RemoveAt(_undo.Count - 1);
                _failedOnce = null;
                return PlacementCheck.Fail($"{result.Reason} Bu adım geri alınamadı ve atlandı.");
            }
            _failedOnce = step;
            return PlacementCheck.Fail($"{result.Reason} (Engel kalkınca yeniden dene; bir daha başarısız olursa adım atlanır.)");
        }

        private void GiveBack(List<ItemDef> defs, int from = 0)
        {
            for (int i = from; i < defs.Count; i++) _house.AddItem(defs[i].Id);
        }

        /// <summary>Kaldırılan eşyaları durumlarıyla envantere koyar (döküntü yığını içindekileri de verir, kendisi envantere girmez).</summary>
        private void GiveBack(List<ItemInstance> items, int from = 0)
        {
            for (int i = from; i < items.Count; i++)
            {
                var it = items[i];
                if (it.Def.Id == Decor.Debris) { foreach (var c in it.Contents) _house.AddItem(c.ItemId, 1, c.Condition); continue; }
                _house.AddItem(it.Def.Id, 1, it.Condition);
            }
        }

        /// <summary>Örneğin satış değeri: fiyatın oranı × durum çarpanı (hasarlı %50, onarılmış %75).</summary>
        public int SellValue(ItemInstance item) => item == null ? 0 : (int)Math.Floor(item.Def.Price * SellRatio * Decor.SellFactor(item.Condition));

        /// <summary>Envanterdeki belirli durumdaki bir adedin satış değeri.</summary>
        public int SellValue(ItemDef def, ItemCondition condition) => (int)Math.Floor(def.Price * SellRatio * Decor.SellFactor(condition));

        /// <summary>Depo kapasitesi; null ise sınırsız (testler, eski kurulumlar).</summary>
        public StorageService Storage { get; set; }

        /// <summary>Hesap seviyesi sağlayıcı (T10); null ise her eşya açık.</summary>
        public Func<int> AccountLevel { get; set; }

        /// <summary>İndirim oranı (0-0.5); hane reisinin Tüccar/Usta dalı gibi özelliklerinden gelir. Yoksa indirim yok.</summary>
        public Func<float> Discount { get; set; }

        /// <summary>İndirimli alış fiyatı (en az 1 akçe).</summary>
        public int PriceFor(ItemDef def)
        {
            float d = Discount == null ? 0f : Math.Max(0f, Math.Min(0.5f, Discount()));
            return d <= 0f ? def.Price : Math.Max(1, (int)Math.Round(def.Price * (1f - d)));
        }

        /// <summary>Anıt (mezar) ve yabani toplanabilir eşyalar oyuncu tarafından taşınmaz, satılmaz.</summary>
        public static bool IsLocked(ItemInstance item) =>
            item != null && (item.Def.Category == "memorial" || item.Def.IsForage);

        private const string Locked = "Bu taşınmaz ya da satılmaz.";

        /// <summary>Barınakta yaşayan hayvan sayısı (hayvan sistemi verir); null ise hep 0. Doluyken barınak kaldırılmaz.</summary>
        public Func<int, int> HousingResidents { get; set; }

        private const string HousingOccupied = "İçinde hayvan var: önce hayvanları sat.";
        private bool IsOccupiedHousing(ItemInstance item) => item.Def.IsHousing && HousingResidents != null && HousingResidents(item.Id) > 0;

        private const string StairsInUse = "O katta biri var ve başka merdiven yok: önce onu çağır.";

        /// <summary>Eşya dünyadan kaldırılamıyorsa nedeni (dolu barınak, birini katta bırakacak merdiven); kaldırılabiliyorsa null.</summary>
        private string RemovalBlocker(ItemInstance item)
        {
            if (IsOccupiedHousing(item)) return HousingOccupied;
            if (item.Def.IsStairs && !item.IsDecor && StrandsSomeone(item)) return StairsInUse;
            return null;
        }

        // Merdiven zemin kat ile bir üst/alt katı bağlar; o katta biri varken son merdiven kaldırılmaz.
        private bool StrandsSomeone(ItemInstance stairs)
        {
            int a = stairs.Position.Z, b = a + stairs.Def.StairsDelta;
            int remote = a != 0 ? a : b;
            if (remote == 0 || !_world.AnyCharacterOnLevel(remote)) return false;
            foreach (var other in _world.Items)
            {
                if (other == stairs || !other.Def.IsStairs || other.IsDecor) continue;
                int oa = other.Position.Z, ob = oa + other.Def.StairsDelta;
                if (oa == remote || ob == remote) return false;
            }
            return true;
        }

        private const string StairsCutOff = "Merdiven oraya taşınırsa o katta kalan biri inemez.";

        // Zemin kat dışında duran herkes kendi katındaki bir merdiven ucuna yürüyebiliyor mu.
        private bool AnyoneCutOff()
        {
            if (_world.CharacterPositions == null) return false;
            List<GridPos> ends = null;
            foreach (var p in _world.CharacterPositions())
            {
                if (p.Z == 0) continue;
                if (ends == null)
                {
                    ends = new List<GridPos>();
                    foreach (var it in _world.Items)
                    {
                        if (!it.Def.IsStairs || it.IsDecor) continue;
                        WorldState.StairsEnds(it.Def, it.Position, it.Rotation, out var entrance, out var landing);
                        ends.Add(entrance);
                        ends.Add(landing);
                    }
                }
                var goals = new List<GridPos>();
                foreach (var e in ends) if (e.Z == p.Z) goals.Add(e);
                if (goals.Count == 0) return true;
                if (goals.Contains(p)) continue;
                if (Pathfinder.FindPathToAny(_world.Map, p, goals) == null) return true;
            }
            return false;
        }

        // Kaldırılan eşyanın yeri: geri alırken türüne göre (yer, duvar, yüzey yuvası) aynı yere konur.
        private sealed class Spot
        {
            public ItemDef Def;
            public GridPos Pos;
            public int Rot;
            public int? ParentId;
            public string SlotId;
            public bool Wall;
            public string Finish;
            public int Band = -1;
            public ItemCondition Condition;
        }

        private static Spot SpotOf(ItemInstance it, ItemDef def = null) =>
            new Spot { Def = def ?? it.Def, Pos = it.Position, Rot = it.Rotation, ParentId = it.ParentId, SlotId = it.SlotId, Wall = it.IsWallMounted,
                Finish = def == null || def == it.Def ? it.FinishId : null, Band = it.IsWallMounted ? it.WallBand : -1, Condition = it.Condition };

        private PlacementCheck CanPutBack(Spot s, int? ignoreItemId = null) =>
            s.ParentId.HasValue ? _world.CanPlaceOnSurface(s.Def, s.ParentId.Value, s.SlotId, ignoreItemId)
            : s.Wall ? _world.CanPlaceOnWall(s.Def, s.Pos, s.Rot, s.Band, ignoreItemId)
            : s.Def.IsRug ? _world.CanPlaceRug(s.Def, s.Pos, s.Rot, ignoreItemId)
            : s.Def.IsCeiling ? _world.CanPlaceCeiling(s.Def, s.Pos, ignoreItemId)
            : _world.CanPlaceFloor(s.Def, s.Pos, s.Rot, ignoreItemId);

        private ItemInstance PutBack(Spot s)
        {
            var it = s.ParentId.HasValue ? _world.PlaceOnSurface(s.Def, s.ParentId.Value, s.SlotId)
                : s.Wall ? _world.PlaceOnWall(s.Def, s.Pos, s.Rot, s.Band)
                : _world.PlaceItem(s.Def, s.Pos, s.Rot);
            it.FinishId = s.Finish;
            it.Condition = s.Condition;
            return it;
        }

        public bool IsUnlocked(ItemDef def) => AccountLevel == null || AccountLevel() >= def.UnlockLevel;

        /// <summary>Çiftçilik sistemi; verilirse pencereli sera odalarına da dikilebilir.</summary>
        public Work.PlantSystem Plants { get; set; }

        public BuildService(WorldState world, Household house, ContentDatabase content, double sellRatio = DefaultSellRatio)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            if (sellRatio < 0 || sellRatio > 1) throw new ArgumentOutOfRangeException(nameof(sellRatio));
            SellRatio = sellRatio;
        }

        /// <summary>Bir eşyanın satış değeri (fiyatın oranı, aşağı yuvarlanır).</summary>
        public int SellValue(ItemDef def) => (int)Math.Floor(def.Price * SellRatio);

        public PlacementCheck Buy(string itemId)
        {
            if (!_content.Items.TryGet(itemId, out var def)) return PlacementCheck.Fail("Böyle bir eşya yok.");
            if (def.Price <= 0) return PlacementCheck.Fail("Bu eşya satılmıyor.");
            if (!IsUnlocked(def)) return PlacementCheck.Fail($"Hesap seviyesi {def.UnlockLevel} gerekli.");
            if (Storage != null && !Storage.HasRoomFor(itemId)) return PlacementCheck.Fail("Depo dolu: depo bölgesi çiz ya da sandık koy.");
            int paid = PriceFor(def);
            if (!_house.TrySpend(paid)) return PlacementCheck.Fail("Akçe yetmiyor.");
            _house.AddItem(itemId);
            Record("Satın alma", () =>
            {
                if (!_house.TryTakeItem(itemId)) return PlacementCheck.Fail("Eşya artık envanterde değil.");
                _house.AddAkce(paid);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>Envanterdeki bir eşyayı satar (önce sağlam olanı).</summary>
        public PlacementCheck SellFromInventory(string itemId)
        {
            if (!_content.Items.TryGet(itemId, out var def)) return PlacementCheck.Fail("Böyle bir eşya yok.");
            if (!_house.TryTakeItem(itemId, out var cond)) return PlacementCheck.Fail("Envanterde yok.");
            return FinishInventorySale(itemId, def, cond);
        }

        /// <summary>Envanterden belirli durumdaki (hasarlı, onarılmış) bir adedi satar; değeri durumla düşer.</summary>
        public PlacementCheck SellFromInventory(string itemId, ItemCondition condition)
        {
            if (!_content.Items.TryGet(itemId, out var def)) return PlacementCheck.Fail("Böyle bir eşya yok.");
            if (!_house.TakeCondition(itemId, condition)) return PlacementCheck.Fail($"Envanterde {Decor.ConditionName(condition).ToLowerInvariant()} {def.DisplayName} yok.");
            return FinishInventorySale(itemId, def, condition);
        }

        private PlacementCheck FinishInventorySale(string itemId, ItemDef def, ItemCondition cond)
        {
            int value = SellValue(def, cond);
            _house.AddAkce(value);
            Record("Satış", () =>
            {
                if (!_house.TrySpend(value)) return PlacementCheck.Fail("Akçe yetmiyor, satış geri alınamaz.");
                _house.AddItem(itemId, 1, cond);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>Yerleştirilmiş eşyayı (ve üstündeki dekorları) satar.</summary>
        private const string PlantLocked = "Bitkiler taşınmaz ya da satılmaz; olgunlaşınca kesilir.";

        public PlacementCheck SellPlaced(int itemId)
        {
            var target = _world.GetItem(itemId);
            if (target == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            if (IsLocked(target)) return PlacementCheck.Fail(Locked);
            // Ekin sökülebilir (akçe vermez, geri alınmaz): yanlış yere ekilen ya da ortada kalıp ulaşılamayan ekin
            // tarlayı kilitlemesin. Ağaçlar olgunlaşınca kesilerek kalkar.
            if (target.IsPlant && target.Def.IsCrop)
            {
                _world.RemoveItem(itemId);
                return PlacementCheck.Success;
            }
            if (target.IsPlant) return PlacementCheck.Fail(PlantLocked);
            string blocker = RemovalBlocker(target);
            if (blocker != null) return PlacementCheck.Fail(blocker);
            PlacedItemSold?.Invoke(target);
            var spot = SpotOf(target);
            int total = FinishSellBonus(target);
            var removedItems = _world.RemoveItemInstances(itemId);
            var removed = new List<ItemDef>();
            foreach (var it in removedItems) { removed.Add(it.Def); total += SellValue(it); }
            if (total > 0) _house.AddAkce(total);
            // Geri kazanım: sökülen eşyadan malzemenin bir kısmı geri döner (akçeye ek).
            var salvage = new List<KeyValuePair<string, int>>();
            int salvageLevel = SalvageLevel?.Invoke() ?? 0;
            if (salvageLevel > 0) foreach (var def in removed) salvage.AddRange(SalvageOf(def, salvageLevel));
            foreach (var kv in salvage) _house.AddItem(kv.Key, kv.Value);
            if (salvage.Count > 0) Salvaged?.Invoke(salvage);
            Record("Eşya satışı", () =>
            {
                if (!CanPutBack(spot).Ok) return PlacementCheck.Fail("Eski yeri doldu, satış geri alınamaz.");
                foreach (var kv in salvage) if (_house.Count(kv.Key) < kv.Value) return PlacementCheck.Fail("Kurtarılan malzeme harcandı, geri alınamaz.");
                if (!_house.TrySpend(total)) return PlacementCheck.Fail("Akçe yetmiyor, satış geri alınamaz.");
                foreach (var kv in salvage) for (int i = 0; i < kv.Value; i++) _house.TryTakeItem(kv.Key);
                PutBack(spot);
                GiveBack(removedItems, 1);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        // ---- geri kazanım ----

        /// <summary>Hanenin en iyi geri kazanım seviyesi; null ise sökülen eşyadan malzeme dönmez.</summary>
        public Func<int> SalvageLevel { get; set; }

        /// <summary>Eşya satılırken malzeme kurtarıldı (kimlik, adet).</summary>
        public event Action<List<KeyValuePair<string, int>>> Salvaged;

        /// <summary>Geri kazanım oranı: 1. seviye %10, seviye başına +%5 (10. seviyede %55).</summary>
        public static double SalvageFraction(int level) => level <= 0 ? 0 : 0.10 + 0.05 * (level - 1);

        /// <summary>
        /// Bir eşyanın sökülünce verdiği malzeme: tarifle yapılan eşyada tarifin malzemesinden, satın alınan ahşap/metal/
        /// kumaş/kil eşyada fiyatına göre tahta/çivi/iplik/kil. Oran geri kazanım seviyesiyle artar.
        /// </summary>
        public List<KeyValuePair<string, int>> SalvageOf(ItemDef def, int level)
        {
            var result = new List<KeyValuePair<string, int>>();
            double f = SalvageFraction(level);
            if (f <= 0 || def == null) return result;
            InteractionDef recipe = null;
            foreach (var station in _content.Items.All)
            {
                foreach (var i in station.Interactions)
                    if (i.IsCraft && i.Produce.Count > 0 && i.Produce[0].ItemId == def.Id) { recipe = i; break; }
                if (recipe != null) break;
            }
            if (recipe != null)
            {
                foreach (var c in recipe.AllConsumed())
                {
                    int n = (int)Math.Round(c.Count * f, MidpointRounding.AwayFromZero);
                    if (n > 0) result.Add(new KeyValuePair<string, int>(c.ItemId, n));
                }
                return result;
            }
            string material = def.FinishGroup == "wood" ? "item.plank" : def.FinishGroup == "metal" ? "item.nails"
                : def.FinishGroup == "cloth" ? "item.yarn" : def.FinishGroup == "clay" ? "item.clay" : null;
            if (material == null || !_content.Items.Contains(material)) return result;
            int units = (int)Math.Round(Math.Max(1, def.Price / 10) * f, MidpointRounding.AwayFromZero);
            if (units > 0) result.Add(new KeyValuePair<string, int>(material, units));
            return result;
        }

        private PlacementCheck CheckOwned(ItemDef def, GridPos position, int rotation)
        {
            var (w, h) = Footprint.Dimensions(def.Width, def.Height, rotation);
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    if (!_world.Map.IsOwned(new GridPos(position.X + dx, position.Y + dy, position.Z)))
                        return PlacementCheck.Fail(NotOwned);
            return PlacementCheck.Success;
        }

        public const string NotOwned = "Burası senin arsan değil.";

        public PlacementCheck CanPlaceFromInventory(string itemId, GridPos position, int rotation)
        {
            if (_house.Count(itemId) <= 0) return PlacementCheck.Fail("Envanterde yok.");
            var def = _content.Items.Get(itemId);
            if (def.Placement == ItemPlacement.Wall) return PlacementCheck.Fail("Bu eşya duvara asılır.");
            if (def.IsCeiling)
            {
                if (!_world.Map.IsOwned(position)) return PlacementCheck.Fail(NotOwned);
                return _world.CanPlaceCeiling(def, position);
            }
            var owned = CheckOwned(def, position, rotation);
            if (!owned.Ok) return owned;
            if (def.IsRug) return _world.CanPlaceRug(def, position, rotation);
            var floor = _world.CanPlaceFloor(def, position, rotation, null, landingMayBeCreated: def.IsStairs);
            if (!floor.Ok || !def.IsStairs) return floor;
            var well = StairwellBlocker(def, position, rotation);
            if (well != null) return PlacementCheck.Fail(well);

            // Merdivenin öbür ucunda karo yoksa (üst kat döşemesi / bodrum kazısı) onu da kurabilmeliyiz.
            WorldState.StairsEnds(def, position, rotation, out _, out var landing);
            if (_world.Map.HasTile(landing)) return floor;
            var create = CanBuildLevelTile(landing, stairEnd: true);
            if (!create.Ok) return create;
            return _house.Akce >= TilePrice(landing.Z) ? PlacementCheck.Success : PlacementCheck.Fail($"Merdivenin ucunu kurmak için {TilePrice(landing.Z)} akçe gerekir.");
        }

        public PlacementCheck PlaceFromInventory(string itemId, GridPos position, int rotation, out ItemInstance placed)
        {
            placed = null;
            var check = CanPlaceFromInventory(itemId, position, rotation);
            if (!check.Ok) return check;
            var def = _content.Items.Get(itemId);
            bool grouped = false;
            if (def.IsStairs)
            {
                WorldState.StairsEnds(def, position, rotation, out _, out var landing);
                if (!_world.Map.HasTile(landing))
                {
                    // Dıştaki bir grubun içindeyse (oda şablonu) yeni grup açılmaz; açmadığı grubu da kapatmaz.
                    grouped = _group == null;
                    if (grouped) BeginGroup("Merdiven");
                    var built = BuildLevelTile(landing, stairEnd: true);
                    if (!built.Ok) { if (grouped) EndGroup(); return built; }
                }
            }
            if (grouped == false && def.IsStairs && def.StairsDelta > 0 && StairwellTiles(def, position, rotation).Count > 0 && _group == null)
            {
                grouped = true;
                BeginGroup("Merdiven");
            }
            CutStairwell(def, position, rotation);
            _house.TryTakeItem(itemId, out var cond);
            var placedItem = _world.PlaceItem(def, position, rotation);
            placedItem.Condition = cond;
            placed = placedItem;
            int placedId = placedItem.Id;
            Record("Yerleştirme", () =>
            {
                var current = _world.GetItem(placedId);
                if (current == null) return PlacementCheck.Fail("Eşya artık orada değil.");
                string why = RemovalBlocker(current);      // bu arada hayvan alınmış barınak, katta biri kalmış merdiven
                if (why != null) return PlacementCheck.Fail(why);
                GiveBack(_world.RemoveItemInstances(placedId));
                return PlacementCheck.Success;
            });
            if (grouped) EndGroup();
            if (!def.IsPlant) Furnished?.Invoke(placedItem);
            return PlacementCheck.Success;
        }

        /// <summary>Envanterden bir eşya yerleştirildi (ev düzenleme XP'si için; bitkiler hariç).</summary>
        public event Action<ItemInstance> Furnished;

        /// <summary>Yerleştirilmiş eşya satılmadan hemen önce (yadigâr geçmişi silinir).</summary>
        public event Action<ItemInstance> PlacedItemSold;

        /// <summary>Envantere almayı engelleyen ek kural (yadigâr envantere girmez); null ise yok.</summary>
        public Func<ItemInstance, string> StoreBlocker { get; set; }

        /// <summary>Duvar eşyasının karonun bir kenarına asılıp asılamayacağı (envanter, arsa, duvar).</summary>
        public PlacementCheck CanPlaceWallFromInventory(string itemId, GridPos tile, int side, int band = -1)
        {
            if (_house.Count(itemId) <= 0) return PlacementCheck.Fail("Envanterde yok.");
            var def = _content.Items.Get(itemId);
            foreach (var (t, _) in WallSides.Segments(tile, Footprint.Normalize(side), Math.Max(1, def.WallWidth)))
                if (!_world.Map.IsOwned(t)) return PlacementCheck.Fail(NotOwned);
            return _world.CanPlaceOnWall(def, tile, side, band, null);
        }

        /// <summary>Duvar eşyasını envanterden asar. band &lt; 0: eşyanın ilk boş bandı (alt, orta, üst).</summary>
        public PlacementCheck PlaceWallFromInventory(string itemId, GridPos tile, int side, out ItemInstance placed, int band = -1)
        {
            placed = null;
            var check = CanPlaceWallFromInventory(itemId, tile, side, band);
            if (!check.Ok) return check;
            _house.TryTakeItem(itemId, out var cond);
            var wallItem = _world.PlaceOnWall(_content.Items.Get(itemId), tile, side, band);
            wallItem.Condition = cond;
            placed = wallItem;
            int wallId = wallItem.Id;
            Record("Duvara asma", () =>
            {
                if (_world.GetItem(wallId) == null) return PlacementCheck.Fail("Eşya artık orada değil.");
                GiveBack(_world.RemoveItemInstances(wallId));
                return PlacementCheck.Success;
            });
            Furnished?.Invoke(wallItem);
            return PlacementCheck.Success;
        }

        public PlacementCheck PlaceDecorFromInventory(string itemId, int surfaceItemId, string slotId, out ItemInstance placed)
        {
            placed = null;
            if (_house.Count(itemId) <= 0) return PlacementCheck.Fail("Envanterde yok.");
            var def = _content.Items.Get(itemId);
            var check = _world.CanPlaceOnSurface(def, surfaceItemId, slotId);
            if (!check.Ok) return check;
            _house.TryTakeItem(itemId, out var cond);
            var decor = _world.PlaceOnSurface(def, surfaceItemId, slotId);
            decor.Condition = cond;
            placed = decor;
            int decorId = decor.Id;
            Record("Dekor", () =>
            {
                if (_world.GetItem(decorId) == null) return PlacementCheck.Fail("Dekor artık orada değil.");
                GiveBack(_world.RemoveItemInstances(decorId));
                return PlacementCheck.Success;
            });
            Furnished?.Invoke(decor);
            return PlacementCheck.Success;
        }

        public PlacementCheck Move(int itemId, GridPos position, int rotation)
        {
            var item = _world.GetItem(itemId);
            if (item != null && item.IsPlant) return PlacementCheck.Fail(PlantLocked);
            if (IsLocked(item)) return PlacementCheck.Fail(Locked);
            if (item != null && (item.IsWallMounted || item.IsCeiling))
            {
                if (!_world.Map.IsOwned(position)) return PlacementCheck.Fail(NotOwned);
            }
            else if (item != null)
            {
                var owned = CheckOwned(item.Def, position, rotation);
                if (!owned.Ok) return owned;
            }
            var oldPos = item != null ? item.Position : position;
            int oldRot = item != null ? item.Rotation : rotation;
            var moved = _world.MoveItem(itemId, position, rotation);
            // Merdiven taşınınca üst/alt katta duran biri yeni merdivene yürüyemiyorsa taşıma geri çevrilir.
            if (moved.Ok && item != null && item.Def.IsStairs && !item.IsDecor && AnyoneCutOff())
            {
                _world.MoveItem(itemId, oldPos, oldRot);
                return PlacementCheck.Fail(StairsCutOff);
            }
            if (moved.Ok && item != null)
                Record("Taşıma", () => _world.GetItem(itemId) == null ? PlacementCheck.Fail("Eşya artık yok.") : _world.MoveItem(itemId, oldPos, oldRot));
            return moved;
        }

        /// <summary>Duvar eşyasını aynı duvarda başka banda (alt, orta, üst) taşır; geri alınabilir.</summary>
        public PlacementCheck SetWallBand(int itemId, int band)
        {
            var item = _world.GetItem(itemId);
            if (item == null || !item.IsWallMounted) return PlacementCheck.Fail("Duvar eşyası bulunamadı.");
            if (IsLocked(item)) return PlacementCheck.Fail(Locked);
            if (item.WallBand == band) return PlacementCheck.Fail($"Zaten {Decor.BandName(band)} bantta.");
            int old = item.WallBand;
            var r = _world.MoveWallItem(itemId, item.Position, item.Rotation, band);
            if (r.Ok) Record("Bant", () => _world.GetItem(itemId) == null ? PlacementCheck.Fail("Eşya artık yok.") : _world.MoveWallItem(itemId, item.Position, item.Rotation, old));
            return r;
        }

        /// <summary>Halının sırası: +2 en üste, +1 bir üste, -1 bir alta, -2 en alta (halılar üst üste serilir).</summary>
        public PlacementCheck MoveRugLayer(int itemId, int step)
        {
            var rug = _world.GetItem(itemId);
            if (rug == null || !rug.IsRug) return PlacementCheck.Fail("Halı bulunamadı.");
            int old = rug.LayerOrder;
            bool ok = step >= 2 ? _world.RugToFront(itemId) : step <= -2 ? _world.RugToBack(itemId) : _world.RugStep(itemId, step);
            if (!ok) return PlacementCheck.Fail(step > 0 ? "Üstünde başka halı yok." : "Altında başka halı yok.");
            Record("Halı sırası", () =>
            {
                var r = _world.GetItem(itemId);
                if (r == null) return PlacementCheck.Fail("Halı artık yok.");
                _world.RestoreDecorState(r, -1, old, r.Condition, r.TarnishDays);
                _world.NotifyChanged();
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>Eşyayı (ve dekorlarını) envantere geri alır.</summary>
        public PlacementCheck StoreToInventory(int itemId)
        {
            var target = _world.GetItem(itemId);
            if (target == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            if (target.IsPlant) return PlacementCheck.Fail(PlantLocked);
            if (IsLocked(target)) return PlacementCheck.Fail(Locked);
            string blocker = RemovalBlocker(target) ?? StoreBlocker?.Invoke(target);
            if (blocker != null) return PlacementCheck.Fail(blocker);
            var spot = SpotOf(target);
            // Envanter eşya kimliğiyle sayar: kaplama (renk/malzeme) envantere gitmez, farkının satış oranı iade edilir.
            int finishRefund = FinishSellBonus(target);
            var removed = _world.RemoveItemInstances(itemId);
            GiveBack(removed);
            if (finishRefund > 0) _house.AddAkce(finishRefund);
            Record("Envantere alma", () =>
            {
                if (!CanPutBack(spot).Ok) return PlacementCheck.Fail("Eski yeri doldu.");
                if (finishRefund > 0 && _house.Akce < finishRefund) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz.");
                if (!_house.TakeCondition(spot.Def.Id, spot.Condition) && !_house.TryTakeItem(spot.Def.Id)) return PlacementCheck.Fail("Eşya artık envanterde değil.");
                if (finishRefund > 0) _house.TrySpend(finishRefund);
                PutBack(spot);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        // ---- duvar, kapı, pencere, zemin (envantersiz, doğrudan akçe) ----

        /// <summary>Malzeme kataloğu olmayan içerikte (testler) kenar fiyatları; katalogdaki varsayılan parçalar aynı fiyatlıdır.</summary>
        public const int WallPrice = 3, DoorPrice = 8, WindowPrice = 6, HalfWallPrice = 2, FencePrice = 2, GatePrice = 6, RailingPrice = 3;

        public static int EdgePrice(EdgeKind kind)
        {
            switch (kind)
            {
                case EdgeKind.Wall: return WallPrice;
                case EdgeKind.Door: return DoorPrice;
                case EdgeKind.Window: return WindowPrice;
                case EdgeKind.HalfWall: return HalfWallPrice;
                case EdgeKind.Fence: return FencePrice;
                case EdgeKind.Gate: return GatePrice;
                case EdgeKind.Railing: return RailingPrice;
                default: return 0;
            }
        }

        public static int FloorPrice(FloorType floor)
        {
            switch (floor)
            {
                case FloorType.Dirt: return 1;
                case FloorType.Wood: return 2;
                case FloorType.Stone: return 3;
                case FloorType.Water: return 6;     // gölet kazmak
                default: return 0;
            }
        }

        private int Refund(int price) => (int)Math.Floor(price * SellRatio);

        // ---- katlar: üst kat döşemesi ve bodrum kazısı ----

        public const int SlabPrice = 4, DigPrice = 5;

        /// <summary>Üst kat döşemesi ya da bodrum kazısının karo başına akçesi.</summary>
        public static int TilePrice(int level) => level > 0 ? SlabPrice : DigPrice;

        private static readonly GridPos[] FourDirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        /// <summary>Üst kat döşemesi, altındaki karo kapalı bir odanın içindeyse ya da altında duvar/kapı/pencere varsa taşınır.</summary>
        private bool IsSupported(GridPos upper)
        {
            var map = _world.Map;
            var below = upper.OnLevel(upper.Z - 1);
            if (!map.HasTile(below)) return false;
            var room = map.Rooms.RoomAt(below);
            if (room != null && room.IsIndoor) return true;
            foreach (var d in FourDirs)
                if (EdgeKinds.IsFullHeight(map.GetEdge(GridMap.EdgeBetween(below, below + d)))) return true;
            return false;
        }

        // Balkon: desteksiz üst kat karosu, desteklenen bir üst kat karosunun hemen yanında (bir karo çıkma) olabilir.
        private bool IsBalconySpot(GridPos upper)
        {
            var map = _world.Map;
            foreach (var d in FourDirs)
            {
                var n = upper + d;
                if (map.HasTile(n) && IsSupported(n)) return true;
            }
            return false;
        }

        /// <summary>Verilen karoda üst kat döşemesi (Z &gt; 0) ya da bodrum kazısı (Z &lt; 0) yapılabilir mi.</summary>
        /// <summary>Kat -2'ye inen kuyunun engeli (Madencilik seviyesi); null ise serbest.</summary>
        public Func<GridPos, string> DeepDigBlocker { get; set; }
        /// <summary>Merdivenle kat -2'de kuyu inişi açıldı (kazı motoru karoyu açık sayar).</summary>
        public event Action<GridPos> DeepShaftOpened;
        /// <summary>Korunan eşya (hazine odasındaki yadigâr): balyoz ve yıkım kaldırmaz.</summary>
        public Func<ItemInstance, bool> ProtectedItem { get; set; }

        public PlacementCheck CanBuildLevelTile(GridPos p, bool stairEnd = false)
        {
            var map = _world.Map;
            if (p.Z == 0) return PlacementCheck.Fail("Zemin kat zaten var.");
            if (!map.InBounds(p)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(p)) return PlacementCheck.Fail(NotOwned);
            if (map.HasTile(p)) return PlacementCheck.Fail("Orası zaten yapılmış.");
            if (p.Z <= -2)
            {
                // Arsa kazısı: kat -2 yapı modunda akçeyle kazılmaz; bodrumdan inen merdivenle kuyu açılır, gerisi kazmayla.
                if (!stairEnd) return PlacementCheck.Fail("Bodrumun altı kazmayla kazılır (canlı mod, Madencilik 2).");
                var why = DeepDigBlocker?.Invoke(p);
                return why != null ? PlacementCheck.Fail(why) : PlacementCheck.Success;
            }
            if (p.Z > 0)
            {
                if (IsAboveStairs(p)) return PlacementCheck.Fail("Merdiven boşluğu: merdivenin üstüne döşeme yapılmaz.");
                if (!IsSupported(p) && !IsBalconySpot(p))
                    return PlacementCheck.Fail("Üst kat, duvarlı bir odanın üstüne kurulur (ya da bir karo çıkma: balkon).");
                return PlacementCheck.Success;
            }
            // Bodrum: mevcut bodrumun bitişiğinden kazılır; ilk karo merdivenin ucundan başlar.
            foreach (var d in FourDirs)
                if (map.HasTile(p + d)) return PlacementCheck.Success;
            return stairEnd ? PlacementCheck.Success : PlacementCheck.Fail("Bodrumu bir merdivenle başlat ya da mevcut bodrumun yanını kaz.");
        }

        /// <summary>Üst kat döşemesi yapar ya da bodrum karosu kazar (akçe harcar).</summary>
        public PlacementCheck BuildLevelTile(GridPos p, bool stairEnd = false)
        {
            // Merdiven ucu için ilk bodrum karosu komşusuz da kazılabilir.
            var check = CanBuildLevelTile(p, stairEnd);
            if (!check.Ok) return check;
            int price = TilePrice(p.Z);
            if (!_house.TrySpend(price)) return PlacementCheck.Fail("Akçe yetmiyor.");
            var map = _world.Map;
            map.SetTile(p, true);
            map.SetFloor(p, p.Z < 0 ? FloorType.Dirt : FloorType.Wood);
            if (p.Z <= -2) DeepShaftOpened?.Invoke(p);
            Record(p.Z > 0 ? "Üst kat döşemesi" : "Bodrum kazısı", () =>
            {
                if (_world.GetItemAt(p) != null) return PlacementCheck.Fail("Üzerinde eşya var, geri alınamaz.");
                if (_world.IsOccupiedByCharacter(p)) return PlacementCheck.Fail("Üzerinde biri duruyor, geri alınamaz.");
                map.SetTile(p, false);
                _house.AddAkce(price);      // geri alma tam iade eder (kaldırma yarısını verir)
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        /// <summary>Üst kat döşemesini kaldırır ya da bodrum karosunu doldurur; yarısı iade. Üstünde eşya, çevresinde duvar ya da merdiven bağı olmamalı.</summary>
        public PlacementCheck RemoveLevelTile(GridPos p)
        {
            var map = _world.Map;
            if (p.Z == 0 || !map.HasTile(p)) return PlacementCheck.Fail("Orada kaldırılacak bir şey yok.");
            if (p.Z <= -2) return PlacementCheck.Fail("Kazılan yer doldurulmaz.");
            if (!map.IsOwned(p)) return PlacementCheck.Fail(NotOwned);
            if (_world.GetItemAt(p) != null) return PlacementCheck.Fail("Önce üstündeki eşyayı kaldır.");
            if (_world.IsOccupiedByCharacter(p)) return PlacementCheck.Fail("Üzerinde biri duruyor.");
            if (map.HasLinkAt(p)) return PlacementCheck.Fail("Merdiven buraya bağlı; önce merdiveni kaldır.");
            foreach (var d in FourDirs)
                if (map.GetEdge(GridMap.EdgeBetween(p, p + d)) != EdgeKind.None) return PlacementCheck.Fail("Önce çevresindeki duvarları kaldır.");
            // Ayakta durulan karo: başka bir üst kat karosu bunun üstüne dayanıyorsa kaldırılamaz (şimdilik yalnızca MaxLevel=1, bodrumun üstü zemin).
            var oldFloor = map.GetFloor(p);
            int price = TilePrice(p.Z);
            int refund = Refund(price);
            map.SetTile(p, false);
            _house.AddAkce(refund);
            Record(p.Z > 0 ? "Döşeme kaldırma" : "Bodrum doldurma", () =>
            {
                if (!_house.TrySpend(refund)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz.");
                map.SetTile(p, true);
                map.SetFloor(p, oldFloor);
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        // ---- dikim ----

        /// <summary>
        /// Envanterdeki fidanı/tohumu dikiyor: kendi arsanda, dış mekânda (oda içinde değil), çim ya da
        /// toprak zeminli, boş karoya. Bitki büyümeye 0'dan başlar.
        /// </summary>
        public PlacementCheck Plant(string seedId, GridPos tile, out ItemInstance planted)
        {
            planted = null;
            if (!_content.Items.TryGet(seedId, out var seed) || seed.PlantsAs == null) return PlacementCheck.Fail("Bu dikilemez.");
            if (_house.Count(seedId) <= 0) return PlacementCheck.Fail("Envanterde yok.");
            var map = _world.Map;
            if (!map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(tile)) return PlacementCheck.Fail(NotOwned);
            var room = map.Rooms.RoomAt(tile);
            if (room != null && room.IsIndoor && !(Plants != null && Plants.IsGreenhouse(room)))
                return PlacementCheck.Fail("Ev içine dikilmez, dışarıda bir yer seç (ya da odaya pencere aç: sera).");
            var floor = map.GetFloor(tile);
            var plantDef = _content.Items.Get(seed.PlantsAs);
            if (plantDef.NeedsTilledSoil && floor != FloorType.Dirt) return PlacementCheck.Fail("Ekin yalnızca toprağa ekilir (yapı modu > Toprak).");
            if (floor != FloorType.Grass && floor != FloorType.Dirt) return PlacementCheck.Fail("Yalnızca çim ya da toprağa dikilir.");
            var check = _world.CanPlaceFloor(plantDef, tile, 0);
            if (!check.Ok) return check;

            _house.TryTakeItem(seedId);
            planted = _world.PlaceItem(plantDef, tile);
            return PlacementCheck.Success;
        }

        // ---- bölgeler ----

        /// <summary>Kendi arsandaki karoya bölge çizer ya da siler (ücretsiz).</summary>
        public PlacementCheck SetZone(GridPos tile, ZoneType zone)
        {
            var map = _world.Map;
            if (!map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            if (!map.IsOwned(tile)) return PlacementCheck.Fail(NotOwned);
            if (map.GetZone(tile) == zone) return PlacementCheck.Fail("Orası zaten öyle.");
            map.SetZone(tile, zone);
            return PlacementCheck.Success;
        }

        // ---- arsa ----

        public const int ChunkSize = 4;
        public const int ChunkPrice = 20;

        /// <summary>Verilen karonun ait olduğu 4x4 arsa parçasının sol-üst karosu.</summary>
        public static GridPos ChunkOrigin(GridPos tile) =>
            new GridPos(Math.Max(0, tile.X) / ChunkSize * ChunkSize, Math.Max(0, tile.Y) / ChunkSize * ChunkSize);

        /// <summary>Arsa genişletme: komşu (dik) 4x4 parçayı satın alır. Yalnızca sahip olunan alana bitişik parça alınabilir.</summary>
        public PlacementCheck BuyChunk(GridPos tile)
        {
            var map = _world.Map;
            if (!map.InBounds(tile)) return PlacementCheck.Fail("Harita dışı.");
            var o = ChunkOrigin(tile);
            if (map.IsOwned(o)) return PlacementCheck.Fail("Burası zaten senin.");

            bool adjacent = false;
            var dirs = new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };
            for (int dy = 0; dy < ChunkSize && !adjacent; dy++)
                for (int dx = 0; dx < ChunkSize && !adjacent; dx++)
                {
                    var t = new GridPos(o.X + dx, o.Y + dy);
                    if (!map.InBounds(t)) continue;
                    foreach (var d in dirs)
                    {
                        var n = t + d;
                        bool inside = n.X >= o.X && n.X < o.X + ChunkSize && n.Y >= o.Y && n.Y < o.Y + ChunkSize;
                        if (!inside && map.IsOwned(n)) { adjacent = true; break; }
                    }
                }
            if (!adjacent) return PlacementCheck.Fail("Önce komşu parçayı al.");
            if (!_house.TrySpend(ChunkPrice)) return PlacementCheck.Fail("Akçe yetmiyor.");

            for (int dy = 0; dy < ChunkSize; dy++)
                for (int dx = 0; dx < ChunkSize; dx++)
                {
                    var t = new GridPos(o.X + dx, o.Y + dy);
                    if (map.InBounds(t)) map.SetOwned(t, true);
                }
            return PlacementCheck.Success;
        }

        // ---- varyantlar (Adım 43) ----

        /// <summary>Yerleştirilmiş eşyanın aynı yerde seçilebilecek varyantları (aynı boyutlu, yer eşyası).</summary>
        public List<ItemDef> VariantsOf(ItemDef def)
        {
            var list = new List<ItemDef>();
            foreach (var id in def.Variants)
                if (_content.Items.TryGet(id, out var v) && v.Width == def.Width && v.Height == def.Height && v.Placement == def.Placement && IsUnlocked(v))
                    list.Add(v);
            return list;
        }

        /// <summary>
        /// Yerleştirilmiş eşyayı varyantıyla değiştirir (aynı yer ve yön). Fiyat farkı ödenir; ucuza geçişte satış oranı kadar iade edilir.
        /// Üstünde dekor varsa önce onlar alınmalı.
        /// </summary>
        public PlacementCheck SwapVariant(int itemId, string variantId, out ItemInstance replaced)
        {
            replaced = null;
            var item = _world.GetItem(itemId);
            if (item == null) return PlacementCheck.Fail("Eşya bulunamadı.");
            if (item.IsPlant) return PlacementCheck.Fail(PlantLocked);
            if (IsLocked(item)) return PlacementCheck.Fail(Locked);
            if (!item.Def.Variants.Contains(variantId)) return PlacementCheck.Fail("Bu eşyanın böyle bir varyantı yok.");
            if (!_content.Items.TryGet(variantId, out var variant)) return PlacementCheck.Fail("Varyant bulunamadı.");
            if (!IsUnlocked(variant)) return PlacementCheck.Fail($"Hesap seviyesi {variant.UnlockLevel} gerekli.");
            foreach (var other in _world.Items)
                if (other.ParentId == itemId) return PlacementCheck.Fail("Önce üstündeki dekoru al.");

            if (IsOccupiedHousing(item)) return PlacementCheck.Fail(HousingOccupied);
            // Eski eşya kaldırılmadan önce yenisinin aynı yere sığacağından emin ol (yoksa eşya ve akçe boşa giderdi).
            var oldDef = item.Def;
            var oldSpot = SpotOf(item);
            var newSpot = SpotOf(item, variant);
            var fits = CanPutBack(newSpot, itemId);
            if (!fits.Ok) return fits;
            int diff = variant.Price - oldDef.Price;
            int delta = diff > 0 ? -diff : (int)Math.Floor(-diff * SellRatio);   // eksi = ödedi, artı = iade aldı
            if (diff > 0 && !_house.TrySpend(diff)) return PlacementCheck.Fail("Akçe yetmiyor.");
            if (diff < 0 && delta > 0) _house.AddAkce(delta);

            _world.RemoveItem(itemId);
            var created = PutBack(newSpot);
            CarryState(item, created);      // ocağın yakıtı, ahırın yemi ve biriken ürünü varyantla birlikte kalır
            replaced = created;
            int newId = created.Id;
            Record("Varyant", () =>
            {
                var cur = _world.GetItem(newId);
                if (cur == null) return PlacementCheck.Fail("Eşya artık yok.");
                if (delta > 0) { if (!_house.TrySpend(delta)) return PlacementCheck.Fail("Akçe yetmiyor, geri alınamaz."); }
                else if (delta < 0) _house.AddAkce(-delta);
                _world.RemoveItem(newId);
                CarryState(cur, PutBack(oldSpot));
                return PlacementCheck.Success;
            });
            return PlacementCheck.Success;
        }

        // Varyant değişiminde eşyanın üstünde biriken durum (yakıt, yem, ürün) yeni eşyaya geçer.
        private static void CarryState(ItemInstance from, ItemInstance to)
        {
            to.FuelDays = from.FuelDays;
            to.FeedStock = from.FeedStock;
            foreach (var kv in new List<KeyValuePair<string, int>>(from.Stock))
                if (kv.Value > 0) to.SetStock(kv.Key, kv.Value);
        }

        /// <summary>Mağazada satılan eşyalar (fiyatı olanlar), kategoriye ve ada göre.</summary>
        public List<ItemDef> ShopList()
        {
            var list = new List<ItemDef>();
            // Kaynaklar (odun, şalgam...) mağazada değil pazarda alınıp satılır.
            foreach (var d in _content.Items.All) if (d.Price > 0 && d.Placement != ItemPlacement.Resource) list.Add(d);
            list.Sort((a, b) =>
            {
                int c = string.CompareOrdinal(a.Category, b.Category);
                if (c != 0) return c;
                c = a.UnlockLevel.CompareTo(b.UnlockLevel);
                return c != 0 ? c : string.CompareOrdinal(a.DisplayName, b.DisplayName);
            });
            return list;
        }
    }
}
