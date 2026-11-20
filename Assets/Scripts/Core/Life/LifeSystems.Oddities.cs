using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    // Easter egg'ler ve Tuhaflıklar (Project belgesi 55): bağlama ve sıralama. Kurallar Oddities.cs'te.
    public sealed partial class LifeSystems
    {
        /// <summary>Görülen tuhaflıklar ve başarımlar (Doğa Defteri'ndeki gizli sayfa).</summary>
        public OddityBook Oddities { get; } = new OddityBook();
        /// <summary>Hayvanla konuşma ve bunu başkasına anlatma (belge 55 §3).</summary>
        public AnimalTalkService AnimalTalk { get; private set; }
        /// <summary>Bulunan silah ve sonraki kuşakların kalıntıları (belge 55 §4).</summary>
        public FoundRelicService Relics { get; private set; }
        public HelloCounter Hellos { get; } = new HelloCounter();

        /// <summary>
        /// Easter egg satırı (anahtarlı metin, ilgili karakter). Oyun katmanı çözer ve gösterir; bildirim akışına da
        /// çözülmüş hâliyle yazabilir. Core metni çevirmez (docs/yerellestirme.md).
        /// </summary>
        public event Action<LocText, int> OddityLine;

        /// <summary>Kahve falının dördüncü duvarı yıkma şansı.</summary>
        public const double FortuneFourthWallChance = 1.0 / 25.0;
        /// <summary>Test ve hata ayıklama: bir sonraki fal dördüncü duvarı yıkar.</summary>
        public bool ForceFortuneOddity { get; set; }

        private Random _oddityRng;

        private void WireOddities(int seed)
        {
            _oddityRng = new Random(seed != 0 ? Seeds.Derive(seed, 55) : 55);
            AnimalTalk = new AnimalTalkService(seed != 0 ? Seeds.Derive(seed, 56) : 56)
            {
                Book = Oddities,
                Today = () => Clock.TotalDays,
                Find = id => Population.Find(id),
                HouseKeyOf = OddityHouseKey,
                IsCompanion = (c, a) =>
                {
                    var b = Upbringing?.Companions.Get(c.Id, a.Id);
                    return b != null && (b.Unlocked || b.Friend);
                },
                NicknameDays = () => Clock.Config.DaysPerSeason
            };
            AnimalTalk.Tried += (c, a, res) => OddityLine?.Invoke(res.Line, c.Id);
            AnimalTalk.Confided += (rec, a) => OddityLine?.Invoke(LocText.Of("core.oddity.talk.confided", ("who", rec.Name), ("animal", a.Name)), rec.CharacterId);

            Relics = new FoundRelicService(seed != 0 ? Seeds.Derive(seed, 57) : 57)
            {
                Book = Oddities,
                Place = PlaceRelic,
                Remove = id => World.RemoveItem(id),
                PlaceAt = (itemId, x, y) =>
                {
                    if (!Content.Items.TryGet(itemId, out var def)) return true;   // içerikte yoksa beklemeyi bırak
                    var pos = new GridPos(x, y);
                    if (!World.CanPlaceFloor(def, pos, 0).Ok) return false;
                    World.PlaceItem(def, pos);
                    return true;
                },
                AddToHouse = (itemId, n) => House.AddItem(itemId, n)
            };
        }

        private string OddityHouseKey(Character c)
        {
            if (c == null) return null;
            if (!c.IsVillager) return AnimalTalkService.PlayerHouse;
            var h = Houses?.HouseOf(c);
            return h != null ? "house:" + h.Id : null;
        }

        /// <summary>Hanenin yaşayan en genç kuşağı (kalıntılar kuşak başına bir çıkar).</summary>
        public int HouseholdGeneration()
        {
            int g = 1;
            foreach (var c in Household()) g = Math.Max(g, c.Generation);
            return g;
        }

        /// <summary>Kalıntıyı köy yolunda boş, yürünebilir bir karoya koyar. Bölgesiz haritada (eski kayıt, test) çıkmaz.</summary>
        private (int itemId, int x, int y)? PlaceRelic(string itemId)
        {
            if (itemId == null || !Content.Items.TryGet(itemId, out var def)) return null;
            var map = World.Map;
            if (!Regions.IsRegional(map) || !Regions.Exists(map, Regions.Road)) return null;
            var r = Regions.Road;
            for (int i = 0; i < 60; i++)
            {
                var pos = new GridPos(r.X + 2 + _oddityRng.Next(Math.Max(1, r.W - 4)), r.Y + 2 + _oddityRng.Next(Math.Max(1, r.H - 4)));
                if (!map.InBounds(pos) || !map.IsWalkable(pos) || map.IsWater(pos)) continue;
                if (!World.CanPlaceFloor(def, pos, 0).Ok) continue;
                var item = World.PlaceItem(def, pos);
                if (item != null) return (item.Id, pos.X, pos.Y);
            }
            return null;
        }

        private void AttachOddities(FamilyService family)
        {
            family.AnimalTalk = AnimalTalk;
            AnimalTalk.AddAffection = (a, b, d) => family.SetAffection(a, b, family.Affection(a, b) + d);
            family.ToldAnimalTalk += (teller, listener, res) => OddityLine?.Invoke(res.Line, teller.Id);
            family.Chatted += OnOddChat;   // isim kardeşi, lakap, aynı kıyafet, dalgın dede, kulaktan kulağa
            // Fazla selam: aynı kişiyle günün üçüncü sohbeti (çocukların kovalamacası sayılmaz).
            family.Chatted += (a, b) =>
            {
                if (a.IsVillager || FamilyService.IsPlaymates(a, b)) return;
                if (!Hellos.OnChat(a.Id, b.Id, Clock.DayIndex)) return;
                Oddities.Mark(OddityIds.TooManyHellos);
                OddityLine?.Invoke(LocText.Of("core.oddity.hello", ("who", b.Name)), a.Id);
            };
        }

        private void AttachOddities(JobRunner runner)
        {
            runner.ItemUsed += OnRelicUsed;
            runner.ItemUsed += OnOddItemUsed;   // korkuluk, koyun saymak
            runner.WorkDone += OnOddWork;       // kulaktan kulağa
            runner.InteractionCompleted += (c, interaction) =>
            {
                if (c == null || c.IsVillager || interaction == null || !interaction.IsCraft) return;
                foreach (var p in interaction.Produce) Relics.OnCrafted(p.ItemId);
            };
        }

        /// <summary>Kalıntının "İncele", "Göm" ve "Al" etkileşimleri (items.json). Göm ve Al sahneyi atlamaz: önce inceleme oynar.</summary>
        private void OnRelicUsed(Character c, ItemInstance item, InteractionDef interaction)
        {
            if (c == null || item == null || interaction == null || c.IsVillager) return;
            var kind = FoundRelicService.KindOf(item.Def.Id);
            if (kind == RelicKind.None) return;
            bool tracked = item.Id == Relics.ItemId;
            if (kind == RelicKind.Sword && !tracked) return;
            bool inspect = interaction.Id == FoundRelicService.InspectInteraction;
            if (inspect || (tracked && Relics.Stage == RelicStage.Lying))
            {
                var line = Relics.Inspect(c, kind, ParentNearby(c), tracked);
                if (line != null)
                {
                    OddityLine?.Invoke(line.Line, c.Id);
                    if (!line.Reply.IsEmpty) OddityLine?.Invoke(line.Reply, c.Id);
                }
            }
            if (interaction.Id == FoundRelicService.BuryInteraction)
            {
                if (Relics.Bury(Clock.DayIndex)) OddityLine?.Invoke(LocText.Of("core.oddity.relic.buried", ("who", c.Name)), c.Id);
            }
            else if (interaction.Id == FoundRelicService.TakeInteraction)
            {
                if (kind == RelicKind.Sword)
                {
                    string got = Relics.Take();
                    if (got != null) OddityLine?.Invoke(LocText.Of("core.oddity.relic.taken", ("who", c.Name)), c.Id);
                }
                else if (Relics.TakeLater(kind, item.Id) != null)
                    OddityLine?.Invoke(LocText.Of("core.oddity.relic.taken_garden", ("who", c.Name)), c.Id);   // miğfer ve kalkan bahçe süsü olur
            }
        }

        private bool ParentNearby(Character child)
        {
            foreach (var m in Household())
            {
                if (m == child || m.Stage < AgeStage.Teen || m.Position.Z != child.Position.Z) continue;
                if (Math.Abs(m.Position.X - child.Position.X) + Math.Abs(m.Position.Y - child.Position.Y) <= 6) return true;
            }
            return false;
        }

        internal void OdditiesNewDay(long day)
        {
            AnimalTalk.OnNewDay();
            PruneKnocked();
            OddEventsNewDay(day);
            Relics.OnNewDay(day, Clock.Config.DaysPerSeason, Clock.Season == Season.Spring, HouseholdGeneration());
        }

        /// <summary>Kahve falı: çok nadiren dördüncü duvarı yıkar. Yıktıysa true (olağan fal söylenmez).</summary>
        private bool FortuneOddity(Character c)
        {
            if (!ForceFortuneOddity && _oddityRng.NextDouble() >= FortuneFourthWallChance) return false;
            ForceFortuneOddity = false;
            Oddities.Mark(OddityIds.FortuneFourthWall);
            OddityLine?.Invoke(LocText.Of("core.oddity.fortune", ("who", c.Name)), c.Id);
            return true;
        }

        /// <summary>Kurbağayı öp (yaban ziyaretçisi kurbağa): Hayal kırıklığı; çok nadiren minik taç. Taç belirdiyse true.</summary>
        public bool KissFrog(Character c, bool force = false, int visitorId = 0)
        {
            if (c == null) return false;
            bool crown = FrogKiss.Kiss(c, Oddities, _oddityRng.NextDouble(), force);
            if (crown && visitorId != 0)
            {
                // Taç kurbağanın başında görünür; kurbağa o sırada kaçmaz.
                Scenes.SetWildFx(visitorId, "crown", SceneBook.CrownDays);
                var frog = Roamers?.Find(visitorId);
                if (frog != null) { frog.Fleeing = false; frog.LeaveAt = Math.Max(frog.LeaveAt, Clock.TotalDays + SceneBook.CrownDays); }
            }
            OddityLine?.Invoke(LocText.Of(crown ? "core.oddity.frog.crown" : "core.oddity.frog.kiss", ("who", c.Name)), c.Id);
            return crown;
        }

        internal OdditySave ExportOddities() => new OdditySave
        {
            seen = Oddities.Export(),
            talks = AnimalTalk.Export(),
            relic = Relics.Export(),
            talkHistory = AnimalTalk.ExportHistory(),
            events = OddEvents.Export(),
            knocked = Scenes.ExportKnocked()
        };

        internal void ImportOddities(OdditySave data)
        {
            Oddities.Import(data?.seen);
            AnimalTalk.Import(data?.talks);
            Relics.Import(data?.relic);
            AnimalTalk.ImportHistory(data?.talkHistory);
            OddEvents.Import(data?.events);
            Scenes.ImportKnocked(data?.knocked);
            Walkers.Clear();
            _sceneQueue.Clear();
            // Johan Wick ziyareti sırasında kaydedildiyse ziyaret yüklemede tamamlanır (sikke kaybolmasın).
            if (OddEvents.Has(JohanFlag)) { _johanCount = 0; FinishJohan(null); }
        }
    }
}
