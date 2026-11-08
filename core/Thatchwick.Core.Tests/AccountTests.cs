using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;
using Thatchwick.Core.Progress;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // T9 (hesap XP'si: günlük tavan, ilk kez olayları, rozetler) ve T10 (seviyeye göre dükkân çeşidi).
    public class AccountTests
    {
        [Fact]
        public void SeviyeEgrisi()
        {
            Assert.Equal(1, AccountProgress.LevelForXp(0));
            Assert.Equal(2, AccountProgress.LevelForXp(100));
            Assert.Equal(2, AccountProgress.LevelForXp(299));
            Assert.Equal(3, AccountProgress.LevelForXp(300));
            Assert.Equal(10, AccountProgress.LevelForXp(4500));
            Assert.Equal(AccountProgress.MaxLevel, AccountProgress.LevelForXp(int.MaxValue / 2));
        }

        [Fact]
        public void T9_GunlukIsXpTavanli_YeniGundeSifirlanir()
        {
            var a = new AccountProgress();
            for (int i = 0; i < 100; i++) a.AddWorkXp(AccountProgress.WorkXp, 3);
            Assert.Equal(AccountProgress.DailyWorkCap, a.Xp);
            Assert.Equal(0, a.AddWorkXp(5, 3));
            Assert.Equal(5, a.AddWorkXp(5, 4));
            Assert.Equal(AccountProgress.DailyWorkCap + 5, a.Xp);
        }

        [Fact]
        public void T9_IlkKezBirKez_TavanaTakilmaz()
        {
            var a = new AccountProgress();
            for (int i = 0; i < 40; i++) a.AddWorkXp(5, 0);   // tavan doldu
            int before = a.Xp;
            Assert.True(a.FirstTime("first:chop"));
            Assert.False(a.FirstTime("first:chop"));
            Assert.Equal(before + AccountProgress.FirstTimeXp, a.Xp);
        }

        [Fact]
        public void T9_RozetSayacaUlasincaBirKezVerilir()
        {
            var a = new AccountProgress();
            var earned = new List<string>();
            a.BadgeEarned += b => earned.Add(b.Id);
            for (int i = 0; i < 12; i++) a.Record("chop", i);
            Assert.Equal(new[] { "badge.woodcutter" }, earned);
            Assert.True(a.HasBadge("badge.woodcutter"));
            Assert.Equal(12, a.Counter("act:chop"));
            // 12 iş (5'er) + ilk kez 50 + rozet 100
            Assert.Equal(12 * 5 + 50 + 100, a.Xp);
        }

        [Fact]
        public void BeceriSeviyesiXpVerir_SeviyeOlayiTetiklenir()
        {
            var a = new AccountProgress();
            int newLevel = 0;
            a.LevelChanged += (_, to) => newLevel = to;
            for (int i = 0; i < 4; i++) a.OnSkillLevelUp();
            Assert.Equal(100, a.Xp);
            Assert.Equal(2, newLevel);
        }

        [Fact]
        public void IsOlmayanKayit_IsXpVermez_AmaIlkKezVeSayacIsler()
        {
            var a = new AccountProgress();
            a.Record("room", 0, isWork: false);
            Assert.Equal(AccountProgress.FirstTimeXp, a.Xp);
            Assert.Equal(1, a.Counter("act:room"));
        }

        private static (BuildService b, AccountProgress acc) Shop()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "stool", DisplayName = "Tabure", Price = 5 });
            db.Items.Register(new ItemDef { Id = "chair", DisplayName = "Sandalye", Price = 10, UnlockLevel = 2 });
            db.Items.Register(new ItemDef { Id = "carved", DisplayName = "Oymalı sandalye", Price = 30, UnlockLevel = 10 });
            Assert.Empty(db.Validate());
            var acc = new AccountProgress();
            var b = new BuildService(new WorldState(new GridMap(6, 6)), new Household(500), db) { AccountLevel = () => acc.Level };
            return (b, acc);
        }

        [Fact]
        public void T10_SeviyeAltindakiEsyaAlinmaz_SeviyeGelinceAcilir()
        {
            var (b, acc) = Shop();
            Assert.True(b.Buy("stool").Ok);
            var r = b.Buy("chair");
            Assert.False(r.Ok);
            Assert.Contains("seviyesi 2", r.Reason);
            acc.AddWorkXp(100, 0);
            Assert.True(b.Buy("chair").Ok);
            Assert.False(b.Buy("carved").Ok);
            for (int d = 1; d < 40; d++) acc.AddWorkXp(150, d);
            Assert.True(acc.Level >= 10);
            Assert.True(b.Buy("carved").Ok);
        }

        [Fact]
        public void T10_MagazaListesiKilitliEsyalariDaGosterir_SeviyeyeGoreSirali()
        {
            var (b, _) = Shop();
            var list = b.ShopList();
            Assert.Equal(new[] { "stool", "chair", "carved" }, list.Select(d => d.Id).ToArray());
            Assert.False(b.IsUnlocked(list[1]));
        }

        [Fact]
        public void Icerik_AcilmaSeviyesiDogrulanir()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "a", DisplayName = "x", UnlockLevel = 0 });
            db.Items.Register(new ItemDef { Id = "b", DisplayName = "x", UnlockLevel = 25 });
            Assert.Equal(2, db.Validate().Count);
        }
    }
}
