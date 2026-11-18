using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Takvim, pazar günü ve festivaller (Adım 34). Festival günü gün ortasında herkes katılır: Eğlence/Sosyal
    /// dolar, şenlik ruh hali verilir, hesap "ilk kez" XP'si kazanılır. Pazar günü ve şenlik pazara ek teklif getirir.
    /// </summary>
    public sealed class FestivalService
    {
        public const double FestivalHour = 0.4;          // günün kaçıncı oranında kutlama başlar
        public const int MarketDayInterval = 5;          // her 5. gün pazar günü
        public const double FestivalMoodDays = 3.0;

        private readonly ContentDatabase _content;
        private readonly GameClock _clock;
        private long _celebratedDay = -1;

        public VillageEffects Village { get; set; }

        /// <summary>(festival, katılan kişi sayısı)</summary>
        public event Action<FestivalDef, int> Celebrated;
        /// <summary>Sabah festival günü başlayınca (arayüz bildirimi).</summary>
        public event Action<FestivalDef> Announced;
        private long _announcedDay = -1;

        public long CelebratedDay { get => _celebratedDay; set => _celebratedDay = value; }
        /// <summary>Şenliğin duyurulduğu son gün (gün ortası kayıttan yükleyince yeniden duyurulmasın).</summary>
        public long AnnouncedDay { get => _announcedDay; set => _announcedDay = value; }

        public FestivalService(ContentDatabase content, GameClock clock)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public FestivalDef TodaysFestival() => FestivalOn(_clock.DayIndex);

        public FestivalDef FestivalOn(long day)
        {
            int dps = _clock.Config.DaysPerSeason;
            var season = (Season)((day / dps) % TimeConfig.SeasonsPerYear);
            int dayOfSeason = (int)(day % dps);
            foreach (var f in _content.Festivals.All)
                if (f.Season == season && Math.Min(f.Day, dps - 1) == dayOfSeason) return f;
            return null;
        }

        /// <summary>Bugünden itibaren ilk festival ve kaç gün sonra (bugün dahil 0).</summary>
        public FestivalDef NextFestival(out int daysUntil)
        {
            long day = _clock.DayIndex;
            int span = _clock.Config.DaysPerYear;
            for (int i = 0; i < span; i++)
            {
                var f = FestivalOn(day + i);
                if (f != null) { daysUntil = i; return f; }
            }
            daysUntil = -1;
            return null;
        }

        public bool IsMarketDay(long day) => day % MarketDayInterval == MarketDayInterval - 1;
        public bool IsMarketDayToday => IsMarketDay(_clock.DayIndex);

        /// <summary>Market.ExtraOffers sağlayıcısı: pazar günü +1, festivalin ek teklifi.</summary>
        public int ExtraOffers()
        {
            var f = TodaysFestival();
            return (IsMarketDayToday ? 1 : 0) + (f != null ? f.ExtraOffers : 0);
        }

        /// <summary>Takvimde bir günün adı: "Sonbahar 3. gün, 2. yıl".</summary>
        public string DateText(long day)
        {
            int dps = _clock.Config.DaysPerSeason;
            var season = (Season)((day / dps) % TimeConfig.SeasonsPerYear);
            long year = day / _clock.Config.DaysPerYear + 1;
            return $"{SeasonName(season)} {day % dps + 1}. gün, {year}. yıl";
        }

        public static string SeasonName(Season s)
        {
            switch (s)
            {
                case Season.Spring: return "İlkbahar";
                case Season.Summer: return "Yaz";
                case Season.Autumn: return "Sonbahar";
                default: return "Kış";
            }
        }

        /// <summary>Her karede çağrılır; festival saatinde bir kez kutlar. Katılan sayısı (kutlama olmadıysa 0).</summary>
        public int Tick(IEnumerable<Character> everyone)
        {
            var f = TodaysFestival();
            if (f == null) return 0;
            long day = _clock.DayIndex;
            if (_announcedDay != day) { _announcedDay = day; Announced?.Invoke(f); }
            if (_celebratedDay == day || _clock.DayFraction < FestivalHour) return 0;
            _celebratedDay = day;

            int count = 0;
            int mood = f.MoodValue + (Village != null ? Village.FestivalMood : 0);
            foreach (var c in everyone)
            {
                if (c.IsDead || c.Stage == AgeStage.Baby) continue;
                if (f.FunRestore > 0) c.Needs.Restore(NeedType.Fun, f.FunRestore);
                if (f.SocialRestore > 0) c.Needs.Restore(NeedType.Social, f.SocialRestore * c.Modifiers.SocialGain);
                if (mood != 0) c.AddMoodlet("mood.festival", f.DisplayName, mood, FestivalMoodDays);
                count++;
            }
            Celebrated?.Invoke(f, count);
            return count;
        }
    }
}
