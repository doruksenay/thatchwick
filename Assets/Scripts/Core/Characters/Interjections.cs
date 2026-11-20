using System;
using System.Collections.Generic;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Characters
{
    /// <summary>Ara klip (Project belgesi 69 §4): uzun işin, durmanın arasına giren kısa, tek seferlik hareket.</summary>
    public enum Interjection
    {
        None = 0,
        WipeBrow = 1,     // A1 alın terini siler
        BackStretch = 2,  // A2 belini tutup doğrulur
        Yawn = 3,         // A3 esner
        ScratchHead = 4,  // A4 başını kaşır
        ShakeOff = 5,     // A5 silkelenir
        HoldBelly = 6,    // A6 karnını tutar
        BlowHands = 7,    // A7 ellerine üfler
        SmallCheer = 8,   // A8 küçük sevinç
        Sigh = 9,         // A9 omuz silker, iç çeker
        Wave = 10,        // A10 el sallar
        Cough = 11,       // A11 öksürür
        RubHand = 12      // A12 eline bakar, ovuşturur
    }

    /// <summary>Ara klibin seçildiği an.</summary>
    public enum InterjectionMoment
    {
        Working = 0,
        Idle = 1,
        Walking = 2,
        WorkDone = 3
    }

    /// <summary>
    /// Ara klip seçimi (69 §4 kuralları): her klip bir nedene bağlıdır (koşulsuz rastgele oynama yok); bir karakterde aynı
    /// klip <see cref="PerClipCooldown"/> saniyeden sık oynamaz; ekranda aynı anda en çok <see cref="MaxConcurrent"/> kişi.
    /// Zaman gerçek saniyedir (oyun katmanı verir); Core saat tutmaz.
    /// </summary>
    public sealed class InterjectionPicker
    {
        public const double PerClipCooldown = 120.0;
        public const int MaxConcurrent = 2;
        /// <summary>Klip süresi (6 kare): kare yoksa kodla küçük hareket bu kadar sürer.</summary>
        public const double ClipSeconds = 1.2;

        private readonly Dictionary<long, double> _lastPlayed = new Dictionary<long, double>();
        private readonly Dictionary<int, double> _activeUntil = new Dictionary<int, double>();
        private readonly List<int> _scratch = new List<int>();
        private readonly Random _rng;

        public InterjectionPicker(int seed = 6969) { _rng = new Random(seed); }

        public static string Key(Interjection i)
        {
            switch (i)
            {
                case Interjection.WipeBrow: return "ix_wipe_brow";
                case Interjection.BackStretch: return "ix_back_stretch";
                case Interjection.Yawn: return "ix_yawn";
                case Interjection.ScratchHead: return "ix_scratch_head";
                case Interjection.ShakeOff: return "ix_shake_off";
                case Interjection.HoldBelly: return "ix_hold_belly";
                case Interjection.BlowHands: return "ix_blow_hands";
                case Interjection.SmallCheer: return "ix_small_cheer";
                case Interjection.Sigh: return "ix_sigh";
                case Interjection.Wave: return "ix_wave";
                case Interjection.Cough: return "ix_cough";
                case Interjection.RubHand: return "ix_rub_hand";
                default: return null;
            }
        }

        /// <summary>Şu an bu karakter için uygun klipler (nedeni olanlar). <paramref name="friendNear"/>: Arkadaş ya da daha yakın biri görüş alanında.</summary>
        public static void Candidates(in AnimInputs i, InterjectionMoment m, bool friendNear, bool morning, List<Interjection> into)
        {
            into.Clear();
            if (i.Stage == AgeStage.Baby) return;
            bool working = m == InterjectionMoment.Working;
            bool hot = i.Outdoors && i.Temperature >= 24f;
            bool coldOut = i.Outdoors && i.Temperature < 5f && i.Warmth < 40f;
            if (working && (i.Work == WorkContext.Heavy || i.Work == WorkContext.Bend) && (hot || i.Energy < 30f)) into.Add(Interjection.WipeBrow);
            if ((working && i.Work == WorkContext.Bend) || (m == InterjectionMoment.WorkDone && i.Work == WorkContext.Bend)
                || (i.Stage == AgeStage.Elder && working && i.Work != WorkContext.Calm)) into.Add(Interjection.BackStretch);
            if (m != InterjectionMoment.WorkDone && (i.Energy < 30f || (morning && m == InterjectionMoment.Idle))) into.Add(Interjection.Yawn);
            if ((working && i.SkillLevel > 0 && i.SkillLevel <= 3 && (i.Work == WorkContext.Craft || i.Work == WorkContext.Cook))
                || (i.Hal == Hal.Tense && m == InterjectionMoment.Idle)) into.Add(Interjection.ScratchHead);
            if (m != InterjectionMoment.Working && (i.Wetness > 0.3f || i.Dusty)) into.Add(Interjection.ShakeOff);
            if (i.Hunger < 25f && m != InterjectionMoment.WorkDone) into.Add(Interjection.HoldBelly);
            if (coldOut) into.Add(Interjection.BlowHands);
            if (m == InterjectionMoment.WorkDone && (i.Hal == Hal.Proud || i.Hal == Hal.Eager || i.SkillLevel >= 7)) into.Add(Interjection.SmallCheer);
            if (i.Hal == Hal.Sad && m != InterjectionMoment.Walking) into.Add(Interjection.Sigh);
            if (m == InterjectionMoment.Walking && friendNear && i.Hal != Hal.Sad && i.Hal != Hal.Cross) into.Add(Interjection.Wave);
            if (i.Dusty) into.Add(Interjection.Cough);   // tozlu: maden, harman, ahır (Dusty iç sesi)
            if (working && i.SkillLevel > 0 && i.SkillLevel <= 3 && (i.Work == WorkContext.Craft || i.Work == WorkContext.Cook)) into.Add(Interjection.RubHand);
        }

        private readonly List<Interjection> _cands = new List<Interjection>();

        /// <summary>
        /// Bir karakter için klip seçer ve oynuyor olarak işaretler; uygun klip yoksa, sınırlar doluysa ya da hepsi beklemedeyse None.
        /// </summary>
        public Interjection Pick(int characterId, in AnimInputs i, InterjectionMoment m, bool friendNear, bool morning, double now)
        {
            Prune(now);
            if (_activeUntil.ContainsKey(characterId) || _activeUntil.Count >= MaxConcurrent) return Interjection.None;
            Candidates(i, m, friendNear, morning, _cands);
            for (int k = _cands.Count - 1; k >= 0; k--)
                if (_lastPlayed.TryGetValue(KeyOf(characterId, _cands[k]), out double at) && now - at < PerClipCooldown) _cands.RemoveAt(k);
            if (_cands.Count == 0) return Interjection.None;
            var pick = _cands[_rng.Next(_cands.Count)];
            _lastPlayed[KeyOf(characterId, pick)] = now;
            _activeUntil[characterId] = now + ClipSeconds;
            return pick;
        }

        public bool IsPlaying(int characterId, double now) => _activeUntil.TryGetValue(characterId, out double until) && now < until;

        public int ActiveCount(double now) { Prune(now); return _activeUntil.Count; }

        private void Prune(double now)
        {
            _scratch.Clear();
            foreach (var kv in _activeUntil) if (now >= kv.Value) _scratch.Add(kv.Key);
            foreach (var id in _scratch) _activeUntil.Remove(id);
        }

        private static long KeyOf(int characterId, Interjection i) => ((long)characterId << 8) | (long)(int)i;
    }
}
