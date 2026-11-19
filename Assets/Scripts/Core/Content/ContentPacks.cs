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
    }
}
