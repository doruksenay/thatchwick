using System;
using UnityEngine;

namespace Thatchwick.Game.Platform
{
    /// <summary>
    /// Mağaza / platform hizmetleri (yayın altyapısı, 20.11.2026; belge 49 §5). Oyun kodu mağazayı doğrudan çağırmaz:
    /// başarım, ek paket (DLC) sahipliği ve cihaz bilgisi bu arayüzün arkasındadır. Bugün iki uygulama var:
    /// <see cref="NullPlatform"/> (mağazasız derleme, editör, mobil) ve <c>SteamPlatform</c> (yalnızca
    /// <c>STEAMWORKS_NET</c> derleme sembolüyle; Steamworks.NET paketi gerekir). Game Center ve Play Games ileride
    /// aynı arayüze takılır.
    /// </summary>
    public interface IPlatform
    {
        /// <summary>Günlük ve hata raporu için kısa ad: "steam", "yok".</summary>
        string Name { get; }
        /// <summary>Mağaza istemcisine bağlı mı (başarımlar oraya gidiyor mu).</summary>
        bool Connected { get; }
        /// <summary>Elde taşınan cihaz (Steam Deck): arayüz ölçeği ve kumanda ipuçları için.</summary>
        bool Handheld { get; }
        /// <summary>Her karede bir kez (mağaza geri çağrıları).</summary>
        void Tick();
        /// <summary>Başarımı mağaza tarafında açar. Yeniden çağırmak zararsızdır.</summary>
        void UnlockAchievement(string platformName);
        /// <summary>Ek paketin sahibi mi. Paket adları <see cref="PlatformConfig"/> içinde.</summary>
        bool OwnsDlc(string pack);
        void Shutdown();
    }

    /// <summary>
    /// Mağaza kimlikleri. Steamworks hesabı açılınca doldurulur (docs/yayin-altyapisi.md). 0 = henüz yok: Steam
    /// başlatılmaz, oyun mağazasız çalışır.
    /// </summary>
    public static class PlatformConfig
    {
        public const string AdultPack = "adult";

        /// <summary>Steam uygulama kimliği (Steamworks > App Admin).</summary>
        public const uint SteamAppId = 0;
        /// <summary>+18 paketinin (DLC) Steam uygulama kimliği.</summary>
        public const uint SteamAdultDlcId = 0;
    }

    /// <summary>Mağazasız çalışma. Başarımlar yalnızca yerel profilde kalır.</summary>
    public sealed class NullPlatform : IPlatform
    {
        private readonly bool _ownsDlc;

        /// <param name="ownsDlc">Mağazasız geliştirme derlemesinde ek paketler açıktır; mağaza derlemesi mağazaya
        /// bağlanamadıysa kapalıdır (sahiplik doğrulanamaz).</param>
        public NullPlatform(bool ownsDlc) { _ownsDlc = ownsDlc; }

        public string Name => "yok";
        public bool Connected => false;
        public bool Handheld => false;
        public void Tick() { }
        public void UnlockAchievement(string platformName) { }
        public bool OwnsDlc(string pack) => _ownsDlc;
        public void Shutdown() { }
    }

    /// <summary>Etkin platform. <see cref="Init"/> bir kez çalışır (sahne yeniden yüklense de aynı bağlantı kalır).</summary>
    public static class PlatformServices
    {
        private static IPlatform _current;

        public static IPlatform Current => _current ?? (_current = new NullPlatform(ownsDlc: !SteamBuild));

        /// <summary>Bu derleme Steam için mi yapıldı (<c>STEAMWORKS_NET</c> sembolü).</summary>
        public static bool SteamBuild
        {
            get
            {
#if STEAMWORKS_NET && !UNITY_ANDROID && !UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }

        public static void Init()
        {
            if (_current != null) return;
#if STEAMWORKS_NET && !UNITY_ANDROID && !UNITY_IOS
            try { _current = SteamPlatform.TryCreate(); }
            catch (Exception ex) { Debug.LogWarning($"Steam başlatılamadı: {ex.Message}"); }
#endif
            if (_current == null) _current = new NullPlatform(ownsDlc: !SteamBuild);
            Debug.Log($"Platform: {_current.Name} (bağlı: {_current.Connected})");
        }

        public static void Shutdown()
        {
            if (_current == null) return;
            try { _current.Shutdown(); }
            catch (Exception ex) { Debug.LogWarning($"Platform kapatılamadı: {ex.Message}"); }
            _current = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _current = null;   // editörde alan yeniden yükleme kapalıyken eski bağlantı kalmasın
    }
}
