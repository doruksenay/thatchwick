using System.Collections.Generic;

namespace Thatchwick.Core.Audio
{
    /// <summary>
    /// Müzik seçimi (Project 45a bölüm 3): bağlama göre havuz (ör. "spring.day"), son çalınan 2 parça
    /// yeniden seçilmez, parçalar arasına rastgele sessizlik girer. Bağlamda parça yoksa üst bağlama
    /// düşer ("spring.day" → "spring" → "default").
    /// </summary>
    public sealed class MusicPlaylist
    {
        private readonly AudioRandom _rng;
        private readonly Dictionary<string, List<string>> _pools = new Dictionary<string, List<string>>();
        private readonly List<string> _history = new List<string>();

        public int NoRepeat = 2;
        public float SilenceMin = 60f;
        public float SilenceMax = 240f;

        public MusicPlaylist(int seed) { _rng = new AudioRandom(seed); }

        public void Add(string context, string trackKey)
        {
            if (string.IsNullOrEmpty(context)) context = "default";
            if (!_pools.TryGetValue(context, out var l)) _pools[context] = l = new List<string>();
            if (!l.Contains(trackKey)) l.Add(trackKey);
        }

        public IReadOnlyList<string> PoolFor(string context)
        {
            string c = context ?? "";
            while (true)
            {
                if (_pools.TryGetValue(c, out var l) && l.Count > 0) return l;
                int dot = c.LastIndexOf('.');
                if (dot < 0) break;
                c = c.Substring(0, dot);
            }
            if (c != "default" && _pools.TryGetValue("default", out var d) && d.Count > 0) return d;
            return System.Array.Empty<string>();
        }

        /// <summary>Sıradaki parça (null: bu bağlamda müzik yok).</summary>
        public string Next(string context)
        {
            var pool = PoolFor(context);
            if (pool.Count == 0) return null;
            int avoid = System.Math.Min(NoRepeat, pool.Count - 1);
            var recent = _history.Count > avoid ? _history.GetRange(_history.Count - avoid, avoid) : new List<string>(_history);
            var choices = new List<string>();
            foreach (var t in pool) if (avoid == 0 || !recent.Contains(t)) choices.Add(t);
            if (choices.Count == 0) choices.AddRange(pool);
            string pick = choices[_rng.Range(0, choices.Count)];
            _history.Add(pick);
            if (_history.Count > 16) _history.RemoveAt(0);
            return pick;
        }

        /// <summary>Parça bitince ne kadar sessiz kalınacak (sn).</summary>
        public float NextSilence() => _rng.Range(SilenceMin, SilenceMax);
    }
}
