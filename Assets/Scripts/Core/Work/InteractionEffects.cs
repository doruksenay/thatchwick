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

            var farmNotes = new List<string>();
            if (interaction.PetsAnimals && Animals != null)
            {
                int petted = Animals.PetResidents(item.Id);
                if (petted > 0) farmNotes.Add($"{petted} hayvan sevildi");
            }
            string slaughterId = null; int slaughterCount = 0;
            if (interaction.Slaughter && Animals != null && Animals.Slaughter(item.Id, out var killed, out slaughterId, out slaughterCount))
                farmNotes.Add($"{killed.Species.DisplayName} kesildi");
            if (interaction.Water) { PlantSystem.Water(item); farmNotes.Add("sulandı"); }
            if (interaction.Fertilize > 0f && Plants != null)
            {
                Plants.Fertilize(item.Position, interaction.Fertilize);
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
                if (p.Chance < 1f && _rng.NextDouble() >= p.Chance) continue;
                int count = p.Count;
                // Yeşil parmak / usta eller gibi: hasat ve kesimde ilk üründen bir tane daha çıkabilir.
                if (firstRow && character.Modifiers.YieldChance > 0f && (interaction.RequiresGrown || interaction.RemovesItem)
                    && _rng.NextDouble() < character.Modifiers.YieldChance) count += 1;
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
            if (slaughterId != null)
            {
                int added = Storage != null ? Storage.AddUpTo(slaughterId, slaughterCount) : AddAll(slaughterId, slaughterCount);
                lost += slaughterCount - added;
                if (added > 0)
                {
                    if (!totals.ContainsKey(slaughterId)) { totals[slaughterId] = 0; order.Add(slaughterId); }
                    totals[slaughterId] += added;
                }
            }
            if (interaction.Collect)
            {
                foreach (var kv in AnimalSystem.TakeStock(item))
                {
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
            if (parts0 != null) parts.Add(parts0);
            if (lost > 0) parts.Add($"depo dolu, {lost} adet kayboldu");

            if (interaction.TakesFruit) { item.FruitReady = false; item.FruitDays = 0; }
            if (cropHarvest && interaction.RemovesItem) Plants?.OnHarvest(item);
            if (tier > 0) parts.Add($"kalite: {PlantSystem.QualityName(tier)}");
            parts.AddRange(farmNotes);

            if (interaction.RemovesItem && _world.GetItem(item.Id) != null)
                _world.RemoveItem(item.Id);

            Applied?.Invoke(character, interaction, parts.Count > 0 ? string.Join(", ", parts) : interaction.DisplayName);
        }
    }
}
