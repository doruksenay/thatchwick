using System;
using System.IO;
using Thatchwick.Core.Diagnostics;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// Oyunun hata toplayıcısı (yayın altyapısı, 20.11.2026). Üç kaynağı tek günlükte birleştirir:
    /// Unity'nin yakaladığı her hata ve istisna (arayüz çizimi, Update, arka plan iş parçacıkları),
    /// simülasyon koruması (<see cref="SimGuard"/>) ve doğrudan bildirilenler (kayıt hatası gibi).
    /// Günlük: <c>persistentDataPath/thatchwick_errors.log</c> (dolunca <c>.1</c>). Oyuncuya çıkan bildirim ve
    /// ayarlardaki "raporu kopyala / klasörü aç" düğmeleri GameRoot tarafındadır (GameRootPlatform.cs).
    /// Ayrıca yarım kalan oturumu (çökme, zorla kapatma) bir sonraki açılışta bildirir.
    /// </summary>
    public static class ErrorReporter
    {
        public const string LogFileName = "thatchwick_errors.log";
        private const string MarkerFileName = "thatchwick_session.lock";

        private static ErrorJournal _journal;
        private static SessionMarker _marker;
        private static Func<string> _context;
        private static string _header;
        [ThreadStatic] private static bool _reporting;   // kendi yazdığımız konsol satırını yeniden kaydetmeyelim

        public static bool Ready => _journal != null;
        public static int Total => _journal != null ? _journal.Total : 0;
        public static int Distinct => _journal != null ? _journal.Distinct : 0;
        public static string LastSummary => _journal != null ? _journal.LastSummary : "";
        /// <summary>Önceki oturum düzgün kapanmadı (çökme, zorla kapatma, güç kesintisi).</summary>
        public static bool PreviousSessionUnclean { get; private set; }

        public static string LogFolder => Application.persistentDataPath;

        /// <summary>Bir kez kurulur; sahne yeniden yüklenince yeniden çağrılması zararsızdır.</summary>
        public static void Init()
        {
            if (_journal != null) return;
            string dir = Application.persistentDataPath;
            if (string.IsNullOrEmpty(dir)) return;
            RefreshHeader();
            _journal = new ErrorJournal(Path.Combine(dir, LogFileName), () => _header);
            _marker = new SessionMarker(Path.Combine(dir, MarkerFileName));
            PreviousSessionUnclean = !Application.isEditor && _marker.Begin(SessionNote());
            if (Application.isEditor) _marker.End();   // editörde oyun modu her an durdurulur; yanlış "çökme" uyarısı vermesin
            Application.logMessageReceivedThreaded += OnLog;
        }

        /// <summary>Hata bağlamı (oyun günü, yuva): her kayda eklenir. Ana iş parçacığı dışında çağrılabilir; hızlı olmalı.</summary>
        public static void SetContext(Func<string> context) => _context = context;

        /// <summary>Hata bildirir (konsola da yazar). Yeni bir hataysa true.</summary>
        public static bool Report(string source, Exception ex)
        {
            if (ex == null) return false;
            bool fresh = _journal != null && _journal.Report(source, ex, SafeContext());
            if (fresh || _journal == null) WriteConsole($"[{source}] {ex}");
            return fresh;
        }

        public static bool Report(string source, string message)
        {
            bool fresh = _journal != null && _journal.Report(source, message, null, SafeContext());
            if (fresh || _journal == null) WriteConsole($"[{source}] {message}");
            return fresh;
        }

        /// <summary>Oyuncuya henüz gösterilmemiş yeni hata sayısı (okununca sıfırlanır).</summary>
        public static int TakeUnseen() => _journal != null ? _journal.TakeUnseen() : 0;

        /// <summary>Geliştiriciye gönderilecek metin: sürüm, cihaz ve günlüğün son kısmı.</summary>
        public static string BuildReport()
        {
            RefreshHeader();
            return _journal != null ? _journal.BuildReport() : _header;
        }

        /// <summary>Uygulama düzgün kapanıyor ya da arka plana alınıyor.</summary>
        public static void SessionEnded() => _marker?.End();

        /// <summary>Uygulama ön plana döndü.</summary>
        public static void SessionResumed()
        {
            if (!Application.isEditor) _marker?.Mark(SessionNote());
        }

        /// <summary>
        /// Rapor başlığını yeniler (sürüm, cihaz, dil, platform). Unity bilgileri yalnızca ana iş parçacığında okunabilir;
        /// günlük başka iş parçacığından yazabildiği için başlık burada bir kez üretilip saklanır.
        /// </summary>
        public static void RefreshHeader()
        {
            try { _header = Header(); }
            catch (Exception) { if (_header == null) _header = "Thatchwick"; }
        }

        private static string Header()
        {
            return $"Thatchwick {Application.version} · Unity {Application.unityVersion} · {Application.platform}\n" +
                   $"{SystemInfo.operatingSystem} · {SystemInfo.deviceModel} · {SystemInfo.systemMemorySize} MB · {SystemInfo.graphicsDeviceName}\n" +
                   $"dil {GameLocalization.Current.Language} · platform {Platform.PlatformServices.Current.Name}";
        }

        private static string SessionNote() => $"{DateTime.UtcNow:o} {Application.version}";

        private static string SafeContext()
        {
            var c = _context;
            if (c == null) return null;
            try { return c(); }
            catch (Exception) { return null; }
        }

        private static void WriteConsole(string text)
        {
            _reporting = true;
            try { Debug.LogError(text); }
            finally { _reporting = false; }
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (_reporting || _journal == null) return;
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert) return;
            _journal.Report(type == LogType.Exception ? "unity" : "log", condition, stackTrace, SafeContext());
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            // Editörde alan yeniden yükleme kapalı: önceki oyun modundan kalan abonelik ve günlük bırakılır.
            Application.logMessageReceivedThreaded -= OnLog;
            _journal = null;
            _marker = null;
            _context = null;
            _header = null;
            PreviousSessionUnclean = false;
        }
    }
}
