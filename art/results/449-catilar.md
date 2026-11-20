unity-import: pending

# 449 Çatı varyantları: sonuç (bulut işçisi, şerit 0, 2026-11-19)

Durum: **review (kısmi)**. 6 karodan 3 eğim karosu teslim edildi; 3 sırt karosu teslim edilmedi.

- Yan yana sayfası: [449-yanyana.png](449-yanyana.png)
- Döşeme testleri (8x8): [taş](449-doseme-stone_a.png), [sırlı](449-doseme-glazed_a.png), [kara kiremit](449-doseme-black_a.png)
- Reddedilen sırtlar: `449-red/`

## Araç ve ayarlar
- 336b yöntemi: `create_isometric_tile` x6 tek tek, `size 64`, `thin tile`, `selective outline`; 336b'deki eğim ve sırt açıklama ekleri aynen. Araç stil görseli almıyor (336b ile aynı).
- Araç 64x64 kalın döşeme döndürdü; üst yüz 64x32 elmas maskesiyle kırpıldı (yalnızca maske, yeniden boyama yok; kara sırtta 6 boş piksel en yakın komşuyla dolduruldu). Hız sınırı hatası olmadı.
- Maliyet araç açıklamasında yazmıyor. Bakiye 3112,9 → 3104,9 (paralel şeritler dahil), bu iş ≤8. **Kalan: 3104,9.** Masaüstü kopyası: yerel.

## Dosyalar (teslim)
| Dosya | Boyut | Pivot x, y (alttan) |
|---|---|---|
| Assets/Art/Iso/iso_roof_stone_slab_a.png | 64x32 | 32, 16 |
| Assets/Art/Iso/iso_roof_tile_glazed_a.png | 64x32 | 32, 16 |
| Assets/Art/Iso/iso_roof_tile_black_a.png | 64x32 | 32, 16 |

## Doğrulama
| Karo | validate_iso ground | Stil (yapi) | Doygunluk | Dikiş oranı | Karar |
|---|---|---|---|---|---|
| taş levha eğim | geçti | UYARI | x0,38 (soluk) | 0,90 | teslim |
| sırlı eğim | geçti | UYARI (%82) | x0,74 | 0,77 | teslim; yeşil baskın, okra/siyah elmas deseni zayıf |
| kara kiremit eğim | geçti | UYARI | x1,15 | 1,10 | teslim |
| taş sırt | geçti | UYARI | x0,30 | | sırt görünmüyor (düz doku); teslim edilmedi |
| sırlı sırt | geçti | KALDI (%34, x2,25); fit sonrası da KALDI (%68) | | | parlak yeşil/turuncu, sırt yok; teslim edilmedi |
| kara sırt | geçti | UYARI | x1,39 | | sırt görünmüyor; teslim edilmedi |

## Sorunlar
- 336b'deki gibi `create_isometric_tile` elmas içinde sırt örtüsü vermiyor (6 karonun 3'ü sırt, hiçbirinde okunur sırt hattı yok). `iso_roof_*_ridge` dosyaları eksik; oyun için sırt eğimden türetilmeli (336b önerisi: orta hatta ince koyu bant ya da `create_building_kit`). Karar kullanıcıda.
- Sırlı eğimin deseni tarifteki üç renkli elmas deseni yerine ağırlıkla yeşil.
