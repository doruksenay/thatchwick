using System.Collections.Generic;

namespace Thatchwick.Core.Jobs
{
    /// <summary>
    /// Karakterin iş kuyruğu (çekirdek mekanik). Uzunluk sınırı vardır; Premium daha uzun kuyruk verir.
    /// </summary>
    public sealed class JobQueue
    {
        public const int DefaultMaxLength = 5;

        private readonly List<Job> _jobs = new List<Job>();

        public int MaxLength { get; set; } = DefaultMaxLength;
        public int Count => _jobs.Count;
        public Job Current => _jobs.Count > 0 ? _jobs[0] : null;
        public IReadOnlyList<Job> Jobs => _jobs;

        /// <summary>Kuyruk doluysa false döner ve iş eklenmez.</summary>
        public bool TryEnqueue(Job job)
        {
            if (job == null || _jobs.Count >= MaxLength) return false;
            _jobs.Add(job);
            return true;
        }

        public void Clear() => _jobs.Clear();

        internal bool RemoveAt(int index)
        {
            if (index < 0 || index >= _jobs.Count) return false;
            _jobs.RemoveAt(index);
            return true;
        }

        internal bool MoveToFront(int index)
        {
            if (index <= 0 || index >= _jobs.Count) return false;
            var job = _jobs[index];
            _jobs.RemoveAt(index);
            _jobs.Insert(0, job);
            return true;
        }

        internal void RemoveCurrent()
        {
            if (_jobs.Count > 0) _jobs.RemoveAt(0);
        }
    }
}
