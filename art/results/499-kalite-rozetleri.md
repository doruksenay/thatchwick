unity-import: pending

# 499 Eşya kalitesi rozetleri (4)

- Araç: `create_map_object` 32x32, `view: side`, `single color outline`, `basic shading`, `medium detail` (480 ile aynı yöntem, tarifin ortak eki dahil).
- Çağrılar: 4 paralel (1 gen/çağrı) + mavi rozet için 1 yeniden deneme (`max_attempts: 2`) = 5 generation. `cost_max` 12.
- 16 renge indirme yerel PIL ile yapıldı (medyan kesme, dithering yok, alfa 0/255); `reduce_colors` çağrılmadı.
- `get_balance` sonrası: 2461,9 (diğer şeritlerle ortak bakiye; bu işin harcaması 5).
- Yan yana sayfası: [499-yanyana.png](499-yanyana.png). Aile `arayuz` kilidi kurulmadığı için sayfada referans yok (0 referans + 4 yeni).
- Masaüstü kopyası: yerel.

## Dosyalar
| Dosya | Boyut | Pivot x | Pivot y (alttan) | Renk |
|---|---|---|---|---|
| Assets/Art/Iso/_aday/ui_icon_quality_1.png | 32x32 | 16 | 0 | 7 |
| Assets/Art/Iso/_aday/ui_icon_quality_2.png | 32x32 | 16 | 0 | 15 |
| Assets/Art/Iso/_aday/ui_icon_quality_3.png | 32x32 | 16 | 0 | 14 |
| Assets/Art/Iso/_aday/ui_icon_quality_4.png | 32x32 | 16 | 0 | 15 |

UI ikonu olduğu için pivot anlamsızdır, hücre merkezi alındı.

## Doğrulama
| Dosya | 32x32 | yarı saydam | renk | validate_iso | style check |
|---|---|---|---|---|---|
| 1 | evet | 0 | 7 | geçti | UYARI (kilit yok; palete yakın %69) |
| 2 | evet | 0 | 15 | geçti | UYARI (palete yakın %37) |
| 3 | evet | 0 | 14 | geçti | UYARI (palete yakın %29) |
| 4 | evet | 0 | 15 | geçti | UYARI (palete yakın %54) |

Stil denetimindeki UYARI'lar yalnızca "kilit kurulmadı, palet uyumu uyarıdır" notundan gelir; KALDI yok.

## Sorunlar ve notlar
- **Mavi rozet (2) ilk denemede lacivert çıktı** (en sık renk #2B2647, hedef #5C99F2'den çok koyu). İkinci denemede tarife "bright sky blue thread, light blue stars on cream" eklendi; sonuç #0A63D9 / #49A5F0 ağırlıklı, hedefe yakın. İlk ham hâl `art/results/499-red/q2-lacivert-ham.png`.
- Rozetlerde keten yama çizilmedi: yıldız rozetler (1, 2) yalnızca yıldız şekli olarak geldi. Yeşil ve mavi yıldız sayısıyla ayrılıyor (1 ve 2), okunurluk bakımından sorun yok.
- 3 (mor mühür) ve 4 (altın mühür) renkle ve 4'teki madalya/kurdele ile ayrılıyor; 3'te kurdele yok.
- 12x12 ve 16x16 küçültme okunurluğu görsel olarak kullanıcı kontrolüne bırakıldı; yan yana sayfası yalnızca 32 px.
- Kabul sonrası dosyalar `Assets/Resources/UI/Icons/` altına da kopyalanacak (bu turda yapılmadı, yasaklı olmasa da iş dosyasında "kabul edilince" deniyor).
