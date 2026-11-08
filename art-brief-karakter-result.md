# Art Brief Karakter: Sonuç

## Faz K0: Plan (2026-11-08, harcama 0)

**Bakiye (`get_balance`, 13:37):** 1975 / 2000 generation, Tier 1 (Pixel Apprentice), yenilenme 2026-12-08. Bu brief'te harcanan: 0 / 120.

### Referans ölçüm: mevcut köylü (`iso_char_villager_*`, dokunulmadı)
| Yön | Tuval | Gövde boyu | Genişlik | Renk | Yarı saydam | Ayak altı boşluğu |
|---|---|---|---|---|---|---|
| se | 68x68 | 45 | 17 | 32 | 0 | 12 |
| s | 68x68 | 44 | 20 | 17 | 0 | 13 |
| e | 68x68 | 44 | 15 | 32 | 0 | 11 |
| n | 68x68 | 45 | 20 | 22 | 0 | 13 |

Gövde/tuval oranı yaklaşık 0,66. Hedef boylar için tuval: 48 px boy → 72, 56 px → 84, 64 px → 96. Gerçek boy üretimden sonra ölçülecek.

### Araçlar (açıklamalardan, 2026-11-08)
| Araç | Maliyet (8 yön) | Boyut | Detay ve kontrol | 2:1 uygunluğu |
|---|---|---|---|---|
| `create_character` v3, metinden | 2 (≤90 px), 3 (91-128) | kare, 256'ya kadar | En detaylı. Seed yok. `max_colors` ve palet yok. Dış hat yalnızca ipucu | `view` seçenekleri: low (~20°), **high top-down (~35°)**, side. 30°'ye en yakın high |
| `create_character` v3, referansla | ceil(w·h·8/65536), 64x96 için 1 | referansın boyutu | Verilen güney sprite'ını 8 yöne döndürür | Güney sprite'ı bizim çizmemiz gerekir |
| `create_character` standard | 1 | 128'e kadar (tuval ~%40 büyük) | Şablon iskelet, en az detay. `max_colors` yok | low/high/side |
| `create_character` pro | 10 (≤85), 15 (86-113), 25 (114-128) | 128'e kadar | `max_colors`, `style_character_id` ile set tutarlılığı. Pahalı | high |
| `create_character_pro_flash` | 6 (16-88), 7 (92-96), 8 (100-128). Ücretsiz sorguyla doğrulandı: 84x84 = 5 görüntü + 1 döndürme = 6 | 4'ün katı, özel boyut beta | Seed var, `max_colors` var, `style_image` ile set boyunca stil çapası | low/high/side |

### Plan
1. **K1 aracı:** `create_character` v3, `view=high top-down`, `outline=single color black outline`, `detail=high detail`. Gerekçe: en detaylı çıktı en ucuz fiyata. Brief'in ana hedefi detay, maliyet pro_flash'ın üçte biri.
2. **K1 boyutları:** tuval 72, 84 ve 96. Maliyet 2 + 2 + 3 = **7 generation**. Bu, brief'teki 120'lik tavanın %6'sı.
3. **K1 sınırı:** v3 seed almıyor. Üç boyut üç ayrı çizim olur, aynı tarifle ama yüz ve kıyafet birebir aynı çıkmaz. Boy karşılaştırması için yeterli, piksel düzeyinde birebir karşılaştırma değil.
4. **32 renk sınırı:** v3 bunu zorlayamaz. Her çıktıda renk sayısı ölçülecek. Aşılırsa elle renk azaltma yapılmayacak, rapora yazılacak. K2'de aşım sürerse `pro_flash` + `max_colors=32` seçeneği masada.
5. **K2 aracı (boy kararından sonra):** varsayılan v3, 8 karakter × 2-3 = **16-24 generation**. Seçilen K1 karakteri stil sapması yaparsa fallback: `create_character_pro_flash` + `style_image` = onaylı K1 SE sprite'ı + `max_colors=32`. Bu yolda 8 × 6-7 = **48-56 generation**. İki yol da 120 tavanı altında.
6. **Çocuk ve yaşlı:** v3'te `size` tuvali belirliyor, gövdeyi değil. Kısa boy yalnızca tarifle ("small child, about two thirds adult height") istenecek, sonuç ölçülecek. Tutmazsa çocuk daha küçük tuvalde üretilip seçilen tuvale ortalanarak yerleştirilir: piksel ölçeklemesi yok, yalnızca dolgu.

