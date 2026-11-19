unity-import: pending

# 448 Zemin varyantları (6 çift)

Yan yana sayfası: [448-yanyana.png](448-yanyana.png). Döşeme testleri: `art/results/448-doseme/`.

## Araç ve ayarlar
- `create_tiles_pro`, stil modu (333/335 yöntemi). `style_images` = `iso_ground_grass_a.png`, `iso_ground_dirt_a.png` (64x32, `get_upload_url` ile yüklendi); renk paleti kapalı (malzeme renkleri çimen paletinden çıkabilsin), outline/detail/shading açık.
- Tek çağrı, 12 numaralı açıklama (6 malzeme x 2). Maliyet çağrıdan önce okundu: 10-25 (tuval); gerçek 10. cost_max 35 içinde. Tile id `d5cfa678-274c-4eec-b1df-dd03a4ba7ddf`. 16 varyasyon döndü: t0-t11 sırayla a/b çiftleri, t12-t15 fazladan (kullanılmadı).
- Hız sınırı hatası olmadı. Yeniden deneme yok.
- Harcanan 10 gen. `get_balance` sonrası: 3112,9 (ortak bakiye, paralel şeritler).
- Tüm dosyalar maskeyle kırpıldı (elmas dışı ~100-120 köşe/dış çizgi pikseli silindi, içeriğe dokunulmadı); maske içinde delik yok.

## Dosyalar
| Dosya | Boyut | Pivot (x, y alttan) | validate_iso | Stil denetimi | Dikiş* |
|---|---|---|---|---|---|
| iso_floor_terracotta_a | 64x32 | 32, 16 | geçti | UYARI palete uzak (13.1, x1.22) | 0.865 |
| iso_floor_terracotta_b | 64x32 | 32, 16 | geçti | GEÇTİ | 0.814 |
| iso_floor_brick_herring_a | 64x32 | 32, 16 | geçti | GEÇTİ | 0.835 |
| iso_floor_brick_herring_b | 64x32 | 32, 16 | geçti | GEÇTİ | 0.838 |
| iso_floor_checker_a | 64x32 | 32, 16 | geçti | UYARI soluk (x0.18, siyah-beyaz doğası gereği) | 0.730 |
| iso_floor_checker_b | 64x32 | 32, 16 | geçti | UYARI soluk (x0.24) | 0.767 |
| iso_floor_black_flag_a | 64x32 | 32, 16 | geçti | UYARI soluk (x0.08, bazalt) | 0.844 |
| iso_floor_black_flag_b | 64x32 | 32, 16 | geçti | UYARI soluk (x0.08) | 0.898 |
| iso_floor_painted_boards_a | 64x32 | 32, 16 | geçti | UYARI doygun (x1.43) | 0.853 |
| iso_floor_painted_boards_b | 64x32 | 32, 16 | geçti | UYARI doygun (x1.34) | 0.848 |
| iso_floor_walnut_a | 64x32 | 32, 16 | geçti | GEÇTİ | 0.908 |
| iso_floor_walnut_b | 64x32 | 32, 16 | geçti | UYARI soluk (x0.59) | 0.915 |

*Dikiş = kenar bandı (elmas mesafesi > 0.85) / iç ortalama parlaklık, 8x8 döşemede; 333/335'teki ölçümden farklı tanım olabilir (kenar çizgisi koyu olduğu için 0.73-0.92). Checker en belirgin (0.73): kullanıcı döşeme testine bakmalı. Gölge pişirilmemiş, ışık sol-üstten. Zemin ailesi kilidi kurulmadı, palet uyumu yalnızca uyarı. Dönem kuralları: Yeni Dünya malzemesi yok. Masaüstü kopyası: yerel.

## Sorunlar
- Doygunluk `fit` uygulanmadı (şekil doğru; uyarılar kullanıcı kararına bırakıldı).
- Ham çıktılar `art/tmp448` altında, commit edilmez.
