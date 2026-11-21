unity-import: pending

# 790 Doğal kaya zemini (rockbed x3, quarry x3)

Sonuç: 6 karo üretildi, doğrulandı, teslim edildi. İş `review`.
Yan yana sayfası: [790-yanyana.png](790-yanyana.png) (3 zemin referansı + 6 yeni). Döşeme testi: [790-doseme.png](790-doseme.png).
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
- `create_tiles_pro`, isometric 64, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`, seed 7901, **şekil modu**. Tek çağrı, 6 numaralı karo (1-3 rockbed, 4-6 quarry). Tile id `4b9ac234-9e20-4c30-9df8-a7d6194e918f`. PixelLab 16 varyasyon döndü (0-7 rockbed, 8-15 quarry).
- **Sapma:** tarif stil modu (`style_images`) istiyordu. Referansları bulutta URL/upload olarak veremedim (satır içi base64 kesilme riski, `get_upload_url` kullanılmadı); 612 ile aynı şekilde şekil modu kullanıldı. Stil denetimi yine de geçti (palete %100 yakın).
- Seçilen: rockbed a/b/c = tile_0/1/2, quarry a/b/c = tile_8/9/10. Her biri 64x64 tuvalden 64x32 elmasa kırpıldı; maske dışı 31-35 piksel yalnızca maskeyle silindi (içerik yeniden boyanmadı).
- Harcanan: 10 gen (`cost_max` 12 içinde, `cost_est` 6'nın altı değil; araç fiyatı 10, sapma 2 katı aşmıyor). `get_balance` sonrası: 9666 (paralel şeritler dahil).
- Hız sınırı hatası olmadı.

## Dosyalar (Assets/Art/Iso/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_ground_rockbed_a.png | 64x32 | 32 | 16 |
| iso_ground_rockbed_b.png | 64x32 | 32 | 16 |
| iso_ground_rockbed_c.png | 64x32 | 32 | 16 |
| iso_ground_quarry_a.png | 64x32 | 32 | 16 |
| iso_ground_quarry_b.png | 64x32 | 32 | 16 |
| iso_ground_quarry_c.png | 64x32 | 32 | 16 |

## Doğrulama
| Dosya | validate_iso --kind ground | Renk | Stil denetimi (zemin) |
|---|---|---|---|
| rockbed_a | geçti (opak 1024, maske dışı 0, boşluk 0) | 17 | UYARI soluk (%100 palete yakın, uzaklık 8,0, x0,29) |
| rockbed_b | geçti | 14 | UYARI soluk (%100, 9,5, x0,30) |
| rockbed_c | geçti | 21 | UYARI soluk (%100, 8,9, x0,32) |
| quarry_a | geçti | 8 | UYARI soluk (%100, 8,9, x0,54) |
| quarry_b | geçti | 10 | UYARI soluk (%100, 8,8, x0,55) |
| quarry_c | geçti | 10 | UYARI soluk (%100, 8,8, x0,55) |

KALDI yok. Uyarı "ailesinden soluk": tarif "doygunluk düşük" istiyordu, gri kaya için beklenen.
Döşeme (8x8 elmas, adım 32,16, çeşitler karışık): kenar/iç parlaklık rockbed 0,992, quarry 1,008 (hedef 0,97-1,03, tuttu). 8x8 testte kenarda çim, ortada 2x2 quarry. Gölge pişirilmemiş, ışık sol-üstten.

## Sorunlar
- rockbed çatlak ağı yer yer "kırık kaldırım taşı" gibi okunabilir (çatlaklar poligon hücre çiziyor); harç çizgisi ya da sıra yok, ızgara seçilmiyor. Görsel karar kullanıcıda.
- Yan yana sayfasına bugünkü `Stone` (taş döşeme) karşılaştırması eklenmedi (araç yalnızca zemin referanslarını koyuyor).
- Kullanılmayan 10 varyasyon (tile_3-7, 11-15) teslim edilmedi; id yukarıda.
