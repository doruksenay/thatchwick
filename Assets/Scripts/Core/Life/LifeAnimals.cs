using Thatchwick.Core.Localization;
using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Animals.Genetics;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    // Hayvan genetiği, köy merası ve yayla, böcek ve balık defteri, çekirge, perhiz (Project belgesi 48).
    public sealed partial class LifeSystems
    {
        /// <summary>Böcek ve balık defteri (belge 48 §9); hanedanla kalır.</summary>
        public CreatureBook Book { get; } = new CreatureBook();
        /// <summary>Çekirge sürüsü (yaz, kuru günler).</summary>
        public LocustSwarm Locusts { get; } = new LocustSwarm();
        private AnimalSystem _animalsRef;
        private readonly Random _petRng = new Random(5153);

        public const double MushroomNoseChance = 0.15, FisherPawChance = 0.2, TreasureNoseChance = 0.5;
        /// <summary>Efsane balık: Balıkçılık 10, suyun efsanesi yılda bir, tutma başına şans.</summary>
        public const int LegendFishLevel = 10;
        public const double LegendFishChance = 0.03, PearlChance = 0.02;

        private static readonly string[] TreasureRelics = { "item.relic_coin", "item.relic_horseshoe", "item.relic_bone_comb", "item.relic_buried_jar" };

        /// <summary>Bugün perhiz günü mü (balık pahalı, et ucuz).</summary>
        public bool IsFasting => FastingCalendar.IsFasting(Clock.Season, Clock.DayOfSeason);

        private void AttachAnimalGenetics(AnimalSystem animals)
        {
            _animalsRef = animals;
            animals.ReputationNow = () => Upbringing.Reputation.Points;
            animals.TodayNow = () => Clock.TotalDays;
            animals.GeneEvent += (baby, key) =>
            {
                string text = key == "event.male_calico" ? $"Çok nadir bir doğum: erkek üç renkli bir kedi yavrusu ({baby.Name})!"
                    : key == "event.miracle_foal" ? $"Mucize: katırımız bir tay doğurdu ({baby.Name})! Köyde günlerce konuşulacak."
                    : null;
                if (text == null) return;
                Say(text, NoticeLevel.Info);
                string savedText = SavedText.Encode(key == "event.male_calico" ? "core.chronicle.male_calico" : "core.chronicle.miracle_foal", ("name", baby.Name));
                foreach (var m in Population.Members) if (!m.IsDead && !m.IsVillager) m.AddChronicle(savedText);
            };
            animals.AwayLost += (a, why) =>
                Say(why == "old_age" ? $"{a.Name} uzaktayken yaşlılıktan öldü." : $"{a.Name} {(why == "lost" ? "yaylada ya da merada kayboldu (kurt ya da uçurum)" : "döndü")}.",
                    why == "lost" ? NoticeLevel.Warning : NoticeLevel.Info);
            animals.AwayReturned += (back, stuck) =>
                Say(stuck > 0 ? $"Kış geldi: {back} hayvan meradan ve yayladan döndü; {stuck} hayvana barınakta yer yok, uzakta kaldı."
                              : $"Kış geldi: {back} hayvan meradan ve yayladan döndü.", stuck > 0 ? NoticeLevel.Warning : NoticeLevel.Info);
            Mischief.DogGuarded += () => DeedToType(animals, PetLegends.Guards, "dog.mastiff", "dog.alaunt");
            if (Roamers != null)
            {
                Roamers.HoundEffects = () => (animals.AnyGift(Gift.Pathfinder) ? 0.15 : 0.0, animals.AnyGift(Gift.SoftMouth) ? 1 : 0);
                Roamers.HoundSucceeded += () => DeedToType(animals, PetLegends.Tracks, "dog.scenthound", "dog.tracker", "dog.laketracker", "dog.lurcher");
            }
            Home.Caught += item => Book.Record(item);
            Book.SetCompleted += set =>
            {
                Upbringing.Reputation.Add(CreatureBook.SetReputation, "doğa defteri");
                var who = BestAt(set.Kind == CreatureKind.Fish ? SkillType.Fishing : SkillType.Foraging);
                who?.GainSkillXp(set.Kind == CreatureKind.Fish ? SkillType.Fishing : SkillType.Foraging, CreatureBook.SetSkillXp);
                Say($"Defterde bir set tamamlandı: {SetName(set)}.", NoticeLevel.Info);
            };
            Book.FirstRecorded += (item, legendary) =>
            {
                if (!legendary) return;
                string name = Content.Items.TryGet(item, out var d) ? d.DisplayName : item;
                string text = $"Efsanevi bir canlı deftere girdi: {name}.";
                Say(text, NoticeLevel.Info);
                string savedText = SavedText.Encode("core.chronicle.legendary_creature", ("item", SavedText.Content(item)));
                foreach (var m in Population.Members) if (!m.IsDead && !m.IsVillager) m.AddChronicle(savedText);
            };
        }

        private static string SetName(CreatureSet set)
        {
            switch (set.Id)
            {
                case "insects_spring": return "İlkbahar böcekleri";
                case "insects_summer": return "Yaz böcekleri";
                case "insects_autumn": return "Güz böcekleri";
                case "insects_winter": return "Kış böcekleri";
                case "insects_water": return "Su böcekleri";
                case "fish_still": return "Durgun su balıkları";
                case "fish_river": return "Irmak balıkları";
                default: return "Sazlık balıkları";
            }
        }

        private static void DeedToType(AnimalSystem animals, string deed, params string[] types)
        {
            foreach (var a in new List<Animal>(animals.Animals))
                if (!a.IsDead && a.IsMature && a.TypeId != null && Array.IndexOf(types, a.TypeId) >= 0) animals.AddDeed(a, deed);
        }

        // Balık: defter, sudaki efsane (Balıkçılık 10, yılda bir), midyeden inci.
        private void AttachFishing(Thatchwick.Core.Work.FarmWork farm)
        {
            farm.FishCaught += OnOddFish;   // eski çizme, şişe mektubu, dev balık (belge 55 §1)
            farm.FishCaught += (c, fish) =>
            {
                Book.Record(fish);
                if (fish == "item.mussel" && _petRng.NextDouble() < PearlChance && Content.Items.Contains("item.pearl"))
                {
                    House.AddItem("item.pearl", 1);
                    Say($"{c.Name} midyeyi açtı: içinden bir inci çıktı!", NoticeLevel.Info, c.Id);
                }
            };
            farm.LegendFishAt = (c, pond) =>
            {
                if (c == null || pond == null || c.Skills.GetLevel(SkillType.Fishing) < LegendFishLevel) return null;
                string legend = LegendOf(pond);
                int year = (int)(Clock.DayIndex / Clock.Config.DaysPerYear);
                if (legend == null || Book.LegendFishTaken(legend, year) || _petRng.NextDouble() >= LegendFishChance) return null;
                Book.TryClaimLegendFish(legend, year);
                if (_animalsRef != null) DeedToType(_animalsRef, PetLegends.LegendFish, "dog.waterdog");
                return legend;
            };
        }

        /// <summary>Suyun efsane balığı balık düzeninden: yayın ya da kadife varsa göl (dev yayın), alabalık varsa ırmak (yaşlı mersin), yılan balığı ve turna sazlık (yaşlı turna), yoksa göl.</summary>
        public static string LegendOf(Pond pond)
        {
            if (pond == null) return null;
            if (pond.Fish.ContainsKey("item.fish_catfish") || pond.Fish.ContainsKey("item.fish_tench")) return "item.fish_catfish_giant";
            if (pond.Fish.ContainsKey("item.fish_trout") || pond.Fish.ContainsKey("item.fish_salmon")) return "item.fish_sturgeon_old";
            if (pond.Fish.ContainsKey("item.fish_eel") && pond.Fish.ContainsKey("item.fish_pike")) return "item.fish_pike_old";
            return "item.fish_catfish_giant";
        }

        /// <summary>
        /// Gün başı (belge 48): hüner etkileri (mantar burnu, define burnu, balıkçı pati, hava sezgisi, ev ruhu, şifacı mırıltı),
        /// kedilerin fare avı sayacı, çekirge sürüsü, perhiz duyurusu.
        /// </summary>
        private void AnimalDay(long day)
        {
            if (FastingCalendar.StartsTomorrow(Clock.Season, Clock.DayOfSeason))
                Say("Yarın perhiz başlıyor: üç gün balık pahalı, et ucuz satılır.", NoticeLevel.Info);
            var animals = _animalsRef;
            if (animals != null)
            {
                bool stormTomorrow = Weather.Tomorrow(Clock.TotalDays) == WeatherType.Storm;
                bool spiritDone = false, purrDone = false;
                foreach (var a in new List<Animal>(animals.Animals))
                {
                    if (a.IsDead || !a.IsMature || a.Genome == null) continue;
                    var g = a.Genome;
                    if (a.Species.Id == GeneticsCatalog.Cat)
                        animals.AddDeed(a, PetLegends.Rats, 1 + g.Stat(GeneStat.Hunting) / 4);   // ambar faresi (efsane yolu)
                    if (g.HasGift(Gift.MushroomNose) && _petRng.NextDouble() < MushroomNoseChance && Clock.Season != Season.Winter)
                    { House.AddItem("item.mushroom", 1); Say($"{a.Name} ormandan bir mantar buldu getirdi.", NoticeLevel.Info); }
                    if (g.HasGift(Gift.TreasureNose) && day % 7 == 0 && _petRng.NextDouble() < TreasureNoseChance)
                    {
                        string relic = TreasureRelics[_petRng.Next(TreasureRelics.Length)];
                        if (Content.Items.Contains(relic)) { House.AddItem(relic, 1); Say($"{a.Name} toprağı eşeledi, eski bir şey çıkardı: {Content.Items.Get(relic).DisplayName}.", NoticeLevel.Info); }
                    }
                    if (g.HasGift(Gift.FisherPaw) && _petRng.NextDouble() < FisherPawChance && Clock.Season != Season.Winter)
                    { House.AddItem("item.fish_crayfish", 1); Say($"{a.Name} dereden bir kerevit kaptı getirdi.", NoticeLevel.Info); }
                    if (g.HasGift(Gift.WeatherSense) && stormTomorrow)
                        Say($"{a.Name} huzursuz, kapıya bakıp duruyor: yarın fırtına var.", NoticeLevel.Warning);
                    if (g.HasGift(Gift.HouseSpirit) && !spiritDone)
                    {
                        spiritDone = true;
                        foreach (var c in Household())
                            if (c.MoodEnabled) c.Mood.Add(new Mood.MoodletDef { Id = "mood.house_spirit", DisplayName = "Evin ruhu", Description = $"{a.Name} evi sıcacık tutuyor.", Value = 2, DurationDays = 1 });
                    }
                    if (g.HasGift(Gift.HealingPurr) && !purrDone)
                    {
                        purrDone = true;
                        double today = Clock.TotalDays;
                        foreach (var c in Household())
                            if (c.Injury != null && c.Injury.HealDay > today) c.Injury.HealDay = Math.Max(today, c.Injury.HealDay - 0.25);
                    }
                }
            }

            // Çekirge sürüsü
            var crops = new List<ItemInstanceRef>();
            foreach (var it in World.Items) if (it.IsPlant && it.Def.IsCrop) crops.Add(new ItemInstanceRef(it.Id));
            int poultry = 0;
            if (animals != null && animals.Released)
                foreach (var a in animals.Animals) if (!a.IsDead && a.IsMature && a.Species.Housing == "coop") poultry++;
            bool rainy = Weather.Today(Clock.TotalDays) == WeatherType.Rain || Weather.Today(Clock.TotalDays) == WeatherType.Storm;
            int r = Locusts.OnNewDay(Clock.Season, rainy, crops.Count, poultry, out int eaten);
            if (r == 1) Say("Ufukta kara bir bulut: çekirge sürüsü yarın tarlaya inecek. Tavuk, ördek ve kazları salmak zararı azaltır.", NoticeLevel.Warning);
            else if (r == 2)
            {
                for (int i = 0; i < eaten && crops.Count > 0; i++)
                {
                    int k = _petRng.Next(crops.Count);
                    World.RemoveItem(crops[k].Id);
                    crops.RemoveAt(k);
                }
                if (Content.Items.Contains("item.locust")) House.AddItem("item.locust", LocustSwarm.LocustYield);
                Say($"Çekirge sürüsü tarlaya indi, {eaten} ekin yendi." + (poultry >= LocustSwarm.PoultryNeeded ? " Kümes hayvanları sürünün yarısını yedi." : "") + " Toplanan çekirgeler yem olur.", NoticeLevel.Warning);
            }
        }

        private readonly struct ItemInstanceRef { public readonly int Id; public ItemInstanceRef(int id) { Id = id; } }
    }
}
