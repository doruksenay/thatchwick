using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Mood
{
    /// <summary>
    /// Moodlet tanımları. Varsayılan katalog kodda (denge turunda ayarlanır); içerik etkileşimleri
    /// (items.json "moodlet") buradaki kimliklere başvurur ve ContentDatabase doğrular.
    /// </summary>
    public sealed class MoodCatalog
    {
        // İhtiyaç moodlet'leri (koşul): ihtiyaç eşiğin altındayken açık.
        public const string Hungry = "need_hungry", Tired = "need_tired", Cold = "need_cold", Bored = "need_bored", Dirty = "need_dirty", Lonely = "need_lonely";
        public const float NeedMoodThreshold = 25f;
        /// <summary>
        /// Aç ve Yorgun moodlet'i bunun altında açılır (18.11.2026: 25'ten indirildi). Otonomi 30'da arar; eşikler çakışınca
        /// iyi işleyen hanede bile zamanın yarısında -25 kalıyordu (Project belgesi 23, bölüm 3).
        /// </summary>
        public const float HungerTiredMoodThreshold = 15f;
        public const float ColdMoodThreshold = 40f;

        // Olay moodlet'leri.
        public const string GoodMeal = "good_meal", Rested = "rested", Clean = "clean", Inspired = "inspired", HadFun = "had_fun",
            NiceChat = "nice_chat", Newlywed = "newlywed", NewBaby = "new_baby", Grief = "grief", Harmony = "harmony",
            BrightRoom = "bright_room", GoodHarvest = "good_harvest", Proud = "proud", PetLove = "pet_love",
            AnimalDied = "animal_died", CropFrozen = "crop_frozen", SharpTools = "sharp_tools", Slaughtered = "slaughtered",
            PotionVigor = "potion_vigor", PotionFocus = "potion_focus", PotionCalm = "potion_calm", NewFoal = "new_foal",
            SameMeal = "same_meal", VariedTable = "varied_table",
            Bursting = "need_bladder", Accident = "accident", Wet = "wet", Chilled = "chilled", Relieved = "relieved", Ashamed = "ashamed",
            HotBath = "hot_bath", ColdWater = "cold_water", Swim = "swim",
            Catharsis = "catharsis", HarmonyFine = "harmony_fine",
            HarmonyTidy = "harmony_tidy", HarmonyFineTidy = "harmony_fine_tidy",
            // Beceri sosyal işleri (belge 45).
            Complimented = "complimented", Unburdened = "unburdened", Hopeful = "hopeful", Persuasive = "persuasive",
            FestivalSpeech = "festival_speech", Lulled = "lulled", NewSong = "new_song", PlayedTavern = "played_tavern",
            Composed = "composed", FirstSteps = "first_steps", Understood = "understood", MidwifeCare = "midwife_care",
            DoctorVisit = "doctor_visit", WellCared = "well_cared", GotLetter = "got_letter", MasterWork = "master_work",
            Feast = "feast", SkillShowcase = "skill_showcase";

        // Easter egg'ler (belge 55): hayvanla konuşma, bulunan silah, kurbağayı öpme. İkonlar: ui_icon_moodlet_<kimlik> (iş 494).
        public const string AnimalSpoke = "animal_spoke", Confidant = "confidant", NotCrazy = "not_crazy", Believed = "believed",
            FamilyTale = "family_tale", NobodyBelieves = "nobody_believes", OddChat = "odd_chat", Thoughtful = "thoughtful",
            LetDown = "let_down";

        // Kaza yaraları (InjurySystem; süre yaraya göre ayarlanır, buradaki varsayılan).
        public const string InjuredAxe = "injured_axe", InjuredBurn = "injured_burn", InjuredAnimal = "injured_animal",
            InjuredSickle = "injured_sickle", InjuredFall = "injured_fall",
            InjuredCaveIn = "injured_cave_in", CaveInSurvived = "cave_in_survived", VeinFound = "vein_found", Dusty = "dusty";

        private readonly Dictionary<string, MoodletDef> _defs = new Dictionary<string, MoodletDef>(StringComparer.Ordinal);
        // Default paylaşılan bir örnek: her ContentDatabase içerik yüklerken yemek ve içki etkilerini buraya kaydeder.
        // Testler paralel koştuğunda (xUnit sınıf başına) aynı anda yazıldığı için sözlük bozuluyordu (CI, 2026-11-18).
        private readonly object _lock = new object();

        /// <summary>Kayıtların anlık kopyası (döngü sırasında başka kayıt gelse de güvenli).</summary>
        public IEnumerable<MoodletDef> All { get { lock (_lock) return new List<MoodletDef>(_defs.Values); } }

        public void Register(MoodletDef def)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.Id)) throw new ArgumentException("Moodlet kimliği boş.");
            lock (_lock) _defs[def.Id] = def;
        }

        public bool TryGet(string id, out MoodletDef def) { lock (_lock) return _defs.TryGetValue(id ?? "", out def); }
        public bool Contains(string id) { if (id == null) return false; lock (_lock) return _defs.ContainsKey(id); }

        public static readonly MoodCatalog Default = CreateDefault();

        private static MoodletDef M(string id, string name, int value, double days, string desc)
            => new MoodletDef { Id = id, DisplayName = name, Value = value, DurationDays = days, Description = desc };

        public static MoodCatalog CreateDefault()
        {
            var c = new MoodCatalog();

            var m = M(Hungry, "Aç", -15, 0, "Karnı aç: tüm işler %10 yavaş."); m.AllSpeed = -0.10f; c.Register(m);
            m = M(Tired, "Yorgun", -10, 0, "Uykusu var: odunculuk ve çiftçilik %20 yavaş.");
            m.SkillSpeed[SkillType.Woodcutting] = -0.20f; m.SkillSpeed[SkillType.Farming] = -0.20f; c.Register(m);
            m = M(Cold, "Üşüyor", -15, 0, "Elleri donuyor: marangozluk ve yemek %20 yavaş.");
            m.SkillSpeed[SkillType.Carpentry] = -0.20f; m.SkillSpeed[SkillType.Cooking] = -0.20f; c.Register(m);
            m = M(Bored, "Sıkılmış", -10, 0, "Canı sıkkın: tüm becerilerde %15 az XP."); m.AllXp = -0.15f; c.Register(m);
            m = M(Dirty, "Kirli", -10, 0, "Yıkanmalı: yemek becerisinde %20 az XP, sohbette yakınlık yarı."); m.SkillXp[SkillType.Cooking] = -0.20f; c.Register(m);

            m = M(Lonely, "Yalnız", -10, 0, "Kimseyle konuşmadı: sohbet ve şenlik iyi gelir."); m.AllXp = -0.10f; c.Register(m);

            c.Register(M(GoodMeal, "Doyurucu yemek", 10, 0.5, "Sıcak bir yemek yedi."));
            // Bunalımdan sonra (RimWorld'deki catharsis gibi): art arda bunalım sarmalını önler.
            c.Register(M(Catharsis, "İçini döktü", 20, 2.0, "Bunalımı atlattı, biraz hafifledi."));
            m = M(Rested, "Dinlenmiş", 10, 0.6, "İyi uyudu: odunculuk ve çiftçilik %15 hızlı.");
            m.SkillSpeed[SkillType.Woodcutting] = 0.15f; m.SkillSpeed[SkillType.Farming] = 0.15f; c.Register(m);
            c.Register(M(Clean, "Tertemiz", 10, 1.0, "Yıkandı, kendini iyi hissediyor."));
            m = M(Inspired, "İlham dolu", 15, 1.0, "Okudu ya da güzel bir şey gördü: marangozlukta %30 fazla XP."); m.SkillXp[SkillType.Carpentry] = 0.30f; c.Register(m);
            m = M(HadFun, "Eğlendi", 10, 0.5, "Oyun oynadı: tüm becerilerde %5 fazla XP."); m.AllXp = 0.05f; c.Register(m);
            c.Register(M(NiceChat, "Güzel sohbet", 5, 0.5, "Biriyle hoş bir sohbet etti."));
            c.Register(M(Newlywed, "Yeni evli", 25, 6.0, "Evlendi."));
            c.Register(M(NewBaby, "Bebeğimiz oldu", 20, 5.0, "Aileye yeni bir bebek katıldı."));
            m = M(Grief, "Yas", -30, 8.0, "Yakınını kaybetti: tüm becerilerde %20 az XP, işler %10 yavaş."); m.AllXp = -0.20f; m.AllSpeed = -0.10f; c.Register(m);
            c.Register(M(Harmony, "Uyumlu ev", 10, 0.5, "Odadaki eşyalar aynı takımdan ya da aynı tarzdan (4+)."));
            c.Register(M(HarmonyFine, "Özenli ev", 15, 0.5, "Odada hem aynı takımdan hem aynı tarzdan 4+ eşya var."));
            // Ev düzenleme 5+ olan hanede uyum moodlet'leri x1,5 (belge 45 karar 7, 9.5).
            c.Register(M(HarmonyTidy, "Uyumlu ve derli toplu ev", 15, 0.5, "Aynı takım ya da tarz, usta bir elle yerleştirilmiş (Ev düzenleme 5+)."));
            c.Register(M(HarmonyFineTidy, "Özenli ve derli toplu ev", 22, 0.5, "Hem aynı takım hem aynı tarz, usta bir elle yerleştirilmiş (Ev düzenleme 5+)."));
            c.Register(M(BrightRoom, "Aydınlık oda", 5, 0.3, "Odada kandil, meşale ya da lamba var."));
            // Beceri sosyal işleri ve ustalık (belge 45).
            c.Register(M(Complimented, "İltifat aldı", 6, 0.5, "Biri güzel sözler söyledi."));
            c.Register(M(Unburdened, "İçini döktü", 8, 1.0, "Derdini anlattı, hafifledi."));
            c.Register(M(Hopeful, "Umutlu", 6, 2.0, "Görücü çıkıldı; talip bekleniyor."));
            c.Register(M(Persuasive, "İkna edici", 5, 1.0, "Bugün pazarlıkta dili tatlı: satışlar %5 iyi."));
            c.Register(M(FestivalSpeech, "Şenlikte söz aldık", 8, 1.0, "Hanenin sözü şenlikte dinlendi."));
            c.Register(M(Lulled, "Ninniyle uyudu", 8, 0.5, "Ninni dinledi."));
            m = M(NewSong, "Yeni türkü", 6, 2.0, "Yeni bir türkü öğrendi: müzikte %20 fazla XP."); m.SkillXp[SkillType.Music] = 0.20f; c.Register(m);
            c.Register(M(PlayedTavern, "Meyhanede çaldı", 6, 1.0, "Meyhanede çaldı, alkış aldı."));
            c.Register(M(Composed, "Yeni ezgi", 10, 2.0, "Yeni bir ezgi doğdu."));
            c.Register(M(FirstSteps, "İlk adımlar", 12, 1.0, "Bebek ilk adımlarını attı."));
            c.Register(M(Understood, "Anlaşıldı", 10, 2.0, "Biri onu gerçekten dinledi."));
            c.Register(M(MidwifeCare, "Ebe gözetiminde", 8, 6.0, "Doğuma bir ebe bakacak; içi rahat."));
            c.Register(M(DoctorVisit, "Hekim baktı", 6, 1.0, "Hekim hâlini sordu."));
            c.Register(M(WellCared, "Bakımlı", 8, 1.0, "Biri ona özenle baktı."));
            c.Register(M(GotLetter, "Mektup aldı", 6, 1.0, "Bir mektup geldi."));
            c.Register(M(MasterWork, "Ustalık eseri", 15, 2.0, "Adını taşıyan bir ustalık eseri yaptı."));
            c.Register(M(Feast, "Ziyafet", 12, 1.0, "Hane birlikte bol bir sofraya oturdu."));
            c.Register(M(SkillShowcase, "Ustayı seyretti", 5, 0.5, "Bir ustanın işini seyretti."));
            // Easter egg'ler (belge 55 §3.3, §4). Ortak kural: olumsuz olan kısa sürer, kalıcı kayıp yok.
            c.Register(M(AnimalSpoke, "Hayvan benimle konuştu!", 10, 3.0, "Bir hayvan derdini insan gibi anlattı. Kime anlatsa inanmaz."));
            c.Register(M(Confidant, "Dert ortağı", 5, 2.0, "Hayvanın anlattığı derdi giderdi."));
            c.Register(M(NotCrazy, "Deli değilmişim", 8, 2.0, "Aynı şeyi yaşamış biriyle konuştu."));
            c.Register(M(Believed, "İnanıldı", 5, 1.0, "Anlattığına inanan biri çıktı: onların ailesinde de olmuş."));
            c.Register(M(FamilyTale, "Aile hikâyesi doğruymuş", 3, 1.0, "Ailede anlatılan hikâyeyi bir başkasından da duydu."));
            c.Register(M(NobodyBelieves, "Kimse inanmıyor", -3, 0.25, "Hayvanın konuştuğunu anlattı, inanmadılar."));
            c.Register(M(OddChat, "Tuhaf bir sohbet", 2, 0.25, "Biri ona hayvanların konuştuğunu anlattı."));
            c.Register(M(Thoughtful, "Düşünceli", 3, 1.0, "Barışın kıymetini düşündü."));
            c.Register(M(LetDown, "Hayal kırıklığı", -2, 0.25, "Kurbağayı öptü. Hiçbir şey olmadı."));
            m = M(GoodHarvest, "Bereketli hasat", 10, 1.0, "Ürün topladı: çiftçilikte %20 fazla XP."); m.SkillXp[SkillType.Farming] = 0.20f; c.Register(m);
            m = M(Proud, "Gururlu", 15, 1.0, "Kendi elleriyle bir eşya yaptı: marangozluk %10 hızlı."); m.SkillSpeed[SkillType.Carpentry] = 0.10f; c.Register(m);
            c.Register(M(PetLove, "Hayvan dostu", 10, 0.5, "Evcil hayvanıyla oynadı."));
            c.Register(M(AnimalDied, "Hayvanımız öldü", -10, 2.0, "Bir hayvan açlıktan ya da yaşlılıktan öldü."));
            m = M(SharpTools, "Keskin aletler", 5, 0.5, "Baltasını biledi: odunculuk %20 hızlı."); m.SkillSpeed[SkillType.Woodcutting] = 0.20f; c.Register(m);
            c.Register(M(Slaughtered, "Zor bir iş", -4, 1.0, "Bir hayvanı kesmek zorunda kaldı."));
            c.Register(M(CropFrozen, "Ekin dondu", -10, 2.0, "Hasat edilmemiş ekin kışta dondu."));
            // İksirler (otacılık).
            m = M(PotionVigor, "Dinç", 8, 1.0, "Dinçlik iksiri içti: bütün işler %15 hızlı."); m.AllSpeed = 0.15f; c.Register(m);
            m = M(PotionFocus, "Odaklanmış", 6, 1.0, "Zihin iksiri içti: bütün becerilerde %30 fazla XP."); m.AllXp = 0.30f; c.Register(m);
            c.Register(M(PotionCalm, "Huzurlu", 15, 1.0, "Huzur şerbeti içti: içi rahat."));
            c.Register(M(NewFoal, "Yavru sevinci", 8, 1.0, "Ahırda bir yavru doğdu."));
            // Sofra çeşidi.
            c.Register(M(SameMeal, "Hep aynı yemek", -3, 1.0, "Üç öğündür aynı şeyi yiyor."));
            c.Register(M(VariedTable, "Zengin sofra", 4, 1.0, "Son üç öğünü birbirinden farklıydı."));
            m = M(InjuredAxe, "Yaralı", -10, 3.0, "Baltayla kendini kesti: odunculuk ve marangozluk %20 yavaş.");
            m.SkillSpeed[SkillType.Woodcutting] = -0.20f; m.SkillSpeed[SkillType.Carpentry] = -0.20f; c.Register(m);
            m = M(InjuredBurn, "Yaralı", -10, 3.0, "Elini yaktı: yemek ve ocak işleri %20 yavaş.");
            m.SkillSpeed[SkillType.Cooking] = -0.20f; m.SkillSpeed[SkillType.Smithing] = -0.20f; m.SkillSpeed[SkillType.Baking] = -0.20f; c.Register(m);
            m = M(InjuredAnimal, "Yaralı", -10, 3.0, "Bir hayvan ısırdı: hayvancılık ve binicilik %20 yavaş.");
            m.SkillSpeed[SkillType.AnimalHusbandry] = -0.20f; m.SkillSpeed[SkillType.Riding] = -0.20f; c.Register(m);
            m = M(InjuredSickle, "Yaralı", -8, 2.0, "Orakla elini kesti: çiftçilik %20 yavaş.");
            m.SkillSpeed[SkillType.Farming] = -0.20f; c.Register(m);
            m = M(InjuredFall, "Yaralı", -8, 2.0, "Düşüp yaralandı: bütün işler %10 yavaş."); m.AllSpeed = -0.10f; c.Register(m);
            // Madencilik (23. belge 6.3, 7.1): göçük yaralar, öldürmez.
            m = M(InjuredCaveIn, "Yaralı", -10, 4.0, "Göçük altında kaldı: madencilik %30, bütün işler %10 yavaş.");
            m.SkillSpeed[SkillType.Mining] = -0.30f; m.AllSpeed = -0.10f; c.Register(m);
            c.Register(M(CaveInSurvived, "Göçükten kurtuldu", -15, 2.0, "Az kalsın taşın altında kalıyordu."));
            c.Register(M(VeinFound, "Damar buldu!", 10, 1.0, "Kayanın içinden bir kalıntı ya da parlak bir taş çıktı."));
            c.Register(M(Dusty, "Tozlu", -5, 0.25, "Kazıdan toz toprak içinde çıktı; yıkanınca geçer."));
            // Tuvalet, ıslaklık, yıkanma.
            m = M(Bursting, "Sıkışmış", -10, 0, "Tuvalete gitmeli: işler %10 yavaş."); m.AllSpeed = -0.10f; c.Register(m);
            c.Register(M(Accident, "Altına kaçırdı", -15, 1.0, "Tuvalete yetişemedi."));
            c.Register(M(Relieved, "Rahatladı", 3, 0.3, "Tuvalete gitti."));
            c.Register(M(Ashamed, "Utandı", -4, 0.5, "Çalı dibine gitmek zorunda kaldı."));
            c.Register(M(Wet, "Islak", -5, 0, "Islak: soğukta ısıyı iki kat hızlı kaybeder; ocak başında kurur."));
            m = M(Chilled, "Üşüttü", -8, 2.0, "Islak ve soğukta kaldı: soğuktan %30 fazla etkilenir, işler %10 yavaş. Hekim iyileştirir.");
            m.AllSpeed = -0.10f; m.ColdResist = -0.3f; c.Register(m);
            c.Register(M(HotBath, "Sıcak banyo", 8, 1.0, "Ocak yanan odada sıcak suyla yıkandı."));
            m = M(ColdWater, "Buz gibi su", -6, 0.5, "Soğuk suyla yıkandı, titredi."); c.Register(m);
            c.Register(M(Swim, "Suya girdi", 6, 0.5, "Yazın dereye girip serinledi."));
            return c;
        }
    }
}
