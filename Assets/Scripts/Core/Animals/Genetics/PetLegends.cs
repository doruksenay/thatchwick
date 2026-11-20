using System;

namespace Thatchwick.Core.Animals.Genetics
{
    /// <summary>
    /// Kedi, köpek ve katırın efsane yolları (Project belgesi 48 §6.4, §7.4). Sayaç eşiğe gelince ve hayvan Soylu + 5. seviyedeyse
    /// Efsane olur (29 kuralı). Katır ve bardo soy sürdüremediği için istisna: kendi soyunun en yüksek seviyesi yeter.
    /// Tazı koşusu ve kedi güzellik yarışması şenlikte simüle edilir (<see cref="ContestKind"/>).
    /// </summary>
    public static class PetLegends
    {
        public const string Wolves = "wolves", Tracks = "tracks", Guards = "guards", LegendFish = "legend_fish", Rats = "rats", Trips = "trips";

        public const int WolvesNeeded = 10, TracksNeeded = 20, GuardsNeeded = 3, LegendFishNeeded = 1, RatsNeeded = 40, TripsNeeded = 50;

        /// <summary>Bu hayvanın sayaç yolu (sayaç, eşik, unvan); yoksa null.</summary>
        public static (string deed, int needed, string title)? PathFor(Animal a)
        {
            if (a?.Genome == null) return null;
            switch (a.TypeId)
            {
                case "dog.shepherd": case "dog.wolfbreaker": return (Wolves, WolvesNeeded, "Kurt Kıran");
                case "dog.scenthound": case "dog.tracker": case "dog.laketracker": case "dog.lurcher": return (Tracks, TracksNeeded, "İz Ustası");
                case "dog.mastiff": case "dog.alaunt": return (Guards, GuardsNeeded, "Kapı Aslanı");
                case "dog.waterdog": return (LegendFish, LegendFishNeeded, "Göl Getireni");
                case "mule.common": case "hinny.common": return (Trips, TripsNeeded, "Dağ Aşan");
            }
            if (a.Species.Id == GeneticsCatalog.Cat) return (Rats, RatsNeeded, "Ambar Sultanı");
            return null;
        }

        /// <summary>Eşik ve kural tutuyorsa unvanı verir; verildiyse true.</summary>
        public static bool TryGrant(Animal a)
        {
            if (a == null || a.IsDead || a.LegendaryTitle != null) return false;
            var path = PathFor(a);
            if (path == null || a.Deed(path.Value.deed) < path.Value.needed) return false;
            if (GeneticsCatalog.IsSterileHybrid(a.Species.Id))
            {
                if (a.Level < AnimalProgress.MaxLevelFor(a.Lineage)) return false;
                a.Lineage = AnimalLineage.Legendary;
                a.LegendaryTitle = path.Value.title;
                return true;
            }
            return AnimalProgress.GrantLegend(a, path.Value.title);
        }

        /// <summary>İlerleme metni için: (sayaç, eşik).</summary>
        public static (int have, int need) Progress(Animal a)
        {
            var path = PathFor(a);
            return path == null ? (0, 0) : (a.Deed(path.Value.deed), path.Value.needed);
        }
    }
}
