---
id: "324"
title: Saklama eşyaları 2 (tütsülük, peynir presi)
status: pending
approved_by: pending
family: esya
style_gate: false
cost_est: 2
cost_max: 5
depends_on: ["301", "312"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_smokehouse_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_cheese_press_se.png
created: 2026-11-11
---
# 324 Saklama eşyaları 2

## Amaç
Yiyecek saklama genişlemesi (11.11): `item.smokehouse` (tütsülük, 2x1, dış mekân; tuzlu eti ve peyniri tüttürür) ve `item.cheese_press` (peynir presi, 1x1, mutfak; süt → peynir). Tuzlama fıçısı ve kurutma askısı 312/303'teki gibi kalır, yeni tarifler yalnızca etkileşimdir ve görsel gerektirmez.

## Tarif
Ayrıntılı yöntem (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, doğrulama tablosu, masaüstü kopyası, ayna notu) **301'deki ile aynıdır**; bu dosya yalnızca farkları verir. Kabul edilen 300 eşyaları renk ve ölçek referansıdır (masa, sandalye, tabure, mum, saksı).

| # | Dosya | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama (+ ortak ek) |
|---|---|---|---|---|---|---|
| 1 | `iso_item_smokehouse_2x1_se.png` | 2x1 | 112x96 | `--n 2` | 56 | `small wooden smokehouse hut with a plank door, a clay chimney pipe on the roof, a few strips of cured meat hanging visible through a gap, thin grey smoke wisp, closed door, no person` |
| 2 | `iso_item_cheese_press_se.png` | 1x1 | 72x64 | `--n 1` | 30 | `wooden cheese press with a round wheel of pale cheese in a slatted mould and a heavy stone weight on a lever, no person` |

- Yön: 2x1 simetrik değil (kapı SE yüzünde); diğer yönler sonraki iş, ayna kullanılabilir (301 ayna notu).
- Ortak ek: 301'deki ekten "dark brown wood" kalır; duman tek açık gri ton, en fazla 6 piksel.
- Maliyet 2 gen. İkinci deneme payı 3 → `cost_max` 5.

## Doğrulama
301 doğrulama tablosu. Ek: tütsülük dış mekân (çimen + toprak karosu) üzerinde okunuyor mu; askıdaki et şeritleri 312'deki tuzlama fıçısıyla aynı renk ailesinde mi; peynir presi masa üstü değil zemin eşyası olarak köylü boyunu (≈48 px) aşmıyor mu.

## Notlar
- Yeni ürün ikonları (tuz, et, kümes eti, tuzlu et, pastırma, füme et, peynir, beyaz peynir, füme peynir, 4 yeni turşu, 4 kuru meyve, 2 reçel) 316'nın envanter ikon listesine eklenmedi; 316 ileride güncellenecek.
