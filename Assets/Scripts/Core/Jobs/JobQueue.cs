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

        internal void RemoveCurrent()
        {
            if (_jobs.Count > 0) _jobs.RemoveAt(0);
        }
    }
}
