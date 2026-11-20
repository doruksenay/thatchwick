#if UNITY_EDITOR
using System.Collections.Generic;
using Thatchwick.Core.Audio;
using Thatchwick.Game.Audio;
using UnityEditor;
using UnityEngine;

namespace Thatchwick.EditorTools.Audio
{
    /// <summary>
    /// Pilot sesleri oyuna bağlamadan Unity'de dinlemek için (Play modunda): Thatchwick > Ses > Pilot …
    /// Oyunda nasıl duyulacağını gösterir: karıştırma torbası, perde/ses oynaması, katmanlar, ortam ve müzik.
    /// </summary>
    public static class AudioPilotMenu
    {
        private static readonly List<(double at, System.Action act)> Queue = new List<(double, System.Action)>();
        private static bool _hooked;

        private static void Schedule(double delay, System.Action a)
        {
            if (!_hooked) { EditorApplication.update += Tick; _hooked = true; }
            Queue.Add((EditorApplication.timeSinceStartup + delay, a));
        }

        private static void Tick()
        {
            double now = EditorApplication.timeSinceStartup;
            for (int i = Queue.Count - 1; i >= 0; i--)
                if (Queue[i].at <= now) { var a = Queue[i].act; Queue.RemoveAt(i); a(); }
        }

        private static bool Ready()
        {
            if (Application.isPlaying) return true;
            Debug.Log("[Ses] Önce Play'e bas.");
            return false;
        }

        private static void Repeat(string key, int times, float spacing)
        {
            if (!Ready()) return;
            for (int i = 0; i < times; i++)
            {
                double d = i * spacing * Random.Range(0.85f, 1.2f);
                Schedule(d, () => AudioService.Instance.Play(key));
            }
        }

        [MenuItem("Thatchwick/Ses/Pilot: ortam ve müziği başlat (ilkbahar sabahı)")]
        private static void StartScene()
        {
            if (!Ready()) return;
            var amb = AmbienceDirector.Instance;
            amb.SetContext(new AmbienceContext { Hour = 9f, Season = 0, Weather = AmbWeather.Clear, Indoor = false });
            amb.SetBed(SoundKeys.AmbFarmDay);
            var mus = MusicDirector.Instance;
            mus.FirstSilence = 1f;
            mus.SetContext("spring.day");
        }

        [MenuItem("Thatchwick/Ses/Pilot: balta x20")]
        private static void Chop() => Repeat(SoundKeys.WorkChop, 20, 0.9f);

        [MenuItem("Thatchwick/Ses/Pilot: ayak sesi x24")]
        private static void Steps() => Repeat(SoundKeys.StepDirt, 24, 0.38f);

        [MenuItem("Thatchwick/Ses/Pilot: arayüz dokunma x12")]
        private static void Taps() => Repeat(SoundKeys.UiTap, 12, 0.45f);

        [MenuItem("Thatchwick/Ses/Pilot: köy dili (mutlu, soru, üzgün, kızgın)")]
        private static void Talk()
        {
            if (!Ready()) return;
            var emos = new[] { VoiceEmotion.Happy, VoiceEmotion.Question, VoiceEmotion.Sad, VoiceEmotion.Angry };
            double t = 0;
            for (int i = 0; i < emos.Length; i++)
            {
                var e = emos[i];
                Schedule(t, () => VoicePlayer.Speak("kalin_alcak", 0f, 1001, e, 30));
                t += 3.0;
            }
            Schedule(t, () => VoicePlayer.Exclaim("kalin_alcak", "laugh", 0f, 1001));
        }

        [MenuItem("Thatchwick/Ses/Pilot: aynı cümle 3 farklı kişi")]
        private static void ThreePeople()
        {
            if (!Ready()) return;
            for (int i = 0; i < 3; i++)
            {
                long id = 2000 + i;
                Schedule(i * 3.0, () => VoicePlayer.Speak("kalin_alcak", 0f, id, VoiceEmotion.Neutral, 30));
            }
        }
    }
}
#endif
