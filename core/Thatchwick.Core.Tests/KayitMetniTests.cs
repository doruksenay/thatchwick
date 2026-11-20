using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Kayda anahtarlı metin (docs/yerellestirme.md: "kayıtlara çevrilmiş metin yazılmaz").</summary>
    public class KayitMetniTests
    {
        private static Localizer Loc(string lang, params (string key, string text)[] rows)
        {
            var sb = new System.Text.StringBuilder("{");
            for (int i = 0; i < rows.Length; i++)
                sb.Append(i > 0 ? "," : "").Append('"').Append(rows[i].key).Append("\":\"").Append(rows[i].text).Append('"');
            return new Localizer(lang, LocTable.Parse(sb.Append('}').ToString(), new List<string>()));
        }

        [Fact]
        public void KodlananMetin_DileGoreCozulur()
        {
            string saved = SavedText.Encode("core.chronicle.vintage_won", ("name", "Aldric"), ("akce", 40));
            Assert.True(SavedText.IsEncoded(saved));
            var tr = Loc("tr", ("core.chronicle.vintage_won", "En iyi üzüm yarışmasını {name} kazandı! +{akce} akçe."));
            var en = Loc("en", ("core.chronicle.vintage_won", "{name} won the best-grape contest! +{akce} akçe."));
            Assert.Equal("En iyi üzüm yarışmasını Aldric kazandı! +40 akçe.", SavedText.Resolve(saved, tr));
            Assert.Equal("Aldric won the best-grape contest! +40 akçe.", SavedText.Resolve(saved, en));
        }

        [Fact]
        public void IcerikVeBeceri_GosterilirkenCozulur()
        {
            string saved = SavedText.Encode("k", ("trait", SavedText.Content("trait.diligent")), ("skill", SavedText.Skill(SkillType.Farming)));
            var en = Loc("en", ("k", "{trait} / {skill}"), ("skill.Farming", "Farming"));
            Assert.Equal("Diligent / Farming", SavedText.Resolve(saved, en, id => id == "trait.diligent" ? "Diligent" : null));
        }

        [Fact]
        public void AyracVeTersBolu_AdlardaBozulmaz()
        {
            string name = "A|b=c\\p d";
            string saved = SavedText.Encode("k", ("name", name), ("n", 2.5));
            Assert.True(SavedText.TryDecode(saved, out var key, out var args));
            Assert.Equal("k", key);
            Assert.Equal(name, args[0].value);
            Assert.Equal('n', args[1].type);
            Assert.Equal("2.5", args[1].value);
        }

        /// <summary>Kodun kroniğe yazdığı her anahtar iki arayüz tablosunda da olmalı (yoksa oyuncu yalnızca argümanları görür).</summary>
        [Fact]
        public void KoddakiKronikAnahtarlari_IkiTablodaDaVar()
        {
            string loc = System.IO.Path.GetFullPath(System.IO.Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));
            string core = System.IO.Path.GetFullPath(System.IO.Path.Combine(ContentFilesTests.ContentDir(), "..", "..", "Scripts", "Core"));
            var keys = new HashSet<string>();
            foreach (var f in System.IO.Directory.GetFiles(core, "*.cs", System.IO.SearchOption.AllDirectories))
                foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(System.IO.File.ReadAllText(f), "\"(core\\.chronicle\\.[a-z_]+)\""))
                    keys.Add(m.Groups[1].Value);
            foreach (var kind in System.Enum.GetNames(typeof(Thatchwick.Core.Animals.ContestKind))) keys.Add("core.contest." + kind);
            foreach (var skill in System.Enum.GetNames(typeof(SkillType))) keys.Add("skill." + skill);
            Assert.True(keys.Count > 20);
            // Ana tablo ve alan dosyaları (ui.<alan>.<dil>.json) birlikte: yeni anahtarlar alan dosyasına yazılır.
            foreach (var lang in new[] { "en", "tr" })
            {
                var table = new LocTable();
                foreach (var path in System.IO.Directory.GetFiles(loc, "ui*." + lang + ".json"))
                    table.Merge(LocTable.Parse(System.IO.File.ReadAllText(path), new List<string>()));
                foreach (var k in keys) Assert.True(table.TryGet(k, out _), $"ui*.{lang}.json: {k} yok");
            }
        }

        [Fact]
        public void EskiKayittakiDuzMetin_OlduguGibiKalir()
        {
            Assert.Equal("Neşeli bir soydan geldi.", SavedText.Resolve("Neşeli bir soydan geldi.", Loc("en")));
            Assert.Equal("", SavedText.Resolve(null, Loc("en")));
        }

        [Fact]
        public void BilinmeyenAnahtar_AnahtariDegilArgumanlariGosterir()
        {
            string saved = SavedText.Encode("core.chronicle.gelecekte_eklenen", ("name", "Maud"));
            Assert.Equal("Maud", SavedText.Resolve(saved, Loc("en")));
        }
    }
}
