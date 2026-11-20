using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Time;
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
        /// <summary>Bitki ve süsün tabandaki yeri (0 sol, 1 sağ); oyuncu yakın görünümde kaydırır.</summary>
        public double Pos = 0.5;
        public string Encode() => FormattableString.Invariant($"{Item}|{Health:0.##}|{Hunger:0.##}|{Age:0.###}|{Growth:0.###}|{Pos:0.###}");
        public static VivEntry Decode(string s)
        {
            var p = s?.Split('|');
            if (p == null || p.Length < 5) return null;
            double D(int i) { double.TryParse(p[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v); return v; }
            return new VivEntry { Item = p[0], Health = D(1), Hunger = D(2), Age = D(3), Growth = D(4), Pos = p.Length > 5 ? D(5) : 0.5 };
        }
    }

    /// <summary>Bir cam bahçenin içi: habitat, nem (tuzlu suda tazelik), küf, türler ve son olaylar.</summary>
    public sealed class Vivarium
    {
        public string Habitat;            // null = boş kap (henüz zemin seçilmedi)
        public string Substrate;          // zemine konan malzeme (orman toprağı, göl kumu, deniz kumu, sazlık çamuru)
        public double Moisture = 70;      // nemli ve kuru: nem; tuzlu ve tatlı su: suyun tazeliği
        public double Mold;               // ölü canlı ve fazla nemle birikir; temizleyiciler azaltır
        public readonly List<VivEntry> Entries = new List<VivEntry>();
        /// <summary>Son olaylar: anahtarlı metin (dil kayda girmez). Eşya argümanları kimlik olarak taşınır (bkz. VivText).</summary>
        public readonly List<LocText> Log = new List<LocText>();
        public void Note(LocText t) { Log.Add(t); while (Log.Count > 8) Log.RemoveAt(0); }
        public void Note(string key, params (string name, object value)[] args) => Note(LocText.Of(key, args));
    }

    /// <summary>
    /// Cam bahçe metinleri (yerelleştirme kuralı, docs/yerellestirme.md): Core anahtar ve argüman verir, Game çözer.
    /// "item." ile başlayan string argüman eşya kimliğidir; Game o anki dildeki eşya adına çevirir.
    /// Kayıt için kodlanır: anahtar ve argümanlar (s: metin, n: sayı, #: argümansız iç anahtar).
    /// </summary>
    public static class VivText
    {
        public static LocText Habitat(string h) => LocText.Of("core.viv.habitat." + (h ?? "none"));

        /// <summary>
        /// Çözer: "item." argümanları eşya adına çevrilir; ad cümle başında değilse dilin kuralıyla küçük harfe iner
        /// ("Uğur böceği, yaprak biti yer"). Game katmanı ve testler kullanır.
        /// </summary>
        public static string Resolve(Localizer loc, LocText t, Func<string, string> itemName)
        {
            if (t.IsEmpty) return "";
            if (t.Literal != null) return t.Literal;
            string template = loc.Get(t.Key);
            var args = new (string name, object value)[t.Args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var (n, val) = t.Args[i];
                if (val is string str && str.StartsWith("item.", StringComparison.Ordinal))
                {
                    string name = itemName?.Invoke(str) ?? str;
                    val = template.StartsWith("{" + n + "}", StringComparison.Ordinal) ? name : loc.ToLower(name);
                }
                args[i] = (n, val);
            }
            return loc.Fill(template, args);
        }
        public static LocText Need(string n) => LocText.Of("core.viv.need." + n);

        private const char Sep = '\u001f', Eq = '\u001e';

        public static string Encode(LocText t)
        {
            if (t.Literal != null) return "=" + t.Literal;
            var sb = new System.Text.StringBuilder(t.Key ?? "");
            foreach (var (name, value) in t.Args)
            {
                sb.Append(Sep).Append(name).Append(Eq);
                switch (value)
                {
                    case LocText lt: sb.Append('#').Append(lt.Key); break;
                    case string str: sb.Append('s').Append(str); break;
                    case IFormattable f: sb.Append('n').Append(f.ToString(null, System.Globalization.CultureInfo.InvariantCulture)); break;
                    default: sb.Append('s').Append(value); break;
                }
            }
            return sb.ToString();
        }

        public static LocText Decode(string s)
        {
            if (string.IsNullOrEmpty(s)) return LocText.Plain("");
            if (s[0] == '=') return LocText.Plain(s.Substring(1));
            var parts = s.Split(Sep);
            if (parts[0].IndexOf(' ') >= 0 || parts[0].IndexOf('.') < 0) return LocText.Plain(s);   // eski kayıt: düz Türkçe metin
            var args = new List<(string, object)>();
            for (int i = 1; i < parts.Length; i++)
            {
                int k = parts[i].IndexOf(Eq);
                if (k <= 0 || k + 1 >= parts[i].Length) continue;
                string name = parts[i].Substring(0, k), raw = parts[i].Substring(k + 2);
                char kind = parts[i][k + 1];
                object val = kind == '#' ? (object)LocText.Of(raw)
                    : kind == 'n' && double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d
                    : raw;
                args.Add((name, val));
            }
            return LocText.Of(parts[0], args.ToArray());
        }
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
        public const string Moist = "nemli", Dry = "kuru", Salt = "tuzlu", Fresh = "tatli";
        public static readonly string[] Habitats = { Moist, Dry, Fresh, Salt };
        public static bool IsWater(string h) => h == Salt || h == Fresh;
        public const double PredationChance = 0.55, ShelteredPredationChance = 0.3, UnmetNeedDamage = 8;
        public const string CatchJar = "item.glass_jar";
        public const string FreshWaterItem = "item.viv_fresh_water", SeaWaterItem = "item.viv_sea_water";

        /// <summary>Zemin kurmak için doğadan getirilen malzeme (kullanıcı 20.11.2026: her şey doğadan).</summary>
        public sealed class HabitatNeed
        {
            public string Habitat;
            public string[] Substrates;   // biri yeter; oyuncu seçer
            public string Water;          // su zemininde ayrıca 1 kavanoz su
        }

        public static readonly HabitatNeed[] HabitatNeeds =
        {
            new HabitatNeed { Habitat = Moist, Substrates = new[] { "item.viv_forest_soil" } },
            new HabitatNeed { Habitat = Dry, Substrates = new[] { "item.sand", "item.viv_sea_sand" } },
            new HabitatNeed { Habitat = Fresh, Substrates = new[] { "item.sand", "item.viv_marsh_mud" }, Water = FreshWaterItem },
            new HabitatNeed { Habitat = Salt, Substrates = new[] { "item.viv_sea_sand" }, Water = SeaWaterItem },
        };

        public static HabitatNeed NeedFor(string habitat) { foreach (var n in HabitatNeeds) if (n.Habitat == habitat) return n; return null; }

        /// <summary>Su zemininde suyu tazelemek için gereken kavanoz suyu (yoksa null).</summary>
        public static string WaterFor(string habitat) => NeedFor(habitat)?.Water;
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

        public static LocText HabitatName(string h) => VivText.Habitat(h);
        private ItemDef Def(string id) => _content.Items.TryGet(id, out var d) ? d : null;
        private VivDef V(string id) => Def(id)?.Viv;

        public int CountPlants(Vivarium v) { int n = 0; foreach (var e in v.Entries) if (V(e.Item)?.IsPlant == true) n++; return n; }
        public int CountDecor(Vivarium v) { int n = 0; foreach (var e in v.Entries) if (V(e.Item)?.IsDecor == true) n++; return n; }
        public int CountCreatures(Vivarium v) { int n = 0; foreach (var e in v.Entries) if (V(e.Item)?.IsCreature == true) n++; return n; }

        /// <summary>Cam bahçedeki süslerin etiketleri (hide, bask, shell).</summary>
        public HashSet<string> DecorTags(Vivarium v)
        {
            var set = new HashSet<string>();
            foreach (var e in v.Entries) { var s = V(e.Item); if (s != null && s.IsDecor) foreach (var t in s.Tags) set.Add(t); }
            return set;
        }

        /// <summary>Canlının karşılanmayan isteği (süs etiketi) ya da null.</summary>
        public string UnmetNeed(Vivarium v, VivDef s)
        {
            if (s == null || s.Needs.Count == 0) return null;
            var tags = DecorTags(v);
            foreach (var n in s.Needs) if (!tags.Contains(n)) return n;
            return null;
        }

        public static LocText NeedName(string n) => VivText.Need(n);

        /// <summary>Bitki ya da süsü tabanda kaydırır (yakın görünümde düzenleme).</summary>
        public void MoveInViv(ItemInstance item, int index, double delta)
        {
            var v = VivOf(item);
            if (v == null || index < 0 || index >= v.Entries.Count) return;
            var e = v.Entries[index];
            e.Pos = Math.Max(0.06, Math.Min(0.94, e.Pos + delta));
        }

        private static double NextPos(Vivarium v, ContentDatabase db)
        {
            int n = 0;
            foreach (var e in v.Entries) if (db.Items.TryGet(e.Item, out var d) && d.Viv != null && !d.Viv.IsCreature) n++;
            double p = (0.5 + n * 0.618) % 1.0;
            return 0.08 + p * 0.84;
        }

        /// <summary>Boş kabın zeminini seçer (toprak, kum ya da tuzlu su). Dolu kapta değişmez.</summary>
        /// <summary>Zemin kurulamıyorsa neden (eksik malzeme); kurulabiliyorsa null.</summary>
        public LocText? HabitatBlocker(ItemInstance item, string habitat, string substrate)
        {
            var v = VivOf(item);
            if (v == null) return LocText.Of("core.viv.err.not_viv");
            var need = NeedFor(habitat);
            if (need == null) return LocText.Of("core.viv.err.unknown_ground");
            if (v.Habitat != null) return LocText.Of(v.Entries.Count > 0 ? "core.viv.err.ground_has_life" : "core.viv.err.empty_first");
            if (Array.IndexOf(need.Substrates, substrate) < 0) return LocText.Of("core.viv.err.wrong_substrate", ("ground", HabitatName(habitat)), ("item", substrate));
            if (_house.Count(substrate) <= 0) return LocText.Of("core.viv.err.no_substrate", ("item", substrate));
            if (need.Water != null && _house.Count(need.Water) <= 0) return LocText.Of(habitat == Salt ? "core.viv.err.no_sea_water" : "core.viv.err.no_fresh_water", ("item", need.Water));
            return null;
        }

        /// <summary>Zemini kurar: malzeme (ve su zemininde 1 kavanoz su) envanterden harcanır. substrate null ise elde olan ilk uygun malzeme.</summary>
        public LocText? SetHabitat(ItemInstance item, string habitat, string substrate = null)
        {
            var need = NeedFor(habitat);
            if (need == null) return LocText.Of("core.viv.err.unknown_ground");
            if (substrate == null) { substrate = need.Substrates[0]; foreach (var s in need.Substrates) if (_house.Count(s) > 0) { substrate = s; break; } }
            var why = HabitatBlocker(item, habitat, substrate);
            if (why != null) return why;
            var v = VivOf(item);
            _house.TryTakeItem(substrate);
            if (need.Water != null) _house.TryTakeItem(need.Water);
            v.Habitat = habitat; v.Substrate = substrate; v.Moisture = habitat == Dry ? 25 : IsWater(habitat) ? 100 : 80; v.Mold = 0;
            v.Note("core.viv.log.ground", ("ground", HabitatName(habitat)), ("item", substrate));
            _world.NotifyChanged();
            return null;
        }

        /// <summary>Boş kabın zeminini döker (malzeme geri gelmez); yeni zemin kurulabilir.</summary>
        public LocText? EmptyViv(ItemInstance item)
        {
            var v = VivOf(item);
            if (v == null || v.Habitat == null) return LocText.Of("core.viv.err.already_empty");
            if (v.Entries.Count > 0) return LocText.Of("core.viv.err.take_out_first");
            v.Habitat = null; v.Substrate = null; v.Mold = 0;
            v.Note("core.viv.log.emptied");
            _world.NotifyChanged();
            return null;
        }

        /// <summary>Bu türün bu cam bahçeye eklenemeyeceği neden; eklenebiliyorsa null. "Oluşmaması gereken" kombinasyonlar burada.</summary>
        public LocText? CanAddToViv(ItemInstance item, string speciesId)
        {
            var v = VivOf(item);
            if (v == null) return LocText.Of("core.viv.err.not_viv");
            var sd = Def(speciesId);
            if (sd?.Viv == null) return LocText.Of("core.viv.err.not_species");
            var s = sd.Viv;
            if (v.Habitat == null) return LocText.Of("core.viv.err.no_ground");
            if (!s.FitsHabitat(v.Habitat))
                return LocText.Of("core.viv.err.wrong_ground", ("item", speciesId), ("wants", HabitatName(s.Habitats[0])), ("here", HabitatName(v.Habitat)));
            if (s.IsPlant && CountPlants(v) >= item.Def.VivPlants) return LocText.Of("core.viv.err.plants_full", ("max", item.Def.VivPlants));
            if (s.IsDecor && CountDecor(v) >= item.Def.VivDecor) return LocText.Of("core.viv.err.decor_full", ("max", item.Def.VivDecor));
            if (s.IsCreature && CountCreatures(v) >= item.Def.VivCreatures) return LocText.Of("core.viv.err.creatures_full", ("max", item.Def.VivCreatures));
            foreach (var e in v.Entries)
            {
                var o = V(e.Item);
                if (o == null) continue;
                if (e.Item == speciesId && s.Solitary) return LocText.Of("core.viv.err.solitary", ("item", speciesId));
                if (s.Avoid.Contains(e.Item) || o.Avoid.Contains(speciesId))
                    return LocText.Of((s.Has("toxic") || o.Has("toxic")) ? "core.viv.err.toxic" : "core.viv.err.avoid", ("a", e.Item), ("b", speciesId));
            }
            if (_house.Count(speciesId) <= 0) return LocText.Of("core.viv.err.none_in_inventory", ("item", speciesId));
            return null;
        }

        /// <summary>Eklemeden önce uyarı: kim kimi yer (eklenebilir ama sonucu oyuncu bilsin).</summary>
        public List<LocText> AddWarnings(ItemInstance item, string speciesId)
        {
            var list = new List<LocText>();
            var v = VivOf(item);
            var s = V(speciesId);
            if (v == null || s == null) return list;
            foreach (var e in v.Entries)
            {
                var o = V(e.Item);
                if (o == null) continue;
                if (s.Eats.Contains(e.Item)) list.Add(LocText.Of("core.viv.warn.eats", ("a", speciesId), ("b", e.Item)));
                if (o.Eats.Contains(speciesId)) list.Add(LocText.Of("core.viv.warn.eats", ("a", e.Item), ("b", speciesId)));
            }
            if (s.IsCreature && s.Eats.Count > 0 && !HasFoodFor(v, s, speciesId)) list.Add(LocText.Of("core.viv.warn.no_food", ("item", speciesId)));
            if (s.IsCreature) { var need = UnmetNeed(v, s); if (need != null) list.Add(LocText.Of("core.viv.warn.need", ("item", speciesId), ("need", NeedName(need)))); }
            return list;
        }

        public bool AddToViv(ItemInstance item, string speciesId)
        {
            if (CanAddToViv(item, speciesId) != null) return false;
            if (!_house.TryTakeItem(speciesId)) return false;
            var v = VivOf(item);
            var entry = new VivEntry { Item = speciesId };
            if (!V(speciesId).IsCreature) entry.Pos = NextPos(v, _content);
            v.Entries.Add(entry);
            v.Note("core.viv.log.added", ("item", speciesId));
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
            if (release)
            {
                v.Note("core.viv.log.released", ("item", e.Item));
                by?.AddMoodlet("mood.released", "Doğaya saldı", 3, 0.5);
                if (V(e.Item)?.IsCreature != true) { _house.AddItem(e.Item, 1); }   // süs ve bitki salınmaz, geri alınır
            }
            else
            {
                _house.AddItem(e.Item, 1);
                v.Note("core.viv.log.removed", ("item", e.Item));
            }
            _world.NotifyChanged();
        }

        /// <summary>Nemlendir (nemli ve kuru), tuzlu suda suyu tazele (1 tuz).</summary>
        public LocText? Mist(ItemInstance item)
        {
            var v = VivOf(item);
            if (v?.Habitat == null) return LocText.Of("core.viv.err.no_ground");
            if (IsWater(v.Habitat))
            {
                if (v.Moisture > 70) return LocText.Of("core.viv.err.water_fresh");
                string w = WaterFor(v.Habitat);
                if (w != null && !_house.TryTakeItem(w)) return LocText.Of("core.viv.err.need_water", ("item", w));
                v.Moisture = 100;
                v.Note("core.viv.log.water_changed");
                return null;
            }
            double target = v.Habitat == Dry ? 30 : 85;
            if (v.Moisture >= target - 5) return LocText.Of(v.Habitat == Dry ? "core.viv.err.sand_moist" : "core.viv.err.soil_moist");
            v.Moisture = target;
            v.Note("core.viv.log.misted");
            return null;
        }

        /// <summary>Habitata göre uygun nem aralığı.</summary>
        public static (double lo, double hi) MoistRange(string habitat) => habitat == Dry ? (5, 45) : IsWater(habitat) ? (35, 100) : (40, 95);

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
                if (s.IsCreature && e.Hunger >= 60) return false;
                if (s.IsCreature && UnmetNeed(v, s) != null) return false;
                if (s.IsPlant && e.Health < 50) return false;
                if (s.Has("pest")) pests++;
                if (s.Eats.Exists(x => V(x)?.Has("pest") == true)) control = true;
            }
            return pests <= 1 || control;
        }

        /// <summary>Durum satırı: "Nemli toprak · 3 bitki, 4 canlı · nem 62 · dengede".</summary>
        public LocText? VivSummary(ItemInstance item)
        {
            var v = VivOf(item);
            if (v == null) return null;
            if (v.Habitat == null) return LocText.Of("core.viv.sum.empty");
            string state = IsBalanced(v) ? "core.viv.sum.balanced" : v.Entries.Count >= 2 ? "core.viv.sum.unbalanced" : "core.viv.sum.none";
            return LocText.Of(IsWater(v.Habitat) ? "core.viv.sum.water" : "core.viv.sum.soil",
                ("ground", HabitatName(v.Habitat)), ("plants", CountPlants(v)), ("creatures", CountCreatures(v)), ("decor", CountDecor(v)),
                ("value", (int)Math.Round(v.Moisture)), ("mold", (int)Math.Round(v.Mold)), ("state", LocText.Of(state)));
        }

        private double VivBeauty(ItemInstance item)
        {
            if (!item.Def.IsVivarium || !_vivs.TryGetValue(item.Id, out var v) || v.Entries.Count == 0) return 0;
            var species = new HashSet<string>();
            double decor = 0;
            foreach (var e in v.Entries)
            {
                var s = V(e.Item);
                if (s != null && s.IsDecor) decor += s.Beauty;
                else if (e.Health > 30) species.Add(e.Item);
            }
            double b = species.Count * VivBeautyPerSpecies + Math.Min(6, decor) + (IsBalanced(v) ? VivBalancedBeauty : 0) - (v.Mold >= 50 ? 3 : 0);
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
                v.Moisture = Math.Max(0, v.Moisture - gameDays * (IsWater(v.Habitat) ? SaltFreshDecayPerDay : MoistureDecayPerDay));
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
                bool lightOk = s.Has("shade") ? !sun || IsWater(v.Habitat) : s.Has("sun") ? sun : true;
                if (wetOk && lightOk) { e.Health = Math.Min(100, e.Health + 15 + 5 * cleaners); e.Growth = Math.Min(3, e.Growth + 0.5); }
                else e.Health -= wetOk ? 8 : 20;
                if (v.Mold >= 50) e.Health -= 10;
            }

            // 2. Canlılar: yer, acıkır, yaşlanır. Avcı avını (şansla), otçul bitkiyi yer.
            var eaten = new HashSet<VivEntry>();
            bool shelter = DecorTags(v).Contains("hide");
            foreach (var e in new List<VivEntry>(v.Entries))
            {
                var s = V(e.Item);
                if (s == null || !s.IsCreature || eaten.Contains(e)) continue;
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
                        // Taş ya da kabuk altında saklanan yürüyen av daha zor yakalanır.
                        bool hides = shelter && (fs.Move == "crawl" || fs.Move == "burrow" || fs.Move == "bottom");
                        if (Rng.NextDouble() < (hides ? ShelteredPredationChance : PredationChance))
                        {
                            eaten.Add(f);
                            v.Note("core.viv.log.ate", ("a", e.Item), ("b", f.Item));
                            fed = true; break;
                        }
                    }
                    e.Hunger = fed ? Math.Max(0, e.Hunger - 40) : e.Hunger + 30;
                }
                if (!wetOk) e.Health -= v.Habitat == Dry ? 15 : 20;
                else e.Health = Math.Min(100, e.Health + 5);
                var unmet = UnmetNeed(v, s);
                if (unmet != null) e.Health -= UnmetNeedDamage;
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
                    v.Note("core.viv.log.became", ("a", e.Item), ("b", s.Becomes));
                    Say($"Cam bahçede {Def(e.Item).DisplayName.ToLowerInvariant()} {Def(s.Becomes).DisplayName.ToLowerInvariant()} oldu.");
                    e.Item = s.Becomes; e.Age = 0; e.Hunger = 0;
                }
            }

            // 4. Ölüm: sağlık bitti ya da ömür doldu. Ölü canlı küf getirir.
            for (int i = v.Entries.Count - 1; i >= 0; i--)
            {
                var e = v.Entries[i];
                var s = V(e.Item);
                if (s != null && s.IsDecor) continue;
                bool old = s != null && s.IsCreature && s.LifeDays > 0 && e.Age >= s.LifeDays;
                if (e.Health > 0 && !old) continue;
                v.Note(old ? "core.viv.log.died_old" : s != null && s.IsPlant ? "core.viv.log.withered" : "core.viv.log.died", ("item", e.Item));
                v.Entries.RemoveAt(i);
                v.Mold = Math.Min(MoldMax, v.Mold + MoldPerDeath);
            }

            // 5. Küf: fazla nem artırır, temizleyiciler azaltır.
            if (!IsWater(v.Habitat) && v.Moisture > hi - 5) v.Mold = Math.Min(MoldMax, v.Mold + 8);
            v.Mold = Math.Max(0, v.Mold - 6 - 10 * cleaners);

            // 6. Üreme: aynı türden iki tok ve sağlıklı canlı, yer varsa. Yaprak biti uğur böceği yoksa hızla çoğalır.
            var counts = new Dictionary<string, int>();
            foreach (var e in v.Entries) if (V(e.Item)?.Breeds == true && e.Hunger < 50 && e.Health > 60) counts[e.Item] = (counts.TryGetValue(e.Item, out var n) ? n : 0) + 1;
            foreach (var kv in counts)
            {
                if (kv.Value < 2 || CountCreatures(v) >= item.Def.VivCreatures) continue;
                double chance = V(kv.Key).Has("pest") || V(kv.Key).Has("swarm") ? 0.6 : 0.2;
                if (Rng.NextDouble() < chance)
                {
                    v.Entries.Add(new VivEntry { Item = kv.Key, Hunger = 30 });
                    v.Note("core.viv.log.bred", ("item", kv.Key));
                }
            }
        }

        // ---- yakalama ----

        /// <summary>
        /// Oyuncu kendisi yakalıyorsa (balık, karides gibi suda yüzen tür) mini oyun açılır: true dönerse tür envantere
        /// hemen girmez, <see cref="CatchBite"/> olayı gider; oyun bitince <see cref="GrantCatch"/>.
        /// </summary>
        public Func<Character, string, bool> PlayerCatches { get; set; }
        public event Action<Character, string> CatchBite;

        /// <summary>Kavanoza girmesi mini oyun ister mi (suda yüzen ya da dipte gezen canlı).</summary>
        public bool IsSwimmer(string id) { var s = V(id); return s != null && s.IsCreature && (s.Move == "swim" || s.Move == "bottom"); }

        /// <summary>Kavanoza bir canlı girdi (eşya kimliği): böcek ve balık defteri (belge 48 §9).</summary>
        public event Action<string> Caught;

        /// <summary>Yakalama başlama engeli: cam kavanoz gerekir; bu saat ve mevsimde bir şey yoksa söylenir.</summary>
        public string CatchBlocker(ItemInstance node)
        {
            if (node.Def.Catches.Count == 0) return "Burada yakalanacak bir şey yok.";
            if (_house.Count(CatchJar) <= 0) return "Yakalamak için envanterde boş cam kavanoz olmalı.";
            return CatchPool(node.Def.Catches).Count == 0 ? "Şu an burada yakalanacak bir şey yok (mevsim, saat ya da hava)." : null;
        }

        private List<CatchDef> CatchPool(List<CatchDef> catches)
        {
            var list = new List<CatchDef>();
            bool night = IsNight(_clock.DayFraction), rain = Raining?.Invoke() == true;
            foreach (var c in catches)
            {
                if (c.Seasons.Count > 0 && !c.Seasons.Contains(_clock.Season)) continue;
                if (c.Night && !night) continue;
                if (c.Rain && !rain) continue;
                list.Add(c);
            }
            return list;
        }

        /// <summary>Yakalar: havuzdaki her tür kendi şansıyla görünür, görünenlerden biri kavanoza girer. Yakalanan ya da null.</summary>
        public string Catch(ItemInstance node, Character by = null) => CatchFrom(CatchPool(node.Def.Catches), by);

        private string CatchFrom(List<CatchDef> pool, Character by)
        {
            var seen = new List<string>();
            foreach (var c in pool) if (Rng.NextDouble() < c.Chance) seen.Add(c.Item);
            if (seen.Count == 0) { Say("Bir şey yakalanamadı."); return null; }
            string id = seen[Rng.Next(seen.Count)];
            if (by != null && IsSwimmer(id) && PlayerCatches?.Invoke(by, id) == true)
            {
                CatchBite?.Invoke(by, id);   // mini oyun: kavanoza girmesi oyuncunun elinde
                return id;
            }
            GrantCatch(id);
            return id;
        }

        /// <summary>Yakalanan tür kavanoza (envantere) girer.</summary>
        public void GrantCatch(string id)
        {
            _house.AddItem(id, 1);
            Say($"Cam kavanoza {Def(id).DisplayName.ToLowerInvariant()} yakalandı.");
            Caught?.Invoke(id);   // böcek defteri (belge 48 §9)
        }

        /// <summary>Toplama yerinde kavanoza doldurulacak zemin malzemesi (orman toprağı, kum ...): kesin gelir.</summary>
        public string FillBlocker(ItemInstance node)
        {
            if (node.Def.Gathers.Count == 0) return "Burada kavanoza doldurulacak bir şey yok.";
            if (_house.Count(CatchJar) <= 0) return "Doldurmak için envanterde boş cam kavanoz olmalı.";
            if (GathersHere(node).Count == 0)
            {
                var g = Def(node.Def.Gathers[0]);
                return $"{g?.DisplayName} yalnızca {string.Join(", ", g.ForageRegions.ConvertAll(RegionName)).ToLowerInvariant()} bölgesinde bulunur.";
            }
            return null;
        }

        private static string RegionName(string id) { foreach (var r in Regions.All) if (r.Id == id) return r.Name; return id; }

        /// <summary>Bu toplama yerinde gerçekten bulunan malzemeler (orman toprağı yalnızca ormanda).</summary>
        private List<string> GathersHere(ItemInstance node)
        {
            var list = new List<string>();
            foreach (var g in node.Def.Gathers) { var gd = Def(g); if (gd != null && ForageSystem.InRegion(gd, node.Position)) list.Add(g); }
            return list;
        }

        public void Fill(ItemInstance node)
        {
            foreach (var g in GathersHere(node))
            {
                _house.AddItem(g, 1);
                Say($"Kavanoza {Def(g)?.DisplayName.ToLowerInvariant()} dolduruldu.");
            }
        }

        // ---- su kenarında kavanoz (göl, dere, sazlık, deniz) ----

        public const string SeaWater = "sea", MarshWater = "marsh", FreshWater = "fresh";

        /// <summary>Su karosunun türü: deniz kıyısında tuzlu, sazlıkta bataklık, öteki her yerde tatlı.</summary>
        public static string WaterKindAt(GridPos p) => Regions.Coast.Contains(p) ? SeaWater : Regions.Marsh.Contains(p) ? MarshWater : FreshWater;

        private static readonly Season[] W3 = { Season.Spring, Season.Summer, Season.Autumn };
        private static CatchDef Cd(string item, float chance, params Season[] seasons) { var c = new CatchDef { Item = item, Chance = chance }; c.Seasons.AddRange(seasons); return c; }

        /// <summary>Su kenarında kavanozla yakalananlar (veri: belge 47 ek). Testler kimliklerin içerikte olduğunu denetler.</summary>
        public static readonly Dictionary<string, List<CatchDef>> WaterPools = new Dictionary<string, List<CatchDef>>
        {
            [SeaWater] = new List<CatchDef>
            {
                Cd("item.viv_seaweed", 0.45f), Cd("item.viv_sea_lettuce", 0.4f, W3), Cd("item.viv_shrimp", 0.35f, W3), Cd("item.viv_periwinkle", 0.35f),
                Cd("item.viv_hermit", 0.2f, W3), Cd("item.viv_goby", 0.2f, W3), Cd("item.viv_whelk", 0.15f),
            },
            [FreshWater] = new List<CatchDef>
            {
                Cd("item.viv_water_flea", 0.5f, W3), Cd("item.viv_minnow", 0.4f, W3), Cd("item.viv_stickleback", 0.3f, W3), Cd("item.viv_loach", 0.2f, W3),
                Cd("item.viv_tadpole", 0.45f, Season.Spring), Cd("item.viv_pond_snail", 0.35f, W3), Cd("item.viv_duckweed", 0.4f, W3),
                Cd("item.viv_hornwort", 0.35f, W3), Cd("item.viv_lily", 0.2f, Season.Summer), Cd("item.viv_water_moss", 0.3f), Cd("item.river_pebble", 0.25f),
            },
            [MarshWater] = new List<CatchDef>
            {
                Cd("item.viv_tadpole", 0.5f, Season.Spring), Cd("item.viv_frog", 0.3f, Season.Spring, Season.Summer), Cd("item.viv_water_flea", 0.5f, W3),
                Cd("item.viv_pond_snail", 0.4f, W3), Cd("item.viv_duckweed", 0.5f, W3), Cd("item.viv_hornwort", 0.3f, W3), Cd("item.viv_water_moss", 0.35f),
            },
        };

        /// <summary>Su kenarında kavanoza doldurulanlar: suyun kendisi; sazlıkta dipteki çamur da.</summary>
        public static string[] WaterFills(string kind) =>
            kind == SeaWater ? new[] { SeaWaterItem } : kind == MarshWater ? new[] { FreshWaterItem, "item.viv_marsh_mud" } : new[] { FreshWaterItem };

        public string WaterJarBlocker(GridPos tile, bool fill)
        {
            if (!_world.Map.IsWater(tile)) return "Burada su yok.";
            if (_world.Map.FrozenAt(tile)) return "Su donmuş.";
            if (_house.Count(CatchJar) <= 0) return "Envanterde boş cam kavanoz olmalı.";
            if (fill) return null;
            return CatchPool(WaterPools[WaterKindAt(tile)]).Count == 0 ? "Şu an bu suda yakalanacak bir şey yok (mevsim)." : null;
        }

        /// <summary>Su kenarında kavanozla yakalar (balıkta oyuncuysa mini oyun).</summary>
        public string CatchAtWater(Character by, GridPos tile) => CatchFrom(CatchPool(WaterPools[WaterKindAt(tile)]), by);

        /// <summary>Kavanoza su (ve sazlıkta çamur) doldurur; not döner.</summary>
        public string FillAtWater(GridPos tile)
        {
            var got = new List<string>();
            foreach (var id in WaterFills(WaterKindAt(tile))) { _house.AddItem(id, 1); got.Add(Def(id)?.DisplayName ?? id); }
            string note = "+1 " + string.Join(", +1 ", got);
            Say("Kavanoza dolduruldu: " + string.Join(", ", got).ToLowerInvariant() + ".");
            return note;
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
                list.Add(new HomeVivSave { id = kv.Key, habitat = v.Habitat, substrate = v.Substrate, moisture = v.Moisture, mold = v.Mold, entries = e.ToArray(), log = v.Log.ConvertAll(VivText.Encode).ToArray() });
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
                var v = new Vivarium { Habitat = string.IsNullOrEmpty(s.habitat) ? null : s.habitat, Substrate = string.IsNullOrEmpty(s.substrate) ? null : s.substrate, Moisture = s.moisture, Mold = s.mold };
                if (s.entries != null) foreach (var x in s.entries) { var e = VivEntry.Decode(x); if (e != null && Def(e.Item)?.Viv != null) v.Entries.Add(e); }
                if (s.log != null) foreach (var l in s.log) v.Log.Add(VivText.Decode(l));
                _vivs[s.id] = v;
            }
        }
    }
}
