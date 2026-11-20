using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// +18 paketi (yalnızca Steam; ContentPacks.Adult) açıkken easter egg satırlarının küfürlü çeşidi: anahtarın sonuna
    /// ".adult" eklenir. Çeşidi olmayan satır olduğu gibi kalır. Yeni çeşit eklemek için yalnızca ui tablolarına
    /// "&lt;anahtar&gt;.adult" satırı yazmak yeter.
    /// </summary>
    public static class OddityLines
    {
        public const string AdultSuffix = ".adult";

        public static Thatchwick.Core.Localization.LocText ForPack(Thatchwick.Core.Localization.LocText text, bool adult, Func<string, bool> hasKey)
        {
            if (!adult || text.Key == null || hasKey == null || !hasKey(text.Key + AdultSuffix)) return text;
            return Thatchwick.Core.Localization.LocText.Of(text.Key + AdultSuffix, text.Args);
        }

        public const string SaySuffix = ".say";

        /// <summary>
        /// Satırın konuşma balonunda görünecek kısmı (kullanıcı 20.11.2026: tepkiler bildirim değil balon). Önce tablodaki
        /// "&lt;anahtar&gt;.say" satırı; yoksa çözülmüş satırın ilk tırnak içi sözü. Söz yoksa null: satır bildirim olarak kalır.
        /// </summary>
        public static string BubbleText(Thatchwick.Core.Localization.LocText line, string resolved, Func<string, bool> hasKey, Func<Thatchwick.Core.Localization.LocText, string> resolve)
        {
            if (line.Key != null && hasKey != null && resolve != null && hasKey(line.Key + SaySuffix))
                return resolve(Thatchwick.Core.Localization.LocText.Of(line.Key + SaySuffix, line.Args));
            return Quote(resolved);
        }

        /// <summary>Metindeki ilk tırnak içi söz (düz ya da kıvrık tırnak); yoksa null.</summary>
        public static string Quote(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            int a = text.IndexOfAny(new[] { '"', '\u201C' });
            if (a < 0) return null;
            int b = text.IndexOfAny(new[] { '"', '\u201D' }, a + 1);
            if (b <= a + 1) return null;
            return text.Substring(a + 1, b - a - 1).Trim();
        }

        /// <summary>Bugün +18 çeşidi yazılmış satırlar (testler tabloda olduklarını denetler).</summary>
        public static readonly string[] AdultKeys =
        {
            "core.oddity.ev.top_show.v0", "core.oddity.ev.top_show.v1", "core.oddity.ev.top_show.v2", "core.oddity.ev.top_show.v3",
            "core.oddity.tell.disbelief", "core.oddity.tell.disbelief_nickname", "core.oddity.ev.nickname_talk",
            "core.oddity.ev.chicken_revenge", "core.oddity.ev.johan_wick.field", "core.oddity.ev.johan_wick.nofield",
            "core.oddity.ev.the_usual", "core.oddity.ev.the_usual.heir",
            "core.oddity.ev.crier.v0", "core.oddity.ev.crier.v1", "core.oddity.ev.crier.v2"
        };
    }

    /// <summary>
    /// Küçük easter egg'lerin ortak durumu (Project belgesi 55 §1, §2, §2b): şans zarları, günlük ve kalıcı sayaçlar,
    /// bayraklar. Kurallar burada, dünyaya etkileri LifeSystems.OddityEvents.cs'te. Her olayın bir kimliği vardır;
    /// <see cref="Force"/> ile bir sonraki zarı kesin tutturulur (test ve hata ayıklama).
    /// </summary>
    public sealed class OddityEvents
    {
        // Olay kimlikleri (zorlama anahtarı; çoğu aynı adlı tuhaflığı işaretler).
        public const string JohanWick = "johan_wick", HorselessKnight = "horseless_knight", Crier = "crier", ScreamingGoat = "screaming_goat",
            CatGift = "cat_gift", AlexDecor = "alex_decor", HoneyThief = "honey_thief", ServantCaught = "servant_caught",
            AprilFirst = "april_first", Rumor = "rumor", NameTwin = "name_twin", NameSurprise = "name_surprise",
            NicknameTalk = "nickname_talk", AbsentElder = "absent_elder", SameOutfit = "same_outfit", VillageMemory = "village_memory",
            OldBoot = "old_boot", MessageBottle = "message_bottle", FishTale = "fish_tale", MineTablet = "mine_tablet",
            SwordInStone = "sword_in_stone", TopShow = "top_show", NeverStoppingTop = "never_stopping_top", OxStare = "ox_stare",
            BoxCat = "box_cat", JealousDog = "jealous_dog", DogGift = "dog_gift", Howl = "howl", CountingSheep = "counting_sheep",
            TheUsual = "the_usual", Regift = "regift", Scarecrow = "scarecrow", ChickenRevenge = "chicken_revenge",
            CrowGift = "crow_gift", OddBequest = "odd_bequest";

        // Şanslar (belge 55: sayılar denemeyle ayarlanır).
        public const double JohanChance = 1.0 / 120, JohanDaisyFactor = 4, KnightChance = 1.0 / 150, CrierChance = 1.0 / 60,
            GoatChance = 1.0 / 50, CatGiftChance = 1.0 / 30, AlexChance = 1.0 / 25, HoneyChance = 1.0 / 80, ServantChance = 1.0 / 40,
            RumorChance = 1.0 / 6, NicknameTalkChance = 1.0 / 3, ElderChance = 1.0 / 40, BootChance = 1.0 / 40, BottleChance = 1.0 / 150,
            FishTaleChance = 1.0 / 3, TabletChance = 1.0 / 300, StoneSwordChance = 1.0 / 300, TopShowChance = 1.0 / 3,
            NeverStoppingChance = 1.0 / 100, OxChance = 1.0 / 3, BoxCatChance = 1.0 / 2, JealousChance = 1.0 / 3, DogRelicChance = 0.1,
            OddBequestChance = 0.1;

        public const int UsualOrders = 10, DogGiftPets = 1000, ShooLimit = 3, FishTalePrice = 25;
        /// <summary>Dolunay: her 14. gece (mevsim uzunluğundan bağımsız, gün sayacına göre).</summary>
        public const int MoonCycle = 14;

        private readonly Dictionary<string, double> _vals = new Dictionary<string, double>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _texts = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly HashSet<string> _forced = new HashSet<string>(StringComparer.Ordinal);
        private readonly Random _rng;

        public OddityBook Book { get; set; }
        /// <summary>Zar (0..1); testte sabitlenebilir.</summary>
        public Func<double> Roll { get; set; }

        public OddityEvents(int seed = 555) { _rng = new Random(seed); Roll = () => _rng.NextDouble(); }

        public void Force(string eventId) { if (!string.IsNullOrEmpty(eventId)) _forced.Add(eventId); }
        public bool IsForced(string eventId) => _forced.Contains(eventId);

        /// <summary>Olay bu sefer tuttu mu (zorlanmışsa kesin).</summary>
        public bool Chance(string eventId, double p)
        {
            if (_forced.Remove(eventId)) return true;
            return Roll() < p;
        }

        public int Pick(int n) => n <= 1 ? 0 : _rng.Next(n);

        // ---- kalıcı sayaç ve bayraklar ----
        public double Get(string key) => _vals.TryGetValue(key, out var v) ? v : 0;
        public void Set(string key, double value) => _vals[key] = value;
        public bool Has(string key) => _vals.ContainsKey(key);
        public double Add(string key, double delta = 1) { double v = Get(key) + delta; _vals[key] = v; return v; }
        public string Text(string key) => _texts.TryGetValue(key, out var v) ? v : null;
        public void SetText(string key, string value) { if (value == null) _texts.Remove(key); else _texts[key] = value; }
        public void Clear(string key) { _vals.Remove(key); _texts.Remove(key); }

        /// <summary>Bugün ilk kez mi (günde bir kez olan olaylar).</summary>
        public bool FirstToday(string key, long day)
        {
            string k = "day:" + key;
            if (Has(k) && (long)Get(k) == day) return false;
            Set(k, day);
            return true;
        }

        /// <summary>Günlük sayaç: bugünkü yeni değeri döner (gün değişince sıfırdan).</summary>
        public int CountToday(string key, long day)
        {
            string kd = "cday:" + key, kc = "cnt:" + key;
            if (!Has(kd) || (long)Get(kd) != day) { Set(kd, day); Set(kc, 0); }
            return (int)Add(kc);
        }

        public static bool IsFullMoon(long dayIndex) => dayIndex % MoonCycle == MoonCycle - 1;
        public static bool IsNight(double dayFraction) => dayFraction >= 0.75 || dayFraction < 0.2;

        // ---- "her zamankinden mi?" (meyhane) ----

        /// <summary>
        /// Meyhane siparişi. Aynı şey 10. kez ısmarlanınca "her zamanki" olur (1 döner); sonraki kuşaktan biri aynısını
        /// ısmarlayınca "dedeninkinden mi?" (2 döner). Diğer durumda 0.
        /// </summary>
        public int OnTavernOrder(string itemId, string buyerName, int generation, long day)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;
            string usual = Text("usual.item");
            if (usual == null)
            {
                if (Add("tavern:" + itemId) < UsualOrders && !_forced.Remove(TheUsual)) return 0;
                SetText("usual.item", itemId); SetText("usual.name", buyerName); Set("usual.gen", generation);
                Book?.Mark(OddityIds.TheUsual);
                return 1;
            }
            if (usual != itemId || generation <= (int)Get("usual.gen")) return 0;
            if (!FirstToday("usual.heir", day)) return 0;
            return 2;
        }

        public string UsualName => Text("usual.name");

        // ---- köyün hafızası (attan düşme) ----

        /// <summary>Düşüşü yazar; daha önceki bir kuşaktan düşen varsa onun adını döner ("Deden de düşmüştü").</summary>
        public string OnFall(string name, int generation)
        {
            string first = Text("fall.name");
            if (first == null) { SetText("fall.name", name); Set("fall.gen", generation); return null; }
            if (generation <= (int)Get("fall.gen")) return null;
            Book?.Mark(OddityIds.VillageMemory);
            return first;
        }

        // ---- kulaktan kulağa ----

        /// <summary>Bugün yapılan dikkat çekici iş (balık, odun, tarla, maden); söylenti kaynağı.</summary>
        public void NoteWork(int characterId, string kind, long day)
        {
            Set("work.day:" + characterId, day);
            SetText("work.kind:" + characterId, kind);
        }

        /// <summary>Köylüyle sohbet: bugün iş yaptıysa ve şans tutarsa yarına söylenti düşer.</summary>
        public bool OnGossip(int characterId, string name, long day)
        {
            if (Text("rumor.kind") != null) return false;
            if (!Has("work.day:" + characterId) || (long)Get("work.day:" + characterId) != day) return false;
            if (!Chance(Rumor, RumorChance)) return false;
            SetText("rumor.kind", Text("work.kind:" + characterId)); SetText("rumor.name", name); Set("rumor.day", day);
            return true;
        }

        /// <summary>Ertesi gün dönen söylenti: (kim, iş türü) ya da null.</summary>
        public (string name, string kind)? TakeRumor(long day)
        {
            string kind = Text("rumor.kind");
            if (kind == null || day <= (long)Get("rumor.day")) return null;
            string name = Text("rumor.name");
            Clear("rumor.kind"); Clear("rumor.name");
            Book?.Mark(OddityIds.Rumor);
            return (name, kind);
        }

        // ---- aynı kıyafet ----

        public void OnOutfitChanged(int characterId, double today) => Set("outfit:" + characterId, today);

        /// <summary>Bir mevsimdir üstünü değiştirmediyse true (mevsimde bir kez söylenir).</summary>
        public bool SameOutfitTooLong(int characterId, double today, int daysPerSeason)
        {
            string k = "outfit:" + characterId;
            if (!Has(k)) { Set(k, today); return false; }
            if (_forced.Remove(SameOutfit)) { Set(k, today); Book?.Mark(OddityIds.SameOutfit); return true; }
            if (today - Get(k) < daysPerSeason) return false;
            Set(k, today);   // söylendi; bir mevsim sonra yeniden
            Book?.Mark(OddityIds.SameOutfit);
            return true;
        }

        // ---- kayıt ----

        public string[] Export()
        {
            var list = new List<string>();
            foreach (var kv in _vals.OrderBy(k => k.Key, StringComparer.Ordinal))
                list.Add("n|" + kv.Key + "|" + kv.Value.ToString("R", CultureInfo.InvariantCulture));
            foreach (var kv in _texts.OrderBy(k => k.Key, StringComparer.Ordinal))
                list.Add("t|" + kv.Key + "|" + kv.Value);
            return list.ToArray();
        }

        public void Import(string[] data)
        {
            _vals.Clear(); _texts.Clear(); _forced.Clear();
            if (data == null) return;
            foreach (var line in data)
            {
                if (string.IsNullOrEmpty(line) || line.Length < 4) continue;
                int sep = line.IndexOf('|', 2);
                if (sep < 0) continue;
                string key = line.Substring(2, sep - 2), val = line.Substring(sep + 1);
                if (line[0] == 'n') { if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) _vals[key] = d; }
                else if (line[0] == 't') _texts[key] = val;
            }
        }
    }
}