### Süreç: kuyruğa taşıma ve çelişkiler (karar senin)
- `CLAUDE.md`'ye göre kod oturumu PixelLab'de üretim yapmaz, iş kuyruktan `pixellab-artist` ile yürür. K0'da yalnızca ücretsiz okuma yaptım (`get_balance`, `get_pro_flash_capabilities`). K1'i **`art/queue/010-karakter-boy-testi.md`** olarak ekledim: `approved_by: pending`, onaylamadan çalışmaz.
- **Çelişki 1:** Brief "commit/push yapma" diyor. `CLAUDE.md` ise kuyruk düzeninin bu kısıtın yerine geçtiğini söylüyor; işçi kendi yollarını push eder.
- **Çelişki 2:** Brief masaüstü kopyası istiyor (`C:\Users\User\OneDrive\Desktop\art`). İşçi yalnızca `art/`, `Assets/Art/Iso/` ve `tools/art/` altına yazabiliyor. 010'un tarifine, senin brief'ine dayanarak, açık istisna olarak yazdım.
- **Çelişki 3:** İşçi kuyruğu numara sırasıyla işliyor. 001-008 (zemin, eşya, duvar) hâlâ `pending`. Öte yandan diğer oturum zemini eski brief düzeniyle yürütüyor (`_aday` altında çimen ve toprak adayları var). İşçiyi başlatırsam önce 001 ve 005'i alır ve diğer oturumun işini çift harcamayla tekrarlar.

## Faz K1: Boy ve detay testi (2026-11-08, kuyruk işi 010, status: review)

Ayrıntılı rapor: `art/results/010-karakter-boy-testi.md` (commit de5b254). Araç: `create_character` v3, `view=high top-down`, `outline=single color black outline`, `detail=high detail`.

### Generation dökümü
| Çağrı | Generation |
|---|---|
| v3 size 72 | 2 |
| v3 size 84 | 2 |
| v3 size 96 | 3 |
| **Bu brief toplam** | **7 / 120** |

Kalan bakiye (`get_balance`, iş sonrası): 1928. Hesap ortak: aynı aralıkta 007 (10 gen) ve paralel oturumlar da harcadı.

### SE ölçümleri (kod oturumunda yeniden ölçüldü, işçi raporuyla aynı)
| Hedef boy | Tuval | Gerçek gövde boyu | Genişlik | Renk | Yarı saydam | Pivot x | Pivot y (alttan) |
|---|---|---|---|---|---|---|---|
| 48 | 72x72 | **70** | 38 | 63 | 0 | 36,5 | 1 |
| 56 | 84x84 | **81** | 42 | 47 | 0 | 45,0 | 1 |
| 64 | 96x96 | **86** | 43 | 64 | 0 | 50,5 | 4 |

8 yönde renk sayıları: 48 için 48-64, 56 için 32-48, 64 için 53-64. Yarı saydam piksel hiçbir dosyada yok. `validate_iso.py --kind object` üç SE dosyasında da geçti.

### Detay farkı
| Detay | 48 | 56 | 64 |
|---|---|---|---|
| Yüz | Kaş ve göz okunuyor; burun yalnızca gölgeyle belli; ağız sakalda kayboluyor | Kaş, burun ve ağız çizgisi seçiliyor | Göz bebeği, kaş, burun ve ağız ayrı ayrı okunuyor; en iyisi |
| Eller | Yumruk lekesi | Parmak ipucu var | 2-3 parmak seçiliyor |
| Kemer, toka, kese | Var | Var, ayrıca omuz kayışı | Var, ayrıca omuz kayışı ve bileklik |
| Bacak sargısı | Zayıf okunuyor | Belirgin | Belirgin, şerit dokusu görünüyor |
| Ayakkabı bağı | Yok | Yok | Yok |
| Oran | İri baş, chibi'ye yakın | Gerçekçi | Gerçekçi |

