---
id: "301c"
title: Pazar tezgâhı, kazan ve toprak bardak (301b reddedildi)
status: accepted
claimed_at: 2026-11-19T07:28Z
priority: 10            # üçü de başlangıç haritasında
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 (onay grubu 2: 301b ret, yeniden üretim)
family: esya
style_gate: false
cost_est: 3
cost_max: 9
depends_on: ["302"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_market_stall_2x1_se_nw_v3.png
  - Assets/Art/Iso/_aday/iso_item_cooking_pot_se_v3.png
  - Assets/Art/Iso/_aday/iso_item_clay_cup_se_v2.png
created: 2026-11-18
---
# 301c Pazar tezgâhı, kazan ve toprak bardak

## Amaç
Kullanıcı 301b'nin üç görselini reddetti; neden belirtmedi. Görsellerden çıkan varsayım:
- **tezgâh:** sade ve boş, pazar tezgâhı gibi okunmuyor;
- **kazan:** küçük, siyah bir leke gibi, kazan olduğu anlaşılmıyor;
- **bardak:** çok küçük, tanınmıyor.

Kullanıcı başka bir neden söylerse tarif ona göre düzeltilir.

## Tarif
Yöntem **301 ile aynı** (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, masaüstü kopyası). **Stil referansı kabul edilmiş 302-304 eşyaları**: kütük masa, hazırlık tezgâhı, fıçı, kuyu, leğen.

| # | Dosya (`_aday/`) | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `iso_item_market_stall_2x1_se_nw_v3.png` | 2x1 | 112x136 | `--n 2 --axis x` | 76 | `medieval village market stall: wooden plank counter on four posts under a sloped striped awning of faded linen (muted ochre and off-white stripes), goods on the counter: a basket of apples, bread loaves, a few clay jugs and a cheese wheel; looks lively and clearly a shop` |
| 2 | `iso_item_cooking_pot_se_v3.png` | 1x1 | 80x80 | `--n 1` | 34 | `large black iron cauldron hanging from a wooden tripod over the floor, round belly with highlights, thick rim, bail handle, wooden ladle sticking out, interior visible from above with dark stew, unlit, no fire` |
| 3 | `iso_item_clay_cup_se_v2.png` | yüzey dekoru | 32x32 | 301 dekor kılavuzu | 16 | `brown glazed clay drinking cup with a small handle, slightly larger than a hand, clearly readable cup shape, upright` |

- Maliyet 3 × 1 gen. İkinci deneme payı: `cost_max` 9.

## Doğrulama
- 301 doğrulaması.
- Yan yana sayfası: kabul edilen 302-304 eşyaları ve köylü ile. Tezgâh pazar yeri gibi okunuyor mu; kazan uzaktan kazan olarak tanınıyor mu; bardak masanın üstünde seçiliyor mu.

## Notlar
- Dönem: dökme demir kazan ve üçayak, keten tente, sırlı toprak bardak ortaçağ köyüne uygun.
- Kod tarafı (`IsoArt.Items`), review sonrası v3 ve v2 adlarına geçer.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
