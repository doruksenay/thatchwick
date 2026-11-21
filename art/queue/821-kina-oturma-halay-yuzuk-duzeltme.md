---
id: "821"
title: "Kına gecesi oturan gelin, halay, yüzük takma ve testi kırma düzeltmesi (24)"
status: running
claimed_at: 2026-11-21T00:42Z
approved_by: user        # kullanıcı 21.11.2026: "evet ekle" (748 kabulünden sonra halay ve yüzük düzeltmesi + kına oturuşu)
priority: 40
family: karakter
style_gate: false
cost_est: 24
cost_max: 30
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_woman_r84_henna_sit_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_halay_v2_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_woman_r84_put_ring_v2_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_throw_jar_v2_{se,sw,nw,ne}_<NN>.png
  - art/results/821-*.md
created: 2026-11-21
---
# 821 Kına gecesi oturan gelin, halay, yüzük takma ve testi kırma düzeltmesi (24)

## Amaç
Gönül yolu sahneleri (Project belgesi 67b §3.5-3.7). 748 kabul edildi ama iki pozu zayıf kaldı; kına gecesinde gelinin oturuşu da yoktu.

| Anahtar | Nerede | Neden |
|---|---|---|
| `henna_sit` (yalnız kadın) | kına gecesi: gelin alçak taburede oturur, avucunu öne açar, annesi `henna` pozuyla kına sürer | genel `sit` (572) elleri dizde; kınada avuç açık olmalı |
| `halay_v2` (erkek, kadın) | düğün halayı | 748'de kollar yan komşuya uzanmıyor (erkek tek yumruk, kadının elleri göğsünde) |
| `put_ring_v2` (yalnız kadın) | nişan: yüzüğü takma ya da takılan elini uzatma | 748'de kadının karesinde yüzük ve uzanan el görünmüyor |
| `throw_jar_v2` (erkek, kadın) | düğün: eşikte testi kırma | 748'de tuvalden taşan 3 yön teslim edilmedi (erkek SW, kadın SE ve SW; Unity kütüphanesinde 25 ve 17 kare); 4 yön yeniden |

Kod `<anahtar>_v2` karesi varsa onu, yoksa eskisini oynatır (ayrı kod PR'ı); var olan 748 PNG'lerinin üzerine yazılmaz.

## Tarif
- 572 ve 748 ile aynı karakterler: erkek `c63fe39d-b168-4732-bf11-b476172c98e8`, kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d`; 84x84.
- Çağrılar (her biri 4 yön, `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi; aynalama yok):
  - kadın `henna_sit` (4): `sitting on a low wooden stool, holding one open palm forward at chest height, other hand resting on the knee, shy smile, small idle movement, loop`
  - erkek ve kadın `halay_v2` (8): `folk line dance: both arms raised out to the sides at shoulder height with elbows bent, as if holding the hands of dancers on the left and right, stepping side to side and kicking lightly in rhythm, loop`
  - kadın `put_ring_v2` (4): `holding one hand forward with fingers spread, a small shiny gold ring visible on the ring finger, looking at it and smiling, the other hand touching the chest`
  - erkek ve kadın `throw_jar_v2` (8): `holding a small clay jug at chest height with both hands, then throwing it down to the ground in front, joyful; the jug never goes above the head`
- Toplam 24 gen. `halay_v2` ve `throw_jar_v2`'de kollar ve testi 84x112 ham karenin içinde kalmalı (748'de taşma oldu); taşan yön teslim edilmez, raporda yazılır. Bağımsız çağrılar paralel gönderilir.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (ham kare 84x112 → 84 genişliğe ortalama, ayak satırı y=81, palet `iso_char_<cinsiyet>_r84_cand_se.png`, alfa ikili, IoU > 0,95 ise son kare atılır, pivot x=42, y=2 alttan).

## Doğrulama
018a ölçütleri her klip için. Ek: `halay_v2`'de iki kol omuz hizasında yanlara açık (SE karesinde iki el de gövdenin dışında); `put_ring_v2`'de öne uzanan el ve parmakta altın renkli en az 2 piksellik yüzük okunur; `throw_jar_v2`'de 4 yönün hepsi teslim; `henna_sit`'te karakter oturuyor (kalça diz hizasında), öne açık avuç görünüyor. Yan yana sayfası (748'in halay ve put_ring kareleriyle karşılaştırmalı) ve klip başına GIF `art/results/` altına.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
