unity-import: pending

# 404 Asil ve Tüccar kademe eşyaları (rapor): 4/10 teslim, 6 stil denetiminde KALDI, review

Yan yana sayfası: [404-yanyana.png](404-yanyana.png) (sırayla: canopy_bed, carved_chest, rug_crest, rug_kufic, rug_knotted, canopy_bed_curtained, throne, tapestry_hunt, tapestry_garden, dynasty_banner). Teslim edilmeyenler: `art/results/404-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object` stil eşleme, özel maske, `low detail`, `basic shading`, `single color outline`; zeminde/mobilyada `high top-down`, duvar eşyasında `side` (306/380 ile aynı). Zemin ve mobilya kılavuzu `tools/art/footprint_guide.py` (halı yükseklik 2 px, yatak 64, taht 60, sandık 28), duvar kılavuzu "/" eğimli paralelkenar (duvar rengi 184,160,122; betik `art/tmp404/`, commit edilmez). Açıklama + 301 ortak eki (halı/kumaşta "dark brown wood" yok, "muted faded wool colors" eklendi).
- 10 çağrı paralel, çağrı başına 1 gen = 10 generation (cost_max 24). Tek deneme, yeniden deneme yok. get_balance: iş başı 4161,9 (şerit başlangıcı), iş sonu 4138,9 (paralel şeritler de harcıyor).
- Tuvaller: goblen 3 kenar 128x96, sancak 40x64, halı 3x3 208x112, halı 4x3 240x128, yatak 144x144, taht 80x104, sandık 112x80.

## Dosyalar (teslim; pivot ALTTAN)
| Yol (`Assets/Art/Iso/_aday/`) | Piksel | Pivot x | Pivot y |
|---|---|---|---|
| iso_item_canopy_bed_2x2_se.png | 144x144 | 72 | 40 |
| iso_item_carved_chest_2x1_se.png | 112x80 | 56 | 32 |
| iso_item_rug_crest_3x3_se.png | 208x112 | 104 | 56 |
| iso_item_rug_kufic_4x3_se.png | 240x128 | 120 | 64 |

## Doğrulama
| Eşya | validate_iso (en çok 48 renk) | Renk | Stil denetimi | Durum |
|---|---|---|---|---|
| canopy_bed | geçti | 35 | UYARI (%100, uzaklık 10,5, x1,18) | teslim |
| carved_chest | geçti | 37 | GEÇTİ (%100, x0,70) | teslim |
| rug_crest | geçti | 41 | GEÇTİ (%100, x0,98) | teslim |
| rug_kufic | geçti | 34 | GEÇTİ (%94, x0,82) | teslim |
| rug_knotted | geçti | 44 | KALDI (%29 palet dışı, uzaklık 23,0) | `404-red/` |
| canopy_bed_curtained | geçti | 45 | KALDI (%60, x1,30) | `404-red/` |
| throne_chair | BAŞARISIZ (50 renk > 48) | 50 | KALDI (%54, x1,72) | `404-red/` |
| tapestry_hunt_3w | geçti | 37 | KALDI (%76, uzaklık 14,8) | `404-red/` |
| tapestry_garden_3w | BAŞARISIZ (49 renk > 48) | 49 | KALDI (%63, uzaklık 20,4) | `404-red/` |
| dynasty_banner | geçti | 32 | KALDI (%60, x1,69) | `404-red/` |

Hepsi şekil olarak tanınıyor (yan yana sayfası): halılar düz ve ayak izini dolduruyor, yataklar dört direkli, goblenler uzun şerit. Kırmızı doygun kumaşlar (perde, sancak, taht) ve renkli desenler esya paletinin dışında kaldı. `esya` ailesi kilitli olduğu için `fit` uygulanmadı (şekil sorunu da yok; kullanıcı red klasörüne bakıp elle kabul edebilir).

## Sorunlar
- Teslim edilmeyen 6 eşya için yeni tarif (daha soluk/az doygun renk, "faded" vurgusu) kullanıcı kararı; max_attempts 1.
- Halılarda döşeme testi yapılmadı (zemin karosu değil; halı yere serilen nesne).
- Işık/gölge: kum rengi yatak üst yüzü ve halılar gölgesiz; ışık sol-üst ölçülmedi, sayfaya bakın.
- Kanopili yatağın örtüsü (üst) mavimsi-petrol: Tüccar boyalı ahşap tarifiyle uyumlu, kullanıcı göz kararı.
