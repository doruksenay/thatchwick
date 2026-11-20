using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Animals.Genetics;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Çiftlik hayvanları ve yaban ziyaretçilerinin görünümleri (GameRoot'tan ayrıldı, performans incelemesi 20.11.2026).
    /// <see cref="Sync"/> simülasyon adımında çalışır: görünüm ekler/siler, hedef konumu, yönü ve görseli günceller.
    /// <see cref="Frame"/> her karede yalnızca konumu ara değerler ve sıralar. Kare başına bellek ayırmaz.
    /// </summary>
    public sealed class AnimalViews
    {
        private sealed class View
        {
            public Transform T;
            public SpriteRenderer R;
            public SpriteRenderer Shadow;
            public int Dir = IsoArt.DefaultDirection;
            public int Level;
            public int RiderId;
            public SmoothPosition Pos;
        }

        private readonly Transform _parent;
        private readonly WorldView _world;
        private readonly Func<Animal, Phenotype> _lookOf;
        private readonly Func<int, Transform> _riderView;
        private readonly Dictionary<int, View> _animals = new Dictionary<int, View>();
        private readonly Dictionary<int, View> _wild = new Dictionary<int, View>();
        private readonly HashSet<int> _alive = new HashSet<int>();
        private readonly List<int> _gone = new List<int>();
        private static readonly Dictionary<string, Color?> TintCache = new Dictionary<string, Color?>(StringComparer.Ordinal);

        public AnimalViews(Transform parent, WorldView world, Func<Animal, Phenotype> lookOf, Func<int, Transform> riderView)
        {
            _parent = parent;
            _world = world;
            _lookOf = lookOf;
            _riderView = riderView;
        }

        /// <summary>Simülasyon adımı: canlı hayvanlar ve ziyaretçilerle eşitle.</summary>
        public void Sync(AnimalSystem animals, WildRoamers roamers)
        {
            if (animals != null) SyncAnimals(animals);
            if (roamers != null) SyncWild(roamers);
        }

        /// <summary>Kare: konum ara değeri ve çizim sırası.</summary>
        public void Frame(float alpha)
        {
            foreach (var kv in _animals)
            {
                var v = kv.Value;
                if (v.T == null) continue;
                Vector3 pos;
                var rider = v.RiderId != 0 ? _riderView(v.RiderId) : null;
                if (rider != null) pos = rider.position - new Vector3(0f, 0.02f, 0f);   // binicinin altında, onunla birlikte
                else pos = v.Pos.At(alpha);
                v.T.position = pos;
                v.R.sortingOrder = Iso.SortOrder(pos.y, 1, v.Level);
                if (v.Shadow != null) { v.Shadow.enabled = v.R.enabled && rider == null; v.Shadow.sortingOrder = Shadows.Order(v.Level); }
            }
            foreach (var kv in _wild)
            {
                var v = kv.Value;
                if (v.T == null) continue;
                var pos = v.Pos.At(alpha);
                v.T.position = pos;
                v.R.sortingOrder = Iso.SortOrder(pos.y, 1, v.Level);
            }
        }

        /// <summary>Bölge değişince ara değer sıfırlanır (eski konumdan kayarak gelmesin).</summary>
        public void ResetSmoothing()
        {
            foreach (var v in _animals.Values) v.Pos.Reset();
            foreach (var v in _wild.Values) v.Pos.Reset();
        }

        private View Create(string name, bool shadow)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_parent, false);
            var v = new View { T = go.transform, R = go.AddComponent<SpriteRenderer>() };
            v.R.sharedMaterial = SpriteMaterials.Unlit;
            if (shadow) v.Shadow = Shadows.Attach(go.transform, new Vector3(0.06f, -0.03f, 0f), 0.6f);
            return v;
        }

        private void SyncAnimals(AnimalSystem system)
        {
            _alive.Clear();
            foreach (var a in system.Animals)
            {
                _alive.Add(a.Id);
                if (!_animals.TryGetValue(a.Id, out var v)) { v = Create(a.Name, true); _animals[a.Id] = v; }
                Vector3 pos = _world.TileToWorld(a.Position);
                if (a.RiderId == 0 && a.NextTile.HasValue)
                {
                    var to = _world.TileToWorld(a.NextTile.Value);
                    v.Dir = IsoArt.DirectionIndex(new Vector2(to.x - pos.x, to.y - pos.y));
                    pos = Vector3.Lerp(pos, to, a.StepProgress);
                }
                v.Pos.Push(pos);
                v.RiderId = a.RiderId;
                v.Level = a.Position.Z;
                v.R.enabled = _world.IsLevelVisible(a.Position.Z);
                // Genetik (belge 48): tipin gövdesi + genden renk; gövde görseli yoksa türün görseli, o da yoksa genden renkli küp.
                var look = _lookOf(a);
                Sprite real = look != null && look.Layers.Count > 0 ? IsoArt.AnimalBody(look.Layers[0], look.LongCoat, v.Dir) : null;
                Color tint = Color.white;
                var gene = Tint(look?.TintHex);
                if (real != null && gene.HasValue) tint = Color.Lerp(Color.white, gene.Value, 0.8f);
                if (real == null) real = IsoArt.Animal(a.Species.Id, v.Dir);
                float scale = a.IsMature ? 1f : 0.7f;
                if (real != null) { if (v.R.sprite != real) v.R.sprite = real; v.R.color = tint; }
                else
                {
                    var cube = SpriteMaterials.Cube(gene ?? ColorOf(a.Species.Id));
                    if (v.R.sprite != cube) v.R.sprite = cube;
                    v.R.color = Color.white;
                    scale *= ScaleOf(a.Species.Id);
                }
                v.T.localScale = new Vector3(scale, scale, 1f);
            }
            RemoveGone(_animals);
        }

        private void SyncWild(WildRoamers roamers)
        {
            _alive.Clear();
            foreach (var w in roamers.Visitors)
            {
                _alive.Add(w.Id);
                if (!_wild.TryGetValue(w.Id, out var v)) { v = Create(w.Species.Name, false); _wild[w.Id] = v; }
                Vector3 pos = _world.TileToWorld(w.Position);
                if (w.NextTile.HasValue)
                {
                    var to = _world.TileToWorld(w.NextTile.Value);
                    v.Dir = IsoArt.DirectionIndex(new Vector2(to.x - pos.x, to.y - pos.y));
                    pos = Vector3.Lerp(pos, to, (float)w.StepProgress);
                }
                v.Pos.Push(pos);
                v.Level = w.Position.Z;
                v.R.enabled = _world.IsLevelVisible(w.Position.Z);
                var real = IsoArt.Animal(w.Species.Id, v.Dir);
                float scale = w.Young ? 0.7f : 1f;
                var sprite = real ?? SpriteMaterials.Cube(ColorOf(w.Species.Id));
                if (real == null) scale *= ScaleOf(w.Species.Id);
                if (v.R.sprite != sprite) v.R.sprite = sprite;
                v.T.localScale = new Vector3(scale, scale, 1f);
            }
            RemoveGone(_wild);
        }

        private void RemoveGone(Dictionary<int, View> views)
        {
            _gone.Clear();
            foreach (var kv in views) if (!_alive.Contains(kv.Key)) _gone.Add(kv.Key);
            foreach (var id in _gone)
            {
                if (views[id].T != null) UnityEngine.Object.Destroy(views[id].T.gameObject);
                views.Remove(id);
            }
        }

        /// <summary>Gen renginin (#rrggbb) çözümü önbellekli: her adımda yeniden ayrıştırılmaz.</summary>
        private static Color? Tint(string hex)
        {
            if (hex == null) return null;
            if (TintCache.TryGetValue(hex, out var c)) return c;
            c = ColorUtility.TryParseHtmlString(hex, out var parsed) ? parsed : (Color?)null;
            TintCache[hex] = c;
            return c;
        }

        public static Color ColorOf(string id)
        {
            switch (id)
            {
                case "animal.chicken": return new Color(0.95f, 0.92f, 0.85f);
                case "animal.duck": return new Color(0.55f, 0.7f, 0.45f);
                case "animal.goat": return new Color(0.75f, 0.68f, 0.55f);
                case "animal.sheep": return new Color(0.92f, 0.9f, 0.86f);
                case "animal.cow": return new Color(0.55f, 0.4f, 0.3f);
                case "animal.ox": return new Color(0.42f, 0.32f, 0.25f);
                case "wild.rabbit": case "wild.squirrel": case "wild.roe_deer": return new Color(0.6f, 0.45f, 0.3f);
                case "wild.fox": return new Color(0.85f, 0.45f, 0.15f);
                case "wild.boar": case "wild.hedgehog": case "wild.rat": return new Color(0.35f, 0.3f, 0.27f);
                case "wild.crow": case "wild.owl": case "wild.sparrow": case "wild.stork": return new Color(0.2f, 0.2f, 0.22f);
                case "wild.frog": return new Color(0.4f, 0.6f, 0.3f);
                case "wild.butterfly": return new Color(0.95f, 0.85f, 0.3f);
                case "wild.firefly": return new Color(1f, 1f, 0.5f);
                case "animal.horse": return new Color(0.45f, 0.3f, 0.2f);
                case "animal.donkey": case "animal.mule": case "animal.hinny": return new Color(0.55f, 0.5f, 0.45f);
                case "animal.goose": return new Color(0.93f, 0.93f, 0.9f);
                case "animal.rabbit": return new Color(0.58f, 0.5f, 0.42f);
                case "animal.pigeon": return new Color(0.55f, 0.57f, 0.62f);
                case "animal.dog": return new Color(0.6f, 0.45f, 0.3f);
                case "animal.cat": return new Color(0.85f, 0.55f, 0.25f);
                default: return new Color(0.95f, 0.8f, 0.2f);
            }
        }

        public static float ScaleOf(string id)
        {
            switch (id)
            {
                case "animal.chicken": case "animal.duck": case "animal.bees": return 0.22f;
                case "animal.cat": return 0.26f;
                case "animal.dog": return 0.3f;
                case "animal.cow": case "animal.ox": return 0.5f;
                case "wild.butterfly": case "wild.firefly": case "wild.sparrow": case "wild.frog": return 0.12f;
                case "wild.rabbit": case "wild.squirrel": case "wild.hedgehog": case "wild.rat": case "wild.crow": return 0.18f;
                case "wild.owl": case "wild.fox": case "wild.stork": return 0.26f;
                case "wild.roe_deer": case "wild.boar": return 0.38f;
                case "animal.horse": return 0.55f;
                default: return 0.4f;
            }
        }
    }
}
