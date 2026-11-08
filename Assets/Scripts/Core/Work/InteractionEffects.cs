using System;
using System.Collections.Generic;
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

        public InteractionEffects(WorldState world, Household house, ContentDatabase content, int seed = 1)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _rng = new Random(seed);
        }

        private string Name(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        public string CanStart(Character character, ItemInstance item, InteractionDef interaction)
        {
            if (interaction.ConsumeItem != null && _house.Count(interaction.ConsumeItem) <= 0)
                return $"{Name(interaction.ConsumeItem)} yok.";
            return null;
        }

        public void Apply(Character character, ItemInstance item, InteractionDef interaction)
        {
            if (interaction.ConsumeItem != null && !_house.TryTakeItem(interaction.ConsumeItem))
                return; // bu arada harcanmış: ürün yok

            // Aynı ürün birden çok satırda olabilir (kesin 1 fidan + %50 ihtimalle 1 fidan): özette toplanır.
            var order = new List<string>();
            var totals = new Dictionary<string, int>();
            foreach (var p in interaction.Produce)
            {
                if (p.Chance < 1f && _rng.NextDouble() >= p.Chance) continue;
                _house.AddItem(p.ItemId, p.Count);
                if (!totals.ContainsKey(p.ItemId)) { totals[p.ItemId] = 0; order.Add(p.ItemId); }
                totals[p.ItemId] += p.Count;
            }
            var parts = new List<string>();
            foreach (var id in order) parts.Add($"+{totals[id]} {Name(id)}");

            if (interaction.RemovesItem && _world.GetItem(item.Id) != null)
                _world.RemoveItem(item.Id);

            Applied?.Invoke(character, interaction, parts.Count > 0 ? string.Join(", ", parts) : interaction.DisplayName);
        }
    }
}
