using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Work
{
    /// <summary>
    /// İş etkileşimlerini haneye ve dünyaya uygular: kaynak harcar (odun yararken kütük), ürün verir
    /// (ağaç kesince kütük ve fidan), eşyayı kaldırır (kesilen ağaç). Olasılıklı ürünler tohumlu
    /// rastgele sayıyla belirlenir (testlerde tekrarlanabilir).
    /// </summary>
    public sealed class InteractionEffects : IInteractionEffects
    {
        private readonly WorldState _world;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly Random _rng;

        /// <summary>İş bitince (karakter, etkileşim, özet metin: "+3 Kütük, +1 Meşe fidanı").</summary>
        public event Action<Character, InteractionDef, string> Applied;
        /// <summary>Etkileşim uygulandı (eşyasıyla birlikte).</summary>
        public event Action<Character, ItemInstance, InteractionDef> AppliedOn;
        /// <summary>Ekin hasat edildi (karakter, ekin, kalite basamağı 0-2): hasat kurdelesi (D3b).</summary>
        public event Action<Character, ItemInstance, int> CropHarvested;
        /// <summary>Balık tutuldu (karakter, balık eşyası): ilk balık hatırası (D3b).</summary>
        public event Action<Character, string> FishCaught;

        /// <summary>Oyuncu kendisi mi balık tutuyor (mini oyun, belge 50). True ise vuran balık <see cref="FishBite"/> ile bildirilir, <see cref="GrantFish"/> bekler.</summary>
        public Func<Character, bool> PlayerFishing { get; set; }
        public event Action<Character, ItemInstance, string, int> FishBite;

        /// <summary>Mini oyunda tutulan balık envantere (depoya) girer.</summary>
        public void GrantFish(Character character, string fishId, int count, bool perfect = false)
        {
            // Kalite (53 §4.3): Balıkçılık tablosu; kusursuz tutuş +1; efsane balık her zaman Has.
            int level = character?.Skills.GetLevel(SkillType.Fishing) ?? 1;
            bool graded = _content.Items.TryGet(fishId, out var fd) && fd.HasQuality;
            int added = 0;
            for (int i = 0; i < count; i++)
            {
                int q = !graded ? 0 : CreatureBook.IsLegendary(fishId) ? ItemQuality.Superior
                    : Math.Min(ItemQuality.NormalMax, ItemQuality.Gathered(level, _rng) + (perfect ? 1 : 0));
                added += AddKeyed(new ItemKey(fishId, q), 1);
            }
            if (added > 0) FishCaught?.Invoke(character, fishId);
        }
        /// <summary>Boş cam şişe (D3 tamamlama): iksir yapılırken biri harcanır, bir iksir fazla çıkar. null ise kapalı.</summary>
        public string BottleItemId { get; set; } = "item.glass_bottle";

        /// <summary>Depo kapasitesi; null ise sınırsız. Dolunca fazla ürün kaybolur ve özette yazılır.</summary>
        public StorageService Storage { get; set; }

        /// <summary>Hayvan sistemi (barınakta hayvan var mı); null ise hayvan gerektiren işler başlamaz.</summary>
        public AnimalSystem Animals { get; set; }

        /// <summary>Karo sıcaklığı (buz kesmek için don); null ise don yok sayılır.</summary>
        public Func<GridPos, float> TemperatureAt { get; set; }

        /// <summary>Hane üyeleri (ders vermek için); null ise ders verilemez.</summary>
        public Func<IEnumerable<Character>> Household { get; set; }
        /// <summary>Etkilerin işlediği hane envanteri (görev planlayıcı odun sayısını buradan okur).</summary>
        public Household House => _house;
        public ContentDatabase Content => _content;

        /// <summary>Şu anki mevsim (balık türleri); null ise her mevsimin ürünü çıkabilir.</summary>
        public Func<Time.Season> Season { get; set; }

        /// <summary>Tesisata (çeşme, akarsulu hela, borulu küvet) şu an su geliyor mu (su işleri).</summary>
        public Func<ItemInstance, bool> PipedWater { get; set; }
        private bool Piped(ItemInstance item) => item != null && item.Def.IsFixture && PipedWater != null && PipedWater(item);

        /// <summary>Buzluk en çok bu kadar günlük buz alır.</summary>
        public const float MaxIceDays = 30f;

        /// <summary>Envanterdeki bir yiyeceğin en eski partisinin yaşı (doldurulan dekora o yaşla girer); null ise taze.</summary>
        public Func<string, double> FoodAge { get; set; }

        /// <summary>Çiftçilik sistemi (toprak verimi, kalite); null ise hasat kalitesiz ve toprak yorulmaz.</summary>
        public PlantSystem Plants { get; set; }

        /// <summary>Aile tarifinden gelen ek ürün şansı (beceri → oran; belge 34-D1). Null: yok.</summary>
        public Func<SkillType, double> FamilyYieldChance { get; set; }

        /// <summary>Ek başlama engeli (ev bakımı, belge 45: Life.HomeLife.CanStart); null ise yok.</summary>
        public Func<Character, ItemInstance, InteractionDef, string> ExtraBlocker { get; set; }

        public InteractionEffects(WorldState world, Household house, ContentDatabase content, int seed = 1)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _rng = new Random(seed);
        }

        private string Name(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        private int AddAll(string id, int count)
        {
            _house.AddItem(id, count);
            return count;
        }

        private int AddKeyed(ItemKey key, int count)
        {
            if (count <= 0) return 0;
            if (Storage != null) return Storage.AddUpTo(key, count);
            _house.Add(key, count);
            return count;
        }

        // ---- kalite (Project belgesi 53) ----

        /// <summary>Kalite defteri: tezgâhta "En iyi malzeme", "Ustalık eseri dene", eser kaydı. Null ise kalite seçimleri yok.</summary>
        public Life.QualityBook Quality { get; set; }
        /// <summary>Bugünün gün indeksi ve yılı (ustalık eseri günde bir; eser yazısındaki yıl).</summary>
        public Func<long> DayIndex { get; set; }
        public Func<int> Year { get; set; }
        public Func<double> Today { get; set; }
        /// <summary>Günün saati (0-24): en iyi saat kuralı için (safran şafakta); null ise kural çalışmaz.</summary>
        public Func<double> HourOfDay { get; set; }
        /// <summary>Aşçının bildiği aile tarifinin kuşak sayısı ve adı (yadigâr sofra: 3 kuşaklık tarifle ustalık eseri). Null: yok.</summary>
        public Func<Character, SkillType, (int generations, string name)> FamilyRecipe { get; set; }
        /// <summary>Son işte harcanan kaliteli girdinin en yüksek kalitesi (yeme: doyum ve sofra ruh hali; JobRunner okur).</summary>
        public int LastConsumedQuality { get; private set; }
        /// <summary>Ustalık eseri yapıldı (karakter, eşya, kalite).</summary>
        public event Action<Character, string, int> MasterworkMade;

        private static readonly SkillType[] GatherSkills = { SkillType.Foraging, SkillType.Fishing, SkillType.Hunting, SkillType.Horticulture, SkillType.Farming };

        // Rozet dökümü (dilden bağımsız): "★2 ★★1".
        private static string Breakdown(int[] byQuality)
        {
            var parts = new List<string>();
            for (int q = 1; q < byQuality.Length; q++) if (byQuality[q] > 0) parts.Add(ItemQuality.Badge(q) + byQuality[q]);
            return parts.Count > 0 ? " (" + string.Join(" ", parts) + ")" : "";
        }

        public string CanStart(Character character, ItemInstance item, InteractionDef interaction)
        {
            // Onarılacak eşya yoksa malzeme sorulmaz (önce asıl neden).
            if (interaction.RepairMaterial != null && FindRepairTarget(interaction.RepairMaterial, out _, out _) == null)
                return $"Onarılacak hasarlı {Build.DecorSystem.MaterialName(interaction.RepairMaterial)} eşya yok.";
            foreach (var c in interaction.AllConsumed())
            {
                int have = _house.Count(c.ItemId);
                if (have < c.Count)
                    return c.Count == 1 ? $"{Name(c.ItemId)} yok." : $"{Name(c.ItemId)} yetmiyor ({have}/{c.Count}).";
            }
            if (interaction.RequiresAnimal && (Animals == null || Animals.Residents(item.Id) == 0)) return "Burada hayvan yok.";
            if (interaction.Collect && item.StockTotal == 0) return "Toplanacak ürün yok.";
            if (interaction.Teach && Pupils(character).Count == 0) return "Ders verilecek çocuk ya da ergen yok.";
            if (interaction.EmptyFill && item.Fill <= 0) return "Boşaltılacak bir şey yok.";
            if (interaction.EmptyFill && item.Def.FillProduct != null && Storage != null && !Storage.HasRoomFor(item.Def.FillProduct))
                return "Depoda gübreye yer yok: gübreyi sat, tarlaya ser ya da depoyu büyüt.";
            if (interaction.RequiresFrost && (TemperatureAt == null || TemperatureAt(item.Position) > 0f)) return "Buz yok: don olunca (kışın) kesilir.";
            if (interaction.NeedsPipedWater && !Piped(item)) return "Su gelmiyor: bir kaynağa bağlı boruya bağla.";
            if (interaction.AddIce > 0f && item.FuelDays + interaction.AddIce > MaxIceDays + 1e-6) return $"{item.Def.DisplayName} buzla dolu.";
            if (interaction.Water)
            {
                if (!item.IsPlant || !item.Def.IsCrop) return "Bu sulanacak bir ekin değil.";
                if (item.IsGrown) return "Ekin olgun, sulamaya gerek yok.";
                if (item.Watered) return "Zaten sulu.";
            }
            if (interaction.Fertilize > 0f)
            {
                if (!item.IsPlant || !item.Def.IsCrop) return "Gübre ekinin toprağına atılır.";
                if (Plants != null && Plants.FertilityAt(item.Position) >= 0.99f) return "Toprak zaten çok verimli.";
            }
            if (interaction.Slaughter)
            {
                if (Animals == null) return "Burada hayvan yok.";
                var why = Animals.SlaughterBlocker(item.Id);
                if (why != null) return why;
            }
            if (interaction.PetsAnimals && Animals != null && Animals.Residents(item.Id) > 0 && Animals.PettableResidents(item.Id) == 0)
                return "Hayvanlar bugün zaten sevildi.";
            if (interaction.RequiresFruit && !item.FruitReady)
                return item.IsGrown ? "Meyve henüz olgunlaşmadı." : $"{item.Def.DisplayName} henüz büyümedi.";
            if (interaction.Polish && !item.IsTarnished) return "Ayna zaten parlak.";
            if (interaction.Embellish && (item.Embellished || item.Quality >= ItemQuality.NormalMax)) return item.Embellished ? "Bu eşya zaten süslendi." : "Bu eşya zaten has.";
            if (interaction.CollectContents && !item.HasContents) return item.Def.IsFillDecor ? $"{item.Def.DisplayName} zaten boş." : "Toplanacak bir şey yok.";
            if (interaction.FillDecor) { var why = DecorSystem.FillBlocker(item, _house, _content); if (why != null) return why; }
            if (interaction.EatContents && DecorSystem.EdibleCount(item, _content) == 0)
                return item.HasContents ? "İçindekiler çürümüş; önce boşalt." : $"{item.Def.DisplayName} boş.";
            if (ExtraBlocker != null) return ExtraBlocker(character, item, interaction);
            return null;
        }

        /// <summary>
        /// Onarılacak eşya: önce envanterdeki (en değerli) hasarlı eşya, yoksa yerleştirilmiş hasarlı eşya. Bulunan
        /// envanter kimliği ya da dünya örneği döner; hiçbiri yoksa null.
        /// </summary>
        private string FindRepairTarget(string material, out ItemInstance placed, out ItemDef def)
        {
            placed = null; def = null;
            string best = null; int bestPrice = -1;
            foreach (var kv in _house.DamagedList())
            {
                if (!_content.Items.TryGet(kv.Key, out var d) || d.EffectiveMaterial != material) continue;
                if (d.Price > bestPrice) { best = kv.Key; bestPrice = d.Price; def = d; }
            }
            if (best != null) return best;
            foreach (var it in _world.Items)
            {
                if (it.Condition != ItemCondition.Damaged || it.Def.EffectiveMaterial != material) continue;
                if (placed == null || it.Def.Price > placed.Def.Price) placed = it;
            }
            if (placed != null) { def = placed.Def; return placed.Def.Id; }
            return null;
        }

        public void Apply(Character character, ItemInstance item, InteractionDef interaction)
        {
            var consumed = interaction.AllConsumed();
            foreach (var c in consumed)
                if (_house.Count(c.ItemId) < c.Count) return; // bu arada harcanmış: ürün yok
            // Harcama sırası (53 §4.7): en düşük kalite önce; tezgâhta "En iyi malzemeyi kullan" seçiliyse en iyisi.
            string recipeKey = item != null ? Life.QualityBook.RecipeKey(item.Def, interaction) : null;
            var takePolicy = Quality != null && Quality.BestIngredients(recipeKey) ? TakePolicy.Highest : TakePolicy.Lowest;
            var inputQualities = new List<int>();
            LastConsumedQuality = 0;
            foreach (var c in consumed)
                for (int k = 0; k < c.Count; k++)
                    if (_house.TryTake(c.ItemId, takePolicy, out var took) && _content.Items.TryGet(c.ItemId, out var inDef) && inDef.HasQuality)
                    {
                        inputQualities.Add(took.Quality);
                        LastConsumedQuality = Math.Max(LastConsumedQuality, took.Quality);
                    }

            if (interaction.AddFeed > 0) AnimalSystem.AddFeed(item, interaction.AddFeed);
            if (interaction.AddIce > 0f) item.FuelDays = Math.Min(MaxIceDays, item.FuelDays + interaction.AddIce);

            var farmNotes = new List<string>();
            int husbandry = character.Skills.GetLevel(SkillType.AnimalHusbandry);
            if (interaction.BefriendAnimals > 0f && Animals != null)
            {
                int n = Animals.Befriend(item.Id, interaction.BefriendAnimals * AnimalSystem.HusbandryFactor(husbandry), out int cured);
                if (n > 0) farmNotes.Add($"{n} hayvan merhemlendi");
                if (cured > 0) farmNotes.Add($"{cured} hasta hayvan iyileşti");
            }
            if (interaction.PetsAnimals && Animals != null)
            {
                int petted = Animals.PetResidents(item.Id, husbandry);
                if (petted > 0) farmNotes.Add($"{petted} hayvan sevildi");
            }
            string slaughterId = null; int slaughterCount = 0;
            string hideId = null; int hideCount = 0;
            int slaughterTier = 0;
            if (interaction.Slaughter && Animals != null && Animals.Slaughter(item.Id, out var killed, out slaughterId, out slaughterCount))
            {
                slaughterTier = Animals.QualityOf(killed);   // bakımlı hayvanın eti ve derisi kaliteli (53 §4.3)
                farmNotes.Add($"{killed.Species.DisplayName} kesildi");
                hideId = killed.Species.Hide;            // büyükbaş ve küçükbaş deri de verir (tabakhane → saraç)
                hideCount = hideId != null ? Math.Max(1, killed.Species.HideCount) : 0;
            }
            if (interaction.Water) { PlantSystem.Water(item); farmNotes.Add("sulandı"); }
            if (interaction.Teach)
            {
                int taught = Teach(character);
                if (taught > 0) farmNotes.Add($"{taught} öğrenci ders aldı");
            }
            if (interaction.Heal > 0f && character.Treat(interaction.Heal)) farmNotes.Add("yaraları iyileşti");
            // Dolu hela/lazımlığı kullanan önce kendisi boşaltır (sıkışan beklemez); boşaltılan gübre olur.
            // Akarsulu hela: su gelirken yıkanır, dolmaz (gübre çıkmaz, boşaltma işi kalmaz).
            bool flushed = interaction.AddFill > 0 && Piped(item);
            if (flushed) { if (item.Fill > 0) item.Fill = 0; }
            else
            {
                if (interaction.AddFill > 0 && item.Def.FillCapacity > 0 && item.Fill + interaction.AddFill > item.Def.FillCapacity)
                    farmNotes.Add("doluydu, boşalttı " + EmptyInto(item));
                if (interaction.AddFill > 0) item.Fill += interaction.AddFill;
            }
            if (interaction.EmptyFill && item.Def.FillProduct != null) farmNotes.Add(EmptyInto(item));
            if (interaction.Bath) farmNotes.Add(BathEffect(character, item));
            if (interaction.Wets > 0f) character.Soak(interaction.Wets);
            if (interaction.Dries && character.Wetness > 0f) { character.Dry(); farmNotes.Add("kurulandı"); }
            if (interaction.Polish && item.TarnishDays > 0) { item.TarnishDays = 0; farmNotes.Add($"{item.Def.DisplayName} parladı"); }
            if (interaction.Embellish && !item.Embellished && item.Def.HasQuality)
            {
                item.Quality = Math.Min(ItemQuality.NormalMax, item.Quality + 1);
                item.Embellished = true;
                _world.NotifyChanged();
                farmNotes.Add($"{item.Def.DisplayName} süslendi ({ItemQuality.Badge(item.Quality)})");
            }
            if (interaction.RepairMaterial != null)
            {
                string target = FindRepairTarget(interaction.RepairMaterial, out var placedTarget, out var targetDef);
                if (target != null && placedTarget == null && _house.RepairOne(target)) farmNotes.Add($"{targetDef.DisplayName} onarıldı");
                else if (placedTarget != null) { placedTarget.Condition = ItemCondition.Repaired; _world.NotifyChanged(); farmNotes.Add($"{targetDef.DisplayName} onarıldı"); }
            }
            if (interaction.FillDecor)
            {
                int added = DecorSystem.Fill(item, _house, _content, FoodAge);
                if (added > 0) { farmNotes.Add($"{item.Def.DisplayName} dolduruldu ({DecorSystem.FillLabel(item, _content)})"); _world.NotifyChanged(); }
            }
            if (interaction.EatContents)
            {
                string ate = DecorSystem.EatOne(item, _content);
                if (ate != null) { farmNotes.Add($"{Name(ate)} yedi"); _world.NotifyChanged(); }
            }
            if (interaction.CollectContents && item.HasContents && item.Def.IsFillDecor)
            {
                var (back, gone) = DecorSystem.EmptyInto(item, _house, _content);
                if (back > 0) farmNotes.Add($"+{back} envantere döndü");
                if (gone > 0) farmNotes.Add($"{gone} çürük atıldı");
                _world.NotifyChanged();
            }
            else if (interaction.CollectContents && item.HasContents)
            {
                var got = new Dictionary<string, int>();
                var gotOrder = new List<string>();
                foreach (var c in item.Contents)
                {
                    _house.AddItem(c.ItemId, 1, c.Condition);
                    string key = (c.Condition == ItemCondition.Damaged ? "hasarlı " : "") + Name(c.ItemId);
                    if (!got.ContainsKey(key)) { got[key] = 0; gotOrder.Add(key); }
                    got[key]++;
                }
                item.Contents.Clear();
                foreach (var k in gotOrder) farmNotes.Add($"+{got[k]} {k}");
            }
            if (interaction.Fertilize > 0f && Plants != null)
            {
                // Toprak bakımı: gübre seviye başına %5 daha etkili.
                Plants.Fertilize(item.Position, (float)(interaction.Fertilize * SkillNames.Factor(character.Skills.GetLevel(SkillType.SoilCare), 0.05)));
                farmNotes.Add($"toprak verimi %{Plants.FertilityAt(item.Position) * 100f:0}");
            }

            // Beceri seviyesi (+ özellik/dal kalite bonusu) ürün satırlarını seçer: aynı yemeğin sıradan ve iyi hali.
            int level = interaction.Skill.HasValue ? character.Skills.GetLevel(interaction.Skill.Value) + character.Modifiers.QualityLevels : 0;

            // Aynı ürün birden çok satırda olabilir (kesin 1 fidan + %50 ihtimalle 1 fidan): özette toplanır.
            int lost = 0;
            string parts0 = null;
            var order = new List<string>();
            var totals = new Dictionary<string, int>();
            bool firstRow = true;
            bool playerRod = interaction.Skill == SkillType.Fishing && PlayerFishing?.Invoke(character) == true;
            string bite = null; int biteCount = 0;
            // Ekin hasadında kalite: sulama düzeni, toprak verimi ve çiftçilik seviyesi ürün miktarını artırır (tohum hariç).
            bool cropHarvest = interaction.RequiresGrown && item.IsPlant && item.Def.IsCrop;
            int tier = cropHarvest && Plants != null ? Plants.QualityOf(item, level) : 0;
            float yieldMult = PlantSystem.QualityYield(tier);
            // Kalite kaynağı (53 §4.3-4.6): ekin puanı (birim başına dağılım), toplama/balık/av/meyve (toplayanın becerisi),
            // işleme (girdilerin ortalaması + usta), yapım (beceri tablosu); "Ustalık eseri dene" bütün partiyi belirler.
            int rawLevel = interaction.Skill.HasValue ? character.Skills.GetLevel(interaction.Skill.Value) : 0;
            bool processing = consumed.Count > 0 && interaction.Skill.HasValue;
            bool gathering = !processing && !cropHarvest && interaction.Skill.HasValue && Array.IndexOf(GatherSkills, interaction.Skill.Value) >= 0
                && (interaction.RemovesItem || interaction.TakesFruit || interaction.RequiresGrown || interaction.Skill == SkillType.Fishing || interaction.Skill == SkillType.Hunting);
            int levelAboveGate = level - Math.Max(1, interaction.MinSkillLevel);
            int? batchQuality = null;
            int batchMaker = 0;
            if (processing || (interaction.IsCraft && interaction.Skill.HasValue))
            {
                long day = DayIndex?.Invoke() ?? 0;
                if (Quality != null && Quality.WillAttempt(character, item, interaction, day) && ItemQuality.CanAttemptMasterwork(rawLevel, inputQualities))
                {
                    Quality.MarkAttempted(character, item, interaction, day);
                    if (_rng.NextDouble() < ItemQuality.MasterworkChance)
                    {
                        // Yadigâr sofra: yemek, ekmek ve içecekte 3 kuşaklık aile tarifiyle yapılan eser (karar 1).
                        var fam = interaction.Skill == SkillType.Cooking || interaction.Skill == SkillType.Baking || interaction.Skill == SkillType.Brewing
                            ? FamilyRecipe?.Invoke(character, interaction.Skill.Value) : null;
                        bool heirloom = fam.HasValue && fam.Value.generations >= ItemQuality.HeirloomGenerations;
                        batchQuality = heirloom ? ItemQuality.Heirloom : ItemQuality.Masterwork;
                        string mainId = null;
                        foreach (var p0 in interaction.Produce) if (_content.Items.TryGet(p0.ItemId, out var md) && md.HasQuality) { mainId = p0.ItemId; break; }
                        if (mainId != null && Quality != null)
                            batchMaker = Quality.Register(mainId, character, Today?.Invoke() ?? 0, Year?.Invoke() ?? 1, heirloom ? fam.Value.name : null, heirloom).Serial;
                        farmNotes.Add(heirloom ? "✦ yadigâr sofra" : "✦ ustalık eseri");
                        if (mainId != null) MasterworkMade?.Invoke(character, mainId, batchQuality.Value);
                    }
                    else { batchQuality = ItemQuality.Superior; farmNotes.Add("eser tutmadı (★★)"); }
                }
                else
                {
                    if (Quality != null && Quality.WillAttempt(character, item, interaction, day)) farmNotes.Add("malzeme has değil, eser denenmedi");
                    batchQuality = ItemQuality.Processed(inputQualities, levelAboveGate, rawLevel, _rng);
                }
            }
            var qualityTotals = new Dictionary<string, int[]>();
            foreach (var p in interaction.Produce)
            {
                if (p.MinLevel > 0 && level < p.MinLevel) continue;
                if (p.MaxLevel > 0 && level > p.MaxLevel) continue;
                if (p.Seasons != null && Season != null && !p.Seasons.Contains(Season())) continue;   // mevsimi değil
                if (p.Chance < 1f && _rng.NextDouble() >= p.Chance) continue;
                int count = p.Count;
                // Yeşil parmak / usta eller gibi: hasat ve kesimde ilk üründen bir tane daha çıkabilir.
                if (firstRow && character.Modifiers.YieldChance > 0f && (interaction.RequiresGrown || interaction.RemovesItem || interaction.Skill == SkillType.Fishing)
                    && _rng.NextDouble() < character.Modifiers.YieldChance) count += 1;
                // Toplayıcılık, maden, av, bahçe, fırın…: ilk üründen seviye başına şansla bir fazlası.
                if (firstRow && interaction.Skill.HasValue)
                {
                    double per = SkillNames.ExtraYieldPerLevel(interaction.Skill.Value);
                    if (per > 0 && _rng.NextDouble() < per * Math.Max(0, character.Skills.GetLevel(interaction.Skill.Value) - 1)) count += 1;
                    double fam = FamilyYieldChance?.Invoke(interaction.Skill.Value) ?? 0;
                    if (fam > 0 && _rng.NextDouble() < fam) count += 1;   // aile tarifi
                }
                // En iyi saat (32 §8.10): safran şafakta toplanırsa bir tel fazla; öğle sıcağında tel azalmaz ama fırsat kaçar.
                if (firstRow && interaction.HasBestHours && HourOfDay != null)
                {
                    double hour = HourOfDay();
                    bool best = hour >= interaction.BestHourFrom && hour < interaction.BestHourTo;
                    if (best) count += interaction.BestHoursBonus;
                    farmNotes.Add(best ? "tam saatinde toplandı" : $"en iyisi saat {interaction.BestHourFrom}-{interaction.BestHourTo} arası");
                }
                firstRow = false;
                if (yieldMult > 1f && _content.Items.TryGet(p.ItemId, out var rowDef) && rowDef.PlantsAs == null)
                {
                    double scaled = count * yieldMult;
                    count = (int)Math.Floor(scaled);
                    if (_rng.NextDouble() < scaled - count) count++;
                }
                if (playerRod)
                {
                    // Oyuncunun oltası: ilk vuran balık mini oyuna gider; ötekiler (aynı atışta) sayılmaz.
                    if (bite == null && _content.Items.TryGet(p.ItemId, out var bd) && bd.HasTag("fish")) { bite = p.ItemId; biteCount = count; }
                    continue;
                }
                bool graded = _content.Items.TryGet(p.ItemId, out var outDef) && outDef.HasQuality;
                int added = 0;
                if (!graded) added = AddKeyed(new ItemKey(p.ItemId), count);
                else if (batchQuality.HasValue)
                    added = AddQ(p.ItemId, Math.Max(p.MinQuality, batchQuality.Value), batchQuality.Value >= ItemQuality.Masterwork ? batchMaker : 0, count, qualityTotals);
                else
                    for (int u = 0; u < count; u++)
                    {
                        int q = cropHarvest ? ItemQuality.Spread(tier, _rng) : gathering ? ItemQuality.Gathered(rawLevel, _rng) : 0;
                        added += AddQ(p.ItemId, Math.Max(p.MinQuality, q), 0, 1, qualityTotals);
                    }
                lost += count - added;
                if (added <= 0) continue;
                if (!totals.ContainsKey(p.ItemId)) { totals[p.ItemId] = 0; order.Add(p.ItemId); }
                totals[p.ItemId] += added;
            }
            // Şişeleme (27 §18.3): iksir yapılırken envanterde boş cam şişe varsa biri kullanılır; şişelenen iksir dökülmez, bir fazla çıkar.
            if (BottleItemId != null && interaction.Consume.Count > 0)
            {
                string potion = null;
                foreach (var id in order) if (_content.Items.TryGet(id, out var pd) && pd.HasTag("potion")) { potion = id; break; }
                if (potion != null && _house.TryTakeItem(BottleItemId))
                {
                    int pq = batchQuality.HasValue ? Math.Min(batchQuality.Value, ItemQuality.Superior) : 0;
                    int extra = AddQ(potion, pq, 0, 1, qualityTotals);
                    if (extra > 0) { totals[potion] += extra; farmNotes.Add("şişelendi (+1, 1 cam şişe)"); }
                    else _house.AddItem(BottleItemId);
                }
            }
            foreach (var (gotId, gotCount) in new[] { (slaughterId, slaughterCount), (hideId, hideCount) })
            {
                if (gotId == null || gotCount <= 0) continue;
                bool gradedMeat = _content.Items.TryGet(gotId, out var meatDef) && meatDef.HasQuality;
                int added = 0;
                for (int u = 0; u < gotCount; u++) added += AddQ(gotId, gradedMeat ? ItemQuality.Spread(slaughterTier, _rng) : 0, 0, 1, qualityTotals);
                lost += gotCount - added;
                if (added > 0)
                {
                    if (!totals.ContainsKey(gotId)) { totals[gotId] = 0; order.Add(gotId); }
                    totals[gotId] += added;
                }
            }
            if (interaction.Collect)
            {
                // Hayvancılık: toplanan her üründen seviye başına %3 (ve dal şansıyla) bir fazlası.
                double extraChance = 0.03 * Math.Max(0, husbandry - 1) + character.Modifiers.YieldChance;
                foreach (var kv0 in AnimalSystem.TakeStock(item))
                {
                    // Barınak yığını kaliteyi taşır ("kimlik#kalite"); toplayanın Hayvancılığı birim başına +1 şansı verir (karar 4).
                    var stockKey = ItemKey.FromStockKey(kv0.Key);
                    bool gradedStock = _content.Items.TryGet(stockKey.Id, out var sd) && sd.HasQuality;
                    int bonus = 0;
                    if (extraChance > 0) for (int u = 0; u < kv0.Value; u++) if (_rng.NextDouble() < extraChance) bonus++;
                    int units = kv0.Value + bonus, added = 0;
                    for (int u = 0; u < units; u++)
                    {
                        int q = gradedStock ? ItemQuality.CollectorBonus(stockKey.Quality, husbandry, _rng) : 0;
                        int got = AddQ(stockKey.Id, q, 0, 1, qualityTotals);
                        if (got <= 0) { AnimalSystem.PutStock(item, ItemKey.StockKey(stockKey.Id, q), units - u); break; }   // sığmayan barınakta kalır
                        added += got;
                    }
                    if (added <= 0) continue;
                    if (!totals.ContainsKey(stockKey.Id)) { totals[stockKey.Id] = 0; order.Add(stockKey.Id); }
                    totals[stockKey.Id] += added;
                }
                if (item.StockTotal > 0) parts0 = "depo dolu, kalanı barınakta";
            }

            var parts = new List<string>();
            foreach (var id in order) parts.Add($"+{totals[id]} {Name(id)}" + (qualityTotals.TryGetValue(id, out var qt) ? Breakdown(qt) : ""));
            if (interaction.AddFeed > 0) parts.Add($"yem +{interaction.AddFeed} (stok {item.FeedStock:0.#})");
            if (interaction.AddIce > 0f) parts.Add($"buz {item.FuelDays:0.#} gün");
            if (parts0 != null) parts.Add(parts0);
            if (lost > 0) parts.Add($"depo dolu, {lost} adet kayboldu");

            if (bite != null) { parts.Add("oltaya bir şey vurdu!"); FishBite?.Invoke(character, item, bite, biteCount); }
            else if (interaction.Skill == SkillType.Fishing && order.Count == 0) parts.Add("bu sefer bir şey çıkmadı");
            if (interaction.TakesFruit) { item.FruitReady = false; item.FruitDays = 0; }
            if (cropHarvest && interaction.RemovesItem) Plants?.OnHarvest(item);
            if (cropHarvest && order.Count > 0) CropHarvested?.Invoke(character, item, tier);
            if (interaction.Skill == SkillType.Fishing)
                foreach (var id in order)
                    if (_content.Items.TryGet(id, out var fishDef) && fishDef.HasTag("fish")) { FishCaught?.Invoke(character, id); break; }
            parts.AddRange(farmNotes);

            if (interaction.RemovesItem && _world.GetItem(item.Id) != null)
                _world.RemoveItem(item.Id);

            AppliedOn?.Invoke(character, item, interaction);
            Applied?.Invoke(character, interaction, parts.Count > 0 ? string.Join(", ", parts) : interaction.DisplayName);
        }

        private int AddQ(string id, int quality, int maker, int count, Dictionary<string, int[]> totals)
        {
            int added = AddKeyed(new ItemKey(id, quality, ItemCondition.Sound, false, maker), count);
            if (added > 0)
            {
                if (!totals.TryGetValue(id, out var a)) { a = new int[ItemQuality.Max + 1]; totals[id] = a; }
                a[ItemQuality.Clamp(quality)] += added;
            }
            return added;
        }

        // ---- öğretmenlik ----

        private List<Character> Pupils(Character teacher)
        {
            var list = new List<Character>();
            if (Household == null) return list;
            foreach (var c in Household())
                if (c != teacher && !c.IsDead && !c.IsVillager && (c.Stage == AgeStage.Child || c.Stage == AgeStage.Teen)) list.Add(c);
            return list;
        }

        /// <summary>
        /// Ders: her öğrenci okuryazarlık ve öğretmenin en iyi becerisinden XP alır (öğretmenlik seviyesiyle artar).
        /// Öğretmenlik ve okuryazarlık "en iyi beceri" sayılmaz.
        /// </summary>
        public const int SecondSubjectLevel = 4;

        private int Teach(Character teacher)
        {
            int level = teacher.Skills.GetLevel(SkillType.Teaching);
            SkillType best = SkillType.Farming; double bestXp = -1;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                if (s == SkillType.Teaching || s == SkillType.Literacy) continue;
                double xp = teacher.Skills.GetXp(s);
                if (xp > bestXp) { bestXp = xp; best = s; }
            }
            // Öğretmenlik 4 (belge 45): aynı derste ikinci konu (öğretmenin ikinci en iyi becerisi).
            SkillType second = best; double secondXp = -1;
            if (level >= SecondSubjectLevel)
                foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
                {
                    if (s == SkillType.Teaching || s == SkillType.Literacy || s == best) continue;
                    double xp = teacher.Skills.GetXp(s);
                    if (xp > secondXp) { secondXp = xp; second = s; }
                }
            var pupils = Pupils(teacher);
            foreach (var p in pupils)
            {
                p.GainSkillXp(SkillType.Literacy, 8 + 2 * level);
                p.GainSkillXp(best, 5 + 2 * level);
                if (second != best) p.GainSkillXp(second, 3 + level);
            }
            return pupils.Count;
        }

        // ---- banyo: sıcak ya da soğuk su ----

        /// <summary>Bu sıcaklığın altında ocak yanmayan yerde yıkanmak üşütür.</summary>
        public const float ColdBathTemperature = 12f;

        /// <summary>
        /// Ocak yanan bir odadaki banyo sıcaktır ("Sıcak banyo"). Değilse ve hava serinse soğuk su Isınma'yı düşürür
        /// ("Buz gibi su"); kışın dışarıda ıslak kalan üşütür (karakter ıslaklığı).
        /// </summary>
        private string BathEffect(Character c, ItemInstance item)
        {
            var room = _world.Map.Rooms.RoomAt(item.Position);
            bool hot = false;
            if (room != null && room.IsIndoor)
                foreach (var it in _world.Items)
                    if (!it.IsDecor && it.Def.HeatOutput > 0f && it.IsLit && room == _world.Map.Rooms.RoomAt(it.Position)) { hot = true; break; }
            if (hot) { c.AddMoodlet(Mood.MoodCatalog.HotBath); return "sıcak banyo"; }
            float t = TemperatureAt != null ? TemperatureAt(item.Position) : 15f;
            if (t >= ColdBathTemperature) return "ılık su";
            float loss = Math.Min(40f, (ColdBathTemperature - t) * 2f);
            c.Needs.Set(NeedType.Warmth, c.Needs.Get(NeedType.Warmth) - loss);
            c.AddMoodlet(Mood.MoodCatalog.ColdWater);
            return "buz gibi su";
        }

        // Dolumu gübreye çevirip depoya koyar. Tavan yok: depo ne kadar alırsa; sığmayan (dolu helayı kullanan
        // boşaltınca) yığına dökülür.
        private string EmptyInto(ItemInstance item)
        {
            if (item.Def.FillProduct == null) { item.Fill = 0; return ""; }
            int n = Math.Max(1, item.Fill / item.Def.FillPerProduct);
            item.Fill = 0;
            int got = Storage != null ? Storage.AddUpTo(item.Def.FillProduct, n) : AddAll(item.Def.FillProduct, n);
            string s = got > 0 ? $"+{got} {Name(item.Def.FillProduct)}" : "";
            if (got < n) s += (s.Length > 0 ? ", " : "") + $"{n - got} {Name(item.Def.FillProduct)} depoya sığmadı, yığına döküldü";
            return s;
        }
    }
}
