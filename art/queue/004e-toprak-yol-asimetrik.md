---
id: "004e"
title: Toprak yol - çim geçiş seti, asimetrik ve çeşitli (004c yerine)
status: running
claimed_at: 2026-11-19T12:15Z
approved_by: user        # kullanıcı: "gerekirse yeni asset çizdir" (18.11); 004c "simetrik durmamalı" diye reddedildi (19.11)
family: zemin
style_gate: true         # sonuç kullanıcıya önizlemeyle gösterilir; kabul edilmeden kod bağlanmaz
cost_est: 32
cost_max: 50
depends_on: []
supersedes: ["004c", "004d"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/iso_path_dirt_wang_<NN>_<v>.png   (NN = 00..15 köşe maskesi, v = a/b/c; 48 karo)
  - Assets/Art/Iso/iso_path_dirt_fill_a.png, _b.png, _c.png   (fill_a 004c'den var: üzerine yazma, yeni üçü fill_v2_a/_b/_c)
  - art/results/004e-yanyana.png                      (her maskenin 3 çeşidi, etiketli ızgara)
created: 2026-11-19
---
# 004e Toprak yol – çim geçiş seti (asimetrik, 3 çeşit)

## Neden
004c (tek set, 4 karo) reddedildi: uzun yolda iki kenar birbirinin aynası, her karede aynı kenar tekrar ediyor; yol
simetrik ve yapay görünüyor. Stardew'da yol kenarı her parçada farklı: düzensiz, iki yanı birbirine benzemiyor.

## Tarif
- Araç: `create_tiles_pro`, `tile_feature=tileset`, isometric 64 px, `tile_view_angle=30`, `tile_depth_ratio=0`,
  `outline_mode=segmentation` (koyu çerçeve yok), palet `iso_ground_grass_a` + `iso_ground_dirt_a` (004c ile aynı).
- **Üç ayrı çağrı** (her biri 16 karo, ~10 gen), her çağrıda farklı tohum ve `boundary_amplitude` (a: 0.25, b: 0.35,
  c: 0.30) ve açıklamada farklı kenar ayrıntısı:
  - a: "sarkan uzun çim tutamları, seyrek çakıl"
  - b: "kısa kırpılmış çim, kenarda ezilmiş toprak ve tek tük kök"
  - c: "çim öbekleri ve aralarda toprağa taşan küçük yabani ot, birkaç taş"
  Böylece aynı maskenin üç çeşidi birbirinden belirgin farklı kenar konturuna sahip olur.
- **Aynalama, çevirme, döndürme yok:** her maske ayrıca çizilmiş olmalı (PixelLab tileset 16 maskeyi ayrı üretir;
  işçi hiçbir karoyu başka bir karodan türetmez).
- Karo kenarındaki köşe noktalarında toprak/çim geçiş konumu her çeşitte aynı kalmalı (karolar birbirine bağlanır);
  yalnızca karonun içindeki kıvrım ve ayrıntı farklı.
- **Maske ve dosya adı:** 004c sonuç raporundaki not geçerli (PixelLab'in set bit'i çimi işaretler). Dosya adındaki
  NN iki haneli, iş dosyası düzeninde: bit 0 = üst köşe (K), bit 1 = sağ (D), bit 2 = alt (G), bit 3 = sol (B);
  1 = toprak. Örnek: yalnız üst köşe toprak = `wang_01_a`; üst+sağ = `wang_03_a`; hepsi toprak = dolgu (`fill_v2_a`).
  Kod bu adları okur (`IsoArt.PathWang`); başka ad kullanma.
- Dolgu (tam toprak): her çağrının tam toprak karosu → `iso_path_dirt_fill_v2_a/_b/_c`.

## Doğrulama
- `validate_iso.py --kind ground` her karoda.
- Yan yana sayfası: 16 maske x 3 çeşit, etiketli.
- Ölçüm: dolgu kenar/iç parlaklık oranı 0,97-1,03 (004c'de 0,94 idi; ızgara seçiliyordu). Tutmazsa kenar piksellerini
  içe göre düzelten bir `style.py fit` adımı dene, yeniden boyama yapma.
- Kod oturumu sonuçla 12 karo uzunluğunda kıvrılan yol önizlemesi yapıp kullanıcıya gösterir (dual-grid; çeşit seçimi
  konuma göre gürültüyle, iki kenar bağımsız).
