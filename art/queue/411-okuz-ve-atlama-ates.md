---
id: "407"
title: "Öküz, örme çit ve bekçi ateşi"
status: rejected
claimed_at: 2026-11-18T19:22Z
approved_by: user        # otomatik akış (art/budget.md); karar: Project belgesi 31, 29 (kullanıcı 18.11.2026 "önerdiklerinin tamamını yap")
family: hayvan
style_gate: false
cost_est: 6
cost_max: 10
depends_on: ["375"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_ox_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_item_wattle_hurdle_se.png
  - Assets/Art/Iso/_aday/iso_item_bonfire_se.png
  - Assets/Art/Iso/_aday/iso_item_bonfire_lit_se.png
created: 2026-11-18
---
# 407 Öküz, örme çit ve bekçi ateşi

## Amaç
- **Öküz** artık ayrı tür (`animal.ox`): çeki hayvanı, kütük çekme yarışması. İneğin erkeği artık boğa.
- **Örme çit** (`item.wattle_hurdle`): atla atlanan alçak engel, ağıl kenarı.
- **Bekçi ateşi** (`item.bonfire`): açık havada yanan ateş; çoban yanındayken kurt savunmasına +1.

## Tarif
- Öküz: 375 tarifi (hayvan ailesi, 8 yön, tuval 48), tek aday. Açıklama: `heavy draught ox, dun-brown, long curved horns, wooden yoke on neck`. `style.py fit --family hayvan`.
- Örme çit: 1x1 yer eşyası, 302 ölçeği, `esya` ailesi. Açıklama: `low woven hazel wattle hurdle panel standing upright, knee high`.
- Bekçi ateşi: 1x1, iki kare (söndü ve yanıyor; `_lit` son eki diğer ısı eşyaları gibi). Açıklama: `small ring of field stones with stacked split logs` ve yanan hâli `... burning with low orange flames`. Yanan kare `check --light`.

## Doğrulama
`validate_iso.py`, öküz için `check --family hayvan`, eşyalar için `check --family esya` (ateşte `--light`).

## Notlar
- Kod eşlemesi: öküz `IsoArt.Animals` içinde hazır; eşyaların `IsoArt.cs` eşlemesi kod oturumunun işidir.
- Dönem: öküz, örme çit (çobanların ağıl panosu) ve taş halkalı ateş ortaçağ köyüne uygun.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı reddetti (sohbette toplu inceleme). Neden: kullanıcı öküzü beğenmedi; yeniden çizilecek (104t). Sönük bekçi ateşi taş halkası mavi taşlı ve garip; örme çit teslim edilmemişti. Yanık bekçi ateşi iyi.
