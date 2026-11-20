using System;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Mining
{
    /// <summary>
    /// Arsa kazısının karo işleri (JobRunner, FarmWork üzerinden): kaz, destek kur, su boşalt, ışık koy. Karakter kat -2'de
    /// açık bir komşu karodan çalışır; su ve destek işleri herhangi bir açık karodan yapılır.
    /// </summary>
    public sealed class MineWork : ITileWork, ITileDuration
    {
        private readonly MineSystem _mines;
        public const double SupportDays = 0.03, BailDays = 0.02, LightDays = 0.005;

        public MineWork(MineSystem mines) { _mines = mines ?? throw new ArgumentNullException(nameof(mines)); }

        /// <summary>Karonun ait olduğu kazı yeri: arsa kazısı (kat -2) ya da tepenin altındaki galeri (kat -1; belge 58d M1).</summary>
        private MineMap Site(GridPos tile, out int x, out int y) => _mines.SiteAt(tile, out x, out y);

        public string CanDo(Character actor, GridPos tile, TileAction action, string arg = null)
        {
            var m = Site(tile, out int x, out int y);
            if (m == null)
                return _mines.Get(MineSystem.PlotId) == null && _mines.Get(MineSystem.MineId) == null
                    ? "Önce bodrumdan aşağı bir merdivenle kuyu aç." : Localization.CoreText.T("ui.mine.not_dig_area");
            switch (action)
            {
                case TileAction.Dig: return _mines.CanDig(m, x, y, actor);
                case TileAction.Support: return _mines.CanSupport(m, x, y, arg == "stone" ? SupportKind.StoneArch : SupportKind.Wood, actor);
                case TileAction.Bail: return m.Water > 0 ? null : "Kazıda su yok.";
                case TileAction.Light: return _mines.CanLight(m, x, y, arg ?? "item.tallow_candle");
                default: return "Bilinmeyen iş.";
            }
        }

        public double Duration(Character actor, TileAction action)
        {
            switch (action)
            {
                case TileAction.Support: return SupportDays;
                case TileAction.Bail: return BailDays;
                case TileAction.Light: return LightDays;
                default: return MineSystem.HitDays * 3;     // kesin süre Apply anında karoya göre; iş tahmini ortalama taş
            }
        }

        /// <summary>Karoya göre gerçek süre: kazıda karonun sertliği, ışık ve su.</summary>
        public double Duration(Character actor, GridPos tile, TileAction action) =>
            action == TileAction.Dig && Site(tile, out int x, out int y) is MineMap m ? _mines.DigDays(m, x, y, actor) : Duration(actor, action);

        public void Apply(Character actor, GridPos tile, TileAction action, string arg = null)
        {
            var m = Site(tile, out int x, out int y);
            if (m == null || CanDo(actor, tile, action, arg) != null) return;
            switch (action)
            {
                case TileAction.Dig: _mines.Dig(m, x, y, actor); break;
                case TileAction.Support: _mines.PlaceSupport(m, x, y, arg == "stone" ? SupportKind.StoneArch : SupportKind.Wood, actor); break;
                case TileAction.Bail: _mines.Bail(m, actor); break;
                case TileAction.Light: _mines.PlaceLight(m, x, y, arg ?? "item.tallow_candle"); break;
            }
        }
    }
}
