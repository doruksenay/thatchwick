#if STEAMWORKS_NET && !UNITY_ANDROID && !UNITY_IOS
using System;
using Steamworks;
using UnityEngine;

namespace Thatchwick.Game.Platform
{
    /// <summary>
    /// Steam bağlantısı (Steamworks.NET). Yalnızca <c>STEAMWORKS_NET</c> sembolüyle derlenir; kurulum adımları
    /// docs/yayin-altyapisi.md'de. Steam istemcisi kapalıysa ya da kimlik girilmemişse null döner ve oyun mağazasız
    /// sürer (ek paketler kapalı kalır).
    /// </summary>
    public sealed class SteamPlatform : IPlatform
    {
        private bool _alive;

        private SteamPlatform() { _alive = true; }

        public static SteamPlatform TryCreate()
        {
            if (PlatformConfig.SteamAppId == 0) return null;
            try
            {
                // Oyun Steam dışından başlatıldıysa Steam üzerinden yeniden başlatılır (kopya koruması değil; bağlam için).
                if (!Application.isEditor && SteamAPI.RestartAppIfNecessary(new AppId_t(PlatformConfig.SteamAppId)))
                {
                    Application.Quit();
                    return null;
                }
                if (!SteamAPI.Init()) return null;
            }
            catch (DllNotFoundException ex)
            {
                Debug.LogWarning($"Steam kitaplığı bulunamadı: {ex.Message}");
                return null;
            }
            return new SteamPlatform();
        }

        public string Name => "steam";
        public bool Connected => _alive;
        public bool Handheld => _alive && SteamUtils.IsSteamRunningOnSteamDeck();

        public void Tick()
        {
            if (_alive) SteamAPI.RunCallbacks();
        }

        public void UnlockAchievement(string platformName)
        {
            if (!_alive || string.IsNullOrEmpty(platformName)) return;
            if (SteamUserStats.GetAchievement(platformName, out bool has) && has) return;
            if (SteamUserStats.SetAchievement(platformName)) SteamUserStats.StoreStats();
        }

        public bool OwnsDlc(string pack)
        {
            if (!_alive) return false;
            uint id = pack == PlatformConfig.AdultPack ? PlatformConfig.SteamAdultDlcId : 0;
            return id != 0 && SteamApps.BIsDlcInstalled(new AppId_t(id));
        }

        public void Shutdown()
        {
            if (!_alive) return;
            _alive = false;
            SteamAPI.Shutdown();
        }
    }
}
#endif
