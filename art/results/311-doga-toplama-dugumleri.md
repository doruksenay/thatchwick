unity-import: pending

# 311 Toplama düğümleri (rapor)

Sonuç: **TESLİM YOK**. 6 görselin tamamı `style.py check --family bitki` denetiminde KALDI (310'daki ile aynı neden: bitki ailesinde yalnızca 2 referans var, palet kapısı doygun yeşilleri eliyor). Yan yana sayfası: [311-yanyana.png](311-yanyana.png). Adaylar `art/results/311-red/` altında (Assets'e konmadı). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`, referanssız), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ortak ek ("dark brown wood" çıkarıldı, "muted natural colours" eklendi). Tuval tarifteki 64x56 yerine **80x72** (1x1 elması 64 px genişlik ve kenar boşluğu ister; 310'daki gibi). Pivot x=40, y(alttan)=24. Araçta `seed` yok.
Çağrılar: 6 ilk deneme × 1 gen = 6 generation (mantar çağrısı maskedeki yazım hatasıyla, dal çağrısı hız sınırıyla bir kez reddedildi, ücret yok; ikisi de yeniden gönderildi). Altısı da ilk denemede stil kapısında kaldı; ikinci deneme yapılmadı (310'da ikinci denemeler de kapıyı geçmedi, bütçe boşa harcanmadı). `get_balance` iş sonu: 947,70 (paralel şeritler de harcıyor; bu işin payı 6).

## Sonuç
| Parça | Stil denetimi | validate_iso | Not |
|---|---|---|---|
| berry_bush | KALDI (%39, x1.54) | geçti (20 renk) | çalı tanınır, toprak bloğu üstünde ("no ground" tutmadı) |
| mushroom | KALDI (%50, x1.74) | - | üç şapkalı mantar tanınır, zemin yok; yalnızca doygunluk/palet |
| hazel | KALDI (%23, x2.57) | - | çalı değil tam ağaç (turuncu yaprak); ölçek şartına uymuyor |
| herbs | KALDI (%3, x2.55) | - | çim zemin karosu üstünde; bitki ince |
| hive | KALDI (%63, x1.64) | BAŞARISIZ (51 renk > 48) | hasır (skep) kovan, kütük değil; altında çim elması |
| dry_branch | KALDI (%84, x2.42) | geçti (47 renk) | dal tanınır, yapraklar yeşil (kuru değil); en olumlu aday |

## Dosyalar
Teslim edilen yok. Adaylar: `art/results/311-red/{berry,mushroom,hazel,herbs,hive,branch}_raw.png` (80x72, pivot 40,24).

## Sorunlar ve öneri
- Bitki ailesi stil kapısı 310 ile aynı yüzden geçilemiyor; kullanıcı yan yana sayfasına bakıp kapıyı gevşetmeli ya da referans eklemeli. Sonra mantar, dal ve (renk indirimiyle) kovan `311-red/`'den teslim edilebilir; çalı, ot ve fındık yeniden üretim ister (zemin ve ölçek sorunu).
- Döşeme testi ve 6 düğüm + çimen karosu sayfası teslim olmadığı için yapılmadı.
