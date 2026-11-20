unity-import: pending

# 336t Çatı tamamlama: sonuç (bulut işçisi, şerit 0, 2026-11-20)

Durum: **review (kısmi)**. 11 karodan 1'i teslim edildi (saz v2); 10'u teslim edilmedi.

- Yan yana sayfası (saz fit, şindıl, kara kiremit, arduvaz + referanslar): [336t-yanyana.png](336t-yanyana.png)
- Saz döşeme testi (8x8): [336t-doseme-thatch.png](336t-doseme-thatch.png). Reddedilenler: `336t-red/`

## Araç ve ayarlar
- 336b yöntemi: `create_isometric_tile` x11 (4 eğim + 7 sırt), `size 64`, `thin tile`, `selective outline`. Tarifteki yeni malzeme ve renk tarifleri kullanıldı. Araç stil görseli almıyor. Maliyet araç açıklamasında yok (çağrı başına ~1 gen varsayımı). Bir sırt çağrısı hız sınırına (10/10) takıldı, bekleyip bir kez yeniden denendi.
- Üst yüz (satır 24-55) 64x32 elmas maskesiyle kırpıldı; maske dışı silindi, içte en çok 4 boş piksel en yakın komşuyla dolduruldu.
- Bakiye: 2663,9 → 2648,9 (paylaşılan şeritler dahil; bu iş ~11). **Kalan: 2648,9.** Masaüstü kopyası: yerel.

## Dosyalar (teslim)
| Dosya | Boyut | Pivot x, y (alttan) |
|---|---|---|
| Assets/Art/Iso/_aday/iso_roof_thatch_a_v2.png | 64x32 | 32, 16 |

## Doğrulama
| Karo | validate_iso ground | Stil (yapi) | Ton | Dikiş oranı | Karar |
|---|---|---|---|---|---|
| saz eğim v2 | geçti | ham KALDI (x2,76); fit sonrası GEÇTİ x1,15 | altın-buğday | 0,93 | teslim (fit uygulandı; ham `336t-red/`) |
| şindıl eğim v2 | geçti | UYARI (x0,47) | gri-mor/pembemsi (ton 346°) | 0,85 | teslim edilmedi: mor-pembe ton, tarif "mor olmamalı" |
| kara kiremit v2 | geçti | UYARI (x0,72) | mor-koyu (ton 280°) | 0,86 | teslim edilmedi: antrasit değil, mor |
| arduvaz eğim | geçti | KALDI (palet dışı %37) | mavi-gri | 0,81 | teslim edilmedi |
| 7 sırt (saz, şindıl, kiremit, arduvaz, taş, sırlı, kara) | geçti | 5 KALDI, 2 UYARI/GEÇTİ | | | teslim edilmedi |

## Sorunlar
- **Sırtlar yine başarısız:** `create_isometric_tile` okunur bir sırt vermiyor. Taş ve arduvaz sırtı düz doku; saz sırtı çerçeveli düz levha; kiremit sırtı pembe şevron; sırlı sırt parlak yeşil piramit; kara sırt lacivert diyagonal çizgiler; şindıl sırtı mor. Üçüncü kez aynı sonuç: araç sırt için uygun değil.
- **Şindıl ve kara kiremit** gene mor/pembe kaydı; renk sözcükleri ("not purple", "no pink") yön vermedi.
- Öneri: sırtları eğim karosundan türetmek (orta hatta bant, kod ya da ayrı `pixellab: false` iş) ya da `create_building_kit` `roof_profile`. Şindıl/kara kiremit için renk, `fit` yerine renk eşleme (hue kaydırma) ile düzeltilebilir. Karar kullanıcıda.
