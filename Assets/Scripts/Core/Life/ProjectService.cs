using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Content;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Köy projeleri (Adım 38): kuyu, değirmen, çan kulesi... Akçe ve malzeme harcanarak bir kez yapılır;
    /// tamamlananların etkileri köy geneline (VillageEffects) işlenir ve "köy durumu" puanını artırır.
    /// </summary>
    public sealed class ProjectService
    {
        private readonly ContentDatabase _content;
        private readonly Household _house;
        private readonly VillageEffects _village;
        private readonly List<string> _done = new List<string>();

        public event Action<ProjectDef> Completed;

        public ProjectService(ContentDatabase content, Household house, VillageEffects village)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _village = village ?? throw new ArgumentNullException(nameof(village));
        }

        public IReadOnlyList<string> Done => _done;
        public int CompletedCount => _done.Count;
        public bool IsDone(string projectId) => _done.Contains(projectId);

        /// <summary>"Köy durumu" puanı: tamamlanan projelerin durum etkilerinin toplamı.</summary>
        public int VillageStatus => _village.Status;

        public string CanBuild(string projectId)
        {
            if (!_content.Projects.TryGet(projectId, out var p)) return "Böyle bir proje yok.";
            if (IsDone(projectId)) return "Bu proje zaten tamamlandı.";
            if (_house.Akce < p.Akce) return $"Akçe yetmiyor ({p.Akce} gerekir).";
            foreach (var c in p.Costs)
                if (_house.Count(c.ItemId) < c.Count)
                {
                    string name = _content.Items.TryGet(c.ItemId, out var d) ? d.DisplayName : c.ItemId;
                    return $"{name} yetmiyor ({_house.Count(c.ItemId)}/{c.Count}).";
                }
            return null;
        }

        public bool Build(string projectId, out string reason)
        {
            reason = CanBuild(projectId);
            if (reason != null) return false;
            var p = _content.Projects.Get(projectId);
            if (p.Akce > 0) _house.TrySpend(p.Akce);
            foreach (var c in p.Costs) for (int i = 0; i < c.Count; i++) _house.TryTakeItem(c.ItemId);
            _done.Add(projectId);
            Rebuild();
            Completed?.Invoke(p);
            return true;
        }

        /// <summary>Köy etkilerini tamamlanan projelerden baştan kurar (yükleme sonrası da çağrılır).</summary>
        public void Rebuild()
        {
            _village.Clear();
            foreach (var id in _done)
                if (_content.Projects.TryGet(id, out var p)) _village.Add(p.Effects);
        }

        public void Restore(IEnumerable<string> ids)
        {
            _done.Clear();
            if (ids != null)
                foreach (var id in ids)
                    if (!string.IsNullOrEmpty(id) && _content.Projects.Contains(id) && !_done.Contains(id)) _done.Add(id);
            Rebuild();
        }
    }
}
