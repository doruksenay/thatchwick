unity-import: pending

# 512 Easter egg sahne kareleri (çocuk 64 px): topaç, uluma, yerden alma

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = yönün durma karesi (`get_upload_url` ile yüklendi, 64x64). Metnin başına yön eklendi (`facing south-east: ...`). 24 çağrı (2 karakter x 3 anahtar x 4 çapraz yön), çağrı başına 1 generation (makbuz); toplam 24 (tahmin 24, tavan 34).
- Karakterler: boy_r64 `6940a34e-75c4-4cb6-b88d-8ef7e0e7719e`, girl_r64 `9d58ae77-0243-403e-84d3-33799a2f7e70` (ikisi `completed`).
- `get_balance` sonrası: 2266,7 (diğer şeritler eşzamanlı harcıyor, sayı yalnızca bu işe ait değil). Defter: art/ledger/512.md.
- Hız sınırı: iş yuvası "10/10" hatası birkaç çağrıda geldi (ücret yok); yuva açılınca aynı çağrılar gönderildi, çift üretim yok.
- Masaüstü kopyası: yerel
- Ham çıktı 64x84..96 (bir klipte 80x104), 9 kare. İşleme (015c/018e kuralları): her karenin en alt opak satırı durma karesinin ayak satırına çekildi; yatay: birleşik kutu ortası durma karesininkine. 64x64 tuvale maskeyle kırpma (ölçekleme yok). Palet: yönün durma karesinin renkleri, en yakın RGB, alfa ikili. Kapanış IoU > 0,93 ise son kare atıldı.
- Ham kareler art/tmp512 altında (commit edilmedi); işleme betikleri art/tmp altında.

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<boy|girl>_r64_<top|howl|pickup>_<se|sw|nw|ne>_<NN>.png`, hepsi 64x64, pivot x=32, pivot y=2 (ALTTAN). Toplam 197 PNG.

Kare sayısı (kapanış IoU):
| Anahtar | Erkek se/sw/nw/ne | Kız se/sw/nw/ne |
|---|---|---|
| top | 8 (1,00) / 8 (0,98) / 8 (0,99) / 8 (1,00) | 8 (0,98) / 8 (1,00) / 8 (0,98) / 8 (1,00) |
| howl | 8 (1,00) / **9** (0,75) / 8 (1,00) / **9** (0,92) | **9** (0,86) / **9** (0,73) / 8 (1,00) / **9** (0,88) |
| pickup | 8 (0,95) / 8 (1,00) / 8 (0,95) / 8 (0,94) | 8 (0,99) / 8 (0,94) / 8 (1,00) / 8 (0,95) |

9 kareli klipler (00-08): döngüye girerken 08'den 00'a geçiş sert olabilir (özellikle erkek howl sw 0,75, kız howl sw 0,73).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (--kind object --max-colors 32) | 197/197 geçti |
| style.py check --family karakter | 188 GEÇTİ, 9 UYARI, 0 KALDI. UYARI: "palet dışı renkler (kilit kurulmadı)", erkek howl se/sw karelerinde; `fit` uygulanmadı |
| Ayak satırı | kare başına en alt opak satır durma karesiyle aynı |
| Yan yana sayfası | art/results/512-yanyana.png (2 eski referans + 6 yeni kare, SE yönü, kare 03) |

## Kırpma ve kimlik
- Tuval dışına taşan opak piksel (toplam, klip başına): erkek top se 83, erkek top sw 22, erkek howl se 7, kız top se 33 (topaç ipi ve yan taşma); diğer 20 klipte 0.
- Tepe: en üst opak satır erkekte 0-2, kızda 3-6 (kırpılma yalnızca erkek top se / howl se'de, yukarıdaki sayılar).
- Kimlik (yüz, saç, giysi rengi): yan yana sayfasında gözle bakıldı, korunmuş görünüyor (erkek kahverengi saç ve keten tunik, kız kızıl örgü ve zeytin elbise). Otomatik ölçüm yok.
- Nesne okunurluğu (SE kare 03): erkek topaç ve ip okunuyor; kız topaç yerine uzun ip/çubuk okunuyor, topaç küçük. Uluma ve yerden alma iyi okunuyor. Kullanıcı GIF yerine kareleri Unity'de görsün.

## Sorunlar
- GIF üretilmedi (tarifte istenmişti); yalnızca yan yana sayfası var.
- Erkek top se kırpılması 83 piksel: ip ucu tuval dışında kesilmiş olabilir.
