---
id: "565"
title: "Tarım animasyonları 1/2: orak, tırpan, meyve toplama, eğilip toplama, harman (40)"
status: review
claimed_at: 2026-11-20T15:56Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 40
family: karakter
style_gate: false
cost_est: 40
cost_max: 52
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_reap_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_scythe_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_pick_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_bend_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_thresh_{se,sw,nw,ne}_<NN>.png
  - art/results/565-*.md
created: 2026-11-20
---
# 565 Tarım animasyonları 1/2: orak, tırpan, meyve toplama, eğilip toplama, harman (40)

## Amaç
Tarım işlerinin etkileşim animasyonları (84 px erkek ve kadın). Project belgeleri 59 (orak, tırpan), 59a (saban, fidelik) ve 60 §3.4 (aile 36-42). Kod poz anahtarını adla arar; kare gelene kadar çapa pozuna düşer.

| Anahtar | Nerede oynar |
|---|---|
| `reap` | ekin hasadı (orak): `ActionKeys["reap"]` |
| `scythe` | tırpanla biçme (tahıl, lif, ot) |
| `pick` | meyve toplama, budama, aşılama: `ActionKeys["pick"]` |
| `bend` | yerden toplama: toplama düğümleri, ot yolma, sararmış ekini sökme, dev ürün |
| `thresh` | harman yerinde dövme |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `reap`: `bending forward and cutting crop stalks low with a short curved sickle in one hand while the other hand gathers them, rhythmic loop`
  - `scythe`: `standing and swinging a long two-handed scythe in a wide low arc from right to left, rhythmic loop`
  - `pick`: `reaching up with one arm to pick fruit from a branch and lowering it to a basket at the hip, loop`
  - `bend`: `bending down at the waist, pulling a plant from the ground with both hands and straightening up, loop`
  - `thresh`: `swinging a wooden flail overhead and striking the ground in front, rhythmic loop`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval 84x84, ayak satırı sabit, pivot x=42 y=2 alttan, 012b paleti, dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<yön>_<NN>.png`).

## Doğrulama
018a ölçütleri her klip için; ek olarak elde tutulan aletin okunurluğu (orak kısa ve kavisli, tırpan uzun saplı) ve ayak satırı sabitliği. Klip başına GIF `art/results/` altına.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
