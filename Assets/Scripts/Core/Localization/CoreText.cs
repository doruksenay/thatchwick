using System;

namespace Thatchwick.Core.Localization
{
    /// <summary>
    /// Core'un metin döndüren eski imzalarında (neden dizgileri, ipucu satırları) anahtarlı metne geçiş köprüsü.
    /// Yeni kod mümkünse <see cref="LocText"/> döndürür; imzası <c>string</c> olan yerlerde sabit Türkçe metin yerine
    /// <c>CoreText.T("ui.farm.block.already_staked")</c> yazılır. Oyun katmanı açılışta <see cref="Resolve"/>'u kurar
    /// (seçili dil); kurulmadıysa (testler) anahtarın kendisi döner.
    /// </summary>
    public static class CoreText
    {
        /// <summary>Anahtarlı metni seçili dile çevirir (GameRoot kurar).</summary>
        public static Func<LocText, string> Resolve { get; set; }

        public static string T(string key, params (string name, object value)[] args)
        {
            var resolve = Resolve;
            return resolve != null ? resolve(LocText.Of(key, args)) : key;
        }
    }
}
