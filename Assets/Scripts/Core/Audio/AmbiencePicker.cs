namespace Thatchwick.Core.Audio
{
    /// <summary>
    /// Ortam yatağı seçimi (Project 45b; belge 50 ile kıyı eklendi): oyuncunun bulunduğu yere ve saate göre hangi
    /// yatak çalar. Oyun katmanı (GameRoot) yalnızca değiştiğinde AmbienceDirector.SetBed çağırır.
    /// </summary>
    public static class AmbiencePicker
    {
        public const float NightFrom = 20f, NightUntil = 5.5f;

        public static bool IsNight(float hour) => hour >= NightFrom || hour < NightUntil;

        /// <param name="regionId">Regions kimliği (farm, town, coast ...); bilinmiyorsa null.</param>
        public static string Bed(string regionId, bool indoor, bool underground, float hour)
        {
            if (underground) return SoundKeys.AmbMine;
            if (indoor) return SoundKeys.AmbInterior;
            switch (regionId)
            {
                case "coast": return SoundKeys.AmbCoast;
                case "town": return SoundKeys.AmbTown;
                default: return IsNight(hour) ? SoundKeys.AmbFarmNight : SoundKeys.AmbFarmDay;
            }
        }

        /// <summary>Hava katmanı: yağmur, fırtına; kar ve açık hava sessiz. İçeride ve yer altında da duyulur ama kısık (yönetici süzer).</summary>
        public static string WeatherBed(int weather, bool underground)
        {
            if (underground) return null;
            return weather == 1 ? SoundKeys.WeatherRain : weather == 3 ? SoundKeys.WeatherStorm : null;
        }

        /// <summary>Müzik bağlamı: "spring.day", "winter.night" (MusicPlaylist üst bağlama düşer).</summary>
        public static string MusicContext(int season, float hour)
        {
            string s = season == 0 ? "spring" : season == 1 ? "summer" : season == 2 ? "autumn" : "winter";
            return s + (IsNight(hour) ? ".night" : ".day");
        }
    }
}
