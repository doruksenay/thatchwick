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

        private MineMap Plot => _mines.Get(MineSystem.PlotId);

        public string CanDo(Character actor, GridPos tile, TileAction action, string arg = null)
        {
            var m = Plot;
            if (m == null) return "Önce bodrumdan aşağı bir merdivenle kuyu aç.";
            if (tile.Z != m.Level) return "Kazı kat -2'de yapılır.";
            switch (action)
            {
                case TileAction.Dig: return _mines.CanDig(m, tile.X, tile.Y, actor);
                case TileAction.Support: return _mines.CanSupport(m, tile.X, tile.Y, arg == "stone" ? SupportKind.StoneArch : SupportKind.Wood, actor);
                case TileAction.Bail: return m.Water > 0 ? null : "Kazıda su yok.";
                case TileAction.Light: return _mines.CanLight(m, tile.X, tile.Y, arg ?? "item.tallow_candle");
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
            action == TileAction.Dig && Plot != null ? _mines.DigDays(Plot, tile.X, tile.Y, actor) : Duration(actor, action);

        public void Apply(Character actor, GridPos tile, TileAction action, string arg = null)
        {
            var m = Plot;
            if (m == null || CanDo(actor, tile, action, arg) != null) return;
            switch (action)
            {
                case TileAction.Dig: _mines.Dig(m, tile.X, tile.Y, actor); break;
                case TileAction.Support: _mines.PlaceSupport(m, tile.X, tile.Y, arg == "stone" ? SupportKind.StoneArch : SupportKind.Wood, actor); break;
                case TileAction.Bail: _mines.Bail(m, actor); break;
                case TileAction.Light: _mines.PlaceLight(m, tile.X, tile.Y, arg ?? "item.tallow_candle"); break;
            }
        }
    }
}
