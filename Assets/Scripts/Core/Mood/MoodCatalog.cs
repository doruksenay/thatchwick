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
        public const float ColdMoodThreshold = 40f;

        // Olay moodlet'leri.
        public const string GoodMeal = "good_meal", Rested = "rested", Clean = "clean", Inspired = "inspired", HadFun = "had_fun",
            NiceChat = "nice_chat", Newlywed = "newlywed", NewBaby = "new_baby", Grief = "grief", Harmony = "harmony",
            BrightRoom = "bright_room", GoodHarvest = "good_harvest", Proud = "proud", PetLove = "pet_love",
            AnimalDied = "animal_died", CropFrozen = "crop_frozen", SharpTools = "sharp_tools", Slaughtered = "slaughtered",
            PotionVigor = "potion_vigor", PotionFocus = "potion_focus", PotionCalm = "potion_calm", NewFoal = "new_foal",
            SameMeal = "same_meal", VariedTable = "varied_table";

        // Kaza yaraları (InjurySystem; süre yaraya göre ayarlanır, buradaki varsayılan).
        public const string InjuredAxe = "injured_axe", InjuredBurn = "injured_burn", InjuredAnimal = "injured_animal",
            InjuredSickle = "injured_sickle", InjuredFall = "injured_fall";

        private readonly Dictionary<string, MoodletDef> _defs = new Dictionary<string, MoodletDef>(StringComparer.Ordinal);

        public IEnumerable<MoodletDef> All => _defs.Values;

        public void Register(MoodletDef def)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.Id)) throw new ArgumentException("Moodlet kimliği boş.");
            _defs[def.Id] = def;
        }

        public bool TryGet(string id, out MoodletDef def) => _defs.TryGetValue(id ?? "", out def);
        public bool Contains(string id) => id != null && _defs.ContainsKey(id);

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
            m = M(Rested, "Dinlenmiş", 10, 0.6, "İyi uyudu: odunculuk ve çiftçilik %15 hızlı.");
            m.SkillSpeed[SkillType.Woodcutting] = 0.15f; m.SkillSpeed[SkillType.Farming] = 0.15f; c.Register(m);
            c.Register(M(Clean, "Tertemiz", 10, 1.0, "Yıkandı, kendini iyi hissediyor."));
            m = M(Inspired, "İlham dolu", 15, 1.0, "Okudu ya da güzel bir şey gördü: marangozlukta %30 fazla XP."); m.SkillXp[SkillType.Carpentry] = 0.30f; c.Register(m);
            m = M(HadFun, "Eğlendi", 10, 0.5, "Oyun oynadı: tüm becerilerde %5 fazla XP."); m.AllXp = 0.05f; c.Register(m);
            c.Register(M(NiceChat, "Güzel sohbet", 5, 0.5, "Biriyle hoş bir sohbet etti."));
            c.Register(M(Newlywed, "Yeni evli", 25, 6.0, "Evlendi."));
            c.Register(M(NewBaby, "Bebeğimiz oldu", 20, 5.0, "Aileye yeni bir bebek katıldı."));
            m = M(Grief, "Yas", -30, 8.0, "Yakınını kaybetti: tüm becerilerde %20 az XP, işler %10 yavaş."); m.AllXp = -0.20f; m.AllSpeed = -0.10f; c.Register(m);
            c.Register(M(Harmony, "Uyumlu ev", 10, 0.5, "Odadaki eşyalar aynı takımdan (4+)."));
            c.Register(M(BrightRoom, "Aydınlık oda", 5, 0.3, "Odada kandil, meşale ya da lamba var."));
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
            return c;
        }
    }
}
