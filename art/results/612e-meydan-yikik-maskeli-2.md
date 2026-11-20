unity-import: pending

# 612e Köy meydanı: yıkık hâller (maskeli 2) — KISMİ

Sonuç: 1/3 teslim (köprü). Çark ve ocak yapılmadı: kaynakları (`iso_square_millwheel_v2_se`, `iso_square_hearth_se`) 612c'de teslim edilmedi (`art/results/612c-red/` içinde; kendi zemini ve alevli/korlu olduğu için reddedildi), `_aday/` altında yok. Redli kaynakla maskeli düzenleme yapmak tahmin olur; bu yüzden `blocked`.

## Köprü
- Araç: `inpaint_image_pro_flash` (Modify current layer, no_background), kaynak `_aday/iso_square_bridge_v2_se.png`, dikdörtgen maske x72-137, y10-94 (`art/tmp/bridge_mask.png`), tek deneme, 6 gen. Kalan (get_balance): 809,6.
- Dosya: `Assets/Art/Iso/_aday/iso_square_bridge_ruin_v2_se.png`, 192x128, pivot x 96, y 4 (alttan; en alt opak satır 123).
- Yan yana sayfası: [612e-yanyana.png](612e-yanyana.png) (3 referans + yeni).

| Denetim | Sonuç |
|---|---|
| validate_iso --kind object --max-colors 48 | geçti, 29 renk |
| style.py check --family yapi | UYARI: %100 palete yakın, uzaklık 6,6, ailesinden soluk (x0.31); teslim |
| Maske dışı fark | 0 piksel |
| Uçlar | sol ve sağ uç kaynakla aynı pikselde |

Ortada kalaslı geçit ve moloz okunuyor; sağ üstte korkuluk taşları dağılmış.

## Sorular
- Çark ve ocak için önce 612c'nin yeniden üretimi gerekir (çark: dikey su çarkı, kendi zemini yok; ocak: sönük, kor yok). Sonra yıkık hâlleri ayrı iş olarak maskeli yapılabilir.
- Masaüstü kopyası: yerel.
