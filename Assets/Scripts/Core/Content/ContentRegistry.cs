using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Content
{
    /// <summary>Kimliğe göre içerik kaydı. Kayıt sırası korunur (belirleyici sıralama).</summary>
    public sealed class ContentRegistry<T> where T : class, IContentDef
    {
        private readonly Dictionary<string, T> _byId = new Dictionary<string, T>(StringComparer.Ordinal);
        private readonly List<T> _ordered = new List<T>();

        public int Count => _ordered.Count;

        public IReadOnlyList<T> All => _ordered;

        public void Register(T def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (string.IsNullOrWhiteSpace(def.Id))
                throw new ArgumentException("İçerik kimliği boş olamaz.", nameof(def));
            if (_byId.ContainsKey(def.Id))
                throw new InvalidOperationException($"Aynı kimlik iki kez kaydedildi: {def.Id}");

            _byId.Add(def.Id, def);
            _ordered.Add(def);
        }

        public bool Contains(string id) => id != null && _byId.ContainsKey(id);

        public bool TryGet(string id, out T def)
        {
            if (id == null) { def = null; return false; }
            return _byId.TryGetValue(id, out def);
        }

        public T Get(string id)
        {
            if (!TryGet(id, out var def))
                throw new KeyNotFoundException($"İçerik bulunamadı: {id}");
            return def;
        }
    }
}
