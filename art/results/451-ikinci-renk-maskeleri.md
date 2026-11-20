unity-import: pending

# 451 İkinci renk maskeleri (rapor)

Yan yana sayfası: [451-yanyana.png](451-yanyana.png). Masaüstü kopyası: yerel. PixelLab kullanılmadı (0 generation). `get_balance` sonrası: 3098,9.

## Araç
`tools/art/accent_mask.py` (anahtar renk + `--box`). Ana görsellere dokunulmadı. Chest, barrel ve hearth ilk denemede dağınık çıktı; kendi ürettiğim maskeler yeni anahtarlarla bir kez yeniden üretildi.

## Dosyalar (`Assets/Art/Iso/`)
| Eşya | Dosya | Boyut | Pivot | Bölge |
|---|---|---|---|---|
| `item.table_wood` | `_aday/iso_item_table_wood_2x1_se_nw_accent.png` | 112x88 | ana görselle aynı | ayak ve altın kenar (kutulu) |
| `item.chair_wood` | `_aday/iso_item_chair_wood_sw_accent.png` | 80x84 | ana görselle aynı | oturak |
| `item.chair_carved` | `_aday/iso_item_chair_carved_se_accent.png` | 80x88 | ana görselle aynı | oturak |
| `item.chest_wood` | `_aday/iso_item_chest_wood_se_accent.png` | 80x72 | ana görselle aynı | demir kulp ve kuşak |
| `item.bed_straw` | `iso_item_bed_straw_2x1_se_nw_accent.png` | 112x84 | ana görselle aynı | yorgan |
| `item.hearth_stone` | `iso_item_hearth_stone_unlit_se_accent.png` | 80x56 | ana görselle aynı | ocak demiri (yalnız 61 px; taşla aynı renkte, zayıf) |
| `item.barrel` | `_aday/iso_item_barrel_se_accent.png` | 80x72 | ana görselle aynı | çember (gölge/kontur payı dahil) |
| `item.bench_long` | `_aday/iso_item_bench_long_2x1_se_nw_accent.png` | 112x80 | ana görselle aynı | oturak (üst tahtalar) |

## Doğrulama
`validate_iso.py --kind object`: 8/8 geçti. `style.py check --family esya`: 8/8 UYARI (gri maske, ailesinden soluk; beklenen, maske oyunda boyanır).

## Sorunlar
- Eşya görselleri `_aday/` altında olduğundan maskeler de orada (hearth ve bed ana klasörde).
- Hearth maskesi çok küçük (61 px): demir ile taş neredeyse aynı renk; elle düzeltme önerilir.
- Chest ve barrel maskesi kontur/gölge pikseli de içerebilir; kullanıcı kontrol etmeli.
