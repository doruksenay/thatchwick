using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Cooking
{
    [Flags]
    public enum SlaughterWarning { None = 0, Pregnant = 1, Pedigree = 2, LastMale = 4, Loved = 8, Old = 16 }

    /// <summary>Kesim ekranının bir satırı (belge 60b §2.2).</summary>
    public sealed class SlaughterRow
    {
        public Animal Animal;
        /// <summary>Kesilemiyorsa nedeni (dost, yavru, eti yok); kesilebilirse null.</summary>
        public string Blocked;
        public SlaughterWarning Warnings;
        public bool Marked;
        /// <summary>Yaş evresi: 0 yavru, 1 genç, 2 yetişkin, 3 yaşlı.</summary>
        public int AgeStage;
        public string MeatId; public int MeatCount; public string HideId; public int HideCount;
        public List<ByproductDef> Byproducts = new List<ByproductDef>();
        public int ExpectedQuality;
    }

    /// <summary>
    /// Mezbaha (belge 60a §2, 60b §2; kararlar Z3, M1-M4):
    /// <list type="bullet">
    /// <item>Kesim, kesim kancası olan kapalı odada yapılır ve görünmez. Hayvanı oyuncu seçer (kesim listesi); görevli ve
    /// kesen kişi kendi başına hayvan seçmez.</item>
    /// <item>Et, deri ve yan ürünler mezbahanın deposuna düşer; "Depoyu boşalt" ile kilere taşınır. Soğutma yoksa et
    /// depoda çabuk bozulur.</item>
    /// <item>Mezbaha yoksa hayvan pazarında kestirilir: ertesi sabah <b>yalnızca et</b> gelir; deri ve öbür parçalar
    /// kasapta kalır.</item>
    /// <item>Kümes hayvanı da aynı kuralla kesilir. Dost hayvan, can yoldaşı ve yavru kesilemez.</item>
    /// </list>
    /// </summary>
    public sealed class Slaughterhouse
    {
        public const string HookTag = "slaughterhook", StoreTag = "meatstore", CoolerTag = "meatcooler";
        public const string SlaughterAction = "slaughter", CollectAction = "collectMeat";
        /// <summary>Depoda çiğ etin dayanma süresi (gün): soğutmasız ve buz teknesiyle.</summary>
        public const double DepotDays = 1.0, CooledDepotDays = 3.0;
        /// <summary>Genç hayvan: erişkinliğin bu katına kadar (şirden mayası).</summary>
        public const double YoungFactor = 2.0;
        public const double OldFraction = 0.8;

        private AnimalSystem _animals;
        /// <summary>Hayvan sistemi sonradan bağlanır (LifeSystems.Attach).</summary>
        public AnimalSystem Animals { get => _animals; set => _animals = value; }
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly Random _rng;
        private readonly List<int> _list = new List<int>();
        private readonly List<(ItemKey key, int count, double at)> _depot = new List<(ItemKey, int, double)>();
        private readonly List<(long day, string item, int count, int tier)> _atMarket = new List<(long, string, int, int)>();

        public Slaughterhouse(AnimalSystem animals, Household house, ContentDatabase content, int seed = 60)
        {
            _animals = animals; _house = house; _content = content; _rng = new Random(seed);
        }

        public Func<double> Now { get; set; } = () => 0;
        public Func<LocText, string> Text { get; set; } = t => t.Literal ?? t.Key ?? "";
        /// <summary>Kanca bir mezbaha odasında mı (kapalı oda).</summary>
        public Func<ItemInstance, bool> InRoom { get; set; }
        /// <summary>Kancanın odasında buz teknesi var mı.</summary>
        public Func<bool> Cooled { get; set; }
        /// <summary>Hanenin kurulu mezbahası var mı (arayüz: sol sütun).</summary>
        public Func<bool> HasSlaughterhouse { get; set; }
        /// <summary>Can yoldaşı ya da çocuğun sevdiği hayvan mı.</summary>
        public Func<Animal, bool> IsCompanion { get; set; }

        public event Action<Character, Animal, List<(string item, int count)>> Slaughtered;
        public event Action<LocText> Notice;
        /// <summary>Kesim sürüyor: Game mezbaha odasını kapalı çizer (görünmezlik).</summary>
        public event Action<ItemInstance, bool> DoorClosed;

        public IReadOnlyList<int> List => _list;
        public bool IsMarked(Animal a) => a != null && _list.Contains(a.Id);
        public int DepotCount { get { int n = 0; foreach (var e in _depot) n += e.count; return n; } }
        public IEnumerable<(ItemKey key, int count)> Depot => _depot.Select(e => (e.key, e.count));

        /// <summary>Neden kesilemez; kesilebilirse null.</summary>
        public string Blocked(Animal a)
        {
            if (a == null || a.IsDead) return Text(LocText.Of("cook.sl.block.gone"));
            if (a.Species.Meat == null) return Text(LocText.Of("cook.sl.block.no_meat"));
            if (!a.IsMature) return Text(LocText.Of("cook.sl.block.young"));
            if (a.Friendship >= AnimalSystem.SpareFriendship) return Text(LocText.Of("cook.sl.block.friend"));
            if (IsCompanion != null && IsCompanion(a)) return Text(LocText.Of("cook.sl.block.companion"));
            if (a.RiderId != 0) return Text(LocText.Of("cook.sl.block.ridden"));
            return null;
        }

        public bool Mark(Animal a, bool on)
        {
            if (a == null) return false;
            if (!on) return _list.Remove(a.Id);
            if (Blocked(a) != null || _list.Contains(a.Id)) return false;
            _list.Add(a.Id);
            return true;
        }

        private Animal FindAnimal(int id) { if (_animals == null) return null; foreach (var a in _animals.Animals) if (a.Id == id) return a; return null; }

        /// <summary>Listedeki sıradaki kesilebilir hayvan; geçersizler listeden düşer.</summary>
        public Animal Next()
        {
            for (int k = 0; k < _list.Count; k++)
            {
                var a = FindAnimal(_list[k]);
                if (a != null && Blocked(a) == null) return a;
                _list.RemoveAt(k--);
            }
            return null;
        }

        public bool IsYoung(Animal a) => a.AgeDays < a.Species.MatureDays * YoungFactor;

        public SlaughterRow Row(Animal a)
        {
            var row = new SlaughterRow { Animal = a, Blocked = Blocked(a), Marked = IsMarked(a) };
            double frac = a.LifespanDays > 0 ? a.AgeDays / a.LifespanDays : 0;
            row.AgeStage = !a.IsMature ? 0 : frac >= OldFraction ? 3 : IsYoung(a) ? 1 : 2;
            if (a.IsPregnant) row.Warnings |= SlaughterWarning.Pregnant;
            if ((int)a.Lineage > 0 || a.LegendaryTitle != null) row.Warnings |= SlaughterWarning.Pedigree;
            if (row.AgeStage == 3) row.Warnings |= SlaughterWarning.Old;
            if (a.Friendship >= 40f) row.Warnings |= SlaughterWarning.Loved;
            if (!a.IsFemale && !a.Species.MaleOnly)
            {
                bool otherMale = false;
                foreach (var o in _animals.Animals) if (o != a && !o.IsDead && o.Species == a.Species && !o.IsFemale && o.IsMature) { otherMale = true; break; }
                if (!otherMale) row.Warnings |= SlaughterWarning.LastMale;
            }
            row.MeatId = a.Species.Meat; row.MeatCount = Math.Max(1, a.Species.MeatCount);
            row.HideId = a.Species.Hide; row.HideCount = a.Species.Hide != null ? Math.Max(1, a.Species.HideCount) : 0;
            foreach (var b in a.Species.Byproducts) if (!b.YoungOnly || IsYoung(a)) row.Byproducts.Add(b);
            row.ExpectedQuality = _animals.QualityOf(a);
            return row;
        }

        /// <summary>Kesim ekranı: eti olan bütün hayvanlar; işaretliler ve kesilebilirler önce, sonra yaşa göre.</summary>
        public List<SlaughterRow> Rows()
        {
            var rows = new List<SlaughterRow>();
            if (_animals == null) return rows;
            foreach (var a in _animals.Animals) if (!a.IsDead && a.Species.Meat != null) rows.Add(Row(a));
            return rows.OrderBy(r => r.Blocked != null).ThenByDescending(r => r.AgeStage).ThenBy(r => r.Animal.Friendship).ToList();
        }

        // ---- kendi mezbahan ----

        public string HookBlocker(Character c, ItemInstance hook, InteractionDef i)
        {
            if (i.MealAction == CollectAction) return DepotCount == 0 ? Text(LocText.Of("cook.sl.block.depot_empty")) : null;
            if (i.MealAction != SlaughterAction) return null;
            if (c.Stage < AgeStage.YoungAdult) return Text(LocText.Of("cook.sl.block.child"));
            if (InRoom != null && !InRoom(hook)) return Text(LocText.Of("cook.sl.block.no_room"));
            if (_list.Count == 0) return Text(LocText.Of("cook.sl.block.list_empty"));
            return Next() == null ? Text(LocText.Of("cook.sl.block.list_empty")) : null;
        }

        /// <summary>İş başladı ya da bitti: oda kapalı çizilsin diye haber verir.</summary>
        public void SetBusy(ItemInstance hook, bool busy) => DoorClosed?.Invoke(hook, busy);

        /// <summary>InteractionEffects.AppliedOn: kesim ya da depoyu boşaltma.</summary>
        public void OnApplied(Character c, ItemInstance hook, InteractionDef i)
        {
            if (i.MealAction == CollectAction) { CollectDepot(); return; }
            if (i.MealAction != SlaughterAction) return;
            var a = Next();
            if (a == null) return;
            var row = Row(a);
            int tier = row.ExpectedQuality;
            int level = c.Skills.GetLevel(SkillType.AnimalHusbandry);
            _list.Remove(a.Id);
            _animals.RemoveForSlaughter(a);
            var got = new List<(string item, int count)>();
            void Put(string id, int count, bool graded)
            {
                if (id == null || count <= 0) return;
                bool hasQ = graded && _content.Items.TryGet(id, out var d) && d.HasQuality;
                for (int u = 0; u < count; u++) AddDepot(new ItemKey(id, hasQ ? ItemQuality.Spread(tier, _rng) : 0), 1);
                got.Add((id, count));
            }
            Put(row.MeatId, row.MeatCount, true);
            // Acemi deriyi yırtar, sakatatı ziyan eder: yan ürün şansı Hayvancılık seviyesiyle artar (seviye 9'da tam).
            double care = Math.Min(1.0, 0.55 + 0.05 * level);
            int hides = 0;
            for (int u = 0; u < row.HideCount; u++) if (u == 0 || _rng.NextDouble() < care) hides++;
            Put(row.HideId, hides, true);
            foreach (var b in row.Byproducts)
            {
                int n = 0;
                for (int u = 0; u < b.Count; u++) if (_rng.NextDouble() < b.Chance * care) n++;
                Put(b.ItemId, n, false);
            }
            c.AddMoodlet(Mood.MoodCatalog.Slaughtered);
            c.GainSkillXp(SkillType.AnimalHusbandry, 8f);
            Slaughtered?.Invoke(c, a, got);
        }

        private void AddDepot(ItemKey key, int count)
        {
            double now = Now();
            for (int k = 0; k < _depot.Count; k++)
                if (_depot[k].key.Equals(key) && now - _depot[k].at < 0.05) { _depot[k] = (key, _depot[k].count + count, _depot[k].at); return; }
            _depot.Add((key, count, now));
        }

        private int CollectDepot()
        {
            int n = 0;
            foreach (var e in _depot) { _house.Add(e.key, e.count); n += e.count; }
            _depot.Clear();
            return n;
        }

        /// <summary>Sürekli: depoda bekleyen çiğ et soğutma yoksa bozulur.</summary>
        public void Tick()
        {
            if (_depot.Count == 0) return;
            double now = Now(), limit = Cooled != null && Cooled() ? CooledDepotDays : DepotDays;
            int lost = 0;
            for (int k = _depot.Count - 1; k >= 0; k--)
            {
                var e = _depot[k];
                if (now - e.at < limit || !_content.Items.TryGet(e.key.Id, out var d) || d.SpoilDays <= 0f) continue;
                lost += e.count;
                _depot.RemoveAt(k);
            }
            if (lost > 0) Notice?.Invoke(LocText.Of("cook.sl.n.depot_spoiled", ("n", lost)));
        }

        // ---- hayvan pazarında kestir ----

        /// <summary>İşaretli hayvanları pazara gönderir: ertesi sabah yalnızca et gelir. Gönderilen sayısı döner.</summary>
        public int SendListToMarket(long today)
        {
            int n = 0;
            while (true)
            {
                var a = Next();
                if (a == null) break;
                _list.Remove(a.Id);
                _atMarket.Add((today + 1, a.Species.Meat, Math.Max(1, a.Species.MeatCount), _animals.QualityOf(a)));
                _animals.RemoveForSlaughter(a);
                n++;
            }
            return n;
        }

        public int PendingAtMarket => _atMarket.Count;

        public void OnNewDay(long day)
        {
            int meat = 0;
            for (int k = _atMarket.Count - 1; k >= 0; k--)
            {
                var e = _atMarket[k];
                if (e.day > day) continue;
                bool graded = _content.Items.TryGet(e.item, out var d) && d.HasQuality;
                for (int u = 0; u < e.count; u++) _house.AddItem(e.item, 1, graded ? ItemQuality.Spread(e.tier, _rng) : 0);
                meat += e.count;
                _atMarket.RemoveAt(k);
            }
            if (meat > 0) Notice?.Invoke(LocText.Of("cook.sl.n.market_meat", ("n", meat)));
        }

        // ---- kayıt: l:<id>,… | d:<eşya>:<kalite>:<adet>:<an> | m:<gün>:<eşya>:<adet>:<kademe> ----

        public string[] Export()
        {
            var l = new List<string>();
            string I(long v) => v.ToString(CultureInfo.InvariantCulture);
            if (_list.Count > 0) l.Add("l:" + string.Join(",", _list.Select(x => I(x))));
            foreach (var e in _depot) l.Add(string.Join(":", "d", e.key.Id, I(e.key.Quality), I(e.count), e.at.ToString("R", CultureInfo.InvariantCulture)));
            foreach (var e in _atMarket) l.Add(string.Join(":", "m", I(e.day), e.item, I(e.count), I(e.tier)));
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _list.Clear(); _depot.Clear(); _atMarket.Clear();
            if (data == null) return;
            foreach (var line in data)
            {
                var p = (line ?? "").Split(':');
                if (p[0] == "l" && p.Length == 2) { foreach (var s in p[1].Split(',')) if (int.TryParse(s, out int id)) _list.Add(id); }
                else if (p[0] == "d" && p.Length == 5 && int.TryParse(p[2], out int q) && int.TryParse(p[3], out int n)
                    && double.TryParse(p[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double at)) _depot.Add((new ItemKey(p[1], q), n, at));
                else if (p[0] == "m" && p.Length == 5 && long.TryParse(p[1], out long day) && int.TryParse(p[3], out int c) && int.TryParse(p[4], out int t))
                    _atMarket.Add((day, p[2], c, t));
            }
        }
    }
}
