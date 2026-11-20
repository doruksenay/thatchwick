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

        /// <summary>Kazı motoru (maden görevi, arsa kazısı); null ise maden görevi iş bulmaz.</summary>
        public Mining.MineSystem Mines { get; set; }

        /// <summary>Bu görevi alabilir mi: maden görevi ergene verilmez (kazıya inmez).</summary>
        public static bool CanTakeDuty(Character c, DutyKind d) => CanTakeDuty(c) && !(d == DutyKind.Mine && c.Stage == AgeStage.Teen);

        /// <summary>Karo işleri (yol onarımı); null ise tarla görevlisi yol onarmaz.</summary>
        public ITileWork TileWork { get; set; }

        /// <summary>
        /// Hanedeki odun sayısı; null ise tarla görevlisi odun işine bakmaz. Odun LowFirewood altına inince tarla görevlisi
        /// kütük yarar, kütük yoksa arsadaki olgun (meyvesiz) ağacı keser (Denge 41: kimse odun kesmediği için hane donuyordu).
        /// </summary>
        public Func<int> FirewoodStock { get; set; }
        public const int LowFirewood = 6;
        /// <summary>Sonbahar ve kışta tutulan odun (kış ~2 kütük/gün; Denge 41'de kış stoğu yapılmadığı için hane donuyordu).</summary>
        public const int WinterFirewood = 24;
        /// <summary>Mevsim (odun hedefi için); null ise Now ve varsayılan takvimden hesaplanır.</summary>
        public Func<Thatchwick.Core.Time.Season> Season { get; set; }

        /// <summary>Bu mevsim tutulacak odun: ilkbahar ve yaz LowFirewood, sonbahar ve kış WinterFirewood.</summary>
        public int FirewoodTarget()
        {
            Thatchwick.Core.Time.Season season;
            if (Season != null) season = Season();
            else
            {
                var cfg = Thatchwick.Core.Time.TimeConfig.Default;
                long day = (long)Math.Floor(Now?.Invoke() ?? 0);
                season = (Thatchwick.Core.Time.Season)((day / cfg.DaysPerSeason) % Thatchwick.Core.Time.TimeConfig.SeasonsPerYear);
            }
            return season == Thatchwick.Core.Time.Season.Autumn || season == Thatchwick.Core.Time.Season.Winter ? WinterFirewood : LowFirewood;
        }
        public const string FirewoodId = "item.firewood";
        public const string LogId = "item.log";
        /// <summary>Meyve toplama etkileşimi: bunu taşıyan ağaç odun için kesilmez.</summary>
        public const string PickId = "pick";

        private bool FirewoodLow => FirewoodStock != null && FirewoodStock() < FirewoodTarget();

        /// <summary>
        /// Ev görevinin işleri (belge 45): bakım isteyen eşya ve etkileşimi (mum, sepet, döküntü, ayna, saksı, ot serpme,
        /// badana, solan süs). Life.HomeLife.Chores verir; null ise ev görevlisi iş bulmaz.
        /// </summary>
        public Func<IEnumerable<(ItemInstance item, InteractionDef interaction)>> HouseChores { get; set; }
        /// <summary>
        /// Avlu işleri (belge 57b §5.1): görevine göre pislik toplama, kompost çevirme, ahır temizliği, ev içi temizlik.
        /// Avlu sistemi verir; null ise yok.
        /// </summary>
        public Func<DutyKind, IEnumerable<(ItemInstance item, InteractionDef interaction)>> YardChores { get; set; }

        private void ConsiderYard(Character c, Action<long, GridPos, Func<Job>> consider)
        {
            if (YardChores == null) return;
            foreach (var (it, i) in YardChores(c.Duty))
            {
                if (it == null || i == null || (_effects != null && _effects.CanStart(c, it, i) != null)) continue;
                var item = it; var inter = i;
                consider(HouseKey(item.Id, inter.Id), item.Position, () => Job.UseItem(item.Id, inter.Id));
            }
        }
        /// <summary>Mutfak görevlisinin işleri, öncelik sırasıyla (karakter null: yalnızca sayım).</summary>
        public Func<Character, IEnumerable<(ItemInstance item, InteractionDef interaction)>> KitchenChores { get; set; }
        /// <summary>Ev görevi akşam da sürer (mum yakmak akşam işidir): bu saate kadar.</summary>
        public Func<bool> HouseHours { get; set; }

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
            if (effects is InteractionEffects ie && ie.House != null) FirewoodStock = () => ie.House.Count(FirewoodId);
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
                case DutyKind.Mine: return "Maden";
                case DutyKind.Steward: return "Kâhya";
                case DutyKind.House: return "Ev";
                case DutyKind.Kitchen: return "Mutfak";
                default: return "Görev yok";
            }
        }

        /// <summary>Görevin bugün bekleyen iş sayısı (aile panelinde "3 iş bekliyor").</summary>
        public int Pending(DutyKind duty)
        {
            int n = 0;
            if (duty == DutyKind.Farm)
            {
                foreach (var it in _world.Items) { if (FarmInteraction(it) != null || EmptyInteraction(it) != null) n++; }
                if (FirewoodLow) n++;   // odun azaldı
                if (YardChores != null) foreach (var _ in YardChores(duty)) n++;
            }
            // (Ekilecek boş toprak "bekleyen iş" sayılmaz: tohum varsa görevli kendiliğinden eker.)
            else if (duty == DutyKind.Animals)
            {
                foreach (var it in _world.Items) if (FeedInteraction(it) != null || CollectInteraction(it) != null) n++;
                if (_animals != null) foreach (var a in _animals.Animals) if (!a.IsDead && a.PetCooldownDays <= 0) n++;
                if (YardChores != null) foreach (var _ in YardChores(duty)) n++;
            }
            else if (duty == DutyKind.House && HouseChores != null)
                foreach (var _ in HouseChores()) n++;
            else if (duty == DutyKind.Kitchen && KitchenChores != null)
                foreach (var _ in KitchenChores(null)) n++;
            else if (duty == DutyKind.Mine && Mines != null)
                foreach (var m in Mines.Sites)
                    foreach (var (_, _, t) in m.All()) if (t.Marked || t.Creaking) n++;
            return n;
        }

        /// <summary>Görevliye sıradaki iş; yoksa null. JobRunner.DutyProvider olarak verilir.</summary>
        public Job Next(Character c)
        {
            if (!CanTakeDuty(c, c.Duty) || c.Duty == DutyKind.None) return null;
            // Ev ve Mutfak görevi akşam da sürer (mum yakmak, akşam öğünü).
            if (c.Duty == DutyKind.House || c.Duty == DutyKind.Kitchen ? (HouseHours ?? WorkHours) != null && !(HouseHours ?? WorkHours)() : WorkHours != null && !WorkHours()) return null;
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
                ConsiderYard(c, Consider);
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
                // Harman: harman yerinde dövülecek demet varsa (elle döven).
                foreach (var it in _world.Items)
                {
                    if (it.IsDecor || !it.Def.HasTag("threshing_floor") || _effects == null) continue;
                    foreach (var i in it.Def.Interactions)
                    {
                        if (_effects.CanStart(c, it, i) != null) continue;
                        var item = it; var inter = i;
                        Consider(item.Id, item.Position, () => Job.UseItem(item.Id, inter.Id));
                        break;
                    }
                }
                // Odun: hane odunu azaldıysa önce kütük yarılır, kütük yoksa arsadaki olgun meyvesiz ağaç kesilir.
                if (best == null && FirewoodLow && _effects != null)
                {
                    foreach (var it in _world.Items)
                    {
                        if (it.IsDecor) continue;
                        var i = SplitInteraction(it);
                        if (i == null || _effects.CanStart(c, it, i) != null) continue;
                        var item = it; var inter = i;
                        Consider(item.Id, item.Position, () => Job.UseItem(item.Id, inter.Id));
                    }
                    if (best == null)
                        foreach (var it in _world.Items)
                        {
                            var i = FellInteraction(it);
                            if (i == null || !_world.Map.IsOwned(it.Position) || _effects.CanStart(c, it, i) != null) continue;
                            var item = it; var inter = i;
                            Consider(item.Id, item.Position, () => Job.UseItem(item.Id, inter.Id));
                        }
                }
                // Tohum serpen tarlacı: hasat ve sulama yoksa boş toprağa mevsime ve nöbete uyan tohumu eker.
                if (best == null && TileWork != null)
                {
                    var map = _world.Map;
                    for (int y = 0; y < map.Height; y++)
                        for (int x = 0; x < map.Width; x++)
                        {
                            var tile = new GridPos(x, y);
                            if (map.GetFloor(tile) != FloorType.Dirt || _world.GetItemAt(tile) != null) continue;
                            if (TileWork.CanDo(c, tile, TileAction.Sow) != null) continue;
                            var t = tile;
                            Consider(RoadKey(t) + (1L << 41), t, () => Job.OnTile(t, TileAction.Sow));
                        }
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
                ConsiderYard(c, Consider);
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
            else if (c.Duty == DutyKind.House && HouseChores != null)
            {
                ConsiderYard(c, Consider);
                foreach (var (it, i) in HouseChores())
                {
                    if (it == null || i == null || (_effects != null && _effects.CanStart(c, it, i) != null)) continue;
                    var item = it; var inter = i;
                    Consider(HouseKey(item.Id, inter.Id), item.Position, () => Job.UseItem(item.Id, inter.Id));
                }
            }
            else if (c.Duty == DutyKind.Kitchen && KitchenChores != null)
            {
                // Mutfak görevi (belge 58a §4): sıradaki iş çizelgeden gelir; görevli kendi başına tarif seçmez.
                foreach (var (it, i) in KitchenChores(c))
                {
                    if (it == null || i == null || (_effects != null && _effects.CanStart(c, it, i) != null)) continue;
                    var item = it; var inter = i;
                    Consider(HouseKey(item.Id, inter.Id), item.Position, () => Job.UseItem(item.Id, inter.Id));
                    break;   // çizelge sırası önemli: en yakın değil, ilk yapılabilir iş
                }
            }
            else if (c.Duty == DutyKind.Mine && TileWork != null
                && (Mines != null && Mines.InMine(c) ? Mines.Get(Mining.MineSystem.MineId) : Mines?.Get(Mining.MineSystem.PlotId)) is Mining.MineMap plot)
            {
                // Galerideki görevli galeride, öbürü arsa kazısında çalışır (belge 58d M1): madendeki iş fiziksel, ekran dışında da sürer.
                // Önce güvenlik: çatırdayan karoya destek; sonra su; sonra işaretli karoları kaz (en yakını).
                foreach (var (x, y, t) in plot.All())
                {
                    if (!t.Creaking) continue;
                    var tile = plot.ToWorld(x, y);
                    if (TileWork.CanDo(c, tile, TileAction.Support) != null) continue;
                    Consider(MineKey(tile), tile, () => Job.OnTile(tile, TileAction.Support));
                }
                if (best == null && plot.WaterDepth >= 1)
                    foreach (var (x, y, t) in plot.All())
                    {
                        if (!t.IsOpen) continue;
                        var tile = plot.ToWorld(x, y);
                        Consider(MineKey(tile) + 1, tile, () => Job.OnTile(tile, TileAction.Bail));
                        break;
                    }
                if (best == null)
                    foreach (var (x, y, t) in plot.All())
                    {
                        if (!t.Marked) continue;
                        var tile = plot.ToWorld(x, y);
                        if (TileWork.CanDo(c, tile, TileAction.Dig) != null) continue;
                        Consider(MineKey(tile), tile, () => Job.OnTile(tile, TileAction.Dig));
                    }
            }
            if (best != null && Now != null) _claims[bestKey] = now + ClaimDays;     // saat yoksa tutma da yok
            if (best != null) Issued?.Invoke(c, best);
            return best;
        }

        // Ev işinin tutma anahtarı: aynı eşyada iki ayrı iş (doldur, cilala) ayrı tutulur.
        private static long HouseKey(int itemId, string interaction) => (1L << 43) + ((long)itemId << 8) + ((interaction?.GetHashCode() ?? 0) & 0xFF);

        private static long MineKey(GridPos p) => (1L << 42) + ((long)p.Y << 20) + p.X;

        /// <summary>
        /// Kâhya (sabah): hanede "Kâhya" görevli biri varsa öbür görev alabilen üyeler bekleyen işe göre dağıtılır. Her yeni
        /// görevli, işi en çok / görevlisi en az olan göreve gider (eşitlikte tarla, hayvan, maden, ev sırası); hiç iş yoksa
        /// görevsiz kalır. Oyuncunun karakteri ve kâhyanın kendisi atanmaz. (karakter, eski görev, yeni görev) listesi döner.
        /// </summary>
        /// <summary>Görev önceliği çarpanı (belge 45, Kâhyalık 2 "Öncelik sırala"); null ise hepsi 1.</summary>
        public Func<DutyKind, double> PriorityWeight { get; set; }

        public List<(Character who, DutyKind from, DutyKind to)> AutoAssign(IEnumerable<Character> members)
        {
            var changes = new List<(Character, DutyKind, DutyKind)>();
            Character steward = null;
            var pool = new List<Character>();
            foreach (var c in members)
            {
                if (!CanTakeDuty(c)) continue;
                if (c.Duty == DutyKind.Steward && steward == null) { steward = c; continue; }
                if (c.Duty == DutyKind.Steward) continue;          // ikinci kâhya kendi işinde kalır
                pool.Add(c);
            }
            if (steward == null || pool.Count == 0) return changes;
            pool.Sort((a, b) => a.Id.CompareTo(b.Id));
            var kinds = new[] { DutyKind.Farm, DutyKind.Animals, DutyKind.Mine, DutyKind.House };
            var pending = new Dictionary<DutyKind, int>();
            var assigned = new Dictionary<DutyKind, int>();
            foreach (var k in kinds) { pending[k] = Pending(k); assigned[k] = 0; }
            // Kâhyalık: seviye başına bir görevli daha (en az 2): büyük hanede iyi kâhya herkesi işe koşar.
            int capacity = Math.Max(2, steward.Skills.GetLevel(SkillType.Stewardship) + 1);
            int given = 0;
            foreach (var c in pool)
            {
                DutyKind pick = DutyKind.None;
                double bestScore = 0;
                if (given < capacity)
                    foreach (var k in kinds)
                    {
                        if (pending[k] <= 0 || !CanTakeDuty(c, k)) continue;
                        double score = pending[k] / (double)(assigned[k] + 1) * (PriorityWeight != null ? PriorityWeight(k) : 1.0);
                        if (score > bestScore) { bestScore = score; pick = k; }
                    }
                if (pick != DutyKind.None) { assigned[pick]++; given++; }
                if (c.Duty != pick) { changes.Add((c, c.Duty, pick)); c.Duty = pick; }
            }
            if (changes.Count > 0) steward.GainSkillXp(SkillType.Stewardship, 2 * changes.Count);
            return changes;
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

        // Tarla işi: olgun ekin hasadı, sararmış ekini sökme, olgun meyve, susuz ekin.
        private static InteractionDef FarmInteraction(ItemInstance it)
        {
            if (it.IsDecor || !it.IsPlant) return null;
            // Sararmış ekin sulanmaz, hasat edilmez: görevli söker (yan ürün depoya girer).
            if (it.Withered)
            {
                foreach (var i in it.Def.Interactions) if (i.RequiresWithered) return i;
                return null;
            }
            foreach (var i in it.Def.Interactions)
            {
                if (i.RequiresGrown && i.RemovesItem && it.Def.IsCrop && it.IsGrown) return i;
                if (i.RequiresFruit && it.FruitReady) return i;
            }
            if (it.Def.IsCrop && !it.IsGrown && !it.Watered)
                foreach (var i in it.Def.Interactions) if (i.Water) return i;
            return null;
        }

        // Odun yarma: kütük harcayıp odun veren (yarma kütüğü).
        private static InteractionDef SplitInteraction(ItemInstance it)
        {
            foreach (var i in it.Def.Interactions)
                if (!i.RemovesItem && i.ConsumeItem == LogId && Produces(i, FirewoodId)) return i;
            return null;
        }

        // Ağaç kesme: olgun, meyve vermeyen ağaç; kütük ya da odun verir.
        private static InteractionDef FellInteraction(ItemInstance it)
        {
            if (it.IsDecor || !it.IsPlant || it.Def.IsCrop || !it.IsGrown) return null;
            InteractionDef fell = null;
            foreach (var i in it.Def.Interactions)
            {
                if (i.RequiresFruit && i.Id == PickId) return null;   // meyve ağacı kesilmez (mazılı meşe kesilebilir)
                if (i.RequiresGrown && i.RemovesItem && (Produces(i, LogId) || Produces(i, FirewoodId))) fell = i;
            }
            return fell;
        }

        private static bool Produces(InteractionDef i, string itemId)
        {
            foreach (var p in i.Produce) if (p.ItemId == itemId) return true;
            return false;
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
            // Barınak ürünü ve tuzağa düşen av (kement haritada kuruludur; karar 20.11.2026: otomatik toplanır).
            if (it.IsDecor || !(it.Def.IsHousing || it.Def.IsSnare) || it.StockTotal <= 0) return null;
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
