---
id: "772"
title: "Ağaç stil ve ölçek pilotu: meşe, çam, elma (genç, olgun, meyveli; _v2)"
status: review
claimed_at: 2026-11-20T23:21Z
approved_by: user        # kullanıcı 21.11.2026: ölçek ("ağaçları ... yeniden üret", belge 66) ve stil birliği ("ciddi stil tutarsızlığı var ağaçlarda"; seçim: kabarık kümeli taç, hepsi tek seferde)
priority: 10
family: bitki
style_gate: true         # 772b ve 773 buna bağlı
cost_est: 8
cost_max: 16
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_oak_{young,grown}_se_v2.png
  - Assets/Art/Iso/_aday/iso_tree_pine_{young,grown}_se_v2.png
  - Assets/Art/Iso/_aday/iso_tree_apple_{young,grown,fruit}_se_v2.png
  - art/results/772-*.md
created: 2026-11-21
---
# 772 Ağaç stil ve ölçek pilotu

## Amaç
İki sorun birlikte çözülür:
1. **Ölçek** (tester raporu 66 §1.2): olgun meşe pivot üstü 75 px, insan boyunda. Tablo (`docs/gorsel-yonerge.md`): olgun yaban ağacı **120-140 px**, genç **60-85**; meyve ağacı olgun ve meyveli **95-115**, genç **50-72**.
2. **Stil** (kullanıcı, 21.11.2026 Unity testi, ekran görüntüsü): ağaçlarda dört ayrı çizim dili var. Haritada olgun çam (düz renkli geometrik koni) ve "Genç koru" (parlak yeşil çizgi film konileri) ötekilerin arasında hemen göze batıyor. Elma, kiraz, erik yumuşak düz gölgeli büyük taç; badem, limon, şeftali taçsız iri yapraklı. **Kullanıcı kararı: hepsi "kabarık kümeli taç" stilinde birleşir.**

Bu iş pilottur: üç farklı ağaç tipiyle (yayvan yaprak döken, iğne yapraklı, meyve ağacı) stil ve ölçek tutuyor mu sınanır. Kalanlar 772b (huş, söğüt, korular) ve 773 (armut, kiraz, erik, zeytin).

## Stil çapası (bağlayıcı)
**Referans görseller** (stil eşlemesinde bunlar verilir; türün eski görseli değil):
- Yaprak döken taç: `Assets/Art/Iso/_aday/iso_tree_oak_grown_se.png` (olgun meşe), ikinci referans `iso_tree_mulberry_grown_se.png` (dut).
- İğne yapraklı: `iso_tree_pine_young_se.png` (genç çam) ve `iso_edge_trees_b.png` (koru b).

**Stilin tarifi** (açıklama metnine de girer):
- Taç, çok sayıda küçük yuvarlak yaprak kümesinden oluşur; her kümenin sol üstünde açık vurgu, sağ altında koyu ton (ışık sol üstten). Kümeler arasında koyu boşluklar görünür, taç düz tek leke değildir.
- Çamda: katman katman, dokulu iğne yapraklı dallar; düz renkli düzgün geometrik koni **olmaz**.
- Tek koyu dış hat (siyaha yakın koyu yeşil-kahve), dithering yok, doygunluk düşük, toprak tonlu yeşiller (çapa görsellerinin paleti).
- Gövde koyu kahve, kökler yere hafif yayılır; gölge, çimen, toprak yok.

**Yasak** (eski görsellerde görülen ve tekrar edilmeyecek): düz renkli geometrik koni; parlak limon yeşili çizgi film koni; yumuşak gradyanlı tek büyük taç lekesi; taçsız, tek tek iri yapraklı "botanik" çizim; çapalardan farklı dış hat kalınlığı.

## Tarif
319/301 ağaç yöntemi: `create_map_object`, stil eşleme (yukarıdaki çapalar), `view="high top-down"`, `outline="single color black outline"`, `shading="basic shading"`, `detail="medium detail"`.
- **Yaban ağacı tuvali 112x160, pivot (56, 24)**; **meyve ağacı tuvali 96x136, pivot (48, 24)**. Gövde tabanı tuvalin yatay ortasında, alttan 24 px'te. Kılavuz ve maske `footprint_guide.py` ile (taban elması pivotta).
- Açıklamalar (sonuna ortak ek):
  - meşe olgun: `mature oak tree, broad spreading crown made of many small rounded leaf clusters, thick gnarled trunk visible below the crown, roots spreading slightly`
  - meşe genç: `young oak tree, slender trunk, small sparse crown of a few rounded leaf clusters`
  - çam olgun: `tall mature scots pine, layered textured needle branches in uneven tiers, straight reddish brown trunk visible at the bottom, dark green needles`
  - çam genç: `young pine tree, small layered textured needle branches, thin trunk`
  - elma olgun: `mature apple tree, rounded crown of many small leaf clusters, short twisted trunk`
  - elma genç: `young apple tree, thin trunk, small crown of a few leaf clusters`
  - elma meyveli: olgun görselden `edit_image_pro_flash` ile `same tree, 10-14 small visible ripe red apples on the crown`; taç ve gövde piksel piksel aynı kalır.
  - Ortak ek: `, many small rounded leaf clusters with light top-left highlights and dark gaps, isometric view, single tree, no ground, no grass, no shadow, transparent background, light from top-left, single dark outline, earthy muted palette, no dithering`
- Renk: `python tools/art/style.py fit --family bitki <girdi> <çıktı>` (ücretsiz), sonra `check`.
- 7 görsel ≈ 7-8 gen; ikinci deneme payıyla `cost_max` 16.

## Doğrulama
- `validate_iso.py --kind object`, `style.py check --family bitki` (KALDI teslim edilmez), `python tools/art/scale_check.py <png>...` (KALDI teslim edilmez; genç < olgun; meyveli = olgun ±4 px).
- **Stil sayfası** (`art/results/772-stil.png`, 2 kat, çim yeşili zemin): yeni 7 görsel + iki çapa (olgun meşe, genç çam) + kenara reddedilen eski stiller (olgun çam, genç koru, eski elma) aynı taban çizgisinde. Yeni görseller çapalarla aynı aileden görünmeli; eski stillerden hiçbirine benzememeli.
- **Ölçek sayfası** (`art/results/772-yanyana.png`): yeni ağaçlar ve `iso_char_man_r84_idle_se_00`.

## Notlar
- Oyun `_v2`'yi kendiliğinden tercih eder (`IsoArt.GetPlant`, pivot w/2, 24).
- Ulu ağaçlar (492a2-c) bu boydan en az 1,4 kat büyük kalır; stil çapası onlar için de geçerli.
- Gölge sprite'a pişirilmez; rüzgârda sallanma oyunun gölgelendiricisinde.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
