unity-import: pending

# 333 Zemin stilleri: ahşap (çam, meşe, balıksırtı)

Yan yana sayfası: [333-yanyana.png](333-yanyana.png). Döşeme testleri: [pine](333-doseme-pine.png), [oak](333-doseme-oak.png), [herringbone](333-doseme-herringbone.png).

## Araç ve ayarlar
- `create_tiles_pro`, stil modu. `style_images` = `iso_ground_grass_a.png`, `iso_ground_dirt_a.png` (64x32; seçilen: renk paleti kapalı, outline/detail/shading açık). Palet kapalı: ahşap tonları çimen paletinden çıkamaz.
- Tek çağrı, 6 numaralı açıklama (çam a/b, meşe a/b, balıksırtı a/b). Maliyet: 10 generation (araç açıklaması 10-25; 64x32 için 10). Cost_est 15, cost_max 20: sınır içinde.
- Araç 16 varyasyon döndürdü (6 açıklama 16 karoya yayıldı). Ek ücret yok, seçim aşağıda.
- Hız sınırı hatası olmadı. Yeniden deneme yok.

## Seçim (üretilen 16 karodan)
| Dosya | Kaynak karo | Not |
|---|---|---|
| iso_floor_pine_a | tile_12 | soluk, açık çam; ilk seçim (tile_0) stil denetiminde KALDI, bu yüzden değiştirildi |
| iso_floor_pine_b | tile_15 | sarımsı açık çam, çapraz tahta |
| iso_floor_oak_a | tile_4 | koyu meşe |
| iso_floor_oak_b | tile_6 | koyu meşe, farklı derz |
| iso_floor_herringbone_a | tile_11 | balıksırtı |
| iso_floor_herringbone_b | tile_10 | balıksırtı, daha az kontrast |

Not: tile_0/1/2/3 (parlak sarı çam) stil denetiminde KALDI (doygunluk x1.6-2.0, palete uzak), teslim edilmedi. Çam görünümü bu yüzden tarifteki "açık çam"dan biraz soluk (a) ve bir parça daha doygun (b, UYARI).

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/iso_floor_pine_a.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_floor_pine_b.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_floor_oak_a.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_floor_oak_b.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_floor_herringbone_a.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_floor_herringbone_b.png | 64x32 | 32 | 16 |

## Doğrulama
Tam 2:1 olmayan çıktı: ham karolarda maske dışı opak piksel vardı (120-124, kenardaki basamaklı çerçeve). Yalnızca maskeyle kırpıldı (maske dışı silindi, içerik yeniden boyanmadı). Kırpmadan sonra: maske dışı 0, maske içi delik 0, yarı saydam 0.

| Dosya | validate_iso (ground) | Renk sayısı | Stil denetimi | Doygunluk / aile |
|---|---|---|---|---|
| pine_a | geçti | 13 | GEÇTİ | x0.98 |
| pine_b | geçti | — | UYARI | x1.30 |
| oak_a | geçti | — | UYARI | x1.34 |
| oak_b | geçti | — | UYARI | x1.34 |
| herringbone_a | geçti | — | UYARI | x1.29 |
| herringbone_b | geçti | — | UYARI | x1.28 |

Renk sayısı: `--max-colors 0` ile ölçüldü (zemin için tavan tanımsız); ilk seçimde 13 renk görüldü, ahşap desen çimenden (4 renk) çok renk içeriyor. Kullanıcı gözüyle bakmalı.

Döşeme testi (8x8 elmas ızgara, (32,16) adımı, a/b karışık): dikiş parlaklık oranı (kenar/iç) çam 0.99, meşe 1.01, balıksırtı 1.01 (1.0 = görünür dikiş yok). Gölge pişirilmemiş, ışık sol-üstten (görsel kontrol).

## Sorunlar
- Çamın parlak sarı varyantları stil kilidini geçemedi; soluk çam tercih edildi. Kabul kararı kullanıcının.
- Kalan bakiye (`get_balance`, paralel şeritlerle ortak): 1031.7. Bu işin payı 10.
- Masaüstü kopyası: yerel.
