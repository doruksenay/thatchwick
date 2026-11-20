// Sahte Steamworks.NET tipleri: SteamPlatform.cs'in (STEAMWORKS_NET sembolü) Unity ve paket olmadan derleme denetimi.
// Yalnızca kullanılan üyeler; imzalar Steamworks.NET ile aynı. Gerçek pakette olmayan üye ekleme.
namespace Steamworks {
    public struct AppId_t { public uint m_AppId; public AppId_t(uint value) { m_AppId = value; } }
    public static class SteamAPI { public static bool Init() => false; public static bool RestartAppIfNecessary(AppId_t unOwnAppID) => false; public static void RunCallbacks() { } public static void Shutdown() { } }
    public static class SteamUserStats { public static bool GetAchievement(string pchName, out bool pbAchieved) { pbAchieved = false; return false; } public static bool SetAchievement(string pchName) => false; public static bool StoreStats() => false; }
    public static class SteamApps { public static bool BIsDlcInstalled(AppId_t appID) => false; }
    public static class SteamUtils { public static bool IsSteamRunningOnSteamDeck() => false; }
}
