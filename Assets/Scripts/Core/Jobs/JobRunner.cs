using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Jobs
{
    /// <summary>
    /// Bir karakterin iş kuyruğunu oyun günü bütçesiyle yürütür: yürüme ve eşya kullanma.
    /// Oyun saatinde işler, bu yüzden hız çarpanı otomatik uygulanır ve parçalı ilerleme
    /// tek adımla aynı sonucu verir. NPC'ler (otonom) ihtiyaçları düşünce kendi iş seçer.
    /// </summary>
    public sealed class JobRunner
    {
        /// <summary>Yürüme hızı: oyun günü başına karo (3 karo/sn gerçek zaman, 1x hızda).</summary>
        public const double TilesPerGameDay = 1080.0;

        /// <summary>Otonom karakter, ihtiyacı bu değerin altına düşünce kendiliğinden iş arar.</summary>
        public const float AutonomyThreshold = 30f;

        private const int MaxIterations = 64;

        private static readonly GridPos[] AdjacentOffsets =
        {
            new GridPos(0, 1), new GridPos(1, 0), new GridPos(0, -1), new GridPos(-1, 0)
        };

        private readonly Character _character;
        private readonly WorldState _world;

        private List<GridPos> _path;
        private int _pathIndex;
        private double _stepFraction;
        private bool _using;
        private double _useProgress;

        public JobQueue Queue { get; } = new JobQueue();

        /// <summary>Açıksa, kuyruk boşken ve ihtiyaç kritikken karakter kendi iş seçer (NPC).</summary>
        public bool Autonomous { get; set; }

        /// <summary>Son başarısız işin nedeni (arayüzde gösterilir); yoksa null.</summary>
        public string LastFailure { get; private set; }

        public JobRunner(Character character, WorldState world)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            Autonomous = !character.IsPlayerControlled;
        }

        /// <summary>Yürürken gidilen sonraki karo; yürümüyorsa null (görünüm ara değerlemesi için).</summary>
        public GridPos? NextTile =>
            _path != null && !_using && _pathIndex < _path.Count ? _path[_pathIndex] : (GridPos?)null;

        /// <summary>Mevcut karodan NextTile'a 0-1 arası ilerleme.</summary>
        public float StepProgress => (float)_stepFraction;

        public bool IsUsingItem => _using;

        public bool IsIdle => Queue.Current == null;

        /// <summary>Mevcut işi ve kuyruğu bırakıp yeni bir işi tek iş olarak başlatır (oyuncu komutu).</summary>
        public bool Command(Job job)
        {
            Cancel();
            return Queue.TryEnqueue(job);
        }

        public void Cancel()
        {
            Queue.Clear();
            ResetJobState();
        }

        public void Tick(double gameDays)
        {
            if (gameDays < 0) throw new ArgumentOutOfRangeException(nameof(gameDays));

            double budget = gameDays;
            for (int guard = 0; guard < MaxIterations && budget > 1e-12; guard++)
            {
                var job = Queue.Current;
                if (job == null)
                {
                    if (!Autonomous || !TryStartAutonomy()) return;
                    continue;
                }

                budget -= Advance(job, budget);
            }
        }

        // Bir iş için en çok `budget` kadar ilerler; harcanan oyun gününü döner.
        private double Advance(Job job, double budget)
        {
            switch (job.Kind)
            {
                case JobKind.MoveTo: return AdvanceMove(job, budget);
                case JobKind.UseItem: return AdvanceUse(job, budget);
                default:
                    Fail($"Bilinmeyen iş türü: {job.Kind}");
                    return 0;
            }
        }

        private double AdvanceMove(Job job, double budget)
        {
            if (_path == null)
            {
                _path = Pathfinder.FindPath(_world.Map, _character.Position, job.Target);
                if (_path == null) { Fail("Oraya ulaşılamıyor."); return 0; }
                _pathIndex = 1;
                _stepFraction = 0;
            }

            double used = Walk(budget);
            if (_path == null) return used;      // yol kapandı: bir sonraki turda yeniden hesaplanır
            if (_pathIndex >= _path.Count) Finish();
            return used;
        }

        private double AdvanceUse(Job job, double budget)
        {
            var item = _world.GetItem(job.ItemId);
            InteractionDef interaction = item == null ? null : FindInteraction(item.Def, job.InteractionId);
            if (interaction == null) { Fail("Eşya ya da etkileşim bulunamadı."); return 0; }

            if (!_using && IsUnlitHeatSource(item, interaction)) { Fail("Ocak sönük, önce odun at."); return 0; }

            if (_path == null && !_using)
            {
                _path = PathToAdjacent(item);
                if (_path == null) { Fail("Eşyaya ulaşılamıyor."); return 0; }
                _pathIndex = 1;
                _stepFraction = 0;
            }

            double used = 0;
            if (!_using)
            {
                used = Walk(budget);
                if (_path == null) return used;  // yol kapandı: yeniden hesaplanır
                if (_pathIndex < _path.Count) return used;
                _using = true;
            }

            double remaining = interaction.DurationDays - _useProgress;
            double available = budget - used;
            if (available >= remaining)
            {
                _character.Needs.Restore(interaction.Need, interaction.Restore);
                if (interaction.Skill.HasValue && interaction.SkillXp > 0f)
                    _character.GainSkillXp(interaction.Skill.Value, interaction.SkillXp);
                Finish();
                return used + remaining;
            }

            _useProgress += available;
            return used + available;
        }

        // Yol boyunca yürür (kesirli ilerleme korunur); harcanan oyun gününü döner.
        private double Walk(double budget)
        {
            double used = 0;
            while (_pathIndex < _path.Count && budget - used > 0)
            {
                // Yol yenileme: sonraki adım (yeni duvar, eşya) kapandıysa yol iptal edilir.
                if (_stepFraction <= 0 && !_world.Map.CanStep(_character.Position, _path[_pathIndex]))
                {
                    _path = null;
                    _stepFraction = 0;
                    return used;
                }
                double needed = (1.0 - _stepFraction) / TilesPerGameDay;
                double available = budget - used;
                if (available >= needed)
                {
                    used += needed;
                    _character.Position = _path[_pathIndex];
                    _pathIndex++;
                    _stepFraction = 0;
                }
                else
                {
                    _stepFraction += available * TilesPerGameDay;
                    used += available;
                }
            }
            return used;
        }

        // Eşyaya komşu (4 yön) karolardan en kısa yolu olanına giden yol. Zaten yanındaysa tek elemanlı yol.
        private List<GridPos> PathToAdjacent(ItemInstance item)
        {
            // Dekor, üzerinde durduğu eşyanın yanından kullanılır; çok karolu eşyada her kaplanan karonun komşusu aday.
            var owner = item.IsDecor ? _world.GetItem(item.ParentId.Value) : item;
            if (owner == null) return null;

            List<GridPos> best = null;
            foreach (var occupied in owner.OccupiedTiles())
            {
                foreach (var offset in AdjacentOffsets)
                {
                    var tile = occupied + offset;
                    if (!_world.Map.IsWalkable(tile)) continue;
                    if (_world.Map.EdgeBlocks(GridMap.EdgeBetween(tile, occupied))) continue; // duvarın öbür yanından kullanılmaz

                    var path = Pathfinder.FindPath(_world.Map, _character.Position, tile);
                    if (path != null && (best == null || path.Count < best.Count)) best = path;
                }
            }
            return best;
        }

        private static bool IsUnlitHeatSource(ItemInstance item, InteractionDef interaction) =>
            interaction.Need == NeedType.Warmth && item.Def.HeatOutput > 0f && !item.IsLit;

        private static InteractionDef FindInteraction(ItemDef def, string interactionId)
        {
            foreach (var i in def.Interactions)
                if (i.Id == interactionId) return i;
            return null;
        }

        // En düşük ihtiyacı (eşik altında) en iyi doyuracak eşyayı seçer: önce en çok doyuran, sonra en yakın.
        private bool TryStartAutonomy()
        {
            NeedType? worst = null;
            float lowest = AutonomyThreshold;
            foreach (NeedType need in Enum.GetValues(typeof(NeedType)))
            {
                float value = _character.Needs.Get(need);
                if (value < lowest) { lowest = value; worst = need; }
            }
            if (worst == null) return false;

            ItemInstance bestItem = null;
            InteractionDef bestInteraction = null;
            int bestDistance = int.MaxValue;
            foreach (var item in _world.Items)
            {
                foreach (var interaction in item.Def.Interactions)
                {
                    if (interaction.Need != worst.Value) continue;
                    if (IsUnlitHeatSource(item, interaction)) continue;   // sönük ocağa ısınmaya gidilmez

                    int distance = Math.Max(
                        Math.Abs(item.Position.X - _character.Position.X),
                        Math.Abs(item.Position.Y - _character.Position.Y));

                    bool better = bestInteraction == null
                        || interaction.Restore > bestInteraction.Restore
                        || (interaction.Restore == bestInteraction.Restore && distance < bestDistance);
                    if (better)
                    {
                        bestItem = item;
                        bestInteraction = interaction;
                        bestDistance = distance;
                    }
                }
            }
            if (bestItem == null) return false;

            ResetJobState();
            return Queue.TryEnqueue(Job.UseItem(bestItem.Id, bestInteraction.Id));
        }

        private void Finish()
        {
            Queue.RemoveCurrent();
            ResetJobState();
            LastFailure = null;
        }

        private void Fail(string reason)
        {
            LastFailure = reason;
            Queue.RemoveCurrent();
            ResetJobState();
        }

        private void ResetJobState()
        {
            _path = null;
            _pathIndex = 0;
            _stepFraction = 0;
            _using = false;
            _useProgress = 0;
        }
    }
}
