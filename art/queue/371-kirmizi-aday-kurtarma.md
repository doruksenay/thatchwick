---
id: "371"
title: Reddedilen adayları doygunluk eşlemesiyle kurtarma (ağaç, düğüm, ekin, mezar, şenlik)
status: running
claimed_at: 2026-11-18T12:04Z
priority: 10            # başlangıç haritasında görünenler önce (görsel analiz)
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 ("yap bunu": yeniden deneme işleri)
family: bitki
style_gate: false
cost_est: 0
cost_max: 1
depends_on: []
max_attempts: 1
pixellab: false
outputs:
  - Assets/Art/Iso/_aday/iso_tree_birch_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_pine_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_apple_{young,grown,fruit}_se.png
  - Assets/Art/Iso/_aday/iso_tree_pear_{young,grown,fruit}_se.png
  - Assets/Art/Iso/_aday/iso_tree_cherry_{young,grown,fruit}_se.png
  - Assets/Art/Iso/_aday/iso_tree_plum_fruit_se.png
  - Assets/Art/Iso/_aday/iso_item_mushroom_bed_se.png
  - Assets/Art/Iso/_aday/iso_forage_dry_branch_se.png
  - Assets/Art/Iso/_aday/iso_forage_hive_se.png
  - Assets/Art/Iso/_aday/iso_forage_mushroom_se.png
  - Assets/Art/Iso/_aday/iso_forage_wild_garlic_se.png
  - Assets/Art/Iso/_aday/iso_forage_cress_se.png
  - Assets/Art/Iso/_aday/iso_crop_cabbage_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_leek_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_pea_{1,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_pumpkin_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_crop_turnip_{1,3}_se.png
  - Assets/Art/Iso/_aday/iso_item_grave_se.png
  - Assets/Art/Iso/_aday/iso_deco_bonfire_se.png
  - Assets/Art/Iso/_aday/iso_deco_lantern_se.png
created: 2026-11-18
---
# 371 Reddedilen adayları doygunluk eşlemesiyle kurtarma

## Amaç
310, 311, 313, 319, 320 ve 322'de üretilen ama stil kapısında kalan adayların çoğunun biçimi iyi; tek sorunları bitki ailesinden 1,5-2,7 kat doygun olmaları. `tools/art/style.py fit` doygunluğu aileye indirir (renk ve ton aynı kalır). Kod oturumunun denemesinde elma ağacı x2,73'ten x1,15'e indi, erik referansıyla aynı ailede göründü ve kapıdan UYARI ile geçti (Project `26-gorsel-analiz.md`).

Bitki ailesine iki referans eklendi (`art/style/references.json`, 2026-11-18): `iso_tree_plum_grown_se` ve `iso_forage_pinecone_se`.

**PixelLab çağrılmaz.** Yeni üretim gerektiren adaylar 372-374'te.

## Tarif
Her satır için:
1. Kaynağı al.
2. `python tools/art/style.py fit --family <aile> <kaynak> <geçici>` çalıştır.
3. `style.py check --family <aile> <geçici>` ve `validate_iso.py --kind object` çalıştır. Pivot her satırda (40, 24); mezar ve fener (32, 24); kaynak işlerin tablolarından.
4. KALDI değilse `cp -n` ile çıktı adına koy. Var olan dosyanın üzerine yazma.
5. UYARI raporda yazılır.

| Kaynak (`art/results/`) | Çıktı (`Assets/Art/Iso/_aday/`) | Aile |
|---|---|---|
| `320-red/iso_tree_birch_young_se.png` | `iso_tree_birch_young_se.png` | bitki |
| `320-red/iso_tree_pine_grown_se.png` | `iso_tree_pine_grown_se.png` | bitki |
| `319-red/iso_tree_apple_{young,grown,fruit}_se.png` | aynı adlar | bitki |
| `319-red/iso_tree_pear_{young,grown,fruit}_se.png` | aynı adlar | bitki |
| `319-red/iso_tree_cherry_{young,grown,fruit}_se.png` | aynı adlar | bitki |
| `319-red/iso_tree_plum_fruit_se.png` | aynı ad | bitki |
| `320-red/iso_item_mushroom_bed_se.png` | aynı ad | bitki |
| `311-red/branch_raw.png` | `iso_forage_dry_branch_se.png` | bitki |
| `311-red/hive_raw.png` | `iso_forage_hive_se.png` | bitki |
| `311-red/mushroom_raw.png` | `iso_forage_mushroom_se.png` | bitki |
| `322-red/garlic_raw.png` | `iso_forage_wild_garlic_se.png` | bitki |
| `322-red/cress_raw.png` | `iso_forage_cress_se.png` | bitki |
| `310-red/iso_crop_cabbage_{1,2,3}_se.png` | aynı adlar | bitki |
| `310-red/iso_crop_leek_2_se.png` | aynı ad | bitki |
| `310-red/iso_crop_pea_{1,3}_se.png` | aynı adlar | bitki |
| `310-red/iso_crop_pumpkin_{1,2,3}_se.png` | aynı adlar | bitki |
| `310-red/iso_crop_turnip_{1,3}_se.png` | aynı adlar | bitki |
| `313-red/grave_1.png` | `iso_item_grave_se.png` | esya |
| `313-red/iso_deco_bonfire_se.png` | aynı ad | esya |
| `313-red/iso_deco_lantern_se.png` | aynı ad | esya |

- **Ateş ve fener:** alev pikselleri doygun kalmalı. `fit` alevi soldurursa (alev turuncu yerine kahve), o dosyayı fit'siz ham haliyle değerlendir. Kapıdan KALDI alırsa teslim etme; raporda yan yana göster, kullanıcı karar verir.
- **Renk sayısı:** fit sonrası 48 renkten fazlaysa (elma ve armut genç 49-51) `uv run --with pillow` ile 48 renge en yakın komşu nicemleme yap (titreme yok). Bu PixelLab değil, ücretsiz.
- **Zemin lekesi:** mezar 1'in tabanındaki toprak tümseği mezarın parçası sayılır. Başka satırda opak zemin karosu ya da çim elması varsa teslim etme, raporla.

## Doğrulama
- Her satır için tablo: kapı sonucu (fit öncesi ve sonrası doygunluk), palete yakınlık, renk sayısı, pivot, teslim ya da neden.
- Yan yana sayfası (`style.py sheet`): ağaçlar erik olgunuyla, düğümler kozalakla, ekinler saksıyla, mezar ve şenlik süsleri tabureyle.
- Çimen karosu üstünde 4x4 sayfa: ağaçlar, düğümler ve ekinler çimle aynı ailede mi.

## Notlar
- Kaynak işler (310, 311, 313, 319, 320, 322) kendi durumlarında kalır. Bu iş yalnızca teslimi tamamlar; raporda hangi kaynağın hangi çıktıyı verdiğini yaz.
- Bu işle teslim edilmeyen ve yeniden üretilecekler:
  - 372 ağaçlar: meşe, huş olgun, çam genç, söğüt genç ve olgun, erik genç;
  - 373 düğümler: böğürtlen, fındık, otlar, mürver, çilek, kestane, kuşburnu, ısırgan;
  - 374 ekinler: pırasa 1 ve 3, bezelye 2, şalgam 2.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
