using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Cam bahçedeki bir bitki ya da canlı.</summary>
    public sealed class VivEntry
    {
        public string Item;
        public double Health = 100;
        public double Hunger;
        public double Age;
        public double Growth;
        public bool IsPlant(ContentDatabase db) => db.Items.TryGet(Item, out var d) && d.Viv != null && d.Viv.IsPlant;
        public string Encode() => $"{Item}|{Health:0.##}|{Hunger:0.##}|{Age:0.###}|{Growth:0.###}";
        public static VivEntry Decode(string s)
        {
            var p = s?.Split('|');
            if (p == null || p.Length < 5) return null;
            double D(int i) { double.TryParse(p[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v); return v; }
            return new VivEntry { Item = p[0], Health = D(1), Hunger = D(2), Age = D(3), Growth = D(4) };
        }
    }

    /// <summary>Bir cam bahçenin içi: habitat, nem (tuzlu suda tazelik), küf, türler ve son olaylar.</summary>
    public sealed class Vivarium
    {
        public string Habitat;            // null = boş kap (henüz zemin seçilmedi)
        public double Moisture = 70;      // nemli ve kuru: nem; tuzlu: suyun tazeliği
        public double Mold;               // ölü canlı ve fazla nemle birikir; temizleyiciler azaltır
        public readonly List<VivEntry> Entries = new List<VivEntry>();
        public readonly List<string> Log = new List<string>();
        public void Note(string s) { Log.Add(s); while (Log.Count > 8) Log.RemoveAt(0); }
    }

    /// <summary>
    /// Cam bahçe (Project belgesi 47; kullanıcı 20.11.2026): teraryumun dönem karşılığı. Orman camından kap; doğadan cam kavanozla
    /// yakalanan böcekler ve toplanan fidanlar konur. Türler birbirini etkiler: otçul bitkiyi yer, avcı avını yer, uğur böceği
    /// yaprak bitini temizler, solucan ve tespih böceği küfü azaltıp bitkiyi besler, tırtıl kelebeğe döner. Bazı kombinasyonlar
    /// hiç kurulamaz (habitat, zehir, iki örümcek). Oyun katmanı yakın görünümde canlıları hareket ettirir.
    /// Not (dönem): cam kap orman camından ve pahalıdır; kapalı ekosistem fikri oyunun bilinçli istisnasıdır (belge 20 tablosu).
    /// Ömür ölçeği istisnası: canlılar mevsim ölçeğinde yaşar (1 gün ≈ yarım yıl kuralı burada uygulanmaz).
    /// </summary>
    public sealed partial class HomeLife
    {
        public const string Moist = "nemli", Dry = "kuru", Salt = "tuzlu";
        public static readonly string[] Habitats = { Moist, Dry, Salt };
        public const string CatchJar = "item.glass_jar";
        public const string SeaWaterItem = "item.salt";
        public const int VivBeautyPerSpecies = 1, VivBalancedBeauty = 3;
        public const double MoistureDecayPerDay = 18, SaltFreshDecayPerDay = 12, MoldPerDeath = 20, MoldMax = 100;

        private readonly Dictionary<int, Vivarium> _vivs = new Dictionary<int, Vivarium>();
        /// <summary>Bugün yağmur yağıyor mu (salyangoz yalnızca yağmurda çıkar); null ise hayır.</summary>
        public Func<bool> Raining { get; set; }
        /// <summary>Rastgele (testte sabit tohum).</summary>
        public Random Rng { get; set; } = new Random(4711);

        public Vivarium VivOf(ItemInstance item)
        {
            if (item == null || !item.Def.IsVivarium) return null;
            if (!_vivs.TryGetValue(item.Id, out var v)) { v = new Vivarium(); _vivs[item.Id] = v; }
            return v;
        }

        public static string HabitatName(string h) => h == Moist ? "Nemli toprak" : h == Dry ? "Kuru kum" : h == Salt ? "Tuzlu su" : "Boş";
        private ItemDef Def(string id) => _content.Items.TryGet(id, out var d) ? d : null;
        private VivDef V(string id) => Def(id)?.Viv;

        public int CountPlants(Vivarium v) { int n = 0; foreach (var e in v.Entries) if (V(e.Item)?.IsPlant == true) n++; return n; }
        public int CountCreatures(Vivarium v) => v.Entries.Count - CountPlants(v);

        /// <summary>Boş kabın zeminini seçer (toprak, kum ya da tuzlu su). Dolu kapta değişmez.</summary>
        public string SetHabitat(ItemInstance item, string habitat)
        {
            var v = VivOf(item);
            if (v == null) return "Bu bir cam bahçe değil.";
            if (Array.IndexOf(Habitats, habitat) < 0) return "Bilinmeyen zemin.";
            if (v.Entries.Count > 0) return "İçinde canlı varken zemin değişmez; önce boşalt.";
            if (habitat == Salt && _house.Count(SeaWaterItem) <= 0) return "Tuzlu su için envanterde tuz olmalı.";
            if (habitat == Salt) _house.TryTakeItem(SeaWaterItem);
            v.Habitat = habitat; v.Moisture = habitat == Dry ? 25 : 80; v.Mold = 0;
            v.Note($"Zemin: {HabitatName(habitat)}.");
            _world.NotifyChanged();
            return null;
        }

        /// <summary>Bu türün bu cam bahçeye eklenemeyeceği neden; eklenebiliyorsa null. "Oluşmaması gereken" kombinasyonlar burada.</summary>
        public string CanAddToViv(ItemInstance item, string speciesId)
        {
            var v = VivOf(item);
            if (v == null) return "Bu bir cam bahçe değil.";
            var sd = Def(speciesId);
            if (sd?.Viv == null) return "Bu bir cam bahçe canlısı ya da fidanı değil.";
            var s = sd.Viv;
            if (v.Habitat == null) return "Önce zemini seç (toprak, kum ya da tuzlu su).";
            if (!s.Habitats.Contains(v.Habitat)) return $"{sd.DisplayName} {string.Join(" ya da ", s.Habitats.ConvertAll(HabitatName)).ToLowerInvariant()} ister; burası {HabitatName(v.Habitat).ToLowerInvariant()}.";
            if (s.IsPlant ? CountPlants(v) >= item.Def.VivPlants : CountCreatures(v) >= item.Def.VivCreatures)
                return s.IsPlant ? $"Bitki yeri dolu ({item.Def.VivPlants})." : $"Canlı yeri dolu ({item.Def.VivCreatures}).";
            foreach (var e in v.Entries)
            {
                var o = V(e.Item);
                if (o == null) continue;
                if (e.Item == speciesId && s.Solitary) return $"İki {sd.DisplayName.ToLowerInvariant()} bir arada durmaz; biri ötekini öldürür.";
                if (s.Avoid.Contains(e.Item) || o.Avoid.Contains(speciesId))
                    return Def(e.Item).DisplayName + " ile " + sd.DisplayName.ToLowerInvariant() + " bir arada olmaz" +
                           ((s.Has("toxic") || o.Has("toxic")) ? ": ateş böceği kurbağa ve kertenkele için zehirlidir." : ".");
            }
            if (_house.Count(speciesId) <= 0) return $"Envanterde {sd.DisplayName.ToLowerInvariant()} yok.";
            return null;
        }

        /// <summary>Eklemeden önce uyarı: kim kimi yer (eklenebilir ama sonucu oyuncu bilsin).</summary>
        public List<string> AddWarnings(ItemInstance item, string speciesId)
        {
            var list = new List<string>();
            var v = VivOf(item);
            var s = V(speciesId);
            if (v == null || s == null) return list;
            string name = Def(speciesId).DisplayName;
            foreach (var e in v.Entries)
            {
                var o = V(e.Item);
                if (o == null) continue;
                string other = Def(e.Item).DisplayName;
                if (s.Eats.Contains(e.Item)) list.Add($"{name}, {other.ToLowerInvariant()} yer.");
                if (o.Eats.Contains(speciesId)) list.Add($"{other}, {name.ToLowerInvariant()} yer.");
            }
            if (!s.IsPlant && s.Eats.Count > 0 && !HasFoodFor(v, s, speciesId)) list.Add($"{name} için yiyecek yok; aç kalır.");
            return list;
        }

        public bool AddToViv(ItemInstance item, string speciesId)
        {
            if (CanAddToViv(item, speciesId) != null) return false;
            if (!_house.TryTakeItem(speciesId)) return false;
            var v = VivOf(item);
            v.Entries.Add(new VivEntry { Item = speciesId });
            v.Note($"{Def(speciesId).DisplayName} kondu.");
            if (_house.Count(CatchJar) < 1 && !V(speciesId).IsPlant) { }   // kavanoz yakalarken harcanmaz; burada da geri verilmez
            _world.NotifyChanged();
            return true;
        }

        /// <summary>Çıkarır: canlıysa envantere döner (release ise doğaya salınır, ruh hali verir).</summary>
        public void RemoveFromViv(ItemInstance item, int index, bool release, Character by = null)
        {
            var v = VivOf(item);
            if (v == null || index < 0 || index >= v.Entries.Count) return;
            var e = v.Entries[index];
            v.Entries.RemoveAt(index);
            string name = Def(e.Item)?.DisplayName ?? e.Item;
            if (release)
            {
                v.Note($"{name} doğaya salındı.");
                by?.AddMoodlet("mood.released", "Doğaya saldı", 3, 0.5);
            }
            else
            {
                _house.AddItem(e.Item, 1);
                v.Note($"{name} çıkarıldı.");
            }
            _world.NotifyChanged();
        }

        /// <summary>Nemlendir (nemli ve kuru), tuzlu suda suyu tazele (1 tuz).</summary>
        public string Mist(ItemInstance item)
        {
            var v = VivOf(item);
            if (v?.Habitat == null) return "Önce zemini seç.";
            if (v.Habitat == Salt)
            {
                if (v.Moisture > 70) return "Su hâlâ taze.";
                if (!_house.TryTakeItem(SeaWaterItem)) return "Suyu tazelemek için envanterde tuz olmalı.";
                v.Moisture = 100;
                v.Note("Su tazelendi.");
                return null;
            }
            double target = v.Habitat == Dry ? 30 : 85;
            if (v.Moisture >= target - 5) return v.Habitat == Dry ? "Kum yeterince nemli; fazlası kertenkeleyi hasta eder." : "Toprak zaten nemli.";
            v.Moisture = target;
            v.Note("Nemlendirildi.");
            return null;
        }

        /// <summary>Habitata göre uygun nem aralığı.</summary>
        public static (double lo, double hi) MoistRange(string habitat) => habitat == Dry ? (5, 45) : habitat == Salt ? (35, 100) : (40, 95);

        public bool NeedsMist(ItemInstance item)
        {
            var v = item.Def.IsVivarium ? VivOf(item) : null;
            if (v?.Habitat == null || v.Entries.Count == 0) return false;
            return v.Moisture < MoistRange(v.Habitat).lo + 10;
        }

        private bool HasFoodFor(Vivarium v, VivDef s, string self)
        {
            foreach (var e in v.Entries)
                if (Edible(s, e.Item, self)) return true;
            return false;
        }

        private bool Edible(VivDef eater, string food, string self)
        {
            var f = V(food);
            if (f == null) return false;
            if (eater.Eats.Contains(food)) return true;
            if (f.IsPlant && eater.Eats.Contains("*plant")) return true;
            if (f.IsPlant && f.Has("flower") && eater.Eats.Contains("*flower")) return true;
            return false;
        }

        /// <summary>Cam bahçe dengede mi: aç canlı yok, zararlı yaprak biti kontrolsüz değil, nem aralıkta, küf düşük.</summary>
        public bool IsBalanced(Vivarium v)
        {
            if (v?.Habitat == null || v.Entries.Count < 2) return false;
            var (lo, hi) = MoistRange(v.Habitat);
            if (v.Moisture < lo || v.Moisture > hi || v.Mold >= 40) return false;
            int pests = 0; bool control = false;
            foreach (var e in v.Entries)
            {
                var s = V(e.Item);
                if (s == null) continue;
                if (!s.IsPlant && e.Hunger >= 60) return false;
                if (s.IsPlant && e.Health < 50) return false;
                if (s.Has("pest")) pests++;
                if (s.Eats.Exists(x => V(x)?.Has("pest") == true)) control = true;
            }
            return pests <= 1 || control;
        }

        /// <summary>Durum satırı: "Nemli toprak · 3 bitki, 4 canlı · nem 62 · dengede".</summary>
        public string VivSummary(ItemInstance item)
        {
            var v = VivOf(item);
            if (v == null) return null;
            if (v.Habitat == null) return "Boş cam bahçe: zemin seç.";
            string m = v.Habitat == Salt ? $"su tazeliği {v.Moisture:0}" : $"nem {v.Moisture:0}";
            return $"{HabitatName(v.Habitat)} · {CountPlants(v)} bitki, {CountCreatures(v)} canlı · {m}" + (v.Mold >= 20 ? $" · küf {v.Mold:0}" : "") +
                   (IsBalanced(v) ? " · dengede" : v.Entries.Count >= 2 ? " · dengesiz" : "");
        }

        private double VivBeauty(ItemInstance item)
        {
            if (!item.Def.IsVivarium || !_vivs.TryGetValue(item.Id, out var v) || v.Entries.Count == 0) return 0;
            var species = new HashSet<string>();
            foreach (var e in v.Entries) if (e.Health > 30) species.Add(e.Item);
            double b = species.Count * VivBeautyPerSpecies + (IsBalanced(v) ? VivBalancedBeauty : 0) - (v.Mold >= 50 ? 3 : 0);
            if (IsNight(_clock.DayFraction)) foreach (var e in v.Entries) if (V(e.Item)?.Has("glow") == true) { b += 2; break; }
            return b;
        }

        /// <summary>Gece ateş böceği olan cam bahçe odaya ışık verir.</summary>
        public int VivLight(ItemInstance item)
        {
            if (!item.Def.IsVivarium || !IsNight(_clock.DayFraction) || !_vivs.TryGetValue(item.Id, out var v)) return 0;
            int n = 0;
            foreach (var e in v.Entries) if (V(e.Item)?.Has("glow") == true) n++;
            return n >= 3 ? 1 : 0;
        }

        public const string CricketMoodId = "mood.cricket";
        public const int CricketMood = 2;

        /// <summary>Gece cam bahçesinde cırcır böceği öten odalar (ev halkına küçük huzur).</summary>
        private HashSet<string> SingingRooms()
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (_vivs.Count == 0 || !IsNight(_clock.DayFraction)) return set;
            foreach (var it in _world.Items)
            {
                if (!it.Def.IsVivarium || !_vivs.TryGetValue(it.Id, out var v)) continue;
                foreach (var e in v.Entries)
                    if (V(e.Item)?.Has("sings") == true && e.Health > 40) { var r = RoomOf(it); if (r != null) set.Add(RoomKey(r)); break; }
            }
            return set;
        }

        // ---- gün ----

        public void VivariumTick(double gameDays)
        {
            foreach (var kv in _vivs)
            {
                var v = kv.Value;
                if (v.Habitat == null || v.Entries.Count == 0) continue;
                v.Moisture = Math.Max(0, v.Moisture - gameDays * (v.Habitat == Salt ? SaltFreshDecayPerDay : MoistureDecayPerDay));
            }
        }

        /// <summary>Günlük ekosistem adımı: büyüme, beslenme, avlanma, açlık, nem, küf, dönüşüm, üreme, ölüm.</summary>
        public void VivariumDay()
        {
            var alive = new HashSet<int>();
            foreach (var it in _world.Items) if (it.Def.IsVivarium) alive.Add(it.Id);
            var gone = new List<int>();
            foreach (var id in _vivs.Keys) if (!alive.Contains(id)) gone.Add(id);
            foreach (var id in gone) _vivs.Remove(id);

            foreach (var it in _world.Items)
            {
                if (!it.Def.IsVivarium || !_vivs.TryGetValue(it.Id, out var v) || v.Habitat == null || v.Entries.Count == 0) continue;
                StepViv(it, v);
            }
        }

        private void StepViv(ItemInstance item, Vivarium v)
        {
            var (lo, hi) = MoistRange(v.Habitat);
            bool wetOk = v.Moisture >= lo && v.Moisture <= hi;
            bool sun = Sunlit(item);
            int cleaners = 0;
            foreach (var e in v.Entries) if (V(e.Item)?.Has("cleaner") == true) cleaners++;

            // 1. Bitkiler: ışık ve nemle büyür; temizleyici (solucan, tespih böceği) toprağı besler.
            foreach (var e in v.Entries)
            {
                var s = V(e.Item);
                if (s == null || !s.IsPlant) continue;
                bool lightOk = s.Has("shade") ? !sun || v.Habitat == Salt : s.Has("sun") ? sun : true;
                if (wetOk && lightOk) { e.Health = Math.Min(100, e.Health + 15 + 5 * cleaners); e.Growth = Math.Min(3, e.Growth + 0.5); }
                else e.Health -= wetOk ? 8 : 20;
                if (v.Mold >= 50) e.Health -= 10;
            }

            // 2. Canlılar: yer, acıkır, yaşlanır. Avcı avını (şansla), otçul bitkiyi yer.
            var eaten = new HashSet<VivEntry>();
            foreach (var e in new List<VivEntry>(v.Entries))
            {
                var s = V(e.Item);
                if (s == null || s.IsPlant || eaten.Contains(e)) continue;
                e.Age += 1;
                if (s.Eats.Count == 0) e.Hunger = Math.Max(0, e.Hunger - 10);   // ateş böceği yetişkini yemez; temizleyici artıkla yaşar
                else
                {
                    bool fed = false;
                    foreach (var f in v.Entries)
                    {
                        if (f == e || eaten.Contains(f) || !Edible(s, f.Item, e.Item)) continue;
                        var fs = V(f.Item);
                        if (fs.IsPlant)
                        {
                            // Zararlı (yaprak biti) bitkiyi kurutana dek emer; öteki otçullar zayıf bitkiyi bırakır.
                            if (!s.Has("pest") && f.Health <= 30) continue;
                            f.Health -= s.Has("pest") ? 12 : 8;
                            fed = true; break;
                        }
                        if (Rng.NextDouble() < 0.55)
                        {
                            eaten.Add(f);
                            v.Note($"{Def(e.Item).DisplayName}, {Def(f.Item).DisplayName.ToLowerInvariant()} yedi.");
                            fed = true; break;
                        }
                    }
                    e.Hunger = fed ? Math.Max(0, e.Hunger - 40) : e.Hunger + 30;
                }
                if (!wetOk) e.Health -= v.Habitat == Dry ? 15 : 20;
                else e.Health = Math.Min(100, e.Health + 5);
                if (e.Hunger >= 100) e.Health -= 35;
                if (v.Mold >= 60 && !s.Has("cleaner")) e.Health -= 10;
            }
            foreach (var f in eaten) v.Entries.Remove(f);

            // 3. Dönüşüm (tırtıl → kelebek).
            foreach (var e in v.Entries)
            {
                var s = V(e.Item);
                if (s?.Becomes != null && e.Age >= s.BecomeDays && e.Health > 40)
                {
                    v.Note($"{Def(e.Item).DisplayName} {Def(s.Becomes).DisplayName.ToLowerInvariant()} oldu!");
                    Say($"Cam bahçede {Def(e.Item).DisplayName.ToLowerInvariant()} {Def(s.Becomes).DisplayName.ToLowerInvariant()} oldu.");
                    e.Item = s.Becomes; e.Age = 0; e.Hunger = 0;
                }
            }

            // 4. Ölüm: sağlık bitti ya da ömür doldu. Ölü canlı küf getirir.
            for (int i = v.Entries.Count - 1; i >= 0; i--)
            {
                var e = v.Entries[i];
                var s = V(e.Item);
                bool old = s != null && !s.IsPlant && s.LifeDays > 0 && e.Age >= s.LifeDays;
                if (e.Health > 0 && !old) continue;
                string name = Def(e.Item)?.DisplayName ?? e.Item;
                v.Note(old ? $"{name} yaşlılıktan öldü." : s != null && s.IsPlant ? $"{name} kurudu." : $"{name} öldü.");
                v.Entries.RemoveAt(i);
                v.Mold = Math.Min(MoldMax, v.Mold + MoldPerDeath);
            }

            // 5. Küf: fazla nem artırır, temizleyiciler azaltır.
            if (v.Habitat != Salt && v.Moisture > hi - 5) v.Mold = Math.Min(MoldMax, v.Mold + 8);
            v.Mold = Math.Max(0, v.Mold - 6 - 10 * cleaners);

            // 6. Üreme: aynı türden iki tok ve sağlıklı canlı, yer varsa. Yaprak biti uğur böceği yoksa hızla çoğalır.
            var counts = new Dictionary<string, int>();
            foreach (var e in v.Entries) if (V(e.Item)?.Breeds == true && e.Hunger < 50 && e.Health > 60) counts[e.Item] = (counts.TryGetValue(e.Item, out var n) ? n : 0) + 1;
            foreach (var kv in counts)
            {
                if (kv.Value < 2 || CountCreatures(v) >= item.Def.VivCreatures) continue;
                double chance = V(kv.Key).Has("pest") ? 0.6 : 0.2;
                if (Rng.NextDouble() < chance)
                {
                    v.Entries.Add(new VivEntry { Item = kv.Key, Hunger = 30 });
                    v.Note($"{Def(kv.Key).DisplayName} çoğaldı.");
                }
            }
        }

        // ---- yakalama ----

        /// <summary>Yakalama başlama engeli: cam kavanoz gerekir; bu saat ve mevsimde bir şey yoksa söylenir.</summary>
        public string CatchBlocker(ItemInstance node)
        {
            if (node.Def.Catches.Count == 0) return "Burada yakalanacak bir şey yok.";
            if (_house.Count(CatchJar) <= 0) return "Yakalamak için envanterde boş cam kavanoz olmalı.";
            return CatchPool(node).Count == 0 ? "Şu an burada yakalanacak bir şey yok (mevsim, saat ya da hava)." : null;
        }

        private List<CatchDef> CatchPool(ItemInstance node)
        {
            var list = new List<CatchDef>();
            bool night = IsNight(_clock.DayFraction), rain = Raining?.Invoke() == true;
            foreach (var c in node.Def.Catches)
            {
                if (c.Seasons.Count > 0 && !c.Seasons.Contains(_clock.Season)) continue;
                if (c.Night && !night) continue;
                if (c.Rain && !rain) continue;
                list.Add(c);
            }
            return list;
        }

        /// <summary>Yakalar: havuzdan şansla bir tür envantere gelir. Yakalanan ya da null.</summary>
        public string Catch(ItemInstance node)
        {
            var pool = CatchPool(node);
            foreach (var c in pool)
                if (Rng.NextDouble() < c.Chance)
                {
                    _house.AddItem(c.Item, 1);
                    Say($"Cam kavanoza {Def(c.Item).DisplayName.ToLowerInvariant()} yakalandı.");
                    return c.Item;
                }
            Say("Bir şey yakalanamadı.");
            return null;
        }

        // ---- kayıt ----

        private void ExportViv(HomeSave save)
        {
            var list = new List<HomeVivSave>();
            foreach (var kv in _vivs)
            {
                var v = kv.Value;
                if (v.Habitat == null && v.Entries.Count == 0) continue;
                var e = new List<string>();
                foreach (var x in v.Entries) e.Add(x.Encode());
                list.Add(new HomeVivSave { id = kv.Key, habitat = v.Habitat, moisture = v.Moisture, mold = v.Mold, entries = e.ToArray(), log = v.Log.ToArray() });
            }
            save.vivs = list.ToArray();
        }

        private void ImportViv(HomeSave save)
        {
            _vivs.Clear();
            if (save?.vivs == null) return;
            foreach (var s in save.vivs)
            {
                if (s == null) continue;
                var v = new Vivarium { Habitat = string.IsNullOrEmpty(s.habitat) ? null : s.habitat, Moisture = s.moisture, Mold = s.mold };
                if (s.entries != null) foreach (var x in s.entries) { var e = VivEntry.Decode(x); if (e != null && Def(e.Item)?.Viv != null) v.Entries.Add(e); }
                if (s.log != null) v.Log.AddRange(s.log);
                _vivs[s.id] = v;
            }
        }
    }
}
