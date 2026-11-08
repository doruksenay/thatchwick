using System;
using System.Collections.Generic;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Hanedan hedefleri (Adım 41): "3. nesle ulaş", "5 kişilik köy", "1000 akçe"... Ölçütü bir sağlayıcı verir
    /// (LifeSystems hanenin durumundan hesaplar); hedef bir kez tamamlanır, ödülü bir kez verilir.
    /// </summary>
    public sealed class GoalService
    {
        private readonly ContentDatabase _content;
        private readonly List<string> _done = new List<string>();

        /// <summary>(ölçüt adı) -> şu anki değer.</summary>
        public Func<string, int> Metric { get; set; }

        public event Action<GoalDef> Completed;

        public GoalService(ContentDatabase content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public IReadOnlyList<string> Done => _done;
        public bool IsDone(string goalId) => _done.Contains(goalId);

        public int Progress(GoalDef goal)
        {
            if (IsDone(goal.Id)) return goal.Target;
            int v = Metric != null ? Metric(goal.Metric) : 0;
            return Math.Max(0, Math.Min(goal.Target, v));
        }

        /// <summary>Yeni tamamlanan hedefleri bulur, kaydeder ve Completed olayını tetikler.</summary>
        public List<GoalDef> Update()
        {
            var newly = new List<GoalDef>();
            if (Metric == null) return newly;
            foreach (var g in _content.Goals.All)
            {
                if (IsDone(g.Id)) continue;
                if (Metric(g.Metric) >= g.Target) newly.Add(g);
            }
            // Önce hepsi tamamlandı sayılır: ödül (XP) seviye atlatıp yeniden Update çağırsa bile çift sayılmaz.
            foreach (var g in newly) _done.Add(g.Id);
            foreach (var g in newly) Completed?.Invoke(g);
            return newly;
        }

        public void Restore(IEnumerable<string> ids)
        {
            _done.Clear();
            if (ids == null) return;
            foreach (var id in ids)
                if (!string.IsNullOrEmpty(id) && _content.Goals.Contains(id) && !_done.Contains(id)) _done.Add(id);
        }
    }
}
