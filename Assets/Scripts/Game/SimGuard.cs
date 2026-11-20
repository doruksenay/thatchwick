using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Simülasyon adımlarının hata yalıtımı (ikinci inceleme, 20.11.2026). Eskiden bir sistem hata fırlatınca o adımdaki
    /// sonraki bütün sistemler atlanır ve hata her adımda yinelenirdi: oyuncu donmuş bir oyun görürdü. Artık her sistem
    /// kendi korumasında çalışır; hata günlüğe (konsol ve dosya) yazılır, diğer sistemler sürer.
    /// Günlük dosyası ortak hata günlüğüdür (<see cref="ErrorReporter"/>): <c>persistentDataPath/thatchwick_errors.log</c>.
    /// </summary>
    public sealed class SimGuard
    {
        public const string LogFileName = ErrorReporter.LogFileName;
        /// <summary>Aynı sistemin ilk bu kadar hatası tam yazılır; sonrası her 200 hatada bir (günlük dolmasın).</summary>
        private const int FullReports = 3;

        private readonly Dictionary<string, int> _counts = new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>Bir sistem ilk kez hata verdiğinde (oyuncuya tek satırlık bilgi için): sistem adı.</summary>
        public event Action<string> FirstFailure;

        /// <param name="context">Hata bağlamı (oyun günü gibi); her hata kaydına eklenir.</param>
        public SimGuard(Func<string> context = null) { if (context != null) ErrorReporter.SetContext(context); }

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
            // Günlük, tekilleştirme ve dosya ErrorReporter'da (bütün hata kaynakları tek dosyada). Kurulmadıysa
            // (testler, çok erken açılış) yalnızca konsola yazar.
            ErrorReporter.Report("sim:" + system, ex);
            if (n == 1) FirstFailure?.Invoke(system);
        }
    }
}
