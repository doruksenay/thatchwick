using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Build
{
    /// <summary>Bir rafın çökmesinin dökümü (bildirim ve testler için).</summary>
    public sealed class CollapseReport
    {
        public ItemInstance Surface;
        public int Broken;
        public int Damaged;
        /// <summary>Doldurulan dekordan yere dökülüp kaybolan yiyecek.</summary>
        public int Spilled;
        public ItemInstance Debris;
        public string Text;
    }

    /// <summary>
    /// Ev dekorasyonunun zamanla işleyen kuralları (Project belgesi 27, D1):
    /// - Aşırı yüklenen raf çöker (karar 18.11.2026): yük taşımayı aşınca her oyun saatinde aşım oranının iki katı
    ///   (en az %10) şansla. Kil ve cam düşünce kırılır, diğerleri hasar alır; duvar rafı duvardan kopar. Düşenler
    ///   önündeki karoda döküntü yığını olur ("Topla" ile envantere gelir). Odadakiler "Ödüm koptu" der.
    /// - Metal ayna kararır (güzelliği yarıya iner; "Cilala" düzeltir).
    /// - Ergenlikteki ücretsiz Ayna hakkı, evde ayna yoksa bir kez hatırlatılır.
    /// D2 (§9): doldurulan dekor (meyve sepeti, peynir tahtası) envanterdeki gerçek ürünle dolar; içerik oda sıcaklığında
    /// bozulur (taze → bayat → çürük), yenebilir, boşaltılınca envantere döner (çürük atılır). Perde ve kumaş duvar
    /// örtüsü asıldığı kenarın ısı kaybını azaltır (§4.5).
    /// Sayılar yer tutucudur (Adım 28).
    /// </summary>
    public sealed class DecorSystem
    {
        public const double HourDays = 1.0 / 24.0;
        public const double MinCollapseChance = 0.10;
        public const string ScareMoodId = "mood.shelf_fell";
        public const int ScareMoodValue = -5;
        public const double ScareMoodDays = 0.1;

        private readonly WorldState _world;
        private readonly ContentDatabase _content;
        private readonly Random _rng;
        private double _hourAccum;
        private readonly HashSet<int> _mirrorReminded = new HashSet<int>();

        /// <summary>Çökme zarı (0-1); testler sabit değer verebilir. Null ise tohumlu rastgele.</summary>
        public Func<double> Roll { get; set; }
        /// <summary>Hane üyeleri (moodlet ve ayna hatırlatması); null ise yok.</summary>
        public Func<IEnumerable<Character>> Members { get; set; }
        /// <summary>Döküntü konamazsa düşenlerin gideceği envanter; null ise kaybolur.</summary>
        public Household House { get; set; }
        /// <summary>Bildirim (metin, karakter kimliği; 0 = genel).</summary>
        public Action<string, int> Notify { get; set; }
        /// <summary>Bir raf çöktü.</summary>
        public event Action<CollapseReport> Collapsed;
        /// <summary>Çökme şansı çarpanı (yükseltme; belge 45); null ise 1.</summary>
        public Func<ItemInstance, double> CollapseFactor { get; set; }
        /// <summary>Karo sıcaklığı (içerik bozulma hızı); null ise normal hız.</summary>
        public Func<GridPos, float> TemperatureAt { get; set; }

        public DecorSystem(WorldState world, ContentDatabase content, int seed = 1)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _rng = new Random(seed);
        }

        private double NextRoll() => Roll != null ? Roll() : _rng.NextDouble();

        /// <summary>Saatlik çökme şansı: aşım oranının iki katı, en az %10, en çok %100. Aşım yoksa 0.</summary>
        public static double CollapseChance(int load, int maxLoad)
        {
            if (maxLoad <= 0 || load <= maxLoad) return 0;
            double over = (double)load / maxLoad - 1.0;
            return Math.Min(1.0, Math.Max(MinCollapseChance, 2.0 * over));
        }

        public void Tick(double gameDays)
        {
            if (gameDays <= 0) return;
            foreach (var it in _world.Items)
                if (it.Def.TarnishDays > 0f) it.TarnishDays += gameDays;
            AgeContents(gameDays);

            _hourAccum += gameDays;
            while (_hourAccum >= HourDays)
            {
                _hourAccum -= HourDays;
                CheckOverloads();
            }
        }

        /// <summary>Aşırı yüklü her yüzey için çökme zarı atar.</summary>
        public void CheckOverloads()
        {
            var overloaded = new List<ItemInstance>();
            foreach (var it in _world.Items)
                if (!it.IsDecor && it.Def.MaxLoad > 0 && it.Condition != ItemCondition.Damaged && _world.IsOverloaded(it)) overloaded.Add(it);
            foreach (var s in overloaded)
            {
                if (_world.GetItem(s.Id) == null) continue;
                double chance = CollapseChance(_world.LoadOf(s.Id), s.Def.MaxLoad);
                if (CollapseFactor != null) chance *= CollapseFactor(s);   // cilalı, demir destekli raf (belge 45)
                chance *= ItemQuality.DamageFactor(s.Quality);              // usta işi raf daha az çöker (belge 53)
                if (NextRoll() < chance) Collapse(s);
            }
        }

        /// <summary>Rafı çökertir (testler ve olaylar doğrudan çağırabilir).</summary>
        public CollapseReport Collapse(ItemInstance surface)
        {
            if (surface == null || _world.GetItem(surface.Id) == null) return null;
            var report = new CollapseReport { Surface = surface };
            var fallen = new List<StoredItem>();
            foreach (var d in _world.GetDecorOn(surface.Id))
            {
                _world.RemoveSingle(d);
                if (d.Def.IsFillDecor && d.HasContents) { report.Spilled += d.Contents.Count; d.Contents.Clear(); }   // yiyecek yere döküldü
                string mat = d.Def.EffectiveMaterial;
                if (Decor.IsFragile(mat))
                {
                    string shard = mat == Decor.Glass ? Decor.BrokenGlass : Decor.BrokenPottery;
                    if (_content.Items.Contains(shard)) fallen.Add(new StoredItem(shard, ItemCondition.Sound));
                    report.Broken++;
                }
                else
                {
                    fallen.Add(new StoredItem(d.Def.Id, ItemCondition.Damaged) { Quality = d.Quality, Maker = d.Maker });
                    report.Damaged++;
                }
            }
            var tile = surface.Position;
            string what = surface.Def.DisplayName;
            if (surface.IsWallMounted)
            {
                // Duvar rafı duvardan kopar: hasarlı olarak döküntüye düşer.
                _world.RemoveSingle(surface);
                fallen.Add(new StoredItem(surface.Def.Id, ItemCondition.Damaged) { Quality = surface.Quality, Maker = surface.Maker });
            }
            else surface.Condition = ItemCondition.Damaged;

            report.Debris = Drop(tile, fallen);
            var parts = new List<string>();
            if (report.Broken > 0) parts.Add($"{report.Broken} eşya kırıldı");
            if (report.Damaged > 0) parts.Add($"{report.Damaged} eşya hasar aldı");
            if (report.Spilled > 0) parts.Add($"{report.Spilled} yiyecek yere döküldü");
            report.Text = $"{what} aşırı yükten çöktü" + (parts.Count > 0 ? ": " + string.Join(", ", parts) : "") + ".";
            _world.RaiseChanged();
            Scare(tile);
            Notify?.Invoke(report.Text, 0);
            Collapsed?.Invoke(report);
            return report;
        }

        /// <summary>Düşenleri karodaki döküntü yığınına ekler; yoksa yenisini serer. Serilemezse envantere verir.</summary>
        public ItemInstance Drop(GridPos tile, List<StoredItem> fallen)
        {
            if (fallen == null || fallen.Count == 0) return null;
            ItemInstance pile = null;
            foreach (var rug in _world.RugsAt(tile)) if (rug.Def.Id == Decor.Debris) pile = rug;
            if (pile == null && _content.Items.TryGet(Decor.Debris, out var debrisDef) && _world.CanPlaceRug(debrisDef, tile, 0).Ok)
                pile = _world.PlaceItem(debrisDef, tile, 0);
            if (pile == null)
            {
                if (House != null) foreach (var f in fallen) House.AddItem(f.ItemId, 1, f.Condition);
                return null;
            }
            pile.Contents.AddRange(fallen);
            return pile;
        }

        private void Scare(GridPos tile)
        {
            if (Members == null) return;
            var room = _world.Map.Rooms.RoomAt(tile);
            if (room == null) return;
            foreach (var c in Members())
            {
                if (c.IsDead || !c.MoodEnabled) continue;
                if (_world.Map.Rooms.RoomAt(c.Position) == room) c.AddMoodlet(ScareMoodId, "Ödüm koptu", ScareMoodValue, ScareMoodDays);
            }
        }

        // ---- ayna ----

        /// <summary>Evde (dünyada) bir ayna var mı: Mirror (görünüş değiştirme) bunu ister (karar 18.11.2026).</summary>
        public static bool HasMirror(WorldState world)
        {
            if (world == null) return false;
            foreach (var it in world.Items) if (it.Def.IsMirror) return true;
            return false;
        }

        /// <summary>Ücretsiz Ayna hakkı olup evde aynası olmayanları bir kez hatırlatır (yeni günde çağrılır).</summary>
        // ---- mevsim süsü (D3b, §7.11, §9) ----

        /// <summary>Solmuş mevsim süsünün güzelliği (eksi).</summary>
        public const int WiltedBeauty = -2;

        /// <summary>Mevsimi geçmiş süs soldu mu (bahar dalı yazın, kış dalı baharda)?</summary>
        public static bool IsWilted(ItemInstance item, Time.Season season) => item != null && item.Def.IsSeasonal && !item.Def.InSeason(season);

        /// <summary>Güzellik eki: solmuş süs kendi güzelliğini kaybeder ve WiltedBeauty olur; değilse 0.</summary>
        public static double SeasonalBeauty(ItemInstance item, Time.Season season) => IsWilted(item, season) ? -item.Def.Beauty + WiltedBeauty : 0;

        /// <summary>Mevsim döndü: solan süsleri sayar ve bildirir. Solan adet.</summary>
        public int OnSeasonChanged(Time.Season season)
        {
            var names = new List<string>();
            int n = 0;
            foreach (var it in _world.Items)
            {
                if (!IsWilted(it, season)) continue;
                n++;
                if (!names.Contains(it.Def.DisplayName)) names.Add(it.Def.DisplayName);
            }
            if (n > 0) Notify?.Invoke($"Mevsim döndü: {n} mevsim süsü soldu ({string.Join(", ", names).ToLowerInvariant()}). Kaldırıp mevsimin süsünü as.", 0);
            return n;
        }

        public void RemindFreeMirror()
        {
            if (Members == null || HasMirror(_world)) return;
            foreach (var c in Members())
            {
                if (c.IsDead || !c.FreeMirrorAvailable || _mirrorReminded.Contains(c.Id)) continue;
                _mirrorReminded.Add(c.Id);
                Notify?.Invoke($"{c.Name} ücretsiz Ayna hakkı kazandı, ama evde ayna yok. Bir ayna asınca hakkını kullanabilir.", c.Id);
            }
        }

        // ---- doldurulan dekor (D2, §9) ----

        private string Name(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        private void AgeContents(double gameDays)
        {
            foreach (var it in _world.Items)
            {
                if (!it.Def.IsFillDecor || !it.HasContents) continue;
                double rate = TemperatureAt != null ? Life.SpoilageService.TemperatureFactor(TemperatureAt(it.Position)) : 1.0;
                if (it.Def.HasTag(Decor.SealedTag)) rate *= Decor.SealedAgeFactor;   // ağzı bağlı kavanoz: içindeki dört kat yavaş bozulur
                bool rotted = false;
                var list = it.Contents;
                for (int i = 0; i < list.Count; i++)
                {
                    var c = list[i];
                    if (!_content.Items.TryGet(c.ItemId, out var food) || food.SpoilDays <= 0f) continue;
                    int before = Decor.Freshness(food, c.Age);
                    c.Age += gameDays * rate;
                    list[i] = c;
                    if (before != Decor.Rotten && Decor.Freshness(food, c.Age) == Decor.Rotten) rotted = true;
                }
                if (rotted) Notify?.Invoke($"{it.Def.DisplayName} içindekiler çürüdü; boşaltılmalı.", 0);
            }
        }

        /// <summary>Doldurmanın engeli (metin) ya da null: dolu mu, envanterde uygun ürün var mı.</summary>
        public static string FillBlocker(ItemInstance item, Household house, ContentDatabase content)
        {
            var def = item.Def;
            if (!def.IsFillDecor) return "Bu eşya doldurulmaz.";
            if (item.Contents.Count >= def.FillCount) return $"{def.DisplayName} zaten dolu.";
            int have = 0;
            foreach (var f in def.FillWith) have += house.Count(f);
            if (have > 0) return null;
            string first = def.FillWith.Count > 0 && content.Items.TryGet(def.FillWith[0], out var fd) ? fd.DisplayName.ToLowerInvariant() : "ürün";
            string what = def.FillWith.Count > 1 ? $"{first} ya da benzeri" : first;
            return $"Envanterde {what} yok (0/{def.FillCount - item.Contents.Count}).";
        }

        /// <summary>
        /// Envanterden gerçek ürünle doldurur: önce hanenin kendi ürünü, sonra pazar malı; tercih sırası FillWith sırasıdır.
        /// Yaşı envanterdeki en eski partiden alır (ageOf; yoksa taze). Eklenen adet.
        /// </summary>
        public static int Fill(ItemInstance item, Household house, ContentDatabase content, Func<string, double> ageOf = null)
        {
            var def = item.Def;
            if (!def.IsFillDecor) return 0;
            int added = 0;
            for (int pass = 0; pass < 2 && item.Contents.Count < def.FillCount; pass++)
                foreach (var f in def.FillWith)
                    while (item.Contents.Count < def.FillCount && (pass == 0 ? house.OwnCount(f) > 0 : house.Count(f) > 0))
                    {
                        if (def.HasTag(Decor.DistinctTag) && HasContent(item, f)) break;   // sergi rafı: her eşyadan bir tane (belge 45)
                        double age = ageOf != null ? Math.Max(0, ageOf(f)) : 0;
                        if (!house.TryTakePreferOwn(f, out bool own, out var key, TakePolicy.Highest)) break;
                        item.Contents.Add(new StoredItem(f, age, own) { Quality = key.Quality, Maker = key.Maker });
                        added++;
                    }
            return added;
        }

        private static bool HasContent(ItemInstance item, string id)
        {
            foreach (var c in item.Contents) if (c.ItemId == id) return true;
            return false;
        }

        /// <summary>İçindekilerden bir tane yer: önce en bayatı (çürük yenmez). Yenen ürünün kimliği ya da null.</summary>
        /// <summary>Bir sonraki yenecek olan (en eski, çürük olmayan) içerik; yoksa null. İçerikten çıkarmaz.</summary>
        public static string NextToEat(ItemInstance item, ContentDatabase content)
        {
            int i = NextToEatIndex(item, content);
            return i < 0 ? null : item.Contents[i].ItemId;
        }

        private static int NextToEatIndex(ItemInstance item, ContentDatabase content)
        {
            int best = -1; double bestAge = -1;
            for (int i = 0; i < item.Contents.Count; i++)
            {
                var c = item.Contents[i];
                content.Items.TryGet(c.ItemId, out var food);
                if (Decor.Freshness(food, c.Age) == Decor.Rotten) continue;
                if (c.Age > bestAge) { best = i; bestAge = c.Age; }
            }
            return best;
        }

        public static string EatOne(ItemInstance item, ContentDatabase content)
        {
            int best = -1; double bestAge = -1;
            for (int i = 0; i < item.Contents.Count; i++)
            {
                var c = item.Contents[i];
                content.Items.TryGet(c.ItemId, out var food);
                if (Decor.Freshness(food, c.Age) == Decor.Rotten) continue;
                if (c.Age > bestAge) { best = i; bestAge = c.Age; }
            }
            if (best < 0) return null;
            string id = item.Contents[best].ItemId;
            item.Contents.RemoveAt(best);
            return id;
        }

        /// <summary>Yenebilir (çürük olmayan) içerik adedi.</summary>
        public static int EdibleCount(ItemInstance item, ContentDatabase content)
        {
            int n = 0;
            foreach (var c in item.Contents)
                if (!content.Items.TryGet(c.ItemId, out var food) || Decor.Freshness(food, c.Age) != Decor.Rotten) n++;
            return n;
        }

        /// <summary>
        /// İçindekileri envantere döndürür: çürük atılır, pazar malı yine "alınmış" sayılır. (dönen, atılan) adet.
        /// Doldurulan dekor kaldırılırken ya da satılırken de çağrılır.
        /// </summary>
        public static (int returned, int discarded) EmptyInto(ItemInstance item, Household house, ContentDatabase content)
        {
            int ret = 0, gone = 0;
            if (item == null || !item.HasContents) return (0, 0);
            foreach (var c in item.Contents)
            {
                if (!content.Items.TryGet(c.ItemId, out var food)) { gone++; continue; }
                if (Decor.Freshness(food, c.Age) == Decor.Rotten) { gone++; continue; }
                house.Add(new ItemKey(c.ItemId, c.Quality, c.Condition, !c.Own && item.Def.IsFillDecor, c.Maker), 1);
                ret++;
            }
            item.Contents.Clear();
            return (ret, gone);
        }

        /// <summary>Doldurulan dekorun (meyve sepeti gibi) içinde çürümüş bir şey var mı: güzelliği düşürür, sinek ve koku kaynağıdır.</summary>
        public static bool IsRottenFill(ItemInstance item, ContentDatabase content) =>
            item != null && item.Def.IsFillDecor && item.HasContents && WorstFreshness(item, content) == Decor.Rotten;

        /// <summary>İçeriğin en kötü tazeliği (boşsa taze).</summary>
        public static int WorstFreshness(ItemInstance item, ContentDatabase content)
        {
            int worst = Decor.Fresh;
            foreach (var c in item.Contents)
                if (content.Items.TryGet(c.ItemId, out var food)) worst = Math.Max(worst, Decor.Freshness(food, c.Age));
            return worst;
        }

        /// <summary>
        /// Doldurulan dekorun güzelliği: boşken taban güzelliğin %30'u, dolulukla tama çıkar; bayat içerik yarım sayılır.
        /// Tam dolu ve hepsi hanenin kendi ürünü ise "Kendi emeğimiz" eki. Çürük içerik varsa sinek: RottenBeauty.
        /// </summary>
        public static double FillBeauty(ItemInstance item, ContentDatabase content)
        {
            var def = item.Def;
            if (!def.IsFillDecor) return def.Beauty;
            if (WorstFreshness(item, content) == Decor.Rotten) return Decor.RottenBeauty;
            double filled = 0; bool allOwn = item.Contents.Count >= def.FillCount;
            foreach (var c in item.Contents)
            {
                content.Items.TryGet(c.ItemId, out var food);
                filled += Decor.Freshness(food, c.Age) == Decor.Stale ? 0.5 : 1.0;
                if (!c.Own) allOwn = false;
            }
            double ratio = Math.Min(1.0, filled / def.FillCount);
            return def.Beauty * (0.3 + 0.7 * ratio) + (allOwn && item.Contents.Count > 0 ? Decor.OwnWorkBonus : 0);
        }

        /// <summary>Doldurulan dekorun kısa durumu: "4/6 · bayat · Kendi emeğimiz".</summary>
        public static string FillLabel(ItemInstance item, ContentDatabase content)
        {
            var def = item.Def;
            if (!def.IsFillDecor) return null;
            if (!item.HasContents) return $"Boş (0/{def.FillCount})";
            var parts = new List<string> { $"{item.Contents.Count}/{def.FillCount}" };
            int worst = WorstFreshness(item, content);
            if (worst != Decor.Fresh) parts.Add(Decor.FreshnessName(worst));
            bool allOwn = item.Contents.Count >= def.FillCount;
            foreach (var c in item.Contents) if (!c.Own) allOwn = false;
            if (allOwn) parts.Add("Kendi emeğimiz");
            return string.Join(" · ", parts);
        }

        // ---- ısı: perde ve kumaş duvar örtüsü (§4.5) ----

        /// <summary>Kenarın ısı kaybı çarpanı: iki yüzünde asılı eşyaların en iyisi (perde 0,75; kumaş örtü 0,85). Yoksa 1.</summary>
        public static float EdgeInsulation(WorldState world, Edge edge)
        {
            float f = 1f;
            if (world == null) return f;
            foreach (var it in world.WallItemsOn(edge))
                if (it.Def.Insulation > 0f && it.Def.Insulation < f && it.Condition != ItemCondition.Damaged) f = it.Def.Insulation;
            return f;
        }

        public static string MaterialName(string material)
        {
            switch (material)
            {
                case Decor.Wood: return "ahşap";
                case Decor.Metal: return "metal";
                case Decor.Cloth: return "kumaş";
                case Decor.Clay: return "kil";
                case Decor.Glass: return "cam";
                case Decor.Stone: return "taş";
                default: return material;
            }
        }
    }
}
