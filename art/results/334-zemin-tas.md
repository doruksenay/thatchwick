unity-import: pending

# 334 Zemin stilleri: taş (yassı taş, arnavut, tuğla; sırlı çini teslim EDİLMEDİ)

Yan yana sayfası: [334-yanyana.png](334-yanyana.png). Döşeme testleri: [flag](334-doseme-flag.png), [cobble](334-doseme-cobble.png), [brick](334-doseme-brick.png).

## Araç ve ayarlar
- `create_tiles_pro`, stil modu (002/333 ile aynı). `style_images` = `iso_ground_grass_a.png`, `iso_ground_dirt_a.png` (64x32); `style_options` = renk paleti kapalı, outline/detail/shading açık (taş, tuğla ve çini tonları çimen paletinden çıkamaz; 333'teki gibi).
- Tek çağrı, 8 numaralı açıklama (yassı taş a/b, arnavut a/b, tuğla a/b, sırlı çini a/b). Araç 16 karo döndürdü. Maliyet: **10 generation** (araç açıklaması 10-25, 64x32 için 10). cost_est 20, cost_max 25: sınır içinde.
- Hız sınırı hatası olmadı. Yeniden deneme yok.
- `get_balance` çağrı sonrası: 916,70 (paralel şeritler aynı hesabı kullandığı için bu sayı yalnızca bu işin harcaması değil).

## Seçim (16 karodan)
| Dosya | Kaynak karo | Not |
|---|---|---|
| iso_floor_flag_a | tile_8 | gri yassı taş |
| iso_floor_flag_b | tile_9 | açık gri yassı taş |
| iso_floor_cobble_a | tile_10 | arnavut kaldırımı |
| iso_floor_cobble_b | tile_11 | arnavut kaldırımı, farklı düzen |
| iso_floor_brick_a | tile_5 | kırmızı tuğla, stil denetimi GEÇTİ |
| iso_floor_brick_b | tile_4 | kırmızı tuğla |

tile_0-3 (ilk yassı taş ve arnavut kümesi) maske içinde 8-14 delik bıraktığı için seçilmedi; deliksiz tile_8-11 alındı. Boşluk doldurma yapılmadı.

**Teslim edilmeyenler: iso_floor_tile_a, iso_floor_tile_b (sırlı çini).** Dört çini adayının hepsi (tile_6, 7, 14, 15) stil denetiminde KALDI (palete yakın %33-39, palet dışı renkler: turkuaz/krem çimen-toprak ailesinin dışında). Tek deneme kuralı gereği yeniden üretilmedi. Öneri: palet kapalı olmadan farklı bir stil referansıyla (örn. kabul edilmiş bir çini/duvar karosu) yeni iş, ya da `style_options.color_palette` açık + turkuaz paletle ayrı bir çağrı. Çıktı dosyalarında 2 eksik var.

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/iso_floor_flag_a.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_floor_flag_b.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_floor_cobble_a.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_floor_cobble_b.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_floor_brick_a.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_floor_brick_b.png | 64x32 | 32 | 16 |

## Doğrulama
Tam 2:1 olmayan çıktı: ham karolarda maske dışı opak piksel vardı (110-120 piksel, kenarda basamaklı çerçeve). Yalnızca maskeyle kırpıldı (maske dışı silindi, içerik yeniden boyanmadı). Sonuç: maske dışı 0, maske içi boş 0, yarı saydam 0.

| Dosya | validate_iso (ground) | Renk sayısı | Stil denetimi | Doygunluk / aile |
|---|---|---|---|---|
| flag_a | geçti | 19-23 | UYARI (palete uzak, soluk) | x0.05 |
| flag_b | geçti | 19-23 | UYARI (palete uzak, soluk) | x0.08 |
| cobble_a | geçti | ~20 | UYARI (soluk) | x0.10 |
| cobble_b | geçti | ~20 | UYARI (soluk) | x0.12 |
| brick_a | geçti | ~24-26 | GEÇTİ | x1.02 |
| brick_b | geçti | ~24-26 | UYARI (palete uzak) | x1.09 |

Gri taşlar "soluk" uyarısı alıyor çünkü çimen/toprak ailesinden doğal olarak az doygun; taş için beklenen durum, kullanıcı gözüyle bakmalı. Renk sayısı çimene göre (4-5) çok yüksek (`--max-colors 0` ile ölçüldü).

Döşeme testi (8x8 elmas ızgara, (32,16) adımı, a/b karışık): dikiş parlaklık oranı (kenar bandı / iç) yassı taş 0.81, arnavut 0.83, tuğla 0.86. 1.0'ın belirgin altında: karolar koyu iç çerçeveyle geliyor (`outline_mode: outline`), döşenince ızgara çizgisi görünebilir. 333'teki ahşapta 0.99-1.01'di; bu iş için `outline_mode: segmentation` ile yeni bir deneme düşünülebilir. Gölge pişirilmemiş, ışık sol-üstten (görsel kontrol).
