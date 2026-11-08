using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Hanedan geçmişine bağlı mezar kaydı (Adım 18e).</summary>
    public sealed class GraveRecord
    {
        public int CharacterId;
        public string Name;
        public int Generation;
        public Sex Sex;
        public double DeathDay;
        public DeathCause Cause;
        public double AgeDays;
        public int ItemId;        // haritadaki mezar eşyası; 0 = yerleştirilemedi
        public GridPos Position;
    }

    /// <summary>
    /// Ölünce karakter haritadan kalkar, yakın boş karoda taşınamayan bir mezar eşyası oluşur
    /// (kimliği "item.grave"). Mezar kaydı kalıcıdır; kayıttan yüklemede eşya zaten dünyada olduğundan
    /// yalnızca kayıtlar geri konur.
    /// </summary>
    public sealed class GraveService
    {
        public const string GraveItemId = "item.grave";
        public const int SearchRadius = 8;

        private readonly WorldState _world;
        private readonly ContentDatabase _content;
        private readonly List<GraveRecord> _graves = new List<GraveRecord>();

        public IReadOnlyList<GraveRecord> Graves => _graves;
        public event Action<GraveRecord> GraveCreated;

        public GraveService(WorldState world, ContentDatabase content)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public GraveRecord FindFor(int characterId)
        {
            foreach (var g in _graves) if (g.CharacterId == characterId) return g;
            return null;
        }

        public bool IsGrave(ItemInstance item) => item != null && item.Def.Id == GraveItemId;

        public void Restore(IEnumerable<GraveRecord> records)
        {
            _graves.Clear();
            if (records != null) _graves.AddRange(records);
        }

        /// <summary>Ölen için mezar kaydı açar ve (mümkünse) mezar eşyasını yakın boş karoya koyar.</summary>
        public GraveRecord Bury(Character dead, double day)
        {
            if (dead == null) throw new ArgumentNullException(nameof(dead));
            var existing = FindFor(dead.Id);
            if (existing != null) return existing;

            var rec = new GraveRecord
            {
                CharacterId = dead.Id, Name = dead.Name, Generation = dead.Generation, Sex = dead.Sex,
                DeathDay = day, Cause = dead.CauseOfDeath, AgeDays = dead.AgeDays, Position = dead.Position
            };
            if (_content.Items.TryGet(GraveItemId, out var def))
            {
                var spot = FindSpot(dead.Position);
                if (spot.HasValue)
                {
                    var item = _world.PlaceItem(def, spot.Value);
                    rec.ItemId = item.Id;
                    rec.Position = spot.Value;
                }
            }
            _graves.Add(rec);
            GraveCreated?.Invoke(rec);
            return rec;
        }

        // En yakın boş karo (halka halka): iç mekâna gömülmez, tercihen dış mekân.
        private GridPos? FindSpot(GridPos origin)
        {
            var rooms = _world.Map.Rooms;
            GridPos? indoorFallback = null;
            for (int r = 0; r <= SearchRadius; r++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;
                        var p = new GridPos(origin.X + dx, origin.Y + dy);
                        if (!_world.Map.InBounds(p) || _world.Map.IsBlocked(p)) continue;
                        var room = rooms.RoomAt(p);
                        if (room != null && room.IsIndoor) { if (!indoorFallback.HasValue) indoorFallback = p; continue; }
                        return p;
                    }
                }
            }
            return indoorFallback;
        }
    }
}
