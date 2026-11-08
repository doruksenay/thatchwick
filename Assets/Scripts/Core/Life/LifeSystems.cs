using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Yaşam katmanının tek giriş noktası (Adım 18-43): oyun katmanı bunu kurar, her karede Tick'i çağırır,
    /// kayıtta Export/Restore kullanır. Alt sistemler (mezar, ruh hali, ...) burada toplanır; her biri
    /// bağımsız test edilebilir, burası yalnızca bağlama ve sıralamadır.
    /// </summary>
    public sealed class LifeSystems
    {
        public ContentDatabase Content { get; }
        public WorldState World { get; }
        public Household House { get; }
        public Population Population { get; }
        public GameClock Clock { get; }

        public MoodService Mood { get; }
        public GraveService Graves { get; }
        public VillageEffects Village { get; } = new VillageEffects();

        /// <summary>Yaşam olayları (arayüzdeki bildirim akışı bunu dinler): mezar, yas vb.</summary>
        public event Action<string> Notice;

        private double _moodAccumulator;
        public const double MoodUpdateStepDays = 0.02;

        public LifeSystems(ContentDatabase content, WorldState world, Household house, Population population, GameClock clock)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            World = world ?? throw new ArgumentNullException(nameof(world));
            House = house ?? throw new ArgumentNullException(nameof(house));
            Population = population ?? throw new ArgumentNullException(nameof(population));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));

            Mood = new MoodService(world);
            Graves = new GraveService(world, content);

            population.Content = content;
            population.Village = Village;
            population.MemberDied += OnMemberDied;
        }

        private void Say(string message) => Notice?.Invoke(message);

        private void OnMemberDied(Character dead)
        {
            Graves.Bury(dead, Clock.TotalDays);
            int mourners = Mood.Mourn(dead, Population.Members);
            Say(mourners > 0
                ? $"{dead.Name} toprağa verildi; {mourners} kişi yasta."
                : $"{dead.Name} toprağa verildi.");
        }

        /// <summary>Ruh hali kaynaklarını yeniler (oyun hızından bağımsız, küçük adımlarla).</summary>
        public void Tick(double gameDays)
        {
            _moodAccumulator += gameDays;
            if (_moodAccumulator < MoodUpdateStepDays) return;
            _moodAccumulator = 0;
            Mood.Update(Population.Members);
        }

        // ---- kayıt ----

        public LifeSaveData Export()
        {
            var data = new LifeSaveData();
            var graves = new List<GraveSave>();
            foreach (var g in Graves.Graves)
                graves.Add(new GraveSave
                {
                    characterId = g.CharacterId, name = g.Name, generation = g.Generation, sex = (int)g.Sex, cause = (int)g.Cause,
                    itemId = g.ItemId, x = g.Position.X, y = g.Position.Y, deathDay = g.DeathDay, ageDays = g.AgeDays
                });
            data.graves = graves.ToArray();
            return data;
        }

        public void Restore(LifeSaveData data)
        {
            if (data == null) { Population.RecomputeAll(); return; }
            var graves = new List<GraveRecord>();
            if (data.graves != null)
                foreach (var g in data.graves)
                    if (g != null)
                        graves.Add(new GraveRecord
                        {
                            CharacterId = g.characterId, Name = g.name, Generation = g.generation,
                            Sex = Enum.IsDefined(typeof(Sex), g.sex) ? (Sex)g.sex : Sex.Female,
                            Cause = Enum.IsDefined(typeof(DeathCause), g.cause) ? (DeathCause)g.cause : DeathCause.None,
                            ItemId = g.itemId, Position = new GridPos(g.x, g.y), DeathDay = g.deathDay, AgeDays = g.ageDays
                        });
            Graves.Restore(graves);
            Population.RecomputeAll();
        }
    }
}
