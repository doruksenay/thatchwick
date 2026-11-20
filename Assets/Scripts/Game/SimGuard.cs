using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Simülasyon adımlarının hata yalıtımı (ikinci inceleme, 20.11.2026). Eskiden bir sistem hata fırlatınca o adımdaki
    /// sonraki bütün sistemler atlanır ve hata her adımda yinelenirdi: oyuncu donmuş bir oyun görürdü. Artık her sistem
    /// kendi korumasında çalışır; hata günlüğe (konsol ve dosya) yazılır, diğer sistemler sürer.
    /// Günlük dosyası: <c>persistentDataPath/thatchwick_errors.log</c> (en çok ~256 KB; dolunca baştan başlar).
    /// </summary>
    public sealed class SimGuard
    {
        public const string LogFileName = "thatchwick_errors.log";
        private const long MaxLogBytes = 256 * 1024;
        /// <summary>Aynı sistemin ilk bu kadar hatası tam yazılır; sonrası her 200 hatada bir (günlük dolmasın).</summary>
        private const int FullReports = 3;

        private readonly Dictionary<string, int> _counts = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Func<string> _context;

        /// <summary>Bir sistem ilk kez hata verdiğinde (oyuncuya tek satırlık bilgi için): sistem adı.</summary>
        public event Action<string> FirstFailure;

        public SimGuard(Func<string> context = null) { _context = context; }

        public int TotalErrors { get; private set; }
        public int ErrorsOf(string system) => _counts.TryGetValue(system, out int n) ? n : 0;

        /// <summary>Adımı çalıştırır; hata olursa kaydeder ve false döner (çağıran sonraki sistemle sürer).</summary>
        public bool Run(string system, Action step)
        {
            try { step(); return true; }
            catch (Exception ex) { Report(system, ex); return false; }
        }

        public void Report(string system, Exception ex)
        {
            TotalErrors++;
            _counts.TryGetValue(system, out int n);
            _counts[system] = ++n;
            if (n > FullReports && n % 200 != 0) return;
            string where = _context != null ? SafeContext() : "";
            Debug.LogError($"[Sim] {system} hata verdi ({n}. kez){where}: {ex}");
            Append($"{DateTime.UtcNow:o} {system} #{n}{where}\n{ex}\n\n");
            if (n == 1) FirstFailure?.Invoke(system);
        }

        private string SafeContext()
        {
            try { return " [" + _context() + "]"; }
            catch (Exception) { return ""; }
        }

        private static void Append(string text)
        {
            try
            {
                string dir = Application.persistentDataPath;
                if (string.IsNullOrEmpty(dir)) return;
                string path = Path.Combine(dir, LogFileName);
                if (File.Exists(path) && new FileInfo(path).Length > MaxLogBytes) File.Delete(path);
                File.AppendAllText(path, text);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
