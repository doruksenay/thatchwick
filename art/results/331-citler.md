unity-import: pending

# 331 Çitler (bulut işçisi, şerit 0, 2026-11-18)

Yan yana sayfası: [331-yanyana.png](331-yanyana.png) (sol üç görsel referans; sonra hedge, picket, stone, wattle; her biri se_nw ve sw_ne)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`; 5 gen/çağrı (araç açıklaması ve `estimated_generations` 5), 8 çağrı = **40 generation** (cost_est 40).
- Kaynak `iso_wall_wall_low_*` 96x96 kırpma (sw_ne ofset (0,19), se_nw (6,19)); maske = alçak duvar opak bölgesi + üstünde 10 px (betikle). Çıktı aynı ofsetle 102x115 şeffaf tuvale geri konuldu. Görseller `upload:` ile gönderildi (ilk base64 denemesi bozuldu, 0 gen).
- Eşzamanlılık sınırı (8 iş): taş se_nw çağrısı reddedildi, slot açılınca bir kez yeniden denendi.
- `get_balance` sonrası: **635.1** (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (teslim edilen)
| Dosya | Boyut | Pivot (x, y ALTTAN) |
|---|---|---|
| iso_wall_fence_wattle_sw_ne.png | 102x115 | 32, 19 |
| iso_wall_fence_wattle_se_nw.png | 102x115 | 70, 19 |
| iso_wall_fence_picket_sw_ne.png | 102x115 | 32, 19 |
| iso_wall_fence_picket_se_nw.png | 102x115 | 70, 19 |
| iso_wall_fence_stone_sw_ne.png | 102x115 | 32, 19 |
| iso_wall_fence_stone_se_nw.png | 102x115 | 70, 19 |

## Teslim EDİLMEYEN
- `iso_wall_fence_hedge_sw_ne/se_nw`: stil denetimi KALDI (palete yakın %11/%0, doygunluk x1.64-1.66; çok parlak yeşil). Tek deneme kuralı gereği tekrar yok; çıktı `art/tmp331/` içinde (commit dışı), yeniden iş gerekir.

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: sekizi geçti (renk 13-30).

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| hedge_se_nw | %0 | 31.0 | x1.66 | KALDI |
| hedge_sw_ne | %11 | 28.9 | x1.64 | KALDI |
| picket_se_nw | %100 | 4.0 | x0.97 | GEÇTİ |
| picket_sw_ne | %100 | 5.7 | x0.91 | GEÇTİ |
| stone_se_nw | %100 | 6.6 | x0.27 | UYARI (soluk) |
| stone_sw_ne | %100 | 7.6 | x0.24 | UYARI (soluk) |
| wattle_se_nw | %100 | 4.3 | x1.06 | GEÇTİ |
| wattle_sw_ne | %100 | 6.2 | x1.06 | GEÇTİ |

## Sorunlar
- Çit boyu ve duvarla dikiş yalnızca yan yana sayfasından gözle bakılabilir; döşeme testi zemin işi olmadığı için yapılmadı.
- Çit bitkisi için yeniden iş önerilir (daha sönük yeşil, ana palet).
