using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Life;
using Thatchwick.Core.Life.Requests;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Upbringing;
using Thatchwick.Core.Work;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // İstekler, süreli görev ve çocuk gezme metinleri anahtarlıdır (belge 21, 46 §17): her anahtar iki tabloda da olmalı,
    // kayıt anahtarı (LocCodec) bozulmadan gidip gelmeli.
    public class IstekYerelTests
    {
        private static string LocDir() =>
            Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "Localization"));

        private static string ScriptsDir() =>
            Path.GetFullPath(Path.Combine(ContentFilesTests.ContentDir(), "..", "..", "Scripts"));

        private static LocTable Load(string file)
        {
            var errors = new List<string>();
            var t = LocTable.Parse(File.ReadAllText(Path.Combine(LocDir(), file)), errors, file);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return t;
        }

        private static readonly Lazy<LocTable> En = new Lazy<LocTable>(() => Load("ui.en.json"));
        private static readonly Lazy<LocTable> Tr = new Lazy<LocTable>(() => Load("ui.tr.json"));

        private static void AssertKeys(IEnumerable<string> keys)
        {
            var missing = keys.Distinct().Where(k => !En.Value.Contains(k) || !Tr.Value.Contains(k)).ToList();
            Assert.True(missing.Count == 0, "Eksik anahtar: " + string.Join(", ", missing.Take(20)));
        }

        [Fact]
        public void Katalog_HerAnahtar_IkiTablodaVar()
        {
            var keys = RequestCatalog.AllKeys().ToList();
            Assert.True(keys.Count > 300, keys.Count.ToString());
            AssertKeys(keys);
        }

        [Fact]
        public void BilesikAnahtarlar_IkiTablodaVar()
        {
            var keys = new List<string>();
            foreach (RequestKind k in Enum.GetValues(typeof(RequestKind))) keys.Add(Request.KindKey(k));
            foreach (RequestState s in Enum.GetValues(typeof(RequestState))) keys.Add("ui.req.state." + s.ToString().ToLowerInvariant());
            foreach (SkillType s in Enum.GetValues(typeof(SkillType))) keys.Add("skill." + s);
            foreach (var r in Regions.All) keys.Add("region." + r.Id);
            foreach (TimedDutyKind k in Enum.GetValues(typeof(TimedDutyKind))) keys.Add(TimedDutyService.KindText(k).Key);
            foreach (TimedDutyEnd e in Enum.GetValues(typeof(TimedDutyEnd))) keys.Add("duty.end." + e.ToString().ToLowerInvariant());
            foreach (var p in TimedDutyService.Places) keys.Add(TimedDutyService.PlaceText(p.Id).Key);
            foreach (RoamPermission p in Enum.GetValues(typeof(RoamPermission))) keys.Add(ChildRoaming.PermissionText(p).Key);
            for (int i = 0; i < ChildRoaming.ScoldCount; i++) keys.Add(ChildRoaming.Scold(i).Key);
            for (int i = 0; i < ChildRoaming.KindCount; i++) keys.Add(ChildRoaming.Kind(i).Key);
            for (int q = 0; q <= Thatchwick.Core.Content.ItemQuality.Max; q++) keys.Add(Thatchwick.Core.Content.ItemQuality.NameKey(q));
            foreach (var sh in ProduceShowService.Shows) keys.Add(sh.Name.Key);
            keys.Add("content.act.embellish_carve"); keys.Add("content.act.embellish_stitch");
            AssertKeys(keys);
        }

        [Fact]
        public void KoddakiDuzAnahtarlar_IkiTablodaVar()
        {
            // İstek, görev ve gezme kodunda yazılı anahtarlar ("core.req.x", "duty.err.y" ...); önek olanlar (nokta ile biten) hariç.
            var files = new[]
            {
                "Core/Life/Requests/RequestService.cs", "Core/Life/Requests/RequestCatalog.cs", "Core/Work/TimedDuties.cs",
                "Core/Upbringing/ChildRoaming.cs", "Core/Life/LifeSystems.cs", "Game/RequestPanel.cs", "Game/FamilyPanel.cs",
                "Game/VillagePanel.cs", "Game/GameRoot.cs", "Core/Life/QualityBook.cs", "Game/MarketPanel.cs", "Game/UiSkin.cs", "Core/Life/ProduceShow.cs"
            };
            var rx = new Regex("(?:LocText\\.Of|\\bL|\\bT|GameLocalization\\.T)\\(\\s*\"((?:core\\.req|req|duty|roam|ui\\.req|ui\\.duty|ui\\.roam|quality|ui\\.quality|ui\\.market|show|ui\\.show)\\.[a-z0-9_.]*[a-z0-9_])\"");
            var keys = new List<string>();
            foreach (var f in files)
            {
                var path = Path.Combine(ScriptsDir(), f);
                foreach (Match m in rx.Matches(File.ReadAllText(path))) keys.Add(m.Groups[1].Value);
            }
            Assert.True(keys.Count > 150, keys.Count.ToString());
            AssertKeys(keys);
        }

        [Fact]
        public void Kodlayici_IcIceAnahtar_SayiVeOzelKarakter_GidipGelir()
        {
            var t = LocText.Of("core.req.n.done", ("title", LocText.Of("req.order.title", ("n", 3), ("item", "item.bread"))),
                ("name", "Ali | Veli: (usta) @ ev = 1 \\"), ("akce", 12), ("frac", 2.5));
            var s = LocCodec.Encode(t);
            var back = LocCodec.Decode(s);
            Assert.Equal(s, LocCodec.Encode(back));
            Assert.Equal("core.req.n.done", back.Key);
            var inner = (LocText)back.Args[0].value;
            Assert.Equal("req.order.title", inner.Key);
            Assert.Equal("item.bread", inner.Args[1].value);
            Assert.Equal("Ali | Veli: (usta) @ ev = 1 \\", back.Args[1].value);

            var plain = LocText.Plain("Elle yazılmış: not");
            Assert.Equal("Elle yazılmış: not", LocCodec.Decode(LocCodec.Encode(plain)).Literal);
            // Eski kayıttaki düz Türkçe metin düz metin olarak okunur.
            Assert.Equal("Değirmenciye un götür", LocCodec.Decode("Değirmenciye un götür").Literal);
            Assert.True(LocCodec.Decode("").IsEmpty);
        }

        [Fact]
        public void Cozumleyici_IcIceMetin_VeEsyaAdi_Doldurur()
        {
            var en = new Localizer("en", En.Value);
            var tr = new Localizer("tr", Tr.Value, En.Value);
            Func<string, string> item = id => id == "item.bread" ? "Ekmek" : id;
            var t = LocText.Of("duty.sum", ("name", "Ali"), ("kind", TimedDutyService.KindText(TimedDutyKind.Fish)),
                ("place", TimedDutyService.PlaceText("fish:pond")), ("why", LocText.Of("duty.end.done")),
                ("what", LocText.Of("duty.sum.item", ("n", 2), ("item", "item.bread"))));
            string s = LocResolve.Deep(tr, t, item);
            Assert.Contains("Çiftlik göleti", s);
            Assert.Contains("2 ekmek", s);
            Assert.DoesNotContain("{", s);
            Assert.Contains("Farm pond", LocResolve.Deep(en, t, item));
        }
    }
}
