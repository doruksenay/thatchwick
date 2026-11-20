using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    // Easter egg sahneleri (Project belgesi 55, kullanıcı 20.11.2026): haritada görünen ziyaretçiler, hayvan pozları,
    // karakter gösterileri ve kedinin yere düşürdüğü eşya. Kurallar ve durum burada; çizim oyun katmanında.

    /// <summary>Crier: tellal (karar 20.11.2026); easter egg değil, her sabah yeni ilan varsa panonun yanında durur.</summary>
    public enum OddWalkerKind { JohanWick = 0, HorselessKnight = 1, Crier = 2 }

    /// <summary>
    /// Haritada yürüyen easter egg ziyaretçisi. Nüfusa girmez (evlenmez, yaşlanmaz, hane sayılmaz); yaban ziyaretçileri gibi
    /// yalnızca yürür ve sahnesini oynar. Görsel takımı oyun katmanında (iş 496: iso_char_johan_wick_r84, iso_char_horseless_knight_r84).
    /// </summary>
    public sealed class OddWalker
    {
        public int Id;
        public OddWalkerKind Kind;
        public GridPos Position;
        public GridPos? NextTile;
        public double StepProgress;
        public List<GridPos> Path;
        /// <summary>Yerinde oynanan hareket ("pet", "wreck"); yürürken null.</summary>
        public string Action;
        public double ActionLeft;
        /// <summary>Hareketin yöneldiği karo (yüzünü oraya döner).</summary>
        public GridPos? Facing;
        /// <summary>Senaryo adımı (LifeSystems yürütür).</summary>
        public int Phase;
        public bool Gone;
    }

    public sealed class OddWalkers
    {
        /// <summary>Ziyaretçi yürüyüş hızı (karo/oyun günü); karakter yürüyüşüne yakın.</summary>
        public const double StepsPerDay = 260;
        private readonly List<OddWalker> _walkers = new List<OddWalker>();
        private int _nextId = 1;

        public IReadOnlyList<OddWalker> Walkers => _walkers;
        /// <summary>Yolun sonuna varıldı ya da hareket bitti: senaryonun sıradaki adımı.</summary>
        public event Action<OddWalker> Ready;

        public OddWalker Spawn(OddWalkerKind kind, GridPos at)
        {
            var w = new OddWalker { Id = _nextId++, Kind = kind, Position = at };
            _walkers.Add(w);
            return w;
        }

        public OddWalker Find(OddWalkerKind kind) { foreach (var w in _walkers) if (w.Kind == kind && !w.Gone) return w; return null; }

        /// <summary>Bu karoda (ya da adım attığı karoda) duran ziyaretçi; oyuncu dokununca adı görünür.</summary>
        public OddWalker At(GridPos tile)
        {
            foreach (var w in _walkers) if (!w.Gone && (w.Position == tile || w.NextTile == tile)) return w;
            return null;
        }

        public void Walk(OddWalker w, List<GridPos> path)
        {
            w.Path = path != null && path.Count > 0 ? path : null;
            w.Action = null; w.ActionLeft = 0; w.Facing = null;
        }

        public void Act(OddWalker w, string action, double days, GridPos? facing = null)
        {
            w.Path = null; w.NextTile = null; w.StepProgress = 0;
            w.Action = action; w.ActionLeft = days; w.Facing = facing;
        }

        public void Tick(double gameDays)
        {
            if (gameDays <= 0) return;
            for (int i = _walkers.Count - 1; i >= 0; i--)
            {
                var w = _walkers[i];
                if (w.Gone) { _walkers.RemoveAt(i); continue; }
                if (w.Action != null)
                {
                    w.ActionLeft -= gameDays;
                    if (w.ActionLeft > 0) continue;
                    w.Action = null;
                    Ready?.Invoke(w);
                    continue;
                }
                if (w.NextTile == null)
                {
                    if (w.Path == null || w.Path.Count == 0) { w.Path = null; Ready?.Invoke(w); continue; }
                    w.NextTile = w.Path[0];
                    w.Path.RemoveAt(0);
                    w.StepProgress = 0;
                }
                w.StepProgress += gameDays * StepsPerDay;
                for (int guard = 0; guard < 64 && w.StepProgress >= 1 && w.NextTile.HasValue; guard++)
                {
                    w.StepProgress -= 1;
                    w.Position = w.NextTile.Value;
                    if (w.Path != null && w.Path.Count > 0) { w.NextTile = w.Path[0]; w.Path.RemoveAt(0); }
                    else { w.NextTile = null; w.Path = null; w.StepProgress = 0; Ready?.Invoke(w); }
                }
            }
            _walkers.RemoveAll(x => x.Gone);
        }

        public void Clear() => _walkers.Clear();
    }

    /// <summary>Hayvanın kısa süreli pozu ("howl", "stare", "box"): yalnızca hayvan o karoda dururken geçerlidir.</summary>
    public sealed class AnimalPose
    {
        public string Pose;
        public GridPos At;
        public double Until;
        /// <summary>Kutuya giren kedi için: üstüne çizileceği eşya (yoksa 0).</summary>
        public int ItemId;
    }

    /// <summary>Kedinin yere düşürdüğü süs: eşya dünyada yerinde durur (kalite, yadigâr, hatıra kaydı bozulmaz), yalnızca yerde çizilir.</summary>
    public sealed class KnockedItem
    {
        public int ItemId;
        public GridPos Floor;
    }

    /// <summary>
    /// Sahne durumu: hayvan pozları, yaban hayvanı efektleri (taç), karakter gösterileri (topaç, uluma) ve yere düşen süsler.
    /// Pozlar ve efektler kayda yazılmaz (kısa ömürlü); düşen süsler yazılır.
    /// </summary>
    public sealed class SceneBook
    {
        public const double HowlDays = 0.02, StareDays = 0.06, BoxDays = 0.12, CrownDays = 0.05, TopShowDays = 0.02, HowlCharacterDays = 0.015;

        private readonly Dictionary<int, AnimalPose> _poses = new Dictionary<int, AnimalPose>();
        private readonly Dictionary<int, (string fx, double until)> _wildFx = new Dictionary<int, (string, double)>();
        private readonly Dictionary<int, (string key, double until)> _characters = new Dictionary<int, (string, double)>();
        private readonly Dictionary<int, KnockedItem> _knocked = new Dictionary<int, KnockedItem>();

        public Func<double> Now { get; set; } = () => 0;

        // ---- hayvan pozu ----
        public void SetPose(int animalId, string pose, GridPos at, double days, int itemId = 0) =>
            _poses[animalId] = new AnimalPose { Pose = pose, At = at, Until = Now() + days, ItemId = itemId };

        /// <summary>Hayvan şu an poz veriyor mu: süresi dolmamış ve hayvan poz karosunda duruyor.</summary>
        public AnimalPose PoseOf(int animalId, GridPos position, bool moving)
        {
            if (!_poses.TryGetValue(animalId, out var p)) return null;
            if (Now() >= p.Until) { _poses.Remove(animalId); return null; }
            return !moving && position == p.At ? p : null;
        }

        public bool HasPendingPose(int animalId) => _poses.TryGetValue(animalId, out var p) && Now() < p.Until;

        // ---- yaban hayvanı efekti (kurbağadaki taç) ----
        public void SetWildFx(int visitorId, string fx, double days) => _wildFx[visitorId] = (fx, Now() + days);

        public string WildFxOf(int visitorId)
        {
            if (!_wildFx.TryGetValue(visitorId, out var f)) return null;
            if (Now() >= f.until) { _wildFx.Remove(visitorId); return null; }
            return f.fx;
        }

        // ---- karakter gösterisi (topaç çevirme, uluma) ----
        public void SetCharacterScene(int characterId, string key, double days) => _characters[characterId] = (key, Now() + days);

        public string CharacterSceneOf(int characterId)
        {
            if (!_characters.TryGetValue(characterId, out var s)) return null;
            if (Now() >= s.until) { _characters.Remove(characterId); return null; }
            return s.key;
        }

        // ---- yere düşen süs ----
        public IEnumerable<KnockedItem> Knocked => _knocked.Values;
        public bool HasKnocked => _knocked.Count > 0;
        public bool IsKnocked(int itemId) => _knocked.ContainsKey(itemId);
        public KnockedItem KnockedOf(int itemId) => _knocked.TryGetValue(itemId, out var k) ? k : null;

        public KnockedItem KnockedAt(GridPos tile)
        {
            foreach (var k in _knocked.Values) if (k.Floor == tile) return k;
            return null;
        }

        public void Knock(int itemId, GridPos floor) => _knocked[itemId] = new KnockedItem { ItemId = itemId, Floor = floor };
        public bool PutBack(int itemId) => _knocked.Remove(itemId);

        /// <summary>Dünyada artık olmayan eşyaların kaydını siler (satıldı, taşındı).</summary>
        public void Prune(Func<int, bool> exists)
        {
            foreach (var id in _knocked.Keys.Where(id => !exists(id)).ToList()) _knocked.Remove(id);
        }

        public string[] ExportKnocked() =>
            _knocked.Values.OrderBy(k => k.ItemId)
                .Select(k => string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}:{3}", k.ItemId, k.Floor.X, k.Floor.Y, k.Floor.Z)).ToArray();

        public void ImportKnocked(string[] data)
        {
            _knocked.Clear(); _poses.Clear(); _wildFx.Clear(); _characters.Clear();
            if (data == null) return;
            foreach (var line in data)
            {
                var p = (line ?? "").Split(':');
                if (p.Length < 4) continue;
                if (int.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                    && int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)
                    && int.TryParse(p[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y)
                    && int.TryParse(p[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var z))
                    _knocked[id] = new KnockedItem { ItemId = id, Floor = new GridPos(x, y, z) };
            }
        }
    }
}
