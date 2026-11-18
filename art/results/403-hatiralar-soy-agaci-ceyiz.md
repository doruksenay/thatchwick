unity-import: pending

# 403 Hatıralar, soy ağacı işlemesi, çeyiz sandığı (rapor): 5/7 teslim, 2 KALDI, review

Yan yana sayfası: [403-yanyana.png](403-yanyana.png) (7 yeni + 7 referans). Teslim edilmeyenler: `art/results/403-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (382 ve 380 yöntemi; betik `art/tmp403/`, commit edilmez). Masa üstü eşyaları 32x32 tuval, taban elması alt kenardan 4 px yukarıda (kupa/ayakkabı/teneke 16x8, kurdele 20x10), maske eşya yüksekliği + 1 px. Duvar eşyaları: "/" eğimli paralelkenar (duvar rengi 184,160,122), balık plaketi 40x48, soy ağacı 112x88 (3 kenar = 96 px yüz). Sandık: `footprint_guide.py --w 112 --h 80 --n 2 --axis x --height 28`. Masa üstü ve sandık `high top-down`, duvar `side`; hepsi `low detail`, `basic shading`, `single color outline`, tarif açıklaması + duvar eki + 301 ortak eki. 7 çağrı paralel, tek deneme, çağrı başı 1 gen = 7 generation (cost_max 16). Maliyet araç açıklamasında yazmıyor, 306/380/382 ile aynı varsayıldı; bakiye farkı 4170,90 -> 4161,90 (paralel şeritler de harcıyor).
get_balance sonrası: 4161,90.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | festival_cup | teslim | UYARI: palete uzak (ort. 12.2) |
| 2 | harvest_ribbon | TESLİM EDİLMEDİ | KALDI: palete yakın %68 (palet dışı renk, kırmızı). Doygunluk sorunu değil, `fit` uygulanmadı |
| 3 | baby_first_shoes | teslim | GEÇTİ (düz kahverengi tek parça; çift ayakkabı okunmuyor, göz kararı) |
| 4 | elder_tin_cup | teslim | UYARI: ailesinden soluk (x0.35), koyu gövde |
| 5 | first_fish_scale | teslim | GEÇTİ, plaket ve balık okunuyor |
| 6 | family_tree_embroidery | TESLİM EDİLMEDİ | KALDI: %55 palet, x1.59 doygun; şekil de tutmadı: keten zemin yok, yalnızca kırmızı dal ve mavi çizgiler |
| 7 | dowry_chest | teslim | GEÇTİ (%90), kırmızı-yeşil motifli boyalı sandık, demir bantlar |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_festival_cup_se.png | 32x32 | 16 | 8 |
| iso_item_baby_first_shoes_se.png | 32x32 | 16 | 8 |
| iso_item_elder_tin_cup_se.png | 32x32 | 16 | 8 |
| iso_wall_item_first_fish_scale_sw_ne.png | 40x48 | 20 | 24 (görsel merkezi) |
| iso_item_dowry_chest_2x1_se.png | 112x80 | 56 | 32 |

## Doğrulama (validate_iso --max-colors 48 hepsi geçti)
| Dosya | Opak kutu | Renk | Yarı saydam | Stil denetimi |
|---|---|---|---|---|
| festival_cup | x8-23 y5-26 | 7 | 0 | UYARI (%96, 12.2, x1.12) |
| harvest_ribbon (teslim değil) | x6-24 y11-21 | 5 | 0 | KALDI (%68, 12.9, x1.10) |
| baby_first_shoes | x8-23 y11-20 | 5 | 0 | GEÇTİ (%100, 4.8, x0.91) |
| elder_tin_cup | x9-22 y9-21 | 8 | 0 | UYARI (%100, 7.7, x0.35) |
| first_fish_scale | x4-35 y6-42 | 15 | 0 | GEÇTİ (%100, 7.3, x1.09) |
| family_tree (teslim değil) | x9-103 y6-78 | 25 | 0 | KALDI (%55, 23.6, x1.59) |
| dowry_chest | x13-101 y3-72 | 33 | 0 | GEÇTİ (%90, 8.9, x0.97) |

Işık sol-üst, gölge pişirilmemiş (gözle, ölçülmedi). Döşeme testi eşya için uygulanmadı.

## Sorunlar ve öneriler
- Soy ağacı işlemesi tutmadı. Öneri: kılavuzu keten renginde (açık bej) doldurup kenarlık/kasnaklı ayrı iş, ya da `create_image_pixflux` ile önden çizip kodda eğmek. Yeniden deneme yeni iş gerektirir (max_attempts 1).
- Hasat kurdelesi: kırmızı palet dışı; "muted faded red, brownish" ipucuyla yeniden denenebilir.
- Bebek ayakkabısı tek parça kahverengi lekeye yakın; tanınırlık yan yana sayfasından göz kararı kontrol edilmeli.