Her boy kendi içinde 8 yönde tutarlı. Ancak seed olmadığı için üç boy birebir aynı tasarım değil; örneğin 64'te tunik daha turuncu, saç daha uzun ve kızıl.

### Sorunlar
1. **Boy hedefi tutmadı (K0 planındaki hata).** K0'da gövde/tuval oranını mevcut köylüden 0,66 varsaydım. v3 ise tuvali neredeyse tamamen dolduruyor (oran 0,90-0,97). Bu yüzden üç aday da hedefin çok üstünde: 70, 81 ve 86 px.
   - Mevcut köylü (45 px) ve 64x32 elmas ölçeğinde bu adaylar çok büyük kalır.
   - 48/56/64 gövde boyu için tuval yaklaşık 52, 60 ve 68 olmalı (tahmin).
   - O boylarda yeniden test: 2 + 2 + 2 = 6 gen, yeni karakter işi 011.
2. **Renk sayısı 32'nin üstünde** (yalnızca 56'nın S yönü 32'de kalıyor). 32 renk şartı K2'de de geçerli kalacaksa iki yol var:
   - `create_character_pro_flash` ile `max_colors=32`: 6 gen/karakter.
   - Ayrı bir renk azaltma adımı.
3. **Pivot y yöne göre 1-3 px değişiyor.** Arka yönler (n, nw, ne) daha yukarıda bitiyor. Unity'de yön başına pivot gerekir.
4. **Ayakkabı bağı** hiçbir boyda çizilmedi.

