unity-import: pending

# 862 Zemin leke çıkartmaları (bulut şeridi 3): FAILED, teslim yok

- Araç: `create_map_object`, stil eşleme (`background_image` = `iso_ground_grass_a` ile döşenmiş opak 64x32 çim, kesit), lineless, basic shading, low detail, 64x32. 358b'deki "opak çim arka plan + maske" yöntemi.
- Harcanan: 5 çağrı x 1 gen = 5. `get_balance` sonrası: 9721 (başlangıç 9733; paralel şeritlerin harcamaları dahil).
- Masaüstü kopyası: yerel.

## Denemeler
| Çağrı | Maske | Sonuç |
|---|---|---|
| shade (oval) | oval 43x23 | Düzgün, sert kenarlı yeşil oval; çok doygun, ortası parlak. Düzensiz değil |
| worn (oval) | oval | Çim halka + düz bej iç; kesin oval, çiğnenmiş çim gibi değil |
| tallgrass (oval) | oval | Neon sarı-yeşil, SİYAH dış çizgili oval (tarif dış çizgi yok diyor) |
| shade_a | düzensiz gürültülü (tohum 11, elmasın ~%70'i) | Açık yeşil, parçalı çerçeve; kaplama %19, koyu değil, ortası boş |
| shade_b | düzensiz (tohum 23) | Yarı saydam açık yeşil tek renk leke; doku yok |

Ham çıktılar ve sayfa: `art/results/862-red/`, `art/results/862-yanyana.png` (sol üst shade_a, orta üst shade_b, sağ üst tallgrass oval, sol alt worn oval, orta alt shade oval).

## Doğrulama
Hiçbir görsel biçim ölçütünü (düzensiz kenar, ana çimden 2-3 ton koyu aynı aile, %60-85 kaplama, dış çizgi yok) karşılamadı; validate/style/öbek testi çalıştırılmadı, `Assets/Art/Iso`'ya hiçbir şey konmadı.

## Sorunlar ve öneri
- Model maske içini kendi stiliyle (doygun, çizgili ya da sert kenarlı) dolduruyor; maske dışında çim bağlamı çıktıda saydam kalıyor. Oval maske ovali, düzensiz maske iskelet/solgun leke üretiyor. 358b'nin küçük süsleri için çalışan yöntem, karoyu kaplayan geniş leke için çalışmıyor.
- Öneri: lekeyi çimin kendi pikselleriyle yapmak: `pixellab: false` betik işi (ana çim karosunu yeniden renklendirip gürültü maskesiyle kesme, `art/tmp` betiği) ya da `create_tiles_pro`/`create_isometric_tile` ile düzensiz kenarlı ayrı karo. Karar kullanıcıda.
- Tarifteki "`iso_decal_clover_v2` hangi araçla üretildiyse" yöntemi 358b'de bulundu ama yukarıdaki nedenle yetersiz kaldı.
