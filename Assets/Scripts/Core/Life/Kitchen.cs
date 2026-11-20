using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Bir pişirme işinde kullanılan kap ve aletler (63a §5.2).</summary>
    public sealed class KitchenUse
    {
        /// <summary>Seçilen kap; kap istemeyen yöntemde null.</summary>
        public ItemDef Vessel;
        public int Tier;
        /// <summary>Ekipman puanı, 0-10.</summary>
        public int Score;
        /// <summary>Sofra puanı tavanı: 70, 85 ya da 100.</summary>
        public int Cap;
        /// <summary>Bu işte kirlenecek eşyalar (kap ve el aletleri).</summary>
        public readonly List<string> Used = new List<string>();
        /// <summary>Evde olmayan yardımcı aletler (iş yavaşlar, puan düşer).</summary>
        public readonly List<string> MissingHelpers = new List<string>();
        public bool UsesKnife;
    }

    /// <summary>
    /// Ekipman puanı ve kalite tavanı (Project belgesi 63a §5.2). Kabın tek bir kademe sayısı vardır; aynı sayı hem puana
    /// hem tavana gider. Kabın kendi kalitesi ve temizliği yalnız puana gider, tavana dokunmaz.
    /// </summary>
    public static class KitchenQuality
    {
        public const int FineAt = 50, SuperiorAt = 75, MasterworkAt = 90;
        public const int DirtyPenalty = 2, DullPenalty = 1, HelperPoints = 2, MaxScore = 10;

        /// <summary>Tavan = 55 + 15 × kademe (70 / 85 / 100); kap yoksa 100.</summary>
        public static int Cap(int tier) => tier <= 0 ? 100 : Math.Min(100, 55 + 15 * tier);

        /// <summary>
        /// 2 × kademe + kabın kalite basamağı (0-2) + yardımcı aletler (0-2, oransal) − durum (kirli kap 2, kör bıçak 1)
        /// + sabunla yıkanmış takım 1; 0-10 arası.
        /// </summary>
        public static int EquipmentScore(int tier, int vesselQuality, int helpersHave, int helpersNeeded, bool dirtyVessel, bool dullKnife, bool sparkling)
        {
            int score = 2 * Math.Max(0, tier) + Math.Max(0, Math.Min(ItemQuality.NormalMax, vesselQuality));
            score += helpersNeeded <= 0 ? HelperPoints : (int)Math.Round(HelperPoints * (double)helpersHave / helpersNeeded, MidpointRounding.AwayFromZero);
            if (dirtyVessel) score -= DirtyPenalty;
            if (dullKnife) score -= DullPenalty;
            if (sparkling) score += 1;
            return Math.Max(0, Math.Min(MaxScore, score));
        }

        /// <summary>Tavanın izin verdiği en iyi basamak: 70 → Seçme, 85 → Has, 100 → ustalık eseri ve yadigâr da olur.</summary>
        public static int MaxQuality(int cap) => cap < SuperiorAt ? ItemQuality.Fine : cap < MasterworkAt ? ItemQuality.Superior : ItemQuality.Max;

        /// <summary>
        /// Ekipman puanının 53 §4.4 kalite kuralına ("kapının üstü" seviyesi) etkisi: yemek sınıfı olmayan pişirme işlerinde
        /// (reçel, pekmez, peynir, çay) puan, usta şansını belirleyen seviyeyi kaydırır. Yemeklerde puan sofra puanının
        /// ekipman bileşenidir (58 §6; <see cref="KitchenSystem.EquipmentPointsFor"/>), seviye kaydırılmaz.
        /// </summary>
        public static int LevelShift(int score) => score <= 1 ? -2 : score <= 3 ? -1 : score <= 6 ? 0 : score <= 8 ? 1 : 2;
    }

    [Serializable]
    public sealed class KitchenSave
    {
        /// <summary>1 = bu kurallarla yazıldı. JsonUtility eksik alanı boş örnekle doldurduğu için eski kayıt 0 okunur.</summary>
        public int version;
        public string[] dirty;        // "eşya:adet"
        public int dirtyTableware;
        public double dirtySince = -1;
        public double sparkleUntil;
        public int knifeUses;
    }

    /// <summary>
    /// Mutfak aletleri (Project belgeleri 63, 63a): "gerekenler" modeli, aletleri alıp getirme, yakıt, bulaşık ve
    /// ekipman puanı. Kurallar yalnızca oyuncunun hanesine uygulanır; köylüler muaftır.
    /// </summary>
    public sealed class KitchenSystem
    {
        public const string FirewoodId = "item.firewood";
        public const string BasketId = "item.dish_basket";
        public const string DirtyPlateMood = "mood.dirty_plate", ColdHandsMood = "mood.cold_hands";
        public static readonly string[] SoapIds = { "item.soap", "item.soap_olive", "item.bay_soap" };
        public static readonly string[] WaterIds = { "item.kitchen_spout", "item.water_barrel", "item.water_bucket", "item.rain_barrel", "item.well" };

        /// <summary>Bıçak bu kadar doğramadan sonra körelir (bileği taşında bilenir).</summary>
        public const int KnifeDullAfter = 30;
        /// <summary>Yardımcı alet eksikse iş bu oranda uzar (alet başına).</summary>
        public const double MissingHelperSlow = 0.15;
        /// <summary>Bulaşık: parça başına süre payı ve tavanı; sabunla çarpan.</summary>
        public const double WashPerPiece = 0.15;
        public const int WashPieceCap = 20;
        public const double SoapSpeed = 0.8;
        /// <summary>Kıyıdaki yıkama taşının suya en çok uzaklığı (karo).</summary>
        public const int StoneWaterRadius = 2;
        public const float ColdHandsBelow = 5f;
        /// <summary>Ev görevlisi bu kadar parça birikince ya da bulaşık bu kadar gün bekleyince yıkar.</summary>
        public const int DutyWashPieces = 5;
        public const double DutyWashAfterDays = 0.5;

        private readonly WorldState _world;
        private readonly Household _house;
        private readonly ContentDatabase _content;

        /// <summary>Kurallar açık mı. Kapalıyken hiçbir şey istenmez (eski davranış).</summary>
        public bool Enforce { get; set; } = true;
        public Func<double> Now { get; set; }
        public Func<GridPos, float> TemperatureAt { get; set; }
        /// <summary>Ocağa bir kütük atar (HeatSystem.TryStoke); yoksa kütük doğrudan hane envanterinden alınır.</summary>
        public Func<ItemInstance, bool> Stoke { get; set; }
        /// <summary>Bir kütüğün yanma süresi (oyun günü; HeatConfig.BurnDaysPerLog). Yakıt payları kütük cinsinden yazılır.</summary>
        public double DaysPerLog { get; set; } = 0.5;
        public event Action<Character, LocText> Notice;
        /// <summary>Karakter pişirmek için ocağa odun attı (sönüğü tutuşturdu ya da besledi): "Ocağı yak" rehber adımı.</summary>
        public event Action<Character, ItemInstance> Stoked;
        /// <summary>Anahtarlı metni oyuncunun diline çevirir (LifeSystems.Text); yoksa anahtar döner (testler).</summary>
        public Func<LocText, string> Text { get; set; } = t => t.Literal ?? t.Key ?? "";

        private readonly Dictionary<string, int> _dirty = new Dictionary<string, int>(StringComparer.Ordinal);
        private int _dirtyTableware;
        private double _dirtySince = -1;
        private double _sparkleUntil;
        private int _knifeUses;

        // Geçici durum (kayda girmez).
        private readonly Dictionary<string, int> _inUse = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<int, List<string>> _reserved = new Dictionary<int, List<string>>();
        // Karakterin sürmekte olan pişirme işinin seçimi; ocak ve iş kimliğiyle eşlenir (başka işe karışmasın).
        private readonly Dictionary<int, (int station, string interaction, KitchenUse use)> _lastUse = new Dictionary<int, (int, string, KitchenUse)>();

        private KitchenUse UseFor(Character c, ItemInstance station, InteractionDef i)
        {
            if (station != null && _lastUse.TryGetValue(c.Id, out var e) && e.station == station.Id && e.interaction == i.Id) return e.use;
            return Resolve(c, station, i, out _);
        }
        private readonly Dictionary<string, int> _placed = new Dictionary<string, int>(StringComparer.Ordinal);
        private int _placedVersion = -1;

        private readonly List<ItemDef> _vessels = new List<ItemDef>();
        private readonly Dictionary<string, List<ItemDef>> _tools = new Dictionary<string, List<ItemDef>>(StringComparer.Ordinal);
        private readonly List<ItemDef> _tableware = new List<ItemDef>();

        public KitchenSystem(WorldState world, Household house, ContentDatabase content)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            foreach (var d in content.Items.All)
            {
                if (d.IsVessel) _vessels.Add(d);
                else if (d.IsKitchenTool)
                {
                    if (!_tools.TryGetValue(d.ToolKind, out var list)) _tools[d.ToolKind] = list = new List<ItemDef>();
                    list.Add(d);
                }
                else if (d.Equip == Equip.Tableware) _tableware.Add(d);
            }
        }

        private double Today => Now != null ? Now() : 0;
        private static bool Exempt(Character c) => c == null || c.IsVillager;

        // ---- sayım ----

        /// <summary>Hanenin elindeki adet: envanter + arsada yerleştirilmiş olanlar.</summary>
        public int Owned(string defId) => _house.Count(defId) + Placed(defId);

        private int Placed(string defId)
        {
            if (_placedVersion != _world.ItemsVersion)
            {
                _placed.Clear();
                foreach (var it in _world.Items)
                {
                    if (it.Def.Equip == null || !OnPlot(it)) continue;
                    _placed[it.Def.Id] = (_placed.TryGetValue(it.Def.Id, out int n) ? n : 0) + 1;
                }
                _placedVersion = _world.ItemsVersion;
            }
            return _placed.TryGetValue(defId, out int count) ? count : 0;
        }

        private bool OnPlot(ItemInstance it)
        {
            var at = it.IsDecor && it.ParentId.HasValue ? (_world.GetItem(it.ParentId.Value)?.Position ?? it.Position) : it.Position;
            return _world.Map.IsOwned(at);
        }

        private int Free(string defId, Character c)
        {
            int busy = _inUse.TryGetValue(defId, out int n) ? n : 0;
            if (c != null && _reserved.TryGetValue(c.Id, out var mine)) foreach (var id in mine) if (id == defId) busy--;
            return Owned(defId) - Math.Max(0, busy);
        }

        public int DirtyCount(string defId) => _dirty.TryGetValue(defId, out int n) ? n : 0;
        /// <summary>Yıkanmayı bekleyen parça: kirli kaplar, aletler ve sofra takımı.</summary>
        public int DirtyPieces { get { int n = _dirtyTableware; foreach (var kv in _dirty) n += kv.Value; return n; } }
        public int DirtyTableware => _dirtyTableware;
        public bool KnifeDull => _knifeUses >= KnifeDullAfter;
        public bool Sparkling => Today < _sparkleUntil;
        /// <summary>En eski kirlinin beklediği süre (gün); kirli yoksa 0.</summary>
        public double DirtyAgeDays => DirtyPieces > 0 && _dirtySince >= 0 ? Math.Max(0, Today - _dirtySince) : 0;

        public int TablewareOwned { get { int n = 0; foreach (var d in _tableware) n += Owned(d.Id); return n; } }
        /// <summary>Masalarda duran, henüz toplanmamış kirli tabak (sofra: MealService.PlatesOnTables).</summary>
        public Func<int> PlatesOnTables { get; set; }
        /// <summary>Temiz sofra takımı: ne bulaşıkta ne masada bekleyen.</summary>
        public int CleanTableware => TablewareOwned - _dirtyTableware - (PlatesOnTables?.Invoke() ?? 0);

        // ---- gerekenler ----

        private static string VesselClass(InteractionDef i)
        {
            foreach (var n in i.NeedTools) if (n.StartsWith(Equip.VesselPrefix, StringComparison.Ordinal)) return n.Substring(Equip.VesselPrefix.Length);
            return CookMethods.DefaultVessel(i.Cook);
        }

        private static void ToolsFor(InteractionDef i, List<string> required, List<string> helpers)
        {
            foreach (var n in i.NeedTools)
            {
                if (!n.StartsWith(Equip.ToolPrefix, StringComparison.Ordinal)) continue;
                string t = n.Substring(Equip.ToolPrefix.Length);
                (Equip.IsRequiredTool(t) ? required : helpers).Add(t);
            }
            if (required.Contains(Equip.Knife) && !helpers.Contains(Equip.Board)) helpers.Add(Equip.Board);
            if ((i.Cook == CookMethods.Boil || i.Cook == CookMethods.Fry || i.Cook == CookMethods.Stew) && !helpers.Contains(Equip.Spoon)) helpers.Add(Equip.Spoon);
            if ((i.Cook == CookMethods.Boil || i.Cook == CookMethods.Stew) && i.Produce.Count > 0 && !helpers.Contains(Equip.Ladle)) helpers.Add(Equip.Ladle);
        }

        // Bu yöntemin en iyi boştaki kabı: önce puan (kademe, temizlik), eşitse kademe.
        private ItemDef BestVessel(string vesselClass, Character c, out bool busy)
        {
            busy = false;
            ItemDef best = null; int bestKey = int.MinValue;
            foreach (var d in _vessels)
            {
                if (d.VesselFor != vesselClass || Owned(d.Id) <= 0) continue;
                if (Free(d.Id, c) <= 0) { busy = true; continue; }
                int key = (2 * d.EquipTier - (AllDirty(d.Id) ? KitchenQuality.DirtyPenalty : 0)) * 10 + d.EquipTier;
                if (key > bestKey) { best = d; bestKey = key; }
            }
            return best;
        }

        private bool AllDirty(string defId) => DirtyCount(defId) >= Owned(defId) && Owned(defId) > 0;

        private ItemDef BestTool(string kind, Character c)
        {
            if (!_tools.TryGetValue(kind, out var list)) return null;
            ItemDef best = null;
            foreach (var d in list) if (Free(d.Id, c) > 0 && (best == null || d.Price > best.Price)) best = d;
            return best;
        }

        private bool OwnsTool(string kind)
        {
            if (!_tools.TryGetValue(kind, out var list)) return false;
            foreach (var d in list) if (Owned(d.Id) > 0) return true;
            return false;
        }

        /// <summary>Pişirmenin ocaktan yaktığı yakıt (oyun günü cinsinden): yöntem payı × kütük süresi.</summary>
        private double FuelNeed(ItemInstance station, InteractionDef i) =>
            station != null && station.Def.CookFire ? CookMethods.Fuel(i.Cook) * DaysPerLog : 0.0;

        /// <summary>
        /// Bu iş için seçilecek kap ve aletler. Zorunlu bir şey eksikse <paramref name="missing"/> dolar (emir verilemez).
        /// </summary>
        public KitchenUse Resolve(Character c, ItemInstance station, InteractionDef i, out string missing)
        {
            missing = null;
            if (i == null || i.Cook == null) return null;
            var use = new KitchenUse { Cap = 100 };
            var lacks = new List<string>();

            double fuel = FuelNeed(station, i);
            if (fuel > 0 && station.FuelDays + 1e-9 < fuel && _house.Count(FirewoodId) <= 0) lacks.Add(Text(LocText.Of("kitchen.need.firewood")));

            string vesselClass = VesselClass(i);
            if (vesselClass != null)
            {
                var vessel = BestVessel(vesselClass, c, out bool busy);
                if (vessel == null) lacks.Add(Name(CookMethods.VesselKey(vesselClass), busy));
                else
                {
                    use.Vessel = vessel; use.Tier = vessel.EquipTier; use.Cap = KitchenQuality.Cap(vessel.EquipTier);
                    use.Used.Add(vessel.Id);
                }
            }

            var required = new List<string>(); var helpers = new List<string>();
            ToolsFor(i, required, helpers);
            foreach (var t in required)
            {
                var tool = BestTool(t, c);
                if (tool == null) lacks.Add(Name(Equip.ToolKey(t), OwnsTool(t)));
                else { use.Used.Add(tool.Id); if (t == Equip.Knife) use.UsesKnife = true; }
            }
            int have = 0;
            foreach (var t in helpers)
            {
                var tool = BestTool(t, c);
                if (tool == null) use.MissingHelpers.Add(t);
                else { have++; if (t != Equip.Board) use.Used.Add(tool.Id); }
            }

            if (lacks.Count > 0) { missing = Text(LocText.Of("kitchen.block.missing", ("list", string.Join(", ", lacks)))); return use; }

            bool dirty = use.Vessel != null && AllDirty(use.Vessel.Id);
            int vesselQuality = use.Vessel != null ? Math.Min(ItemQuality.NormalMax, _house.BestQuality(use.Vessel.Id)) : 0;
            use.Score = KitchenQuality.EquipmentScore(use.Tier, vesselQuality, have, helpers.Count, dirty, use.UsesKnife && KnifeDull, Sparkling);
            return use;
        }

        private string Name(string key, bool busy) => busy ? Text(LocText.Of("kitchen.busy", ("name", Text(LocText.Of(key))))) : Text(LocText.Of(key));

        /// <summary>Başlama engeli (InteractionEffects.ExtraBlocker); yoksa null.</summary>
        public string Blocker(Character c, ItemInstance item, InteractionDef i)
        {
            if (!Enforce || i == null || Exempt(c)) return null;
            if (i.WashDishes) return WashBlocker(item);
            if (i.Cook == null) return null;
            Resolve(c, item, i, out string missing);
            return missing;
        }

        private string WashBlocker(ItemInstance place)
        {
            if (Owned(BasketId) <= 0) return Text(LocText.Of("kitchen.block.no_basket"));
            if (DirtyPieces <= 0) return Text(LocText.Of("kitchen.block.no_dishes"));
            if (place != null && place.Def.WashPlace == WashPlaces.Stone)
            {
                bool water = false, frozen = false;
                for (int dy = -StoneWaterRadius; dy <= StoneWaterRadius && !water; dy++)
                    for (int dx = -StoneWaterRadius; dx <= StoneWaterRadius; dx++)
                    {
                        var p = new GridPos(place.Position.X + dx, place.Position.Y + dy, place.Position.Z);
                        if (_world.Map.IsOpenWater(p)) { water = true; break; }
                        if (_world.Map.IsWater(p)) frozen = true;
                    }
                if (!water) return Text(LocText.Of(frozen ? "kitchen.block.frozen" : "kitchen.block.stone_dry"));
            }
            return null;
        }

        // ---- alıp getirme ----

        /// <summary>İşin ayak işleri (JobRunner.Errands): kabı ve aletleri al, su doldur; bitince yerine koy.</summary>
        public Errand ErrandFor(Character c, ItemInstance station, InteractionDef i)
        {
            if (!Enforce || i == null || Exempt(c)) return null;
            if (i.WashDishes) return WashErrand(c, station);
            if (i.Cook == null) return null;
            var use = Resolve(c, station, i, out string missing);
            if (use == null || missing != null) return null;
            _lastUse[c.Id] = (station.Id, i.Id, use);

            var errand = new Errand();
            var taken = new List<ItemInstance>();
            Reserve(c, use.Used);
            errand.OnEnd = () => { Release(c); foreach (var t in taken) if (t.TakenBy == c.Id) { t.TakenBy = 0; _world.NotifyChanged(t.Id); } };

            // Önce kap, sonra su, sonra el aletleri; aynı yerden alınanlar tek durak.
            string carry = null;
            ItemInstance lastAt = null;
            var origins = new List<(ItemInstance at, string defId)>();
            foreach (var defId in use.Used)
            {
                var at = Locate(defId, station, c);
                if (at == null) continue;
                origins.Add((at, defId));
                var holder = at.IsDecor && at.ParentId.HasValue ? _world.GetItem(at.ParentId.Value) : at;
                bool sameSpot = lastAt != null && ReferenceEquals(holder, lastAt);
                bool isVessel = use.Vessel != null && defId == use.Vessel.Id;
                if (!sameSpot)
                    errand.Before.Add(new JobStop { At = at, StepId = isVessel ? "fetch_vessel" : "fetch_tool", Pose = "reach", Carry = carry });
                var grabbed = at;
                var stopRef = errand.Before[errand.Before.Count - 1];
                var prev = stopRef.OnDone;
                stopRef.OnDone = who => { prev?.Invoke(who); if (grabbed.Def.Id == defId && grabbed.TakenBy == 0) { grabbed.TakenBy = who.Id; taken.Add(grabbed); _world.NotifyChanged(grabbed.Id); } };
                lastAt = holder;
                if (carry == null || isVessel) carry = defId;

                if (isVessel && (i.Cook == CookMethods.Boil || i.Cook == CookMethods.Stew))
                {
                    var water = NearestWater(station);
                    if (water != null)
                    {
                        errand.Before.Add(new JobStop { At = water, StepId = "fill_water", Pose = "pour", Sfx = "bath.splash", Carry = carry });
                        lastAt = water;
                    }
                }
            }
            errand.CarryToMain = carry;

            // Dönüş: aldıklarını yerine koyar (ters sırayla; aynı yer tek durak).
            ItemInstance lastBack = null;
            for (int k = origins.Count - 1; k >= 0; k--)
            {
                var (at, defId) = origins[k];
                var holder = at.IsDecor && at.ParentId.HasValue ? _world.GetItem(at.ParentId.Value) : at;
                if (lastBack != null && ReferenceEquals(holder, lastBack)) continue;
                var back = at;
                errand.After.Add(new JobStop
                {
                    At = at, StepId = "put_back", Pose = "reach", Carry = defId,
                    OnDone = who => { if (back.TakenBy == who.Id) { back.TakenBy = 0; _world.NotifyChanged(back.Id); } }
                });
                lastBack = holder;
            }
            return errand;
        }

        private Errand WashErrand(Character c, ItemInstance place)
        {
            if (WashBlocker(place) != null) return null;
            var errand = new Errand();
            var basket = Locate(BasketId, place, c);
            if (basket != null)
            {
                errand.Before.Add(new JobStop { At = basket, StepId = "fetch_basket", Pose = "reach" });
                errand.After.Add(new JobStop { At = basket, StepId = "put_back", Pose = "reach", Carry = BasketId });
            }
            // Kirliler ocağın ve sofranın başından toplanır.
            ItemInstance fire = null; int best = int.MaxValue;
            foreach (var it in _world.Items)
            {
                if (!it.Def.CookFire || it.IsDecor || !_world.Map.IsOwned(it.Position)) continue;
                int d = Distance(it.Position, c.Position);
                if (d < best) { best = d; fire = it; }
            }
            if (fire != null) errand.Before.Add(new JobStop { At = fire, StepId = "collect_dishes", Pose = "bend", Carry = BasketId, PauseDays = JobStop.DefaultPauseDays * 2 });
            errand.CarryToMain = BasketId;
            return errand;
        }

        // Eşyanın durduğu yer: yerleştirilmişse kendisi (yuvasındaki dekor), değilse en yakın depo eşyası; hiçbiri yoksa null.
        private ItemInstance Locate(string defId, ItemInstance near, Character c)
        {
            var from = near != null ? near.Position : c.Position;
            ItemInstance best = null; int bestD = int.MaxValue;
            foreach (var it in _world.Items)
            {
                if (it.Def.Id != defId || (it.TakenBy != 0 && it.TakenBy != c.Id) || !OnPlot(it)) continue;
                var pos = it.IsDecor && it.ParentId.HasValue ? (_world.GetItem(it.ParentId.Value)?.Position ?? it.Position) : it.Position;
                int d = Distance(pos, from);
                if (d < bestD) { bestD = d; best = it; }
            }
            if (best != null) return best;
            if (_house.Count(defId) <= 0) return null;
            // Envanterdeki alet ve kap genel depodan alınır (sandık, raf, dolap, kiler rafı); odun sepeti, odunluk, fıçı,
            // tohum sandığı ve ambar sayılmaz (Unity testi 63e, bulgu 2). Mutfak dolabı ve rafları aynı uzaklıkta öne geçer.
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || !IsToolStore(it.Def) || !_world.Map.IsOwned(it.Position)) continue;
                int d = Distance(it.Position, from) * 2 + (Array.IndexOf(KitchenStores, it.Def.Id) >= 0 ? 0 : 1);
                if (d < bestD) { bestD = d; best = it; }
            }
            return best;
        }

        /// <summary>Mutfak aletinin durabileceği depo eşyaları; önce mutfak dolabı ve rafları.</summary>
        public static readonly string[] KitchenStores = { "item.aumbry", "item.pantry_shelf", "item.shelf_wood", "item.spice_chest" };
        public static readonly string[] GeneralStores =
            { "item.chest_wood", "item.chest_iron", "item.chest_bench", "item.carved_chest", "item.catki_chest", "item.yoruk_chest", "item.bed_end_chest" };

        public static bool IsToolStore(ItemDef d) =>
            d != null && d.Storage > 0 && d.Placement != ItemPlacement.Wall && (Array.IndexOf(KitchenStores, d.Id) >= 0 || Array.IndexOf(GeneralStores, d.Id) >= 0);

        private ItemInstance NearestWater(ItemInstance near)
        {
            ItemInstance best = null; int bestD = int.MaxValue;
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || Array.IndexOf(WaterIds, it.Def.Id) < 0 || !_world.Map.IsOwned(it.Position)) continue;
                int d = Distance(it.Position, near.Position);
                if (d < bestD) { bestD = d; best = it; }
            }
            return best;
        }

        private static int Distance(GridPos a, GridPos b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z) * JobRunner.OtherLevelDistance;

        private void Reserve(Character c, List<string> ids)
        {
            Release(c);
            var mine = new List<string>(ids);
            _reserved[c.Id] = mine;
            foreach (var id in mine) _inUse[id] = (_inUse.TryGetValue(id, out int n) ? n : 0) + 1;
        }

        private void Release(Character c)
        {
            if (!_reserved.TryGetValue(c.Id, out var mine)) return;
            foreach (var id in mine)
                if (_inUse.TryGetValue(id, out int n)) { if (n <= 1) _inUse.Remove(id); else _inUse[id] = n - 1; }
            _reserved.Remove(c.Id);
        }

        // ---- ateş ve yakıt (63a §3) ----

        /// <summary>
        /// Ocağın başına varınca (JobRunner.PrepareUse): yakıt yetmiyorsa odun atılır, pişirme payı düşülür. Ateş bu iş için
        /// yeni tutuşturulduysa zincirin "tutuştur" adımı oynar.
        /// </summary>
        public UsePrep Prepare(Character c, ItemInstance station, InteractionDef i)
        {
            var prep = new UsePrep();
            if (i == null || i.Cook == null) return prep;
            if (!Enforce || Exempt(c)) { prep.Flags = new[] { ChainConditions.Knife }; return prep; }   // alet kuralı yok: eskisi gibi doğrar
            var use = UseFor(c, station, i);
            var flags = new List<string>();
            if (use != null && use.UsesKnife && use.MissingHelpers.Contains(Equip.Board)) flags.Add(ChainConditions.NoBoard);   // doğranacak bir şey yoksa dizinde de doğramaz
            if (use != null && use.UsesKnife) flags.Add(ChainConditions.Knife);   // bıçaksız yemekte doğrama adımı oynamaz (63e bulgu 4)
            if (flags.Count > 0) prep.Flags = flags.ToArray();
            double need = FuelNeed(station, i);
            if (need <= 0) return prep;
            bool wasLit = station.IsLit;
            int logs = 0;
            for (int guard = 0; guard < 8 && station.FuelDays + 1e-9 < need; guard++)
            {
                bool added = Stoke != null ? Stoke(station) : _house.TryTakeItem(FirewoodId);
                if (!added) break;
                logs++;
                if (Stoke == null) station.FuelDays += DaysPerLog;
            }
            if (logs > 0) Stoked?.Invoke(c, station);
            if (station.FuelDays + 1e-9 < need) { prep.Fail = Text(LocText.Of("kitchen.block.no_wood")); return prep; }
            station.FuelDays = Math.Max(0, station.FuelDays - need);
            prep.Kindled = !wasLit;
            _world.NotifyChanged(station.Id);
            return prep;
        }

        // ---- süre ve kalite ----

        /// <summary>İş süresi çarpanı (JobRunner.DurationFactor): eksik yardımcı alet yavaşlatır; bulaşık parça sayısıyla uzar.</summary>
        public double DurationFactor(Character c, ItemInstance item, InteractionDef i)
        {
            if (!Enforce || i == null || Exempt(c)) return 1.0;
            if (i.WashDishes) return (1.0 + WashPerPiece * Math.Min(WashPieceCap, DirtyPieces)) * (HasSoap() ? SoapSpeed : 1.0);
            if (i.Cook == null) return 1.0;
            var use = UseFor(c, item, i);
            if (use == null) return 1.0;
            double f = 1.0 + MissingHelperSlow * use.MissingHelpers.Count;
            if (use.UsesKnife && KnifeDull) f += MissingHelperSlow;
            return f;
        }

        /// <summary>Kalite tavanı ve seviye kaydırması (InteractionEffects.KitchenQuality).</summary>
        public (int maxQuality, int levelShift)? QualityFor(Character c, ItemInstance item, InteractionDef i)
        {
            if (!Enforce || i == null || i.Cook == null || Exempt(c)) return null;
            var use = UseFor(c, item, i);
            if (use == null) return null;
            return (KitchenQuality.MaxQuality(use.Cap), i.Dish != null ? 0 : KitchenQuality.LevelShift(use.Score));
        }

        /// <summary>
        /// Sofra puanının ekipman bileşeni (MealService.EquipmentPoints; 58 §6, 63a §5.2). Kap istemeyen işte (çiğ, köz,
        /// tepsisiz fırın) null: bileşen ocağın ya da fırının durumundan gelir.
        /// </summary>
        public int? EquipmentPointsFor(Character c, ItemInstance station, InteractionDef i)
        {
            if (!Enforce || i == null || i.Cook == null || Exempt(c)) return null;
            var use = UseFor(c, station, i);
            return use == null || use.Vessel == null ? (int?)null : use.Score;
        }

        /// <summary>Kabın kalite tavanı (MealService.QualityCap; 63a §5.4); tavan yoksa null.</summary>
        public int? QualityCapFor(Character c, ItemInstance station, InteractionDef i) => QualityFor(c, station, i)?.maxQuality;

        /// <summary>Tarif kartı için: bu iş bugün hangi kapla, kaç puanla ve hangi tavanla yapılır; eksik varsa nedeni.</summary>
        public KitchenUse Preview(Character c, ItemInstance item, InteractionDef i, out string missing) => Resolve(c, item, i, out missing);

        // ---- kirlenme ve bulaşık (63a §4) ----

        /// <summary>İş uygulandı (InteractionEffects.AppliedOn): pişirme kirletir, bulaşık yıkama temizler, bileme bıçağı da biler.</summary>
        public void OnApplied(Character c, ItemInstance item, InteractionDef i)
        {
            if (i == null) return;
            if (i.SkillWork == "sharpen" || (item != null && item.Def.Id == "item.whetstone")) _knifeUses = 0;
            if (!Enforce || Exempt(c)) return;
            if (i.WashDishes) { Wash(c, item); return; }
            if (i.Cook == null) return;
            var use = UseFor(c, item, i);
            if (use == null) return;
            foreach (var id in use.Used) MarkDirty(id);
            if (use.UsesKnife) _knifeUses++;
            _lastUse.Remove(c.Id);
        }

        private void MarkDirty(string defId)
        {
            int owned = Owned(defId);
            if (owned <= 0) return;
            int n = DirtyCount(defId);
            if (n >= owned) return;
            _dirty[defId] = n + 1;
            if (_dirtySince < 0) _dirtySince = Today;
        }

        /// <summary>Bir öğün bitti (JobRunner.InteractionCompleted): pişmiş yemek bir tabak kirletir; temiz tabak kalmadıysa kasvet.</summary>
        public void OnCompleted(Character c, InteractionDef i)
        {
            if (!Enforce || i == null || Exempt(c) || !i.RestoresNeed || i.Need != NeedType.Hunger || i.ConsumeItem == null) return;
            if (!_content.Items.TryGet(i.ConsumeItem, out var food) || !food.HasTag("cooked")) return;
            if (TablewareOwned <= 0) return;
            if (CleanTableware > 0)
            {
                _dirtyTableware++;
                if (_dirtySince < 0) _dirtySince = Today;
            }
            else c.AddMoodlet(DirtyPlateMood, Text(LocText.Of("kitchen.mood.dirty_plate")), -1, 0.25);
        }

        /// <summary>
        /// Sofrada bir porsiyon yendi (MealService.PlateUsed): tabak masada kirli kalır, sofra toplanınca bulaşığa girer.
        /// Temiz tabak kalmadıysa kasvet (kirli tabaktan yedi).
        /// </summary>
        public void OnTablePlate(Character c)
        {
            if (!Enforce || Exempt(c) || TablewareOwned <= 0) return;
            if (CleanTableware <= 0) c.AddMoodlet(DirtyPlateMood, Text(LocText.Of("kitchen.mood.dirty_plate")), -1, 0.25);
        }

        /// <summary>Sofra toplandı (MealService.PlatesCleared): masadaki kirli tabaklar bulaşığa girer (elde olan takım kadar).</summary>
        public void AddDirtyTableware(int plates)
        {
            if (!Enforce || plates <= 0) return;
            int n = Math.Min(TablewareOwned, _dirtyTableware + plates);
            if (n <= _dirtyTableware) return;
            _dirtyTableware = n;
            if (_dirtySince < 0) _dirtySince = Today;
        }

        private bool HasSoap() { foreach (var id in SoapIds) if (_house.Count(id) > 0) return true; return false; }

        private void Wash(Character c, ItemInstance place)
        {
            int pieces = DirtyPieces;
            if (pieces <= 0) return;
            _dirty.Clear();
            _dirtyTableware = 0;
            _dirtySince = -1;
            bool soap = false;
            foreach (var id in SoapIds) if (_house.TryTakeItem(id)) { soap = true; break; }
            if (soap) _sparkleUntil = Today + 1.0;
            if (place != null && place.Def.WashPlace == WashPlaces.Stone && TemperatureAt != null && TemperatureAt(place.Position) < ColdHandsBelow)
                c.AddMoodlet(ColdHandsMood, Text(LocText.Of("kitchen.mood.cold_hands")), -2, 0.2);
            Notice?.Invoke(c, LocText.Of(soap ? "kitchen.n.washed_soap" : "kitchen.n.washed", ("count", pieces)));
        }

        /// <summary>Ev görevlisi için: bulaşık yıkanmalı mı (63a §4.6).</summary>
        public bool WashDue => Enforce && Owned(BasketId) > 0 && (DirtyPieces >= DutyWashPieces || (DirtyPieces > 0 && DirtyAgeDays >= DutyWashAfterDays));

        /// <summary>
        /// Ev görevi (DutyPlanner.HouseChores): bulaşık yıkanmalıysa hanenin yıkama yerleri. Görevli en uygununu seçer;
        /// başlama engeli olan yer (kıyı donmuş) zaten elenir.
        /// </summary>
        public IEnumerable<(ItemInstance item, InteractionDef interaction)> WashChores()
        {
            if (!WashDue) yield break;
            foreach (var it in _world.Items)
            {
                if (it.Def.WashPlace == null || it.IsDecor || !_world.Map.IsOwned(it.Position)) continue;
                foreach (var x in it.Def.Interactions) if (x.WashDishes) { yield return (it, x); break; }
            }
        }

        /// <summary>Ev görevlisinin yıkayacağı yer: önce lüle, sonra leğen, sonra kıyı taşı; uygun yer yoksa null.</summary>
        public Job WashJob(Character c)
        {
            if (!WashDue) return null;
            ItemInstance best = null; InteractionDef bestI = null; int bestKey = int.MaxValue;
            foreach (var it in _world.Items)
            {
                if (it.Def.WashPlace == null || it.IsDecor || !_world.Map.IsOwned(it.Position)) continue;
                InteractionDef wash = null;
                foreach (var x in it.Def.Interactions) if (x.WashDishes) { wash = x; break; }
                if (wash == null || WashBlocker(it) != null) continue;
                int rank = it.Def.WashPlace == WashPlaces.Spout ? 0 : it.Def.WashPlace == WashPlaces.Tub ? 1 : 2;
                int key = rank * 10000 + Distance(it.Position, c.Position);
                if (key < bestKey) { bestKey = key; best = it; bestI = wash; }
            }
            return best == null ? null : Job.UseItem(best.Id, bestI.Id);
        }

        // ---- başlangıç ve eski kayıt ----

        /// <summary>Yeni oyunun mutfak takımı (63a §2): kil tencere, bıçak, tahta kaşık, dört kâse, bulaşık sepeti.</summary>
        public static readonly (string itemId, int count)[] StarterKit =
        {
            ("item.pot_clay", 1), ("item.kitchen_knife", 1), ("item.wooden_spoon", 1), ("item.wooden_bowl", 4), (BasketId, 1),
        };

        /// <summary>
        /// Eski kaydın hediyesi: bu kurallardan önce her yemeği pişirebilen hane, güncellemeyle kilitlenmesin diye her
        /// yöntemin temel kabını ve aletlerini alır. Fırını olan haneye kürek, oklava ve tepsi de verilir.
        /// </summary>
        public void GrantLegacyKit()
        {
            Give("item.cooking_pot"); Give("item.griddle_iron"); Give("item.guvec_plain"); Give("item.spit_iron");
            Give("item.kitchen_knife"); Give("item.wooden_spoon"); Give("item.ladle"); Give(BasketId);
            foreach (var it in _world.Items)
                if (it.Def.Id == "item.stone_oven" && _world.Map.IsOwned(it.Position))
                { Give("item.baker_peel"); Give("item.rolling_board"); Give("item.baking_tray_clay"); break; }
        }

        private void Give(string id) { if (_content.Items.Contains(id) && Owned(id) <= 0) _house.AddItem(id, 1); }

        public const string WashTubId = "item.dish_tub";
        /// <summary>Leğenin aranacağı yarıçap: ocaktan en çok bu kadar karo.</summary>
        public const int WashTubRadius = 4;

        /// <summary>Arsada bulaşık yıkanacak bir yer (leğen, yalak, çamaşır leğeni, kıyı taşı) var mı.</summary>
        public bool HasWashPlace()
        {
            foreach (var it in _world.Items)
                if (!it.IsDecor && it.Def.WashPlace != null && _world.Map.IsOwned(it.Position)) return true;
            return false;
        }

        /// <summary>
        /// Yıkama yeri güvencesi (Unity testi 63e, bulgu 1): arsada hiç yıkama yeri yoksa ocağın yanına bir bulaşık leğeni
        /// konur; uygun karo bulunamazsa leğen envantere girer ve oyuncuya yapı modunda yerleştirmesi söylenir. Yeni oyunda
        /// leğen başlangıç evinin mutfak şablonundan gelir; bu yol leğensiz eski kayıtlar ve şablonun koyamadığı durum içindir.
        /// Koyduğu leğeni döner; bir şey yapmadıysa null.
        /// </summary>
        public ItemInstance EnsureWashPlace(bool announce = true)
        {
            if (!Enforce || HasWashPlace() || !_content.Items.TryGet(WashTubId, out var tub)) return null;
            if (_house.Count(WashTubId) > 0) return null;   // envanterde bekliyor: oyuncu yerleştirecek
            ItemInstance fire = null;
            foreach (var it in _world.Items)
                if (!it.IsDecor && it.Def.CookFire && _world.Map.IsOwned(it.Position)) { fire = it; break; }
            if (fire != null)
            {
                for (int r = 1; r <= WashTubRadius; r++)
                    for (int dy = -r; dy <= r; dy++)
                        for (int dx = -r; dx <= r; dx++)
                        {
                            if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;
                            var p = new GridPos(fire.Position.X + dx, fire.Position.Y + dy, fire.Position.Z);
                            if (!_world.Map.IsOwned(p) || !LeavesWayOpen(p)) continue;
                            if (!_world.CanPlaceFloor(tub, p, 0).Ok) continue;
                            var placed = _world.PlaceItem(tub, p);
                            if (placed == null) continue;
                            if (announce) Notice?.Invoke(null, LocText.Of("kitchen.n.tub_placed"));
                            return placed;
                        }
            }
            _house.AddItem(WashTubId, 1);
            if (announce) Notice?.Invoke(null, LocText.Of("kitchen.n.tub_given"));
            return null;
        }

        // Leğen yolu kapatmasın: karo yürünebilir ve en az iki yanı yürünebilir kalır; eşyaların önü (ocak, sandık) tıkanmaz.
        private bool LeavesWayOpen(GridPos p)
        {
            if (!_world.Map.IsWalkable(p)) return false;
            int open = 0;
            foreach (var n in new[] { new GridPos(p.X + 1, p.Y, p.Z), new GridPos(p.X - 1, p.Y, p.Z), new GridPos(p.X, p.Y + 1, p.Z), new GridPos(p.X, p.Y - 1, p.Z) })
            {
                if (_world.Map.IsWalkable(n)) open++;
            }
            if (open < 2) return false;
            // Etkileşimli bir eşyanın tek boş yanını kapatma.
            foreach (var it in _world.Items)
            {
                if (it.IsDecor || it.Position.Z != p.Z || it.Def.Interactions.Count == 0) continue;
                if (Math.Abs(it.Position.X - p.X) + Math.Abs(it.Position.Y - p.Y) != 1) continue;
                int free = 0;
                foreach (var n in new[] { new GridPos(it.Position.X + 1, it.Position.Y, p.Z), new GridPos(it.Position.X - 1, it.Position.Y, p.Z), new GridPos(it.Position.X, it.Position.Y + 1, p.Z), new GridPos(it.Position.X, it.Position.Y - 1, p.Z) })
                    if (!n.Equals(p) && _world.Map.IsWalkable(n)) free++;
                if (free == 0) return false;
            }
            return true;
        }

        // ---- kayıt ----

        public KitchenSave Export()
        {
            var list = new List<string>();
            foreach (var kv in _dirty) if (kv.Value > 0) list.Add(kv.Key + ":" + kv.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return new KitchenSave { version = 1, dirty = list.ToArray(), dirtyTableware = _dirtyTableware, dirtySince = _dirtySince, sparkleUntil = _sparkleUntil, knifeUses = _knifeUses };
        }

        /// <summary>Kaydı yükler. Kayıtta mutfak durumu yoksa (bu kurallardan önceki kayıt) hediye takımı verilir.</summary>
        public void Restore(KitchenSave save)
        {
            _dirty.Clear(); _inUse.Clear(); _reserved.Clear(); _lastUse.Clear();
            _dirtyTableware = 0; _dirtySince = -1; _sparkleUntil = 0; _knifeUses = 0;
            if (save == null || save.version < 1) { GrantLegacyKit(); EnsureWashPlace(); return; }
            if (save.dirty != null)
                foreach (var s in save.dirty)
                {
                    int k = s == null ? -1 : s.LastIndexOf(':');
                    if (k <= 0) continue;
                    if (int.TryParse(s.Substring(k + 1), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int n) && n > 0
                        && _content.Items.Contains(s.Substring(0, k))) _dirty[s.Substring(0, k)] = n;
                }
            _dirtyTableware = Math.Max(0, save.dirtyTableware);
            _dirtySince = save.dirtySince;
            _sparkleUntil = save.sparkleUntil;
            _knifeUses = Math.Max(0, save.knifeUses);
            EnsureWashPlace();   // leğensiz kayıt (63e bulgu 1)
        }
    }
}
