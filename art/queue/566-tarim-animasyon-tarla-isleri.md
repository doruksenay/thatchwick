---
id: "566"
title: "Tarım animasyonları 2/2: kalbur, sırıkla silkeleme, saban, gübre serpme (32)"
status: review
claimed_at: 2026-11-20T16:09Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 40
family: karakter
style_gate: false
cost_est: 32
cost_max: 44
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_sieve_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_shake_pole_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_plough_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_spread_{se,sw,nw,ne}_<NN>.png
  - art/results/566-*.md
created: 2026-11-20
---
# 566 Tarım animasyonları 2/2: kalbur, sırıkla silkeleme, saban, gübre serpme (32)

## Amaç
Tarım işlerinin etkileşim animasyonları (84 px erkek ve kadın). Project belgeleri 59 (orak, tırpan), 59a (saban, fidelik) ve 60 §3.4 (aile 36-42). Kod poz anahtarını adla arar; kare gelene kadar çapa pozuna düşer.

| Anahtar | Nerede oynar |
|---|---|
| `sieve` | tohum kalburu, harman savurma |
| `shake_pole` | zeytin, ceviz ve badem silkeleme |
| `plough` | sabanla çift sürme (öküz ya da atın arkasında) |
| `spread` | gübre, kompost, kül ve kireç serpme |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 4 anahtar x 8 = 32 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `sieve`: `holding a round sieve with both hands at waist height and shaking it side to side, loop`
  - `shake_pole`: `holding a long thin pole with both hands and knocking it upward against high branches, loop`
  - `plough`: `walking slowly forward leaning on the two handles of a plough with both hands, loop`
  - `spread`: `scooping from a basket held at the hip and flinging the contents forward with a short underhand sweep, loop`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval 84x84, ayak satırı sabit, pivot x=42 y=2 alttan, 012b paleti, dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<yön>_<NN>.png`).

## Doğrulama
018a ölçütleri her klip için; ek olarak elde tutulan aletin okunurluğu (orak kısa ve kavisli, tırpan uzun saplı) ve ayak satırı sabitliği. Klip başına GIF `art/results/` altına.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
