using System.Collections.Generic;

namespace Thatchwick.Core.Audio
{
    /// <summary>
    /// Karıştırma torbası (Project 45a bölüm 3): öğeler karışık sırayla çıkar, torba bitince yeniden karışır.
    /// Son çıkan <see cref="NoRepeat"/> öğe, torba yenilense bile hemen yeniden seçilmez; art arda aynı ses hiç çıkmaz.
    /// </summary>
    public sealed class ShuffleBag
    {
        private readonly int _count;
        private readonly AudioRandom _rng;
        private readonly List<int> _bag = new List<int>();
        private readonly Queue<int> _recent = new Queue<int>();

        public int Count => _count;
        public int NoRepeat { get; }

        /// <param name="noRepeat">Negatifse varsayılan: öğe sayısının yarısı (en az 1, en çok count-1).</param>
        public ShuffleBag(int count, AudioRandom rng, int noRepeat = -1)
        {
            _count = count;
            _rng = rng;
            int def = count / 2;
            NoRepeat = count <= 1 ? 0 : Clamp(noRepeat < 0 ? def : noRepeat, 1, count - 1);
        }

        public int Next()
        {
            if (_count <= 0) return -1;
            if (_count == 1) return 0;
            if (_bag.Count == 0) Refill();
            int pick = -1;
            for (int i = 0; i < _bag.Count; i++)
            {
                if (!_recent.Contains(_bag[i])) { pick = i; break; }
            }
            if (pick < 0)
            {
                // torbada yalnız yakın zamanda çalınanlar kaldı: torbayı yenile (çalınmayanlar gelir)
                Refill();
                for (int i = 0; i < _bag.Count; i++)
                    if (!_recent.Contains(_bag[i])) { pick = i; break; }
                if (pick < 0) pick = 0;
            }
            int v = _bag[pick];
            _bag.RemoveAt(pick);
            _recent.Enqueue(v);
            while (_recent.Count > NoRepeat) _recent.Dequeue();
            return v;
        }

        private void Refill()
        {
            _bag.Clear();
            for (int i = 0; i < _count; i++) _bag.Add(i);
            for (int i = _bag.Count - 1; i > 0; i--)
            {
                int j = _rng.Range(0, i + 1);
                int t = _bag[i]; _bag[i] = _bag[j]; _bag[j] = t;
            }
        }

        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : v > hi ? hi : v;
    }
}
