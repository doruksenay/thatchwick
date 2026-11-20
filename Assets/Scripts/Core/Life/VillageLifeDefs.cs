using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Life
{
    /// <summary>Mesleğin işyerindeki iş döngüsünün bir adımı (life.json "villagerWork"; belge 66f §C.11).</summary>
    public sealed class VillagerWorkStep
    {
        public string Id;
        /// <summary>Oynayacak kare anahtarı; karesi yoksa <see cref="Fallback"/>.</summary>
        public string Pose;
        /// <summary>Pozun karesi gelene kadar oynayan en yakın genel poz.</summary>
        public string Fallback;
        /// <summary>Adımın süresi (oyun dakikası).</summary>
        public double Minutes = 20;
        /// <summary>Arayüz metni (iş tanımı, balon): "village.work.&lt;id&gt;".</summary>
        public string TextKey => "village.work." + Id;
    }

    public sealed class PantryStaple
    {
        public string Item;
        public int Target;
        public int Min;
    }

    public sealed class PantryProduce
    {
        public string Role;
        public string Item;
        public int Count;
    }

    public sealed class VillageMeal
    {
        public string Id;
        /// <summary>"home" (ocakta pişirme, hane sofrası) ya da "work" (azık).</summary>
        public string Where;
        /// <summary>Pozları veren izlenen eylem zinciri (belge 60): "cook_pot", "eat".</summary>
        public string Chain;
        public readonly List<KeyValuePair<string, int>> Consume = new List<KeyValuePair<string, int>>();
        public float Hunger = 60f;
    }

    /// <summary>
    /// Yaşayan köy ayarları (life.json "villageLife"; Project belgeleri 66e §2, 66f). Sayılar denge koşusunda ayarlanır
    /// (66e §5); kod yalnızca varsayılanı bilir.
    /// </summary>
    public sealed class VillageLifeConfig
    {
        public double GiftChance = 0.15;
        public double Discount = 0.05;
        public double BreakdownMinDays = 1.0, BreakdownMaxDays = 2.0;
        public double ConsoleShare = 0.5;
        /// <summary>"Yakın zamanda kayıp" penceresi (gün); 0 ise bir mevsim.</summary>
        public double SharedLossDays;
        /// <summary>İhtiyaç bu yüzdenin altına inince (≤%10) öğle arası öne alınır.</summary>
        public double CriticalNeed = 10.0;
        public double EveningTavernShare = 0.4;
        public int StockFood = 10, StockCooked = 8, StockRaw = 20, StockSeed = 15, StockOther = 6;
        /// <summary>Köylü kilerinin malları (ekmek, yumurta ...): oyuncu da köylüler de aynı stoktan alır.</summary>
        public int StockStaple = 24;

        public bool IsStaple(string item)
        {
            foreach (var p in Pantry) if (p.Item == item) return true;
            return false;
        }
        public readonly List<PantryStaple> Pantry = new List<PantryStaple>();
        public readonly List<PantryProduce> Produce = new List<PantryProduce>();
        public readonly List<VillageMeal> Meals = new List<VillageMeal>();
        /// <summary>Meslek → keyifli ustanın ikram ettiği küçük eşya (fırıncı simit, aktar nane, balıkçı kerevit).</summary>
        public readonly Dictionary<string, string> Treats = new Dictionary<string, string>(StringComparer.Ordinal);
        /// <summary>Meslek → iş döngüsü (life.json "villagerWork").</summary>
        public readonly Dictionary<string, List<VillagerWorkStep>> Work = new Dictionary<string, List<VillagerWorkStep>>(StringComparer.Ordinal);

        public List<VillagerWorkStep> WorkFor(string role) =>
            role != null && Work.TryGetValue(role, out var list) ? list : null;
    }
}
