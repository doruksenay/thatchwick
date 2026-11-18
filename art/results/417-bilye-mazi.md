unity-import: pending

# 417 Bilye ve mazı yeniden (durum: review)

Yan yana sayfası: [417-yanyana.png](417-yanyana.png). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 2 x `create_map_object` basic mod, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (406 ile aynı yöntem), tarif açıklamaları + "simple still-life object..." eki. Paralel, hız sınırı yok. **2 gen** (cost_est 2).
- Bilye: araç yalnızca harita bağlamı (background_image) kabul ediyor, v3 kesesi init olarak verilemedi; kese tarifi aynen korundu, kese v3'ten farklı çıkabilir (kullanıcı kontrol etmeli).
- `get_balance` sonrası: **4096,9** (paralel şeritler dahil). `style.py fit` gerekmedi.

## Dosyalar (pivot x, pivot y ALTTAN)
| Yol (`Assets/Art/Iso/_aday/`) | Piksel | Pivot x | Pivot y |
|---|---|---|---|
| iso_item_marbles_se_v4.png | 32x32 | 16 | 4 |
| icon_oak_gall_v3.png | 32x32 | 16 | 0 (ikon) |

## Doğrulama (validate_iso --kind object --max-colors 48) ve stil denetimi
| Dosya | Renk | Palete yakın | Doygunluk | Stil |
|---|---|---|---|---|
| marbles_v4 | 25 | %92 | x0.99 | GEÇTİ, validate geçti |
| oak_gall_v3 | 8 | %100 | x1.02 | GEÇTİ, validate geçti |

Gözle (küçük önizleme): mazı kahve yuvarlak toplar, dalda, mor/kırmızı yok. Bilye: kahve kese, önünde açık renkli bilyeler; dört rengin hepsi seçilmeyebilir.

## Sorunlar
- Kese v3 ile birebir aynı olmayabilir (init görseli desteklenmedi).
- Dosya kopyası Python ile yapıldı (var olan dosya üzerine yazılmadı).
