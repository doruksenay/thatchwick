---
id: "004c"
title: Toprak yol - çim geçiş seti (Stardew tarzı, organik kenar)
status: pending
approved_by: user        # kullanıcı: "Stardew'da nasıl ona bak, gerekirse yeni asset çizdir" (18.11.2026)
family: zemin
style_gate: true         # stil pilotu: yalnızca 4 karo; kalanı 004d (kullanıcı onayından sonra)
cost_est: 12
cost_max: 25
depends_on: []
supersedes: ["004b"]
max_attempts: 3
pixellab: true
outputs:
  - Assets/Art/Iso/iso_path_dirt_fill_a.png                      (dikişsiz tam toprak)
  - Assets/Art/Iso/iso_path_dirt_wang_03.png                     (iki toprak köşe: düz yol kenarı)
  - Assets/Art/Iso/iso_path_dirt_wang_01.png                     (tek toprak köşe: dış köşe)
  - Assets/Art/Iso/iso_path_dirt_wang_07.png                     (üç toprak köşe: iç köşe)
  - art/results/004c-yanyana.png                                 (dikiş sayfası, aşağıda)
created: 2026-11-18
---
# 004c Toprak yol – çim geçiş seti (STİL PİLOTU, 4 karo)

> **Pilot:** bu iş yalnızca dolgu + 3 Wang karosunu üretir. Kullanıcı yan yana sayfasına bakıp `status: accepted` yazınca kalan 13 Wang karosu, 2 dolgu varyantı ve süsler 004d ile üretilir. Tarif tüm set için yazıldı; pilotta aynı tarifle yalnızca listelenen dosyalar.

## Pilot dikiş sayfası (art/results/004c-yanyana.png)
Dört karoyla döşenmiş 3 örnek, 2x büyütülmüş: (a) 2 karo genişliğinde düz yol, 6 karo uzunluğunda; (b) L dönüş; (c) çimen ortasında 2x2 toprak alan. Yanına Stardew'un kasaba yolundan bir ekran görüntüsü konmaz (telif); karşılaştırma sözlü.

# (Tam set tarifi)

## Neden
004 setinde (create_path_tiles, outline varsayılan) her karonun çevresinde koyu dış çizgi var ve yol kenarı düz.
Kodla denenen çözümler (karo boyama, piksel sızdırma) kullanıcıya doğal gelmedi. Stardew'daki doğallık elle çizilmiş
geçiş karolarından geliyor: kenarda düzensiz sarkan çim yaprakları, kökler, çakıl; düz çizgi ya da yuvarlatılmış
dikdörtgen çerçeve yok; yan yana gelen karolarda dikiş görünmüyor.

## Tarif
1. **Dikişsiz toprak dolgu (3 varyant):** izometrik 64x32 elmas, outline yok. Yürünmüş, sıkışmış toprak; birkaç küçük
   çakıl, ince tekerlek/ayak izi dokusu. Kenar pikselleri iç piksellerle aynı tonda (ölçüm: ±0,03).
2. **Köşe tabanlı Wang seti (16 karo, dual-grid):** her karonun 4 köşesi toprak ya da çim. Geçiş sınırı karonun içinden
   dalgalı geçer: toprağın üstüne taşan çim yaprak uçları, birkaç kök, geçişte 1-2 çakıl. Sınırda koyu çizgi yok.
   Toprak ve çim tonları `iso_path_dirt_fill_a` ve `iso_ground_grass_a` ile birebir aynı (karışınca dikiş yok).
   PixelLab: izometrik tileset / Wang (köşe) aracı; lower = çim (`iso_ground_grass_a` örnek), upper = toprak (dolgu a).
   Dosya numarası = köşe maskesi (bit 0 = kuzey köşe, 1 = doğu, 2 = güney, 3 = batı; 1 = toprak).
3. **Yol kenarı süsleri (6-8 küçük sprite, 16-24 px):** çim öbeği (2), ufak çakıl kümesi (2), papatya/karahindiba (2),
   kuru dal, tekerlek izi çamuru. Gölgesiz ya da çok hafif gölge.

## Doğrulama
- `validate_iso.py --kind ground` her karoda.
- Dikiş sayfası: 2 ve 3 karoluk düz yol, L dönüş, T, kavşak, tek karoluk patika, yol sonu, kıvrılan yol; 4x4 alanlar.
  Karo sınırı görünmemeli; kenar dalgalı ve doğal olmalı (Stardew'un kasaba yollarıyla yan yana karşılaştırma).
- Ölçüm: dolgu ve Wang karolarında elmas kenar parlaklığı iç parlaklığa ±0,03.

## Kabul edilince (kod)
- `WorldView`: yol katmanı dual-grid ile çizilir: her karo köşesi için "toprak mı" (o köşeye değen 4 karonun yol
  çoğunluğu), karo başına 4 köşeden Wang indeksi; tam toprak karolarda dolgu varyantı (konuma göre).
- Süsler: yol kenarı çim karolarına sabit gürültüyle seyrek (karo başına en çok 1) yerleştirilir.
- `IsoArt.PathSetSeamless` / `WorldView.ProceduralPathFringe` geçici bayrakları kaldırılır.
