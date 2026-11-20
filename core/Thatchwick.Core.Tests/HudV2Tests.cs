using System.Linq;
using Thatchwick.Core.Life;

namespace Thatchwick.Core.Tests
{
    // HUD v2 (Project belgesi 66): kayan bildirim kartları, karakter kartının dokununca açılması, Defter bölümleri.
    public class HudV2Tests
    {
        [Fact]
        public void Bildirim_EnFazlaUcKart_EnEskisiDuser()
        {
            var q = new ToastQueue();
            q.Push("a", NoticeLevel.Info, 0f);
            q.Push("b", NoticeLevel.Info, 0.1f);
            q.Push("c", NoticeLevel.Info, 0.2f);
            q.Push("d", NoticeLevel.Info, 0.3f);
            Assert.Equal(ToastQueue.MaxVisible, q.Items.Count);
            Assert.Equal(new[] { "b", "c", "d" }, q.Items.Select(t => t.Text).ToArray());
        }

        [Fact]
        public void Bildirim_SuresiDolunca_Kalkar_SonundaSolar()
        {
            var q = new ToastQueue();
            q.Push("a", NoticeLevel.Info, 0f);
            q.Tick(1f);
            Assert.Equal(1f, q.Items[0].Alpha);
            q.Tick(ToastQueue.ShowSeconds - ToastQueue.FadeSeconds / 2f);
            Assert.True(q.Items[0].Alpha < 1f && q.Items[0].Alpha > 0f);
            Assert.True(q.Tick(ToastQueue.ShowSeconds + 0.01f));
            Assert.Empty(q.Items);
        }

        [Fact]
        public void Bildirim_AyniMetin_IkinciKartAcmaz_SuresiYenilenir()
        {
            var q = new ToastQueue();
            q.Push("a", NoticeLevel.Info, 0f);
            q.Push("a", NoticeLevel.Danger, 3f);
            Assert.Single(q.Items);
            Assert.Equal(NoticeLevel.Danger, q.Items[0].Level);
            q.Tick(4f);
            Assert.Single(q.Items);
        }

        [Fact]
        public void Bildirim_GelipGecenBilgi_KartOlmaz_UyariOlur()
        {
            Assert.False(ToastQueue.Worth(new NoticeEntry { Text = "acıktı", Transient = true, Level = NoticeLevel.Info }));
            Assert.True(ToastQueue.Worth(new NoticeEntry { Text = "çok aç", Transient = true, Level = NoticeLevel.Warning }));
            Assert.True(ToastQueue.Worth(new NoticeEntry { Text = "doğdu", Level = NoticeLevel.Info }));
            Assert.False(ToastQueue.Worth(new NoticeEntry { Text = " " }));
        }

        [Fact]
        public void Kart_DokununcaAcilir_AyniKisiKapatir_BaskasiGecirir()
        {
            var t = new CardToggle();
            Assert.False(t.Open);
            Assert.True(t.Tap(1));
            Assert.True(t.Open);
            Assert.True(t.Tap(2));
            Assert.Equal(2, t.CharacterId);
            Assert.False(t.Tap(2));
            Assert.False(t.Open);
            t.Tap(3);
            t.Close();
            Assert.False(t.Open);
        }

        [Fact]
        public void Defter_BolumlerTekil_RozetOkunmamislarla()
        {
            Assert.Equal(LedgerSections.Order.Count, LedgerSections.Order.Distinct().Count());
            Assert.Contains(LedgerSections.Notices, LedgerSections.Order);
            Assert.Contains(LedgerSections.Settings, LedgerSections.Order);
            Assert.False(LedgerSections.Badge(0, 0));
            Assert.True(LedgerSections.Badge(2, 0));
            Assert.True(LedgerSections.Badge(0, 1));
            Assert.Equal(2, LedgerSections.Count(LedgerSections.Notices, 2, 5));
            Assert.Equal(5, LedgerSections.Count(LedgerSections.Requests, 2, 5));
            Assert.Equal(0, LedgerSections.Count(LedgerSections.Family, 2, 5));
        }
    }
}
