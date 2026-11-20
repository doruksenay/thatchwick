using System;
using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Life
{
    // Easter egg'ler ve "Tuhaflıklar" (Project belgesi 55; kullanıcı 20.11.2026).
    // Ortak kural: hiçbir easter egg kalıcı kayıp vermez. Olumsuz moodlet kısa sürer, yakınlık ve itibar düşmez.

    /// <summary>Tuhaflık ve başarım kimlikleri. Görülmeyen tuhaflık Doğa Defteri'nde "???" olarak durur.</summary>
    public static class OddityIds
    {
        public const string AnimalTalk = "oddity.animal_talk";
        public const string AnimalStare = "oddity.animal_stare";
        public const string TalkNickname = "oddity.talk_nickname";
        public const string FoundSword = "oddity.found_sword";
        public const string SwordBuried = "oddity.sword_buried";
        public const string SwordPloughshare = "oddity.sword_ploughshare";
        public const string SwordOnWall = "oddity.sword_on_wall";
        public const string HelmetNest = "oddity.helmet_nest";
        public const string BrokenShield = "oddity.broken_shield";
        public const string FrogKiss = "oddity.frog_kiss";
        public const string FrogCrown = "oddity.frog_crown";
        public const string TooManyHellos = "oddity.too_many_hellos";
        public const string FortuneFourthWall = "oddity.fortune_fourth_wall";

        // Küçük easter egg'ler (belge 55 §1, §2, §2b); olay kimlikleri OddityEvents'te.
        public const string JohanWick = "oddity.johan_wick", HorselessKnight = "oddity.horseless_knight", Crier = "oddity.crier",
            ScreamingGoat = "oddity.screaming_goat", CatGift = "oddity.cat_gift", AlexDecor = "oddity.alex_decor",
            HoneyThief = "oddity.honey_thief", ServantCaught = "oddity.servant_caught", AprilFirst = "oddity.april_first",
            Rumor = "oddity.rumor", NameTwin = "oddity.name_twin", NameSurprise = "oddity.name_surprise",
            AbsentElder = "oddity.absent_elder", SameOutfit = "oddity.same_outfit", VillageMemory = "oddity.village_memory",
            OldBoot = "oddity.old_boot", MessageBottle = "oddity.message_bottle", FishTale = "oddity.fish_tale",
            MineTablet = "oddity.mine_tablet", SwordInStone = "oddity.sword_in_stone", TopShow = "oddity.top_show",
            NeverStoppingTop = "oddity.never_stopping_top", OxStare = "oddity.ox_stare", BoxCat = "oddity.box_cat",
            JealousDog = "oddity.jealous_dog", DogGift = "oddity.dog_gift", Howl = "oddity.howl",
            CountingSheep = "oddity.counting_sheep", TheUsual = "oddity.the_usual", Regift = "oddity.regift",
            Scarecrow = "oddity.scarecrow", ChickenRevenge = "oddity.chicken_revenge", CrowGift = "oddity.crow_gift",
            OddBequest = "oddity.odd_bequest";

        /// <summary>Olay kimliğinden tuhaflık kimliği ("johan_wick" → "oddity.johan_wick").</summary>
        public static string Of(string eventId) => "oddity." + eventId;

        /// <summary>Sayfada gösterilen tuhaflıklar (sıra sabit; yenisi sona eklenir).</summary>
        public static readonly string[] All =
        {
            AnimalStare, AnimalTalk, TalkNickname, FoundSword, SwordBuried, SwordPloughshare, SwordOnWall,
            HelmetNest, BrokenShield, FrogKiss, FrogCrown, TooManyHellos, FortuneFourthWall,
            JohanWick, HorselessKnight, Crier, ScreamingGoat, CatGift, AlexDecor, HoneyThief, ServantCaught, AprilFirst,
            Rumor, NameTwin, NameSurprise, AbsentElder, SameOutfit, VillageMemory, OldBoot, MessageBottle, FishTale,
            MineTablet, SwordInStone, TopShow, NeverStoppingTop, OxStare, BoxCat, JealousDog, DogGift, Howl,
            CountingSheep, TheUsual, Regift, Scarecrow, ChickenRevenge, CrowGift, OddBequest
        };

        public const string AchAnimalSpoke = "ach.animal_spoke";
        public const string AchRunsInFamily = "ach.runs_in_family";
        public const string AchQuestionTheSword = "ach.question_the_sword";

        public static readonly string[] Achievements = { AchAnimalSpoke, AchRunsInFamily, AchQuestionTheSword };

        public static bool IsAchievement(string id) => id != null && id.StartsWith("ach.", StringComparison.Ordinal);
    }

    /// <summary>
    /// Görülen tuhaflıklar ve kazanılan başarımlar. Başarım sistemi (yol haritası 21) gelene kadar başarımlar da burada
    /// tutulur; platform başarımına bağlamak oyun katmanının işidir (<see cref="Discovered"/>).
    /// </summary>
    public sealed class OddityBook
    {
        private readonly HashSet<string> _seen = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>İlk kez görülen tuhaflık ya da kazanılan başarım.</summary>
        public event Action<string> Discovered;

        public bool Has(string id) => id != null && _seen.Contains(id);

        /// <summary>İlk görülüşte true döner ve <see cref="Discovered"/> çağrılır.</summary>
        public bool Mark(string id)
        {
            if (string.IsNullOrEmpty(id) || !_seen.Add(id)) return false;
            Discovered?.Invoke(id);
            return true;
        }

        public int SeenCount => OddityIds.All.Count(Has);
        public static int Total => OddityIds.All.Length;
        public int AchievementCount => OddityIds.Achievements.Count(Has);
        /// <summary>Sayfa ilk tuhaflıkla birlikte açılır.</summary>
        public bool PageOpen => SeenCount > 0;

        public string[] Export() => _seen.OrderBy(s => s, StringComparer.Ordinal).ToArray();

        public void Import(string[] ids)
        {
            _seen.Clear();
            if (ids == null) return;
            foreach (var id in ids) if (!string.IsNullOrEmpty(id)) _seen.Add(id);
        }
    }

    // ------------------------------------------------------------------------------------------------------------
    // Hayvanla konuşma (belge 55 §3)
    // ------------------------------------------------------------------------------------------------------------

    /// <summary>Hayvanın derdi: hayvanın gerçek durumundan seçilir, oyuncuya işe yarar bilgi verir.</summary>
    public enum AnimalComplaint
    {
        None = 0,      // "Aslında şikâyetim yok. Sadece biri sorsun istedim."
        Sick = 1,
        Cold = 2,
        Hungry = 3,
        Draft = 4,     // barınak konforu düşük
        Lonely = 5,    // türünden başka hayvan yok
        LostYoung = 6, // yavrusu satıldı ya da kesildi: "Kuzumu nereye götürdünüz?"
        SameFeed = 7   // art arda hep aynı yem: "Yine mi saman?"
    }

    [Serializable]
    public sealed class AnimalTalkSave
    {
        public int characterId, motherId, fatherId, generation, animalId;
        public string name, houseKey, animalName, speciesId;
        public double day;
        public int complaint;
        /// <summary>Dert giderildi mi (ya da süresi geçti mi); true ise bir daha bakılmaz.</summary>
        public bool closed;
        public int[] told;
        public int disbelievers;
        public double nicknameUntil = -1;
    }

    /// <summary>Bir karakterin "hayvan benimle konuştu" anısı. Karakter ölse de kalır (aile hikâyesi olur).</summary>
    public sealed class AnimalTalkRecord
    {
        public int CharacterId, MotherId, FatherId, Generation, AnimalId;
        public string Name, HouseKey, AnimalName, SpeciesId;
        public double Day;
        public AnimalComplaint Complaint;
        public bool Closed;
        public readonly HashSet<int> Told = new HashSet<int>();
        public int Disbelievers;
        public double NicknameUntil = -1;
    }

    public sealed class AnimalTalkResult
    {
        public bool Spoke;
        public AnimalComplaint Complaint;
        /// <summary>Oyuncuya gösterilecek satır (hayvanın sözü ya da "bakıyor").</summary>
        public LocText Line;
    }

    public enum TellOutcome
    {
        /// <summary>Dinleyen de yaşamış.</summary>
        Shared = 0,
        /// <summary>Dinleyenin ailesinden biri yaşamış: "bizde de oldu".</summary>
        FamilyKnows = 1,
        /// <summary>Çocuk her zaman inanır.</summary>
        ChildBelieves = 2,
        Disbelief = 3
    }

    public sealed class TellResult
    {
        public TellOutcome Outcome;
        public LocText Line;
        /// <summary>Bu anlatışla köyde lakap takıldı mı.</summary>
        public bool NicknameGiven;
    }

    /// <summary>
    /// Gönül kazanma 9+: evcil hayvanla "insan gibi konuşmayı dene". Çok çok nadiren hayvan derdini insan diliyle
    /// anlatır; karakter başına ömürde bir kez. Yaşayan bunu başkasına anlatabilir: dinleyen de yaşadıysa ya da
    /// ailesinden biri yaşadıysa inanır, yoksa inanmaz (çocuklar hep inanır).
    /// </summary>
    public sealed class AnimalTalkService
    {
        public const int CharmLevel = 9;
        /// <summary>Deneme başına şans (belge 55 §3.1; denemeyle ayarlanır).</summary>
        public const double BaseChance = 1.0 / 250.0;
        /// <summary>Can yoldaşı ya da dost hayvanda çarpan.</summary>
        public const double CompanionFactor = 3.0;
        /// <summary>Dert bu kadar gün içinde giderilirse "Dert ortağı".</summary>
        public const double ConfidantDays = 3.0;
        public const float ConfidantFriendship = 10f;
        public const int DisbelieversForNickname = 3;
        public const int SharedAffection = 15, FamilyAffection = 8, ChildAffection = 5;
        public const float ChildFun = 15f;
        public const int AncestorDepth = 4;
        /// <summary>Oyuncu hanesinin anahtarı (köylü hanelerininki NeighborHouse kimliğinden gelir).</summary>
        public const string PlayerHouse = "player";

        private readonly Dictionary<int, AnimalTalkRecord> _records = new Dictionary<int, AnimalTalkRecord>();
        private readonly HashSet<(int who, int animal, long day)> _tried = new HashSet<(int, int, long)>();
        private readonly Random _rng;

        public OddityBook Book { get; set; }
        public Func<double> Today { get; set; } = () => 0;
        public Func<int, Character> Find { get; set; } = _ => null;
        /// <summary>Karakterin hanesi: oyuncu hanesi için <see cref="PlayerHouse"/>, köylü için hane anahtarı; bilinmiyorsa null.</summary>
        public Func<Character, string> HouseKeyOf { get; set; } = c => c != null && !c.IsVillager ? PlayerHouse : null;
        public Func<Character, Animal, bool> IsCompanion { get; set; } = (c, a) => false;
        public Func<IEnumerable<Animal>> Animals { get; set; } = () => Enumerable.Empty<Animal>();
        public Action<int, int, int> AddAffection { get; set; }
        /// <summary>Lakabın süresi (gün): bir mevsim.</summary>
        public Func<double> NicknameDays { get; set; } = () => 28;
        /// <summary>Test ve hata ayıklama: bir sonraki deneme kesin tutar.</summary>
        public bool ForceNext { get; set; }

        /// <summary>(karakter, hayvan, sonuç): bildirim ve kronik için.</summary>
        public event Action<Character, Animal, AnimalTalkResult> Tried;
        /// <summary>Dert giderildi: (karakter kaydı, hayvan).</summary>
        public event Action<AnimalTalkRecord, Animal> Confided;

        public AnimalTalkService(int seed = 55) { _rng = new Random(seed); }

        // ---- yavru ve yem geçmişi (dertler için) ----
        /// <summary>Yavrusu götürülen ana bu kadar gün arar.</summary>
        public const double LostYoungDays = 20;
        /// <summary>Art arda bu kadar yemleme aynı yemse hayvanlar bıkar.</summary>
        public const int SameFeedRuns = 3;
        private readonly Dictionary<int, double> _lostYoung = new Dictionary<int, double>();
        private readonly List<string> _feeds = new List<string>();

        /// <summary>Bir hayvan satıldı ya da kesildi: anası yaşıyorsa yavrusunu arar.</summary>
        public void OnAnimalTaken(Animal taken)
        {
            if (taken == null || taken.MotherId == 0) return;
            _lostYoung[taken.MotherId] = Today();
        }

        /// <summary>Yemliğe yem kondu (yem eşyasının kimliği).</summary>
        public void OnFeed(string feedItemId)
        {
            if (string.IsNullOrEmpty(feedItemId)) return;
            _feeds.Add(feedItemId);
            while (_feeds.Count > SameFeedRuns) _feeds.RemoveAt(0);
        }

        public bool MissesYoung(Animal a) => _lostYoung.TryGetValue(a.Id, out var day) && Today() - day <= LostYoungDays;
        public bool BoredOfFeed => _feeds.Count >= SameFeedRuns && _feeds.All(f => f == _feeds[0]);

        public string[] ExportHistory()
        {
            var list = new List<string>();
            foreach (var kv in _lostYoung.OrderBy(k => k.Key)) list.Add("y|" + kv.Key + "|" + kv.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            foreach (var f in _feeds) list.Add("f|" + f);
            return list.ToArray();
        }

        public void ImportHistory(string[] data)
        {
            _lostYoung.Clear(); _feeds.Clear();
            if (data == null) return;
            foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line) || line.Length < 3) continue;
                var parts = line.Split('|');
                if (parts[0] == "f" && parts.Length >= 2) _feeds.Add(parts[1]);
                else if (parts[0] == "y" && parts.Length >= 3 && int.TryParse(parts[1], out var id)
                    && double.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var day))
                    _lostYoung[id] = day;
            }
        }

        public IEnumerable<AnimalTalkRecord> Records => _records.Values;
        public AnimalTalkRecord RecordOf(int characterId) => _records.TryGetValue(characterId, out var r) ? r : null;
        public bool HasSpoken(Character c) => c != null && _records.ContainsKey(c.Id);

        /// <summary>Menüde görünsün mü: beceri tutuyor ve henüz yaşamamış.</summary>
        public bool Offers(Character actor) =>
            actor != null && actor.Skills.GetLevel(SkillType.Charm) >= CharmLevel && !HasSpoken(actor);

        public string CanTry(Character actor, Animal a)
        {
            if (actor == null || a == null || a.IsDead) return "Hayvan bulunamadı.";
            if (actor.Skills.GetLevel(SkillType.Charm) < CharmLevel) return $"Gönül kazanma {CharmLevel} gerekli.";
            if (HasSpoken(actor)) return $"{actor.Name} bunu bir kez yaşadı; hayvanlar bir daha konuşmaz.";
            if (_tried.Contains((actor.Id, a.Id, Day()))) return $"{a.Name} bugün yeterince dinledi.";
            return null;
        }

        private long Day() => (long)Math.Floor(Today());

        public double ChanceFor(Character actor, Animal a) =>
            Math.Min(1.0, BaseChance * (IsCompanion(actor, a) ? CompanionFactor : 1.0));

        /// <summary>Hayvanın gerçek durumundan dert seçer (öncelik: hasta, üşüyor, aç, konfor, yavru, yalnız, yem).</summary>
        public AnimalComplaint ComplaintOf(Animal a)
        {
            if (a.IsSick) return AnimalComplaint.Sick;
            if (a.IsCold) return AnimalComplaint.Cold;
            if (a.HungryDays > 0 || a.Fullness < 35f) return AnimalComplaint.Hungry;
            if (a.Comfort < 40f) return AnimalComplaint.Draft;
            if (MissesYoung(a)) return AnimalComplaint.LostYoung;
            if (IsLonely(a)) return AnimalComplaint.Lonely;
            if (BoredOfFeed) return AnimalComplaint.SameFeed;
            return AnimalComplaint.None;
        }

        private bool IsLonely(Animal a)
        {
            foreach (var o in Animals())
                if (o != a && !o.IsDead && o.Species.Id == a.Species.Id) return false;
            return true;
        }

        /// <summary>Dert hâlâ sürüyor mu (giderildiyse false).</summary>
        public bool StillTroubled(Animal a, AnimalComplaint complaint)
        {
            switch (complaint)
            {
                case AnimalComplaint.Sick: return a.IsSick;
                case AnimalComplaint.Cold: return a.IsCold;
                case AnimalComplaint.Hungry: return a.HungryDays > 0 || a.Fullness < 70f;
                case AnimalComplaint.Draft: return a.Comfort < 50f;
                case AnimalComplaint.Lonely: return IsLonely(a);
                case AnimalComplaint.SameFeed: return BoredOfFeed;       // başka bir yem konunca geçer
                case AnimalComplaint.LostYoung: return true;             // giderilemez; zamanla kapanır
                default: return false;
            }
        }

        /// <summary>Dener. <paramref name="roll"/> testte zarı sabitler (0..1).</summary>
        public AnimalTalkResult Try(Character actor, Animal a, double? roll = null)
        {
            if (CanTry(actor, a) != null) return null;
            _tried.RemoveWhere(t => t.day != Day());
            _tried.Add((actor.Id, a.Id, Day()));
            bool spoke = ForceNext || (roll ?? _rng.NextDouble()) < ChanceFor(actor, a);
            ForceNext = false;
            var res = new AnimalTalkResult { Spoke = spoke };
            if (!spoke)
            {
                Book?.Mark(OddityIds.AnimalStare);
                res.Line = LocText.Of("core.oddity.talk.stare", ("who", actor.Name), ("animal", a.Name));
                Tried?.Invoke(actor, a, res);
                return res;
            }

            var complaint = ComplaintOf(a);
            var rec = new AnimalTalkRecord
            {
                CharacterId = actor.Id, Name = actor.Name, MotherId = actor.BirthMotherId ?? 0, FatherId = actor.BirthFatherId ?? 0,
                Generation = actor.Generation, HouseKey = HouseKeyOf(actor), AnimalId = a.Id, AnimalName = a.Name,
                SpeciesId = a.Species.Id, Day = Today(), Complaint = complaint, Closed = complaint == AnimalComplaint.None
            };
            // "Bizde de oldu": aynı soyda başka bir kuşak da yaşamışsa (gizli başarım).
            bool otherGeneration = _records.Values.Any(o => o.Generation != rec.Generation && SameFamily(actor, o));
            _records[actor.Id] = rec;
            actor.AddMoodlet(MoodCatalog.AnimalSpoke);
            actor.AddChronicle($"{a.Name} adlı {a.Species.DisplayName.ToLowerInvariant()} kendisiyle insan gibi konuştu. Kimse inanmadı.");
            Book?.Mark(OddityIds.AnimalTalk);
            Book?.Mark(OddityIds.AchAnimalSpoke);
            if (otherGeneration) Book?.Mark(OddityIds.AchRunsInFamily);
            res.Complaint = complaint;
            res.Line = LocText.Of("core.oddity.talk.complaint." + ComplaintKey(complaint), ("who", actor.Name), ("animal", a.Name));
            Tried?.Invoke(actor, a, res);
            return res;
        }

        public static string ComplaintKey(AnimalComplaint c)
        {
            switch (c)
            {
                case AnimalComplaint.Sick: return "sick";
                case AnimalComplaint.Cold: return "cold";
                case AnimalComplaint.Hungry: return "hungry";
                case AnimalComplaint.Draft: return "draft";
                case AnimalComplaint.Lonely: return "lonely";
                case AnimalComplaint.LostYoung: return "lost_young";
                case AnimalComplaint.SameFeed: return "same_feed";
                default: return "none";
            }
        }

        /// <summary>Günde bir: derdi giderilen hayvan için "Dert ortağı"; süresi geçen dert kapanır.</summary>
        public void OnNewDay()
        {
            foreach (var rec in _records.Values)
            {
                if (rec.Closed) continue;
                if (Today() - rec.Day > ConfidantDays) { rec.Closed = true; continue; }
                Animal a = null;
                foreach (var o in Animals()) if (o.Id == rec.AnimalId) { a = o; break; }
                if (a == null || a.IsDead) { rec.Closed = true; continue; }
                if (StillTroubled(a, rec.Complaint)) continue;
                rec.Closed = true;
                a.Friendship = Math.Min(100f, a.Friendship + ConfidantFriendship);
                var c = Find(rec.CharacterId);
                if (c != null && !c.IsDead) c.AddMoodlet(MoodCatalog.Confidant);
                Confided?.Invoke(rec, a);
            }
        }

        // ---- başkasına anlatmak (belge 55 §3.4) ----

        public string CanTell(Character teller, Character listener)
        {
            if (teller == null || listener == null) return "Kişi bulunamadı.";
            if (teller == listener) return "Kendisine anlatamaz.";
            if (teller.IsDead || listener.IsDead) return "Ölüyle bu yapılamaz.";
            var rec = RecordOf(teller.Id);
            if (rec == null) return "Anlatacak böyle bir anısı yok.";
            if (listener.Stage == AgeStage.Baby) return "Bebek anlamaz.";
            if (rec.Told.Contains(listener.Id)) return $"{listener.Name} bunu zaten dinledi.";
            return null;
        }

        /// <summary>Dinleyenin ataları (kendisi hariç), <see cref="AncestorDepth"/> kuşak.</summary>
        private HashSet<int> Ancestors(Character c)
        {
            var set = new HashSet<int>();
            var frontier = new List<Character> { c };
            for (int depth = 0; depth < AncestorDepth && frontier.Count > 0; depth++)
            {
                var next = new List<Character>();
                foreach (var p in frontier)
                    foreach (var id in new[] { p.BirthMotherId ?? 0, p.BirthFatherId ?? 0 })
                    {
                        if (id == 0 || !set.Add(id)) continue;
                        var parent = Find(id);
                        if (parent != null) next.Add(parent);
                    }
                frontier = next;
            }
            return set;
        }

        /// <summary>
        /// Kayıt sahibi bu karakterin ailesinden mi: aynı hane, atası, torunu ya da ortak atası olan (kardeş, kuzen).
        /// Ölmüşler de sayılır (kayıt kalır).
        /// </summary>
        public bool SameFamily(Character c, AnimalTalkRecord rec)
        {
            if (c == null || rec == null || rec.CharacterId == c.Id) return false;
            string house = HouseKeyOf(c);
            if (house != null && house == rec.HouseKey) return true;
            var anc = Ancestors(c);
            if (anc.Contains(rec.CharacterId)) return true;                       // ata
            if (rec.MotherId != 0 && (rec.MotherId == c.Id || anc.Contains(rec.MotherId))) return true;   // çocuğu, kardeşi, kuzeni
            if (rec.FatherId != 0 && (rec.FatherId == c.Id || anc.Contains(rec.FatherId))) return true;
            var other = Find(rec.CharacterId);
            return other != null && Ancestors(other).Contains(c.Id);              // torunu
        }

        public bool FamilyHasSpoken(Character c) => _records.Values.Any(r => SameFamily(c, r));

        public TellOutcome OutcomeFor(Character listener)
        {
            if (HasSpoken(listener)) return TellOutcome.Shared;
            if (FamilyHasSpoken(listener)) return TellOutcome.FamilyKnows;
            if (listener.Stage == AgeStage.Child) return TellOutcome.ChildBelieves;
            return TellOutcome.Disbelief;
        }

        public TellResult Tell(Character teller, Character listener)
        {
            if (CanTell(teller, listener) != null) return null;
            var rec = RecordOf(teller.Id);
            rec.Told.Add(listener.Id);
            var res = new TellResult { Outcome = OutcomeFor(listener) };
            var args = new (string name, object value)[] { ("teller", teller.Name), ("listener", listener.Name) };
            switch (res.Outcome)
            {
                case TellOutcome.Shared:
                    teller.AddMoodlet(MoodCatalog.NotCrazy);
                    listener.AddMoodlet(MoodCatalog.NotCrazy);
                    AddAffection?.Invoke(teller.Id, listener.Id, SharedAffection);
                    res.Line = LocText.Of("core.oddity.tell.shared", args);
                    break;
                case TellOutcome.FamilyKnows:
                    teller.AddMoodlet(MoodCatalog.Believed);
                    listener.AddMoodlet(MoodCatalog.FamilyTale);
                    AddAffection?.Invoke(teller.Id, listener.Id, FamilyAffection);
                    res.Line = LocText.Of("core.oddity.tell.family", args);
                    break;
                case TellOutcome.ChildBelieves:
                    listener.Needs.Restore(NeedType.Fun, ChildFun);
                    AddAffection?.Invoke(teller.Id, listener.Id, ChildAffection);
                    res.Line = LocText.Of("core.oddity.tell.child", args);
                    break;
                default:
                    // Kalıcı kayıp yok: yakınlık ve itibar düşmez (belge 55 ortak kural).
                    teller.AddMoodlet(MoodCatalog.NobodyBelieves);
                    listener.AddMoodlet(MoodCatalog.OddChat);
                    rec.Disbelievers++;
                    if (rec.Disbelievers == DisbelieversForNickname)
                    {
                        rec.NicknameUntil = Today() + NicknameDays();
                        res.NicknameGiven = true;
                        Book?.Mark(OddityIds.TalkNickname);
                    }
                    res.Line = LocText.Of(res.NicknameGiven ? "core.oddity.tell.disbelief_nickname" : "core.oddity.tell.disbelief", args);
                    break;
            }
            return res;
        }

        /// <summary>Köyde "Hayvanlarla konuşan" lakabı sürüyor mu (yalnızca köylü balonlarında geçer; etkisi yok).</summary>
        public bool HasNickname(Character c)
        {
            var rec = c != null ? RecordOf(c.Id) : null;
            return rec != null && rec.NicknameUntil >= 0 && Today() < rec.NicknameUntil;
        }

        public AnimalTalkSave[] Export()
        {
            var list = new List<AnimalTalkSave>();
            foreach (var r in _records.Values.OrderBy(r => r.CharacterId))
                list.Add(new AnimalTalkSave
                {
                    characterId = r.CharacterId, motherId = r.MotherId, fatherId = r.FatherId, generation = r.Generation,
                    animalId = r.AnimalId, name = r.Name, houseKey = r.HouseKey, animalName = r.AnimalName, speciesId = r.SpeciesId,
                    day = r.Day, complaint = (int)r.Complaint, closed = r.Closed, told = r.Told.OrderBy(i => i).ToArray(),
                    disbelievers = r.Disbelievers, nicknameUntil = r.NicknameUntil
                });
            return list.ToArray();
        }

        public void Import(AnimalTalkSave[] data)
        {
            _records.Clear();
            _tried.Clear();
            if (data == null) return;
            foreach (var s in data)
            {
                if (s == null || s.characterId == 0) continue;
                var r = new AnimalTalkRecord
                {
                    CharacterId = s.characterId, MotherId = s.motherId, FatherId = s.fatherId, Generation = s.generation,
                    AnimalId = s.animalId, Name = s.name, HouseKey = s.houseKey, AnimalName = s.animalName, SpeciesId = s.speciesId,
                    Day = s.day, Complaint = Enum.IsDefined(typeof(AnimalComplaint), s.complaint) ? (AnimalComplaint)s.complaint : AnimalComplaint.None,
                    Closed = s.closed, Disbelievers = s.disbelievers, NicknameUntil = s.nicknameUntil
                };
                if (s.told != null) foreach (var id in s.told) r.Told.Add(id);
                _records[r.CharacterId] = r;
            }
        }
    }

    // ------------------------------------------------------------------------------------------------------------
    // Bulunan silah ve sonraki kuşakların kalıntıları (belge 55 §4)
    // ------------------------------------------------------------------------------------------------------------

    public enum RelicKind { None = 0, Sword = 1, HelmetNest = 2, BrokenShield = 3 }
    public enum RelicStage { None = 0, Lying = 1, Inspected = 2, Resolved = 3 }
    public enum SwordFate { None = 0, Buried = 1, Taken = 2 }

    [Serializable]
    public sealed class RelicSave
    {
        public int kind, stage, fate, itemId, x, y, generation, foundCount;
        public double spawnDay = -1, buriedDay = -1;
        public bool bloomed;
    }

    public sealed class RelicLine
    {
        public LocText Line;
        /// <summary>Çocuk sorunca yakındaki ebeveynin cevabı; yoksa boş.</summary>
        public LocText Reply;
        public bool First;
    }

    /// <summary>
    /// Oyunda dövüş yok; bu sahne bunu oyunun içinden söyler. Kayıt başına bir kez köy yolunda yarı gömülü paslı bir kılıç
    /// çıkar. İnceleyen "İnsanlar neden savaşır, anlamıyorum" der, Düşünceli olur. Sonra kılıç gömülür (ilkbaharda çiçek açar)
    /// ya da alınır (demircide saban demiri, tezgâhta duvar süsü olur). Sonraki kuşaklarda başka kalıntılar çıkar.
    /// Dünyaya koymak ve kaldırmak oyun katmanının işidir (<see cref="Place"/>, <see cref="Remove"/>).
    /// </summary>
    public sealed class FoundRelicService
    {
        public const string SwordItem = "item.rusty_sword_ground";
        public const string HelmetItem = "item.helmet_nest";
        public const string ShieldItem = "item.broken_shield_ground";
        public const string OldSwordItem = "item.old_sword";
        public const string FlowerItem = "item.flower_clump";
        public const string InspectInteraction = "inspect_relic";
        public const string BuryInteraction = "bury_relic";
        public const string TakeInteraction = "take_relic";

        /// <summary>İkinci mevsimden sonra günlük şans; ilk yılın sonunda garanti.</summary>
        public const double SwordDailyChance = 0.06;
        /// <summary>Yeni kuşakta sonraki kalıntının günlük şansı.</summary>
        public const double LaterDailyChance = 0.03;
        /// <summary>Miğfer ve kalkan kaybolmaz; incelendikten sonra alınıp bahçeye süs olarak konur (kullanıcı 20.11.2026).</summary>
        public const string HelmetGardenItem = "item.helmet_nest_garden", ShieldGardenItem = "item.broken_shield_garden";

        public static string GardenItemFor(RelicKind kind) =>
            kind == RelicKind.HelmetNest ? HelmetGardenItem : kind == RelicKind.BrokenShield ? ShieldGardenItem : null;

        private readonly Random _rng;

        public OddityBook Book { get; set; }
        /// <summary>Kalıntıyı dünyaya koyar; konan eşyanın kimliğini ve yerini döndürür (koyamazsa null).</summary>
        public Func<string, (int itemId, int x, int y)?> Place { get; set; }
        /// <summary>Dünyadaki eşyayı kaldırır.</summary>
        public Action<int> Remove { get; set; }
        /// <summary>Verilen karoya eşya koyar (çiçek).</summary>
        public Func<string, int, int, bool> PlaceAt { get; set; }
        public Action<string, int> AddToHouse { get; set; }
        public bool ForceNext { get; set; }

        public RelicKind Kind { get; private set; }
        public RelicStage Stage { get; private set; }
        public SwordFate Fate { get; private set; }
        public int ItemId { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Generation { get; private set; }
        public int FoundCount { get; private set; }
        public double SpawnDay { get; private set; } = -1;
        public double BuriedDay { get; private set; } = -1;
        public bool Bloomed { get; private set; }

        public FoundRelicService(int seed = 554) { _rng = new Random(seed); }

        public static string ItemFor(RelicKind kind)
        {
            switch (kind)
            {
                case RelicKind.Sword: return SwordItem;
                case RelicKind.HelmetNest: return HelmetItem;
                case RelicKind.BrokenShield: return ShieldItem;
                default: return null;
            }
        }

        public static RelicKind KindOf(string itemId)
        {
            switch (itemId)
            {
                case SwordItem: return RelicKind.Sword;
                case HelmetItem: return RelicKind.HelmetNest;
                case ShieldItem: return RelicKind.BrokenShield;
                default: return RelicKind.None;
            }
        }

        /// <summary>Sıradaki kalıntı: önce kılıç; sonra kuşak başına bir, miğfer ve kalkan sırayla.</summary>
        public RelicKind NextKind(int generation)
        {
            if (FoundCount == 0 && Kind == RelicKind.None) return RelicKind.Sword;
            if (Stage == RelicStage.Lying) return RelicKind.None;                                    // biri henüz bulunmadı
            if (Kind == RelicKind.Sword && Stage == RelicStage.Inspected) return RelicKind.None;     // kılıcın akıbeti bekleniyor
            if (generation <= Generation) return RelicKind.None;
            return FoundCount % 2 == 1 ? RelicKind.HelmetNest : RelicKind.BrokenShield;
        }

        /// <summary>Bugün çıkmalı mı. Kılıç: 2. mevsimden itibaren şansla, ilk yılın son günü kesin.</summary>
        public bool ShouldSpawn(RelicKind kind, long day, int daysPerSeason, double roll)
        {
            if (kind == RelicKind.None) return false;
            if (ForceNext) return true;
            if (kind == RelicKind.Sword)
            {
                if (day < daysPerSeason) return false;
                if (day >= 4L * daysPerSeason - 1) return true;
                return roll < SwordDailyChance;
            }
            return roll < LaterDailyChance;
        }

        /// <summary>Günde bir: kalıntı çıkarır, gömülen kılıcın çiçeğini açtırır, bekleyen sonraki kalıntıyı kaldırır.</summary>
        public RelicKind OnNewDay(long day, int daysPerSeason, bool spring, int generation, double? roll = null)
        {
            // Gömülen kılıç: ilk ilkbaharda o karoda çiçek açar.
            if (Fate == SwordFate.Buried && !Bloomed && spring && day > BuriedDay)
            {
                if (PlaceAt == null || PlaceAt(FlowerItem, X, Y)) Bloomed = true;
            }
            var kind = NextKind(generation);
            if (!ShouldSpawn(kind, day, daysPerSeason, roll ?? _rng.NextDouble())) return RelicKind.None;
            var placed = Place?.Invoke(ItemFor(kind));
            if (placed == null) return RelicKind.None;   // yer yoksa yarın yeniden denenir
            ForceNext = false;
            Kind = kind; Stage = RelicStage.Lying; Fate = SwordFate.None;
            ItemId = placed.Value.itemId; X = placed.Value.x; Y = placed.Value.y;
            SpawnDay = day; Generation = Math.Max(Generation, generation);
            return kind;
        }

        private static string OddityFor(RelicKind kind) =>
            kind == RelicKind.Sword ? OddityIds.FoundSword : kind == RelicKind.HelmetNest ? OddityIds.HelmetNest : OddityIds.BrokenShield;

        /// <summary>
        /// İnceleme sahnesi. <paramref name="parentNearby"/>: çocuğun yanında hanenin bir büyüğü var mı (cevap verir).
        /// Aynı kalıntıya ikinci bakışta kısa bir satır döner, moodlet yenilenir ama sahne tekrar etmez.
        /// </summary>
        public RelicLine Inspect(Character c, RelicKind kind, bool parentNearby = false, bool tracked = true)
        {
            if (c == null || kind == RelicKind.None) return null;
            // Yolda süs olarak bırakılmış eski bir kalıntıya bakmak (tracked = false) yeni kalıntının durumunu değiştirmez.
            bool first = tracked && kind == Kind && Stage == RelicStage.Lying;
            if (first) { Stage = RelicStage.Inspected; FoundCount++; }
            c.AddMoodlet(MoodCatalog.Thoughtful);
            var res = new RelicLine { First = first };
            string age = c.Stage == AgeStage.Child || c.Stage == AgeStage.Baby ? "child" : c.Stage == AgeStage.Elder ? "elder" : "adult";
            string what = kind == RelicKind.Sword ? "sword" : kind == RelicKind.HelmetNest ? "helmet" : "shield";
            res.Line = LocText.Of($"core.oddity.relic.{what}.{age}", ("who", c.Name));
            if (age == "child" && parentNearby && kind == RelicKind.Sword) res.Reply = LocText.Of("core.oddity.relic.sword.parent_reply");
            if (first)
            {
                Book?.Mark(OddityFor(kind));
                if (kind == RelicKind.Sword)
                {
                    Book?.Mark(OddityIds.AchQuestionTheSword);
                    c.AddChronicle("Köy yolunda paslı bir kılıç buldu ve insanların neden savaştığını sordu.");
                }
            }
            return res;
        }

        /// <summary>Kılıcı olduğu yere göm: ilkbaharda çiçek açar.</summary>
        public bool Bury(double day)
        {
            if (Kind != RelicKind.Sword || Stage == RelicStage.Resolved || Stage == RelicStage.None) return false;
            Remove?.Invoke(ItemId);
            Stage = RelicStage.Resolved; Fate = SwordFate.Buried; BuriedDay = day; ItemId = 0;
            Book?.Mark(OddityIds.SwordBuried);
            return true;
        }

        /// <summary>
        /// Miğferi ya da kalkanı al: envantere bahçe süsü olarak girer. Yolda bırakılırsa orada süs olarak kalır;
        /// yeni kalıntı çıktıktan sonra da alınabilir (takip edilen kalıntı olması gerekmez).
        /// </summary>
        public string TakeLater(RelicKind kind, int itemId)
        {
            string give = GardenItemFor(kind);
            if (give == null || itemId == 0) return null;
            Remove?.Invoke(itemId);
            AddToHouse?.Invoke(give, 1);
            if (itemId == ItemId) { Stage = RelicStage.Resolved; ItemId = 0; }
            return give;
        }

        /// <summary>Kılıcı al: envantere "eski kılıç" olarak girer.</summary>
        public string Take()
        {
            if (Kind != RelicKind.Sword || Stage == RelicStage.Resolved || Stage == RelicStage.None) return null;
            Remove?.Invoke(ItemId);
            AddToHouse?.Invoke(OldSwordItem, 1);
            Stage = RelicStage.Resolved; Fate = SwordFate.Taken; ItemId = 0;
            return OldSwordItem;
        }

        /// <summary>Eski kılıçtan yapılan şey tamamlandı (tarif kimliğine göre tuhaflık işaretler).</summary>
        public void OnCrafted(string producedItemId)
        {
            if (producedItemId == "item.sword_ploughshare") Book?.Mark(OddityIds.SwordPloughshare);
            else if (producedItemId == "item.old_sword_wall") Book?.Mark(OddityIds.SwordOnWall);
        }

        public RelicSave Export() => new RelicSave
        {
            kind = (int)Kind, stage = (int)Stage, fate = (int)Fate, itemId = ItemId, x = X, y = Y, generation = Generation,
            foundCount = FoundCount, spawnDay = SpawnDay, buriedDay = BuriedDay, bloomed = Bloomed
        };

        public void Import(RelicSave s)
        {
            Kind = RelicKind.None; Stage = RelicStage.None; Fate = SwordFate.None; ItemId = 0; X = Y = 0; Generation = 0; FoundCount = 0;
            SpawnDay = BuriedDay = -1; Bloomed = false;
            if (s == null) return;
            Kind = Enum.IsDefined(typeof(RelicKind), s.kind) ? (RelicKind)s.kind : RelicKind.None;
            Stage = Enum.IsDefined(typeof(RelicStage), s.stage) ? (RelicStage)s.stage : RelicStage.None;
            Fate = Enum.IsDefined(typeof(SwordFate), s.fate) ? (SwordFate)s.fate : SwordFate.None;
            ItemId = s.itemId; X = s.x; Y = s.y; Generation = s.generation; FoundCount = s.foundCount;
            SpawnDay = s.spawnDay; BuriedDay = s.buriedDay; Bloomed = s.bloomed;
        }
    }

    // ------------------------------------------------------------------------------------------------------------
    // Küçük tuhaflıklar
    // ------------------------------------------------------------------------------------------------------------

    /// <summary>"Fazla selam": aynı kişiyle bir günde üçüncü sohbette karşıdaki söylenir (belge 55 §2b).</summary>
    public sealed class HelloCounter
    {
        public const int Limit = 3;
        private readonly Dictionary<long, int> _count = new Dictionary<long, int>();
        private long _day = long.MinValue;

        private static long Key(int a, int b) => a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;

        /// <summary>Sohbeti sayar; bugün tam <see cref="Limit"/>. sohbetse true (günde bir kez söylenir).</summary>
        public bool OnChat(int a, int b, long day)
        {
            if (day != _day) { _day = day; _count.Clear(); }
            long k = Key(a, b);
            _count.TryGetValue(k, out int n);
            _count[k] = ++n;
            return n == Limit;
        }
    }

    /// <summary>"Kurbağayı öp": hiçbir şey olmaz, Hayal kırıklığı. Çok nadiren kurbağanın başında minik bir taç belirir; yine hiçbir şey olmaz.</summary>
    public static class FrogKiss
    {
        public const string FrogSpecies = "wild.frog";
        public const double CrownChance = 1.0 / 40.0;

        /// <summary>Öper. Taç belirdiyse true.</summary>
        public static bool Kiss(Character c, OddityBook book, double roll, bool force = false)
        {
            c?.AddMoodlet(MoodCatalog.LetDown);
            book?.Mark(OddityIds.FrogKiss);
            bool crown = force || roll < CrownChance;
            if (crown) book?.Mark(OddityIds.FrogCrown);
            return crown;
        }
    }

    [Serializable]
    public sealed class OdditySave
    {
        public string[] seen;
        public AnimalTalkSave[] talks;
        public RelicSave relic;
        /// <summary>Yavru ve yem geçmişi (hayvanın derdi için).</summary>
        public string[] talkHistory;
        /// <summary>Küçük easter egg'lerin sayaç ve bayrakları (OddityEvents).</summary>
        public string[] events;
        /// <summary>Kedinin yere düşürdüğü, henüz yerine konmamış süsler ("eşyaKimliği:x:y:z").</summary>
        public string[] knocked;
    }
}
