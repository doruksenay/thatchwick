using System.Collections.Generic;

namespace Thatchwick.Core.Customization.Layers
{
    /// <summary>
    /// Dünya sprite'ında göz ve kaş biçimi (belge 43 §4.8): baş çapasına göre basılan küçük piksel desenleri
    /// ("damga"), 0 gen. Portredeki 20 göz ve 16 kaş, appearance.json'daki Tag ile bu 10'ar desene eşlenir.
    /// Desenler P1c pilotunda mankenin gerçek yüz ölçüsüne göre ayarlanacak; bugünkü hâller yer tutucu ama ayırt edilir.
    ///
    /// Göz deseni 3x3, merkez (1,1) göz merkezine oturur. Brow deseni 4x2, sol üst köşe gözün (x-1, y-3) noktasına.
    /// Karakterler: '.' dokunma, 'w' göz akı, 'i' iris, 'd' koyu (kirpik/kapak), 's' ten gölgesi, 'b' kaş.
    /// Desenler sol göz içindir; sağ göz yatay aynasıdır.
    /// </summary>
    public static class FacePatterns
    {
        public static readonly Dictionary<string, string[]> Eyes = new Dictionary<string, string[]>
        {
            ["normal"]     = new[] { ".d.", "wi.", "..." },
            ["wide"]       = new[] { "dd.", "wi.", "w.." },
            ["narrow"]     = new[] { "...", "di.", "..." },
            ["hooded"]     = new[] { "dd.", "di.", "..." },
            ["round"]      = new[] { ".d.", "wiw", ".w." },
            ["upturned"]   = new[] { "..d", "wi.", "..." },
            ["downturned"] = new[] { "d..", "wi.", "..." },
            ["small"]      = new[] { "...", ".i.", "..." },
            ["deep"]       = new[] { "s..", "wi.", "..." },
            ["sharp"]      = new[] { "ddd", ".i.", "..." },
        };

        public static readonly Dictionary<string, string[]> Brows = new Dictionary<string, string[]>
        {
            ["natural"]  = new[] { "bbb.", "...." },
            ["thick"]    = new[] { "bbbb", "bbb." },
            ["thin"]     = new[] { ".bb.", "...." },
            ["arched"]   = new[] { ".bb.", "b..b" },
            ["straight"] = new[] { "bbbb", "...." },
            ["bushy"]    = new[] { "bbbb", "bbbb" },
            ["unibrow"]  = new[] { "bbbb", "...." },   // iki göz görünürken araya köprü de çizilir
            ["sparse"]   = new[] { "b.b.", "...." },
            ["angled"]   = new[] { "..bb", "bb.." },
            ["short"]    = new[] { ".b..", "...." },
        };

        public const string DefaultEye = "normal";
        public const string DefaultBrow = "natural";
        public const int EyeOriginX = 1, EyeOriginY = 1;
        public const int BrowOffsetX = -1, BrowOffsetY = -3;

        public static string[] Eye(string pattern) =>
            pattern != null && Eyes.TryGetValue(pattern, out var p) ? p : Eyes[DefaultEye];

        public static string[] Brow(string pattern) =>
            pattern != null && Brows.TryGetValue(pattern, out var p) ? p : Brows[DefaultBrow];
    }
}
