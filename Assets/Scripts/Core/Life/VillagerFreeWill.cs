using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Köylünün özgür iradesinin sınırları (Project belgeleri 66e §2.1, 66f §A.2): program önce gelir. Program köylünün o saatte
    /// nerede olması gerektiğini söyler; özgür irade yalnızca o yerdeki ve izinli yerlerdeki (kendi evi, kendi işyeri,
    /// meyhane, meydan, aşevi, ırmak kıyısı, ulu ağaç) eşyaları puanlar. Oyuncunun arsası hiçbir zaman (47 §8). İş saatinde
    /// köylü işyerinin dışına çıkmaz.
    /// </summary>
    public static class VillagerFreeWill
    {
        /// <summary>Açık havadaki yerin çevresi (karo): meydanın çeşmesi, ırmak kıyısı.</summary>
        public const int OutdoorRadius = 8;
        /// <summary>Kapalı yerin çevresi (karo): evin ya da dükkânın odaları (yatak odası, hela, mutfak).</summary>
        public const int IndoorRadius = 6;

        /// <summary>İzinli yerlerin anahtarları (oyun katmanı karoya çevirir). Ulu ağaç ve aşevi: Treehouses "feast" / "inn".</summary>
        public const string Tavern = "work:tavern", Square = "square", Charity = "inn", GreatTree = "feast", Riverside = "lake";

        /// <summary>
        /// Köylünün şu an kullanabileceği yerler: önce programdaki yer. İş saatinde (programdaki yer işyeri) yalnızca işyeri;
        /// değilse evi, işyeri ve köyün ortak yerleri.
        /// </summary>
        public static List<string> AllowedPlaces(string home, string work, string current)
        {
            var list = new List<string>(8);
            void Add(string k) { if (!string.IsNullOrEmpty(k) && !list.Contains(k)) list.Add(k); }
            Add(current);
            bool atWork = !string.IsNullOrEmpty(work) && current == work;
            if (atWork) return list;
            Add(home); Add(work); Add(Tavern); Add(Square); Add(Charity); Add(Riverside); Add(GreatTree);
            return list;
        }

        /// <summary>Karo oyuncunun arsasında mı (sahipli karo ya da çiftlik bölgesi).</summary>
        public static bool OnPlayerPlot(GridMap map, GridPos at) =>
            map.IsOwned(at) || (Regions.IsRegional(map) && Regions.Farm.Contains(at));

        /// <summary>
        /// Köylü bu eşyayı ihtiyacı için kullanabilir mi: oyuncunun arsasında değil ve izinli yerlerden birinin çevresinde
        /// (kapalı yerde aynı yapı, açıkta biraz daha geniş).
        /// </summary>
        public static bool MayUse(WorldState world, ItemInstance item, IReadOnlyList<GridPos> anchors)
        {
            if (world == null || item == null || anchors == null || anchors.Count == 0) return false;
            var map = world.Map;
            var at = item.IsDecor && item.ParentId.HasValue ? (world.GetItem(item.ParentId.Value)?.Position ?? item.Position) : item.Position;
            // Önce ucuz eleme: izinli yerlerden birinin geniş çevresinde mi (çoğu eşya burada elenir).
            int near = -1;
            for (int i = 0; i < anchors.Count; i++)
            {
                var a = anchors[i];
                if (Math.Max(Math.Abs(a.X - at.X), Math.Abs(a.Y - at.Y)) <= OutdoorRadius) { near = i; break; }
            }
            if (near < 0 || OnPlayerPlot(map, at)) return false;
            var room = map.Rooms.RoomAt(at);
            bool indoor = room != null && room.IsIndoor;
            int radius = indoor ? IndoorRadius : OutdoorRadius;
            var region = Regions.IsRegional(map) ? Regions.At(at) : null;
            for (int i = near; i < anchors.Count; i++)
            {
                var a = anchors[i];
                if (Math.Max(Math.Abs(a.X - at.X), Math.Abs(a.Y - at.Y)) > radius) continue;
                if (region != null && Regions.At(a) != region) continue;
                return true;
            }
            return false;
        }
    }
}
