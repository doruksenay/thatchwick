---
id: "737"
title: "Kıyı karosu yeniden: elmasa oturan su-kara kenarı (346 kıyı karosunun ara düzeltmesi)"
status: running
claimed_at: 2026-11-20T20:30Z
approved_by: user        # kullanıcı onayı 2026-11-20 (sohbet: "kıyı karosu ve çim tonu için PixelLab işi hazırla"; belge 65b B1)
priority: 10
family: zemin
style_gate: false
cost_est: 4
cost_max: 8
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_water_shore_v2.png
  - Assets/Art/Iso/iso_ground_water_shore_v2_b.png
created: 2026-11-20
---
# 737 Kıyı karosu yeniden (ara düzeltme)

## Neden
346'nın kıyı karosu (`iso_ground_water_shore`) 64x32 elmasa oturmuyor (belge 65b B1): 172 piksel elmasın dışına taşıyor,
166 piksel elmasın içinde boş; satır genişlikleri üst/orta/alt 4/50/14 (olması gereken 2/62/2). Sazlar karonun üstünden
taşıyor. Oyunda göl ve gölet kenarında boşluk ve bindirme yapıyor. Öteki bütün zemin karoları elmasa tam oturuyor.

Kalıcı çözüm 617 (çim ↔ su geçiş seti, dual-grid) ama 617, 613 ve 612'ye bağlı bekliyor. Bu iş o gelene kadar oyundaki
kıyıyı düzeltir. 617 kabul edilince bu karo emekliye ayrılabilir.

## Tarif
- Araç: `create_tiles_pro`, isometric 64 px, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`,
  stil modu; `style_images` = `iso_ground_water_a`, `iso_ground_grass_a`, `iso_ground_dirt_a` (346 ve 004e ile aynı aile).
- Tek karo, iki çeşit (a = `_v2`, b = `_v2_b`), farklı tohum.
- Açıklama: "isometric pond edge tile: calm shallow pond water (same muted blue-green and soft ripples as the reference
  water tile) meeting a narrow damp muddy bank along ONE edge only, the upper-right edge of the diamond; a thin strip of
  grass at the very edge matching the reference grass tile; a few short reed tufts lying flat on the mud, inside the
  tile; no tall reeds, no objects sticking out of the tile".
- **Kara kenarı yalnızca sağ üst kenarda** (+x komşusu yönü). Motor bunu sol üst kenar için yatay aynalıyor
  (`WorldView`: `flip = !landX`); bu yüzden kara şeridi tam olarak sağ üst kenar boyunca, köşelere eşit uzaklıkta olmalı.
  Sol üst, sol alt ve sağ alt kenarlar tamamen su.
- **Elmas maske kesin:** karo 64x32, opak alan tam elmas (1024 piksel), elmasın dışında opak piksel yok, içinde boşluk yok.
  Uzun saz istenirse karoya değil ayrı süs çıkartmasına aittir (709/710 düzeni); bu işte istenmez.
- Su kısmı `iso_ground_water_a` ile, çim şeridi `iso_ground_grass_a` ile dikişsiz birleşmeli.

## Doğrulama
- `validate_iso.py --kind ground` her karoda.
- Maske ölçümü: opak 1024, elmas dışı 0, elmas içi boş 0; satır genişlikleri üst/orta/alt 2/62/2. Tutmazsa KALDI.
- `style.py check --family zemin`.
- 3x3 döşeme testi: ortada su a/b, sağ üst kenar boyunca bu karo, onun ötesinde çim; bir de aynalanmış hâli sol üstte.
  Su ve çim tarafında dikiş görünmemeli.
- Su kısmının ortalama rengi `iso_ground_water_a` ile kanal başına ±6 içinde.

## Notlar
- Var olan `iso_ground_water_shore.png`'ye dokunulmaz (bulut işçisi var olan dosyayı değiştiremez); yeni dosyalar `_v2`.
- Kabulden sonra kod işi (kod oturumu): `IsoArt.Shore()` önce `iso_ground_water_shore_v2` / `_v2_b`'yi (konuma göre)
  döndürsün, yoksa eskisine düşsün. İki kenarı birden kara olan köşe karosu (landX ve landY) bugün de aynı karoyla
  çiziliyor; 617 gelince dual-grid bunu çözer.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
