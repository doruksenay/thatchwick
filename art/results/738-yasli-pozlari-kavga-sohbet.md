unity-import: pending

# 738 Yaşlı pozları: cane_threat, argue, sulk, laugh, tell_story, listen_nod (84 px)

- **Bu çalışmada yeni PixelLab çağrısı yapılmadı (0 gen).** `get_character` iki yaşlıda (elder_m_r84 `8dcaede5-cc4d-4068-b56f-ab5e327fea1c`, elder_f_r84 `b7f31e8d-29f2-4f6a-9092-a02759611159`) 6 anahtarın 4 yönünü (SE, SW, NW, NE; pixminimax, 9 kare) zaten gösteriyordu (20.11 tarihli, önceki sahiplenmeden; 48 klip = tarifteki 48 gen). Çift harcama olmasın diye yeniden üretilmedi; ham kareler indirilip işlendi. Maliyet 20.11 harcamasında kalır.
- `get_balance` sonrası: 9186 kalan (Tier 3; diğer şeritler de harcıyor).
- İşleme (018c/615 kuralları): ham 84-100 x 108-120; ölçekleme yok. Ayak satırı yönün durma karesine (elder_m: se_v3, sw/nw/ne_v2; elder_f düz ad), klip başına tek yatay ofset (kare 0 kutu ortası = durma karesi). Palet: karakterin 4 durma karesinin renkleri, en yakın RGB, alfa ikili. Kare 0-08 IoU > 0,95 ise 08 atıldı. Pivot x=42, y=2 (ALTTAN; ayak boşluğu yöne göre 2-5). Hiçbir karede opak piksel tuval dışına taşmadı (elder_f cane_threat SE ve NW'de tepe satırı y=0'a değiyor; kırpma yok).
- Masaüstü kopyası: yerel
- Yan yana: `art/results/738-yanyana.png` (SE, kare 00 ve 03). GIF'ler: `art/results/738-<elder_m|elder_f>-<anahtar>.gif` (4 yön, 2x, 110 ms).

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<elder_m|elder_f>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, 386 PNG, 84x84, pivot x=42, y=2 (alttan). 384 klip 8 kare; elder_m sulk SW ve NE 9 kare (kapanış IoU 0,70 ve 0,82: döngü kapanışına bakılmalı). Teslim edilmeyen yok.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (object, 32 renk) | 386/386 geçti |
| style.py check karakter | GEÇTİ 311, UYARI 75, KALDI 0. UYARI hep "ailesinden soluk" (x0,42-0,60; yaşlı paleti, durma karesiyle aynı renkler; aile kilidi kurulu değil, `fit` uygulanmadı) |
| Ayak satırı | durma karesiyle aynı |
| Kapanış IoU 00-08 | 0,70-1,0 (iki istisna yukarıda) |
| Baston | yan yana sayfasında cane_threat'te elde okunuyor, ince; GIF'ten gözle teyit gerekli |

## Sorunlar / notlar
- Kimlik ve poz okunurluğu (argue parmak, sulk kollar kavuşuk) yalnızca gözle; otomatik ölçülmedi.
- Elder_m cane_threat: baston ince, bazı karelerde belirsiz olabilir.
