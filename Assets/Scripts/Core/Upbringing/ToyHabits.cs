using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Upbringing
{
    /// <summary>Ustalaşılmış oyuncağın boşta gösterisi yaşa göre değişir (belge 30, bölüm 4).</summary>
    public enum ShowKind
    {
        /// <summary>Çocuk ve ergen: oyuncakla şov.</summary>
        Show = 0,
        /// <summary>Yetişkin: yakında çocuk varsa gösterir ve öğretir.</summary>
        Teach = 1,
        /// <summary>Yaşlı: eline alıp seyreder.</summary>
        Nostalgia = 2
    }

    /// <summary>Bir çocukluk eşyası: hangi beceriye bağlı, ustalıkta ne olur (belge 30, bölüm 3).</summary>
    public sealed class ToyDef
    {
        public string ItemId;
        public SkillType Skill;
        /// <summary>Ustalık eşiği (alışkanlık puanı).</summary>
        public double Threshold = ToyHabitSystem.DefaultThreshold;
        /// <summary>Bir oynayışta kazanılan alışkanlık (çocuk için; diğer yaşlar çarpanla).</summary>
        public double HabitPerPlay = 4;
        /// <summary>Bir oynayışta eşyanın aşınması (dayanıklılık 100'den).</summary>
        public double WearPerPlay = 0.8;
        /// <summary>Gösteri balonunda görünen kısa metin.</summary>
        public string ShowText;
        /// <summary>Gösteriyi seyreden çocuklara verilen keyif.</summary>
        public float ShowFun = 15f;
        /// <summary>Ustalıkla yetişkinliğe taşınan iz (null: yok).</summary>
        public string MarkId;
        public string MarkName;
        /// <summary>İz sahibinin bağlı beceride kazandığı ek XP oranı (0,10 = %10).</summary>
        public double MarkXpBonus = 0.10;
    }

    /// <summary>Kişisel eşya: ustalaşılınca envantere kalıcı girer; aşınır, onarılır, mirasa kalır.</summary>
    public sealed class PersonalToy
    {
        public int OwnerId;
        public string ItemId;
        public double Durability = 100;
        /// <summary>İlk sahibi (miras izi). Kendi eşyasıysa OwnerId ile aynı.</summary>
        public int OriginalOwnerId;
        public bool IsHeirloom;
        public bool IsBroken => Durability <= 0;
    }

    /// <summary>Bir karakterin bir oyuncaktaki alışkanlığı.</summary>
    public sealed class HabitRecord
    {
        public int CharacterId;
        public string ItemId;
        public double Habit;
        public bool Mastered;
        public double LastUsedDay;
    }

    /// <summary>Gösterinin sonucu (oyun katmanı animasyon ve balon için kullanır).</summary>
    public sealed class ToyShow
    {
        public Character Performer;
        public ToyDef Toy;
        public ShowKind Kind;
        public int Watchers;
        public string Text;
    }

    /// <summary>
    /// Çocukluk eşyaları ve alışkanlık sistemi (belge 30, onay 18.11.2026).
    /// Oynadıkça alışkanlık birikir; eşik geçilince eşya kişisel eşya olur ve karakter boşta onunla gösteri yapar.
    /// Uzun süre dokunulmayan ustalık yavaşça düşer ama eşiğin yarısının altına inmez ve ustalık kaybolmaz.
    /// </summary>
    public sealed class ToyHabitSystem
    {
        public const double DefaultThreshold = 100;
        /// <summary>Bu kadar gün dokunulmazsa unutma başlar.</summary>
        public const double ForgetAfterDays = 6;
        /// <summary>Unutma: gün başına eşiğin yüzdesi.</summary>
        public const double ForgetPerDay = 0.02;
        /// <summary>Ustalaşılmışta alışkanlığın inebileceği en alt oran.</summary>
        public const double ForgetFloor = 0.5;
        /// <summary>Usta yanında oynayan çocuk (öğretme) kaç kat hızlı öğrenir; usta yaşlıysa ek çarpan.</summary>
        public const double TeachFactor = 2.0, ElderTeachFactor = 1.5;
        /// <summary>Miras kalan eşyanın yeni sahibine başlangıç alışkanlığı (eşik oranı).</summary>
        public const double HeirloomStart = 0.25;

        private readonly Dictionary<string, ToyDef> _defs = new Dictionary<string, ToyDef>();
        private readonly Dictionary<(int, string), HabitRecord> _habits = new Dictionary<(int, string), HabitRecord>();
        private readonly List<PersonalToy> _personal = new List<PersonalToy>();
        private readonly Dictionary<int, List<string>> _marks = new Dictionary<int, List<string>>();

        public ToyHabitSystem(IEnumerable<ToyDef> defs = null)
        {
            foreach (var d in defs ?? DefaultToys()) _defs[d.ItemId] = d;
        }

        public event Action<Character, ToyDef> Mastered;
        public event Action<Character, PersonalToy> Broke;
        public event Action<Character, ToyDef> MarkGained;

        public IEnumerable<ToyDef> Toys => _defs.Values;
        public bool IsToy(string itemId) => itemId != null && _defs.ContainsKey(itemId);
        public ToyDef Def(string itemId) => itemId != null && _defs.TryGetValue(itemId, out var d) ? d : null;

        public HabitRecord Record(int characterId, string itemId) =>
            _habits.TryGetValue((characterId, itemId), out var r) ? r : null;

        public double HabitOf(int characterId, string itemId) => Record(characterId, itemId)?.Habit ?? 0;
        public bool IsMastered(int characterId, string itemId) => Record(characterId, itemId)?.Mastered ?? false;

        public IEnumerable<PersonalToy> PersonalOf(int characterId)
        {
            foreach (var p in _personal) if (p.OwnerId == characterId) yield return p;
        }

        public PersonalToy Personal(int characterId, string itemId)
        {
            foreach (var p in _personal) if (p.OwnerId == characterId && p.ItemId == itemId) return p;
            return null;
        }

        public IReadOnlyList<string> MarksOf(int characterId) =>
            _marks.TryGetValue(characterId, out var l) ? l : (IReadOnlyList<string>)Array.Empty<string>();

        /// <summary>Yaşa göre alışkanlık kazanım çarpanı: çocuk oynar, büyüdükçe azalır; bebek oynamaz.</summary>
        public static double AgeFactor(AgeStage s)
        {
            switch (s)
            {
                case AgeStage.Baby: return 0;
                case AgeStage.Child: return 1.0;
                case AgeStage.Teen: return 0.8;
                case AgeStage.YoungAdult: return 0.5;
                default: return 0.3;
            }
        }

        /// <summary>
        /// Bir oynayış bitti. teacher: yanında öğreten usta (o oyuncakta ustalaşmış yetişkin ya da yaşlı).
        /// Ustalık bu oynayışla açıldıysa true.
        /// </summary>
        /// <summary>Alışkanlık kazanç çarpanı (belge 45: Ebeveynlik 8 "çocukluk izi şansı" ×1,25); null ise 1.</summary>
        public Func<double> GainFactor { get; set; }

        public bool OnPlayed(Character c, string itemId, double today, Character teacher = null)
        {
            var def = Def(itemId);
            if (c == null || def == null || c.IsDead) return false;
            double gain = def.HabitPerPlay * AgeFactor(c.Stage) * (GainFactor != null ? GainFactor() : 1.0);
            if (gain <= 0) return false;
            if (teacher != null && teacher.Id != c.Id && !teacher.IsDead && IsMastered(teacher.Id, itemId))
            {
                gain *= TeachFactor;
                if (teacher.Stage == AgeStage.Elder) gain *= ElderTeachFactor;
                teacher.GainSkillXp(SkillType.Teaching, 2);
            }
            var r = GetOrAdd(c.Id, itemId);
            r.Habit += gain;
            r.LastUsedDay = today;

            var own = Personal(c.Id, itemId);
            if (own != null)
            {
                bool wasBroken = own.IsBroken;
                own.Durability = Math.Max(0, own.Durability - def.WearPerPlay);
                if (own.IsBroken && !wasBroken) Broke?.Invoke(c, own);
            }

            if (r.Mastered || r.Habit < def.Threshold) return false;
            r.Mastered = true;
            if (own == null) _personal.Add(new PersonalToy { OwnerId = c.Id, ItemId = itemId, OriginalOwnerId = c.Id });
            c.GainSkillXp(def.Skill, 20);
            Mastered?.Invoke(c, def);
            return true;
        }

        /// <summary>Kırık kişisel eşyayı onarır (dayanıklılık tam). Kırık değilse ya da yoksa false.</summary>
        public bool Repair(int characterId, string itemId)
        {
            var p = Personal(characterId, itemId);
            if (p == null || p.Durability >= 100) return false;
            p.Durability = 100;
            return true;
        }

        /// <summary>Günlük unutma: uzun süre dokunulmayan alışkanlık düşer; ustalık kalır, taban eşiğin yarısı.</summary>
        public void DailyUpdate(double today)
        {
            foreach (var r in _habits.Values)
            {
                if (today - r.LastUsedDay < ForgetAfterDays) continue;
                var def = Def(r.ItemId);
                if (def == null) continue;
                double floor = r.Mastered ? def.Threshold * ForgetFloor : 0;
                r.Habit = Math.Max(floor, r.Habit - def.Threshold * ForgetPerDay);
            }
        }

        /// <summary>
        /// Boşta gösterisi: ustalaşılmış ve kırık olmayan kişisel eşyalardan biri. Yaşa göre tür değişir:
        /// çocuk/ergen gösteri, yetişkin yakında çocuk varsa öğretme, yaşlı nostalji. Uygun eşya yoksa null.
        /// </summary>
        public ToyShow PickIdleShow(Character c, Random rng, bool childNearby = false)
        {
            if (c == null || c.IsDead || c.Stage == AgeStage.Baby) return null;
            var options = new List<PersonalToy>();
            foreach (var p in PersonalOf(c.Id))
                if (!p.IsBroken && IsMastered(c.Id, p.ItemId)) options.Add(p);
            if (options.Count == 0) return null;
            var pick = options[rng != null ? rng.Next(options.Count) : 0];
            var def = Def(pick.ItemId);
            var kind = c.Stage == AgeStage.Elder ? ShowKind.Nostalgia
                : c.Stage >= AgeStage.YoungAdult && childNearby ? ShowKind.Teach : ShowKind.Show;
            string text = kind == ShowKind.Nostalgia ? $"{c.Name} eski {ItemName(def)} eline alıp seyrediyor."
                : kind == ShowKind.Teach ? $"{c.Name} çocuklara {ItemName(def)} gösteriyor."
                : def.ShowText ?? $"{c.Name} {ItemName(def)} ile gösteri yapıyor.";
            return new ToyShow { Performer = c, Toy = def, Kind = kind, Text = text };
        }

        /// <summary>
        /// Gösteriyi uygular: seyreden çocuk ve ergenlere keyif ve moodlet; yetişkin seyirci yalnız küçük moodlet.
        /// Öğretme gösterisi seyreden çocuğa alışkanlık da kazandırır.
        /// </summary>
        public int ApplyShow(ToyShow show, IEnumerable<Character> nearby, double today)
        {
            if (show == null) return 0;
            int n = 0;
            foreach (var w in nearby)
            {
                if (w == null || w.IsDead || w.Id == show.Performer.Id || w.Stage == AgeStage.Baby && show.Kind != ShowKind.Show) continue;
                n++;
                if (w.Stage <= AgeStage.Teen)
                {
                    w.Needs.Restore(NeedType.Fun, show.Toy.ShowFun);
                    w.AddMoodlet("toy_show", $"{show.Performer.Name}'in gösterisini izledi", 2, 0.3);
                    if (show.Kind == ShowKind.Teach && w.Stage != AgeStage.Baby) OnPlayed(w, show.Toy.ItemId, today, show.Performer);
                }
                else w.AddMoodlet("toy_show", $"{show.Performer.Name}'in gösterisini izledi", 1, 0.2);
            }
            show.Watchers = n;
            if (n > 0) show.Performer.GainSkillXp(SkillType.Charm, 1 + n);
            return n;
        }

        /// <summary>
        /// Başka bir çocuk eşyayı ister. Ödünç verirse istekli sevinir, sahibi gönül kazanır;
        /// vermezse istekli kıskanır. Eşya sahibinde kalır.
        /// </summary>
        public void AskToBorrow(Character owner, Character asker, string itemId, bool lend, double today)
        {
            if (owner == null || asker == null || Personal(owner.Id, itemId) == null) return;
            var def = Def(itemId);
            if (lend)
            {
                asker.AddMoodlet("toy_borrowed", $"{owner.Name} oyuncağını ödünç verdi", 2, 0.5);
                owner.AddMoodlet("toy_lent", "Oyuncağını paylaştı", 1, 0.3);
                owner.GainSkillXp(SkillType.Charm, 3);
                OnPlayed(asker, itemId, today);
            }
            else asker.AddMoodlet("toy_envy", $"{owner.Name}'in {ItemName(def)} oyuncağını kıskandı", -2, 0.5);
        }

        /// <summary>
        /// Genç yetişkinliğe geçiş: ustalaşılmış ve izi olan oyuncaklar kalıcı iz bırakır
        /// (bağlı beceride ek XP; ör. kaval → "Müzisyen ruhlu").
        /// </summary>
        public List<ToyDef> OnGrownUp(Character c)
        {
            var gained = new List<ToyDef>();
            if (c == null) return gained;
            foreach (var r in _habits.Values)
            {
                if (r.CharacterId != c.Id || !r.Mastered) continue;
                var def = Def(r.ItemId);
                if (def?.MarkId == null) continue;
                if (!_marks.TryGetValue(c.Id, out var list)) _marks[c.Id] = list = new List<string>();
                if (list.Contains(def.MarkId)) continue;
                list.Add(def.MarkId);
                gained.Add(def);
                MarkGained?.Invoke(c, def);
            }
            return gained;
        }

        /// <summary>İzlerden gelen beceri XP çarpanı (1 = etkisiz).</summary>
        public double XpFactor(int characterId, SkillType skill)
        {
            if (!_marks.TryGetValue(characterId, out var list)) return 1.0;
            double f = 1.0;
            foreach (var d in _defs.Values)
                if (d.Skill == skill && d.MarkId != null && list.Contains(d.MarkId)) f += d.MarkXpBonus;
            return f;
        }

        /// <summary>
        /// Ölüm: kişisel eşyalar mirasçıya geçer (yadigâr); mirasçı eşiğin dörtte biriyle başlar.
        /// Mirasçı yoksa eşyalar düşer. Geçen eşyaların sayısını döner.
        /// </summary>
        public int Inherit(Character deceased, Character heir, double today)
        {
            if (deceased == null) return 0;
            int n = 0;
            for (int i = _personal.Count - 1; i >= 0; i--)
            {
                var p = _personal[i];
                if (p.OwnerId != deceased.Id) continue;
                if (heir == null || heir.IsDead || Personal(heir.Id, p.ItemId) != null) { _personal.RemoveAt(i); continue; }
                p.OwnerId = heir.Id;
                p.IsHeirloom = true;
                var def = Def(p.ItemId);
                var r = GetOrAdd(heir.Id, p.ItemId);
                r.Habit = Math.Max(r.Habit, (def?.Threshold ?? DefaultThreshold) * HeirloomStart);
                r.LastUsedDay = today;
                n++;
            }
            return n;
        }

        private HabitRecord GetOrAdd(int id, string itemId)
        {
            if (!_habits.TryGetValue((id, itemId), out var r))
                _habits[(id, itemId)] = r = new HabitRecord { CharacterId = id, ItemId = itemId };
            return r;
        }

        private static string ItemName(ToyDef d) => d == null ? "oyuncağı" : ToyNames.Name(d.ItemId);

        // ---- kayıt ----

        public ToyHabitSave Export()
        {
            var s = new ToyHabitSave();
            var h = new List<HabitSaveRow>();
            foreach (var r in _habits.Values)
                h.Add(new HabitSaveRow { characterId = r.CharacterId, item = r.ItemId, habit = r.Habit, mastered = r.Mastered, lastUsedDay = r.LastUsedDay });
            s.habits = h.ToArray();
            var p = new List<PersonalToySave>();
            foreach (var t in _personal)
                p.Add(new PersonalToySave { ownerId = t.OwnerId, item = t.ItemId, durability = t.Durability, originalOwnerId = t.OriginalOwnerId, heirloom = t.IsHeirloom });
            s.personal = p.ToArray();
            var m = new List<MarkSaveRow>();
            foreach (var kv in _marks) m.Add(new MarkSaveRow { characterId = kv.Key, marks = kv.Value.ToArray() });
            s.marks = m.ToArray();
            return s;
        }

        public void Import(ToyHabitSave s)
        {
            _habits.Clear(); _personal.Clear(); _marks.Clear();
            if (s == null) return;
            if (s.habits != null)
                foreach (var r in s.habits)
                    if (r != null && r.item != null)
                        _habits[(r.characterId, r.item)] = new HabitRecord { CharacterId = r.characterId, ItemId = r.item, Habit = r.habit, Mastered = r.mastered, LastUsedDay = r.lastUsedDay };
            if (s.personal != null)
                foreach (var t in s.personal)
                    if (t != null && t.item != null)
                        _personal.Add(new PersonalToy { OwnerId = t.ownerId, ItemId = t.item, Durability = t.durability, OriginalOwnerId = t.originalOwnerId, IsHeirloom = t.heirloom });
            if (s.marks != null)
                foreach (var m in s.marks)
                    if (m?.marks != null) _marks[m.characterId] = new List<string>(m.marks);
        }

        /// <summary>Belge 30, bölüm 3: 14 çocukluk eşyası (içerikteki eşya kimlikleriyle).</summary>
        public static List<ToyDef> DefaultToys() => new List<ToyDef>
        {
            new ToyDef { ItemId = "item.spinning_top", Skill = SkillType.Charm, ShowText = "Avucunda topaç çeviriyor.", MarkId = "mark.nimble", MarkName = "Çevik el" },
            new ToyDef { ItemId = "item.dice_knucklebones", Skill = SkillType.Trade, ShowText = "Aşıkları havaya atıp yakalıyor.", MarkId = "mark.lucky_hand", MarkName = "Şanslı el" },
            new ToyDef { ItemId = "item.wooden_whistle", Skill = SkillType.Music, ShowText = "Kaval çalıyor.", ShowFun = 20f, MarkId = "mark.music_soul", MarkName = "Müzisyen ruhlu" },
            new ToyDef { ItemId = "item.slingshot", Skill = SkillType.Hunting, ShowText = "Sapanla taşlara nişan alıyor.", MarkId = "mark.keen_eye", MarkName = "Keskin göz" },
            new ToyDef { ItemId = "item.toy_sword_wall", Skill = SkillType.Athletics, ShowText = "Tahta kılıçla gölge dövüşü yapıyor.", MarkId = "mark.brave", MarkName = "Yürekli" },
            new ToyDef { ItemId = "item.rag_doll", Skill = SkillType.Parenting, ShowText = "Bebeğini sallayıp onunla konuşuyor.", MarkId = "mark.caring", MarkName = "Şefkatli" },
            new ToyDef { ItemId = "item.jump_rope", Skill = SkillType.Athletics, ShowText = "İp atlıyor.", MarkId = "mark.tireless", MarkName = "Yorulmaz" },
            new ToyDef { ItemId = "item.hoop", Skill = SkillType.Athletics, ShowText = "Çubukla çember sürüyor.", MarkId = "mark.swift", MarkName = "Ayağı tez" },
            new ToyDef { ItemId = "item.whittling_knife", Skill = SkillType.Carpentry, ShowText = "Bir tahta parçası yontuyor.", WearPerPlay = 1.2, MarkId = "mark.whittler", MarkName = "Yontucu" },
            new ToyDef { ItemId = "item.marbles", Skill = SkillType.Charm, ShowText = "Yere çizip bilye oynuyor.", MarkId = "mark.friendly", MarkName = "Arkadaş canlısı" },
            new ToyDef { ItemId = "item.book", Skill = SkillType.Literacy, ShowText = "Bir köşede kitap okuyor.", ShowFun = 8f, MarkId = "mark.storyteller", MarkName = "Hikâyeci" },
            new ToyDef { ItemId = "item.kite", Skill = SkillType.WeatherLore, ShowText = "Uçurtma uçuruyor.", ShowFun = 18f, MarkId = "mark.wind_reader", MarkName = "Rüzgâr okuyan" },
            new ToyDef { ItemId = "item.morris_board", Skill = SkillType.Stewardship, ShowText = "Dokuz taş tahtasında taş diziyor.", MarkId = "mark.sharp_mind", MarkName = "Keskin akıl" },
            new ToyDef { ItemId = "item.juggling_balls", Skill = SkillType.Music, ShowText = "Toplarla jonglörlük yapıyor.", ShowFun = 20f, MarkId = "mark.performer", MarkName = "Gösterici" }
        };
    }

    /// <summary>Oyuncak adları (içerik yüklenmeden de metin üretmek için).</summary>
    public static class ToyNames
    {
        public static Func<string, string> Lookup;

        public static string Name(string itemId)
        {
            var n = Lookup?.Invoke(itemId);
            if (!string.IsNullOrEmpty(n)) return n.ToLowerInvariant();
            switch (itemId)
            {
                case "item.spinning_top": return "topaç";
                case "item.wooden_whistle": return "kaval";
                case "item.rag_doll": return "bez bebek";
                default: return "oyuncak";
            }
        }
    }

    [Serializable] public sealed class HabitSaveRow { public int characterId; public string item; public double habit, lastUsedDay; public bool mastered; }
    [Serializable] public sealed class PersonalToySave { public int ownerId, originalOwnerId; public string item; public double durability; public bool heirloom; }
    [Serializable] public sealed class MarkSaveRow { public int characterId; public string[] marks; }
    [Serializable] public sealed class ToyHabitSave { public HabitSaveRow[] habits; public PersonalToySave[] personal; public MarkSaveRow[] marks; }
}
