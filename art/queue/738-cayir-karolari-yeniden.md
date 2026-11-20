---
id: "738"
title: "Çayır karoları yeniden: ana çimle aynı ton (612 çayırının düzeltmesi)"
status: running
claimed_at: 2026-11-20T20:30Z
approved_by: user        # kullanıcı onayı 2026-11-20 (sohbet: "kıyı karosu ve çim tonu için PixelLab işi hazırla"; belge 65b B2)
priority: 10
family: zemin
style_gate: false
cost_est: 4
cost_max: 8
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_meadow_v2_a.png
  - Assets/Art/Iso/iso_ground_meadow_v2_b.png
  - Assets/Art/Iso/iso_ground_meadow_v2_c.png
created: 2026-11-20
---
# 738 Çayır karoları yeniden (ana çimle aynı ton)

## Neden
`IsoArt.Ground` çimin içine düşük frekanslı gürültüyle (~7 karoluk öbekler, `patch > 0.74`) çayır karosu serpiyor. Ana çim
`iso_ground_grass_a/b/c`; çim piksellerinin ortalaması (115, 178, 94). 612'nin çayırı ise zeytin yeşiline kaymış
(`iso_ground_meadow_a/b/c` çim pikselleri ortalama ~(108, 139, 74); yeşil kanalda -39). 612 tarifi "çim tabanı
`iso_ground_grass_a` ile aynı ton" diyordu; çıktı bunu tutturamadı. Arada geçiş karosu da yok (`iso_trans_*` dosyası
yok). Sonuç: çimin ortasında koyu, keskin elmas kenarlı yamalar ve ızgara görüntüsü (belge 65b B2).

Ton aynı olursa geçiş karosuna gerek kalmaz: çayır, çimin üstüne serpilmiş çiçekler gibi okunur.

## Tarif
- Araç: `create_tiles_pro`, isometric 64 px, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`,
  stil modu; `style_images` = `iso_ground_grass_a`, `iso_ground_grass_b`, `iso_ground_grass_c` (yalnızca bunlar; 612'nin
  çayırını referans verme). 004e ve 612 ile aynı ayarlar. Aynalama ve döndürme yok.
- Tek karo (dolgu), üç çeşit (a, b, c), farklı tohum.
- Açıklama: "exactly the same grass as the reference tiles: same base green, same blade pattern and brightness; add only
  a sparse scatter of tiny wildflowers (a few white and pale yellow dots, a few clover tufts); flowers cover at most 10%
  of the tile; no darker or olive tint, no shading change".
- Çeşitler arasında yalnızca çiçek yeri ve sayısı değişir; zemin tonu üçünde de aynı.

## Doğrulama
- `validate_iso.py --kind ground` her karoda (opak 1024, elmas dışı 0, boşluk 0).
- **Ton ölçümü (kesin):** çim pikselleri (yeşil kanal kırmızı ve maviden 8+ büyük) ortalaması `iso_ground_grass_a` ile
  kanal başına ±5 içinde. Tutmazsa önce `style.py fit` (referans `iso_ground_grass_a`) ile kenar ve tonu düzelt, yeniden
  boyama yapma; yine tutmazsa KALDI.
- Kenar/iç parlaklık oranı 0,97-1,03 (004e ölçütü).
- 6x6 döşeme testi: `grass_a/b/c` karışık zemin, ortasında 3x3 çayır öbeği. Öbeğin kenarı ızgara olarak seçilmemeli;
  yalnızca çiçekler fark edilmeli. Yan yana sayfasına 612'nin çayırıyla aynı testi de koy (karşılaştırma için).

## Notlar
- 612'nin çayır dosyalarına dokunulmaz (bulut işçisi var olanı değiştiremez); yeni dosyalar `_v2`.
- Kabulden sonra kod işi (kod oturumu): `IsoArt.MeadowTiles` `iso_ground_meadow_v2_a/b/c`'yi kullansın (yoksa eskisine
  düşsün). 612 çayırı emekliye ayrılabilir.
- `grass_dark` (612) kodda zaten kapalı ("kare leke gibi duruyordu"); ağaç altı gölgesi istenirse aynı ölçütle ayrı iş.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
