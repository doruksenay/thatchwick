using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Ev bakımı işlerinin kodları (InteractionDef.HomeAction).</summary>
    public static class HomeActions
    {
        public const string Candle = "candle", Strew = "strew", Whitewash = "whitewash", WaterPot = "waterPot",
            HarvestPot = "harvestPot", Upgrade = "upgrade", Stow = "stow", Catch = "catch", Mist = "mist";
    }

    /// <summary>Bir eşya örneğinin ev bakımı durumu (yükseltmeler, desen, canlı saksı, ot serpmesi).</summary>
    public sealed class HomeItemState
    {
        public readonly List<string> Upgrades = new List<string>();
        public string Pattern;
        /// <summary>Canlı saksı: son sulamadan beri geçen gün.</summary>
        public double PotDry;
        /// <summary>Canlı saksı: büyüme (gün).</summary>
        public double PotGrowth;
        /// <summary>Ot serpilmiş hasırın tazeliği bu güne kadar sürer.</summary>
        public double StrewUntil = -1;
        public bool IsEmpty => Upgrades.Count == 0 && Pattern == null && PotDry == 0 && PotGrowth == 0 && StrewUntil < 0;
    }

    /// <summary>Bir odanın ev bakımı durumu (ocak isi, badana).</summary>
    public sealed class HomeRoomState
    {
        public double Soot;
        public double WhitewashUntil = -1;
        public bool Warned;
    }

    /// <summary>
    /// Ev bakımı ve ev otomasyonu (Project belgesi 45). Dekorun zamanla işleyen ikinci yarısı:
    /// - Mum akşamları yanar ve biter ("Mum değiştir"); odanın ışık puanı gece ince işleri hızlandırır ya da yavaşlatır.
    /// - Bacasız ocak odanın duvarlarını isle karartır (güzellik eksi); badana siler ve bir süre "Ak badana" verir.
    /// - Hasıra ot serpilir ("Ferah oda"); canlı saksı sulanır, büyür, ot verir; sulanmazsa solar.
    /// - Eşya yükseltmeleri (şilte, demir kuşak, cila, baca), oda türleri (mutfak, yatak odası, atölye, kiler).
    /// - Ev görevi ve hizmetkâr bu işleri (Chores) yapar. Desen ve arma, koleksiyonlar, oda planları, mevsim setleri
    ///   ve ev yarışması ayrı dosyalarda (partial).
    /// Sayılar yer tutucudur (Adım 28).
    /// </summary>
    public sealed partial class HomeLife
    {
        // ---- sabitler ----
        /// <summary>Mumun yandığı akşam saatleri (gün kesri).</summary>
        public const double EveningStart = 0.75, EveningEnd = 0.96;
        public const string DefaultCandle = "item.candle";
        /// <summary>Bacasız yanan ocak odaya günde bu kadar is katar (baca ile ChimneySootFactor katı).</summary>
        public const double SootPerDay = 0.5, ChimneySootFactor = 0.25;   // denge: sürekli yanan ocakta uyarı ~8 günde (yaklaşık mevsimde bir badana)
        public const double SootWarnAt = 4.0;
        public const int MaxSootPenalty = 6;
        public const int WhitewashBonus = 3;
        public const string FreshRoomMoodId = "mood.fresh_room";
        public const int FreshRoomMood = 5;
        public const int FreshRoomBeauty = 2;
        public const double StrewDays = 3.0;
        public static readonly string[] StrewHerbs = { "item.herb", "item.mint", "item.sage", "item.chamomile", "item.tea_mint", "item.tea_sage", "item.tea_chamomile" };
        public const string StrewableTag = "strewable";
        public const double PotWiltDays = 3.0;
        public const double PotThirstyDays = 1.0;
        public const int WiltedPotBeauty = -1;
        public const string LimeId = "item.lime";
        public const string ChimneyTag = "chimney";

        private readonly WorldState _world;
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly GameClock _clock;
        private readonly Dictionary<int, HomeItemState> _items = new Dictionary<int, HomeItemState>();
        private readonly Dictionary<string, HomeRoomState> _rooms = new Dictionary<string, HomeRoomState>(StringComparer.Ordinal);
        private readonly HashSet<int> _candlesSeen = new HashSet<int>();
        private double _moodAccum;

        /// <summary>Hane üyeleri (Ferah oda, hizmetkâr yakınlığı); null ise yok.</summary>
        public Func<IEnumerable<Character>> Members { get; set; }
        /// <summary>Bildirim (metin, karakter kimliği).</summary>
        public Action<string, int> Notify { get; set; }
        /// <summary>Yapı parçası kataloğu (duvar kaplaması güzelliği) için içerik; kurucu verir.</summary>
        public ContentDatabase Content => _content;
        /// <summary>Etkileşim uygulayıcı (hizmetkârın işleri); LifeSystems.Attach(InteractionEffects) bağlar.</summary>
        public Work.InteractionEffects Effects { get; set; }
        /// <summary>Yapı servisi (oda planı uygulama, sipariş); LifeSystems.Attach(BuildService) bağlar.</summary>
        public BuildService Build { get; set; }

        public HomeLife(WorldState world, ContentDatabase content, Household house, GameClock clock)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        private double Now => _clock.TotalDays;
        private void Say(string text, int id = 0) => Notify?.Invoke(text, id);
        private string Name(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        public HomeItemState StateOf(int itemId) => _items.TryGetValue(itemId, out var s) ? s : null;
        private HomeItemState Ensure(int itemId)
        {
            if (!_items.TryGetValue(itemId, out var s)) { s = new HomeItemState(); _items[itemId] = s; }
            return s;
        }

        /// <summary>Odanın kalıcı anahtarı: en küçük karosu (oda kimliği yeniden hesaplanınca değişir).</summary>
        public static string RoomKey(Room room)
        {
            if (room == null || room.Tiles.Count == 0) return null;
            var min = room.Tiles[0];
            foreach (var t in room.Tiles)
                if (t.Z < min.Z || (t.Z == min.Z && (t.Y < min.Y || (t.Y == min.Y && t.X < min.X)))) min = t;
            return $"{min.X},{min.Y},{min.Z}";
        }

        public HomeRoomState RoomState(Room room)
        {
            string key = RoomKey(room);
            return key != null && _rooms.TryGetValue(key, out var s) ? s : null;
        }
        private HomeRoomState EnsureRoom(Room room)
        {
            string key = RoomKey(room);
            if (key == null) return null;
            if (!_rooms.TryGetValue(key, out var s)) { s = new HomeRoomState(); _rooms[key] = s; }
            return s;
        }

        /// <summary>Eşyanın bulunduğu oda (dekor ve duvar eşyası için ön karosu).</summary>
        public Room RoomOf(ItemInstance item) => item == null ? null : _world.Map.Rooms.RoomAt(item.Position);

        // ---- zaman ----

        public static bool IsEvening(double dayFraction) => dayFraction >= EveningStart && dayFraction < EveningEnd;
        /// <summary>Gece: ince işler ışığa bakar.</summary>
        public static bool IsNight(double dayFraction) => dayFraction >= 0.79 || dayFraction < 0.25;

        public void Tick(double gameDays)
        {
            if (gameDays <= 0) return;
            bool evening = IsEvening(_clock.DayFraction);
            var hearthRooms = new Dictionary<string, (Room room, double rate)>(StringComparer.Ordinal);
            foreach (var it in _world.Items)
            {
                // Mum: ilk görüldüğünde dolu başlar (yeni yerleşen ve eski kayıttaki mumlar), akşamları yanar.
                if (it.Def.Burns)
                {
                    if (_candlesSeen.Add(it.Id) && it.FuelDays <= 0) it.SetCandle(it.Def.BurnDays);
                    else if (evening && it.FuelDays > 0)
                    {
                        it.SetCandle(it.FuelDays - gameDays);
                        if (it.FuelDays <= 0) _burntToday++;
                    }
                }
                // Canlı saksı: kurur; sulu ve güneşteyse büyür.
                if (it.Def.IsLivePot)
                {
                    var s = Ensure(it.Id);
                    s.PotDry += gameDays * it.Def.Thirst;
                    bool sun = Sunlit(it);
                    double speed = it.Def.ShadePlant ? (sun ? 0.5 : 1.0) : (sun ? 1.0 : 0.5);   // eğrelti gölgeyi sever
                    if (s.PotDry < PotWiltDays && s.PotGrowth < it.Def.PotGrowDays)
                        s.PotGrowth = Math.Min(it.Def.PotGrowDays, s.PotGrowth + gameDays * speed);
                }
                // İs: kapalı odada yanan ocak.
                if (!it.IsDecor && it.Def.HeatOutput > 0f && it.IsLit)
                {
                    var room = RoomOf(it);
                    if (room == null || !room.IsIndoor) continue;
                    string key = RoomKey(room);
                    double rate = HasChimney(it) ? ChimneySootFactor : 1.0;
                    if (!hearthRooms.TryGetValue(key, out var prev) || prev.rate < rate) hearthRooms[key] = (room, rate);
                }
            }
            foreach (var kv in hearthRooms)
            {
                var st = EnsureRoom(kv.Value.room);
                st.Soot += gameDays * SootPerDay * kv.Value.rate;
                if (!st.Warned && st.Soot >= SootWarnAt)
                {
                    st.Warned = true;
                    Say($"{RoomName(kv.Value.room)} duvarları isle karardı. Ocakta \"Badana\" yap (2 kireç); baca isi azaltır.");
                }
            }

            ServantTick();
            VivariumTick(gameDays);
            _moodAccum += gameDays;
            if (_moodAccum >= 0.05)
            {
                _moodAccum = 0;
                RefreshMoods();
            }
        }

        private int _burntToday;

        /// <summary>Günlük işler: biten mumların bildirimi, kalkan eşyaların durumu, güvercinlik, hizmetkâr, koleksiyon.</summary>
        public void OnNewDay()
        {
            if (_burntToday > 0) Say($"{_burntToday} mum yanıp bitti. Ev görevlisi ya da hizmetkâr yenisini takar (\"Mum değiştir\").");
            _burntToday = 0;
            Prune();
            Dovecotes();
            ServantDay();
            CheckCollections();
            VivariumDay();
        }

        /// <summary>Dünyadan kalkan eşyaların durumu silinir (envantere alınan yükseltilmiş eşya yükseltmesini kaybeder).</summary>
        private void Prune()
        {
            var alive = new HashSet<int>();
            foreach (var it in _world.Items) alive.Add(it.Id);
            var gone = new List<int>();
            foreach (var kv in _items) if (!alive.Contains(kv.Key) || kv.Value.IsEmpty) gone.Add(kv.Key);
            foreach (var id in gone) _items.Remove(id);
            _candlesSeen.RemoveWhere(id => !alive.Contains(id));
        }

        private void RefreshMoods()
        {
            if (Members == null) return;
            var fresh = new HashSet<string>(StringComparer.Ordinal);
            foreach (var it in _world.Items)
                if (IsStrewnFresh(it)) { var r = RoomOf(it); if (r != null && r.IsIndoor) fresh.Add(RoomKey(r)); }
            var singing = SingingRooms();
            if (fresh.Count == 0 && singing.Count == 0) return;
            foreach (var c in Members())
            {
                if (c.IsDead || !c.MoodEnabled || c.IsVillager) continue;
                var room = _world.Map.Rooms.RoomAt(c.Position);
                if (room == null) continue;
                if (fresh.Contains(RoomKey(room))) c.AddMoodlet(FreshRoomMoodId, "Ferah oda", FreshRoomMood, 0.15);
                if (singing.Contains(RoomKey(room))) c.AddMoodlet(CricketMoodId, "Cırcır böceği ötüyor", CricketMood, 0.15);
            }
        }

        // ---- mum ve ışık ----

        /// <summary>Mum değiştirirken harcanacak mum (envanterde ilk bulunan) ya da null.</summary>
        public string CandleRefill(ItemDef def)
        {
            if (def.BurnRefill.Count > 0)
            {
                foreach (var id in def.BurnRefill) if (_house.Count(id) > 0) return id;
                return null;
            }
            if (_house.Count(def.Id) > 0) return def.Id;
            return _house.Count(DefaultCandle) > 0 ? DefaultCandle : null;
        }

        /// <summary>Odanın ışık puanı: yanan ışıkların gücü (mum 1, fener 2, avize 4).</summary>
        public int LightPoints(Room room)
        {
            if (room == null) return 0;
            int n = 0;
            foreach (var it in _world.Items)
                if (RoomOf(it) != room) continue;
                else if (it.GivesLight) n += it.Def.EffectiveLightPower;
                else n += VivLight(it);   // gece ateş böcekli cam bahçe
            return n;
        }

        /// <summary>Işık basamağı: 0 karanlık, 1 loş, 2 aydınlık (9 karo başına 1 ve 2,5 puan).</summary>
        public int LightLevel(Room room)
        {
            if (room == null || !room.IsIndoor) return 2;
            double density = LightPoints(room) * 9.0 / Math.Max(9, room.Area);
            return density >= 2.5 ? 2 : density >= 1.0 ? 1 : 0;
        }

        public static string LightName(int level) => level >= 2 ? "Aydınlık" : level == 1 ? "Loş" : "Karanlık";

        /// <summary>Gece ince işin (zanaat, okuma, ders) hız çarpanı: karanlık 0,8, loş 0,9, aydınlık 1.</summary>
        public static double NightWorkFactor(int lightLevel) => lightLevel >= 2 ? 1.0 : lightLevel == 1 ? 0.9 : 0.8;

        private static bool IsFineWork(InteractionDef i) =>
            (i.IsCraft && i.Skill != SkillType.Cooking) || i.Skill == SkillType.Literacy || i.Teach || i.Skill == SkillType.Weaving;

        // ---- is, badana, baca ----

        public bool HasChimney(ItemInstance hearth)
        {
            if (hearth == null) return false;
            if (hearth.Def.HasTag(ChimneyTag)) return true;
            var s = StateOf(hearth.Id);
            if (s == null) return false;
            foreach (var u in s.Upgrades) { var d = hearth.Def.FindUpgrade(u); if (d != null && d.Chimney) return true; }
            return false;
        }

        public double SootOf(Room room) => RoomState(room)?.Soot ?? 0;
        public bool IsWhitewashed(Room room) { var s = RoomState(room); return s != null && s.WhitewashUntil > Now; }
        /// <summary>Son birkaç günde bir oda badanalandı mı (şenlik hazırlığı: bayram öncesi badana)?</summary>
        public bool WashedWithin(double days)
        {
            double span = 2 * _clock.Config.DaysPerSeason;
            foreach (var r in _rooms.Values)
                if (r.WhitewashUntil > Now && Now - (r.WhitewashUntil - span) <= days) return true;
            return false;
        }

        public static int SootPenalty(double soot) => Math.Min(MaxSootPenalty, (int)Math.Floor(soot / 2.0));

        /// <summary>Odanın bakım artısı ve eksisi: duvar kaplaması güzelliği, ak badana, ferah ot serpmesi; is.</summary>
        public (int plus, int minus) RoomBeautyExtra(Room room)
        {
            if (room == null || !room.IsIndoor) return (0, 0);
            int plus = WallFinishBeauty(room);
            var st = RoomState(room);
            int minus = 0;
            if (st != null)
            {
                minus = SootPenalty(st.Soot);
                if (st.WhitewashUntil > Now) plus += WhitewashBonus;
            }
            foreach (var it in _world.Items)
                if (IsStrewnFresh(it) && RoomOf(it) == room) { plus += FreshRoomBeauty; break; }
            return (plus, minus);
        }

        private static readonly GridPos[] Dirs = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        /// <summary>İç duvar yüzü kaplamalarının güzelliği: ortalama × 3, en çok 8.</summary>
        public int WallFinishBeauty(Room room)
        {
            var map = _world.Map;
            double sum = 0; int faces = 0;
            foreach (var t in room.Tiles)
                foreach (var d in Dirs)
                {
                    var e = GridMap.EdgeBetween(t, t + d);
                    if (!map.EdgeInBounds(e) || map.GetEdge(e) != EdgeKind.Wall) continue;
                    faces++;
                    var fin = map.GetFinish(e, e.SideB == t);
                    if (fin != null && _content.Parts.TryGet(fin, out var fp)) sum += fp.Beauty;
                }
            if (faces == 0) return 0;
            return Math.Min(8, (int)Math.Round(sum / faces * 3));
        }

        // ---- ot serpme ve canlı saksı ----

        public bool IsStrewable(ItemInstance it) => it != null && it.IsRug && it.Def.HasTag(StrewableTag);
        public bool IsStrewnFresh(ItemInstance it) { var s = IsStrewable(it) ? StateOf(it.Id) : null; return s != null && s.StrewUntil > Now; }
        /// <summary>Serpilmiş ama süresi geçmiş (bayat) hasır.</summary>
        public bool IsStrewnStale(ItemInstance it) { var s = IsStrewable(it) ? StateOf(it.Id) : null; return s != null && s.StrewUntil >= 0 && s.StrewUntil <= Now; }
        public string StrewHerb() { foreach (var h in StrewHerbs) if (_house.Count(h) > 0) return h; return null; }

        /// <summary>Saksı güneş alıyor mu: pencere içi tahtasında ya da pencere kenarındaki karoda.</summary>
        public bool Sunlit(ItemInstance it)
        {
            if (it == null) return false;
            if (it.IsDecor && it.ParentId.HasValue)
            {
                var parent = _world.GetItem(it.ParentId.Value);
                if (parent != null && parent.Def.WindowMount) return true;
                if (parent != null) return NearWindow(parent.Position);
            }
            return NearWindow(it.Position);
        }

        private bool NearWindow(GridPos p)
        {
            var map = _world.Map;
            foreach (var d in Dirs)
            {
                var e = GridMap.EdgeBetween(p, p + d);
                if (map.EdgeInBounds(e) && map.GetEdge(e) == EdgeKind.Window) return true;
            }
            return false;
        }

        public bool PotWilted(ItemInstance it) { var s = it.Def.IsLivePot ? StateOf(it.Id) : null; return s != null && s.PotDry >= PotWiltDays; }
        public bool PotReady(ItemInstance it) { var s = it.Def.PotHerb != null ? StateOf(it.Id) : null; return s != null && s.PotDry < PotWiltDays && s.PotGrowth >= it.Def.PotGrowDays; }
        /// <summary>Süs bitkisi ya da saksı olgun mu (ot vermeyenlerde "gelişmiş").</summary>
        public bool PotGrown(ItemInstance it) { var s = it.Def.IsLivePot ? StateOf(it.Id) : null; return s != null && s.PotDry < PotWiltDays && s.PotGrowth >= it.Def.PotGrowDays; }
        /// <summary>Çiçekte mi: olgun, solmamış ve çiçek mevsiminde.</summary>
        public bool InBloom(ItemInstance it) => it.Def.BloomSeasons.Count > 0 && PotGrown(it) && it.Def.BloomSeasons.Contains(_clock.Season);
        public const int BloomBeauty = 3, GrownPlantBeauty = 1;
        public bool PotThirsty(ItemInstance it) { var s = it.Def.IsLivePot ? StateOf(it.Id) : null; return s == null || s.PotDry >= PotThirstyDays; }

        /// <summary>Saksının kısa durumu: "sulu · büyüyor %40", "susuz", "soldu", "ot hazır".</summary>
        public string PotLabel(ItemInstance it)
        {
            if (!it.Def.IsLivePot) return null;
            var s = StateOf(it.Id);
            if (s == null) return "susuz";
            if (s.PotDry >= PotWiltDays) return "soldu (sula)";
            string water = s.PotDry >= PotThirstyDays ? "susuz" : "sulu";
            if (s.PotGrowth >= it.Def.PotGrowDays)
                return it.Def.PotHerb != null ? $"{Name(it.Def.PotHerb).ToLowerInvariant()} hazır" : InBloom(it) ? $"{water} · çiçekte" : $"{water} · gelişmiş";
            bool slow = it.Def.ShadePlant ? Sunlit(it) : !Sunlit(it);
            return $"{water} · büyüyor %{s.PotGrowth / it.Def.PotGrowDays * 100:0}" + (slow ? (it.Def.ShadePlant ? " (güneşte yavaş, gölge sever)" : " (gölgede yavaş)") : "");
        }

        // ---- yükseltmeler ----

        public IReadOnlyList<string> UpgradesOf(int itemId) => StateOf(itemId)?.Upgrades ?? (IReadOnlyList<string>)Array.Empty<string>();
        public bool HasUpgrade(int itemId, string upgradeId) { var s = StateOf(itemId); return s != null && s.Upgrades.Contains(upgradeId); }

        /// <summary>Bu yükseltme bir başkasının kademe zincirinde aşılmış mı (kuş tüyü varken yün şilte)?</summary>
        private bool Superseded(ItemInstance item, string upgradeId)
        {
            var s = StateOf(item.Id);
            if (s == null) return false;
            foreach (var have in s.Upgrades)
            {
                var d = item.Def.FindUpgrade(have);
                int guard = 0;
                while (d != null && guard++ < 8)
                {
                    if (d.Requires == upgradeId || d.Replaces == upgradeId) return true;
                    d = d.Requires != null ? item.Def.FindUpgrade(d.Requires) : null;
                }
            }
            return false;
        }

        public string UpgradeBlocker(ItemInstance item, string upgradeId)
        {
            var def = item.Def.FindUpgrade(upgradeId);
            if (def == null) return "Böyle bir yükseltme yok.";
            if (item.Condition == ItemCondition.Damaged) return "Hasarlı eşya yükseltilmez; önce onar.";
            if (HasUpgrade(item.Id, upgradeId)) return $"{def.DisplayName} zaten yapıldı.";
            if (Superseded(item, upgradeId)) return "Daha iyisi zaten yapıldı.";
            if (def.Requires != null && !HasUpgrade(item.Id, def.Requires))
                return $"Önce: {item.Def.FindUpgrade(def.Requires)?.DisplayName ?? def.Requires}.";
            return null;
        }

        public void ApplyUpgrade(ItemInstance item, string upgradeId)
        {
            var def = item.Def.FindUpgrade(upgradeId);
            if (def == null) return;
            var s = Ensure(item.Id);
            if (def.Replaces != null) s.Upgrades.Remove(def.Replaces);
            if (def.Requires != null && def.Replaces == null) { }   // zincir: önceki kalır (demir kuşak + kilit gibi)
            if (!s.Upgrades.Contains(upgradeId)) s.Upgrades.Add(upgradeId);
            _world.NotifyChanged();
        }

        private IEnumerable<UpgradeDef> UpgradeDefs(ItemInstance item)
        {
            var s = StateOf(item.Id);
            if (s == null) yield break;
            foreach (var u in s.Upgrades) { var d = item.Def.FindUpgrade(u); if (d != null) yield return d; }
        }

        public int ComfortBonus(ItemInstance item) { int n = 0; foreach (var d in UpgradeDefs(item)) n += d.Comfort; return n; }
        public int ExtraStorage(ItemInstance item) { int n = 0; foreach (var d in UpgradeDefs(item)) n += d.Storage; return n; }
        public double CollapseFactor(ItemInstance item) { double f = 1; foreach (var d in UpgradeDefs(item)) if (d.Collapse > 0f) f *= d.Collapse; return f; }

        /// <summary>Eşyanın güzellik eki: yükseltme, desen, solmuş saksı, koleksiyon (MoodService.BeautyBonus'a eklenir).</summary>
        public double BeautyBonus(ItemInstance item)
        {
            if (item == null) return 0;
            double b = 0;
            foreach (var d in UpgradeDefs(item)) b += d.Beauty;
            b += PatternBeauty(item);
            b += CollectionBeauty(item);
            b += VivBeauty(item);
            if (PotWilted(item)) b += -item.Def.Beauty + WiltedPotBeauty;
            else if (item.Def.LivePlant && PotGrown(item)) b += GrownPlantBeauty + (InBloom(item) ? BloomBeauty : 0);
            return b;
        }

        /// <summary>Eşyanın işe kattığı çarpanlar: yükseltme (şilte), oda türü, gece ışığı. JobRunner.ItemBonus'a eklenir.</summary>
        public ItemBonus BonusFor(ItemInstance item, InteractionDef interaction)
        {
            if (item == null || interaction == null) return ItemBonus.None;
            double speed = 1, xp = 1, restore = 1;
            foreach (var d in UpgradeDefs(item)) restore += d.Restore;
            var room = RoomOf(item);
            if (room != null && room.IsIndoor)
            {
                var (kind, tidy) = Classify(room);
                if (tidy)
                {
                    if (kind == RoomKind.Kitchen && interaction.Skill == SkillType.Cooking) { speed *= TidySpeed; xp *= TidySpeed; }
                    else if (kind == RoomKind.Workshop && interaction.IsCraft && interaction.Skill != SkillType.Cooking) speed *= TidySpeed;
                    else if (kind == RoomKind.Bedroom && interaction.Need == NeedType.Energy && interaction.Restore > 0) restore *= TidySpeed;
                }
                if (IsNight(_clock.DayFraction) && IsFineWork(interaction)) speed *= NightWorkFactor(LightLevel(room));
            }
            return new ItemBonus(speed, xp, restore);
        }

        public static ItemBonus Combine(ItemBonus a, ItemBonus b) => new ItemBonus(a.Speed * b.Speed, a.Xp * b.Xp, a.Restore * b.Restore);

        // ---- etkileşimler ----

        /// <summary>Ev bakımı işinin başlama engeli (InteractionEffects.ExtraBlocker); yoksa null.</summary>
        public string CanStart(Character c, ItemInstance item, InteractionDef i)
        {
            if (i.HomeAction == null || item == null) return null;
            switch (i.HomeAction)
            {
                case HomeActions.Candle:
                    if (!item.Def.Burns) return "Bu bir mum değil.";
                    if (item.FuelDays > item.Def.BurnDays * 0.5) return "Mum daha yanıyor.";
                    return CandleRefill(item.Def) == null ? $"Envanterde mum yok ({Name(item.Def.BurnRefill.Count > 0 ? item.Def.BurnRefill[0] : DefaultCandle).ToLowerInvariant()})." : null;
                case HomeActions.Strew:
                    if (!IsStrewable(item)) return "Ot yalnızca saz hasıra serpilir.";
                    var st = StateOf(item.Id);
                    if (st != null && st.StrewUntil - Now > 1.0) return "Otlar hâlâ taze.";
                    return StrewHerb() == null ? "Envanterde serpilecek ot yok (ot, nane, adaçayı, papatya)." : null;
                case HomeActions.Whitewash:
                {
                    var room = RoomOf(item);
                    if (room == null || !room.IsIndoor) return "Badana kapalı odada yapılır.";
                    var rs = RoomState(room);
                    if ((rs == null || rs.Soot < 1.0) && IsWhitewashed(room)) return "Duvarlar zaten ak.";
                    return null;
                }
                case HomeActions.WaterPot:
                    if (!item.Def.IsLivePot) return "Bu bir saksı değil.";
                    return PotThirsty(item) || PotWilted(item) ? null : "Saksı zaten sulu.";
                case HomeActions.HarvestPot:
                    if (!item.Def.IsLivePot) return "Bu bir saksı değil.";
                    if (PotWilted(item)) return "Saksı solmuş: önce sula.";
                    return PotReady(item) ? null : "Ot henüz yetişmedi.";
                case HomeActions.Upgrade:
                    return UpgradeBlocker(item, i.UpgradeId);
                case HomeActions.Stow:
                    return DecorSystem.IsWilted(item, _clock.Season) ? null : "Bu süs daha taze.";
                case HomeActions.Catch:
                    return CatchBlocker(item);
                case HomeActions.Mist:
                {
                    var v = VivOf(item);
                    if (v?.Habitat == null) return "Önce zemini seç (cam bahçeye yakından bak).";
                    if (v.Habitat == Salt) return v.Moisture > 70 ? "Su hâlâ taze." : _house.Count(SeaWaterItem) > 0 ? null : "Suyu tazelemek için envanterde tuz olmalı.";
                    double target = v.Habitat == Dry ? 30 : 85;
                    return v.Moisture >= target - 5 ? (v.Habitat == Dry ? "Kum yeterince nemli." : "Toprak zaten nemli.") : null;
                }
            }
            return null;
        }

        /// <summary>Ev bakımı işinin etkisi (InteractionEffects.AppliedOn).</summary>
        public void OnApplied(Character c, ItemInstance item, InteractionDef i)
        {
            if (i != null && ServantWalks) NoteServantWork(c, i);
            if (i?.HomeAction == null || item == null) return;
            switch (i.HomeAction)
            {
                case HomeActions.Candle:
                {
                    string refill = CandleRefill(item.Def);
                    if (refill != null && _house.TryTakeItem(refill)) { item.SetCandle(item.Def.BurnDays); _world.NotifyChanged(); }
                    break;
                }
                case HomeActions.Strew:
                {
                    string herb = StrewHerb();
                    if (herb != null && _house.TryTakeItem(herb)) Ensure(item.Id).StrewUntil = Now + StrewDays;
                    break;
                }
                case HomeActions.Whitewash:
                {
                    var room = RoomOf(item);
                    var rs = EnsureRoom(room);
                    if (rs != null) { rs.Soot = 0; rs.Warned = false; rs.WhitewashUntil = Now + 2 * _clock.Config.DaysPerSeason; }
                    break;
                }
                case HomeActions.WaterPot:
                    Ensure(item.Id).PotDry = 0;
                    break;
                case HomeActions.HarvestPot:
                {
                    var s = Ensure(item.Id);
                    s.PotGrowth = 0;
                    _house.AddItem(item.Def.PotHerb, 1);
                    break;
                }
                case HomeActions.Upgrade:
                    ApplyUpgrade(item, i.UpgradeId);
                    break;
                case HomeActions.Catch:
                    Catch(item);
                    break;
                case HomeActions.Mist:
                    Mist(item);
                    break;
                case HomeActions.Stow:
                    if (_world.GetItem(item.Id) != null)
                    {
                        foreach (var gone in _world.RemoveItemInstances(item.Id)) _house.AddItem(gone.Def.Id, 1, gone.Condition);
                        _world.RaiseChanged();
                    }
                    break;
            }
            c?.GainSkillXp(SkillType.Decorating, 2);
        }

        // ---- ev işleri (ev görevi ve hizmetkâr) ----

        private static InteractionDef Find(ItemDef def, Func<InteractionDef, bool> pred)
        {
            foreach (var i in def.Interactions) if (pred(i)) return i;
            return null;
        }

        /// <summary>
        /// Bakım isteyen eşyalar ve işleri (öncelik sırasıyla): çürük sepet boşalt, döküntü topla, mum değiştir, sepet doldur,
        /// ayna cilala, saksı sula ve hasat et, solan süsü kaldır, hasıra ot serp, isli odayı badanala.
        /// Başlama engeli olanlar da listelenir; görevli InteractionEffects.CanStart ile eler.
        /// </summary>
        public IEnumerable<(ItemInstance item, InteractionDef interaction)> Chores()
        {
            var list = new List<(ItemInstance, InteractionDef)>();
            foreach (var it in _world.Items)
            {
                var def = it.Def;
                if (def.IsFillDecor && it.HasContents && DecorSystem.WorstFreshness(it, _content) == Decor.Rotten)
                { var i = Find(def, x => x.CollectContents); if (i != null) list.Add((it, i)); continue; }
                if (def.Id == Decor.Debris && it.HasContents)
                { var i = Find(def, x => x.CollectContents); if (i != null) list.Add((it, i)); continue; }
                if (def.Burns && it.FuelDays <= def.BurnDays * 0.5)
                { var i = Find(def, x => x.HomeAction == HomeActions.Candle); if (i != null) list.Add((it, i)); }
                if (def.IsFillDecor && it.Contents.Count < def.FillCount && !def.IsKeepsake && def.Id != KeepsakeService.DowryChest)
                { var i = Find(def, x => x.FillDecor); if (i != null) list.Add((it, i)); }
                if (it.IsTarnished)
                { var i = Find(def, x => x.Polish); if (i != null) list.Add((it, i)); }
                if (def.IsLivePot)
                {
                    if (PotThirsty(it)) { var i = Find(def, x => x.HomeAction == HomeActions.WaterPot); if (i != null) list.Add((it, i)); }
                    else if (PotReady(it)) { var i = Find(def, x => x.HomeAction == HomeActions.HarvestPot); if (i != null) list.Add((it, i)); }
                }
                if (DecorSystem.IsWilted(it, _clock.Season))
                { var i = Find(def, x => x.HomeAction == HomeActions.Stow); if (i != null) list.Add((it, i)); }
                if (IsStrewable(it) && !IsStrewnFresh(it))
                { var i = Find(def, x => x.HomeAction == HomeActions.Strew); if (i != null) list.Add((it, i)); }
                if (def.IsVivarium && NeedsMist(it))
                { var i = Find(def, x => x.HomeAction == HomeActions.Mist); if (i != null) list.Add((it, i)); }
                if (!it.IsDecor && def.HeatOutput > 0f && SootOf(RoomOf(it)) >= SootWarnAt)
                { var i = Find(def, x => x.HomeAction == HomeActions.Whitewash); if (i != null) list.Add((it, i)); }
            }
            return list;
        }

        // ---- kayıt ----

        public HomeSave Export()
        {
            var save = new HomeSave();
            var items = new List<HomeItemSave>();
            foreach (var kv in _items)
            {
                if (kv.Value.IsEmpty) continue;
                items.Add(new HomeItemSave
                {
                    id = kv.Key, upgrades = kv.Value.Upgrades.ToArray(), pattern = kv.Value.Pattern,
                    potDry = kv.Value.PotDry, potGrowth = kv.Value.PotGrowth, strewUntil = kv.Value.StrewUntil
                });
            }
            save.items = items.ToArray();
            var rooms = new List<HomeRoomSave>();
            foreach (var kv in _rooms) rooms.Add(new HomeRoomSave { key = kv.Key, soot = kv.Value.Soot, whitewashUntil = kv.Value.WhitewashUntil, warned = kv.Value.Warned });
            save.rooms = rooms.ToArray();
            save.candlesSeen = new List<int>(_candlesSeen).ToArray();
            ExportExtras(save);
            return save;
        }

        public void Import(HomeSave save)
        {
            _items.Clear(); _rooms.Clear(); _candlesSeen.Clear();
            if (save == null) { ImportExtras(null); return; }
            if (save.items != null)
                foreach (var s in save.items)
                {
                    if (s == null) continue;
                    var st = Ensure(s.id);
                    if (s.upgrades != null) st.Upgrades.AddRange(s.upgrades);
                    st.Pattern = string.IsNullOrEmpty(s.pattern) ? null : s.pattern;
                    st.PotDry = s.potDry; st.PotGrowth = s.potGrowth; st.StrewUntil = s.strewUntil;
                }
            if (save.rooms != null)
                foreach (var r in save.rooms)
                    if (r != null && r.key != null) _rooms[r.key] = new HomeRoomState { Soot = r.soot, WhitewashUntil = r.whitewashUntil, Warned = r.warned };
            if (save.candlesSeen != null) foreach (var id in save.candlesSeen) _candlesSeen.Add(id);
            ImportExtras(save);
        }
    }
}
