---
id: "104t"
title: "Büyük çiftlik hayvanları tamamlama: inek, keçi, öküz yeniden (8 yön)"
status: running
claimed_at: 2026-11-20T10:33Z
approved_by: user        # kullanıcı kararı 2026-11-20 (sohbette hayvan incelemesi: "önerilerini uygula ama öküz tekrar çizilsin")
priority: 15
family: hayvan
style_gate: true
cost_est: 30
cost_max: 60
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_cow_5_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cow_6_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_goat_5_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_goat_6_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_ox_v2_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_item_bonfire_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_wattle_hurdle_se.png
created: 2026-11-20
---
# 104t Büyük çiftlik hayvanları tamamlama

## Neden
Hayvan incelemesinde (2026-11-20) ortaya çıktı: oyunda **inek ve keçi için hiç sprite yok**. 104 ve 105 üretilmiş, ama her türde en az bir yön stil kapısında kaldığı için tür bütünüyle teslim edilmemişti. Kullanıcı öküzü (411 dosyası, iş 407) de beğenmedi, yeniden çizilecek.

## Tarif
- **İnek:** 104'teki tarif (yol B, pixflux 60x60, template horse, v3 döndürme, palet referansı `iso_animal_sheep_1_cand_se.png`). İki aday:
  - cow_5: 104'teki cow_1 metni (kızıl-kahve, kısa kıvrık boynuz).
  - cow_6: 104'teki cow_2 metni (ala). 104'ün yan yana sayfasında (`art/results/104-yanyana.png`) görünen cow_2 iyi bir yön; bu görünüme yakın kal.
- **Keçi:** 105'teki keçi tarifi. İki aday: goat_5 açık kahve-beyaz, kısa boynuz ve sakal; goat_6 siyah, geriye kıvrık boynuz ve sakal. Domuz 2 ve koyun 4 ile aynı ölçek ailesi.
- **Öküz (yeniden):** 375 tarifi (hayvan ailesi, 8 yön), tek aday. İnekten belirgin iri ve ağır olmalı, omuzları kalın ve kambura yakın olmalı. Renk dun-kahve ya da kızıl-kahve; 407'deki gibi gri olmamalı. Uzun, öne kıvrık boynuz ve boynunda **ahşap boyunduruk** (sarı ip ya da kement değil, kalın düz tahta). Açıklama: `heavy medieval draught ox, broad muscular shoulders, dun brown hide, long forward-curving horns, thick wooden yoke beam resting on the neck`.
- **Bekçi ateşi (sönük, v2):** 411'deki tarif. Tarla taşlarından gri-kahve bir halka, içinde çatılmış yarılmış odunlar. Mavi ya da kenarlı taş olmamalı. 411'deki yanık hâli (`iso_item_bonfire_lit_se`) iyi; v2 onunla aynı taş halkayı kullanmalı.
- **Örme çit:** 411'deki tarif (teslim edilmemişti).
- **Stil kapısı (bu işe özel):** bir türün bir yönü KALDI ise tür bütünüyle atılmaz. Yalnızca o yön v3 döndürmeyle en çok 2 kez yeniden denenir. Yine kalırsa `style.py fit --family hayvan` ile kurtarma denenir. O da kalırsa geçen yönler teslim edilir, kalan yön raporda ve `## Notlar`da yazılır. (Kullanıcı kararı 2026-11-20: oyunda inek, at, keçi ve yaban hayvanları için hiç sprite yoktu.)

## Doğrulama
375 doğrulaması. Yan yana sayfası: `art/results/104t-yanyana.png`. Her türün SE yönü; koyun 4, domuz 2, köpek 1 ve 58 px köylü ile aynı satırda; ölçek oranı gerçeğe uygun (öküz > inek > keçi).

## Notlar
