using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Thatchwick.Core.Content;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Tests
{
    // Yerelleştirme: dil seçimi, tablo okuma, yer tutucu, çoğul, Türkçe büyük/küçük harf ve
    // gerçek içerikteki her metnin İngilizce (kaynak dil) tablosunda karşılığı olması.
    public class LocalizationTests
    {
        private static LocTable Table(params (string key, string value)[] entries)
        {
            var t = new LocTable();
            foreach (var (k, v) in entries) t.Set(k, v);
            return t;
        }

        private static string LocDir() =>
            Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));

        private static LocTable LoadTable(string fileName)
        {
            var errors = new List<string>();
            var path = Path.Combine(LocDir(), fileName);
            var table = LocTable.Parse(File.ReadAllText(path), errors, fileName);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return table;
        }

        // ---- Dil seçimi ----

        [Fact]
        public void Dil_KayitliSecimOnce_SonraCihaz_SonraIngilizce()
        {
            Assert.Equal("tr", Languages.Pick("tr", "en"));
            Assert.Equal("en", Languages.Pick(null, "en-GB"));
            Assert.Equal("tr", Languages.Pick(null, "tr-TR"));
            Assert.Equal("en", Languages.Pick(null, "ja"));
            // Henüz yayında olmayan dil seçilemez (tablosu yok): kaynak dile düşer.
            Assert.Equal("en", Languages.Pick("de", "de-DE"));
            Assert.Equal("tr", Languages.Pick("xx", "TR"));
        }

        [Fact]
        public void Dil_Normalize_TurkceKulturdeDeDogru()
        {
            var old = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
                // "I" Türkçe kültürde "ı" olurdu; Normalize kültürden bağımsız çalışmalı.
                Assert.Equal("it", Languages.Normalize("IT"));
                Assert.Equal("tr", Languages.Normalize(" TR_tr "));
            }
            finally { System.Globalization.CultureInfo.CurrentCulture = old; }
            Assert.Equal(5, Languages.All.Length);
            Assert.Contains("en", Languages.Released);
            Assert.Contains("tr", Languages.Released);
        }

        // ---- Tablo okuma ----

        [Fact]
        public void Tablo_KacislarVeTurkceHarflerOkunur()
        {
            var errors = new List<string>();
            var t = LocTable.Parse("﻿{\n \"a\": \"Çiğ \\\"süt\\\"\\n\\u0130\",\n\"b\":\"\"}", errors);
            Assert.Empty(errors);
            Assert.True(t.TryGet("a", out var a));
            Assert.Equal("Çiğ \"süt\"\nİ", a);
            Assert.True(t.TryGet("b", out var b));
            Assert.Equal("", b);
        }

        [Fact]
        public void Tablo_HataliDosya_HataVerirOkunanlarKalir()
        {
            var errors = new List<string>();
            var t = LocTable.Parse("{\"a\": \"1\", \"b\": 2}", errors, "x.json");
            Assert.Single(errors);
            Assert.Contains("x.json", errors[0]);
            Assert.True(t.Contains("a"));
            Assert.False(t.Contains("b"));

            errors.Clear();
            LocTable.Parse("{\"a\": \"1\", \"a\": \"2\"}", errors);
            Assert.Single(errors); // tekrar eden anahtar uyarısı
        }

        // ---- Localizer ----

        [Fact]
        public void Localizer_SeciliDil_SonraKaynakDil_SonraAnahtar()
        {
            var en = Table(("hello", "Hello"), ("only.en", "English only"));
            var tr = Table(("hello", "Merhaba"));
            var loc = new Localizer("tr", tr, en);

            Assert.Equal("Merhaba", loc.Get("hello"));
            Assert.Equal("English only", loc.Get("only.en"));
            Assert.Equal("missing.key", loc.Get("missing.key"));
            Assert.Contains("missing.key", loc.MissingKeys);
            Assert.False(loc.TryGetExact("only.en", out _));
            Assert.Equal("yedek", loc.GetOr("yok", "yedek"));
        }

        [Fact]
        public void Localizer_AdliYerTutucu_SiraDilegoreDegisebilir()
        {
            var en = Table(("bought", "{name} bought {item}."));
            var tr = Table(("bought", "{name}, {item} satın aldı."), ("egg", "Yumurta"));
            var loc = new Localizer("tr", tr, en);

            Assert.Equal("Edith, Yumurta satın aldı.", loc.Format("bought", ("name", "Edith"), ("item", LocText.Of("egg"))));
            // Eksik argüman görünür kalır; süslü parantez kaçışı çalışır.
            Assert.Equal("{name}, {item} satın aldı.", loc.Format("bought"));
            Assert.Equal("{x} = 1", loc.Fill("{{x}} = {v}", ("v", 1)));
        }

        [Fact]
        public void Localizer_SayilarSeciliDilinKulturuyle()
        {
            var tr = new Localizer("tr", new LocTable());
            var en = new Localizer("en", new LocTable());
            Assert.Equal("1,5", tr.Number(1.5, 1));
            Assert.Equal("1.5", en.Number(1.5, 1));
            Assert.Equal("Fiyat 2,25", tr.Fill("Fiyat {p}", ("p", 2.25)));
        }

        [Fact]
        public void Localizer_Cogul()
        {
            var en = Table(("eggs.one", "{count} egg"), ("eggs.other", "{count} eggs"));
            var tr = Table(("eggs.other", "{count} yumurta"));
            var fr = Table(("eggs.one", "{count} œuf"), ("eggs.other", "{count} œufs"));

            var le = new Localizer("en", en);
            Assert.Equal("1 egg", le.Plural("eggs", 1));
            Assert.Equal("3 eggs", le.Plural("eggs", 3));
            Assert.Equal("0 eggs", le.Plural("eggs", 0));

            var lt = new Localizer("tr", tr, en);
            Assert.Equal("1 yumurta", lt.Plural("eggs", 1)); // TR'de .one yok: .other kullanılır, İngilizceye düşmez
            Assert.Equal("3 yumurta", lt.Plural("eggs", 3));

            var lf = new Localizer("fr", fr, en);
            Assert.Equal("0 œuf", lf.Plural("eggs", 0)); // Fransızcada 0 tekil
            Assert.Equal("2 œufs", lf.Plural("eggs", 2));

            Assert.Equal("3 eggs", le.Resolve(LocText.Counted("eggs", 3)));
            Assert.Equal("düz", le.Resolve(LocText.Plain("düz")));
        }

        [Fact]
        public void Localizer_TurkceBuyukKucukHarf()
        {
            var tr = new Localizer("tr", new LocTable());
            var en = new Localizer("en", new LocTable());
            Assert.Equal("İNEK", tr.ToUpper("inek"));
            Assert.Equal("INEK", en.ToUpper("inek"));
            Assert.Equal("ılık", tr.ToLower("ILIK"));
        }

        // ---- İçerik ----

        [Fact]
        public void Icerik_TurkceSecilinceJsonDegismez_IngilizceSecilinceCevrilir()
        {
            var en = Table(("item.egg.name", "Egg"), ("item.egg.act.eat", "Eat an egg"), ("trait.x.desc", "Works faster."));
            var file = new ContentFileDto
            {
                items = new[] { new ItemDto { id = "item.egg", displayName = "Yumurta", interactions = new[] { new InteractionDto { id = "eat", displayName = "Ye" } } } },
                traits = new[] { new TraitDto { id = "trait.x", displayName = "Hızlı", description = "Hızlı çalışır." } },
                recipes = new[] { new RecipeDto { id = "r", displayName = "" } },
            };

            ContentLocalizer.Apply(file, new Localizer("tr", new LocTable(), en));
            Assert.Equal("Yumurta", file.items[0].displayName);

            ContentLocalizer.Apply(file, new Localizer("en", en));
            Assert.Equal("Egg", file.items[0].displayName);
            Assert.Equal("Eat an egg", file.items[0].interactions[0].displayName);
            Assert.Equal("Hızlı", file.traits[0].displayName); // çevirisi yok: JSON metni kalır
            Assert.Equal("Works faster.", file.traits[0].description);
            Assert.Equal("", file.recipes[0].displayName);     // boş ad boş kalır (koddan üretilir)
        }

        [Fact]
        public void Icerik_IspanyolcaEksikse_IngilizceyeDuser()
        {
            var en = Table(("item.egg.name", "Egg"));
            var es = Table();
            var file = new ContentFileDto { items = new[] { new ItemDto { id = "item.egg", displayName = "Yumurta" } } };
            ContentLocalizer.Apply(file, new Localizer("es", es, en));
            Assert.Equal("Egg", file.items[0].displayName);
        }

        [Fact]
        public void GercekIcerik_HerMetninIngilizcesiVar()
        {
            var en = LoadTable("content.en.json");
            var appearance = JsonSerializer.Deserialize<AppearanceFileDto>(
                File.ReadAllText(Path.Combine(ContentFilesTests.ContentDir(), "appearance.json")),
                new JsonSerializerOptions { IncludeFields = true });
            var entries = ContentLocalizer.Entries(ContentFilesTests.LoadAll(), appearance);
            Assert.True(entries.Count > 1000, $"İçerik metni sayısı beklenenden az: {entries.Count}");

            var missing = entries.Where(e => !en.TryGet(e.Key, out var v) || string.IsNullOrWhiteSpace(v) || v.StartsWith("TODO:", StringComparison.Ordinal))
                .Select(e => $"{e.Key}: {e.Value}").ToList();
            Assert.True(missing.Count == 0,
                "İngilizce karşılığı olmayan içerik metinleri (Assets/Resources/Localization/content.en.json'a ekle; " +
                "liste için: python tools/loc/check.py):\n" + string.Join("\n", missing.Take(40)));
        }

        [Fact]
        public void GercekIcerik_CeviridekiSayilarTutar()
        {
            var en = LoadTable("content.en.json");
            var digits = new Regex(@"\d+");
            var bad = new List<string>();
            foreach (var e in ContentLocalizer.Entries(ContentFilesTests.LoadAll()))
            {
                if (!en.TryGet(e.Key, out var text)) continue;
                var a = digits.Matches(e.Value).Select(m => m.Value).OrderBy(x => x, StringComparer.Ordinal);
                var b = digits.Matches(text).Select(m => m.Value).OrderBy(x => x, StringComparer.Ordinal);
                if (!a.SequenceEqual(b)) bad.Add($"{e.Key}: '{e.Value}' <> '{text}'");
            }
            Assert.True(bad.Count == 0, string.Join("\n", bad));
        }

        [Fact]
        public void ArayuzTablolari_AyniAnahtarlar_AyniYerTutucular()
        {
            var en = LoadTable("ui.en.json");
            var tr = LoadTable("ui.tr.json");
            var holes = new Regex(@"(?<!\{)\{([A-Za-z0-9_]+)\}");
            Assert.Empty(en.Keys.Except(tr.Keys));
            Assert.Empty(tr.Keys.Except(en.Keys));
            foreach (var key in en.Keys)
            {
                en.TryGet(key, out var e);
                tr.TryGet(key, out var t);
                var a = holes.Matches(e).Select(m => m.Groups[1].Value).OrderBy(x => x, StringComparer.Ordinal);
                var b = holes.Matches(t).Select(m => m.Groups[1].Value).OrderBy(x => x, StringComparer.Ordinal);
                Assert.True(a.SequenceEqual(b), $"{key}: yer tutucular farklı");
            }
        }
    }
}
