unity-import: pending
# 399b Yapı menüsü ikonları yeniden (bölüm 2/2)

- Araç: `create_map_object` 32x32, saydam, side, single color outline (enum'da "black" yok), basic shading, medium detail; 21 ikon. Hız sınırı (429) alan 11 çağrı bekleyip yeniden gönderildi. Okunmayan 5 ikon için ikinci deneme `create_image_pixflux` 32x32 no_background.
- `style_image` araç desteklemediği için stil referansı verilemedi.
- 16 renge indirildi (medyan kesim, yarı saydam 0). Maliyet ~26 gen (tahmin; hesap paylaşımlı). get_balance sonrası: 4313,9.
- Yan yana sayfa: `art/results/399b-yanyana.png` (19 yeni + 12 eski teslim edilmiş referans).
- Masaüstü kopyası: yerel.

## Teslim edilen (19): `Assets/Art/Iso/_aday/`, 32x32, pivot (16, 0) alttan orta
cat_animals, cat_barn, cat_decor_wall, cat_farm, cat_hygiene, cat_rug, cat_storage, cat_structure, cat_surfaces, cat_work, tool_copy_room, tool_fence, tool_road_gravel, tool_road_paved, tool_roof, tool_zone_pasture (ilk deneme); tool_floor, tool_road_path, tool_conduit_wood (ikinci deneme, pixflux).

## Teslim edilmeyen (2)
- `tool_gate`: iki denemede de taş kemerli kapı çıktı, "çit kapısı" okunmuyor.
- `tool_conduit_clay`: iki denemede de tütün piposu çıktı; dönem dışı ve boru okunmuyor.
Öneri: yeni açıklama (gate: "two wooden posts with a swinging plank bar", clay: "a short orange clay tube, open ends, straight cylinder").

## Stil denetimi (arayuz, kilit kurulmadı: uyarı)
UYARI: cat_barn, cat_rug, tool_conduit_wood, tool_roof (palet dışı); tool_road_gravel, tool_road_path, tool_road_paved (palete uzak). Diğerleri GEÇTİ, KALDI yok.

## Notlar
- Göz kontrolü (açık zemin): tool_fence çit/kapı arası; tool_road_path yeşil kenarlı tam kare. Kabul kararı kullanıcıda.
- Koyu zeminde ayrıca bakılmadı (yalnızca keten zemin sayfası görüldü).
- Ham ve reddedilen görseller `art/tmp/399b/` altında (git dışı).
