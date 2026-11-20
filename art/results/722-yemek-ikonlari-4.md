unity-import: pending

# 722 Yemek ikonları 4: sonuç (durum: review, KISMİ: 3/13 teslim)

Yan yana: `art/results/722-yanyana.png` (arayuz ailesinde referans yok). Önizleme x4 (parşömen + koyu): `art/results/722-ikonlar-x4.png`.

## Araç ve ayarlar
- `create_image_pixen` 32x32, `no_background`, `single color outline`, `low detail`, her açıklamaya "simple still-life object, no face, no eyes, no creature, no person" eklendi (316 yöntemi: ikon başına çağrı). 13 çağrı x 1 gen = **13 gen** (`cost_est` 13, `cost_max` 26).
- Hız sınırı (10/10 iş, paylaşılan hesap): 6 çağrı reddedildi (harcamasız), bekleyip bir kez yeniden gönderildi.
- Renk: ham 35-71 renk, Pillow median-cut ile 16'ya indirildi (dither yok, alfa 0/255). Ham çıktılar commit edilmedi.
- `get_balance` iş başında 1562,7; iş sonrası değer paylaşılan hesapta paralel şeritlerle karıştığı için yaklaşık 1549 (defter).
- Masaüstü kopyası: yerel.

## Teslim edilenler (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0)
icon_item_chestnut_candy, icon_item_cokelek, icon_item_duck_kaygana

## Teslim edilmeyenler (`art/results/722-red/`)
Tarifteki "yüz/göz/figür yok" kuralı ihlali (pixen yine yüz/uzuv çizdi): cherry_tart (fare figürü), chestnut_bread (yüz), dried_fruit_ration (kollu figür), honey_milk (yüz), honey_nuts (yüz; ayrıca stil denetimi KALDI, %66 palet), milk_wheat (yüz), paste_pilaf (yüz), pear_hosaf (yüz). olive_bread ve walnut_bun yüzsüz ama yanlış nesne (koyu turta; çuval/doku). Tek deneme kuralı gereği yeniden denenmedi.

## Doğrulama
| Dosya | Renk | Stil denetimi |
|---|---|---|
| chestnut_candy | 16 | GEÇTİ |
| cokelek | 14 | UYARI (palete uzak) |
| duck_kaygana | 15 | UYARI (palete uzak) |
| diğer 10 | ≤16 | GEÇTİ/UYARI, honey_nuts KALDI; asıl ret nedeni görsel (yüz/yanlış nesne) |

`validate_iso.py`: uygulanmadı (UI ikonu). 32x32 ve yarı saydam piksel yok: 13/13.

## Sorular
10 ikon için yöntem notundaki yedek yol (`create_map_object` 32x32, 401/417) ayrı bir iş olarak gerekir (~10 gen): paste_pilaf, milk_wheat, honey_milk, honey_nuts, chestnut_bread, walnut_bun, cherry_tart, olive_bread, pear_hosaf, dried_fruit_ration.
