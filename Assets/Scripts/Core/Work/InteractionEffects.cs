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

        /// <summary>Depo kapasitesi; null ise sınırsız. Dolunca fazla ürün kaybolur ve özette yazılır.</summary>
        public StorageService Storage { get; set; }

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
            if (interaction.ConsumeItem != null && _house.Count(interaction.ConsumeItem) <= 0)
                return $"{Name(interaction.ConsumeItem)} yok.";
            foreach (var c in interaction.ConsumeMore)
                if (_house.Count(c.ItemId) < c.Count) return $"{Name(c.ItemId)} yetersiz ({_house.Count(c.ItemId)}/{c.Count}).";
            return null;
        }

        public void Apply(Character character, ItemInstance item, InteractionDef interaction)
        {
            if (CanStart(character, item, interaction) != null) return; // bu arada harcanmış: ürün yok
            if (interaction.ConsumeItem != null) _house.TryTakeItem(interaction.ConsumeItem);
            foreach (var c in interaction.ConsumeMore)
                for (int i = 0; i < c.Count; i++) _house.TryTakeItem(c.ItemId);

            // Beceri seviyesi (+ özellik/dal kalite bonusu) ürün satırlarını seçer: aynı yemeğin sıradan ve iyi hali.
            int level = interaction.Skill.HasValue ? character.Skills.GetLevel(interaction.Skill.Value) + character.Modifiers.QualityLevels : 0;

            // Aynı ürün birden çok satırda olabilir (kesin 1 fidan + %50 ihtimalle 1 fidan): özette toplanır.
            int lost = 0;
            var order = new List<string>();
            var totals = new Dictionary<string, int>();
            bool firstRow = true;
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
                int added = Storage != null ? Storage.AddUpTo(p.ItemId, count) : AddAll(p.ItemId, count);
                lost += count - added;
                if (added <= 0) continue;
                if (!totals.ContainsKey(p.ItemId)) { totals[p.ItemId] = 0; order.Add(p.ItemId); }
                totals[p.ItemId] += added;
            }
            var parts = new List<string>();
            foreach (var id in order) parts.Add($"+{totals[id]} {Name(id)}");
            if (lost > 0) parts.Add($"depo dolu, {lost} adet kayboldu");

            if (interaction.ResetsReady) item.ReadyProgress = 0;

            if (interaction.RemovesItem && _world.GetItem(item.Id) != null)
                _world.RemoveItem(item.Id);

            Applied?.Invoke(character, interaction, parts.Count > 0 ? string.Join(", ", parts) : interaction.DisplayName);
        }
    }
}
