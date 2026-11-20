using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Relations
{
    /// <summary>
    /// Çeyiz defteri (T3, Project belgesi 67c §2): haneye gelin gelenin getirdikleri (çeyiz sandığı, dokumalar) ve düğünde
    /// takılan takı onundur. Bunlar satılır ya da takasla verilirse gelin kocasına kırılır (satan kendisiyse kimse kırılmaz).
    /// Dokumanın evde iş için harcanması sayılmaz: yalnızca satış ve takas, ve yalnızca gelinin payı kadar.
    /// </summary>
    public sealed partial class RelationService
    {
        /// <summary>Düğünde geline takılan takı (T3'ün "düğün takısı").</summary>
        public const string WeddingJewel = "item.jw_bracelet";

        /// <summary>gelin → (eşya → hâlâ evde sayılan adet). Ekleniş sırası korunur: son gelen gelin önce bakılır.</summary>
        private readonly List<(int bride, Dictionary<string, int> goods)> _dowry = new List<(int, Dictionary<string, int>)>();

        /// <summary>Hane envanterine eşya koyar (düğün takısı).</summary>
        public Action<string, int> GiveItem { get; set; }

        public void RecordDowry(int brideId, string itemId, int count)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0) return;
            var goods = DowryOf(brideId, create: true);
            goods.TryGetValue(itemId, out int n);
            goods[itemId] = n + count;
        }

        public int DowryCount(int brideId, string itemId) =>
            DowryOf(brideId, create: false) is Dictionary<string, int> g && g.TryGetValue(itemId, out int n) ? n : 0;

        private Dictionary<string, int> DowryOf(int brideId, bool create)
        {
            foreach (var (b, g) in _dowry) if (b == brideId) return g;
            if (!create) return null;
            var goods = new Dictionary<string, int>();
            _dowry.Add((brideId, goods));
            return goods;
        }

        public void ForgetDowry(int brideId) => _dowry.RemoveAll(x => x.bride == brideId);

        /// <summary>
        /// Mal haneden çıktı (satış, takas): sold adet, sonrasında hanede ownedAfter tane kaldı. Gelinin payı eksildiyse T3.
        /// Gelinin payı, hanede kalandan fazla sayılmaz (evde iş için harcananlar sessizce düşer).
        /// </summary>
        public void OnGoodsLeft(string itemId, int sold, int ownedAfter, Character seller)
        {
            if (string.IsNullOrEmpty(itemId) || sold <= 0) return;
            int before = ownedAfter + sold;
            for (int i = _dowry.Count - 1; i >= 0; i--)
            {
                var (brideId, goods) = _dowry[i];
                if (!goods.TryGetValue(itemId, out int mine) || mine <= 0) continue;
                int share = Math.Min(mine, before);
                int spent = Math.Max(0, share - Math.Max(0, ownedAfter));
                goods[itemId] = Math.Max(0, Math.Min(share, ownedAfter));
                if (spent <= 0) continue;
                var bride = _population.Find(brideId);
                if (bride == null || bride.IsDead || !bride.SpouseId.HasValue) return;
                if (seller != null && seller.Id == bride.Id) return;   // kendi çeyizini kendisi sattı
                var husband = _population.Find(bride.SpouseId.Value);
                if (husband == null || husband.IsDead) return;
                ReportTrigger(SpouseTrigger.SpentDowry, husband, scale: DowryScale(itemId, spent));
                return;
            }
        }

        /// <summary>Kırgınlık katı: sandık ve takı ağır (1,2); dokuma adet başına artar (0,7…1,2).</summary>
        public static double DowryScale(string itemId, int spent)
        {
            if (itemId == WeddingJewel || itemId == "item.dowry_chest" || IsRing(itemId)) return 1.2;
            return Math.Min(1.2, 0.5 + 0.2 * spent);
        }
    }
}
