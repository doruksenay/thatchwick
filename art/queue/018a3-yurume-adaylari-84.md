---
id: "018a3"
title: Yürüme adayları, erkek 84 px SE (kullanıcı seçer)
status: review
claimed_at: 2026-11-18T19:35Z
approved_by: user        # kullanıcı 18.11.2026: 018a zıplıyor, 018a2 şablonu topallıyor
priority: 5
family: karakter
style_gate: true
cost_est: 8
cost_max: 20
depends_on: ["012b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_man_r84_walk3<harf>_se_<NN>.png
  - art/results/018a3-yurume-adaylari-84.md
created: 2026-11-18
---
# 018a3 Yürüme adayları: kullanıcı seçer

## Amaç
- 018a (pixminimax): zıplıyor, kafa 8 px oynuyor.
- 018a2 şablonu (6 kare): zıplamıyor ama **topallama gibi**: iki adım eşit değil, 6 kare tam bir çift adıma yetmiyor.
Bu iş tek bir "doğru" sonuç aramaz: birkaç aday üretir, kullanıcı GIF'lere bakıp seçer. Seçilen yöntem 018b2, 018e ve 368'in yöntemi olur.

## Tarif
018a'nın ortak işleme kuralları (karakter kimlikleri, tuval 84x84, pivot x=42 y=2, palet). Yalnızca erkek, SE. Adaylar (harf dosya adında):
- **a:** `walking-8-frames` şablonu.
- **b:** `walking-6-frames` şablonu, ancak 6 kare tam çift adım (sağ + sol) ise.
- **c:** v3 iskelet yürüyüşü (013'teki `skel` yöntemi) 84 px'te, 8 kare.
- **d:** pixminimax, `frame_count=8`, metin: `walking at a calm steady pace, a full stride cycle with the left foot then the right foot stepping equally, head and shoulders level, no bouncing, no limping`.
Araçta olmayan şablonu atla, raporda yaz.

**Ayak satırı kuralı yok:** adım atarken ayağın kalkması doğaldır. Hizalama ayakla değil, karakterin **yere basan ayağıyla** yapılır; bütün kareyi kaydırmak serbest (yere basan ayak aynı satırda kalsın diye).

## Doğrulama
- Kafa oynaması (opak bbox tepesi) ≤ 3 px: geçmeyen aday yine de rapora konur, "zıplıyor" diye işaretlenir.
- **Adım simetrisi:** sol ve sağ adımın kare sayısı ve adım boyu (ayak ucu x farkı) eşit mi; değilse "topallıyor" diye işaretle.
- Her aday için büyütülmüş GIF (x3, 110 ms) ve hepsi tek yan yana sayfada; 018a ve 018a2 GIF'leri de referans olarak.
- Teslim: ölçütten bağımsız **bütün adaylar** `_aday/anim/`'a; seçim kullanıcının.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
