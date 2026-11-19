using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Family;
using Thatchwick.Core.Mood;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Kazanılmış bir hatıranın kaydı: ne, kimin, hangi nesilde. Yerleştirilince eşya örneğine bağlanır.</summary>
    public sealed class KeepsakeRecord
    {
        public string ItemId;
        public string Label;
        public int Generation;
        public double Day;
        /// <summary>Bağlı olduğu yerleştirilmiş eşya (0 = envanterde).</summary>
        public int InstanceId;
    }

    [Serializable]
    public sealed class KeepsakeRecordSave
    {
        public string item, label;
        public int gen, instance;
        public double day;
    }

    [Serializable]
    public sealed class KeepsakeSave
    {
        public KeepsakeRecordSave[] records;
        public string[] firsts;
    }

    /// <summary>
    /// Ev dekorasyonu D3b (Project belgesi 27 §7.7, §8.2, §8.4): ilerleme dekora dönüşür.
    /// - Hatıralar satın alınmaz, kazanılır: şenlik kupası, hasat kurdelesi, ilk balık, bebeğin ilk ayakkabısı, yaşlının kalaylı kupası.
    ///   Her hatıranın yazısı (kimin, hangi yıl) defterde durur; eşya yerleşince ona bağlanır. Nesiller geçtikçe güzelliği artar.
    /// - Soy ağacı işlemesi: nesil arttıkça dallanır (güzellik), asılı olduğu evde aileye "Köklerimiz" verir ve yası kısaltır.
    /// - Çeyiz sandığı: evlenip komşu haneye giden kız evdeki çeyiz sandığını götürür; doluysa Soy Mührü ve yakınlık.
    ///   Haneye gelin gelince çeyiz sandığı ve dokumalar gelir.
    /// </summary>
    public sealed class KeepsakeService
    {
        public const string FestivalCup = "item.festival_cup";
        public const string HarvestRibbon = "item.harvest_ribbon";
        public const string FirstFish = "item.first_fish_scale";
        public const string FirstShoes = "item.baby_first_shoes";
        public const string ElderCup = "item.elder_tin_cup";
        public const string FamilyTree = "item.family_tree_embroidery";
        public const string DowryChest = "item.dowry_chest";

        /// <summary>Hatıra her nesilde +1 güzellik kazanır, en çok bu kadar.</summary>
        public const int MaxAgeBonus = 3;
        /// <summary>Soy ağacı her nesilde +1 güzellik (ilk nesil 0), en çok bu kadar.</summary>
        public const int MaxTreeBonus = 6;

        public const string RootsMoodId = "mood.roots";
        public const int RootsMood = 4;
        public const double RootsMoodDays = 1.1;
        /// <summary>Soy ağacı asılı evde yas günde bu kadar fazla azalır (yas süresi ~1/3 kısalır).</summary>
        public const double RootsGriefSpeed = 0.5;

        public const string DowryMoodId = "mood.dowry_full";
        public const int DowryFullSeals = 1;
        public const int DowryAffection = 10;

        /// <summary>Haneye gelen gelinin çeyizi: sandık ve dokumalar (envantere).</summary>
        public static readonly (string id, int count)[] BrideDowry =
        {
            ("item.linen", 2), ("item.cloth", 2), ("item.felt", 1), ("item.yarn", 3), ("item.embroidery_panel", 1)
        };

        private readonly WorldState _world;
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly List<KeepsakeRecord> _records = new List<KeepsakeRecord>();
        private readonly HashSet<string> _firsts = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>Hanedanın şu anki nesli; null ise 1.</summary>
        public Func<int> Generation { get; set; }
        /// <summary>Oyun yılı (1'den); null ise 1.</summary>
        public Func<long> Year { get; set; }
        /// <summary>Şu anki gün (kayıt için); null ise 0.</summary>
        public Func<double> Today { get; set; }
        /// <summary>Soy ağacı yazısı için bütün kişiler (ölüler dahil); null ise yazıda yalnızca nesil.</summary>
        public Func<IEnumerable<Character>> People { get; set; }
        /// <summary>Bildirim (metin, karakter kimliği).</summary>
        public Action<string, int> Notify { get; set; }

        public event Action<KeepsakeRecord> Earned;

        public IReadOnlyList<KeepsakeRecord> Records => _records;

        public KeepsakeService(WorldState world, ContentDatabase content, Household house)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _house = house ?? throw new ArgumentNullException(nameof(house));
        }

        private int Gen() => Generation != null ? Math.Max(1, Generation()) : 1;
        private long YearNow() => Year != null ? Math.Max(1, Year()) : 1;
        private string Name(string id) => _content.Items.TryGet(id, out var d) ? d.DisplayName : id;

        // ---------------- kazanma ----------------

        /// <summary>Hatırayı envantere koyar ve deftere yazar. İçerikte yoksa null.</summary>
        public KeepsakeRecord Grant(string itemId, string label, int characterId = 0)
        {
            if (!_content.Items.Contains(itemId)) return null;
            _house.AddItem(itemId);
            var rec = new KeepsakeRecord { ItemId = itemId, Label = label, Generation = Gen(), Day = Today != null ? Today() : 0 };
            _records.Add(rec);
            Notify?.Invoke($"Hatıra kazanıldı: {label}. Envanterde; bir rafa ya da ocak başına koy.", characterId);
            Earned?.Invoke(rec);
            return rec;
        }

        /// <summary>Bir "ilk" daha önce yaşanmadıysa işaretler ve true döner.</summary>
        public bool MarkFirst(string key) => _firsts.Add(key);
        public bool HasFirst(string key) => _firsts.Contains(key);

        /// <summary>Şenlik kutlandı (katılan varsa): şenlik kupası.</summary>
        public KeepsakeRecord OnFestival(string festivalName, int attended)
        {
            if (attended <= 0) return null;
            return Grant(FestivalCup, $"{festivalName} kupası ({YearNow()}. yıl)");
        }

        /// <summary>Birinci sınıf hasat: yılın ilk birinci sınıf hasadı kurdele kazanır.</summary>
        public KeepsakeRecord OnCropHarvested(Character c, ItemInstance crop, int tier)
        {
            if (c == null || c.IsVillager || crop == null || tier < 2) return null;
            if (!MarkFirst($"harvest:{YearNow()}")) return null;
            return Grant(HarvestRibbon, $"Hasat kurdelesi: birinci sınıf {crop.Def.DisplayName.ToLowerInvariant()} ({YearNow()}. yıl, {c.Name})", c.Id);
        }

        /// <summary>Hane üyesinin ilk balığı: çerçeveli pul.</summary>
        public KeepsakeRecord OnFishCaught(Character c, string fishId)
        {
            if (c == null || c.IsVillager) return null;
            if (!MarkFirst($"fish:{c.Id}")) return null;
            return Grant(FirstFish, $"{c.Name}: ilk balık ({Name(fishId).ToLowerInvariant()})", c.Id);
        }

        /// <summary>Hanede bebek doğdu: ilk ayakkabı.</summary>
        public KeepsakeRecord OnBirth(Character baby)
        {
            if (baby == null || baby.IsVillager) return null;
            return Grant(FirstShoes, $"{baby.Name}: ilk ayakkabı ({baby.Generation}. nesil)", baby.Id);
        }

        /// <summary>Hanenin yaşlısı öldü: kalaylı kupası hatıra kalır.</summary>
        public KeepsakeRecord OnDeath(Character dead)
        {
            if (dead == null || dead.IsVillager || dead.Stage != AgeStage.Elder) return null;
            return Grant(ElderCup, $"{dead.Name}: kalaylı kupa ({dead.Generation}. nesil)", dead.Id);
        }

        // ---------------- yazı ve bağlama ----------------

        /// <summary>
        /// Yerleştirilmiş hatıranın kaydı: önce bağlı kayıt, yoksa aynı türden envanterde bekleyen ilk kayıt bağlanır.
        /// Hatıra değilse null.
        /// </summary>
        public KeepsakeRecord RecordOf(ItemInstance item)
        {
            if (item == null || !item.Def.IsKeepsake) return null;
            foreach (var r in _records) if (r.InstanceId == item.Id && r.ItemId == item.Def.Id) return r;
            foreach (var r in _records)
            {
                if (r.ItemId != item.Def.Id || IsBound(r)) continue;
                r.InstanceId = item.Id;
                return r;
            }
            return null;
        }

        private bool IsBound(KeepsakeRecord r)
        {
            if (r.InstanceId == 0) return false;
            var it = _world.GetItem(r.InstanceId);
            return it != null && it.Def.Id == r.ItemId;
        }

        /// <summary>Eşya kartında ve etikette görünen yazı (hatıra ya da soy ağacı); yoksa null.</summary>
        public string LabelOf(ItemInstance item)
        {
            if (item == null) return null;
            if (item.Def.Id == FamilyTree) return TreeLabel();
            return RecordOf(item)?.Label;
        }

        /// <summary>
        /// Defteri dünya ve envanterle eşler: kalkmış eşyanın kaydı envantere döner; envanterde artık olmayan
        /// (atılmış) hatıraların fazlası silinir (en yenisi önce).
        /// </summary>
        public void Reconcile()
        {
            foreach (var r in _records) if (r.InstanceId != 0 && !IsBound(r)) r.InstanceId = 0;
            var defs = new HashSet<string>(StringComparer.Ordinal);
            foreach (var r in _records) defs.Add(r.ItemId);
            foreach (var id in defs)
            {
                int unbound = 0;
                foreach (var r in _records) if (r.ItemId == id && r.InstanceId == 0) unbound++;
                int extra = unbound - _house.Count(id);
                for (int i = _records.Count - 1; i >= 0 && extra > 0; i--)
                    if (_records[i].ItemId == id && _records[i].InstanceId == 0) { _records.RemoveAt(i); extra--; }
            }
        }

        // ---------------- güzellik ve soy ağacı ----------------

        /// <summary>Güzellik eki: hatıra her geçen nesilde +1 (en çok 3), soy ağacı her nesilde +1 (en çok 6).</summary>
        public double BeautyBonus(ItemInstance item)
        {
            if (item == null) return 0;
            if (item.Def.Id == FamilyTree) return Math.Min(MaxTreeBonus, Gen() - 1);
            if (!item.Def.IsKeepsake) return 0;
            var r = RecordOf(item);
            return r != null ? Math.Min(MaxAgeBonus, Math.Max(0, Gen() - r.Generation)) : 0;
        }

        /// <summary>Soy ağacının yazısı: "3 nesil · 11 dal, 4'ü solgun iplikle".</summary>
        public string TreeLabel()
        {
            string text = $"Soy ağacı: {Gen()} nesil";
            if (People == null) return text;
            int all = 0, dead = 0;
            foreach (var c in People())
            {
                if (c.IsVillager) continue;   // köylüler (komşu haneye gidenler dahil) bu evin ağacında sayılmaz
                all++;
                if (c.IsDead) dead++;
            }
            return dead > 0 ? $"{text} · {all} dal, {dead}'i solgun iplikle" : $"{text} · {all} dal";
        }

        /// <summary>Evde (kapalı iç odada) asılı bir soy ağacı var mı?</summary>
        public bool HasFamilyTree()
        {
            foreach (var it in _world.Items)
            {
                if (it.Def.Id != FamilyTree || !it.IsWallMounted) continue;
                var room = _world.Map.Rooms.RoomAt(it.Position);
                if (room != null && room.IsIndoor) return true;
            }
            return false;
        }

        /// <summary>
        /// Günlük: defteri eşle; soy ağacı asılıysa hane üyelerine "Köklerimiz" ver ve yası kısalt.
        /// </summary>
        public void Daily(IEnumerable<Character> household, double days = 1.0)
        {
            Reconcile();
            bool tree = HasFamilyTree();
            foreach (var c in household)
            {
                if (c.IsDead || c.IsVillager) continue;
                if (!tree) { c.Mood.Remove(RootsMoodId); continue; }
                c.AddMoodlet(RootsMoodId, "Köklerimiz", RootsMood, RootsMoodDays);
                foreach (var m in c.Mood.Active)
                    if (m.Def.Id == MoodCatalog.Grief) { m.RemainingDays = Math.Max(0.01, m.RemainingDays - RootsGriefSpeed * days); break; }
            }
        }

        // ---------------- çeyiz sandığı ----------------

        /// <summary>Evdeki en dolu (kurulu) çeyiz sandığının doluluğu; sandık yoksa (0, 0).</summary>
        public (int filled, int max) BestDowryFill()
        {
            ItemInstance chest = null;
            foreach (var it in _world.Items)
                if (it.Def.Id == DowryChest && !it.IsDecor && (chest == null || it.Contents.Count > chest.Contents.Count)) chest = it;
            return chest == null ? (0, 0) : (chest.Contents.Count, chest.Def.FillCount);
        }

        /// <summary>
        /// Hane kızı komşu haneye gelin gitti: evdeki en dolu çeyiz sandığı onunla gider (yoksa envanterdeki boş sandık).
        /// Tam doluysa Soy Mührü +1, hanenin eşe yakınlığı +10 ve gelinde "Çeyizim tam". Bildirim metni ya da null.
        /// </summary>
        public string OnMarriedOut(Character member, Character spouse, FamilyService family = null, Legacy legacy = null, IEnumerable<Character> household = null)
        {
            if (member == null || member.Sex != Sex.Female) return null;
            ItemInstance chest = null;
            foreach (var it in _world.Items)
                if (it.Def.Id == DowryChest && !it.IsDecor && (chest == null || it.Contents.Count > chest.Contents.Count)) chest = it;
            string msg;
            if (chest != null)
            {
                int n = chest.Contents.Count, max = chest.Def.FillCount;
                _world.RemoveItem(chest.Id);   // içindekilerle birlikte gider
                if (n >= max && max > 0)
                {
                    legacy?.Award(SealSource.MarriedOut, DowryFullSeals, Today != null ? Today() : 0, $"{member.Name} dolu çeyiz sandığıyla gitti");
                    if (family != null && spouse != null && household != null)
                        foreach (var m in household)
                            if (!m.IsDead && !m.IsVillager && m.Id != member.Id)
                                family.SetAffection(m.Id, spouse.Id, family.Affection(m.Id, spouse.Id) + DowryAffection);
                    member.AddMoodlet(DowryMoodId, "Çeyizim tam", 8, 3.0);
                    msg = $"{member.Name} dolu çeyiz sandığıyla ({n}/{max}) gelin gitti; yeni hanede itibar ve yakınlık.";
                }
                else msg = n > 0 ? $"{member.Name} yarım çeyiz sandığıyla ({n}/{max}) gelin gitti." : $"{member.Name} boş çeyiz sandığıyla gelin gitti.";
            }
            else if (_house.TryTakeItem(DowryChest)) msg = $"{member.Name} boş çeyiz sandığıyla gelin gitti.";
            else return null;
            Notify?.Invoke(msg, member.Id);
            return msg;
        }

        /// <summary>Haneye gelin geldi: çeyiz sandığı ve dokumaları envantere gelir. Bildirim metni ya da null.</summary>
        /// <summary>Gelinin ailesinin hâli: 1 sıradan, 2 nitelikli talip (ailesi itibarlı). Çeyiz dokumaları bu katla gelir.</summary>
        public Func<int> DowryScale { get; set; }

        public string OnJoined(Character bride)
        {
            int k = Math.Max(1, DowryScale != null ? DowryScale() : 1);
            if (bride == null || bride.Sex != Sex.Female || !_content.Items.Contains(DowryChest)) return null;
            _house.AddItem(DowryChest);
            var parts = new List<string>();
            foreach (var (id, count) in BrideDowry)
            {
                if (!_content.Items.Contains(id)) continue;
                _house.AddItem(id, count * k);
                parts.Add($"{count * k} {Name(id).ToLowerInvariant()}");
            }
            string msg = $"{bride.Name} {(k > 1 ? "dolgun bir " : "")}çeyiz sandığıyla geldi: {string.Join(", ", parts)}. Sandığı evine koyup doldurabilirsin.";
            Notify?.Invoke(msg, bride.Id);
            return msg;
        }

        // ---------------- kayıt ----------------

        public KeepsakeSave Export()
        {
            var list = new List<KeepsakeRecordSave>();
            foreach (var r in _records)
                list.Add(new KeepsakeRecordSave { item = r.ItemId, label = r.Label, gen = r.Generation, day = r.Day, instance = r.InstanceId });
            return new KeepsakeSave { records = list.ToArray(), firsts = new List<string>(_firsts).ToArray() };
        }

        public void Restore(KeepsakeSave data)
        {
            _records.Clear();
            _firsts.Clear();
            if (data == null) return;
            if (data.records != null)
                foreach (var r in data.records)
                    if (r != null && !string.IsNullOrEmpty(r.item))
                        _records.Add(new KeepsakeRecord { ItemId = r.item, Label = r.label, Generation = Math.Max(1, r.gen), Day = r.day, InstanceId = r.instance });
            if (data.firsts != null) foreach (var f in data.firsts) if (!string.IsNullOrEmpty(f)) _firsts.Add(f);
        }
    }
}
