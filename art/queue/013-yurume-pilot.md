---
id: "013"
title: Yürüme animasyonu pilotu (farmer_a2_56, SE, 4 yöntem)
status: review
approved_by: user        # kullanıcı onayı 2026-11-08 (sohbet)
cost_est: 6
cost_max: 6
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - art/results/013-yurume-pilot.md
  - Assets/Art/Iso/_aday/anim/
created: 2026-11-08
---
# 013 Yürüme animasyonu pilotu

## Amaç
Yürüme animasyonu için yöntem seçimi. Önceki south denemesinde üç sorun vardı: bacak lekesi, 46-49 px boy oynaması ve silüet sıçramaları. Dört yöntem aynı karakter ve aynı yönde denenir ve ölçülür. Sonuca göre kullanıcı yöntemi ve aynalama kararını verir. Plan: `art-brief-karakter-result.md`, bölüm "Yürüme animasyonu planı".

## Tarif

### Hedef
- Karakter: `farmer_a2_56`, PixelLab karakter `37cebe59-b761-4dd8-b23c-b11f60f58294` (60x60, v3, 8 yön; 011'de üretildi).
- Yön: yalnızca **south-east**.
- Durma karesi: SE rotasyon karesi.
  - Ham hali: `Assets/Art/Iso/_aday/_ham/iso_char_farmer_a2_56_cand_se.png`
  - Renk azaltılmış hali: `Assets/Art/Iso/_aday/iso_char_farmer_a2_56_cand_se.png`

### 0. Ön kontrol (ücretsiz)
- `get_character(37cebe59-...)` çağır. Karakter `completed` olmalı, 8 rotasyonu bulunmalı ve şablon animasyonları (`walk`) kullanılabilir olmalı.
- Şablon kullanılamıyorsa 1 ve 3 numaralı adımları atla. 2 ve 4 ile devam et, durumu raporla.
- Karakter erişilemiyorsa hiçbir çağrı yapma: `status: blocked` ve `## Sorular`.

### 1. template (1 gen)
`animate_character`:
- `mode=template`, `template_animation_id=walk` (6 kare)
- `directions=["south-east"]`, `ai_freedom=0`
- `animation_name=walk_pilot_template`

### 2. v3 özel (1 gen)
`animate_character`:
- `mode=v3`, `directions=["south-east"]`, `frame_count=6`, `keep_first_frame=true`
- `action_description="walking forward at a calm pace, arms swinging slightly, one full step cycle"`
- `end_frame_url`: SE ham rotasyon karesi (`get_upload_url` ile yükle). Son kare duruş pozuna doğru üretilir; döngü kapanışı bununla sağlanır.
- `animation_name=walk_pilot_v3`

### 3. skeleton-v3 (3 gen)
`animate_character`:
- `mode=skeleton-v3`, `template_animation_id=walk`
- `directions=["south-east"]`
- `animation_name=walk_pilot_skel`

### 4. pixminimax (1 gen)
`animate_character`:
- `mode=pixminimax`, `directions=["south-east"]`, `frame_count=8`, `keep_first_frame=true`
- `action_description="walking forward at a calm pace, one full step cycle"`
- `end_frame_url`: yine SE ham rotasyon karesi
- `animation_name=walk_pilot_pmm`

### Çağrı kuralları
- Her çağrıdan önce bu işte harcanan toplamı ve `get_balance`'ı yaz.
- Yanıttaki maliyet satırı tahminden farklıysa (1, 1, 3, 1) dur ve raporla. Toplam 6'yı geçemez.
- Eşzamanlı iş sınırı çıkarsa (çağrı reddedilir, ücret yazılmaz) 30 saniye bekle, bir kez daha dene.
- Yeniden üretim yok.
- `reduce_colors` kullanılmaz. Palet eşlemesi betikle, ücretsiz yapılır (aşağıda).

### Kayıt
- Ham kareler: `Assets/Art/Iso/_aday/anim/_ham/iso_char_farmer_a2_56_walk_<yöntem>_se_<NN>.png`
  - `<yöntem>`: `tmpl`, `v3`, `skel` ya da `pmm`
  - `NN`: 00'dan başlar; araç başlangıç karesini döndürüyorsa 00 o karedir, raporda belirt.
- İşlenmiş kareler: `Assets/Art/Iso/_aday/anim/iso_char_farmer_a2_56_walk_<yöntem>_se_<NN>.png`. Hepsi 60x60 tuvalde.
  1. Kareyi 60x60'a getir. Araç tuvali büyüttüyse ölçekleme yapma; şeffaf kenarı kırp ya da boşluk ekle, karakter yatayda ortalı kalsın.
  2. Ayak satırını hizala: en alt opak satırı durma karesinin ayak satırına kaydır. Yalnızca tamsayı dikey kaydırma.
  3. Palet eşlemesi: her pikseli 011'in 56 paletine en yakın renge eşle (RGB uzaklığı). Palet = `iso_char_farmer_a2_56_cand_*.png` dosyalarındaki renklerin birleşimi, en fazla 32. Alfa ikili kalır (0 ya da 255).
- Var olan dosyaların üzerine yazma. `.meta` yazma.

## Doğrulama
Ölçümler betikle yapılır, hem ham hem işlenmiş karelerde. Her yöntem için:

| Ölçüm | Hedef |
|---|---|
| Kare sayısı | — |
| Gövde boyu (opak bbox yüksekliği), kare başına ve aralık | Durma karesi ±1 px; durma karesi ~58 px |
| Ayak satırı aralığı, hizalamadan önce | — |
| Ardışık karelerde silüet IoU (min / ortalama) | Ani düşüş = silüet sıçraması |
| Döngü kapanışı: son kare ile ilk kare arasında IoU | Yüksek |
| Bacak bölgesinde lekelenme (alt %35'teki renk sayısı ve palet dışı piksel oranı, ham karelerde) | — |
| Renk sayısı, ham → işlenmiş | ≤32 |
| Yarı saydam piksel | 0 |
| Kimlik tutarlılığı (göz kontrolü) | Yüz, tunik tonu, kese ve kayış durma karesindeki gibi mi? |

Her yöntem için gözle kısa not: bacak lekesi, kol sallanması, adım okunurluğu.

## Önizleme (betikle, ücretsiz)
- **Karşılaştırma görseli:** `art/results/013-yurume-pilot-karsilastirma.png`
  - Dört yöntem alt alta, her satırda işlenmiş kareler yan yana.
  - 4 kat büyütme, en yakın komşu, #333333 zemin.
  - Satır başında köylü (`iso_char_villager_se.png`) ve durma karesi.
  - Her karakter kendi 64x32 elmas konturunun üzerinde, pivotu elmas merkezinde (`tools/art/char_preview.py` mantığı).
- **GIF:** her yöntem için `art/results/013-yurume-<yöntem>-x4.gif`.
  - İşlenmiş kareler, 4 kat, 64x32 elmas üzerinde, köylü yanında sabit.
  - 8 fps (kare süresi 125 ms), sonsuz döngü.
  - Kareler bitince döngü başa dönsün; durma karesi döngüye katılmaz, yalnızca yanda durur.
- **Masaüstü kopyası (kullanıcının brief'indeki açık istisna):**
  - `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_yurume_pilot\` içine ham ve işlenmiş kareler, karşılaştırma görseli ve dört GIF.
  - `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna yöntem başına bir satır ekle: `2026-11-08 | animasyon | karakter/2026-11-08_yurume_pilot | animate_character <mode> | <gen> | aday | <boy aralığı>, IoU <min>`
  - Silme ve üzerine yazma yok.

## Notlar
- 012 ile bağımlılık yok (farmer_a2_56 kullanılır). Sıra tek işçi kuralıyla korunur: 012 review olup kilit kalkınca başlar.
- Aynalama bu işte yok.
- Yöntem seçimi kullanıcıda. Rapora öneri yazılabilir; kalan yönler ve K2 karakterleri bu işte üretilmez.
- Rapor: `art/results/013-yurume-pilot.md`, başta `unity-import: pending`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
