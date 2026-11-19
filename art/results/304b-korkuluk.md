unity-import: pending

# 304b Korkuluk yeniden (rapor)

Yan yana sayfası: [304b-yanyana.png](304b-yanyana.png) (esya referansları + yeni korkuluk, eski 304 korkuluğu, köylü). Ekin evresi (371) sayfaya konmadı: araç yalnızca aile referanslarını ve verilen dosyaları dizer.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, 304 ile aynı: kılavuz + maske (`footprint_guide.py --w 80 --h 112 --n 1 --height 70`), `high top-down`, `low detail`, `basic shading`, `single color outline`. Açıklama: iş dosyasındaki klasik korkuluk metni + 301 ortak eki + `rough light pine wood, rope lashing`. 1 çağrı = 1 generation (ikinci deneme gerekmedi). `get_balance` iş başında 3723,5; iş sonrası ≈ 3722,5 (paralel şeritler de harcıyor).
Kılavuz ve maske `art/tmp304b/`'de (commit dışı).

## Dosya
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_item_scarecrow_se_v2.png | 80x112 | 40 | 24 |

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (48 renk) | geçti: 43 renk, yarı saydam 0, opak x12-68 y9-103, alt boşluk 8 |
| Maske dışı opak piksel | 0 |
| Stil denetimi (esya) | GEÇTİ: palete yakın %91, ort. uzaklık 8.3, doygunluk x1.18 |
| Okunurluk | Görsel: direk ve çapraz kol, çuval baş ve hasır şapka, yamalı gömlek; kollar yana açık, insanla karışmıyor. Kesin karar kullanıcıda |

## Sorunlar
- Direk dar olduğu için ayak izi kaplaması düşüktür (304'teki korkuluk gibi); ölçülmedi.
- Işık/ayna ölçümü yapılmadı; simetrik nesne, tek sprite her yöne.
