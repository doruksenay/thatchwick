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

        /// <summary>Otonom karakter ulaşamadığı eşyayı bu kadar oyun günü boyunca yeniden denemez.</summary>
        public const double UnreachableCooldownDays = 0.25;

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
        private double _elapsedDays;
        private readonly Dictionary<int, double> _unreachableUntil = new Dictionary<int, double>();
        private readonly List<NeedType> _needOrder = new List<NeedType>();

        public JobQueue Queue { get; } = new JobQueue();

        /// <summary>Açıksa, kuyruk boşken ve ihtiyaç kritikken karakter kendi iş seçer (NPC).</summary>
        public bool Autonomous { get; set; }

        /// <summary>
        /// İş etkileşimlerinin (kaynak harcama, ürün, eşyayı kaldırma) uygulayıcısı. Yoksa bu tür
        /// etkileşimler başlamaz. Hane envanterini JobRunner bilmez; bu arayüz üzerinden çalışır.
        /// </summary>
        public IInteractionEffects Effects { get; set; }

        /// <summary>Karakterler arası işler (bakım, sohbet, evlilik). Yoksa bu işler başlamaz.</summary>
        public ISocialEffects Social { get; set; }

        /// <summary>
        /// Ek hız çarpanı (alet kademesi, hava durumu...). Yalnızca iş etkileşimlerine uygulanır; yoksa 1.
        /// Özellik, dal ve ruh hali çarpanları karakterden okunur.
        /// </summary>
        public Func<Character, InteractionDef, double> SpeedProvider { get; set; }

        /// <summary>Şu an yürütülen işin nedeni (otonom ihtiyaç); oyuncu komutunda ya da boştayken null.</summary>
        public NeedType? CurrentReason => Queue.Current?.Reason;

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

        /// <summary>Kuyruktaki işi iptal eder (kuyruk şeridi). Şu anki işse yürütme durumu sıfırlanır.</summary>
        public bool CancelAt(int index)
        {
            bool wasCurrent = index == 0;
            if (!Queue.RemoveAt(index)) return false;
            if (wasCurrent) ResetJobState();
            return true;
        }

        /// <summary>Kuyruktaki işi öne alır; şu anki iş bir sonrakine kayar (ilerlemesi sıfırlanır).</summary>
        public bool Prioritize(int index)
        {
            if (!Queue.MoveToFront(index)) return false;
            ResetJobState();
            return true;
        }

        public void Cancel()
        {
            Queue.Clear();
            ResetJobState();
        }

        public void Tick(double gameDays)
        {
            if (gameDays < 0) throw new ArgumentOutOfRangeException(nameof(gameDays));

            _elapsedDays += gameDays;
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
                case JobKind.Social: return AdvanceSocial(job, budget);
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

        private double AdvanceSocial(Job job, double budget)
        {
            if (Social == null) { Fail("Bu iş şu an yapılamaz."); return 0; }
            var target = Social.Find(job.TargetCharacterId);
            if (target == null || target.IsDead) { Fail("Kişi bulunamadı."); return 0; }

            if (!_using)
            {
                string blocked = Social.CanDo(_character, target, job.Action);
                if (blocked != null) { Fail(blocked); return 0; }

                // Hedefin yanına (ya da durduğu karoya) git; hedef yer değiştirdiyse yolu yenile.
                if (_path != null && _path.Count > 0 && !IsNear(_path[_path.Count - 1], target.Position)) _path = null;
                if (_path == null)
                {
                    var goals = new HashSet<GridPos> { target.Position };
                    foreach (var o in AdjacentOffsets) goals.Add(target.Position + o);
                    _path = Pathfinder.FindPathToAny(_world.Map, _character.Position, goals);
                    if (_path == null) { Fail($"{target.Name} yanına gidilemiyor."); return 0; }
                    _pathIndex = 1;
                    _stepFraction = 0;
                }
            }

            double used = 0;
            if (!_using)
            {
                used = Walk(budget);
                if (_path == null) return used;
                if (_pathIndex < _path.Count) return used;
                if (!IsNear(_character.Position, target.Position)) { _path = null; return used; }
                _using = true;
            }

            double remaining = Social.Duration(job.Action) - _useProgress;
            double available = budget - used;
            if (available >= remaining)
            {
                Finish();
                Social.Apply(_character, target, job.Action);
                return used + remaining;
            }
            _useProgress += available;
            return used + available;
        }

        private static bool IsNear(GridPos a, GridPos b) => a.Z == b.Z && Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) <= 1;

        private double AdvanceUse(Job job, double budget)
        {
            var item = _world.GetItem(job.ItemId);
            InteractionDef interaction = item == null ? null : FindInteraction(item.Def, job.InteractionId);
            if (interaction == null) { Fail("Eşya ya da etkileşim bulunamadı."); return 0; }

            if (!_using)
            {
                string blocked = StartBlocker(item, interaction);
                if (blocked != null) { Fail(blocked); return 0; }
            }

            if (_path == null && !_using)
            {
                _path = PathToAdjacent(item);
                if (_path == null)
                {
                    // Otonomi aynı ulaşılamayan eşyayı sürekli seçip takılmasın.
                    _unreachableUntil[item.Id] = _elapsedDays + UnreachableCooldownDays;
                    Fail("Eşyaya ulaşılamıyor.");
                    return 0;
                }
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

            double remaining = EffectiveDuration(interaction) - _useProgress;
            double available = budget - used;
            if (available >= remaining)
            {
                if (interaction.RestoresNeed) _character.Needs.Restore(interaction.Need, interaction.Restore);
                foreach (var extra in interaction.ExtraRestore) _character.Needs.Restore(extra.Need, extra.Amount);
                if (interaction.GivesMood)
                    _character.AddMoodlet(interaction.MoodId, interaction.MoodText ?? interaction.DisplayName, interaction.MoodValue, interaction.MoodDays);
                if (interaction.Skill.HasValue && interaction.SkillXp > 0f)
                    _character.GainSkillXp(interaction.Skill.Value, interaction.SkillXp);
                if (interaction.Moodlet != null) _character.AddMoodlet(interaction.Moodlet);
                Finish();
                // Etkiler en sonda: eşyayı kaldırabilir (kesilen ağaç) ve dünyayı değiştirir.
                if (interaction.HasEffects) Effects?.Apply(_character, item, interaction);
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
                if (_stepFraction <= 0 && !_world.Map.CanTraverse(_character.Position, _path[_pathIndex]))
                {
                    _path = null;
                    _stepFraction = 0;
                    return used;
                }
                // Merdiven adımı daha uzundur (yatay mesafe + 1 karo).
                int span = GridMap.StepSpan(_character.Position, _path[_pathIndex]);
                double needed = (1.0 - _stepFraction) * span / TilesPerGameDay;
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
                    _stepFraction += available * TilesPerGameDay / span;
                    used += available;
                }
            }
            return used;
        }

        // Eşyanın kapladığı karolara dik komşu, açık ve arada duvar olmayan karolardan en ucuz
        // ulaşılanına tek A* ile gider. Zaten böyle bir karodaysa tek elemanlı yol.
        private List<GridPos> PathToAdjacent(ItemInstance item)
        {
            // Dekor, üzerinde durduğu eşyanın yanından kullanılır.
            var owner = item.IsDecor ? _world.GetItem(item.ParentId.Value) : item;
            if (owner == null) return null;

            var goals = new HashSet<GridPos>();
            if (owner.IsWallMounted)
            {
                // Duvar eşyası önündeki karodan kullanılır.
                if (_world.Map.IsWalkable(owner.Position)) goals.Add(owner.Position);
                return goals.Count == 0 ? null : Pathfinder.FindPathToAny(_world.Map, _character.Position, goals);
            }
            foreach (var occupied in owner.OccupiedTiles())
            {
                foreach (var offset in AdjacentOffsets)
                {
                    var tile = occupied + offset;
                    if (!_world.Map.IsWalkable(tile)) continue;
                    if (_world.Map.EdgeBlocks(GridMap.EdgeBetween(tile, occupied))) continue; // duvarın öbür yanından kullanılmaz
                    goals.Add(tile);
                }
            }
            return Pathfinder.FindPathToAny(_world.Map, _character.Position, goals);
        }

        /// <summary>İş etkileşimi mi (beceri, ürün ya da eşya kaldırma)? İhtiyaç etkileşimleri hız çarpanından etkilenmez.</summary>
        public static bool IsWork(InteractionDef i) => i.Skill.HasValue || i.Produce.Count > 0 || i.RemovesItem;

        /// <summary>İş hızı: özellik/dal × ruh hali × (alet, hava...). İhtiyaç etkileşiminde 1.</summary>
        public double SpeedFactor(InteractionDef interaction)
        {
            if (!IsWork(interaction)) return 1.0;
            double f = _character.Modifiers.WorkSpeed * (interaction.Skill.HasValue ? _character.WorkSpeed(interaction.Skill.Value) : 1.0);
            if (SpeedProvider != null) f *= SpeedProvider(_character, interaction);
            return f < 0.1 ? 0.1 : f;
        }

        /// <summary>Beceri seviyesi işi hızlandırır: seviye başına %5, en çok yarı süre. Hız çarpanı bölünür.</summary>
        public double EffectiveDuration(InteractionDef interaction)
        {
            double duration = interaction.DurationDays;
            if (interaction.Skill.HasValue)
            {
                int level = _character.Skills.GetLevel(interaction.Skill.Value);
                duration *= Math.Max(0.5, 1.0 - 0.05 * (level - 1));
            }
            return duration / SpeedFactor(interaction);
        }

        /// <summary>Etkileşime şu an başlanamıyorsa nedeni (etkileşim menüsünde gri seçenek); başlanabiliyorsa null.</summary>
        public string WhyCannotStart(ItemInstance item, InteractionDef interaction) => StartBlocker(item, interaction);

        // Etkileşime başlanamıyorsa nedeni (arayüzde gösterilir); başlanabiliyorsa null.
        private string StartBlocker(ItemInstance item, InteractionDef interaction)
        {
            if (IsUnlitHeatSource(item, interaction)) return "Ocak sönük, önce odun at.";
            if (interaction.RequiresGrown && !item.IsGrown) return $"{item.Def.DisplayName} henüz büyümedi.";
            if (interaction.MinSkillLevel > 0 && interaction.Skill.HasValue
                && _character.Skills.GetLevel(interaction.Skill.Value) < interaction.MinSkillLevel)
                return $"{SkillName(interaction.Skill.Value)} {interaction.MinSkillLevel}. seviye gerekli.";
            if (interaction.RequiresLit && !item.IsLit) return "Ateş sönük, önce odun at.";
            if (interaction.HasEffects)
            {
                if (Effects == null) return "Bu iş şu an yapılamaz.";
                return Effects.CanStart(_character, item, interaction);
            }
            return null;
        }

        public static string SkillName(SkillType skill)
        {
            switch (skill)
            {
                case SkillType.Farming: return "Çiftçilik";
                case SkillType.Woodcutting: return "Odunculuk";
                case SkillType.Cooking: return "Yemek";
                default: return "Marangozluk";
            }
        }

        private static bool IsUnlitHeatSource(ItemInstance item, InteractionDef interaction) =>
            interaction.Need == NeedType.Warmth && item.Def.HeatOutput > 0f && !item.IsLit;

        private static InteractionDef FindInteraction(ItemDef def, string interactionId)
        {
            foreach (var i in def.Interactions)
                if (i.Id == interactionId) return i;
            return null;
        }

        // Otonomi: eşik altındaki ihtiyaçlar sırayla denenir. Önce hayatî olanlar (Açlık, Enerji, Isınma; en düşükten),
        // sonra Sosyal/Eğlence. Bir ihtiyaç için uygun iş yoksa bir sonrakine geçilir (eskiden takılıp kalırdı).
        private bool TryStartAutonomy()
        {
            // Bakım: kendisi kritik değilse önce aç bebeği besler.
            if (Social != null && !_character.Needs.AnyCritical())
            {
                var dependent = Social.DependentNeedingCare(_character);
                if (dependent != null && Social.CanDo(_character, dependent, SocialAction.Feed) == null)
                {
                    ResetJobState();
                    return Queue.TryEnqueue(Job.Social(dependent.Id, SocialAction.Feed).WithReason(NeedType.Hunger));
                }
            }

            _needOrder.Clear();
            foreach (NeedType need in Enum.GetValues(typeof(NeedType)))
                if (_character.Needs.Get(need) < AutonomyThreshold) _needOrder.Add(need);
            if (_needOrder.Count == 0) return false;
            _needOrder.Sort((a, b) =>
            {
                int ka = NeedKinds.IsSurvival(a) ? 0 : 1, kb = NeedKinds.IsSurvival(b) ? 0 : 1;
                if (ka != kb) return ka.CompareTo(kb);
                return _character.Needs.Get(a).CompareTo(_character.Needs.Get(b));
            });

            foreach (var need in _needOrder)
            {
                if (need == NeedType.Social && TryStartChat()) return true;
                if (TryStartNeedItem(need)) return true;
            }
            return false;
        }

        private bool TryStartChat()
        {
            if (Social == null) return false;
            var friend = Social.FriendFor(_character);
            if (friend == null || Social.CanDo(_character, friend, SocialAction.Chat) != null) return false;
            ResetJobState();
            return Queue.TryEnqueue(Job.Social(friend.Id, SocialAction.Chat).WithReason(NeedType.Social));
        }

        // İhtiyacı en iyi doyuracak eşyayı seçer: önce en çok doyuran, sonra en yakın.
        private bool TryStartNeedItem(NeedType need)
        {
            FindBestFor(need, out var bestItem, out var bestInteraction);
            if (bestItem == null) return false;

            ResetJobState();
            return Queue.TryEnqueue(Job.UseItem(bestItem.Id, bestInteraction.Id).WithReason(need));
        }

        private void FindBestFor(NeedType need, out ItemInstance bestItem, out InteractionDef bestInteraction)
        {
            bestItem = null;
            bestInteraction = null;
            float bestRestore = 0f;
            int bestDistance = int.MaxValue;
            foreach (var item in _world.Items)
            {
                foreach (var interaction in item.Def.Interactions)
                {
                    float restore = interaction.RestoreFor(need);
                    if (restore <= 0f) continue;
                    // Otonomi iş yapmaz (ağaç kesmez, hasat etmez, üretmez, yem koymaz); yalnızca ihtiyaç giderir.
                    // Malzeme ya da hayvan gerektiren ihtiyaç etkileşimi (şalgam yemek, köpekle oynamak) koşul sağlanıyorsa seçilir.
                    if (interaction.RemovesItem || interaction.RequiresGrown || interaction.Produce.Count > 0
                        || interaction.IsCraft || interaction.AddFeed > 0 || interaction.Collect) continue;
                    if (interaction.HasEffects && (Effects == null || Effects.CanStart(_character, item, interaction) != null)) continue;
                    if (interaction.MinSkillLevel > 0) continue;
                    if (interaction.RequiresLit && !item.IsLit) continue;
                    if (IsUnlitHeatSource(item, interaction)) continue;   // sönük ocağa ısınmaya gidilmez
                    if (_unreachableUntil.TryGetValue(item.Id, out var until) && until > _elapsedDays) continue;

                    int distance = Math.Max(
                        Math.Abs(item.Position.X - _character.Position.X),
                        Math.Abs(item.Position.Y - _character.Position.Y));

                    bool better = bestInteraction == null
                        || restore > bestRestore
                        || (restore == bestRestore && distance < bestDistance);
                    if (better)
                    {
                        bestItem = item;
                        bestInteraction = interaction;
                        bestRestore = restore;
                        bestDistance = distance;
                    }
                }
            }
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
