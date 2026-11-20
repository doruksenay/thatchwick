---
id: "772c"
title: "Ağaç stil birliği: çam (genç, olgun), genç meşe ve iğne yapraklı korular (b, d) (_v2)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-21 sohbet: 772 kısmi kabul, "çam yeniden"
priority: 10
family: bitki
style_gate: true         # çamın stil kapısı: korular aynı işte, sonra gelen iğne yapraklılar buna bağlanır
cost_est: 8
cost_max: 18
depends_on: ["772"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_pine_{young,grown}_se_v2.png
  - Assets/Art/Iso/_aday/iso_tree_oak_young_se_v2.png
  - Assets/Art/Iso/_aday/iso_edge_trees_{b,d}_v2.png
  - art/results/772c-*.md
created: 2026-11-21
---
# 772c Çam, genç meşe ve iğne yapraklı korular

## Amaç
772'de çamlar reddedildi (kullanıcı, 2026-11-21): olgun çam yuvarlak kümelerden yapılmış bir "servi" gibi çıktı, arkasında gölgeye benzeyen koyu lekeler var; genç çam iri yapraklı bir fidan. Neden: `create_map_object` stil görseli almadı, stil yalnızca metinden geldi. Genç meşe 56 px kaldı (hedef 60-85). "Genç koru" (`iso_edge_trees_d`, kullanıcının işaretlediği parlak yeşil koniler) ve çam öbeği (`b`) de çamla aynı dilde olmalı.

## Tarif
772 tarifi (stil çapası, yasaklar, ortak ek, tuval, pivot, `style.py fit --family bitki`), şu farklarla:

1. **Araç seçimi (önce):** PixelLab araç listesinde **stil ya da referans görseli alan** bir görsel aracı bul (ör. stil görseli parametresi olan nesne/görsel aracı). Çağrıdan önce araç açıklamasından parametreyi ve maliyeti doğrula, rapora yaz.
   - Stil görselleri (çamlar ve korular b, d için): `Assets/Art/Iso/_aday/iso_tree_pine_young_se.png` (eski genç çam: dokulu iğne yapraklı) ve `Assets/Art/Iso/iso_edge_trees_b.png` (çam öbeği). Renk/ışık uyumu için ek: 772'nin kabul edilen `iso_tree_oak_grown_se_v2.png`.
   - Hiçbir araç stil görseli almıyorsa: `create_image_pixflux` + `color_image` = eski genç çam (palet), ve açıklama aşağıdaki gibi. Bunu rapora "stil görseli yok" diye yaz.
2. **Açıklamalar** (772 ortak ekiyle):
   - çam olgun: `tall mature scots pine, dense dark green needle foliage drawn as many small jagged spiky needle tufts in uneven layered tiers, NOT round puffy clusters, straight reddish brown trunk visible at the bottom, no cast shadow`
   - çam genç: `young pine sapling, small conical shape of jagged spiky dark green needle tufts, thin trunk, NOT broad leaves`
   - koru b: `a group of three pine trees of different heights, dense jagged spiky needle tufts in layered tiers`; en uzunu 120-140 px.
   - koru d ("Genç koru"): `a small grove of four young trees, two young pines with spiky needle tufts and two young broadleaf saplings made of small rounded leaf clusters`; 60-85 px. Parlak limon yeşili, düzgün geometrik koni yasak.
   - meşe genç: 772 metni; maske üstü yaklaşık y=40 (772 raporunun önerisi) ve gövde tabanını sabitleyen kılavuzla; tasarım 772'deki reddedilen genç meşeyle aynı kalabilir (stili uygundu, yalnızca boy kısaydı: 56 → 60-85).
3. Tuval: çam ve korular 112x160, pivot (56, 24); meşe genç 112x160.
4. **Göz kontrolü (teslim kuralı):** çam kareleri 8 kat büyütülerek eski genç çam ve koru b ile yan yana konur. Taç yuvarlak kümelerden ya da iri yapraklardan oluşuyorsa, ya da ağacın arkasında gölge benzeri koyu leke varsa teslim edilmez (KALDI sayılır), raporda yazılır.
- 5 görsel ≈ 5-8 gen; ikinci deneme payıyla `cost_max` 18.

## Doğrulama
772 doğrulaması. Stil sayfası `art/results/772c-stil.png` (2 kat, çim yeşili zemin, alt hizalı): yeni 5 görsel + çapalar (eski genç çam, koru b, 772'nin olgun meşesi) + reddedilen 772 çamları + 84 px insan (`iso_char_man_r84_idle_se_00`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
