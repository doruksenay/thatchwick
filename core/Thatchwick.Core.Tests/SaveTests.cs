using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Progress;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Tests
{
    // 15b yerel kayıt ve T16 (kayıt uyumluluğu).
    public class SaveTests
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

        internal static ContentDatabase Content()
        {
            var db = new ContentDatabase();
            db.Items.Register(new ItemDef { Id = "bed", DisplayName = "Yatak", Width = 2, Price = 30 });
            db.Items.Register(new ItemDef { Id = "hearth", DisplayName = "Ocak", HeatOutput = 80f, Price = 40 });
            db.Items.Register(new ItemDef
            {
                Id = "table", DisplayName = "Masa", Width = 2, Price = 20,
                SurfaceSlots = new List<SurfaceSlotDef> { new SurfaceSlotDef { Id = "a", U = 0.25f, V = 0.5f } }
            });
            db.Items.Register(new ItemDef { Id = "candle", DisplayName = "Mum", Placement = ItemPlacement.Surface, Price = 4 });
            db.Items.Register(new ItemDef { Id = "oak", DisplayName = "Meşe", GrowDays = 12f });
            db.Items.Register(new ItemDef { Id = "firewood", DisplayName = "Odun", Placement = ItemPlacement.Resource, Price = 2, Tags = new List<string> { "fuel" } });
            db.Items.Register(new ItemDef { Id = "turnip", DisplayName = "Şalgam", Placement = ItemPlacement.Resource, Price = 2, Tags = new List<string> { "food" } });
            Assert.Empty(db.Validate());
            return db;
        }

        internal sealed class State
        {
            public ContentDatabase Db; public GameClock Clock; public WorldState World; public Household House;
            public List<Character> Chars; public AccountProgress Account; public HeatSystem Heat; public Market Market;
        }

        /// <summary>Kayıt testleri ve sabit örnek dosyası (save_v1.json) için zengin bir durum.</summary>
        internal static State BuildState()
        {
            var db = Content();
            var clock = new GameClock();
            clock.Restore(33.4, 2);
            var map = new GridMap(12, 10);
            map.SetBlocked(new GridPos(0, 9), true);
            map.RestrictOwnership(1, 1, 8, 8);
            map.SetZone(new GridPos(2, 7), ZoneType.Storage);
            for (int x = 2; x <= 4; x++) { map.SetEdge(new Edge(x, 2, EdgeAxis.Horizontal), EdgeKind.Wall); map.SetEdge(new Edge(x, 5, EdgeAxis.Horizontal), EdgeKind.Wall); }
            for (int y = 2; y <= 4; y++) { map.SetEdge(new Edge(2, y, EdgeAxis.Vertical), EdgeKind.Wall); map.SetEdge(new Edge(5, y, EdgeAxis.Vertical), EdgeKind.Wall); }
            map.SetEdge(new Edge(3, 2, EdgeAxis.Horizontal), EdgeKind.Door);
            for (int x = 2; x <= 4; x++) for (int y = 2; y <= 4; y++) map.SetFloor(new GridPos(x, y), FloorType.Wood);
            var world = new WorldState(map);
            var hearth = world.PlaceItem(db.Items.Get("hearth"), new GridPos(4, 4));
            var table = world.PlaceItem(db.Items.Get("table"), new GridPos(2, 3), 0);
            world.PlaceOnSurface(db.Items.Get("candle"), table.Id, "a");
            world.PlaceItem(db.Items.Get("bed"), new GridPos(6, 6), 1);
            world.PlaceItem(db.Items.Get("oak"), new GridPos(7, 2));

            var house = new Household(57);
            house.AddItem("firewood", 9);
            house.AddItem("turnip", 4);
            var heat = new HeatSystem(world, clock.Config, null, clock.TotalDays);
            heat.TryStoke(hearth.Id, house);
            heat.Tick(0.2, clock.TotalDays);

            var aldric = new Character(1, "Aldric", new GridPos(3, 3), true) { Sex = Sex.Male };
            aldric.SetAge(30);
            aldric.LifespanDays = 120;
            aldric.GainSkillXp(SkillType.Woodcutting, 260);
            aldric.Needs.Set(NeedType.Hunger, 42f);
            aldric.Appearance.HairColor = new Customization.Rgb(10, 20, 30);
            var maud = new Character(2, "Maud", new GridPos(6, 8), false) { Sex = Sex.Female };
            maud.SetAge(40);
            maud.TryStartPregnancy(1);
            var old = new Character(3, "Osric", new GridPos(1, 1), false) { Sex = Sex.Male };
            old.SetAge(143.99);
            old.Tick(0.1);   // yaşlılıktan ölür

            var account = new AccountProgress();
            account.Record("chop", 30);
            account.Record("contract", 31);
            for (int i = 0; i < 3; i++) account.OnSkillLevelUp();

            var market = new Market(db, house, null, null, seed: 5);
            market.Tick(0, clock.TotalDays, clock.Season);
            market.Sell("turnip", 2);
            var job = market.Offers.First(o => o.IsContract);
            house.AddItem(job.GiveItem, job.GiveCount);
            market.Accept(job.Id);

            // Not: save_v1.json bu durumdan üretildi. Bu kurulumu değiştirsen de dosyayı yeniden üretme;
            // eski dosyanın açılmaya devam ettiğini sınamak T16'nın amacı.
            return new State { Db = db, Clock = clock, World = world, House = house, Chars = new List<Character> { aldric, maud, old }, Account = account, Heat = heat, Market = market };
        }

        private static SaveData Capture(State s) => SaveGame.Capture(s.Clock, s.World, s.House, s.Chars, s.Account, s.Heat, s.Market);

        private static SaveData RoundTrip(SaveData d) => JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(d, Json), Json);

        [Fact]
        public void GidisDonus_DunyaHaneVeKarakterlerAyni()
        {
            var s = BuildState();
            var loaded = SaveGame.Load(RoundTrip(Capture(s)), s.Db);
            Assert.Empty(loaded.Warnings);

            Assert.Equal(s.Clock.TotalDays, loaded.Clock.TotalDays, 6);
            Assert.Equal(2, loaded.Clock.RequestedSpeed);

            var m0 = s.World.Map; var m1 = loaded.World.Map;
            for (int y = 0; y < m0.Height; y++)
                for (int x = 0; x < m0.Width; x++)
                {
                    var p = new GridPos(x, y);
                    Assert.Equal(m0.IsBlocked(p), m1.IsBlocked(p));
                    Assert.Equal(m0.GetFloor(p), m1.GetFloor(p));
                    Assert.Equal(m0.IsOwned(p), m1.IsOwned(p));
                    Assert.Equal(m0.GetZone(p), m1.GetZone(p));
                }
            Assert.Equal(m0.Edges.Count(), m1.Edges.Count());
            Assert.Equal(EdgeKind.Door, m1.GetEdge(new Edge(3, 2, EdgeAxis.Horizontal)));
            Assert.True(m1.Rooms.RoomAt(new GridPos(3, 3)).IsIndoor);

            Assert.Equal(s.World.Items.Count, loaded.World.Items.Count);
            foreach (var a in s.World.Items)
            {
                var b = loaded.World.GetItem(a.Id);
                Assert.NotNull(b);
                Assert.Equal(a.Def.Id, b.Def.Id);
                Assert.Equal(a.Position, b.Position);
                Assert.Equal(a.Rotation, b.Rotation);
                Assert.Equal(a.ParentId, b.ParentId);
                Assert.Equal(a.SlotId, b.SlotId);
                Assert.Equal(a.FuelDays, b.FuelDays, 6);
                Assert.Equal(a.GrowthDays, b.GrowthDays, 6);
            }

            Assert.Equal(s.House.Akce, loaded.Household.Akce);
            Assert.Equal(s.House.InventoryList(), loaded.Household.InventoryList());

            Assert.Equal(3, loaded.Characters.Count);
            for (int i = 0; i < 3; i++)
            {
                var a = s.Chars[i]; var b = loaded.Characters[i];
                Assert.Equal(a.Id, b.Id); Assert.Equal(a.Name, b.Name); Assert.Equal(a.IsPlayerControlled, b.IsPlayerControlled);
                Assert.Equal(a.Sex, b.Sex); Assert.Equal(a.Position, b.Position);
                Assert.Equal(a.AgeDays, b.AgeDays, 6); Assert.Equal(a.LifespanDays, b.LifespanDays, 6);
                Assert.Equal(a.IsDead, b.IsDead); Assert.Equal(a.CauseOfDeath, b.CauseOfDeath);
                Assert.Equal(a.IsPregnant, b.IsPregnant); Assert.Equal(a.FatherId, b.FatherId);
                Assert.Equal((double)a.Needs.Get(NeedType.Hunger), (double)b.Needs.Get(NeedType.Hunger), 3);
                Assert.Equal(a.Skills.GetXp(SkillType.Woodcutting), b.Skills.GetXp(SkillType.Woodcutting), 6);
                Assert.Equal(a.Appearance.Key(), b.Appearance.Key());
            }
            Assert.Equal(DeathCause.OldAge, loaded.Characters[2].CauseOfDeath);
        }

        [Fact]
        public void GidisDonus_HamilelikDevamEder()
        {
            var s = BuildState();
            var loaded = SaveGame.Load(RoundTrip(Capture(s)), s.Db);
            var maud = loaded.Characters.First(c => c.Name == "Maud");
            int births = 0;
            maud.GaveBirth += (_, __) => births++;
            for (int i = 0; i < 31; i++) maud.Tick(1);
            Assert.Equal(1, births);
        }

        [Fact]
        public void GidisDonus_HesapIsiVePazar()
        {
            var s = BuildState();
            var loaded = SaveGame.Load(RoundTrip(Capture(s)), s.Db);
            Assert.Equal(s.Account.Xp, loaded.Account.Xp);
            Assert.True(loaded.Account.HasFirst("first:chop"));
            Assert.Equal(s.Account.Counter("act:contract"), loaded.Account.Counter("act:contract"));
            Assert.Equal(s.Account.WorkXpToday(31), loaded.Account.WorkXpToday(31));

            var heat = new HeatSystem(loaded.World, loaded.Clock.Config, null, loaded.Clock.TotalDays);
            heat.RestoreTemperatures(loaded.Temperatures);
            Assert.Equal((double)s.Heat.TemperatureAt(new GridPos(3, 3)), (double)heat.TemperatureAt(new GridPos(3, 3)), 4);

            var market = new Market(s.Db, loaded.Household, null, null, seed: 5);
            market.RestoreState(loaded.MarketDemand, loaded.Clock.TotalDays, loaded.Clock.Season, loaded.MarketTakenOffers);
            Assert.Equal(s.Market.SellPrice("turnip"), market.SellPrice("turnip"));
            Assert.Equal(s.Market.Offers.Count(o => o.Taken), market.Offers.Count(o => o.Taken));
            Assert.Equal(s.Market.Offers.Select(o => s.Market.Describe(o)), market.Offers.Select(o => market.Describe(o)));
        }

        [Fact]
        public void BilinmeyenEsya_UyariylaAtlanir_OyunAcilir()
        {
            var s = BuildState();
            var data = RoundTrip(Capture(s));
            data.items = data.items.Concat(new[] { new ItemSave { id = 99, def = "kaldirilmis_esya", x = 9, y = 9 } }).ToArray();
            data.household.inventory = data.household.inventory.Concat(new[] { new KeyInt { key = "eski_kaynak", value = 3 } }).ToArray();
            var loaded = SaveGame.Load(data, s.Db);
            Assert.Equal(2, loaded.Warnings.Count);
            Assert.Equal(s.World.Items.Count, loaded.World.Items.Count);
        }

        [Fact]
        public void T16_DahaYeniSurum_Reddedilir_SurumsuzBirSayilir()
        {
            var s = BuildState();
            var data = RoundTrip(Capture(s));
            data.version = SaveGame.CurrentVersion + 1;
            Assert.Throws<System.InvalidOperationException>(() => SaveGame.Load(data, s.Db));
            data.version = 0;
            SaveGame.Load(data, s.Db);
            Assert.Equal(SaveGame.CurrentVersion, data.version);   // sürümsüz = 1, sonra güncel sürüme yükseltilir
        }

        [Fact]
        public void T16_EskiKayitDosyasiAcilir()
        {
            // save_v1.json: sürüm 1 ile üretilmiş sabit örnek. Kayıt biçimi değişse de bu dosya açılmaya devam etmeli.
            var path = Path.Combine(System.AppContext.BaseDirectory, "SaveFixtures", "save_v1.json");
            var data = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(path), Json);
            var loaded = SaveGame.Load(data, Content());
            Assert.Empty(loaded.Warnings);
            Assert.Equal(SaveGame.CurrentVersion, data.version);   // v1 → güncel sürüme yükseltildi
            // v2: eski kayıtta keyif ve hijyen yoktu; tam doluyla başlar.
            foreach (var c in loaded.Characters)
            {
                Assert.Equal(100.0, (double)c.Needs.Get(NeedType.Fun), 3);
                Assert.Equal(100.0, (double)c.Needs.Get(NeedType.Hygiene), 3);
            }
            Assert.Empty(loaded.Animals);
            Assert.Equal(new[] { "Aldric", "Maud", "Osric" }, loaded.Characters.Select(c => c.Name).ToArray());
            Assert.Equal(5, loaded.World.Items.Count);
            Assert.True(loaded.World.Map.Rooms.RoomAt(new GridPos(3, 3)).IsIndoor);
            Assert.True(loaded.Household.Count("firewood") > 0);
            Assert.True(loaded.Account.HasFirst("first:chop"));
        }
    }
}
