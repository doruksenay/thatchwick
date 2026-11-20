using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Mood;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>
    /// Adlandırma denetimi (Project belgesi 61b): aynı ada sahip iki eşya, oyuncuya görünen Türkçe metinde yasaklı sözcük,
    /// ad havuzları ve İç Sesler sayımı. Aynı iki denetim tools/loc/check.py'de de var.
    /// </summary>
    public class AdlandirmaTests
    {
        /// <summary>61b §8.1. Sözcük başı eşleşir ("envanterde"); tuhaflık (easter egg) satırları muaf.</summary>
        public static readonly string[] BannedWords =
        {
            "hund", "dren", "şaperon", "kupelasyon", "kalamin", "galen", "tentür", "jonglör", "goblen", "kapitone", "aplike",
            "lambri", "şindıl", "parapet", "serpantin", "füme", "sote", "tunik", "tuniğ", "ceket", "pantolon", "pileli", "şapka",
            "kapüşon", "broş", "kolye", "küvet", "şömine", "karyola", "kanopi", "paravan", "aplik", "parke", "liken", "otomat",
            "rozet", "moodlet", "xp", "bonus", "envanter", "kasaba", "şehir", "şehr", "rota", "jüri", "mastıf", "çikolata",
            "izabel", "bordo", "haki", "hijyen", "enerjisi"
        };

        /// <summary>
        /// Bilerek aynı adı taşıyanlar: toplama yerleri ürünleriyle aynı adda (61b §7.11). "Akıtma oluğu" iki ayrı oturumda
        /// iki kez eklenmiş aynı düzenek; birleştirme kararı bekliyor (61c), o yüzden şimdilik muaf.
        /// </summary>
        private static readonly HashSet<string> DuplicateAllowed = new HashSet<string>(StringComparer.Ordinal) { "Akıtma oluğu" };

        private static readonly Regex Placeholder = new Regex(@"\{[^}]*\}");
        private static readonly Regex Word = new Regex(@"[a-zçğıöşüâîû]+");

        private static string TrLower(string s) => s.Replace('I', 'ı').Replace('İ', 'i').ToLowerInvariant();

        [Fact]
        public void Esyalar_AyniAdiTasimaz()
        {
            var file = ContentFilesTests.LoadAll();
            var dups = file.items.Where(i => i != null && !i.id.StartsWith("item.forage_", StringComparison.Ordinal))
                .GroupBy(i => i.displayName, StringComparer.Ordinal)
                .Where(g => g.Count() > 1 && !DuplicateAllowed.Contains(g.Key))
                .Select(g => $"{g.Key}: {string.Join(", ", g.Select(i => i.id))}").ToList();
            Assert.True(dups.Count == 0, "Aynı görünen ada sahip eşyalar (61b §7.3):\n" + string.Join("\n", dups));
        }

        [Fact]
        public void OyuncuMetinlerinde_YasakliSozcukYok()
        {
            var texts = new List<KeyValuePair<string, string>>();
            var appearance = JsonSerializer.Deserialize<AppearanceFileDto>(
                File.ReadAllText(Path.Combine(ContentFilesTests.ContentDir(), "appearance.json")),
                new JsonSerializerOptions { IncludeFields = true });
            texts.AddRange(ContentLocalizer.Entries(ContentFilesTests.LoadAll(), appearance));
            var locDir = Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));
            foreach (var path in Directory.GetFiles(locDir, "ui*.tr.json"))
            {
                var errors = new List<string>();
                var table = LocTable.Parse(File.ReadAllText(path), errors, Path.GetFileName(path));
                foreach (var key in table.Keys)
                    if (!key.StartsWith("core.oddity.", StringComparison.Ordinal) && table.TryGet(key, out var v))
                        texts.Add(new KeyValuePair<string, string>(key, v));
            }
            var bad = new List<string>();
            foreach (var kv in texts)
            {
                if (string.IsNullOrEmpty(kv.Value)) continue;
                foreach (Match m in Word.Matches(TrLower(Placeholder.Replace(kv.Value, " "))))
                    foreach (var b in BannedWords)
                        if (m.Value.StartsWith(b, StringComparison.Ordinal)) bad.Add($"{kv.Key}: '{m.Value}' ({b}) — {kv.Value}");
            }
            Assert.True(bad.Count == 0, "Yasaklı sözcük (61b §6, §8.1; karşılığı sözlükte):\n" + string.Join("\n", bad.Take(40)));
        }

        [Fact]
        public void AdHavuzlari_YinelenmezVeSabitKadroylaCakismaz()
        {
            var db = ContentFilesTests.LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            Assert.True(db.MaleNamePool.Count >= 10 && db.FemaleNamePool.Count >= 10, "Genel ad havuzu boş ya da çok kısa.");
            var all = new List<string>(db.MaleNamePool.Concat(db.FemaleNamePool));
            var cast = db.Villagers.All.Select(v => v.Name).ToList();
            cast.Add("Selcan");   // meydanda başlayan köylü (GameRoot)
            foreach (var v in db.Villagers.All)
            {
                Assert.True(v.MaleNames.Count >= 2 && v.FemaleNames.Count >= 2, $"{v.Id}: hane ad listesi eksik.");
                all.AddRange(v.MaleNames); all.AddRange(v.FemaleNames);
            }
            var dup = all.GroupBy(n => n, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.True(dup.Count == 0, "Birden çok listede geçen adlar: " + string.Join(", ", dup));
            var clash = all.Where(cast.Contains).ToList();
            Assert.True(clash.Count == 0, "Sabit kadroyla çakışan adlar: " + string.Join(", ", clash));
        }

        [Fact]
        public void AdHavuzu_OnceHaneListesi_SonraGenelHavuz_SonraSiraEki()
        {
            var pools = new NamePools();
            pools.SetGeneral(new[] { "Tolga", "Baran" }, new[] { "Esma" });
            pools.SetHousehold("villager.orhan", new[] { "Davut", "Timur" }, new[] { "Gülsüm" });
            var taken = new HashSet<string>(StringComparer.Ordinal) { "Orhan" };

            Assert.Equal("Davut", pools.Pick(Sex.Male, "villager.orhan", taken));
            taken.Add("Davut");
            Assert.Equal("Timur", pools.Pick(Sex.Male, "villager.orhan", taken));
            taken.Add("Timur");
            Assert.Equal("Tolga", pools.Pick(Sex.Male, "villager.orhan", taken));   // hane listesi bitti: genel havuz
            Assert.Equal("Gülsüm", pools.Pick(Sex.Female, "villager.orhan", taken));
            Assert.Equal("Esma", pools.Pick(Sex.Female, null, taken));               // oyuncu hanesi: genel havuz

            taken.Add("Esma");
            Assert.Equal("Esma II", pools.Pick(Sex.Female, null, taken));            // havuz bitti: sıra eki
        }

        [Fact]
        public void Halef_KendiHanesininListesindenAdAlir()
        {
            var pop = new Population();
            var pools = new NamePools();
            pools.SetGeneral(new[] { "Tolga" }, new[] { "Esma" });
            pools.SetHousehold("villager.orhan", new[] { "Davut" }, new[] { "Gülsüm" });
            pop.NameGenerator = s => pools.Pick(s, null, new HashSet<string>());
            pop.HouseholdNameGenerator = (s, h) => pools.Pick(s, h, new HashSet<string>());
            Assert.Equal("Davut", pop.NameFor(Sex.Male, "villager.orhan"));
            Assert.Equal("Tolga", pop.NameFor(Sex.Male, null));
            Assert.Equal("Tolga", pop.NameFor(Sex.Male, "villager.yok"));            // listesi olmayan hane: genel havuz
        }

        [Fact]
        public void IcSesler_KeyifVeKasvetSayilir_SifirSayilmaz_KeyiflerOnce()
        {
            var c = new Character(1, "Davut", new GridPos(0, 0)) { MoodEnabled = true };
            c.AddMoodlet(MoodCatalog.Dusty);        // eksi
            c.AddMoodlet(MoodCatalog.VeinFound);    // artı
            var (joys, glooms) = MoodRules.InnerVoiceCounts(c.Mood.Active);
            Assert.Equal(1, joys);
            Assert.Equal(1, glooms);
            var ordered = MoodRules.InnerVoicesOrdered(c.Mood.Active);
            Assert.True(ordered[0].Def.Value > 0);
            Assert.True(ordered[ordered.Count - 1].Def.Value < 0);
            var header = MoodRules.InnerVoicesHeader(c.Mood.Active);
            Assert.Equal("ui.mood.inner_voices.header", header.Key);
        }
    }
}
