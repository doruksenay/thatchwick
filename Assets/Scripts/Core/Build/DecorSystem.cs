using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Build
{
    /// <summary>Bir rafın çökmesinin dökümü (bildirim ve testler için).</summary>
    public sealed class CollapseReport
    {
        public ItemInstance Surface;
        public int Broken;
        public int Damaged;
        public ItemInstance Debris;
        public string Text;
    }

    /// <summary>
    /// Ev dekorasyonunun zamanla işleyen kuralları (Project belgesi 27, D1):
    /// - Aşırı yüklenen raf çöker (karar 18.11.2026): yük taşımayı aşınca her oyun saatinde aşım oranının iki katı
    ///   (en az %10) şansla. Kil ve cam düşünce kırılır, diğerleri hasar alır; duvar rafı duvardan kopar. Düşenler
    ///   önündeki karoda döküntü yığını olur ("Topla" ile envantere gelir). Odadakiler "Ödüm koptu" der.
    /// - Metal ayna kararır (güzelliği yarıya iner; "Cilala" düzeltir).
    /// - Ergenlikteki ücretsiz Ayna hakkı, evde ayna yoksa bir kez hatırlatılır.
    /// Sayılar yer tutucudur (Adım 28).
    /// </summary>
    public sealed class DecorSystem
    {
        public const double HourDays = 1.0 / 24.0;
        public const double MinCollapseChance = 0.10;
        public const string ScareMoodId = "mood.shelf_fell";
        public const int ScareMoodValue = -5;
        public const double ScareMoodDays = 0.1;

        private readonly WorldState _world;
        private readonly ContentDatabase _content;
        private readonly Random _rng;
        private double _hourAccum;
        private readonly HashSet<int> _mirrorReminded = new HashSet<int>();

        /// <summary>Çökme zarı (0-1); testler sabit değer verebilir. Null ise tohumlu rastgele.</summary>
        public Func<double> Roll { get; set; }
        /// <summary>Hane üyeleri (moodlet ve ayna hatırlatması); null ise yok.</summary>
        public Func<IEnumerable<Character>> Members { get; set; }
        /// <summary>Döküntü konamazsa düşenlerin gideceği envanter; null ise kaybolur.</summary>
        public Household House { get; set; }
        /// <summary>Bildirim (metin, karakter kimliği; 0 = genel).</summary>
        public Action<string, int> Notify { get; set; }
        /// <summary>Bir raf çöktü.</summary>
        public event Action<CollapseReport> Collapsed;

        public DecorSystem(WorldState world, ContentDatabase content, int seed = 1)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _rng = new Random(seed);
        }

        private double NextRoll() => Roll != null ? Roll() : _rng.NextDouble();

        /// <summary>Saatlik çökme şansı: aşım oranının iki katı, en az %10, en çok %100. Aşım yoksa 0.</summary>
        public static double CollapseChance(int load, int maxLoad)
        {
            if (maxLoad <= 0 || load <= maxLoad) return 0;
            double over = (double)load / maxLoad - 1.0;
            return Math.Min(1.0, Math.Max(MinCollapseChance, 2.0 * over));
        }

        public void Tick(double gameDays)
        {
            if (gameDays <= 0) return;
            foreach (var it in _world.Items)
                if (it.Def.TarnishDays > 0f) it.TarnishDays += gameDays;

            _hourAccum += gameDays;
            while (_hourAccum >= HourDays)
            {
                _hourAccum -= HourDays;
                CheckOverloads();
            }
        }

        /// <summary>Aşırı yüklü her yüzey için çökme zarı atar.</summary>
        public void CheckOverloads()
        {
            var overloaded = new List<ItemInstance>();
            foreach (var it in _world.Items)
                if (!it.IsDecor && it.Def.MaxLoad > 0 && it.Condition != ItemCondition.Damaged && _world.IsOverloaded(it)) overloaded.Add(it);
            foreach (var s in overloaded)
            {
                if (_world.GetItem(s.Id) == null) continue;
                double chance = CollapseChance(_world.LoadOf(s.Id), s.Def.MaxLoad);
                if (NextRoll() < chance) Collapse(s);
            }
        }

        /// <summary>Rafı çökertir (testler ve olaylar doğrudan çağırabilir).</summary>
        public CollapseReport Collapse(ItemInstance surface)
        {
            if (surface == null || _world.GetItem(surface.Id) == null) return null;
            var report = new CollapseReport { Surface = surface };
            var fallen = new List<StoredItem>();
            foreach (var d in _world.GetDecorOn(surface.Id))
            {
                _world.RemoveSingle(d);
                string mat = d.Def.EffectiveMaterial;
                if (Decor.IsFragile(mat))
                {
                    string shard = mat == Decor.Glass ? Decor.BrokenGlass : Decor.BrokenPottery;
                    if (_content.Items.Contains(shard)) fallen.Add(new StoredItem(shard, ItemCondition.Sound));
                    report.Broken++;
                }
                else
                {
                    fallen.Add(new StoredItem(d.Def.Id, ItemCondition.Damaged));
                    report.Damaged++;
                }
            }
            var tile = surface.Position;
            string what = surface.Def.DisplayName;
            if (surface.IsWallMounted)
            {
                // Duvar rafı duvardan kopar: hasarlı olarak döküntüye düşer.
                _world.RemoveSingle(surface);
                fallen.Add(new StoredItem(surface.Def.Id, ItemCondition.Damaged));
            }
            else surface.Condition = ItemCondition.Damaged;

            report.Debris = Drop(tile, fallen);
            var parts = new List<string>();
            if (report.Broken > 0) parts.Add($"{report.Broken} eşya kırıldı");
            if (report.Damaged > 0) parts.Add($"{report.Damaged} eşya hasar aldı");
            report.Text = $"{what} aşırı yükten çöktü" + (parts.Count > 0 ? ": " + string.Join(", ", parts) : "") + ".";
            _world.RaiseChanged();
            Scare(tile);
            Notify?.Invoke(report.Text, 0);
            Collapsed?.Invoke(report);
            return report;
        }

        /// <summary>Düşenleri karodaki döküntü yığınına ekler; yoksa yenisini serer. Serilemezse envantere verir.</summary>
        public ItemInstance Drop(GridPos tile, List<StoredItem> fallen)
        {
            if (fallen == null || fallen.Count == 0) return null;
            ItemInstance pile = null;
            foreach (var rug in _world.RugsAt(tile)) if (rug.Def.Id == Decor.Debris) pile = rug;
            if (pile == null && _content.Items.TryGet(Decor.Debris, out var debrisDef) && _world.CanPlaceRug(debrisDef, tile, 0).Ok)
                pile = _world.PlaceItem(debrisDef, tile, 0);
            if (pile == null)
            {
                if (House != null) foreach (var f in fallen) House.AddItem(f.ItemId, 1, f.Condition);
                return null;
            }
            pile.Contents.AddRange(fallen);
            return pile;
        }

        private void Scare(GridPos tile)
        {
            if (Members == null) return;
            var room = _world.Map.Rooms.RoomAt(tile);
            if (room == null) return;
            foreach (var c in Members())
            {
                if (c.IsDead || !c.MoodEnabled) continue;
                if (_world.Map.Rooms.RoomAt(c.Position) == room) c.AddMoodlet(ScareMoodId, "Ödüm koptu", ScareMoodValue, ScareMoodDays);
            }
        }

        // ---- ayna ----

        /// <summary>Evde (dünyada) bir ayna var mı: Mirror (görünüş değiştirme) bunu ister (karar 18.11.2026).</summary>
        public static bool HasMirror(WorldState world)
        {
            if (world == null) return false;
            foreach (var it in world.Items) if (it.Def.IsMirror) return true;
            return false;
        }

        /// <summary>Ücretsiz Ayna hakkı olup evde aynası olmayanları bir kez hatırlatır (yeni günde çağrılır).</summary>
        public void RemindFreeMirror()
        {
            if (Members == null || HasMirror(_world)) return;
            foreach (var c in Members())
            {
                if (c.IsDead || !c.FreeMirrorAvailable || _mirrorReminded.Contains(c.Id)) continue;
                _mirrorReminded.Add(c.Id);
                Notify?.Invoke($"{c.Name} ücretsiz Ayna hakkı kazandı, ama evde ayna yok. Bir ayna asınca hakkını kullanabilir.", c.Id);
            }
        }

        public static string MaterialName(string material)
        {
            switch (material)
            {
                case Decor.Wood: return "ahşap";
                case Decor.Metal: return "metal";
                case Decor.Cloth: return "kumaş";
                case Decor.Clay: return "kil";
                case Decor.Glass: return "cam";
                case Decor.Stone: return "taş";
                default: return material;
            }
        }
    }
}
