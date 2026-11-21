unity-import: pending

# 747 Gönül yolu pozları 1

Durum: `review`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true` (9 kare), 112x112 ham tuval. Erkek `c63fe39d-…` (man_r84), kadın `135cb0b1-…` (woman_r84).
- 6 anahtar x 2 karakter = 12 çağrı, her çağrı 4 yön (SE, SW, NW, NE) = 48 iş, 1 gen/yön, toplam **48 gen** (`cost_est` 48, sapma yok). Çağrı başına makbuz 4 gen. `get_balance`: 9162 / 10000 (öncesi 9210; fark 48). Aynı anda en çok 3 çağrı.
- Gruplar (erkek): kiss 5ce27a31, kiss_cheek be11fda8, hold_hands 86028f94, offer_handkerchief eaf75b0d, serve_coffee 43259f34, drink_grimace 5734553e. (kadın): kiss 1e3d55f7, kiss_cheek 86ec644f, hold_hands 8361df4b, offer_handkerchief 40c1ac3c, serve_coffee 1cd93e2a, drink_grimace 43a74ac4.
- **Sapma:** `end_frame` (durma karesi) verilmedi; bulutta yükleme yolu yok (572 ile aynı). Başlangıç karesi yönün rotasyonu; döngü kapanışı kare 0 ile son kare IoU'suna göre denetlendi.
- Eylem metinleri tarifle aynı. Aynalama yok.

## İşleme
Merkez kırpma (14,14,98,98), ölçekleme yok; 32 renk paleti `iso_char_<cinsiyet>_r84_cand_se.png`'den en yakın RGB, alfa ikili. Kare 0 ile 8 IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 kare. Pivot x=42, y=2 (ALTTAN). Ayak satırı: SE/SW y=81, NW/NE y=79 (klip içinde sabit). Kırpma dışına taşan opak piksel yok; tepe satırına değen kare yok.

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<kiss|kiss_cheek|hold_hands|offer_handkerchief|serve_coffee|drink_grimace>_<se|sw|nw|ne>_<NN>.png`: 409 PNG, 84x84, pivot (42, 2 alttan). 48 klibin hepsi teslim. Klip başına kare sayısı dosya adlarından sayılır (8 ya da 9).

## Doğrulama
| Denetim | Sonuç |
|---|---|
| `validate_iso.py` (her klibin 00 karesi, 48) | hepsi geçti |
| `style.py check --family karakter` (409 kare) | KALDI 0, UYARI 115, GEÇTİ 294. UYARI yalnızca "ailesinden doygun" (x1,25-1,30; ör. erkek drink_grimace NE); aile kilidi kurulmamış; teslimi engellemedi, `fit` gerekmedi |
| Ayak satırı | klip içinde sabit |
| Yan yana sayfası | `art/results/747-yanyana.png` (48 klibin 00 karesi + referanslar) |
| GIF | `art/results/747-<man ya da woman>-<anahtar>.gif` (4 yön yan yana, 12 dosya) |

## Sorunlar (kullanıcı görsel kararı için)
- Görsel içerik (öpücüğün okunurluğu, mendil/tepsi/fincanın seçilebilirliği) GIF'lerden kullanıcı tarafından değerlendirilmeli; işçi yalnızca teknik denetim yaptı. Pixminimax elde tutulan küçük nesneleri çoğu zaman belirsiz çizer (572'de de görüldü).
- serve_coffee ve offer_handkerchief klipleri 9 kare, son kare başlangıçtan çok farklı (IoU 0,64-0,89): tek seferlik poz, döngü değil.
- Karelere gömülü nesne (mendil, tepsi, fincan) oyunda ayrı eşya olarak çizilirse çift görünebilir.
- İki kişilik pozlarda (kiss, hold_hands) kareler tek karakterlidir; yüz yüze yön eşlemesi kod tarafında seçilir.
