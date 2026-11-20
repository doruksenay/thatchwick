# Ses hattı (ElevenLabs)

Belgeler: Project 43 (ihtiyaç ve araçlar), 45 (hat), 45a (doğallık ve tekrar), 45b (uygulama özeti).

## Kurulum (bir kez, Windows cmd)
```
tools\audio-setup.cmd
```
- ffmpeg yoksa winget ile kurar.
- Python paketlerini yükler (`requirements.txt`).
- Repo kökünde `.env` açar. Dosyaya anahtarı yaz: `ELEVENLABS_API_KEY=...`
- `.env` git'e girmez (`.gitignore`).

Yoklayıcıyı Görev Zamanlayıcı'ya kurmak (15 dakikada bir): `tools\audio-poll-install.cmd`

## Akış
1. **İş dosyası:** `audio/queue/**/<id>-<ad>.md`. Claude yazar. Alanlar Project 45 bölüm 3 ve 45a bölüm 4'te.
2. **Üretim:** `python tools\audio\worker.py --once`. Yoklayıcı kullanılıyorsa onun içinde `--git` ile çalışır.
   - Kalan krediye ve `cost_max`'a bakar.
   - Adayları üretir, son işlemden (kırpma, LUFS, döngü, OGG) ve kapıdan geçirir.
   - Adaylar `.audio_work\<id>\` klasörüne ve masaüstündeki `ses\adaylar` klasörüne yazılır.
   - İş `review` durumuna geçer.
3. **İnceleme:** `tools\audio-review.cmd`. Tarayıcıda `localhost:8765` açılır.
   - Dinle; "Oyundaki gibi çal" ile 24 kez art arda, oyundaki rastgelelikle duy.
   - Seç; Kabul, Yeniden üret ya da Ret.
4. **Yükseltme:** `python tools\audio\promote.py --git` (yoklayıcı bunu da yapar).
   - Seçilenler `Assets\Resources\Audio\<Grup>\<aile>\` altına girer.
   - `soundbank.json` ve `audio\manifest.json` güncellenir.
   - Kayıpsız ana kopyalar masaüstündeki `ses\masters` klasörüne gider.
5. **Unity:** projeyi aç. Yeni `.ogg` ve `.meta` dosyalarını commit et.

## Krediye dokunmadan deneme
```
python tools\audio\worker.py --once --mock
python tools\audio\review.py
python tools\audio\promote.py --mock
```
Sentetik ses üretir. Hattın çalıştığını görmek içindir; sonra `.audio_work` klasörünü ve üretilen dosyaları sil.

## Komutlar
| Komut | Ne yapar |
|---|---|
| `worker.py --list` | hazır işler ve tahmini kredi |
| `worker.py --job s120` | tek iş |
| `gaps.py` | kodda olup sesi olmayan anahtarlar (`SoundKeys.cs`) |
| `gaps.py --draft` | eksikler için taslak iş (`approved_by: pending`) |
| `koy_dili.py` | hece envanteri ve örnek replikler |

## Unity'de dinleme (oyuna bağlamadan)
Play'e bas, sonra menüden **Thatchwick > Ses > Pilot: …**:
- ortam ve müzik;
- balta ×20;
- ayak sesi ×24;
- köy dili.

Rapor: **Thatchwick > Ses > Tekrar raporu**.

## Oyun kodundan çağırma
```csharp
using Thatchwick.Core.Audio;
using Thatchwick.Game.Audio;

AudioService.Instance.Play(SoundKeys.WorkChop, worldPos);                // iş vuruşu
AudioService.Instance.Play(SoundKeys.Step("dirt"), worldPos);            // ayak sesi (zemin)
AudioService.Instance.Play(SoundKeys.AnimalCow, worldPos, cow.Id);       // her ineğin kendi perdesi
AudioService.Instance.Play(SoundKeys.UiTap);                             // arayüz

AmbienceDirector.Instance.SetBed(SoundKeys.AmbFarmDay);
AmbienceDirector.Instance.SetWeatherBed(rain ? SoundKeys.WeatherRain : null);
AmbienceDirector.Instance.SetContext(new AmbienceContext { Hour = h, Season = s, Weather = AmbWeather.Clear, Indoor = inside });
MusicDirector.Instance.SetContext("spring.day");
MusicDirector.Instance.Stinger(SoundKeys.StingBirth);

string voice = VoicePlayer.VoiceKeyFor(app.Timbre, app.Pitch, out float fine);
VoicePlayer.Speak(voice, fine + VoicePlayer.AgeShift(stage), person.Id, VoiceEmotion.Happy, bubbleText.Length, pos);
VoicePlayer.Exclaim(voice, "pain", fine, person.Id, pos);
```
