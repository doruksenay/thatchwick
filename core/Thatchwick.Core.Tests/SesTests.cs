using System.Collections.Generic;
using System.Linq;
using Thatchwick.Core.Audio;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Ses mantığı (Project 45a bölüm 3-4). T-Ses.</summary>
    public class SesTests
    {
        [Fact]
        public void TorbaArtArdaAyniSesiVermez()
        {
            foreach (int n in new[] { 2, 3, 4, 8, 10 })
            {
                var bag = new ShuffleBag(n, new AudioRandom(7));
                int prev = -1;
                for (int i = 0; i < 2000; i++)
                {
                    int v = bag.Next();
                    Assert.InRange(v, 0, n - 1);
                    Assert.NotEqual(prev, v);
                    prev = v;
                }
            }
        }

        [Fact]
        public void TorbaYakinTekrarPenceresiniKorur()
        {
            // 8 varyasyon: son 4 çalınan yeniden seçilmez
            var bag = new ShuffleBag(8, new AudioRandom(3));
            var last = new Queue<int>();
            for (int i = 0; i < 5000; i++)
            {
                int v = bag.Next();
                Assert.DoesNotContain(v, last);
                last.Enqueue(v);
                if (last.Count > bag.NoRepeat) last.Dequeue();
            }
        }

        [Fact]
        public void TorbaHerVaryasyonuEsitCalar()
        {
            var bag = new ShuffleBag(8, new AudioRandom(11));
            var counts = new int[8];
            for (int i = 0; i < 8000; i++) counts[bag.Next()]++;
            Assert.True(counts.Min() > 800 && counts.Max() < 1200, string.Join(",", counts));
        }

        [Fact]
        public void KabulOlcutuOnCalmadaEnCokIki()
        {
            // 45a bölüm 4: A sınıfında aynı klip 10 çalmada en çok 2 kez
            var bag = new ShuffleBag(8, new AudioRandom(5));
            var seq = Enumerable.Range(0, 3000).Select(_ => bag.Next()).ToList();
            for (int i = 0; i + 10 <= seq.Count; i++)
                Assert.True(seq.Skip(i).Take(10).GroupBy(x => x).Max(g => g.Count()) <= 2);
        }

        [Fact]
        public void VaryasyonAraliktaKalir()
        {
            var r = new AudioRandom(1);
            for (int i = 0; i < 1000; i++)
            {
                var p = SoundVariation.Sample(r, 0.05f, 2f, 15f);
                Assert.InRange(p.Pitch, 0.95f, 1.05f);
                Assert.InRange(p.VolumeDb, -2f, 2f);
                Assert.InRange(p.DelayMs, 0f, 15f);
            }
        }

        [Fact]
        public void KisiyeOzguPerdeSabitVeFarkli()
        {
            float a1 = SoundVariation.EntityPitch(42, 1.5f), a2 = SoundVariation.EntityPitch(42, 1.5f);
            float b = SoundVariation.EntityPitch(43, 1.5f);
            Assert.Equal(a1, a2);
            Assert.NotEqual(a1, b);
            Assert.InRange(a1, SoundVariation.SemitonesToRatio(-1.5f), SoundVariation.SemitonesToRatio(1.5f));
            Assert.Equal(1f, SoundVariation.EntityPitch(0, 1.5f));
        }

        [Fact]
        public void YogunlukSiniriAyniAndaEnCokN()
        {
            var lim = new VoiceLimiter();
            int ok = 0;
            for (int i = 0; i < 10; i++)
                if (lim.TryStart("animal.chicken", 0.5f * i / 10f, 2f, 2, 0, out _)) ok++;
            Assert.Equal(2, ok);
            Assert.True(lim.TryStart("animal.chicken", 3f, 2f, 2, 0, out _)); // eskiler bitti
        }

        [Fact]
        public void HizliTekrarKisilirAyniKaredeYutulur()
        {
            var lim = new VoiceLimiter();
            Assert.True(lim.TryStart("work.chop", 1f, 0.5f, 4, 400, out var d0));
            Assert.Equal(0f, d0);
            Assert.False(lim.TryStart("work.chop", 1.05f, 0.5f, 4, 400, out _));  // 50 ms < 100 ms: yut
            Assert.True(lim.TryStart("work.chop", 1.3f, 0.5f, 4, 400, out var d1)); // 300 ms: kısık
            Assert.True(d1 < 0f);
        }

        [Fact]
        public void SaatAgirliklariToplamiBir()
        {
            for (float h = 0; h < 24; h += 0.25f)
                Assert.InRange(AmbienceScheduler.TodBlend(h).Sum(), 0.999f, 1.001f);
            Assert.True(AmbienceScheduler.TodBlend(12.5f)[1] > 0.99f);
            Assert.True(AmbienceScheduler.TodBlend(2f)[3] > 0.99f);
        }

        [Fact]
        public void KuslarGeceVeYagmurdaSusar()
        {
            var bird = new SpotDef { Key = "spot.bird.song", Bed = "amb.farm.day", IntervalMin = 5, IntervalMax = 10, Tod = new float[] { 1.6f, 1f, 0.7f, 0f } };
            int Count(float hour, AmbWeather w)
            {
                var s = new AmbienceScheduler(9);
                s.SetBed("amb.farm.day", 4);
                s.AddSpot(bird);
                var ctx = new AmbienceContext { Hour = hour, Season = 0, Weather = w };
                int n = 0;
                for (int i = 0; i < 6000; i++) n += s.Update(0.1f, ctx).Count; // 10 dakika
                return n;
            }
            int day = Count(10f, AmbWeather.Clear), night = Count(2f, AmbWeather.Clear), rain = Count(10f, AmbWeather.Rain);
            Assert.True(day > 30, "gündüz " + day);
            Assert.Equal(0, night);
            Assert.True(rain < day / 2, $"yağmur {rain} / gündüz {day}");
        }

        [Fact]
        public void NoktaSesiBaskaYatakaCalmaz()
        {
            var s = new AmbienceScheduler(1);
            s.SetBed("amb.town", 3);
            s.AddSpot(new SpotDef { Key = "spot.bird.song", Bed = "amb.farm.day", IntervalMin = 1, IntervalMax = 2 });
            var ctx = new AmbienceContext { Hour = 12, Season = 1, Weather = AmbWeather.Clear };
            for (int i = 0; i < 1000; i++) Assert.Empty(s.Update(0.1f, ctx));
        }

        [Fact]
        public void YatakParcalariArtArdaTekrarlanmaz()
        {
            var s = new AmbienceScheduler(4);
            s.SetBed("amb.farm.day", 5);
            int prev = -1;
            for (int i = 0; i < 500; i++) { int v = s.NextSegment(); Assert.NotEqual(prev, v); prev = v; }
        }

        [Fact]
        public void MuzikSonIkiParcayiTekrarlamaz()
        {
            var p = new MusicPlaylist(2);
            foreach (var k in new[] { "a", "b", "c", "d" }) p.Add("spring.day", k);
            var h = new List<string>();
            for (int i = 0; i < 500; i++)
            {
                var t = p.Next("spring.day");
                Assert.DoesNotContain(t, h.Skip(System.Math.Max(0, h.Count - 2)));
                h.Add(t);
            }
        }

        [Fact]
        public void MuzikUstBaglamaDuser()
        {
            var p = new MusicPlaylist(2);
            p.Add("spring", "s1");
            p.Add("default", "d1");
            Assert.Equal("s1", p.Next("spring.night"));
            Assert.Equal("d1", p.Next("winter.day"));
            Assert.Null(new MusicPlaylist(1).Next("spring.day"));
            var s = p.NextSilence();
            Assert.InRange(s, p.SilenceMin, p.SilenceMax);
        }

        [Fact]
        public void KoyDiliSoruSondaYukselirUzgunAlcalir()
        {
            var who = SpeakerProfile.FromSeed(77);
            float Slope(VoiceEmotion e)
            {
                float sum = 0;
                for (int k = 0; k < 50; k++)
                {
                    var n = GibberishComposer.Compose(40, 8, e, who, 1000 + k);
                    sum += n.Skip(6).Average(x => x.Semitones) - n.Take(2).Average(x => x.Semitones);
                }
                return sum / 50;
            }
            Assert.True(Slope(VoiceEmotion.Question) > 2f);
            Assert.True(Slope(VoiceEmotion.Sad) < -1f);
        }

        [Fact]
        public void KoyDiliAyniHeceArtArdaGelmezVeKisiFarkli()
        {
            var n = GibberishComposer.Compose(40, 14, VoiceEmotion.Neutral, SpeakerProfile.FromSeed(1), 5);
            for (int i = 1; i < n.Count; i++) Assert.NotEqual(n[i - 1].Index, n[i].Index);
            Assert.Equal(0f, n[n.Count - 1].GapMs);
            var a = SpeakerProfile.FromSeed(1);
            var b = SpeakerProfile.FromSeed(2);
            Assert.NotEqual(a.BaseSemitones, b.BaseSemitones);
            Assert.Equal(a.BaseSemitones, SpeakerProfile.FromSeed(1).BaseSemitones);
            Assert.InRange(GibberishComposer.SyllablesFor(1), 2, 14);
            Assert.InRange(GibberishComposer.SyllablesFor(400), 2, 14);
        }

        [Fact]
        public void SabitOzetDegismez()
        {
            // StableHash string.GetHashCode gibi süreçten sürece değişmemeli
            Assert.Equal(AudioRandom.StableHash("work.chop"), AudioRandom.StableHash("work.chop"));
            Assert.Equal(-468965076, AudioRandom.StableHash("a")); // FNV-1a("a") = 0xE40C292C
        }
    }
}
