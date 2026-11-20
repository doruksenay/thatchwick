using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    // Küçük easter egg'ler (Project belgesi 55 §1, §2, §2b): olayların dünyaya etkisi ve satırları.
    // Ortak kural: kalıcı kayıp yok. Johan Wick'in dağıttığı ekin 3-4 karoyla sınırlıdır ve karşılığında sikke bırakır.
    public sealed partial class LifeSystems
    {
        /// <summary>Küçük easter egg'lerin zarları, sayaçları ve bayrakları.</summary>
        public OddityEvents OddEvents { get; private set; }
        /// <summary>Gerçek tarih (1 Nisan); oyun katmanı verir, yoksa o easter egg kapalıdır.</summary>
        public Func<DateTime> RealNow { get; set; }

        public const string SpinningTopItem = "item.spinning_top", RelicCoinItem = "item.relic_coin", OldBootItem = "item.old_boot",
            MessageBottleItem = "item.message_bottle", BeehiveItem = "item.beehive", ScarecrowItem = "item.scarecrow",
            PloughshareItem = "item.sword_ploughshare", ScarecrowInteraction = "confide_scarecrow";
        public const int JohanCropsMin = 3, JohanCropsMax = 4;
        /// <summary>Kılıçtan Saban hanede durdukça çapa işi bu kadar hızlanır (kullanıcı 20.11.2026: gerçek alet).</summary>
        public const float PloughshareHoeBonus = 1.05f;
        public const int SheepRange = 8;
        public const float SheepEnergy = 6f, ScarecrowSocial = 15f, HowlFun = 12f;

        private AnimalSystem _oddAnimals;

        private void WireOddityEvents(int seed)
        {
            OddEvents = new OddityEvents(seed != 0 ? Seeds.Derive(seed, 58) : 58) { Book = Oddities };
            Tools.Bonus = id => id == "hoe" && OwnsPloughshare() ? PloughshareHoeBonus : 1f;
            Villagers.GiftGiven += OnOddGift;
            Upbringing.Showed += OnOddShow;
            // Tuhaf vasiyet: nadiren kim olursa olsun "mezarıma topacımı koyun".
            Requests.OddBequest = () => OddEvents.Chance(OddityEvents.OddBequest, OddityEvents.OddBequestChance);
            Requests.Posted += r => { if (r != null && r.TemplateId == Thatchwick.Core.Life.Requests.RequestCatalog.OddBequestId) Oddities.Mark(OddityIds.OddBequest); };
        }

        private void Odd(string eventId, params (string name, object value)[] args) => Odd(eventId, null, 0, args);

        /// <summary>Tuhaflığı işaretler ve satırını yollar: core.oddity.ev.{olay}[.{çeşit}].</summary>
        private void Odd(string eventId, string variant, int who, params (string name, object value)[] args)
        {
            Oddities.Mark(OddityIds.Of(eventId));
            OddityLine?.Invoke(LocText.Of("core.oddity.ev." + eventId + (variant != null ? "." + variant : ""), args), who);
        }

        private IEnumerable<Animal> OwnedAnimals(string speciesPart)
        {
            if (_oddAnimals == null) yield break;
            foreach (var a in _oddAnimals.Animals)
                if (!a.IsDead && a.Species.Id.Contains(speciesPart)) yield return a;
        }

        private Animal FirstAnimal(string speciesPart) { foreach (var a in OwnedAnimals(speciesPart)) return a; return null; }

        /// <summary>Kılıçtan Saban hanede mi (envanterde ya da bir yere konmuş).</summary>
        public bool OwnsPloughshare()
        {
            if (House.Count(PloughshareItem) > 0) return true;
            foreach (var it in World.Items) if (it.Def.Id == PloughshareItem) return true;
            return false;
        }

        private bool HasPlaced(string itemId) { foreach (var it in World.Items) if (it.Def.Id == itemId) return true; return false; }

        // ---- günlük olaylar ----

        internal void OddEventsNewDay(long day)
        {
            var ev = OddEvents;
            // Kulaktan kulağa: dünkü söylenti abartılmış hâliyle döner.
            var rumor = ev.TakeRumor(day);
            if (rumor.HasValue) OddityLine?.Invoke(LocText.Of("core.oddity.ev.rumor." + rumor.Value.kind, ("who", rumor.Value.name)), 0);

            JohanWickVisit(day);

            if (Regions.IsRegional(World.Map) && ev.Chance(OddityEvents.HorselessKnight, OddityEvents.KnightChance))
                AtDayTime(KnightArrives, () =>
                {
                    StartKnightWalk();   // köy yolunu bir uçtan öbürüne yürür; yol yoksa yalnızca satır
                    Odd(OddityEvents.HorselessKnight);
                });
            if (ev.Chance(OddityEvents.Crier, OddityEvents.CrierChance)) Odd(OddityEvents.Crier, "v" + ev.Pick(3), 0);

            var goat = FirstAnimal("goat");
            if (goat != null && ev.Chance(OddityEvents.ScreamingGoat, OddityEvents.GoatChance))
                Odd(OddityEvents.ScreamingGoat, ev.Pick(2) == 0 ? "scream" : "roof", 0, ("animal", goat.Name));

            var cat = FirstAnimal("animal.cat");
            if (cat != null)
            {
                if (ev.Chance(OddityEvents.CatGift, OddityEvents.CatGiftChance)) Odd(OddityEvents.CatGift, ("animal", cat.Name));
                if (ev.Chance(OddityEvents.AlexDecor, OddityEvents.AlexChance))
                {
                    // Süs yere düşer ama kaybolmaz, bozulmaz: bir karakter "Yerine koy" diyene kadar yerde durur.
                    var knocked = KnockDecor();
                    var decor = knocked != null ? World.GetItem(knocked.ItemId) : null;
                    if (decor != null) Odd(OddityEvents.AlexDecor, null, 0, ("animal", cat.Name), ("item", decor.Def.Id));
                }
            }

            if ((Clock.Season == Season.Summer || Clock.Season == Season.Autumn) && HasPlaced(BeehiveItem)
                && ev.Chance(OddityEvents.HoneyThief, OddityEvents.HoneyChance))
            {
                Journal.Mark("wild.bear", JournalMark.Seen);
                Odd(OddityEvents.HoneyThief);
            }

            var servant = Home.Servant;
            if (servant != null && ev.Chance(OddityEvents.ServantCaught, OddityEvents.ServantChance))
                Odd(OddityEvents.ServantCaught, ev.Pick(2) == 0 ? "hat" : "nap", 0, ("servant", servant.Name), ("who", HeadName()));

            // 1 Nisan (gerçek tarih): tellal yalan söyler, inekler ters möler. Gerçek yılda bir kez.
            if (RealNow != null)
            {
                var now = RealNow();
                bool forced = ev.IsForced(OddityEvents.AprilFirst);
                if ((now.Month == 4 && now.Day == 1 && ev.Get("april.year") != now.Year) || forced)
                {
                    ev.Chance(OddityEvents.AprilFirst, 0);   // zorlamayı tüket
                    ev.Set("april.year", now.Year);
                    Odd(OddityEvents.AprilFirst, "crier", 0);
                    if (FirstAnimal("animal.cow") != null) OddityLine?.Invoke(LocText.Of("core.oddity.ev.april_first.cows"), 0);
                }
            }
        }

        /// <summary>Johan Wick: köpeği sever, 3-4 ekini dağıtır, bir sikke bırakır; o gece kurt baskını varsa kurtlar kaybolur.</summary>
        private void JohanWickVisit(long day)
        {
            var dog = FirstAnimal("animal.dog");
            if (dog == null) return;
            foreach (var d in OwnedAnimals("animal.dog"))
                if (string.Equals(d.Name, "Daisy", StringComparison.OrdinalIgnoreCase)) { dog = d; break; }
            bool daisy = string.Equals(dog.Name, "Daisy", StringComparison.OrdinalIgnoreCase);
            if (!OddEvents.Chance(OddityEvents.JohanWick, OddityEvents.JohanChance * (daisy ? OddityEvents.JohanDaisyFactor : 1))) return;

            // Kuşluk vakti gelir (gün gece yarısı döner; o saatte kimse görmez). Bayrak kayda yazılır: ziyaret
            // başlamadan ya da sürerken kaydedilirse yüklemede tamamlanır.
            OddEvents.Set(JohanFlag, 1);
            int dogId = dog.Id;
            AtDayTime(JohanArrives, () =>
            {
                var d = AnimalById(dogId) ?? FirstAnimal("animal.dog");
                if (d == null) { OddEvents.Clear(JohanFlag); return; }
                // Haritada görünür: kenardan girer, köpeğe yürür, sever, ekinleri dağıtır, sikkeyi bırakıp çıkar.
                if (!StartJohanWalk(d)) JohanInstant(d);
            });
        }

        /// <summary>Yol yoksa (köpek kapalı yerde, arsa kenarı kapalı) ziyaret görünmeden, anında işlenir.</summary>
        private void JohanInstant(Animal dog)
        {
            dog.Friendship = Math.Min(100f, dog.Friendship + 5f);
            var crops = new List<ItemInstance>();
            foreach (var item in World.Items) if (item.IsPlant && item.Def.IsCrop) crops.Add(item);
            int n = Math.Min(crops.Count, JohanCropsMin + OddEvents.Pick(JohanCropsMax - JohanCropsMin + 1));
            for (int i = 0; i < n; i++)
            {
                int k = OddEvents.Pick(crops.Count);
                World.RemoveItem(crops[k].Id);
                crops.RemoveAt(k);
            }
            _johanCount = n;
            FinishJohan(dog.Name);
        }

        // ---- sohbet ----

        internal void OnOddChat(Character a, Character b)
        {
            if (a.IsVillager || !b.IsVillager) return;
            var ev = OddEvents;
            long day = Clock.DayIndex;
            // İsim kardeşi: aynı adı taşıyan köylüyle ilk sohbet.
            if (string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase) && !ev.Has("twin:" + a.Id + ":" + b.Id))
            {
                ev.Set("twin:" + a.Id + ":" + b.Id, 1);
                Odd(OddityEvents.NameTwin, null, a.Id, ("who", a.Name), ("other", b.Name));
                return;
            }
            // İsim sürprizi: belirli adlara köylünün tepkisi (karakter başına bir kez).
            string surprise = NameSurpriseKey(a.Name);
            if (surprise != null && !ev.Has("name:" + a.Id))
            {
                ev.Set("name:" + a.Id, 1);
                Odd(OddityEvents.NameSurprise, surprise, a.Id, ("who", a.Name), ("other", b.Name));
                return;
            }
            if (AnimalTalk.HasNickname(a) && ev.FirstToday("nick:" + a.Id, day) && ev.Chance(OddityEvents.NicknameTalk, OddityEvents.NicknameTalkChance))
            {
                OddityLine?.Invoke(LocText.Of("core.oddity.ev.nickname_talk", ("who", a.Name), ("other", b.Name)), a.Id);
                return;
            }
            // Aynı kıyafet: bir mevsimdir üstünü değiştirmeyene köylü söylenir (giyim sistemi açıksa).
            if (_clothing != null && _clothing.HasWardrobe(a) && ev.SameOutfitTooLong(a.Id, Clock.TotalDays, Clock.Config.DaysPerSeason))
            {
                OddityLine?.Invoke(LocText.Of("core.oddity.ev.same_outfit", ("who", a.Name), ("other", b.Name)), a.Id);
                return;
            }
            // Dalgın dede: yaşlıyla sohbet.
            var elder = a.Stage == AgeStage.Elder ? a : b.Stage == AgeStage.Elder ? b : null;
            if (elder != null && ev.Chance(OddityEvents.AbsentElder, OddityEvents.ElderChance))
            {
                Odd(OddityEvents.AbsentElder, null, a.Id, ("who", elder.Name));
                return;
            }
            // Kulaktan kulağa: bugün yaptığı işi köylüye anlatır; yarın abartılmış hâli döner.
            ev.OnGossip(a.Id, a.Name, day);
        }

        /// <summary>Belirli adlara tepki. Yeni ad eklemek için buraya ve ui tablolarına satır eklenir.</summary>
        public static string NameSurpriseKey(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            // Kullanıcının seçtiği adlar (20.11.2026). Boşluklar teke iner; büyük-küçük harf ayrımı yok.
            string key = string.Join(" ", name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
            switch (key)
            {
                case "DORUK": return "doruk";
                case "SANEM": return "sanem";
                case "BORA": return "bora";
                case "ALPASLAN": return "alpaslan";
                case "ALEX": return "alex";
                case "LONTA": return "lonta";
                case "LONTAJ": return "lontaj";
                case "KAOSLONTAJ": return "kaoslontaj";
                case "SANEM BAL": return "sanem_bal";
                case "MARS": return "mars";
                default: return null;
            }
        }

        // ---- işler, eşyalar, hayvanlar ----

        private void OnOddWork(Character c, SkillType? skill, string interactionId)
        {
            if (c == null || c.IsVillager || !skill.HasValue) return;
            string kind = skill == SkillType.Fishing ? "fish" : skill == SkillType.Woodcutting ? "wood" : skill == SkillType.Farming ? "farm"
                : skill == SkillType.Mining ? "mine" : null;
            if (kind != null) OddEvents.NoteWork(c.Id, kind, Clock.DayIndex);
        }

        internal void OnOddItemUsed(Character c, ItemInstance item, InteractionDef interaction)
        {
            if (c == null || item == null || interaction == null || c.IsVillager) return;
            // Korkuluk: çok yalnız kalan cevap aldığını sanır.
            if (interaction.Id == ScarecrowInteraction)
            {
                bool lonely = c.Mood.Has(MoodCatalog.Lonely);
                if (lonely) c.Needs.Restore(NeedType.Social, ScarecrowSocial);
                Odd(OddityEvents.Scarecrow, lonely ? "answered" : "silent", c.Id, ("who", c.Name));
                return;
            }
            // Koyun saymak: ağılın yanında yatan daha iyi dinlenir.
            if (JobRunner.IsRest(interaction))
            {
                foreach (var sheep in OwnedAnimals("animal.sheep"))
                {
                    if (sheep.Position.Z != c.Position.Z || Math.Abs(sheep.Position.X - c.Position.X) + Math.Abs(sheep.Position.Y - c.Position.Y) > SheepRange) continue;
                    c.Needs.Restore(NeedType.Energy, SheepEnergy);
                    if (!OddEvents.Has("sheep:" + c.Id)) { OddEvents.Set("sheep:" + c.Id, 1); Odd(OddityEvents.CountingSheep, null, c.Id, ("who", c.Name)); }
                    else Oddities.Mark(OddityIds.CountingSheep);
                    break;
                }
            }
        }

        /// <summary>Yeni eşya kondu: öküz gidip bakar; sandığa ya da sepete kedi girer.</summary>
        internal void OnOddFurnished(ItemInstance item)
        {
            if (item == null || !World.Map.IsOwned(item.Position)) return;
            long day = Clock.DayIndex;
            var cat = FirstAnimal("animal.cat");
            if (cat != null && item.Def.Storage > 0 && OddEvents.Chance(OddityEvents.BoxCat, OddityEvents.BoxCatChance) && OddEvents.FirstToday("boxcat", day))
            {
                SendAnimalToItem(cat, item, "box", SceneBook.BoxDays);      // kedi yürür, varınca kutunun içinde görünür
                Odd(OddityEvents.BoxCat, null, 0, ("animal", cat.Name), ("item", item.Def.Id));
                return;
            }
            var ox = FirstAnimal("animal.ox");
            if (ox != null && OddEvents.Chance(OddityEvents.OxStare, OddityEvents.OxChance) && OddEvents.FirstToday("oxstare", day))
            {
                SendAnimalToItem(ox, item, "stare", SceneBook.StareDays);   // öküz gider, yeni yapılanın karşısında durur
                Odd(OddityEvents.OxStare, null, 0, ("animal", ox.Name), ("item", item.Def.Id));
            }
        }

        private void OnOddAnimalCare(Character c, Animal a, AnimalAction action)
        {
            if (c == null || a == null) return;
            var ev = OddEvents;
            if (action == AnimalAction.Shoo)
            {
                // Tavuk intikamı: aynı gün üçüncü kışkışlamada bütün kümes peşine düşer (hasar yok).
                int n = ev.CountToday("shoo:" + c.Id, Clock.DayIndex);
                if (n == OddityEvents.ShooLimit || ev.Chance(OddityEvents.ChickenRevenge, 0))
                {
                    c.Needs.Restore(NeedType.Fun, 10f);
                    c.GainSkillXp(SkillType.Athletics, 4);
                    Odd(OddityEvents.ChickenRevenge, null, c.Id, ("who", c.Name));
                }
                else if (n < OddityEvents.ShooLimit)
                    OddityLine?.Invoke(LocText.Of("core.oddity.ev.shoo", ("who", c.Name), ("animal", a.Name)), c.Id);
                return;
            }
            if (action != AnimalAction.Pet && action != AnimalAction.Brush) return;
            if (a.Species.Id == "animal.dog")
            {
                // Bininci sevişte köpek gömdüğü bir şeyi getirir: çoğunlukla kemik, çok nadiren değerli bir şey.
                double pets = ev.Add("pets:" + a.Id);
                if (pets == OddityEvents.DogGiftPets || ev.Chance(OddityEvents.DogGift, 0))
                {
                    bool relic = ev.Roll() < OddityEvents.DogRelicChance && Content.Items.Contains(RelicCoinItem);
                    string gift = relic ? RelicCoinItem : "item.bone";
                    if (Content.Items.Contains(gift)) House.AddItem(gift, 1);
                    Odd(OddityEvents.DogGift, relic ? "relic" : "bone", c.Id, ("animal", a.Name), ("who", c.Name));
                }
            }
            else if (a.Species.Id == "animal.cat")
            {
                // Kıskanç köpek: kedi sevilirken bugün sevilmemiş köpek araya girer.
                foreach (var dog in OwnedAnimals("animal.dog"))
                {
                    if (dog.PetCooldownDays > 0) continue;
                    if (ev.FirstToday("jealous", Clock.DayIndex) && ev.Chance(OddityEvents.JealousDog, OddityEvents.JealousChance))
                        Odd(OddityEvents.JealousDog, null, c.Id, ("animal", dog.Name), ("other", a.Name));
                    break;
                }
            }
        }

        internal void OnOddFish(Character c, string fishId)
        {
            if (c == null || c.IsVillager) return;
            var ev = OddEvents;
            if (Content.Items.Contains(OldBootItem) && ev.Chance(OddityEvents.OldBoot, OddityEvents.BootChance))
            {
                House.AddItem(OldBootItem, 1);
                Odd(OddityEvents.OldBoot, null, c.Id, ("who", c.Name));
            }
            if (Content.Items.Contains(MessageBottleItem) && ev.Chance(OddityEvents.MessageBottle, OddityEvents.BottleChance))
            {
                House.AddItem(MessageBottleItem, 1);
                Odd(OddityEvents.MessageBottle, "v" + ev.Pick(3), c.Id, ("who", c.Name));
            }
            // Dev balık: köylüler inanmaz.
            if (Content.Items.TryGet(fishId, out var fish) && fish.Price >= OddityEvents.FishTalePrice && ev.Chance(OddityEvents.FishTale, OddityEvents.FishTaleChance))
                Odd(OddityEvents.FishTale, null, c.Id, ("who", c.Name), ("item", fishId));
        }

        /// <summary>Madende kazı (oyun katmanı MineSystem.Dug'dan çağırır): taş tablet ve taşa saplı kılıç, kayıt başına birer kez.</summary>
        public void OnMineDug(Character c)
        {
            if (c == null || c.IsVillager) return;
            var ev = OddEvents;
            if (!ev.Has("mine.tablet"))
            {
                if (!ev.Chance(OddityEvents.MineTablet, OddityEvents.TabletChance)) return;
                ev.Set("mine.tablet", 1);
                Odd(OddityEvents.MineTablet, null, c.Id, ("who", c.Name));
                return;
            }
            if (!ev.Has("mine.sword") && ev.Chance(OddityEvents.SwordInStone, OddityEvents.StoneSwordChance))
            {
                ev.Set("mine.sword", 1);
                Odd(OddityEvents.SwordInStone, null, c.Id, ("who", c.Name));
            }
        }

        /// <summary>Topaç şovu: usta çevirir, seyredenler dönemin meme diliyle tepki verir; çok nadiren topaç hiç durmaz.</summary>
        internal void OnOddShow(Thatchwick.Core.Upbringing.ToyShow show)
        {
            if (show?.Toy == null || show.Performer == null || show.Toy.ItemId != SpinningTopItem) return;
            if (show.Kind != Thatchwick.Core.Upbringing.ShowKind.Show) return;
            Scenes.SetCharacterScene(show.Performer.Id, "top", SceneBook.TopShowDays);   // topaç çevirme karesi oynar
            var ev = OddEvents;
            if (ev.Chance(OddityEvents.NeverStoppingTop, OddityEvents.NeverStoppingChance))
            {
                Odd(OddityEvents.NeverStoppingTop, null, show.Performer.Id, ("who", show.Performer.Name));
                return;
            }
            if (show.Watchers > 0 && ev.Chance(OddityEvents.TopShow, OddityEvents.TopShowChance))
                Odd(OddityEvents.TopShow, "v" + ev.Pick(4), show.Performer.Id, ("who", show.Performer.Name));
        }

        /// <summary>İade hediye: dükkâncıya kendi sattığı mal verilirse bozulur.</summary>
        internal void OnOddGift(Character villager, ItemDef item, GiftTaste taste, string message)
        {
            if (villager == null || item == null) return;
            var def = Villagers.DefOf(villager);
            if (def?.Work == null) return;
            foreach (var shop in Thatchwick.Core.Economy.Shops.All)
            {
                if (shop.WorkKey != def.Work || shop.Goods == null || !shop.Goods(item)) continue;
                Odd(OddityEvents.Regift, null, 0, ("who", villager.Name), ("item", item.Id));
                return;
            }
        }

        /// <summary>Attan düşme: önceki kuşaklardan biri de düşmüşse yaşlı köylü hatırlar.</summary>
        internal void OnOddFall(Character rider)
        {
            if (rider == null || rider.IsVillager) return;
            string ancestor = OddEvents.OnFall(rider.Name, rider.Generation);
            if (ancestor != null) OddityLine?.Invoke(LocText.Of("core.oddity.ev.village_memory", ("who", rider.Name), ("other", ancestor)), rider.Id);
        }

        // ---- oyun katmanının çağırdıkları ----

        /// <summary>Meyhaneden bir şey alındı: "her zamankinden mi?" ve torunlara "dedeninkinden mi?".</summary>
        public void OnTavernOrder(Character buyer, string itemId)
        {
            if (buyer == null) return;
            int r = OddEvents.OnTavernOrder(itemId, buyer.Name, buyer.Generation, Clock.DayIndex);
            if (r == 1) OddityLine?.Invoke(LocText.Of("core.oddity.ev.the_usual", ("who", buyer.Name), ("item", itemId)), buyer.Id);
            else if (r == 2) OddityLine?.Invoke(LocText.Of("core.oddity.ev.the_usual.heir", ("who", buyer.Name), ("other", OddEvents.UsualName), ("item", itemId)), buyer.Id);
        }

        /// <summary>Uluma: dolunay gecesi ve hanede köpek varken.</summary>
        public string HowlBlocker(Character c)
        {
            if (c == null || c.Stage == AgeStage.Baby) return "Bebek ulumaz.";
            if (FirstAnimal("animal.dog") == null) return "Köpek yok.";
            if (!OddityEvents.IsFullMoon(Clock.DayIndex) || !OddityEvents.IsNight(Clock.DayFraction)) return "Yalnızca dolunay gecesi.";
            if (OddEvents.Has("day:howl") && (long)OddEvents.Get("day:howl") == Clock.DayIndex) return "Bu gece ulundu.";
            return null;
        }

        public bool CanHowl(Character c) => OddEvents.IsForced(OddityEvents.Howl) || HowlBlocker(c) == null;

        /// <summary>Ulu: önce köpekler, sonra çocuklar katılır; uzaktan bir kurt cevap verir.</summary>
        public bool Howl(Character c)
        {
            if (c == null) return false;
            bool forced = OddEvents.Chance(OddityEvents.Howl, 0);
            if (!forced && HowlBlocker(c) != null) return false;
            OddEvents.FirstToday("howl", Clock.DayIndex);
            int kids = 0;
            foreach (var m in Household())
            {
                if (m.Stage != AgeStage.Child && m.Stage != AgeStage.Teen) continue;
                m.Needs.Restore(NeedType.Fun, HowlFun);
                kids++;
            }
            c.Needs.Restore(NeedType.Fun, HowlFun);
            var dog = FirstAnimal("animal.dog");
            // Sahne: köpek durup başını kaldırır, ulayan ve çocuklar uluma karesini oynar.
            foreach (var d in OwnedAnimals("animal.dog"))
                if (_oddAnimals.SendTo(d, d.NextTile ?? d.Position, SceneBook.HowlDays)) Scenes.SetPose(d.Id, "howl", d.NextTile ?? d.Position, SceneBook.HowlDays + 0.01);
            Scenes.SetCharacterScene(c.Id, "howl", SceneBook.HowlCharacterDays);
            foreach (var m in Household())
                if (m.Stage == AgeStage.Child || m.Stage == AgeStage.Teen) Scenes.SetCharacterScene(m.Id, "howl", SceneBook.HowlCharacterDays);
            Odd(OddityEvents.Howl, kids > 0 ? "kids" : "alone", c.Id, ("who", c.Name), ("animal", dog?.Name ?? ""));
            return true;
        }
    }
}
