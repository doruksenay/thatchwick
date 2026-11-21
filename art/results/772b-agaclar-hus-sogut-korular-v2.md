unity-import: pending

# 772b Huş, söğüt ve korular (rapor): KISMİ (2/5 teslim)

Yan yana: [772b-yanyana.png](772b-yanyana.png) (referanslar + 2 yeni). Masaüstü kopyası: yerel (bulutta yapılamaz). `772b-stil.png` yapılmadı (eski koru ve 772 çamıyla karşılaştırma yok).

## Araç ve ayarlar
`create_map_object` stil eşleme modu (kılavuz + maske, referans görsel verilemez; stil yalnızca metinden), `high top-down`, `single color outline`, `basic shading`, `medium detail`. Tuval 112x160, ayak izi elması (56,136), pivot (56, 24). Olgun maske 138 px, genç maske önce 80 sonra 62 px yukarı. Renk: `style.py fit --family bitki`.
Çağrılar: 9 `create_map_object` (5 ilk + 3 ikinci deneme + 1 genç söğüt ikinci denemesi), tahmini ≈ 9 gen. get_balance iş sonu: 9649 (paylaşılan hesap; diğer şeritler de harcıyor).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) | Boy (pivot üstü) |
|---|---|---|---|---|
| iso_tree_birch_grown_se_v2.png | 112x160 | 56 | 24 | 125 |
| iso_tree_willow_young_se_v2.png | 112x160 | 56 | 24 | 79 |

## Doğrulama
| Dosya | validate_iso | Renk | Stil | Ölçek |
|---|---|---|---|---|
| birch_grown | geçti | 40 | %99 / x1.15 GEÇTİ | TAMAM 125 |
| willow_young | geçti | 19 | %91 / x1.15 GEÇTİ | TAMAM 79 |

## TESLİM EDİLMEDİ (`art/results/772b-red/`)
- `iso_tree_birch_young_se_v2`: ölçek KALDI (97 px, genç 60-85). 2 deneme tükendi (1. deneme zemin karosu çıktı, 2. deneme temiz ama fazla uzun).
- `iso_tree_willow_grown_se_v2`: 2 denemede de gövde altında zemin/çim karosu pişti ("no ground" ihlali); maske dışı silme kuralı bunu kapsamaz. Ayrıca sarkan dallar belirgin değil.
- `iso_edge_trees_a_v2`: aynı zemin karosu sorunu (2 deneme).
- Genç söğüt (teslim edildi) sarkan dal göstermiyor, genel bir genç yaprak döken gibi; gözle bak.

## Sorular / öneri
- Araç stil görseli almadığı için zemin karosu eğilimi var (772'de de benzer). Referans görsel alan araçla (`create_image_pro_from_style` vb.) ya da kılavuzsuz, serbest tuvalde `create_object_pro_flash` ile kalan üçü 772c benzeri bir işte yeniden denenebilir. Genç huş için maske ~y50 (boy ≤ 80).
