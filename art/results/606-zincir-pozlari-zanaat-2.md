unity-import: pending

# 606 Zincir pozları, zanaat 2/3

## Araç ve ayarlar
- `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün rotasyon karesi. Erkek `c63fe39d…` (man_r84), kadın `135cb0b1…` (woman_r84); ikisi `completed`, 8 rotasyon var.
- 5 anahtar x 2 karakter x 4 yön = 40 klip, 1 gen/yön = 40 gen. Önceki yarım turda 39 klip üretilmişti (aynı istek yeniden gönderilince "already queued or complete" döndü, ücret yok); eksik erkek `plane` north-east bu turda üretildi (1 gen). Yeniden deneme yok.
- Kalan (get_balance): 9150 / 10000 (diğer şeritlerin harcaması da dahil).
- Not: `end_frame` hedefi son kareyi çekiyor ama "saklanan kare" değil; 9 kareden 00-07 alındı, 08 atıldı (018c ile aynı).

## İşleme
- Ham kare tuvalleri 84x112 .. 120x160; 84x84'e maskeyle kırpma/yerleştirme, ölçekleme yok. Yatay ofset klip başına tek tamsayı (00. karenin opak kutusu rotasyonla çakışır), dikey: her karenin en alt opak satırı y=82 (alttan 2 satır boşluk), pivot x=42 y=2 (ALTTAN).
- Palet: `iso_char_{man,woman}_r84_cand_se.png` 32 rengi, en yakın RGB, alfa ikili.

## Dosyalar
Her biri 84x84, pivot (42, 2) alttan: `Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_{wheel_throw,blow_glass,plane,chisel,scrape}_{se,sw,nw,ne}_{00..07}.png`. Toplam 304 (320 - 16).

## Teslim edilmeyenler (`art/results/606-red/`)
- `man plane se` ve `man plane sw` (16 kare): model ünite tezgâhını da çizdi, tezgâh 84x84 tuvale sığmıyor (1513 ve 60 piksel kırpılıyor) ve ayak satırı tezgâh bacağına denk geliyor. Teslim edilmedi. Öneri: yeniden üretim (farklı action_description, "no bench") ayrı iş.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| `validate_iso.py` | 304/304 geçti |
| `style.py check --family karakter` | 241 GEÇTİ, 63 UYARI (yalnızca "ailesinden doygun", ~x1.3-1.4, özellikle blow_glass), 0 KALDI |
| Ayak satırı | tüm karelerde en alt opak satır y=82 |
| Yan yana sayfası | `art/results/606-yanyana.png` (her klibin 03. karesi) |
| GIF | `art/results/606-{man,woman}-{anahtar}.gif` (4 yön yan yana) |

## Sorunlar (kullanıcı görsel kararı için)
- Hareketi zayıf olanlar: `wheel_throw` erkek se/sw/nw (neredeyse duruyor, oturma yok; yalnızca ne oturuyor), kadın wheel_throw yürüme gibi; `chisel` erkek nw/ne ve kadın sw/se düşük hareket; `scrape` erkek se/sw bıçak etrafında dağınık piksel, kadın nw/ne zayıf.
- `plane` kadın nw/ne: tezgâh çiziliyor, küçük ve karakterle okunur; kadın sw/se rende kutusu gibi okunuyor.
- `blow_glass` en iyi okunan set.
- Elde tutulan nesne klipte yer alıyor (tarif "küçük olmalı" diyor); `hand_anchors.txt` için el konumları çıkarılmadı (otomatik tespit güvenilmez, ayrı iş).
- Masaüstü kopyası: yerel.
