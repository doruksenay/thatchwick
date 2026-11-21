unity-import: pending

# 860 Sıcak çim karoları pilotu

Durum: review (görsel onay kullanıcıda). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_tiles_pro`, tek çağrı (maliyet açıklamada 10-25, gerçek: **10 generation**). `isometric`, `tile_size=64`, `tile_view_angle=30`, `tile_depth_ratio=0`, `tile_flat_top_px=2`, `outline_mode=segmentation` (dikişsiz, çerçevesiz yüzey), `max_colors=6`. Tarifteki 3 çim + 1 toprak açıklaması tek çağrıda numaralı (1-4) verildi; çağrı 16 varyasyon döndürdü (id `b78f4602-f4df-4d5f-b04b-3ca55d2b6fff`).
- Çıktı 64x64 tuvalde 64x32 elmas; 16 karodan 4 seçildi, `y=16..48` kırpıldı ve doğrulayıcının elmas maskesiyle maske dışı silindi (içerik yeniden boyanmadı). Çim karolarının otları elmas dışına taşıyordu, bu kırpmada kesildi.
- Seçim: a = varyasyon 0, b = 4, c = 8 (açık sarı-yeşil tutamlı), toprak = 12. b için 3 ve 2 numaralılar maske içinde boş piksel bıraktığından elendi.
- `fit` uygulanmadı (hedef ton bilinçli olarak eski çimden daha sıcak ve az doygun).
- Kalan bakiye (`get_balance`): 9694 (aynı anda çalışan diğer şeritlerin harcamasını da içerir).

## Dosyalar (`Assets/Art/Iso/_aday/`)
| Dosya | Boyut | Pivot x / y (alttan) |
|---|---|---|
| iso_ground_grass_cozy_a.png | 64x32 | 32 / 16 |
| iso_ground_grass_cozy_b.png | 64x32 | 32 / 16 |
| iso_ground_grass_cozy_c.png | 64x32 | 32 / 16 |
| iso_ground_dirt_cozy_a.png | 64x32 | 32 / 16 |

## Doğrulama
| Dosya | validate_iso | Renk | Stil denetimi |
|---|---|---|---|
| grass_cozy_a | geçti | 3 | GEÇTİ (doygunluk x0.75) |
| grass_cozy_b | geçti | 3 | UYARI: palete uzak (x0.76) |
| grass_cozy_c | geçti | 4 | GEÇTİ (x0.78) |
| dirt_cozy_a | geçti | 1 | GEÇTİ (x1.11) |

- Döşeme (12x12, a/b/c karışık): `art/results/860-doseme.png`. Dikiş / karo içi parlaklık oranı **0,99** (eşik 1,25; eski çim 1,01). Izgara çizgisi seçilmiyor.
- Yan yana (eski çim ve yeni çim, aynı ağaç, kuyu, köylü, patika karosu; 2 kat): `art/results/860-yanyana.png`. Stil referanslarıyla sayfa: `art/results/860-yanyana-stil.png`.
- Gölge pişirilmemiş, saydam arka plan, anti-alias yok, ışık nötr.

## Sorunlar
- **Toprak düz renk**: tek renk (194,159,119), çakıl ve doku yok; tarifteki "birkaç silik çakıl" tutmadı. Dört toprak varyasyonunun hepsi aynıydı. Kullanıcı kabul etmeden ikinci deneme yapılmadı; istenirse ayrı iş.
- Çim karolarında tutam desenleri 12x12 alanda hafif tekrar ediyor (yalnızca 3 çeşit); geniş alanda daha çok varyant gerekebilir (16 varyasyonun 12'si çim, biri b gibi dönüştürülebilir).
- Çim varyasyonlarının çoğunda otlar elmas üstüne taştığı için kesildi; yalnızca 0, 4, 8, 10 doğal sığdı.
- Seçilen çim karolarında çiçek yok (açık sarı-yeşil tutamlar çim öbeği).
- Başka varyasyonlar (1-11) ham olarak `art/tmp860/` altında (commit edilmedi).
