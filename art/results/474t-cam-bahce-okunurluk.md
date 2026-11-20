unity-import: pending

# 474t Cam bahçe canlıları okunurluk düzeltmesi

- Araç: `create_image_pixen` x4 (16x16 x2, 24x24 x2; `no_background`, `view=side`, `direction=east`, siyah outline, `low detail`), paralel, 1 gen/çağrı = 4 gen. Hız sınırı hatası yok. get_balance öncesi 2659,9 (sonrası ayrıca okunmadı, havuz paylaşımlı).
- Yan yana: `art/results/474t-yanyana.png`. Masaüstü kopyası: yerel.
- Tek deneme yapıldı; ham indirmeler `art/tmp474t/` (commit dışı).

| Dosya | Boyut | Pivot y (alttan) | validate | Stil |
|---|---|---|---|---|
| iso_viv_firefly_v2.png | 16x16 | 3 | geçti | UYARI (doygun x1.49) |
| iso_viv_ground_beetle_v2.png | 16x16 | 2 | geçti | UYARI (soluk x0.43) |
| iso_viv_goby_v2.png | 24x24 | 6 | geçti | UYARI (doygun x1.37) |
| iso_viv_stickleback_v2.png | 24x24 | 3 | geçti | UYARI (palete uzak, %80) |

## Sorunlar
- Hayvan ailesi kilidi yok; palet yalnızca uyarı. Ateşböceği kuyruk ışığı `check --light` ile ayrıca denenmedi.
- Gözle: yer böceği ve ateşböceği net siluetli; goby kahve-kum, gökkuşağı yok; stickleback zeytin/gümüş, kırmızı boğaz var. 3 dikenin ayrı olup olmadığı kullanıcı kararına bırakıldı.
