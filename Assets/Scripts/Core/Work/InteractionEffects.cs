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

        /// <summary>Depo kapasitesi; null ise sınırsız. Dolunca fazla ürün kaybolur ve özette yazılır.</summary>
        public StorageService Storage { get; set; }

        /// <summary>Hayvan sistemi (barınakta hayvan var mı); null ise hayvan gerektiren işler başlamaz.</summary>
        public AnimalSystem Animals { get; set; }

        /// <summary>Karo sıcaklığı (buz kesmek için don); null ise don yok sayılır.</summary>
        public Func<GridPos, float> TemperatureAt { get; set; }

        /// <summary>Hane üyeleri (ders vermek için); null ise ders verilemez.</summary>
        public Func<IEnumerable<Character>> Household { get; set; }

        /// <summary>Şu anki mevsim (balık türleri); null ise her mevsimin ürünü çıkabilir.</summary>
        public Func<Time.Season> Season { get; set; }

        /// <summary>Tesisata (çeşme, akarsulu hela, borulu küvet) şu an su geliyor mu (su işleri).</summary>
        public Func<ItemInstance, bool> PipedWater { get; set; }
        private bool Piped(ItemInstance item) => item != null && item.Def.IsFixture && PipedWater != null && PipedWater(item);

        /// <summary>Buzluk en çok bu kadar günlük buz alır.</summary>
        public const float MaxIceDays = 30f;

        /// <summary>Çiftçilik sistemi (toprak verimi, kalite); null ise hasat kalitesiz ve toprak yorulmaz.</summary>
        public PlantSystem Plants { get; set; }

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

        public string CanStart(Character character, ItemInstance item, InteractionDef interaction)
        {
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
            return null;
        }

        public void Apply(Character character, ItemInstance item, InteractionDef interaction)
        {
            var consumed = interaction.AllConsumed();
            foreach (var c in consumed)
                if (_house.Count(c.ItemId) < c.Count) return; // bu arada harcanmış: ürün yok
            foreach (var c in consumed)
                for (int k = 0; k < c.Count; k++) _house.TryTakeItem(c.ItemId);

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
            if (interaction.Slaughter && Animals != null && Animals.Slaughter(item.Id, out var killed, out slaughterId, out slaughterCount))
            {
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
            // Ekin hasadında kalite: sulama düzeni, toprak verimi ve çiftçilik seviyesi ürün miktarını artırır (tohum hariç).
            bool cropHarvest = interaction.RequiresGrown && item.IsPlant && item.Def.IsCrop;
            int tier = cropHarvest && Plants != null ? Plants.QualityOf(item, level) : 0;
            float yieldMult = PlantSystem.QualityYield(tier);
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
                }
                firstRow = false;
                if (yieldMult > 1f && _content.Items.TryGet(p.ItemId, out var rowDef) && rowDef.PlantsAs == null)
                {
                    double scaled = count * yieldMult;
                    count = (int)Math.Floor(scaled);
                    if (_rng.NextDouble() < scaled - count) count++;
                }
                int added = Storage != null ? Storage.AddUpTo(p.ItemId, count) : AddAll(p.ItemId, count);
                lost += count - added;
                if (added <= 0) continue;
                if (!totals.ContainsKey(p.ItemId)) { totals[p.ItemId] = 0; order.Add(p.ItemId); }
                totals[p.ItemId] += added;
            }
            foreach (var (gotId, gotCount) in new[] { (slaughterId, slaughterCount), (hideId, hideCount) })
            {
                if (gotId == null || gotCount <= 0) continue;
                int added = Storage != null ? Storage.AddUpTo(gotId, gotCount) : AddAll(gotId, gotCount);
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
                    int bonus = 0;
                    if (extraChance > 0) for (int u = 0; u < kv0.Value; u++) if (_rng.NextDouble() < extraChance) bonus++;
                    var kv = new KeyValuePair<string, int>(kv0.Key, kv0.Value + bonus);
                    int added = Storage != null ? Storage.AddUpTo(kv.Key, kv.Value) : AddAll(kv.Key, kv.Value);
                    AnimalSystem.PutStock(item, kv.Key, kv.Value - added);   // sığmayan barınakta kalır
                    if (added <= 0) continue;
                    if (!totals.ContainsKey(kv.Key)) { totals[kv.Key] = 0; order.Add(kv.Key); }
                    totals[kv.Key] += added;
                }
                if (item.StockTotal > 0) parts0 = "depo dolu, kalanı barınakta";
            }

            var parts = new List<string>();
            foreach (var id in order) parts.Add($"+{totals[id]} {Name(id)}");
            if (interaction.AddFeed > 0) parts.Add($"yem +{interaction.AddFeed} (stok {item.FeedStock:0.#})");
            if (interaction.AddIce > 0f) parts.Add($"buz {item.FuelDays:0.#} gün");
            if (parts0 != null) parts.Add(parts0);
            if (lost > 0) parts.Add($"depo dolu, {lost} adet kayboldu");

            if (interaction.Skill == SkillType.Fishing && order.Count == 0) parts.Add("bu sefer bir şey çıkmadı");
            if (interaction.TakesFruit) { item.FruitReady = false; item.FruitDays = 0; }
            if (cropHarvest && interaction.RemovesItem) Plants?.OnHarvest(item);
            if (tier > 0) parts.Add($"kalite: {PlantSystem.QualityName(tier)}");
            parts.AddRange(farmNotes);

            if (interaction.RemovesItem && _world.GetItem(item.Id) != null)
                _world.RemoveItem(item.Id);

            Applied?.Invoke(character, interaction, parts.Count > 0 ? string.Join(", ", parts) : interaction.DisplayName);
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
            var pupils = Pupils(teacher);
            foreach (var p in pupils)
            {
                p.GainSkillXp(SkillType.Literacy, 8 + 2 * level);
                p.GainSkillXp(best, 5 + 2 * level);
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
