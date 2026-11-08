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

            // Aynı ürün birden çok satırda olabilir (kesin 1 fidan + %50 ihtimalle 1 fidan): özette toplanır.
            int lost = 0;
            string parts0 = null;
            var order = new List<string>();
            var totals = new Dictionary<string, int>();
            foreach (var p in interaction.Produce)
            {
                if (p.Chance < 1f && _rng.NextDouble() >= p.Chance) continue;
                int added = Storage != null ? Storage.AddUpTo(p.ItemId, p.Count) : AddAll(p.ItemId, p.Count);
                lost += p.Count - added;
                if (added <= 0) continue;
                if (!totals.ContainsKey(p.ItemId)) { totals[p.ItemId] = 0; order.Add(p.ItemId); }
                totals[p.ItemId] += added;
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

            if (interaction.RemovesItem && _world.GetItem(item.Id) != null)
                _world.RemoveItem(item.Id);

            Applied?.Invoke(character, interaction, parts.Count > 0 ? string.Join(", ", parts) : interaction.DisplayName);
        }
    }
}
