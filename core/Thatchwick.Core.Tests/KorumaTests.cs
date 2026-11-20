using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Thatchwick.Core.Localization;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Koruma testleri (üçüncü inceleme, 20.11.2026): yazılı kuralların (CLAUDE.md) sessizce delinmesini yakalar.
    //  - Koddaki sabit Türkçe metin sayısı dosya başına artamaz (yeni metin yerelleştirme anahtarıyla girer).
    //  - İçerik adlarında dönem dışı (Yeni Dünya, ortaçağ sonrası) sözcük olamaz.
    public class KorumaTests
    {
        private static string RepoRoot() =>
            Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "..", ".."));

        // ---------------- sabit metin sayacı ----------------

        /// <summary>Taban dosyası: dosya başına izin verilen en çok sabit Türkçe metin satırı.</summary>
        private const string BaselineFile = "tools/loc/sabit_metin_tabani.json";
        /// <summary>Tabanı yeniden yazdırır (yalnızca sayı azaldığında ya da bilerek artırıldığında):
        /// <c>THATCHWICK_SABIT_METIN_YAZ=1 tools/offline-tests/run.sh KorumaTests</c></summary>
        private const string WriteEnv = "THATCHWICK_SABIT_METIN_YAZ";

        private static readonly Regex TurkishLiteral = new Regex("\"[^\"\\n]*[çğıöşüÇĞİÖŞÜ][^\"\\n]*\"", RegexOptions.Compiled);
        // Oyuncuya gitmeyen iletiler sayılmaz: geliştirici günlüğü, içerik doğrulama, istisna, hata raporu.
        private static readonly string[] DeveloperOnly =
        {
            "Debug.Log", "errors.Add(", "Warnings.Add(", "warnings.Add(", "throw new", "ErrorReporter.Report(", "[Tooltip(", "[Header(", "[Obsolete("
        };

        /// <summary>Satırda oyuncuya gidebilecek, Türkçe harf içeren bir sabit metin var mı (kaba ama kararlı ölçü).</summary>
        internal static bool HasHardcodedText(string line)
        {
            string t = line.TrimStart();
            if (t.Length == 0 || t.StartsWith("//", StringComparison.Ordinal) || t.StartsWith("*", StringComparison.Ordinal) || t.StartsWith("/*", StringComparison.Ordinal)) return false;
            foreach (string skip in DeveloperOnly) if (t.IndexOf(skip, StringComparison.Ordinal) >= 0) return false;
            int comment = t.IndexOf(" // ", StringComparison.Ordinal);   // satır sonu açıklaması
            if (comment >= 0) t = t.Substring(0, comment);
            return TurkishLiteral.IsMatch(t);
        }

        private static SortedDictionary<string, int> CountHardcoded()
        {
            string root = RepoRoot();
            var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (string layer in new[] { "Assets/Scripts/Core", "Assets/Scripts/Game" })
            {
                string dir = Path.Combine(root, layer.Replace('/', Path.DirectorySeparatorChar));
                foreach (string file in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
                {
                    int n = File.ReadLines(file).Count(HasHardcodedText);
                    if (n > 0) counts[Path.GetRelativePath(root, file).Replace('\\', '/')] = n;
                }
            }
            return counts;
        }

        [Fact]
        public void SabitMetin_SatirOlcusu()
        {
            Assert.True(HasHardcodedText("    ShowMessage(\"Kaydedildi, köy uyuyor\");"));
            Assert.False(HasHardcodedText("    ShowMessage(T(\"ui.save.done\"));"));
            Assert.False(HasHardcodedText("    // \"Köylü\" eski addı"));
            Assert.False(HasHardcodedText("    Debug.LogWarning(\"İçerik hatası\");"));
            Assert.False(HasHardcodedText("    int x = 3; // \"şaka\""));
            Assert.False(HasHardcodedText("    var id = \"item.egg\";"));
        }

        [Fact]
        public void SabitMetin_DosyaBasinaArtmaz()
        {
            string path = Path.Combine(RepoRoot(), BaselineFile.Replace('/', Path.DirectorySeparatorChar));
            var now = CountHardcoded();

            if (Environment.GetEnvironmentVariable(WriteEnv) == "1")
            {
                var sb = new StringBuilder("{\n");
                int i = 0;
                foreach (var kv in now) sb.Append("  ").Append(JsonSerializer.Serialize(kv.Key)).Append(": ").Append(kv.Value).Append(++i < now.Count ? ",\n" : "\n");
                File.WriteAllText(path, sb.Append("}\n").ToString());
                return;
            }

            Assert.True(File.Exists(path), $"{BaselineFile} yok. Üretmek için: {WriteEnv}=1 tools/offline-tests/run.sh KorumaTests");
            var baseline = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(path));
            var grew = new List<string>();
            foreach (var kv in now)
            {
                baseline.TryGetValue(kv.Key, out int allowed);
                if (kv.Value > allowed) grew.Add($"  {kv.Key}: taban {allowed}, şimdi {kv.Value}");
            }
            Assert.True(grew.Count == 0,
                "Koda yeni sabit Türkçe metin eklenmiş:\n" + string.Join("\n", grew) +
                "\nOyuncuya giden metin anahtarla yazılır: Game'de GameLocalization.T(\"ui...\"), Core'da LocText.Of(...) " +
                "(CLAUDE.md > Dil ve yerelleştirme; yeni anahtar ui.<alan>.en/tr.json dosyasına)." +
                $"\nMetin oyuncuya gitmiyorsa ya da artış bilerek yapıldıysa tabanı yenile: {WriteEnv}=1 tools/offline-tests/run.sh KorumaTests");
        }

        // ---------------- dönem kuralı ----------------

        // CLAUDE.md > Kod > dönem kuralları; ayrıntı Project belgesi 20. Kahve ve siyah çay bilinçli istisnadır.
        private static readonly Regex Anachronism = new Regex(
            @"\b(potato(es)?|tomato(es)?|maize|sweetcorn|pumpkins?|tobacco|turkeys?|chill?i(es)?|cocoa|chocolate|vanilla|sunflowers?|peanuts?|" +
            @"kerosene|hay bales?|bales?|seed drills?|printed books?|refrigerators?|fridges?|pineapples?|avocados?|rubber)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>Kararı bekleyen ya da bilerek kalan adlar (anahtar → neden).</summary>
        private static readonly Dictionary<string, string> PeriodExceptions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["item.seeder.name"] = "ekim makinesinin dönem karşılığı karar bekliyor (Project belgesi 20)",
            // Domates: salça için bilinçli dönem istisnası (kullanıcı kararı 20.11.2026, Project belgesi 60a-E1); yalnızca salça ve salçalı yemekler.
            ["item.cabbage_paste.name"] = Tomato,
            ["item.crop_tomato.name"] = Tomato,
            ["item.paste_pilaf.name"] = Tomato,
            ["item.paste_stew.name"] = Tomato,
            ["item.seed_tomato.name"] = Tomato,
            ["item.tomato.name"] = Tomato,
            ["item.tomato_paste.name"] = Tomato,
            ["item.tomato_puree.name"] = Tomato,
            ["recipe.cook_cabbage_paste.name"] = Tomato,
            ["recipe.cook_paste_pilaf.name"] = Tomato,
            ["recipe.cook_paste_stew.name"] = Tomato,
            ["recipe.seed_tomato.name"] = Tomato,
            ["recipe.tomato_puree.name"] = Tomato,
        };
        private const string Tomato = "domates, salça için bilinçli dönem istisnası (Project belgesi 60a-E1)";

        [Fact]
        public void Donem_IcerikAdlarindaYasakSozcukYok()
        {
            string locDir = Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));
            var errors = new List<string>();
            var table = LocTable.Parse(File.ReadAllText(Path.Combine(locDir, "content.en.json")), errors, "content.en.json");
            Assert.True(errors.Count == 0, string.Join("\n", errors));

            var bad = new List<string>();
            int names = 0;
            foreach (string key in table.Keys)
            {
                if (!key.EndsWith(".name", StringComparison.Ordinal) || !table.TryGet(key, out string text)) continue;
                names++;
                var m = Anachronism.Match(text ?? "");
                if (m.Success && !PeriodExceptions.ContainsKey(key)) bad.Add($"  {key} = \"{text}\" ({m.Value})");
            }
            Assert.True(names > 500, $"İçerik adı sayısı beklenenden az: {names}");
            Assert.True(bad.Count == 0,
                "Dönem dışı içerik adı (Yeni Dünya bitkisi/hayvanı ya da ortaçağ sonrası icat):\n" + string.Join("\n", bad) +
                "\nKural: CLAUDE.md > Kod. Bilinçli istisna ise KorumaTests.PeriodExceptions'a nedeniyle ekle.");
        }
    }
}
