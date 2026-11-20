unity-import: pending

# 104t İnek, keçi, öküz yeniden, ateş v2, örme çit (kısmen teslim)

Sonuç: `status: review`. Teslim: goat_5, goat_6, ox_v2 (8 yön), örme çit (`fit` ile). Teslim edilmeyen: cow_5 ve cow_6 (ayak altında zemin lekesi), bekçi ateşi v2 (yanık çıktı). Masaüstü kopyası: yerel (bulut modu).
Yan yana sayfası (referans koyun/domuz + goat_5, goat_6, ox_v2 SE, cow_6 SE reddedilen): `art/results/104t-yanyana.png`. Reddedilenler: `art/results/104t-red/`.

## Araç ve ayarlar
- Hayvanlar: `create_image_pixflux` (south, high top-down, siyah dış hat, medium detail, basic shading, palet = koyun 1 SE, açıklama eki + `desaturated`), tuvaller inek 60, keçi 46, öküz 68 (öküzün inekten iri olması için 375'teki 48 yerine 68; tahmin). Sonra `create_character` v3 + referans, quadruped, şablon horse (inek, öküz) / dog (keçi), 1 gen/aday. `style.py fit` gerekmedi (KALDI yok).
- Ateş ve çit: `create_map_object` stil eşleme, `footprint_guide.py` (80x64 yük. 20; 80x72 yük. 24) + maske, low detail, basic shading. Çit: `style.py fit --family esya` (x0,73) uygulandı.
- Hız sınırı hatası olmadı.

## Generation
pixflux 5 + v3 5 + map_object 2 = 12. `get_balance` sonrası: 2664,9 (paralel şeritler dahil).

## Tek kare ve sonuç
| Aday | Zemin lekesi | Karar |
|---|---|---|
| cow_5 | var (açık gri elips, ayak altı; 8 yönde de) | teslim edilmedi (kural: maskeyle silme yok) |
| cow_6 | var (aynı); ala desen iyi okunuyor | teslim edilmedi |
| goat_5 | yok | teslim |
| goat_6 | yok | teslim |
| ox_v2 | yok | teslim |

## Dosyalar (`Assets/Art/Iso/_aday/`)
| Dosya | Piksel | SE boy x en | Renk (SE) | Yarı saydam | Pivot x (SE) / y alttan | Stil denetimi |
|---|---|---|---|---|---|---|
| iso_animal_goat_5_cand_{8 yön} | 48x48 | 41 x 34 | 16 (9-17) | 0 | 23,5 / 4 | 8 UYARI (soluk x0,23-0,35) |
| iso_animal_goat_6_cand_{8 yön} | 48x48 | 42 x 31 | 11 (9-12) | 0 | 25,0 / 3 | 8 UYARI (soluk x0,45-0,51) |
| iso_animal_ox_v2_cand_{8 yön} | 68x68 | 62 x 52 | 16 (9-16) | 0 | 35,0 / 2 | 8 UYARI (soluk x0,31-0,36) |
| iso_item_wattle_hurdle_se.png | 80x72 | 36 x 46 | 35 | 0 | 40 / 24 (kılavuz) | UYARI doygun x1,28 (fit sonrası) |

`validate_iso.py` SE: hepsi geçti. Aile kilidi kurulmadı (KALDI yok, yalnız UYARI). Pivot y yönler arası: keçi 3-4, öküz 2-3. Gölge pişirilmemiş, ışık sol-üstten.

## Sorunlar
- **Boy hedefi:** keçi SE boyu 41-42 px (hedef ~36, +15%); öküz 62 px (inek hedefi 48; öküz > inek şartı sağlanıyor). Tuval doluluk: keçi 0,85-0,88, öküz 0,91.
- **Öküz:** boyunduruk (ahşap tahta) görünmüyor; boynuzlar uzun ve öne-yana kıvrık, omuzlar kalın. Renk 407'ye göre kısmen düzeldi ama **hâlâ gri-kahve ve soluk** (palet zorlaması koyun gri paletinden). Kullanıcı göz kontrolü gerekli. Öneri: paletsiz (ya da sıcak kahve palet şeridi) yeniden deneme.
- **Keçi:** goat_5 soluk gri-beyaz; goat_6 siyah, iyi okunuyor, geriye kıvrık boynuz tanınıyor. Koyun 4 / domuz 2 ile aynı satırda ölçek ailesine uyuyor.
- **İnek:** her iki adayda da palet zorlaması açık gri zemin elipsi çizdi (375'teki köpek sorunuyla aynı). Öneri: palet zorlamasız ya da elips maskeli yeniden deneme; cow_6 deseni (siyah-beyaz ala) iyi, ham hâli `104t-red/` içinde.
- **Bekçi ateşi v2:** `unlit ... no flames` denmesine rağmen alevli çıktı (`104t-red/fire.png`); sönük hâl olmadığı için teslim edilmedi. Öneri: `iso_item_bonfire_lit_se` mevcut olduğundan sönük hâl için ayrı odun-halka tarifi ya da edit_image ile alev silme.
- **Örme çit:** ahşap kazıklı, yatay örgülü panel olarak tanınıyor; ham hâli aşırı doygun (x1,57), `fit` ile indirildi, ham `104t-red/hurdle.png`. 1x1 elmas ayak izinde değil, panel kılavuz elmas içinde ortada duruyor.
- Stil kapısı kuralı (bir yön KALDI): KALDI veren yön olmadı.
