unity-import: pending

# 395b Üçüncü tur: çatkı, kandil, kömür yığını (rapor)

Sonuç: **hiçbir parça teslim edilmedi.** Yan yana sayfası: [395b-yanyana.png](395b-yanyana.png). Aday dosyalar: `art/results/395b-red/`. Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
- Çatkı: `style.py fit` (zaten x1.12, değişmedi) + `inpaint_image_pro_flash` (80x96, çapraz kiriş şeridi maskesi, `straight sound pine beam, fresh light brown wood`, `no_background`). 1 çağrı, 5 generation. `get_balance` sonrası: 4290,90.
- Kömür yığını: 396-red ham aday yerel 32/24/16 renge indirildi + `fit` (0 gen).
- Kandil: yapılmadı.

## Sonuç
| Parça | Durum | Neden |
|---|---|---|
| ahşap çatkı | TESLİM EDİLMEDİ | kiriş düz ve açık renk oldu; stil (%97, x1.11) ve validate geçti. Ama çürüyen çatkıdaki çapraz kafes ve koyu bordo dikmeler duruyor: "tam iki dikme + bir kiriş" ölçütü karşılanmadı, dikmeler taze ahşap değil |
| kandil | YAPILMADI | tarifteki `create_map_object` `color_image` parametresi araç şemasında yok (yalnızca `background_image` ve maske); tarif olduğu gibi uygulanamadığı için çağrılmadı |
| kömür yığını | TESLİM EDİLMEDİ | 396 ham adayı renk indirme + `fit` ile %68 palete yakın, KALDI (32, 24, 16 renkte de %68) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| çatkı c | %97 | 6.7 | x1.11 | GEÇTİ (şekil KALDI) |
| kömür yığını fit (32) | %68 | 12.7 | x0.99 | KALDI |

## Sorular
- Kandil: `color_image` yok. Mum görselini `background_image` yapıp maskeli üretmek mi istenir?
- Çatkı: dikmeleri de açık ahşap yapıp kafesi silen ikinci bir inpaint (5 gen) ya da yeni 1 yönlü üretim önerilir.
- Kömür yığını: kavurma yığınını `background_image` alan stil eşlemeli üretim önerilir.
