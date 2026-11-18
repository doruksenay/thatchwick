using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Work
{
    /// <summary>
    /// Görevli aile üyesinin sıradaki işi (otomasyonun işçi tarafı). Tarla görevlisi olgun ekini ve meyveyi toplar,
    /// susuz ekini sular; hayvan görevlisi azalan yemliği doldurur, ürünü toplar, sevilmemiş hayvanı sever (bineği tımarlar).
    /// İki görevli aynı işe koşmasın diye seçilen iş kısa süre "tutulur".
    /// </summary>
    public sealed class DutyPlanner
    {
        /// <summary>Seçilen eşya/hayvan bu kadar oyun günü başka görevliye verilmez.</summary>
        public const double ClaimDays = 0.08;
        /// <summary>Yemlik, günlük ihtiyacın bu katının altına inince doldurulur.</summary>
        public const double RefillBelowDays = 1.0;

        private readonly WorldState _world;
        private readonly AnimalSystem _animals;
        private readonly IInteractionEffects _effects;
        private readonly IAnimalEffects _care;
        private readonly Dictionary<long, double> _claims = new Dictionary<long, double>();

        /// <summary>Görevliye iş verildi (kâhyalık XP'si için).</summary>
        public event Action<Character, Job> Issued;

        /// <summary>Karo işleri (yol onarımı); null ise tarla görevlisi yol onarmaz.</summary>
        public ITileWork TileWork { get; set; }

        /// <summary>Şu an iş saati mi (gündüz); null ise hep.</summary>
        public Func<bool> WorkHours { get; set; }
        /// <summary>Geçen toplam oyun günü (tutma süresi için); null ise iş tutulmaz (tek görevli).</summary>
        public Func<double> Now { get; set; }

        public DutyPlanner(WorldState world, AnimalSystem animals, IInteractionEffects effects, IAnimalEffects care)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _animals = animals;
            _effects = effects;
            _care = care;
        }

        /// <summary>Bu yaşta görev alabilir mi (bebek ve çocuk alamaz)?</summary>
        public static bool CanTakeDuty(Character c) =>
            c != null && !c.IsDead && !c.IsPlayerControlled && c.Stage != AgeStage.Baby && c.Stage != AgeStage.Child;

        public static string DutyName(DutyKind d)
        {
            switch (d)
            {
                case DutyKind.Farm: return "Tarla";
                case DutyKind.Animals: return "Hayvanlar";
                default: return "Görev yok";
            }
        }

        /// <summary>Görevin bugün bekleyen iş sayısı (aile panelinde "3 iş bekliyor").</summary>
        public int Pending(DutyKind duty)
        {
            int n = 0;
            if (duty == DutyKind.Farm)
                foreach (var it in _world.Items) { if (FarmInteraction(it) != null || EmptyInteraction(it) != null) n++; }
            else if (duty == DutyKind.Animals)
            {
                foreach (var it in _world.Items) if (FeedInteraction(it) != null || CollectInteraction(it) != null) n++;
                if (_animals != null) foreach (var a in _animals.Animals) if (!a.IsDead && a.PetCooldownDays <= 0) n++;
            }
            return n;
        }

        /// <summary>Görevliye sıradaki iş; yoksa null. JobRunner.DutyProvider olarak verilir.</summary>
        public Job Next(Character c)
        {
            if (!CanTakeDuty(c) || c.Duty == DutyKind.None) return null;
            if (WorkHours != null && !WorkHours()) return null;
            double now = Now?.Invoke() ?? 0;
            Expire(now);

            Job best = null;
            int bestDist = int.MaxValue;
            long bestKey = 0;
            void Consider(long key, GridPos at, Func<Job> make)
            {
                if (_claims.ContainsKey(key)) return;
                int d = Math.Abs(at.X - c.Position.X) + Math.Abs(at.Y - c.Position.Y) + Math.Abs(at.Z - c.Position.Z) * JobRunner.OtherLevelDistance;
                if (d >= bestDist) return;
                best = make();
                bestDist = d;
                bestKey = key;
            }

            if (c.Duty == DutyKind.Farm)
            {
                foreach (var it in _world.Items)
                {
                    var i = FarmInteraction(it);
                    if (i == null || (_effects != null && _effects.CanStart(c, it, i) != null)) continue;
                    var item = it; var inter = i;
                    Consider(item.Id, item.Position, () => Job.UseItem(item.Id, inter.Id));
                }
                // Dolmaya yüz tutmuş hela çukuru ve lazımlıklar boşaltılır: gübre tarlaya.
                foreach (var it in _world.Items)
                {
                    var i = EmptyInteraction(it);
                    if (i == null || (_effects != null && _effects.CanStart(c, it, i) != null)) continue;
                    var item = it; var inter = i;
                    Consider(item.Id, item.Position, () => Job.UseItem(item.Id, inter.Id));
                }
                // Tarla işi yoksa yıpranmış yolları onarır (taş varsa).
                if (best == null && TileWork != null && _world.Map.HasRoads)
                    foreach (var r in _world.Map.RoadTiles())
                    {
                        if (r.type == RoadType.None || r.natural || r.cond >= GridMap.WornBelow) continue;
                        if (TileWork.CanDo(c, r.pos, TileAction.Repair) != null) continue;
                        var tile = r.pos;
                        Consider(RoadKey(tile), tile, () => Job.OnTile(tile, TileAction.Repair));
                    }
            }
            else if (c.Duty == DutyKind.Animals)
            {
                foreach (var it in _world.Items)
                {
                    var i = FeedInteraction(it) ?? CollectInteraction(it);
                    if (i == null || (_effects != null && _effects.CanStart(c, it, i) != null)) continue;
                    var item = it; var inter = i;
                    Consider(item.Id, item.Position, () => Job.UseItem(item.Id, inter.Id));
                }
                // Yemlik ve ürün yoksa sevgi: binek tımarlanır, öbürleri sevilir.
                if (best == null && _animals != null && _care != null)
                    foreach (var a in _animals.Animals)
                    {
                        if (a.IsDead || a.RiderId != 0 || a.PetCooldownDays > 0) continue;
                        var action = a.Species.Rideable ? AnimalAction.Brush : AnimalAction.Pet;
                        if (_care.CanDo(c, a.Id, action) != null) continue;
                        var animal = a;
                        Consider(-animal.Id, animal.Position, () => Job.WithAnimal(animal.Id, action));
                    }
            }
            if (best != null && Now != null) _claims[bestKey] = now + ClaimDays;     // saat yoksa tutma da yok
            if (best != null) Issued?.Invoke(c, best);
            return best;
        }

        // Yol karosunun tutma anahtarı (eşya ve hayvan kimlikleriyle çakışmaz).
        private static long RoadKey(GridPos p) => (1L << 40) + ((long)p.Y << 20) + p.X;

        private void Expire(double now)
        {
            if (_claims.Count == 0) return;
            var gone = new List<long>();
            foreach (var kv in _claims) if (kv.Value <= now) gone.Add(kv.Key);
            foreach (var k in gone) _claims.Remove(k);
        }

        // Tarla işi: olgun ekin hasadı, olgun meyve, susuz ekin.
        private static InteractionDef FarmInteraction(ItemInstance it)
        {
            if (it.IsDecor || !it.IsPlant) return null;
            foreach (var i in it.Def.Interactions)
            {
                if (i.RequiresGrown && i.RemovesItem && it.Def.IsCrop && it.IsGrown) return i;
                if (i.RequiresFruit && it.FruitReady) return i;
            }
            if (it.Def.IsCrop && !it.IsGrown && !it.Watered)
                foreach (var i in it.Def.Interactions) if (i.Water) return i;
            return null;
        }

        // Yem koyma: yemlik ya da barınak, bir günlük ihtiyacın altına inmişse.
        private InteractionDef FeedInteraction(ItemInstance it)
        {
            if (it.IsDecor || _animals == null || !(it.Def.IsTrough || it.Def.IsHousing)) return null;
            double need = _animals.DailyFeedNeed(it);
            if (need <= 0 || it.FeedStock >= need * RefillBelowDays) return null;
            // Yemlik varsa barınağa ayrıca yem koymaya gerek yok.
            if (it.Def.IsHousing)
                foreach (var near in _animals.ItemsNear(it))
                    if (near.Def.IsTrough && near.FeedStock >= _animals.DailyFeedNeed(near) * RefillBelowDays) return null;
            foreach (var i in it.Def.Interactions) if (i.AddFeed > 0) return i;
            return null;
        }

        private static InteractionDef CollectInteraction(ItemInstance it)
        {
            if (it.IsDecor || !it.Def.IsHousing || it.StockTotal <= 0) return null;
            foreach (var i in it.Def.Interactions) if (i.Collect) return i;
            return null;
        }

        /// <summary>Bu doluluktan sonra tarla görevlisi helayı/lazımlığı boşaltır.</summary>
        public const float EmptyAtFill = 0.75f;

        // Boşaltma etkileşimi; dolu değilse null.
        private static InteractionDef EmptyInteraction(ItemInstance it)
        {
            if (it.IsDecor || it.Def.FillCapacity <= 0 || it.Fill < it.Def.FillCapacity * EmptyAtFill) return null;
            foreach (var i in it.Def.Interactions) if (i.EmptyFill) return i;
            return null;
        }
    }
}
