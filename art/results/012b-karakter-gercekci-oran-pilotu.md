unity-import: pending

# 012b Gerçekçi oranlı karakter pilotu

Durum: review. 4/4 aday teslim edildi (`Assets/Art/Iso/_aday/`), ancak **oran hedefi tutmadı** (aşağıda).
Yan yana (eski 56 px köylü, 4 aday x4, 020 portreleri, 008b pencere duvarı + aday x2): [012b-yanyana.png](012b-yanyana.png). Stil sayfası: [012b-referans.png](012b-referans.png).

## Araç ve ayarlar
- `create_character` v3, humanoid, `high top-down`, siyah tek renk dış çizgi, high detail; tarifteki oran ve renk ekleri aynen. Boylar 72 ve 84, 2 gen x 4 = 8 gen. Bu araç stil referansı almadığı için 020 referansı kullanılmadı. 8 yön üretildi, yalnızca SE teslim edildi.
- `reduce_colors` 32 renk, dithering none, SE başına 1 çağrı x4 = 0,4 gen.
- man_r84: ham hâli "ailesinden çok doygun" (x1.60) yüzünden KALDI; `style.py fit` ile doygunluk x0.72'ye indirildi, yeniden denetimde GEÇTİ (x1.18). Ham: `012b-red/`.
- Toplam harcanan: 8,4 gen. `get_balance` sonrası: 4928,6 (paralel şeritlerin harcamaları dahil).

## Dosyalar (pivot x merkez, y alttan)
| Dosya | Tuval | Opak bbox (genişlik x boy) | Pivot x, y | Renk | Yarı saydam |
|---|---|---|---|---|---|
| _aday/iso_char_man_r72_cand_se.png | 72x72 | 37 x 65 | 36, 3 | 32 | 0 |
| _aday/iso_char_man_r84_cand_se.png | 84x84 | 38 x 78 | 42, 2 | ≤32 | 0 |
| _aday/iso_char_woman_r72_cand_se.png | 72x72 | 29 x 68 | 36, 2 | 32 | 0 |
| _aday/iso_char_woman_r84_cand_se.png | 84x84 | 34 x 80 | 42, 2 | 32 | 0 |

## Oran (kafa, boyun daralmasından kabaca ölçüldü)
| Aday | Boy | Kafa (yakl.) | Boy / kafa | Hedef 6,5-7,5 |
|---|---|---|---|---|
| man_r72 | 65 | 17 | ~3,8 | tutmadı |
| man_r84 | 78 | 22 | ~3,5 | tutmadı |
| woman_r72 | 68 | 17 | ~4,0 | tutmadı |
| woman_r84 | 80 | 20 | ~4,0 | tutmadı |

Sorun: "7 heads tall, not chibi" metnine rağmen v3 hâlâ büyük kafalı, kısa bacaklı çiziyor. Boy büyütmek oranı düzeltmiyor, yalnızca ayrıntıyı artırıyor. Öneri (kullanıcı kararı, ayrı iş): `standard` modda `proportions: realistic_male/realistic_female`, ya da `pro` modda 020 portresini `concept_image` vererek denemek.

## Doğrulama
| Aday | validate_iso | Stil denetimi | Doygunluk |
|---|---|---|---|
| man_r72 | geçti | GEÇTİ (%97) | x0.83 |
| man_r84 (fit) | geçti | GEÇTİ (%100) | x1.18 (ham x1.60 KALDI) |
| woman_r72 | geçti | GEÇTİ (%95) | x1.03 |
| woman_r84 | geçti | GEÇTİ (%99) | x0.94 |

Karakter ailesi kilidi kurulmadı; palet uyumu uyarı niteliğinde. Ayak izi/döşeme testi karakter için geçerli değil.

## Notlar
- Masaüstü kopyası: yerel.
- Karakter id'leri (8 yönlü, PixelLab'de): man_r72 9b9e509f-21b3-4503-af89-0e73d67f28bb, man_r84 c63fe39d-b168-4732-bf11-b476172c98e8, woman_r72 8efa8dd1-1826-4373-a764-8ea8f7fec64b, woman_r84 135cb0b1-bd46-4514-8c87-0834b96ef80d.
- Ham dosyalar `_aday/_ham/iso_char_*_raw_se.png` (diğer yönler indirilmedi).