### Dosyalar
- Proje: `Assets/Art/Iso/_aday/iso_char_farmer_a_{48,56,64}_cand_{se,sw,nw,ne,s,w,n,e}.png` (24 PNG). Paralel oturumun 5703540 commit'i bunları `.meta`'larıyla birlikte erkenden commit etti; içerik işçinin ürettiğiyle bayt bayt aynı.
- Masaüstü:
  - `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_farmer_a_{48,56,64}\` (her birinde 8 PNG ve `_onizleme_x4.png`)
  - `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_K1_toplu_x4.png`
  - `_indeks.md`'ye 3 satır eklendi. Bu satırlardaki "38 gen." gibi notlar genişlik demek, generation değil.

## Faz K1b: Boy testi 2 (2026-11-08, kuyruk işi 011, status: review)

- Ayrıntılı rapor: `art/results/011-karakter-boy-testi-2.md` (commit 321c0b5).
- Araç: `create_character` v3 (`view=high top-down`, siyah dış hat, yüksek detay), ardından `reduce_colors` (`num_colors=32`, dithering yok). Renk azaltma her boyun 8 yönüne tek çağrıda uygulandı, yönler ortak paleti paylaşıyor.
- Açıklama metni 010 ile aynı. Referansla üretim yapılmadı: v3 referansı yeniden çizmiyor, yalnızca döndürüyor.

### Generation dökümü
| Çağrı | Generation |
|---|---|
| v3 size 52 | 2 |
| v3 size 60 | 2 |
| v3 size 68 | 2 |
| reduce_colors × 3 | 0,3 |
| **011 toplam** | **6,3** (cost_max 8) |
| **Bu brief toplam (010 + 011)** | **13,3 / 120** |

Kalan bakiye (`get_balance`, iş sonrası): 1896,7. Aynı aralıkta paralel oturumlar 10 gen harcadı.

### SE ölçümleri (kod oturumunda yeniden ölçüldü, işçi raporuyla aynı)
| Hedef | Tuval | Gerçek boy | Genişlik | Renk (ham → azaltılmış) | Yarı saydam | Pivot x | Pivot y (alttan) | ±3 px |
|---|---|---|---|---|---|---|---|---|
| 48 | 52x52 | 49 | 26 | 46 → 32 | 0 | 28,0 | 1 | tuttu |
| 56 | 60x60 | 58 | 32 | 57 → 32 | 0 | 31,5 | 1 | tuttu |
| 64 | 68x68 | 66 | 35 | 60 → 32 | 0 | 34,5 | 0 | tuttu |
| Köylü | 68x68 | 45 | 17 | 32 | 0 | 34,5 | 12 | — |

- **8 yön (renk azaltılmış):** renk sayısı 29-32, yarı saydam piksel 0.
- **Pivot y:** 0-3 px arası değişiyor; arka yönler 1-2 px yukarıda bitiyor. Unity'de yön başına pivot gerekir.
- **Alfa kanalı:** 24 karenin hepsinde hamla aynı.
- **Doğrulayıcı:** `validate_iso.py --kind object --max-colors 32` üç SE dosyasında da geçti.

### Detay farkı
| Detay | 48 | 56 | 64 |
|---|---|---|---|
| Yüz | Kaş ve sakal okunuyor, ağız kayboluyor | Kaş, burun ve sakal ayrı okunuyor | Bıyık ve ağız da ayrı okunuyor |
| Eller | Yumruk lekesi | 1 px parmak çizgisi | Parmak boğumları |
| Kemer, toka, kayış, kese | Var; kese küçük | Var | Var |
| Bacak sargısı | Var | Daha net | En net |
| Ayakkabı bağı | Yok | Yok | Yok |
| Oran | Tıknaz | En dengeli | İri |

- **Renk azaltma:** 8 kat büyütmede yüzü ve aksesuarları bozmadı; yalnızca birbirine çok yakın tonlar birleşti.
- **Tasarım tutarlılığı:** 010'dan iyi. Saç, sakal, kemer, omuz kayışı ve sargı üç boyda da ortak. Tunik tonu değişiyor: 48 sarı-kahve ve krem kollu, 56 turuncu-okra, 64 zeytin-okra. Bunun nedeni v3'ün seed almaması.

### Sorunlar
1. 64'ün SE ve SW yönleri tuvalin son satırına değiyor (pivot y 0). Görsel kontrolde ayaklar kesik görünmüyor.
2. Ayakkabı bağı hiçbir boyda çizilmedi.
3. K2'de karakterler arası stil tutarlılığı için iki yol var:
   - v3 + aynı tarif dili + ortak paletle `reduce_colors`. Ucuz: karakter başına yaklaşık 2,1 gen.
   - `mode=pro` + `style_character_id`. Pahalı: karakter başına 10-15 gen.

### Dosyalar
- **Proje:**
  - Renk azaltılmış: `Assets/Art/Iso/_aday/iso_char_farmer_a2_{48,56,64}_cand_<yön>.png`
  - Ham: `Assets/Art/Iso/_aday/_ham/iso_char_farmer_a2_{48,56,64}_cand_<yön>.png`
  - Önizleme betiği: `tools/art/char_preview.py`
- **Masaüstü:**
  - `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_farmer_a2_{48,56,64}\`: 8 yön, `_ham\` altında 8 ham yön ve `_onizleme_x4.png` (köylü ve aday 64x32 elmas üzerinde).
  - `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_K1b_toplu_x4.png`
  - `_indeks.md` sonuna 3 satır eklendi (generation sütunu: 2,1).

## Yürüme animasyonu planı (taslak, 2026-11-08, harcama 0, iş dosyası yok)

Hedef: `farmer_a2_56` (PixelLab karakter `37cebe59-b761-4dd8-b23c-b11f60f58294`, 60x60, 8 yön), 6 karelik yürüme ve 1 durma karesi. Maliyetler araç açıklamalarından alındı; çağrı yapılmadı.

### Ortak noktalar
- **Durma karesi:** mevcut rotasyon karesi (`iso_char_farmer_a2_56_cand_<yön>.png`), 0 gen. İstenirse nefes alma döngüsü `breathing-idle` (4 kare) şablonuyla eklenebilir; maliyeti yön başına 1 gen.
- **Palet:** Animasyon kareleri ham gelir. 011'deki 32 renklik palet, PixelLab'in `reduce_colors` aracıyla `palette_image` olarak kilitlenir; 512x512 piksel sınırı içinde 8 yön × 7 kare tek çağrıya sığar, maliyet 0,1 gen. Pilotta paleti betikle eşlemek bedava.
- **Ayak seviyesi:** Her karede en alt opak satır, betikle durma karesinin ayak satırına hizalanır (ölçekleme yok, yalnızca dikey kaydırma). Bu düzeltme bedava ama gövdenin kendi sıçramasını düzeltmez; o, araç seçimiyle çözülür.

### Seçenekler (60x60 karakter)
| Yöntem | Yön başına | 8 yön | Kare başına tutarlılık | Ayak ve boy sıçraması | Döngü kapanışı |
|---|---|---|---|---|---|
| **template** (`animate_character`, `walk`/`walking-6-frames`, 6 kare) | 1 (~92 px tuvale kadar) | **8** | Orta. Her kare iskeletten yeniden çiziliyor; önceki south denemesindeki bacak lekesi ve 46-49 px boy oynaması bu yolun tipik sorunu | Orta-zayıf. `ai_freedom=0` ile iskelete sıkı bağlı | Şablon döngüsel, ama kapanış ölçülmeli |
| **v3 özel** (`mode=v3`, `frame_count=6`, `end_frame` = durma karesi) | ceil(w·h·6/65536); tuval 60-100 px'e büyüse de **1** | **8** | Orta. Metinden hareket, her kare yeniden çiziliyor | Zayıf. Kontrol yok | En iyisi: `end_frame` ile son kare başlangıç pozuna doğru üretilir. Tek yönlü çağrı |
| **pixminimax** (`mode=pixminimax`) | 64 px fiyatı: 8 kare için **1**. 6 kare olmuyor, kare sayısı 4'ün katı olmak zorunda | **8** | Orta-iyi, uzun klipte akıcı. Beta | Bilinmiyor | `end_frame` var. **6 değil 8 kare** olur |
| **skeleton-v3 + şablon** (`mode=skeleton-v3`, `template_animation_id=walk`) | 4-14 kare = **3** | **24** | **En iyisi.** Karakteri yeniden çizmiyor, tek referanstan pozluyor. Kimlik ve renk sabit kalıyor; bacak lekesi ve silüet sıçramasına doğrudan çare. Beta | En iyisi. Poz iskeletten ve boy referanstan geliyor | Şablon döngüsü |
| skeleton-v3 özel (`move={"name":"walk"}` ya da kendi pozlarımız) | 3 (ilk çağrı ücretsiz talimat) | 24 | Yukarıdakiyle aynı. Adım genişliği `energy` ile ayarlanıyor | En iyisi | Bir döngü tam dolar |

**Yön aynalama:** sw/w/nw yönleri se/e/ne'nin yatay aynası alınarak üretilebilir. O zaman 8 yerine 5 yön üretilir: template/v3/pixminimax için 5 gen, skeleton-v3 için 15 gen. Bedeli: kese, kayış ve balta kını gibi tek taraflı aksesuarlar aynada taraf değiştirir. Simetrik karakterlerde sorun değil, kararı karakter bazında verirsin.

### K2 için maliyet (8 karakter, yürüme + palet kilidi)
| Yöntem | Karakter başına (8 yön) | 8 karakter | Aynalamayla (5 yön) karakter başına / 8 karakter |
|---|---|---|---|
| template | 8,1 | **~65** | 5,1 / ~41 |
| v3 özel | 8,1 | **~65** | 5,1 / ~41 |
| pixminimax (8 kare) | 8,1 | **~65** | 5,1 / ~41 |
| skeleton-v3 | 24,1 | **~193** | 15,1 / ~121 |

`farmer_a2_56` de animasyonlanırsa her satıra bir karakter daha eklenir (9 karakter). Çocuklar 40 px tuvalde, yaşlılar 56 px tuvalde; fiyat aynı ya da daha düşük. Aylık kalan bakiye (~1880) her yolu kaldırır, ama brief tavanı (120) ve günlük tavan (150) skeleton-v3'ün 8 yönlü tam setini tek günde karşılamaz.

### Pilot önerisi (tek karakter, tek yön, 5 gen)
- **Karakter ve yön:** `farmer_a2_56`, **SE** (izometrikte en çok görülen yön).
- **Hazırlık:** `get_character` (ücretsiz) ile v3 karakterinde şablon animasyonların açık olduğu doğrulanır. Açık değilse template ve skeleton-v3 satırları düşer ve pilot durur.
- **Üç yöntem aynı yönde yan yana denenir:**
  1. **template** `walk` (6 kare), `ai_freedom=0`: 1 gen
  2. **v3 özel**, `frame_count=6`, `end_frame` = SE durma karesi, "walking forward at a calm pace, arms swinging slightly": 1 gen
  3. **skeleton-v3**, `template_animation_id=walk` (6 kare): 3 gen
  - Toplam **5 gen**. Pixminimax 6 kare veremediği için pilota alınmadı; ayrıca 1 gen ile eklenebilir.
- **Ölçüm** (betikle, bedava), her yöntem için:
  - Kare başına gövde boyu: aralık, hedef ±1 px.
  - Ayak satırı aralığı, hizalamadan önce.
  - Ardışık karelerde silüet örtüşmesi (IoU).
  - Son kare ile ilk kare arasındaki IoU (döngü kapanışı).
  - Bacak bölgesinde ara ton ve renk sayısı (lekelenme göstergesi).
  - Palet eşlemesinden sonra renk sayısı.
- **Önizleme:**
  - Üç yöntem alt alta, 4 kat büyütülmüş.
  - 64x32 elmasın üzerinde, köylünün yanında.
  - Ayrıca GIF, 8-10 fps.
  - Hepsi masaüstüne de kopyalanır.
- **Karar kuralı:** skeleton-v3 belirgin şekilde daha iyiyse, maliyetin üç katına değip değmeyeceğini sen seçersin. template ya da v3 yeterliyse, 8 yön ve K2 seti 8,1 gen/karakter ile gider.

## Faz K2: Karakter seti (2026-11-08, kuyruk işi 012, status: review)

- Ayrıntılı rapor: `art/results/012-karakter-seti.md` (commit 948b633).
- Araç: v3 (`high top-down`, 8 yön) + `reduce_colors` (32 renk, 8 yön ortak palet).
- Tuval: yetişkinler 60, yaşlılar 56, çocuklar 40. Yaşlı ve çocuklar ölçeklemeden, yalnızca saydam boşlukla 60x60'a getirildi; ayak altı farmer_a2_56 ile aynı satırda. Betik: `tools/art/char_pad.py`.
- **Generation:** 8 × 2 + 8 × 0,1 = **16,8**. Bu brief'in toplamı: **30,1 / 120**. Bakiye 1890,6 → 1873,8.

### SE ölçümleri
| Karakter | Gerçek boy (hedef) | Renk ham → azaltılmış | Pivot x / y (alttan) | Pivot y, 8 yön (s, se, e, ne, n, nw, w, sw) | Ana giysi tonu |
|---|---|---|---|---|---|
| woman_a | 58 (58) | 62 → 32 | 30,5 / 1 | 2 1 1 3 3 3 1 1 | #817060 gri-kahve; #cbbb9d keten önlük |
| elder_m | 54 (54) | 50 → 31 | 31,0 / 1 | 1 1 1 2 2 2 1 1 | #7b502a kahve |
| elder_f | 54 (54) | 50 → 32 | 31,0 / 1 | 1 1 1 2 2 2 1 1 | #744038 kırmızı-kahve; #84775b şal |
| boy_a | 38 (~39) | 47 → 31 | 30,5 / 1 | 1 1 1 2 2 2 1 1 | #ebdeaa açık bej (setin en parlağı) |
| girl_a | 38 (~39) | 34 → 31 | 31,0 / 1 | 1 1 1 2 2 2 1 1 | #7a8a95 mavi-gri |
| woodcutter_a | 58 (58) | 52 → 32 | 32,0 / 1 | 1 1 1 2 3 2 1 1 | #535520 koyu zeytin |
| farmer_f | 60 (58, +2) | 51 → 32 | 31,5 / 0 | 1 0 0 2 3 2 0 0 | #c8b798 krem (okra istenmişti) |
| craftsman_a | 58 (58) | 64 → 32 | 31,5 / 1 | 1 1 1 2 3 2 1 1 | #774b28 deri önlük; mavi-gri kollar |

- Yarı saydam piksel: 64 karenin hiçbirinde yok.
- `validate_iso.py`: 8 SE dosyasının hepsi geçti.
- Çocuklar yetişkin boyunun %65,5'i.

### Sorunlar
1. **woodcutter_a:** Ten açık-orta çıktı, "dark skin" tutmadı. Balta kalçadaki kınında değil, elde taşınıyor.
2. **farmer_f:** 60 px. Şapka tuvalin üst satırına değiyor, ayaklar alt satırda; tuvalde pay kalmıyor.
3. **Yakın çiftler:** woman_a ile farmer_f, woodcutter_a ile craftsman_a (craftsman_a "ince" çıkmadı), boy_a ile girl_a (yalnızca silüette).
4. **Seçilmeyen detaylar:** boy_a'nın yırtık yakası ve yamalı pantolonu 38 px'te görünmüyor. elder_f'te sepet yerine yan çanta var.
5. **Masaüstü:** her karakterin `_ham\` klasöründe fazladan bir `_onizleme_x4.png` kopyası var. Masaüstünde silme yasak olduğu için duruyor.

## Yürüme pilotu (2026-11-08, kuyruk işi 013, status: review)

- Ayrıntılı rapor: `art/results/013-yurume-pilot.md` (commit 0a9126a).
- Karşılaştırma görseli: `art/results/013-yurume-pilot-karsilastirma.png`, yanında dört GIF.
- Karakter `farmer_a2_56`, yalnızca SE yönü.
- **Generation:** 1 + 1 + 3 + 1 = **6**. Bu brief'in toplamı **36,1 / 120**. Kalan bakiye 1867,8.

### Ölçümler
| Ölçüm | template | v3 özel | skeleton-v3 | pixminimax |
|---|---|---|---|---|
| Kare sayısı, ham tuval | 6, 60x60 | 7 (00 = durma), 60x84 | 6, 96x96 | 9 (00 = durma), 60x88 |
| Boy, işlenmiş (durma karesi 58) | 56-57 | 56-58 | 57-59 | 56-59 |
| Ardışık IoU, min / ortalama | 0,66 / 0,76 | 0,76 / 0,85 | 0,65 / 0,72 | 0,70 / 0,78 |
| Döngü kapanış IoU | 0,74 | 1,00 | 0,68 | 1,00 |
| Bacaktaki palet dışı oran (ham) | 0,38 | 0,29 | 0,41 | 0,29 |
| Renk ham → işlenmiş, yarı saydam | 55 → 32, 0 | 61 → 32, 0 | 54 → 32, 0 | 57 → 32, 0 |

### Gözle notlar
- **İşçinin notları:**
  - **template:** Gövde 01-03'te daralıyor; kese ve kayış kaybolup geri geliyor.
  - **v3:** Kimlik en iyi, ama adım çok kısa, kollar hareketsiz.
  - **skeleton-v3:** Adımlar en geniş ve okunur, ama silüet sıçraması en yüksek ve 60x60'a sığmıyor.
  - **pixminimax:** Kimlik iyi, adım okunur, bacak lekesi az.
- **Kod oturumunun gözlemi (karşılaştırma görselinden):** template ve skeleton-v3'ün son iki karesinde karakter yön değiştirmiş gibi duruyor: gövde öne dönük, kollar açık. Pixminimax'in son 3-4 karesi de durma karesinden biraz daha öne dönük ve geniş görünüyor. SE yönü döngü boyunca sabit kalmıyor olabilir; GIF'te kontrol edilmeli.

### Sorunlar
1. **Tepe kırpılması:** Ayaklar 60x60 tuvalde 58. satıra hizalanınca uzun karelerin tepesi kırpılıyor (skeleton-v3'te 2, pixminimax'te 1 satır). Çözüm iki yoldan biri: tuvali 64'e çıkarmak ya da ayak satırını değiştirmek.
2. **Tekrar eden son kare:** v3 ve pixminimax'te son kare duruş pozuna dönüyor, yani 00'ın tekrarı. Döngüde atılmalı; pixminimax için 8 karelik döngü 00-07 olur.
3. **Yatay ayak konumu:** Önizlemede her kare kendi ayak bandının ortasına yerleştiriliyor. Unity'de tek pivotla oynatınca yatay kayma farklı görünebilir.

### İşçinin önerisi (karar kullanıcıda)
- **Birinci tercih: pixminimax.** Yön başına 1 gen; 8 karakter ve 8 yön için ~65 gen.
- **İkinci aday: skeleton-v3.** Adımlar daha belirgin, ama 3 kat pahalı ve silüet sıçraması daha yüksek.
