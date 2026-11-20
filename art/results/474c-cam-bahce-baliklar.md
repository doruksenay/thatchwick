unity-import: pending

# 474c Cam bahçe balıkları ve tatlı su canlıları

- Araç: `create_image_pixen` x8 (16x16 ve 24x24, `no_background`, `view=side`, `direction=east`), 1 gen/çağrı, paralel; hız sınırı hatası yok. Toplam 8 gen. get_balance öncesi 3003,9; sonrası ayrıca okunmadı (paylaşılan havuz, diğer şeritler harcıyor), kalan ~3996-... defterde yaklaşık yazıldı.
- Yan yana: `art/results/474c-yanyana.png`. Masaüstü kopyası: yerel.
- Doygunluk düzeltmesi (`style.py fit`, hayvan ailesi): loach x0.62, shrimp x0.71, water_flea x0.56. Ham hâller `art/results/474c-red/`.

## Teslim (Assets/Art/Iso/_aday/), pivot x ortada, pivot y alttan = alt boşluk
| Dosya | Boyut | Pivot y (alttan) | validate | Stil denetimi |
|---|---|---|---|---|
| viv_shrimp.png | 16x16 | 1 | geçti | UYARI (fit sonrası, palet dışı, kilit yok) |
| viv_minnow.png | 24x24 | geçti, değer yazılmadı | geçti | UYARI (doygun) |
| viv_stickleback.png | 24x24 | 4 | geçti | GEÇTİ |
| viv_loach.png | 24x24 | 4 | geçti | GEÇTİ (fit x0.62) |
| viv_tadpole.png | 16x16 | geçti, değer yazılmadı | geçti | GEÇTİ |
| viv_pond_snail.png | 16x16 | geçti, değer yazılmadı | geçti | UYARI (palete uzak) |
| viv_goby.png | 24x24 | 5 | geçti | UYARI (palet dışı) |

Not: validate_iso tek dosya alır; tüm yedi dosya tek tek çalıştırıldı, hepsi geçti. Alt boşluklar: minnow 8 (yüzen), tadpole 2, snail 3.

## Teslim edilmedi
- viv_water_flea: fit sonrası bile "ailesinden çok doygun" (x1.51), turuncu/sarı. Yeniden deneme yeni iş olmalı.

## Sorunlar
- viv_goby: gökkuşağı benzeri benekli renk, "kumlu" değil; kullanıcı görsel kararı versin.
- viv_tadpole: şekil belirsiz; inceleyin.
- Hayvan ailesi kilidi kurulmadı (referans < 3): palet uyumu yalnızca uyarı.
