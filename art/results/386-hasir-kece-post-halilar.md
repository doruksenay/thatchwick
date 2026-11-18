unity-import: pending

# 386 Hasır, keçe ve post halılar, döküntü yığını (rapor)

Yan yana sayfası: [386-yanyana.png](386-yanyana.png). Teslim edilmeyenler ve ham (doygunluk düzeltmesi öncesi) sürümler: `art/results/386-red/`. Masaüstü kopyası: yerel. Döşeme testi: zemin değil halı olduğu için uygulanmadı.

## Araç ve ayarlar
`create_map_object` stil eşleme, referanssız kılavuz + maske (`tools/art/footprint_guide.py`, halılar için yükseklik 4 px, döküntü 10 px), `high top-down`, `low detail`, `basic shading`, `single color outline`, 337 ortak ek metni. 8 çağrı x 1 gen = 8 (cost_max 14). Yeniden deneme yok.
- Hasır salon çağrısı hız sınırı (429) aldı; bekleyip bir kez yeniden gönderildi.
- Döküntü ve hasır salon çağrıları base64 aktarım bozulması yüzünden reddedildi (üretim başlamadı); kodlama değiştirilip aynı kılavuzla gönderildi.
- `get_balance` sonrası 4722,9 (paralel şeritler de harcıyor).

## Sonuç
| Eşya | Durum | Not |
|---|---|---|
| hasır 2x1 | teslim | GEÇTİ |
| hasır büyük 2x2 | teslim (UYARI) | doygunluk x1,49 → `fit` ile x1,15; "palete uzak" uyarısı |
| hasır salon 3x2 | teslim | doygunluk x1,61 → `fit` ile x1,15, GEÇTİ |
| keçe küçük 1x1 | TESLİM EDİLMEDİ | çıktı halı değil, 1 renkli çatı/köşk şekli (tanınmıyor) |
| keçe 2x1 | teslim | doygunluk x1,65 → `fit`, GEÇTİ; motifli |
| keçe büyük 2x2 | teslim | doygunluk x1,57 → `fit`, GEÇTİ; kenar çerçevesi var, koç boynuzu motifi belirsiz |
| keçi postu 1x2 | teslim (UYARI) | kahve lekeli dikdörtgen, gerçek post silueti değil |
| döküntü 1x1 | TESLİM EDİLMEDİ | validate BAŞARISIZ (56 renk > 48); mor-kızıl bloklar, döküntü olarak zayıf |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_rush_mat_2x1_se.png | 112x64 | 56 | 32 |
| iso_item_rush_mat_large_2x2_se.png | 144x80 | 72 | 40 |
| iso_item_rush_mat_hall_3x2_se.png | 176x96 | 88 | 48 |
| iso_item_felt_rug_2x1_se.png | 112x64 | 56 | 32 |
| iso_item_felt_rug_large_2x2_se.png | 144x80 | 72 | 40 |
| iso_item_goat_hide_1x2_se.png | 112x64 | 56 | 32 |

## Doğrulama (validate_iso, en çok 48 renk)
Teslim edilen altısı geçti (yarı saydam 0, alt boşluk 7-9 satır, opak sınırlar ayak izi kutusuyla örtüşür, kırpma gerekmedi). Döküntü 56 renkle kaldı.

## Stil denetimi (teslim edilenler, `style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| felt_rug_2x1 | %100 | 6,0 | x1,15 | GEÇTİ |
| felt_rug_large_2x2 | %100 | 6,8 | x1,16 | GEÇTİ |
| goat_hide_1x2 | %100 | 10,6 | x1,24 | UYARI |
| rush_mat_2x1 | %100 | 7,3 | x1,05 | GEÇTİ |
| rush_mat_hall_3x2 | %100 | 9,3 | x1,15 | GEÇTİ |
| rush_mat_large_2x2 | %100 | 12,1 | x1,15 | UYARI |

## Sorunlar / öneri
- Keçe küçük ve döküntü baştan (tek deneme hakkı bitti): `386b`.
- Keçi postu ve hasır büyük kullanıcı gözüyle bakılmalı. Işık sol-üstten, pişmiş gölge yok (gözle).
