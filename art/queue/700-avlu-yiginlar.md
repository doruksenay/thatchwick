---
id: "700"
title: Avlu yığınları (pislik, köpek pisliği, tezek, kığı, süprüntü)
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (506-509 ile aynı: kullanıcı, 20.11.2026: "daha sonra karar verelim")
family: esya
style_gate: false
cost_est: 35
cost_max: 50
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_dung_pile_se.png
  - Assets/Art/Iso/iso_item_dog_pile_se.png
  - Assets/Art/Iso/iso_item_animal_dung_se.png
  - Assets/Art/Iso/iso_item_sheep_dung_se.png
  - Assets/Art/Iso/iso_item_refuse_pile_se.png
  - Assets/Art/Iso/iso_item_animal_dung_dry_se.png
  - Assets/Art/Iso/iso_item_dung_pile_dry_se.png
created: 2026-11-20
---
# 700 Avlu yığınları (pislik, köpek pisliği, tezek, kığı, süprüntü)

## Amaç
Belge 57 §3 ve 57d §5: yerde kalan yığınlar. Halı katmanında çizilir (yürümeyi engellemez), üstünde kod sinek uçurur. Kurumuş hâller evre 4 içindir (`FilthService.StageOf == Dried`).

## Tarif
Eşya ailesi stil eşlemesi, izometrik 2:1, tek karo, zemine yatık (halı gibi; gölge yok, çok alçak). Boyut karonun üçte biri kadar; köpek pisliği ve kığı daha küçük. Pislik koyu kahve, tezek yassı ve lifli, kığı küçük boncuk öbeği, süprüntü kabuk, kemik ve çürük yaprak karışımı. Kurumuş hâller soluk gri kahve ve çatlak. Mide bulandıracak ayrıntı yok: oyun PEGI 12 hedefinde, şekil okunur ama stilize.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family esya <dosya>`. Ek kontrol: sprite karo sınırını aşmaz, pivot (32, 16).

## Notlar
items.json: `item.dung_pile`, `item.dog_pile`, `item.animal_dung`, `item.sheep_dung`, `item.refuse_pile` (`art.pending`). Kurumuş hâllerin bağlanması görsel gelince `ItemViews`'ta yapılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
