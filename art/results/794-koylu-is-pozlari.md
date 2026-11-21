unity-import: pending

# 794 Köylü iş pozları: grind, lean_staff, measure, mend_net, wipe, play_music (84 px, erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`. Karakterler `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8) ve `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d), ikisi `completed`, 8 rotasyon.
- **Bu turda yeni PixelLab çağrısı yapılmadı.** İşi sahiplenen önceki oturum 48 klibi (6 anahtar x 2 karakter x 4 yön, 9 kare) 2026-11-21'de zaten üretmişti, ama sonuçları işleyip teslim edemeden kesilmişti. İlk çağrı "already queued or complete, not charged" döndü; `get_character` iki karakterde de 6 grubun 4 yönüyle (egemen eylem metinleri iş dosyasındakiyle aynı) olduğunu gösterdi. Ham kareler `get_character` URL'lerinden indirilip işlendi (018c ile aynı yaklaşım, ledger 018c).
- Üretim maliyeti: tahmin 48, tavan 56. Üretim önceki oturumda harcandı; bu turun gen harcaması 0. Bu oturumun `get_balance` okuması: 9153 / 10000 (Tier 3, plan `art/budget.md` ile uyumlu). Önceki oturumun harcamasını ayrıca ayıklayamadım (diğer şeritler eşzamanlı harcıyor); `get_balance` 794 için ilk okuması 9162 idi.
- İşleme (018c/571 kuralları): yalnızca kaydırma, ölçekleme yok; her karenin en alt opak satırı y=81, yatay ortalı, 84x84; kırpılan opak piksel 0. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` 32 rengi, en yakın RGB, alfa ikili. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: [794-yanyana.png](794-yanyana.png) (2 referans + 24 yeni: her klibin SE kare 00 ve 03). GIF (4 yön yan yana, 8x): `art/results/794-<man|woman>-<anahtar>.gif`.
- masaüstü kopyası: yerel

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, toplam 403 PNG, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Her klip 8 kare; 9 kareli olanlar (kapanış yakın değil): man grind se/nw, lean_staff se, measure se/sw/nw, mend_net nw, wipe nw; woman grind nw, lean_staff sw, measure 4 yön, mend_net sw/nw/ne, wipe se/sw (tam liste: dosya adlarındaki `_08`).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (403 dosya) | 403/403 geçti |
| style.py check karakter | KALDI 0, UYARI 89, GEÇTİ 314. UYARI yalnızca "ailesinden doygun" (erkek, yaklaşık x1,27); palet yakınlığı %100. Aile kilidi yok, `fit` uygulanmadı |
| Ayak satırı | her karede en alt opak satır y=81 |
| Kırpma | 0 opak piksel |
| En küçük tepe y | man lean_staff se 0 (asa ucu üst kenarda); diğerleri >= 2 |
| Döşeme testi | zemin işi değil |

## Sorunlar ve notlar
- Ardışık kare IoU'su düşük (en düşük): woman wipe ne 0,57, man measure se/nw 0,58-0,59, woman grind nw 0,64, wipe/measure erkek 0,63-0,68. Bu klipler sıçramalı görünebilir; GIF'ten kullanıcı bakmalı.
- Neredeyse hareketsiz klipler (ardışık IoU ≥ 0,98): man grind sw ve ne (1,0), woman grind se (1,0), woman play_music sw (0,98), man mend_net sw (0,99). Kolu çeviren hareket üretilmemiş olabilir; kullanıcı GIF'ten teyit etsin, gerekirse yeniden iş.
- Elde tutulan nesneler (ud, asa, kumaş, ağ) yan yana sayfasında okunuyor; ağ erkek SE'de büyük. Kimlik kayması ve nesne doğruluğu yalnızca gözle bakıldı.
- Kadın `mend_net` SE sayfada oturma yerine ağı tutan duruş gibi görünüyor, kullanıcı teyit etsin.
- El konumları (`hand_anchors.txt`) ölçülmedi.
