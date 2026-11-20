using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Animals
{
    /// <summary>Arsanın dışında otlayan hayvanın yeri (Project belgesi 48 §8.2; karar 20.11.2026: 1.0'da).</summary>
    public enum AwayPlace { None = 0, Common = 1, Yayla = 2 }

    /// <summary>Köy merasında ya da yaylada olan hayvan: dünyada görünmez, veri olarak yaşar.</summary>
    public sealed class AwayAnimal
    {
        public Animal Animal;
        public AwayPlace Place;
        public double Since;
        /// <summary>Biriken ürün (yaylada süt peynire döner; merada her gün eve gelir).</summary>
        public Dictionary<string, int> Stock = new Dictionary<string, int>(StringComparer.Ordinal);
        internal double Produce;
        internal double MilkForCheese;
    }

    // Köy merası ve yayla (belge 48 §8.2): 64x64 arsa sürüyü sınırlar; sürü hayvanı ortak merada (hane kotası, itibarla artar)
    // ya da yazın yaylada otlar. Hayvan dünyadan çıkar, veri olarak yaşlanır ve ürün verir; kışın kendiliğinden döner.
    public sealed partial class AnimalSystem
    {
        /// <summary>Köy merası kotası (stint): taban 4, itibarın her 100 puanı +2, en çok 16.</summary>
        public const int StintBase = 4, StintPer100Reputation = 2, StintMax = 16;
        /// <summary>Merada köy çobanı güder: yalnızca güz gecelerinde, gün başına bir hayvan kaybı şansı.</summary>
        public const double CommonAutumnLossChance = 0.01;
        /// <summary>Yaylada çoban köpeği yoksa gün başına bir hayvan kaybı şansı (kurt, uçurum).</summary>
        public const double YaylaLossChanceNoDog = 0.03;
        /// <summary>Uzaktayken dostluk günde bu kadar azalır (sevilmez).</summary>
        public const float AwayFriendshipLoss = 0.5f;
        /// <summary>Yaylada 2 süt bir peynir olur (yayla mandırası).</summary>
        public const int YaylaMilkPerCheese = 2;

        private readonly List<AwayAnimal> _away = new List<AwayAnimal>();
        private readonly Random _awayRng = new Random(4421);

        public IReadOnlyList<AwayAnimal> Away => _away;
        /// <summary>Hanenin itibarı (kota için).</summary>
        public Func<int> ReputationNow { get; set; }
        /// <summary>Uzaktaki hayvan öldü ya da kayboldu (hayvan, sebep anahtarı: "old_age", "lost").</summary>
        public event Action<Animal, string> AwayLost;
        /// <summary>Kış geldi, hayvanlar döndü (sayı); dönemeyenler (barınak yok) için ayrı sayı.</summary>
        public event Action<int, int> AwayReturned;

        public int StintQuota
        {
            get
            {
                int rep = Math.Max(0, ReputationNow?.Invoke() ?? 0);
                return Math.Min(StintMax, StintBase + StintPer100Reputation * (rep / 100));
            }
        }

        public int AwayCount(AwayPlace place)
        {
            int n = 0;
            foreach (var w in _away) if (w.Place == place) n++;
            return n;
        }

        /// <summary>Bu hayvan oraya gönderilebilir mi; null = olur.</summary>
        public string SendAwayBlocker(Animal a, AwayPlace place, Season season)
        {
            if (a == null || a.IsDead) return "Hayvan bulunamadı.";
            bool guard = a.Species.Id == Genetics.GeneticsCatalog.Dog;
            if (!a.Species.Herd && !guard) return $"{a.Species.DisplayName} sürüyle otlamaz.";
            if (a.RiderId != 0) return "Üstünde biri var.";
            if (a.IsPregnant) return "Gebe; doğurana dek evde kalsın.";
            if (a.IsSick) return "Hasta.";
            if (season == Season.Winter) return "Kışın mera ve yayla kapalı.";
            if (place == AwayPlace.Yayla && season == Season.Autumn) return "Yaylaya ilkbaharda ve yazın çıkılır.";
            if (place == AwayPlace.Common && !guard && AwayCount(AwayPlace.Common) >= StintQuota)
                return $"Köy merası kotan dolu ({StintQuota}); itibar arttıkça artar.";
            return null;
        }

        /// <summary>Hayvanı köy merasına ya da yaylaya gönderir (dünyadan çıkar).</summary>
        public bool SendAway(Animal a, AwayPlace place, Season season, double today)
        {
            if (SendAwayBlocker(a, place, season) != null) return false;
            a.Path = null; a.NextTile = null;
            _animals.Remove(a);
            _away.Add(new AwayAnimal { Animal = a, Place = place, Since = today });
            return true;
        }

        /// <summary>Geri getirmenin engeli (barınakta yer); null = olur.</summary>
        public string RecallBlocker(AwayAnimal w)
        {
            if (w == null || !_away.Contains(w)) return "Hayvan uzakta değil.";
            var home = _world.GetItem(w.Animal.HomeItemId);
            bool ok = home != null && home.Def.HousingKind == w.Animal.Species.Housing && Residents(home.Id) < home.Def.HousingCapacity;
            if (!ok && FindHome(w.Animal.Species) == null) return "Barınakta yer yok.";
            return null;
        }

        /// <summary>Hayvanı geri getirir; biriken ürün haneye. Barınak yoksa false.</summary>
        public bool Recall(AwayAnimal w, Household house)
        {
            if (RecallBlocker(w) != null) return false;
            var a = w.Animal;
            var home = _world.GetItem(a.HomeItemId);
            if (home == null || home.Def.HousingKind != a.Species.Housing || Residents(home.Id) >= home.Def.HousingCapacity)
            {
                home = FindHome(a.Species);
                a.HomeItemId = home.Id;
            }
            a.Position = SpawnTile(home);
            a.Target = a.Position;
            _away.Remove(w);
            _animals.Add(a);
            if (house != null) foreach (var kv in w.Stock) if (kv.Value > 0) house.AddItem(kv.Key, kv.Value);
            w.Stock.Clear();
            return true;
        }

        /// <summary>
        /// Gün başı: uzaktaki hayvanlar yaşlanır, otlar (yem istemez), ürün verir. Merada ürün her gün eve gelir; yaylada birikir,
        /// süt peynire döner. Kayıp: merada güz gecelerinde az, yaylada çoban köpeği yoksa. Kış gelince hepsi döner.
        /// </summary>
        public void AwayDay(Season season, Household house, double today)
        {
            if (_away.Count == 0) return;
            bool yaylaDog = false;
            foreach (var w in _away) if (w.Place == AwayPlace.Yayla && w.Animal.Species.Id == Genetics.GeneticsCatalog.Dog && w.Animal.IsMature) yaylaDog = true;

            foreach (var w in new List<AwayAnimal>(_away))
            {
                var a = w.Animal;
                a.AgeDays += 1.0;
                a.Fullness = 100f;
                a.Friendship = Math.Max(0f, a.Friendship - AwayFriendshipLoss);
                if (a.AgeDays >= a.LifespanDays) { a.IsDead = true; a.CauseOfDeath = AnimalDeath.OldAge; _away.Remove(w); AwayLost?.Invoke(a, "old_age"); continue; }
                // Ürün (sütü yalnız dişi verir; mevsim çarpanı).
                var sp = a.Species;
                if (sp.Produces != null && a.IsMature && (!sp.FemaleProduces || a.IsFemale))
                {
                    w.Produce += sp.OutputFactor(season) / Math.Max(0.1, sp.ProduceEveryDays);
                    while (w.Produce >= 1.0)
                    {
                        w.Produce -= 1.0;
                        string item = sp.Produces;
                        if (w.Place == AwayPlace.Yayla && item == "item.milk" && _content.Items.Contains("item.cheese"))
                        {
                            w.MilkForCheese += 1;
                            if (w.MilkForCheese >= YaylaMilkPerCheese) { w.MilkForCheese -= YaylaMilkPerCheese; Add(w.Stock, "item.cheese", 1); }
                        }
                        else if (w.Place == AwayPlace.Common && house != null) house.AddItem(item, 1);
                        else Add(w.Stock, item, 1);
                    }
                }
                if (sp.Id == Genetics.GeneticsCatalog.Dog) continue;
                double loss = w.Place == AwayPlace.Common ? (season == Season.Autumn ? CommonAutumnLossChance : 0)
                            : yaylaDog ? 0 : YaylaLossChanceNoDog;
                if (loss > 0 && _awayRng.NextDouble() < loss)
                {
                    a.IsDead = true; a.CauseOfDeath = AnimalDeath.Predator;
                    _away.Remove(w);
                    AwayLost?.Invoke(a, "lost");
                }
            }

            if (season == Season.Winter)
            {
                int back = 0, stuck = 0;
                foreach (var w in new List<AwayAnimal>(_away))
                {
                    if (Recall(w, house)) back++;
                    else stuck++;
                }
                if (back + stuck > 0) AwayReturned?.Invoke(back, stuck);
            }
        }

        private static void Add(Dictionary<string, int> d, string k, int n) { d.TryGetValue(k, out var v); d[k] = v + n; }

        public AwayAnimalSave[] ExportAway()
        {
            if (_away.Count == 0) return null;
            var list = new List<AwayAnimalSave>();
            foreach (var w in _away)
            {
                var a = w.Animal;
                var stock = new List<KeyInt>();
                foreach (var kv in w.Stock) stock.Add(new KeyInt { key = kv.Key, value = kv.Value });
                list.Add(new AwayAnimalSave
                {
                    place = (int)w.Place, since = w.Since, stock = stock.ToArray(),
                    animal = new AnimalSave
                    {
                        id = a.Id, species = a.Species.Id, x = a.Position.X, y = a.Position.Y, home = a.HomeItemId,
                        fullness = a.Fullness, age = a.AgeDays, produce = w.Produce, friendship = a.Friendship, comfort = a.Comfort,
                        sex = a.IsFemale ? 1 : 2, mother = a.MotherId, generation = a.Generation,
                        lineage = (int)a.Lineage, xp = a.Xp, legendDescent = a.LegendaryDescent, legendTitle = a.LegendaryTitle,
                        father = a.FatherId, genome = a.Genome?.Export(), deeds = SaveGame.ExportDeeds(a), saddle = a.Saddle
                    }
                });
            }
            return list.ToArray();
        }

        public void ImportAway(AwayAnimalSave[] data, ContentDatabase content, List<string> warnings = null)
        {
            _away.Clear();
            if (data == null) return;
            foreach (var w in data)
            {
                var s = w?.animal;
                if (s == null) continue;
                if (!content.Animals.TryGet(s.species, out var species)) { warnings?.Add($"Uzaktaki hayvanın türü yok, atlandı: {s.species}"); continue; }
                var a = Restore(s.id, species, new GridPos(s.x, s.y), s.home, s.fullness, s.age, 0, 0, s.friendship, 0,
                    s.saddle, s.comfort >= 0f ? s.comfort : 50f, false, 0, 0, s.sex == 1 ? true : s.sex == 2 ? false : (bool?)null, 0, s.mother, s.generation,
                    s.lineage, s.xp, s.legendDescent, s.legendTitle, -1f, 0, s.genome, s.father, s.deeds);
                _animals.Remove(a);
                var away = new AwayAnimal { Animal = a, Place = (AwayPlace)Math.Max(1, Math.Min(2, w.place)), Since = w.since, Produce = s.produce };
                if (w.stock != null) foreach (var kv in w.stock) if (kv != null && kv.value > 0) away.Stock[kv.key] = kv.value;
                _away.Add(away);
            }
        }
    }
}
