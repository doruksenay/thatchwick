---
id: "613"
title: "Takı katmanı: cam boncuk, ip kolye, bilezik, broş"
status: pending
approved_by: user        # kullanıcı 20.11.2026 sohbet: "aç buna iş" (karakterin üstünde takı görünsün)
priority: 30
family: karakter
style_gate: false
cost_est: 40
cost_max: 60
depends_on: ["612t", "501"]   # 612t: takı bu ölçekte okunuyor mu; 501: bilek noktası (bilezik). İkisi de 18.12.2026 karakter partisini bekler
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/katman/lay_jw_glass_beads_<yön>_fr.png
  - Assets/Art/Iso/katman/lay_jw_cord_necklace_<yön>_fr.png
  - Assets/Art/Iso/katman/lay_jw_bracelet_<yön>_fr.png
  - Assets/Art/Iso/katman/lay_jw_brooch_<yön>_fr.png
  - art/results/613-taki-katmani-kalanlar.md
created: 2026-11-20
---
# 613 Takı katmanı: kalan dört parça

## Amaç
612t "okunuyor" derse kalan görünür takılar. Küpe ve gerdanlık 612t'de; altın ve taşlı türler aynı çizimin metal rampası ve taş pikseliyle gelir (kod).

## Tarif
Yöntem ve anahtar renk **612t ile aynı** (5 benzersiz yön, yalnızca `_fr`, metal #9A9A9A, önce betik).

| Parça | Çapa | Tarif |
|---|---|---|
| `lay_jw_glass_beads` Cam boncuk dizisi | omuz (`ClothingMasks.Shoulder`) | boyun altında 2 piksel kalınlığında yay; boncuklar sırayla mavi (#3B6FA8) ve yeşil (#4E8A5A), anahtar renk kullanılmaz |
| `lay_jw_cord_necklace` İp kolye | omuz | 1 piksel koyu ip (#3A2E26), ortada 2x2 kemik rengi (#E6DCC4) kolye ucu |
| `lay_jw_bracelet` Bilezik | bilek (`ClothingMasks.Hand`, 501'deki bilek noktası) | bilekte 2 piksellik metal bant; yürüme karelerinde eli izler |
| `lay_jw_brooch` Broş | omuz, göğsün sol üstü | 2x2 metal halka; pelerin varken pelerinin üstünde (B katmanının önünde) |

## Doğrulama
612t ölçütleri. Ek: bilezik yürümede elden kopmuyor; broş 500'deki pelerinle birlikte görünüyor; cam boncuk ile gerdanlık x2'de birbirinden ayırt ediliyor.

## Notlar
- 612t "okunmuyor" derse bu iş iptal edilir (`status: rejected`, neden: 612t raporu).
- Kod: 612t Notlar'daki `jw` yuvası; bilezik için el çapası.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
