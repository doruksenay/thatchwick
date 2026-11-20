using Thatchwick.Core.Audio;
using UnityEngine;

namespace Thatchwick.Game.Audio
{
    /// <summary>
    /// Karakter sesi (Project 14 bölüm 11, 45a bölüm 2.5).
    /// Speak: konuşma balonu için köy dilini hece bankasından birleştirir (her seferinde farklı).
    /// Exclaim: gülüş, acı, şaşırma gibi hazır duygu klipleri.
    /// Perde: tını kaydırıcısı (−4..+4 yarım ton) + yaş evresi kayması + kişiye özgü kayma.
    /// </summary>
    public static class VoicePlayer
    {
        private static readonly AudioRandom Rng = new AudioRandom(System.Environment.TickCount);

        /// <param name="voice">Ses anahtarı, ör. "kalin_alcak" (audio/voices.json)</param>
        /// <param name="pitchSemitones">Karakter yaratmadaki perde + yaş kayması (çocuk tiz, yaşlı kalın)</param>
        /// <param name="characterId">Kişiye özgü konuşma profili için kimlik</param>
        /// <param name="textLength">Balondaki metnin uzunluğu (hece sayısını belirler)</param>
        /// <returns>Konuşmanın süresi (sn); ses yoksa 0.</returns>
        public static float Speak(string voice, float pitchSemitones, long characterId, VoiceEmotion emotion,
                                  int textLength, Vector3? worldPos = null)
        {
            var svc = AudioService.Instance;
            if (!svc.Bank.TryGet(SoundKeys.VoiceSyllables(voice), out var bank) || bank.clips.Length == 0) return 0f;
            var who = SpeakerProfile.FromSeed(AudioRandom.StableHash(characterId));
            var notes = GibberishComposer.Compose(bank.clips.Length, GibberishComposer.SyllablesFor(textLength), emotion,
                                                  who, (int)Rng.NextUInt());
            Pan(worldPos, out float pan, out float dist);
            float t = 0f;
            foreach (var n in notes)
            {
                var clip = svc.Bank.Clip(bank.clips[n.Index]);
                if (clip == null) continue;
                float pitch = SoundVariation.SemitonesToRatio(pitchSemitones + n.Semitones) * n.Rate;
                float len = svc.PlayClip(clip, AudioGroup.Voice, pitch, n.VolumeDb, pan, dist, t);
                // heceler hafifçe üst üste biner: akıcı konuşma
                t += len * 0.82f + n.GapMs / 1000f;
            }
            return t;
        }

        /// <summary>Hazır duygu klibi: state = greet, question, yes, no, happy, sad, angry, surprised, thinking,
        /// sleepy, pain, call, laugh, sigh.</summary>
        public static bool Exclaim(string voice, string state, float pitchSemitones, long characterId, Vector3? worldPos = null)
        {
            var who = SpeakerProfile.FromSeed(AudioRandom.StableHash(characterId));
            float ratio = SoundVariation.SemitonesToRatio(pitchSemitones + who.BaseSemitones);
            return AudioService.Instance.Play(SoundKeys.Voice(voice, state), worldPos, 0, 1f, ratio);
        }

        private static readonly string[] Timbres = { "kalin", "sicak", "berrak", "ince" };

        /// <summary>
        /// Karakter yaratmadaki tını (0 Kalın, 1 Sıcak, 2 Berrak, 3 İnce) ve perde (−4..+4) → kayıtlı ses ve kalan ince ayar.
        /// Her tınının iki kaydı var: alçak (−2) ve yüksek (+2); en yakını seçilir, fark perdeyle verilir (Project 14 bölüm 11).
        /// </summary>
        public static string VoiceKeyFor(int timbre, int pitchSemitones, out float residualSemitones)
        {
            string t = Timbres[Mathf.Clamp(timbre, 0, Timbres.Length - 1)];
            bool high = pitchSemitones >= 0;
            residualSemitones = pitchSemitones - (high ? 2f : -2f);
            return t + (high ? "_yuksek" : "_alcak");
        }

        /// <summary>Yaş evresine göre perde kayması (yarım ton).</summary>
        public static float AgeShift(string lifeStage)
        {
            switch (lifeStage)
            {
                case "Bebek": return 7f;
                case "Cocuk": case "Çocuk": return 5f;
                case "Ergen": return 2f;
                case "Yasli": case "Yaşlı": return -1.5f;
                default: return 0f;
            }
        }

        private static void Pan(Vector3? pos, out float pan, out float dist)
        {
            pan = 0f; dist = 0f;
            var cam = Camera.main;
            if (pos == null || cam == null) return;
            float half = cam.orthographic ? cam.orthographicSize * cam.aspect : 10f;
            Vector3 d = pos.Value - cam.transform.position;
            pan = Mathf.Clamp(d.x / half, -1f, 1f);
            dist = Mathf.Clamp01((new Vector2(d.x, d.y).magnitude / half - 0.4f) / 1.6f);
        }
    }
}
