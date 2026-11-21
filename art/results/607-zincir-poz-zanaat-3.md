unity-import: pending

# 607 Zincir pozları, zanaat 3/3

Durum: `review`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true` (9 kare). Erkek `c63fe39d-…` (man_r84), kadın `135cb0b1-…` (woman_r84); ikisi de 84x84, `completed`, 8 yön.
- 5 anahtar x 2 karakter x 4 yön (SE, SW, NW, NE) = 40 yön, 1 gen/yön (makbuz), toplam **40 gen** (`cost_est` 40, sapma yok). Çağrılar en çok 3 eşzamanlı yönle gönderildi. Hız sınırı hatası olmadı.
- Gruplar: erkek write 907e0ac8, pound 14df9ec0, churn a4d02696, dip 194dbf9b, sprinkle 53b7f286; kadın write 235a8b8e, pound d759c607, churn eb034553, dip 34c19b5b, sprinkle d61f907e.
- `get_balance` sonrası kalan: 9252 / 10000 (öncesi 9378; fark başka şeritlerin harcamasını da içerir).
- **Sapma:** tarifteki `end_frame` verilmedi (bulutta yükleme yolu yok; başlangıç karesi yönün rotasyonu). Döngü kapanışı kare 0 ile son kare IoU'suna göre denetlendi.

## İşleme
Ham kare 112x112, karakter merkezde. Yalnızca merkez kırpma (14,14,98,98); ölçekleme yok. Kare 0'ın en alt opak satırı y=81 (kırpma sonrası). Palet: `iso_char_<cinsiyet>_r84_cand_se.png` 32 rengi, en yakın RGB, alfa ikili. Kare 0 ile 8 IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 kare. Pivot x=42, y=2 (ALTTAN). Ayak satırı kareler arası ayrıca ölçülmedi, ham çıktı olduğu gibi kırpıldı.

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<write|pound|churn|dip|sprinkle>_<se|sw|nw|ne>_<NN>.png`: 327 PNG, 84x84, pivot (42, 2 alttan). 38 klip teslim.
Kare sayısı: IoU>0,95 olanlar 8 kare (döngü kapanıyor), diğerleri 9 kare. Ayrıntı `art/tmp607/report.json` (commit edilmez); özet: erkek write SE/pound SE-SW-NW/dip SE/sprinkle NW-NE ve kadın pound SE-SW-NE/churn SW/dip NW/sprinkle SE-NW-NE 8 kare.

## Teslim edilmeyen
- **kadın write SE** (96 piksel) ve **kadın write SW** (21 piksel): opak piksel 84x84 dışına taşıyor (içerik kırpma yok kuralı). Kareler `art/results/607-red/`. Yeniden üretim ya da 96x96 tuval kararı gerekir.

## Doğrulama
| Denetim | Sonuç |
|---|---|
| `validate_iso.py` (her klibin 00 karesi, 38) | hepsi geçti |
| `style.py check --family karakter` (327 kare) | KALDI 0, UYARI 90, GEÇTİ 237. UYARI doygunluk/palet sapması; aile kilidi kurulmadı (`art/tmp607/style.txt`, commit edilmez) |
| Tepe/yan kırpılması | teslim edilenlerde yok |
| Yan yana sayfası | `art/results/607-yanyana.png` |
| GIF | `art/results/607-<man|woman>-<anahtar>.gif` (4 yön yan yana) |

## Sorunlar
- Elde tutulan nesnelerin (kalem, havan, yayık, kazan) okunurluğu ve kare içine gömülü mobilya olup olmadığı görsel incelemede denetlenmedi; kullanıcı GIF'lere bakmalı (572'de benzer gömülü mobilya görülmüştü).
- Tutan el konumları (`hand_anchors.txt`) hesaplanmadı; görsel tespit gerek, ayrı iş önerisi.
- Öneri (`tools/art` yazılmadı): `end_frame` yükleme için bulutta bir yardımcı.
