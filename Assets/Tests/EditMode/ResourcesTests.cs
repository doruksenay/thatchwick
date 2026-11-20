using NUnit.Framework;
using UnityEngine;
using Thatchwick.Game;

namespace Thatchwick.EditTests
{
    /// <summary>Unity'nin Resources klasöründen okunan dosyalar: çekirdek testleri bunları dosya yolundan okur, oyun Resources'tan.</summary>
    public class ResourcesTests
    {
        [Test]
        public void Icerik_ResourcestanHatasizYuklenir()
        {
            var content = ContentLoader.Load(out var errors);
            Assert.IsNotNull(content);
            Assert.IsEmpty(errors, string.Join("\n", errors));
            Assert.Greater(content.Items.Count, 500);
        }

        [TestCase("Localization/ui.en")]
        [TestCase("Localization/ui.tr")]
        [TestCase("Localization/ui.build.en")]
        [TestCase("Localization/ui.build.tr")]
        [TestCase("Localization/ui.system.en")]
        [TestCase("Localization/ui.system.tr")]
        [TestCase("Localization/content.en")]
        [TestCase("Content/items")]
        [TestCase("Content/life")]
        [TestCase("Content/appearance")]
        [TestCase("Content/layouts")]
        public void MetinDosyasi_ResourcestaVar(string path)
        {
            var asset = Resources.Load<TextAsset>(path);
            Assert.IsNotNull(asset, $"Resources/{path} yok ya da TextAsset olarak içe aktarılmamış.");
            Assert.Greater(asset.text.Length, 10);
        }

        [Test]
        public void GorselKutuphanesi_Kurulu()
        {
            Assert.IsNotNull(Resources.Load<IsoArtLibrary>("IsoArt/IsoArtLibrary"), "IsoArtLibrary.asset yok: Assets/Art/Iso değişince editörde yeniden kurulur.");
        }
    }
}
