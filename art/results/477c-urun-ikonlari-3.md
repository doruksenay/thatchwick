unity-import: pending

# 477c Ürün ikonları: beceri tamamlama 3/4: sonuç (durum: review)

Yan yana sayfası: `art/results/477c-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca 16 yeni görsel).

## Araç ve ayarlar
- `create_map_object` basic mod, 32x32, `low detail`, `basic shading`, `single color outline`. Dik nesneler (saddle, mumlar, fener, rayı, bocurgat) `side`, düz nesneler `high top-down`. Açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". Gerekçe: 420a dersi (pixen yüzlü çıkıyor), pixen turu atlandı.
- 18 başarılı çağrı x 1 gen = **18 gen** (`cost_est` 18, `cost_max` 36). Hız sınırı (rate limit) nedeniyle 5 çağrı reddedildi (harcama yok); bekleyip bir kez yeniden gönderildi (tarifin tek istisnası).
- `get_balance` sonrası: **2523,9** (paylaşılan hesap, paralel şeritlerin harcaması dahil).
- Renk indirgeme: >16 renkli 6 ikon (herb_cheese, hund_rail, old_keepsake, patch_quilt, wedding_dish, windlass) 16'ya median-cut ile indirildi (dither yok, alfa 0/255). Yeniden boyama yok.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
race_saddle (7 renk), glass_pane (11), glazed_tile (13), bay_leaf (5), bay_soap (15), festival_candle (8), sealing_wax (10), marble (11), master_lantern (11), hund_rail (15), windlass (15), patch_quilt (15), old_keepsake (15), herb_cheese (15), wedding_dish (15), bazlama (13). Hepsi `icon_item_` önekli.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (16/16) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (16/16) |
| `style.py check --family arayuz` | KALDI yok (teslim edilenlerde). GEÇTİ: herb_cheese, hund_rail, marble, race_saddle, wedding_dish, windlass. UYARI (kilit kurulmadı / palete uzak): bay_leaf, bay_soap, bazlama, festival_candle, glass_pane, glazed_tile, master_lantern, old_keepsake, patch_quilt, sealing_wax |
| `--light` (mine/festival/scented candle, master_lantern) | master_lantern GEÇTİ, festival_candle UYARI, **mine_candle KALDI, scented_candle KALDI** (ışık payı %35'i aşıyor: alev/parıltı) |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

## Teslim edilmeyenler
- `icon_item_mine_candle`, `icon_item_scented_candle`: ışık payı denetiminde KALDI (şekil olarak tanınıyor, sorun doygunluk değil). Ham hâller `art/results/477c-red/` altında. Yeniden üretim önerilir (2 gen, daha az parlak alev tarifiyle).

## Sorunlar
1. Gözle: bay_leaf az renkli ama tanınıyor; bay_soap zümrüt yeşili (aileden doygun, UYARI); old_keepsake kutu+kurdele belirsiz; patch_quilt çok doygun.
2. Teslim edilen 16 ikonda yüz/göz/figür yok.
