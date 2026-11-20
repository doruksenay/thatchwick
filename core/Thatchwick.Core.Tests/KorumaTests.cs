using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Thatchwick.Core.Localization;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Koruma testleri (üçüncü inceleme, 20.11.2026): yazılı kuralların (CLAUDE.md) sessizce delinmesini yakalar.
    //  - İçerik adlarında dönem dışı (Yeni Dünya, ortaçağ sonrası) sözcük olamaz.
    // Yeni sabit Türkçe metin denetimi artık burada değil: PR'ın kendi farkına bakan tools/loc/sabit_metin.py'de
    // (dosya başına sayaç, kırmızı bir PR birleşince main'i ve sonraki bütün PR'ları kırıyordu).
    public class KorumaTests
    {
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
            // Aynı tariflerin büyük şöminedeki ikizleri (belge 63a: kazan tarifleri taş ocakta ve şöminede).
            ["recipe.cook_cabbage_paste__fire.name"] = Tomato,
            ["recipe.cook_paste_pilaf__fire.name"] = Tomato,
            ["recipe.cook_paste_stew__fire.name"] = Tomato,
            ["recipe.tomato_puree__fire.name"] = Tomato,
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
