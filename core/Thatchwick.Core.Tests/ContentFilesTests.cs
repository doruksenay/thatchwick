using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Tests
{
    // Gerçek içerik dosyaları (Assets/Resources/Content) birlikte yüklenip doğrulanır: kırık kimlik, eksik
    // eşya, çakışan özellik gibi veri hataları CI'da yakalanır.
    public class ContentFilesTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        internal static string ContentDir([CallerFilePath] string here = "")
        {
            // Kaynak dosyanın yolundan: core/Thatchwick.Core.Tests/ -> depo kökü.
            var fromSource = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here) ?? ".", "..", "..", "Assets", "Resources", "Content"));
            if (Directory.Exists(fromSource)) return fromSource;
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "Assets", "Resources", "Content");
                if (Directory.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
            throw new DirectoryNotFoundException("Assets/Resources/Content bulunamadı.");
        }

        internal static ContentFileDto LoadAll()
        {
            var dir = ContentDir();
            var file = JsonSerializer.Deserialize<ContentFileDto>(File.ReadAllText(Path.Combine(dir, "items.json")), Json)!;
            var lifePath = Path.Combine(dir, "life.json");
            if (File.Exists(lifePath)) file.Merge(JsonSerializer.Deserialize<ContentFileDto>(File.ReadAllText(lifePath), Json));
            return file;
        }

        internal static ContentDatabase LoadDb(out System.Collections.Generic.List<string> errors)
        {
            var db = new ContentDatabase();
            errors = ContentMapper.Apply(LoadAll(), db);
            return db;
        }

        [Fact]
        public void GercekIcerik_HatasizYuklenir()
        {
            LoadDb(out var errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
        }

        [Fact]
        public void GercekIcerik_OzellikSetiVeKonforVar()
        {
            var db = LoadDb(out _);
            Assert.True(db.Traits.Count >= 10);
            Assert.True(db.Items.Get("item.grave").Category == "memorial");
            Assert.True(db.Items.Get("item.candle").Comfort > 0);
            foreach (var t in db.Traits.All) Assert.False(string.IsNullOrWhiteSpace(t.Description), t.Id);
        }
    }
}
