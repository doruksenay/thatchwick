using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Köylülerin evleri (belge 47 §8, karar 0-9): hiçbir NPC hanesi oyuncunun arsasında yaşamaz. Adlı köylü kendi evinde
    /// (life.json "home"), dükkân hanesi dükkânında oturur. Dükkânsız haneye kasabadaki boş köylü evlerinden biri, o da yoksa
    /// handa bir oda verilir. Evlenen eşinin evine, çocuk ebeveyninin evine gider. Soyu tükenen hanenin evi boşalır.
    /// Köylüler oyuncunun arsasına yalnızca bir işle gelir (misafir, rica teslimi, imece, hizmetkâr) ve evlerine döner.
    /// </summary>
    public sealed class VillageHomes
    {
        /// <summary>Dükkânsız hanelere verilebilecek köylü evleri (adlı köylülerin oturmadıkları).</summary>
        public static readonly string[] Cottages = { "home:1", "home:2", "home:3", "home:4", "home:5", "home:6" };
        /// <summary>Handaki kiralık odalar (belge 44: hanın üst katında 2-3 oda; burada 4).</summary>
        public static readonly string[] InnRooms = { "inn:1", "inn:2", "inn:3", "inn:4" };
        /// <summary>Hiç yer yoksa: hanın ortak odası (birden çok kişi).</summary>
        public const string InnCommon = "inn";

        private readonly VillagerService _villagers;
        private readonly NeighborHouseService _houses;
        private readonly Population _population;
        private readonly Func<IEnumerable<string>> _namedHomes;

        public VillageHomes(VillagerService villagers, NeighborHouseService houses, Population population, Func<IEnumerable<string>> namedHomes)
        {
            _villagers = villagers ?? throw new ArgumentNullException(nameof(villagers));
            _houses = houses ?? throw new ArgumentNullException(nameof(houses));
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _namedHomes = namedHomes ?? (() => Array.Empty<string>());
        }

        /// <summary>Hanın odaları hangi adlandırılmış yere çözülür (oyun katmanı Places'te).</summary>
        public static bool IsInn(string key) => key == InnCommon || (key != null && key.StartsWith("inn:"));

        /// <summary>Bir köylünün evinin adlandırılmış yeri; köylü değilse null.</summary>
        public string HomeOf(Character c)
        {
            if (c == null || !c.IsVillager) return null;
            var def = _villagers.DefOf(c);
            if (def != null && !string.IsNullOrEmpty(def.Home)) return def.Home;
            var house = _houses.HouseOf(c);
            if (house != null)
            {
                if (house.DefId != null && _villagers.Content.Villagers.TryGet(house.DefId, out var hd) && !string.IsNullOrEmpty(hd.Home)) return hd.Home;
                if (house.HomeKey == null) Assign(house);
                return house.HomeKey ?? InnCommon;
            }
            return InnCommon;
        }

        /// <summary>Boş bir ev ya da han odası verir; yoksa ortak oda.</summary>
        public string Assign(NeighborHouse house)
        {
            if (house == null) return null;
            if (house.HomeKey != null) return house.HomeKey;
            var taken = Taken();
            foreach (var k in Cottages) if (!taken.Contains(k)) { house.HomeKey = k; return k; }
            foreach (var k in InnRooms) if (!taken.Contains(k)) { house.HomeKey = k; return k; }
            house.HomeKey = InnCommon;
            return InnCommon;
        }

        /// <summary>Dolu evler: adlı köylülerin evleri ve yaşayan hanelerin evleri.</summary>
        public HashSet<string> Taken()
        {
            var set = new HashSet<string>();
            foreach (var k in _namedHomes()) if (!string.IsNullOrEmpty(k)) set.Add(k);
            foreach (var h in _houses.Houses)
            {
                if (h.Extinct) { h.HomeKey = null; continue; }
                if (h.HomeKey != null && h.HomeKey != InnCommon) set.Add(h.HomeKey);
            }
            return set;
        }

        /// <summary>Boş köylü evi kalmış mı (halef ve yeni gelen için).</summary>
        public int FreeCottages()
        {
            var taken = Taken();
            int n = 0;
            foreach (var k in Cottages) if (!taken.Contains(k)) n++;
            return n;
        }

        /// <summary>Bütün dükkânsız hanelere ev atar (yükleme ve gün başı).</summary>
        public void AssignAll()
        {
            Taken();   // soyu tükenenlerin evini boşalt
            foreach (var h in _houses.Houses)
                if (!h.Extinct && h.DefId == null && h.HomeKey == null) Assign(h);
        }

        /// <summary>
        /// Programı olmayan köylünün bulunacağı yer: gece ve fırtınada evinde, gündüz meydanda, akşam evinde; şenlik günü meydanda.
        /// </summary>
        public string LocationFor(Character c, double dayFraction, WeatherType weather, bool festival)
        {
            string home = HomeOf(c) ?? InnCommon;
            if (weather == WeatherType.Storm) return home;
            if (festival && dayFraction >= VillagerService.HomeUntil && dayFraction < VillagerService.SquareUntil) return "square";
            if (dayFraction < VillagerService.HomeUntil || dayFraction >= VillagerService.SquareUntil) return home;
            if (weather == WeatherType.Rain || weather == WeatherType.Snow) return home;
            return dayFraction < 0.45 ? home : "square";
        }

        /// <summary>Köylü oyuncunun arsasında mı (eski kayıt göçü: evine taşınır).</summary>
        public static bool OnPlayerPlot(Character c) => c != null && c.IsVillager && Regions.Farm.Contains(c.Position);
    }
}
