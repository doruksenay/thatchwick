using System;
using System.Collections.Generic;

namespace Thatchwick.Core.Audio
{
    public enum VoiceEmotion { Neutral, Question, Happy, Sad, Angry, Surprised, Thinking, Sleepy }

    /// <summary>Birleştirilmiş konuşmada bir hece.</summary>
    public struct SyllableNote
    {
        public int Index;        // hece bankasındaki klip
        public float Semitones;  // karakter tabanına göre perde
        public float Rate;       // hız çarpanı (perdeden ayrı; Unity'de pitch'e katılır)
        public float GapMs;      // bu heceden sonraki boşluk
        public float VolumeDb;
    }

    /// <summary>Karakterin konuşma kimliği; kimliğinden (tohum) gelir, hep aynı kalır.</summary>
    public struct SpeakerProfile
    {
        public float BaseSemitones; // ses kaydırıcısına ek kişisel kayma
        public float Speed;         // 0,9 - 1,1
        public float Range;         // ezgi genişliği (yarım ton)
        public float WordGapMs;

        public static SpeakerProfile FromSeed(int seed)
        {
            var r = new AudioRandom(seed);
            return new SpeakerProfile
            {
                BaseSemitones = r.Range(-1.5f, 1.5f),
                Speed = r.Range(0.92f, 1.1f),
                Range = r.Range(1.5f, 3.5f),
                WordGapMs = r.Range(60f, 140f),
            };
        }
    }

    /// <summary>
    /// Köy dilini hece bankasından birleştirir (Project 45a bölüm 2.5; Animal Crossing yöntemi).
    /// Aynı tınıyı seçen iki kişi bile farklı duyulur; duygu tonlamayı değiştirir:
    /// soru sonda yükselir, kızgın hızlı ve kesik, üzgün yavaş ve alçalan.
    /// </summary>
    public static class GibberishComposer
    {
        /// <summary>Metin uzunluğundan hece sayısı (balondaki metnin okunma süresine yakın).</summary>
        public static int SyllablesFor(int textLength) => Math.Max(2, Math.Min(14, textLength / 3));

        public static List<SyllableNote> Compose(int bankSize, int syllables, VoiceEmotion emo,
                                                  SpeakerProfile who, int utteranceSeed)
        {
            var notes = new List<SyllableNote>(syllables);
            if (bankSize <= 0 || syllables <= 0) return notes;
            var rng = new AudioRandom(utteranceSeed);
            var bag = new ShuffleBag(bankSize, rng, Math.Min(bankSize - 1, 4));

            float baseSt = who.BaseSemitones, speed = who.Speed, range = who.Range, vol = 0f, gapMul = 1f;
            switch (emo)
            {
                case VoiceEmotion.Happy: baseSt += 1.5f; range *= 1.3f; speed *= 1.05f; break;
                case VoiceEmotion.Sad: baseSt -= 1.5f; range *= 0.6f; speed *= 0.85f; vol = -2f; gapMul = 1.4f; break;
                case VoiceEmotion.Angry: baseSt += 0.5f; range *= 0.7f; speed *= 1.15f; vol = 2f; gapMul = 0.6f; break;
                case VoiceEmotion.Surprised: baseSt += 2.5f; range *= 1.4f; break;
                case VoiceEmotion.Thinking: speed *= 0.9f; range *= 0.5f; gapMul = 1.6f; break;
                case VoiceEmotion.Sleepy: baseSt -= 2f; range *= 0.4f; speed *= 0.8f; vol = -3f; gapMul = 1.5f; break;
            }

            int wordLeft = rng.Range(2, 4);
            for (int i = 0; i < syllables; i++)
            {
                float t = syllables == 1 ? 1f : (float)i / (syllables - 1); // 0..1 cümle içi konum
                float contour;
                switch (emo)
                {
                    case VoiceEmotion.Question: contour = t > 0.7f ? (t - 0.7f) / 0.3f * 4.5f : -0.3f * t; break;
                    case VoiceEmotion.Sad: contour = -2f * t; break;
                    case VoiceEmotion.Surprised: contour = 2f - 3.5f * t; break;
                    case VoiceEmotion.Happy: contour = (i % 2 == 0 ? 0.8f : -0.4f) - 0.5f * t; break;
                    case VoiceEmotion.Sleepy: contour = -1.5f * t; break;
                    default: contour = -1f * t; break; // düz anlatım hafif alçalır
                }
                float st = baseSt + contour + rng.Range(-range, range) * 0.5f;
                bool wordEnd = --wordLeft <= 0;
                float gap = (wordEnd ? who.WordGapMs : rng.Range(5f, 25f)) * gapMul;
                if (emo == VoiceEmotion.Thinking && i == syllables / 2) gap += 250f; // ortada duraklama
                if (wordEnd) wordLeft = rng.Range(2, 4);
                float v = vol + rng.Range(-1.5f, 1.5f);
                if (emo == VoiceEmotion.Sleepy) v -= 4f * t; // sona doğru söner
                notes.Add(new SyllableNote
                {
                    Index = bag.Next(),
                    Semitones = st,
                    Rate = speed * rng.Range(0.96f, 1.04f),
                    GapMs = i == syllables - 1 ? 0f : gap,
                    VolumeDb = v,
                });
            }
            return notes;
        }
    }
}
