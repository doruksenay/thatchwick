using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Yaşayan köy, parti 1 (Project belgeleri 66d, 66e, 66f): köylülerin programı köy hayatı servisinden
    /// (<see cref="LivingVillage.PlaceFor"/>) gelir; özgür irade yalnızca programdaki ve izinli yerlerde, oyuncunun bulunduğu
    /// bölgede tam simülasyon, başka bölgelerde soyut hesap; işyerinde iş döngüsü pozları; kapıda teselli ve çağırma.
    /// </summary>
    public sealed partial class GameRoot
    {
        /// <summary>Köylünün programdaki yerine varmış sayıldığı uzaklık (karo).</summary>
        private const int VillagePlaceReach = 4;

        // Köylünün şu an kullanabileceği yerlerin karoları (özgür irade süzgeci; programla birlikte güncellenir).
        private readonly Dictionary<int, List<GridPos>> _villagerAnchors = new Dictionary<int, List<GridPos>>();
        // Köylünün yürüdüğü hedef karo (soyut hesap: hedef ve konum oyuncunun bölgesinde değilse yürümez, yerleşir).
        private readonly Dictionary<int, GridPos> _villagerTarget = new Dictionary<int, GridPos>();
        // Yer anahtarı → karo önbelleği (NearestWalkable yol arar); gün başında ve bölge değişince boşalır.
        private readonly Dictionary<string, GridPos?> _placeTiles = new Dictionary<string, GridPos?>(System.StringComparer.Ordinal);
        private long _placeTilesDay = -1;

        // Kapıda yapılacak iş (teselli, ortak kayıp, çağırma): komutan kapıya varınca yapılır.
        private enum DoorAction { None, Console, ShareLoss, Call }
        /// <summary>Çağrılan ebenin yardıma geldiği yer: oyuncunun evinin önü (belge 66e karar 9).</summary>
        private const string FarmhouseKey = LivingVillage.FarmhouseKey;
        private int _doorVillager;
        private DoorAction _doorAction;

        private void WireLivingVillage()
        {
            var lv = _life.Living;
            lv.PlaceAvailable = key => VillagePlaceTile(key).HasValue;
            lv.FullSimulation = c => _region == null || _region.Contains(c.Position);
            lv.AtPlace = (c, key) => NearPlace(c, key);
            _shopPanel.Village = lv;
            // Ölen ustanın halefi ailesiyle gelebilir (belge 66g karar 1): eş ve çocuklar da sahneye çıkar.
            _life.Villagers.FamilyMemberArrived += (member, head) => { GiveRandomLook(member); CreateActorFor(member); };
            _villagePanel.Door = (v, action) => VillageDoor(v, action);
        }

        /// <summary>Yer anahtarının karosu: ağaç ev, aşevi ve ulu ağaç Treehouses'tan; ev, işyeri, meydan Places'ten.</summary>
        private GridPos? VillagePlaceTile(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (_placeTilesDay != _clock.DayIndex) { _placeTiles.Clear(); _placeTilesDay = _clock.DayIndex; }
            if (_placeTiles.TryGetValue(key, out var cached)) return cached;
            GridPos? tile = null;
            var tree = _life.Trees.PlaceTile(key);
            if (key == FarmhouseKey) tile = _regional ? NearestWalkable(new GridPos(33, 31)) : (GridPos?)null;   // çiftlik evinin önü
            else if (tree.HasValue) tile = NearestWalkable(tree.Value, key == "feast" ? 6 : 3);
            else if (Places.ContainsKey(key) || Thatchwick.Core.Life.VillageHomes.IsInn(key)) tile = ResolvePlace(key);
            _placeTiles[key] = tile;
            return tile;
        }

        /// <summary>Köylünün yürüyeceği karo (meydanda herkes aynı karoya yığılmasın diye kişiye göre kaydırılır).</summary>
        private GridPos VillageTarget(string key, Character c)
        {
            if (key == "square") return ResolvePlace(key, c.Id);
            return VillagePlaceTile(key) ?? ResolvePlace(key);
        }

        private bool NearPlace(Character c, string key)
        {
            var t = VillagePlaceTile(key);
            if (!t.HasValue) return true;
            return t.Value.Z == c.Position.Z && System.Math.Max(System.Math.Abs(t.Value.X - c.Position.X), System.Math.Abs(t.Value.Y - c.Position.Y)) <= VillagePlaceReach;
        }

        /// <summary>Köylünün özgür iradesi yalnızca programdaki ve izinli yerlerdeki eşyaları puanlar; oyuncunun arsası asla.</summary>
        private bool VillagerMayUse(Character c, ItemInstance item) =>
            _villagerAnchors.TryGetValue(c.Id, out var anchors) && VillagerFreeWill.MayUse(_world, item, anchors);

        private void UpdateVillagerAnchors(Character c, string place)
        {
            var lv = _life.Living;
            if (!_villagerAnchors.TryGetValue(c.Id, out var list)) { list = new List<GridPos>(8); _villagerAnchors[c.Id] = list; }
            list.Clear();
            foreach (var key in VillagerFreeWill.AllowedPlaces(lv.HomeOf(c), lv.WorkOf(c), place))
            {
                var t = VillagePlaceTile(key);
                if (t.HasValue) list.Add(t.Value);
            }
        }

        /// <summary>Köylü soyut hesapta mı: kendisi de hedefi de oyuncunun bölgesinin dışında (yürümez, animasyon yok).</summary>
        private bool VillagerAbstract(Character c)
        {
            if (!c.IsVillager || _region == null || _region.Contains(c.Position)) return false;
            return !_villagerTarget.TryGetValue(c.Id, out var t) || !_region.Contains(t);
        }

        /// <summary>
        /// Bölgeye girildi: köylüler programdaki yerlerine yerleşir (yolda kalan, bölge dışında bekleyen hemen yerine geçer).
        /// </summary>
        private void SettleVillagers(Region region)
        {
            if (region == null || _life == null) return;
            foreach (var a in _actors)
            {
                var c = a.Character;
                if (c.IsDead || !c.IsVillager || c.Away) continue;
                if (c.Id == _life.Home.ServantId && _life.Home.InService) continue;
                string place = _life.Living.PlaceFor(c);
                if (string.IsNullOrEmpty(place)) continue;
                var tile = VillageTarget(place, c);
                if (!region.Contains(tile) || NearPlace(c, place)) continue;
                a.Runner.Cancel();
                c.Position = tile;
                _villagerLocation[c.Id] = place;
                _villagerTarget[c.Id] = tile;
                UpdateVillagerAnchors(c, place);
            }
            ResetSmoothing();
        }

        /// <summary>
        /// Köylünün çevre sıcaklığı: oyuncunun bölgesinde açıkta gerçek sıcaklık (kışın meydanda üşür, eve döner), kapalı yerde
        /// hanenin ocağı yanıyor sayılır (en az 15°); başka bölgede 15°.
        /// </summary>
        private float VillagerTemperature(Character c)
        {
            if (_heat == null || _region == null || !_region.Contains(c.Position)) return 15f;
            var room = _world.Map.Rooms.RoomAt(c.Position);
            float t = _heat.TemperatureAt(c.Position);
            return room != null && room.IsIndoor ? Mathf.Max(15f, t) : t;
        }

        // ---- pozlar (belge 66f §C.11) ----

        /// <summary>
        /// Köylünün yerinde oynayan karesi: öğün sahnesi (zincir pozları), işyerinde iş döngüsü, meyhanede çalgı. Karesi yoksa
        /// en yakın genel poza, o da yoksa adım pozlarının yedeğine düşer; hiçbiri yoksa null (duruş).
        /// </summary>
        private Sprite VillagePoseFrame(Actor actor, int seed)
        {
            var c = actor.Character;
            if (!c.IsVillager || !actor.Runner.IsIdle) return null;
            var lv = _life.Living;
            string pose = lv.ScenePose(c, _clock.TotalDays), fallback = null;
            if (pose == null && _villagerLocation.TryGetValue(c.Id, out var place) && NearPlace(c, place))
            {
                if (place == lv.WorkOf(c))
                {
                    var step = lv.WorkStep(c, _clock.TotalDays);
                    if (step != null) { pose = step.Pose; fallback = step.Fallback; }
                }
                else if (place == VillagerFreeWill.Tavern && c.Skills.GetLevel(SkillType.Music) >= 1) { pose = "play_music"; fallback = "fun"; }
            }
            if (pose == null) return null;
            float cyc = ActionCycle(seed);
            var f = IsoArt.CharacterAction(c, pose, actor.Direction, cyc);
            if (f == null && fallback != null) f = IsoArt.CharacterAction(c, fallback, actor.Direction, cyc);
            if (f == null && StepPoseStandIns.TryGetValue(pose, out var standIn) && standIn != "idle")
                f = IsoArt.CharacterAction(c, standIn, actor.Direction, cyc);
            return f;
        }

        // ---- kapıda: teselli, ortak kayıp, çağırma (belge 66e karar 7, 9) ----

        /// <summary>Komutan köylünün evinin kapısında mı.</summary>
        private bool AtVillagerHome(Character who, Character villager)
        {
            if (who == null || villager == null) return false;
            var t = VillagePlaceTile(_life.Living.HomeOf(villager));
            return t.HasValue && t.Value.Z == who.Position.Z
                && System.Math.Max(System.Math.Abs(t.Value.X - who.Position.X), System.Math.Abs(t.Value.Y - who.Position.Y)) <= VillagePlaceReach;
        }

        /// <summary>Köy panelinden: kapıdaysa hemen yapar, değilse komutan kapıya yürür ve varınca yapar.</summary>
        private string VillageDoor(Character villager, int action)
        {
            var actor = Commander;
            if (actor == null || villager == null) return null;
            if (AtVillagerHome(actor.Character, villager)) return DoAtDoor(actor.Character, villager, (DoorAction)action);
            var t = VillagePlaceTile(_life.Living.HomeOf(villager));
            if (!t.HasValue) return null;
            actor.Runner.Command(Job.MoveTo(t.Value));
            _doorVillager = villager.Id;
            _doorAction = (DoorAction)action;
            return GameLocalization.T("ui.village.walking_door", ("name", villager.Name));
        }

        /// <summary>Kapıya varan komutanın bekleyen işi (program yenilenirken denetlenir).</summary>
        private void CheckVillageDoor()
        {
            if (_doorAction == DoorAction.None) return;
            var actor = Commander;
            var v = _population.Find(_doorVillager);
            if (actor == null || v == null || v.IsDead) { _doorAction = DoorAction.None; return; }
            if (!actor.Runner.IsIdle || !AtVillagerHome(actor.Character, v)) { if (actor.Runner.IsIdle) _doorAction = DoorAction.None; return; }
            var act = _doorAction;
            _doorAction = DoorAction.None;
            var msg = DoAtDoor(actor.Character, v, act);
            if (!string.IsNullOrEmpty(msg)) ShowMessage(msg, 4f);
        }

        private string DoAtDoor(Character who, Character villager, DoorAction action)
        {
            var lv = _life.Living;
            Sfx(Thatchwick.Core.Audio.SoundKeys.DoorKnock, _view.TileToWorld(who.Position), who.Id);
            Thatchwick.Core.Localization.LocText msg;
            switch (action)
            {
                case DoorAction.Console:
                    lv.Console(who, villager, true, out msg);
                    return GameLocalization.R(msg);
                case DoorAction.ShareLoss:
                    if (lv.ShareLoss(who, villager, true, out msg, out var talk))
                    {
                        // Kısa karşılıklı konuşma: satırlar sırayla balon olur (her biri bir öncekinden sonra).
                        float at = Time.unscaledTime;
                        foreach (var (speaker, line) in talk)
                        {
                            string text = GameLocalization.R(line);
                            _speechQueue.Add((speaker.Id, text, at));
                            at += BubbleSeconds(text);
                        }
                    }
                    return GameLocalization.R(msg);
                case DoorAction.Call:
                    bool home = _villagerLocation.TryGetValue(villager.Id, out var place) && place == lv.HomeOf(villager) && NearPlace(villager, place);
                    // Ebe: hanede gebe biri varsa doğrudan yardıma gelir (evin önüne); ötekiler dükkânı oyuncu için açar.
                    bool midwife = lv.ShopOf(villager)?.Id == "herb" && HouseholdExpecting();
                    lv.Call(who, villager, home, out msg, midwife ? FarmhouseKey : null);
                    _villagerLocation.Remove(villager.Id);   // program hemen yenilensin: usta dükkânına yürür
                    return GameLocalization.R(msg);
            }
            return null;
        }

        private bool HouseholdExpecting()
        {
            foreach (var m in _population.Members) if (!m.IsDead && !m.IsVillager && m.IsPregnant) return true;
            return false;
        }

        // Dükkân kapısının çanı: komutan bir dükkânın içine girince bir kez çalar.
        private string _lastShopArea;
        private Dictionary<string, Thatchwick.Core.World.RectI> _shopAreas;

        private void RingShopBell()
        {
            var who = Commander?.Character;
            if (who == null || !_regional) return;
            if (_shopAreas == null) _shopAreas = Regions.ShopAreas();
            string inside = null;
            foreach (var kv in _shopAreas) if (kv.Value.Contains(who.Position)) { inside = kv.Key; break; }
            if (inside != null && inside != _lastShopArea) Sfx(Thatchwick.Core.Audio.SoundKeys.ShopBell, _view.TileToWorld(who.Position), who.Id);
            _lastShopArea = inside;
        }

        // Sıradaki balonlar (konuşan, metin, başlama zamanı): ortak kayıp konuşması.
        private readonly List<(int speaker, string text, float at)> _speechQueue = new List<(int, string, float)>();

        private void FlushSpeechQueue()
        {
            for (int i = _speechQueue.Count - 1; i >= 0; i--)
            {
                var (speaker, text, at) = _speechQueue[i];
                if (Time.unscaledTime < at) continue;
                _speech[speaker] = (text, Time.unscaledTime + BubbleSeconds(text));
                _speechQueue.RemoveAt(i);
            }
        }
    }
}
