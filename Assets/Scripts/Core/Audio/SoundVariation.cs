using System;

namespace Thatchwick.Core.Audio
{
    /// <summary>Bir çalışın rastgele değerleri.</summary>
    public struct PlayParams
    {
        public float Pitch;      // çarpan (1 = olduğu gibi)
        public float VolumeDb;   // ek kazanç
        public float DelayMs;    // zamanlama sapması
    }

    /// <summary>
    /// Çalarken çeşitlilik (Project 45a bölüm 3): perde ve ses oynaması, zamanlama sapması,
    /// kişiye özgü sabit perde (her ineğin sesi biraz farklı ama hep kendi sesi).
    /// </summary>
    public static class SoundVariation
    {
        /// <param name="pitchVar">0,05 = ±%5</param>
        /// <param name="volVarDb">±dB</param>
        /// <param name="jitterMs">±ms (yalnız gecikme: 0..jitter)</param>
        public static PlayParams Sample(AudioRandom rng, float pitchVar, float volVarDb, float jitterMs)
        {
            return new PlayParams
            {
                Pitch = 1f + rng.Range(-pitchVar, pitchVar),
                VolumeDb = rng.Range(-volVarDb, volVarDb),
                DelayMs = jitterMs > 0 ? rng.Range(0f, jitterMs) : 0f,
            };
        }

        /// <summary>Varlık kimliğinden sabit perde çarpanı: ±maxSemitones yarım ton.</summary>
        public static float EntityPitch(long entityId, float maxSemitones)
        {
            if (entityId == 0 || maxSemitones <= 0) return 1f;
            var r = new AudioRandom(AudioRandom.StableHash(entityId));
            return SemitonesToRatio(r.Range(-maxSemitones, maxSemitones));
        }

        public static float SemitonesToRatio(float semitones) => (float)Math.Pow(2.0, semitones / 12.0);

        public static float DbToLinear(float db) => (float)Math.Pow(10.0, db / 20.0);
    }

    /// <summary>
    /// Yoğunluk sınırı (45a bölüm 3): aynı anahtardan aynı anda en çok N ses; bekleme süresi içinde
    /// yeniden tetiklenirse ses kısılır, çok sıkışırsa hiç çalmaz. On tavuk aynı anda gıdaklamaz.
    /// </summary>
    public sealed class VoiceLimiter
    {
        private sealed class State
        {
            public readonly System.Collections.Generic.List<float> Ends = new System.Collections.Generic.List<float>();
            public float LastStart = float.NegativeInfinity;
        }

        private readonly System.Collections.Generic.Dictionary<string, State> _states =
            new System.Collections.Generic.Dictionary<string, State>();

        /// <summary>Hızlı tekrarda düşülen ses (dB).</summary>
        public float RetriggerDuckDb = -6f;

        /// <returns>Çalınabilirse true; <paramref name="extraDb"/> ek kısma.</returns>
        public bool TryStart(string key, float now, float clipLength, int maxVoices, float cooldownMs, out float extraDb)
        {
            extraDb = 0f;
            if (!_states.TryGetValue(key, out var st)) { st = new State(); _states[key] = st; }
            st.Ends.RemoveAll(e => e <= now);
            if (maxVoices > 0 && st.Ends.Count >= maxVoices) return false;
            float since = (now - st.LastStart) * 1000f;
            if (cooldownMs > 0 && since < cooldownMs)
            {
                if (since < cooldownMs * 0.25f) return false; // aynı karede iki kez: yut
                extraDb = RetriggerDuckDb;
            }
            st.LastStart = now;
            st.Ends.Add(now + Math.Max(0.01f, clipLength));
            return true;
        }

        public int Active(string key, float now)
        {
            if (!_states.TryGetValue(key, out var st)) return 0;
            int n = 0;
            foreach (var e in st.Ends) if (e > now) n++;
            return n;
        }
    }
}
