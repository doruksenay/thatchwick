namespace Thatchwick.Core.Content
{
    /// <summary>
    /// İsteğe bağlı içerik paketleri. <see cref="Adult"/>: +18 paketi (şarap, at yarışı bahsi; belge 30, 32-8.3).
    /// Kapalıyken bu paketin etkileşimleri ve tarifleri hiç görünmez; açıkken yalnızca Genç yetişkin ve üstü yapar.
    /// Değeri oyun ayarından gelir (Game katmanı yükler); çekirdek varsayılanı kapalı.
    /// </summary>
    public static class ContentPacks
    {
        public static bool Adult { get; set; }

        /// <summary>Alkollü içkiler: yalnızca +18 paketiyle yapılır ve istenir (belge 45 karar 5, belge 00 §9).</summary>
        public static readonly string[] AlcoholItems = { "item.ale", "item.cider", "item.mead", "item.wine_new", "item.wine_aged" };

        /// <summary>Bu eşya paket yokken oyunda istenmemeli mi (sipariş, düğün listesi).</summary>
        public static bool IsHiddenAlcohol(string itemId) => !Adult && System.Array.IndexOf(AlcoholItems, itemId) >= 0;
    }
}
