unity-import: pending

# 488 Tavşan yastığı
- Araç: `create_image_pixflux` 112x80, `no_background=true`, `isometric=true`, seed 488; 1 çağrı = 1 gen. `get_balance` (üretimden önce): 2811,9 (havuz paylaşımlı).
- Yan yana: `art/results/488-yanyana.png` (ham hâl). Ham: `art/results/488-red/iso_item_warren_se.png`. Masaüstü kopyası: yerel.
- Sonuç: **teslim edilmedi** (`failed`).

| Dosya | Boyut | validate | Stil denetimi |
|---|---|---|---|
| ham | 112x80, alt boşluk 8 (pivot y alttan 8) | geçti | KALDI (palet dışı, doygunluk x1.95) |
| fit x0.59 | 112x80 | geçti | KALDI (palete yakın %36, uzaklık 20.8) |

Sorunlar: doygunluk `fit` ile indi ama palet uzaklığı eşiğin (20) az üstünde kaldı. Zemin döşeme testi gerekmez (eşya). Pivot items.json'da (56,32).
