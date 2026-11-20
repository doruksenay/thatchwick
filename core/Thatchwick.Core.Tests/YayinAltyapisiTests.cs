using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Diagnostics;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Progress;
using Xunit;

namespace Thatchwick.Core.Tests
{
    // Yayın altyapısı (20.11.2026): hata günlüğü, yarım kalan oturum, başarımlar, görüntü seçenekleri.
    public class YayinAltyapisiTests
    {
        private static string TempFile(string name)
        {
            string dir = Path.Combine(Path.GetTempPath(), "thatchwick-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, name);
        }

        // ---------------- hata günlüğü ----------------

        [Fact]
        public void HataGunlugu_AyniHatayiTekillestirir_IlkUcunuTamYazar()
        {
            string path = TempFile("errors.log");
            var journal = new ErrorJournal(path, () => "Thatchwick test");
            Assert.True(journal.Report("sim:heat", "NullReferenceException: x", "at A.B()\nat C.D()"));
            for (int i = 0; i < 9; i++) Assert.False(journal.Report("sim:heat", "NullReferenceException: x", "at A.B()\nat C.D()"));

            Assert.Equal(10, journal.Total);
            Assert.Equal(1, journal.Distinct);
            string text = File.ReadAllText(path);
            Assert.Contains("Thatchwick test", text);
            Assert.Equal(ErrorJournal.FullReports, text.Split(new[] { "[sim:heat] #" }, StringSplitOptions.None).Length - 1);
        }

        [Fact]
        public void HataGunlugu_SayilariFarkliAyniHata_TekSayilir_FarkliKaynakAyri()
        {
            var journal = new ErrorJournal(TempFile("errors.log"));
            Assert.True(journal.Report("unity", "Karo 12,7 bulunamadı", "at X.Y()"));
            Assert.False(journal.Report("unity", "Karo 3,905 bulunamadı", "at X.Y()"));
            Assert.True(journal.Report("save", "Karo 12,7 bulunamadı", "at X.Y()"));
            Assert.True(journal.Report("unity", "Karo 12,7 bulunamadı", "at Z.W()"));
            Assert.Equal(3, journal.Distinct);
            Assert.Equal(ErrorJournal.Signature("a", "x 1", null), ErrorJournal.Signature("a", "x 999", null));
        }

        [Fact]
        public void HataGunlugu_YeniHatalariBirKezBildirir()
        {
            var journal = new ErrorJournal(TempFile("errors.log"));
            Assert.Equal(0, journal.TakeUnseen());
            journal.Report("a", "bir", null);
            journal.Report("a", "bir", null);
            journal.Report("a", "iki", null);
            Assert.Equal(2, journal.TakeUnseen());
            Assert.Equal(0, journal.TakeUnseen());
            Assert.Contains("iki", journal.LastSummary);
        }

        [Fact]
        public void HataGunlugu_BuyuyenDosyayiDondurur_FirtinadaSismez()
        {
            string path = TempFile("errors.log");
            var journal = new ErrorJournal(path, null, maxBytes: 4096);
            for (int i = 0; i < 400; i++) journal.Report("k" + (char)('a' + i % 26) + (char)('a' + i / 26), new string('x', 200), null);
            Assert.True(File.Exists(path + ".1"), "eski günlük .1 olarak saklanmalı");
            Assert.True(new FileInfo(path).Length < 4096 + 1024, "günlük tavanını aşmamalı");
            Assert.Equal(ErrorJournal.MaxDistinct, journal.Distinct);   // tavandan sonrası sayılmaz, yazılmaz
            Assert.Equal(400, journal.Total);
        }

        [Fact]
        public void HataGunlugu_YazilamayanYoldaOyunuBozmaz_RaporUretir()
        {
            var journal = new ErrorJournal(Path.Combine(Path.GetTempPath(), "yok-" + Guid.NewGuid().ToString("N"), "alt", "errors.log"), () => "başlık");
            Assert.True(journal.Report("a", new InvalidOperationException("olmadı")));
            Assert.Equal(1, journal.Total);
            string report = journal.BuildReport();
            Assert.Contains("başlık", report);
            Assert.Contains("1 hata", report);
        }

        [Fact]
        public void Oturum_DuzgunKapanmayanOturumuBirSonrakiAcilistaBildirir()
        {
            string path = TempFile("session.lock");
            Assert.False(new SessionMarker(path).Begin("v1"));       // ilk açılış
            var second = new SessionMarker(path);
            Assert.True(second.Begin("v2"));                         // öncekisi kapanmadan yeniden açıldı: çökme
            Assert.Equal("v1", second.PreviousNote);
            second.End();                                            // düzgün kapanış
            Assert.False(new SessionMarker(path).Begin("v3"));
        }

        // ---------------- başarımlar ----------------

        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        private static AchievementSnapshot Snapshot(Dictionary<string, int> counters = null, int level = 1, int generation = 1, int years = 0, int heirlooms = 0) =>
            new AchievementSnapshot
            {
                Counter = key => counters != null && counters.TryGetValue(key, out int n) ? n : 0,
                Level = level, Generation = generation, Years = years, Heirlooms = heirlooms
            };

        [Fact]
        public void Basarim_EsigeUlasincaBirKezAcilir_OlayVerir()
        {
            var service = new AchievementService(AchievementCatalog.Default);
            var opened = new List<string>();
            service.Unlocked += d => opened.Add(d.Id);

            Assert.Empty(service.Evaluate(Snapshot()));
            var fresh = service.Evaluate(Snapshot(new Dictionary<string, int> { ["act:harvest"] = 1 }));
            Assert.Equal(new[] { "ach.first_harvest" }, fresh.Select(d => d.Id).ToArray());
            Assert.Empty(service.Evaluate(Snapshot(new Dictionary<string, int> { ["act:harvest"] = 5 })));   // yeniden açılmaz
            Assert.Equal(new List<string> { "ach.first_harvest" }, opened);
            Assert.True(service.IsUnlocked("ach.first_harvest"));
            Assert.Equal(5, service.Best("ach.harvest_100"));
            Assert.Equal(0.05, service.Progress(service.Find("ach.harvest_100")), 6);
        }

        [Fact]
        public void Basarim_IlerlemeDusmez_ProfilYuvadanBagimsizSaklanir()
        {
            var service = new AchievementService(AchievementCatalog.Default);
            service.Evaluate(Snapshot(new Dictionary<string, int> { ["act:harvest"] = 60 }, generation: 3, years: 4));
            // Yeni oyuna başlandı: sayaçlar sıfırdan; ilerleme ve açılanlar kalır.
            service.Evaluate(Snapshot(new Dictionary<string, int> { ["act:harvest"] = 2 }));
            Assert.Equal(60, service.Best("ach.harvest_100"));
            Assert.True(service.IsUnlocked("ach.generation_3"));
            Assert.True(service.IsUnlocked("ach.first_heir"));
            Assert.True(service.Dirty);

            // Dosyaya yazılıp okununca aynı durum (JsonUtility ile aynı şekil: genel alanlar).
            var profile = JsonSerializer.Deserialize<AchievementProfile>(JsonSerializer.Serialize(service.ToProfile(), Json), Json);
            var loaded = new AchievementService(AchievementCatalog.Default, profile);
            Assert.False(loaded.Dirty);
            Assert.Equal(service.UnlockedCount, loaded.UnlockedCount);
            Assert.Equal(60, loaded.Best("ach.harvest_100"));
            Assert.False(string.IsNullOrEmpty(profile.entries.First(e => e.id == "ach.generation_3").unlockedUtc));
            Assert.Empty(loaded.Evaluate(Snapshot(generation: 3)));   // yüklenen profilde yeniden bildirim yok
        }

        [Fact]
        public void Basarim_BilinmeyenProfilGirdisiSaklanir_ElleAcmaCalisir()
        {
            var profile = new AchievementProfile { entries = new[] { new AchievementEntry { id = "ach.eski_surum", best = 3, unlocked = true, unlockedUtc = "2026-01-01T00:00:00Z" }, null } };
            var service = new AchievementService(AchievementCatalog.Default, profile);
            Assert.True(service.Unlock("ach.marriage"));
            Assert.False(service.Unlock("ach.marriage"));
            Assert.False(service.Unlock("ach.yok"));
            var saved = service.ToProfile();
            Assert.Contains(saved.entries, e => e.id == "ach.eski_surum" && e.unlocked);
            Assert.Equal(saved.entries.Select(e => e.id).OrderBy(x => x, StringComparer.Ordinal).ToArray(), saved.entries.Select(e => e.id).ToArray());
        }

        [Fact]
        public void Basarim_Katalog_KimliklerTekil_MetinlerIkiDildeVar()
        {
            var defs = AchievementCatalog.Default;
            Assert.True(defs.Count >= 20);
            Assert.Equal(defs.Count, defs.Select(d => d.Id).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(defs.Count, defs.Select(d => d.PlatformName).Distinct(StringComparer.Ordinal).Count());

            string locDir = Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));
            var bad = new List<string>();
            foreach (string lang in new[] { "en", "tr" })
            {
                var errors = new List<string>();
                var table = LocTable.Parse(File.ReadAllText(Path.Combine(locDir, "ui.achievements." + lang + ".json")), errors, "ui.achievements." + lang + ".json");
                Assert.True(errors.Count == 0, string.Join("\n", errors));
                foreach (var d in defs)
                {
                    Assert.StartsWith("ach.", d.Id);
                    Assert.True(d.Target > 0, d.Id);
                    Assert.True(d.Kind != AchievementKind.Counter || !string.IsNullOrEmpty(d.Key), d.Id + ": sayaç anahtarı yok");
                    // Mağaza API adı: büyük harf, rakam, alt çizgi (Türkçe kültürde de aynı üretilir).
                    Assert.True(d.PlatformName.All(c => c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '_'), d.PlatformName);
                    foreach (string key in new[] { d.NameKey, d.DescKey })
                        if (!table.TryGet(key, out var text) || string.IsNullOrWhiteSpace(text)) bad.Add(lang + ": " + key);
                }
            }
            Assert.True(bad.Count == 0, "Eksik başarım metni:\n" + string.Join("\n", bad));
        }

        // ---------------- görüntü seçenekleri ----------------

        [Fact]
        public void Goruntu_SeceneklerTekilSirali_KucuklerAtilir_UclardaDurur()
        {
            var choices = DisplayOptions.Choices(new[] { (1920, 1080), (1280, 720), (1920, 1080), (800, 600), (2560, 1440) }, (1280, 720));
            Assert.Equal(new List<(int, int)> { (1280, 720), (1920, 1080), (2560, 1440) }, choices);
            Assert.Equal((1920, 1080), DisplayOptions.Step(choices, (1280, 720), +1));
            Assert.Equal((1280, 720), DisplayOptions.Step(choices, (1280, 720), -1));
            Assert.Equal((2560, 1440), DisplayOptions.Step(choices, (2560, 1440), +1));
            Assert.Equal((1920, 1080), DisplayOptions.Step(choices, (1900, 1000), 0));   // listede olmayan boyut: en yakını

            var none = DisplayOptions.Choices(new (int, int)[0], (1024, 600));
            Assert.Equal(new List<(int, int)> { (1024, 600) }, none);
        }
    }
}
