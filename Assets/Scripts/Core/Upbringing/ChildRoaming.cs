using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Life;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Upbringing
{
    /// <summary>Çocuğun tek başına gidebileceği yer (belge 47 §7.2).</summary>
    public enum RoamPermission
    {
        /// <summary>Ev ve arsa (varsayılan).</summary>
        Home = 0,
        /// <summary>Arsa ve köy yolu.</summary>
        Road = 1,
        /// <summary>Arsa, köy yolu ve gündüz kasaba.</summary>
        Town = 2
    }

    /// <summary>Tek gezen çocuğu gören köylünün tepkisi.</summary>
    public sealed class GazeEvent
    {
        public Character Villager;
        public Character Child;
        /// <summary>Köylünün sözü (anahtarlı).</summary>
        public LocText Line;
        /// <summary>Hane itibarı değişimi (0 ya da eksi).</summary>
        public int Reputation;
        /// <summary>Köylü çocuğu eve getiriyor (gece ya da kötü hava).</summary>
        public bool Escort;
        /// <summary>Çocuk izinsiz, gece ya da kötü havada (azarlayan söz); değilse sevimli söz.</summary>
        public bool Scolding;
    }

    /// <summary>
    /// Çocukların gezme izni ve köyün gözü (belge 47 §7). Bebek bir büyüğü olmadan arsanın dışına çıkamaz. Çocuğun izni aile
    /// panelinden verilir; izin yoksa oyuncu en yakın yetişkinden o gün için izin isteyebilir (saat, hava, yaş, ruh hali ve
    /// özelliğe göre karar). Orman, ırmak, göl, sazlık, tepe ve madene tek başına hiçbir izinle gidilmez. İzinsiz ya da gece tek
    /// gezen küçüğü gören yetişkin köylü kendi kendine söylenir; aynı gün ikinci kez görülürse hane itibarı 1 düşer (mevsimde en
    /// çok 3), gece görülen çocuğu iyi kalpli bir köylü eve getirir ve ebeveyn mahcup olur.
    /// </summary>
    public sealed class ChildRoaming
    {
        public const double DayStartHour = 7.0, DayEndHour = 19.0;
        public const int GazeRange = 4;
        public const int EscortRange = 4;
        public const int SeasonReputationCap = 3;
        public const string AshamedMood = "mood.ashamed_parent";
        public const int AshamedValue = -5;
        /// <summary>Aynı köylü aynı çocuğa bu kadar oyun günü içinde ikinci kez söylenmez.</summary>
        public const double RepeatDays = 0.1;

        private readonly Dictionary<int, RoamPermission> _permissions = new Dictionary<int, RoamPermission>();
        private readonly Dictionary<int, (long day, RoamPermission level)> _todayGrants = new Dictionary<int, (long, RoamPermission)>();
        private readonly Dictionary<int, (long day, int count)> _sightings = new Dictionary<int, (long, int)>();
        private readonly Dictionary<long, double> _lastLine = new Dictionary<long, double>();
        private readonly Random _rng;
        private int _seasonPenalty;

        public ChildRoaming(int seed = 409) { _rng = new Random(seed); }

        /// <summary>Söylenme sözleri (ui anahtarları "roam.scold.0".."5").</summary>
        public const int ScoldCount = 6, KindCount = 4;
        public static LocText Scold(int i) => LocText.Of("roam.scold." + i);
        public static LocText Kind(int i) => LocText.Of("roam.kind." + i);

        public static LocText PermissionText(RoamPermission p) => LocText.Of("roam.perm." + p.ToString().ToLowerInvariant());

        public RoamPermission PermissionOf(int childId) => _permissions.TryGetValue(childId, out var p) ? p : RoamPermission.Home;
        public void SetPermission(int childId, RoamPermission p) { if (p == RoamPermission.Home) _permissions.Remove(childId); else _permissions[childId] = p; }

        /// <summary>Bugün geçerli izin (kalıcı izin ile bugün alınan iznin büyüğü).</summary>
        public RoamPermission Effective(int childId, long day)
        {
            var p = PermissionOf(childId);
            if (_todayGrants.TryGetValue(childId, out var g) && g.day == day && g.level > p) p = g.level;
            return p;
        }

        /// <summary>Bu yaşta tek gezme kuralı geçerli mi (bebek ve çocuk).</summary>
        public static bool IsMinor(Character c) => c != null && (c.Stage == AgeStage.Baby || c.Stage == AgeStage.Child);

        /// <summary>
        /// Küçük, bu bölgeye tek başına girebilir mi? Yasaksa nedeni (karakterin ağzından), serbestse boş.
        /// <paramref name="accompanied"/>: yanında hanenin bir yetişkini ya da ergeni var.
        /// </summary>
        public LocText Blocker(Character c, Region region, double hour, long day, bool accompanied)
        {
            if (!IsMinor(c) || region == null || region.Id == Regions.Farm.Id) return default;
            if (accompanied) return default;
            if (c.Stage == AgeStage.Baby) return LocText.Of("roam.block.baby");
            bool wild = region.Id != Regions.Road.Id && region.Id != Regions.Town.Id;
            if (wild) return LocText.Of("roam.block.wild", ("region", LocText.Of("region." + region.Id)));
            var p = Effective(c.Id, day);
            if (region.Id == Regions.Road.Id) return p >= RoamPermission.Road ? default : LocText.Of("roam.block.ask");
            if (p < RoamPermission.Town) return LocText.Of("roam.block.ask");
            if (hour < DayStartHour || hour >= DayEndHour) return LocText.Of("roam.block.dark");
            return default;
        }

        /// <summary>
        /// İzin iste: yetişkin saat, hava, çocuğun yaşı, kendi ruh hali ve özelliğine göre karar verir. Verilirse o gün geçerli.
        /// Dönen metin yetişkinin cevabı.
        /// </summary>
        public bool AskPermission(Character child, Character adult, RoamPermission wanted, double hour, long day, WeatherType weather, out LocText reply, double? roll = null)
        {
            if (child == null || adult == null) { reply = LocText.Of("roam.ask.nobody"); return false; }
            if (child.Stage == AgeStage.Baby) { reply = LocText.Of("roam.ask.baby", ("name", adult.Name)); return false; }
            if (hour < DayStartHour || hour >= DayEndHour) { reply = LocText.Of("roam.ask.dark", ("name", adult.Name)); return false; }
            if (weather == WeatherType.Storm) { reply = LocText.Of("roam.ask.storm", ("name", adult.Name)); return false; }
            double chance = wanted == RoamPermission.Town ? 0.55 : 0.8;
            double childDays = child.AgeDays - child.AgeConfig.ChildStartDay;   // çocukluğun kaçıncı günü
            if (childDays < 4) chance -= 0.25;   // daha küçük
            if (adult.HasTrait("trait.generous") || adult.HasTrait("trait.cheerful") || adult.HasTrait("trait.placid")) chance += 0.15;
            if (adult.MoodEnabled && adult.Mood.Total <= -10) chance -= 0.2;
            if (weather == WeatherType.Rain || weather == WeatherType.Snow) chance -= 0.15;
            bool yes = (roll ?? _rng.NextDouble()) < Math.Max(0.05, Math.Min(0.95, chance));
            if (yes)
            {
                _todayGrants[child.Id] = (day, wanted);
                reply = LocText.Of(wanted == RoamPermission.Town ? "roam.ask.yes_town" : "roam.ask.yes_road", ("name", adult.Name));
            }
            else reply = LocText.Of("roam.ask.no", ("name", adult.Name));
            return yes;
        }

        /// <summary>
        /// Köyün gözü: tek gezen küçük, yakınındaki yetişkin köylülerden birinin dikkatini çeker. İzinsiz, gece ya da kötü havada
        /// köylü söylenir (itibar etkisi olabilir); izinli ve gündüzse sevimli bir söz söyler. Olay yoksa null.
        /// </summary>
        public GazeEvent Observe(Character child, Region region, double hour, long day, double now, WeatherType weather, bool accompanied, IEnumerable<Character> villagers)
        {
            if (!IsMinor(child) || accompanied || region == null || region.Id == Regions.Farm.Id) return null;
            Character seer = null;
            int best = int.MaxValue;
            foreach (var v in villagers)
            {
                if (v == null || v.IsDead || !v.IsVillager || v.Stage == AgeStage.Baby || v.Stage == AgeStage.Child) continue;
                int d = Math.Abs(v.Position.X - child.Position.X) + Math.Abs(v.Position.Y - child.Position.Y);
                if (d > GazeRange || d >= best) continue;
                best = d; seer = v;
            }
            if (seer == null) return null;
            long key = ((long)seer.Id << 32) | (uint)child.Id;
            if (_lastLine.TryGetValue(key, out var last) && now - last < RepeatDays) return null;
            _lastLine[key] = now;

            bool night = hour < DayStartHour || hour >= DayEndHour;
            bool bad = weather == WeatherType.Storm || weather == WeatherType.Snow;
            bool allowed = Blocker(child, region, hour, day, false).IsEmpty;
            var ev = new GazeEvent { Villager = seer, Child = child };
            if (allowed && !night && !bad)
            {
                ev.Line = Kind(_rng.Next(KindCount));
                return ev;
            }
            ev.Scolding = true;
            ev.Line = night && _rng.NextDouble() < 0.5 ? Scold(3) : Scold(_rng.Next(ScoldCount));
            int count = _sightings.TryGetValue(child.Id, out var s) && s.day == day ? s.count + 1 : 1;
            _sightings[child.Id] = (day, count);
            int penalty = count >= 2 ? (child.Stage == AgeStage.Baby ? 2 : 1) : 0;
            if (penalty > 0 && _seasonPenalty < SeasonReputationCap)
            {
                penalty = Math.Min(penalty, SeasonReputationCap - _seasonPenalty);
                _seasonPenalty += penalty;
                ev.Reputation = -penalty;
            }
            if ((night || bad) && (seer.HasTrait("trait.generous") || seer.HasTrait("trait.placid") || _rng.NextDouble() < 0.5))
                ev.Escort = true;
            return ev;
        }

        /// <summary>Mevsim başında itibar tavanı sıfırlanır.</summary>
        public void OnSeasonChanged() => _seasonPenalty = 0;

        public int SeasonPenalty => _seasonPenalty;

        public ChildRoamingSave Export()
        {
            var ids = new List<int>(); var levels = new List<int>();
            foreach (var kv in _permissions) { ids.Add(kv.Key); levels.Add((int)kv.Value); }
            return new ChildRoamingSave { childIds = ids.ToArray(), levels = levels.ToArray(), seasonPenalty = _seasonPenalty };
        }

        public void Import(ChildRoamingSave s)
        {
            _permissions.Clear();
            _seasonPenalty = 0;
            if (s == null) return;
            if (s.childIds != null)
                for (int i = 0; i < s.childIds.Length && i < (s.levels?.Length ?? 0); i++)
                    if (Enum.IsDefined(typeof(RoamPermission), s.levels[i])) SetPermission(s.childIds[i], (RoamPermission)s.levels[i]);
            _seasonPenalty = Math.Max(0, s.seasonPenalty);
        }
    }

    [Serializable]
    public sealed class ChildRoamingSave
    {
        public int[] childIds;
        public int[] levels;
        public int seasonPenalty;
    }
}
