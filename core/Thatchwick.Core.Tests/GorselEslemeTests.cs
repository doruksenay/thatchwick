using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Tests
{
    // Tester raporu 66 §1.3: görseli depoda olduğu halde adı tutmadığı için yer tutucu kutuyla çizilen eşyalar.
    // items.json'daki her görsel adı Assets/Art/Iso altında bir PNG olmalı; dosya henüz gelmediyse "pending" işaretli olur.
    // Onarım betiği: python tools/art/art_link.py --apply
    public class GorselEslemeTests
    {
        private static HashSet<string> ArtNames()
        {
            var content = ContentFilesTests.ContentDir();
            var art = Path.GetFullPath(Path.Combine(content, "..", "..", "Art", "Iso"));
            Assert.True(Directory.Exists(art), art);
            return new HashSet<string>(Directory.EnumerateFiles(art, "*.png", SearchOption.AllDirectories)
                .Select(Path.GetFileNameWithoutExtension), StringComparer.Ordinal);
        }

        private static IEnumerable<string> Names(ItemArtDef a)
        {
            yield return a.AlongX; yield return a.AlongY; yield return a.Lit;
            yield return a.Young; yield return a.Grown; yield return a.Fruit;
            foreach (var f in a.Faces) yield return f;
        }

        [Fact]
        public void EsyaGorselAdlari_DepodaVarYaDaBekliyor()
        {
            var files = ArtNames();
            var db = ContentFilesTests.LoadDb(out _);
            var bad = new List<string>();
            foreach (var item in db.Items.All)
            {
                if (item.Art == null || item.Art.Pending) continue;
                foreach (var n in Names(item.Art))
                    if (n != null && !files.Contains(n)) bad.Add($"{item.Id}: {n}");
            }
            Assert.True(bad.Count == 0, "Depoda olmayan görsel adı (pending değil):\n" + string.Join("\n", bad));
        }

        /// <summary>
        /// Görseli gelmiş ama hâlâ "pending" işaretli eşya main'i bozmaz (kullanıcı kararı 21.11.2026: işçi PNG'yi main'e
        /// koyar, items.json'a dokunamaz). Oyun dosya varsa görseli kullanır (Awaiting false); liste yalnızca uyarı olarak
        /// yazılır, işaret `python tools/art/art_link.py --apply` ile temizlenir.
        /// </summary>
        [Fact]
        public void BekleyenGorsel_DepodaVarsa_OyunBeklemez_YalnizcaUyari()
        {
            var files = ArtNames();
            var db = ContentFilesTests.LoadDb(out _);
            var stale = new List<string>();
            var old = ItemArtDef.FileExists;
            try
            {
                ItemArtDef.FileExists = files.Contains;
                foreach (var item in db.Items.All)
                {
                    if (item.Art == null || !item.Art.Pending) continue;
                    var names = Names(item.Art).Where(n => n != null).ToList();
                    if (names.Count > 0 && names.All(files.Contains))
                    {
                        stale.Add(item.Id);
                        Assert.False(item.Art.Awaiting, item.Id);
                    }
                    else if (names.Count > 0) Assert.True(item.Art.Awaiting, item.Id);
                }
            }
            finally { ItemArtDef.FileExists = old; }
            if (stale.Count > 0) Console.WriteLine("UYARI: görseli gelmiş ama hâlâ pending (art_link.py --apply): " + string.Join(", ", stale));
        }

        [Fact]
        public void Awaiting_DosyaGelinceBitiyor()
        {
            var art = new ItemArtDef { AlongX = "iso_item_x_se", Pending = true };
            var old = ItemArtDef.FileExists;
            try
            {
                ItemArtDef.FileExists = null;
                Assert.True(art.Awaiting);
                ItemArtDef.FileExists = n => false;
                Assert.True(art.Awaiting);
                ItemArtDef.FileExists = n => n == "iso_item_x_se";
                Assert.False(art.Awaiting);
                art.Pending = false;
                ItemArtDef.FileExists = n => false;
                Assert.False(art.Awaiting);
            }
            finally { ItemArtDef.FileExists = old; }
        }
    }
}
