unity-import: pending

# 772 Ağaç stil ve ölçek pilotu (rapor)

Yan yana: [772-yanyana.png](772-yanyana.png) · Stil sayfası: [772-stil.png](772-stil.png) (6 yeni + çapalar olgun meşe ve genç çam). Eski stil kenar karşılaştırması ve ölçek sayfası (insan figürü) yapılmadı.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object` stil eşleme modu (yalnızca kılavuz+maske; referans görsel verilemez, stil açıklama metninden geldi), `high top-down`, `single color outline`, `basic shading`, `medium detail`. Tuval yaban 112x160 / meyve 96x136, ayak izi elması yatayda ortada (pivot 56,24 / 48,24). Genç meşe, genç çam ve genç elma için ilk denemede ağaç olgun boyda çıktığı için maske yukarıdan kısıldı (2. deneme). Meyveli elma: `edit_image_pro_flash` (6 gen), olgun elmadan. Renk: `style.py fit --family bitki` hepsinde.
Çağrılar: 9 create_map_object (7 ilk + 3 yeniden - 1 yok sayılan = ilk elma genç atıldı) + 1 edit (6 gen); tahmini harcama ≈ 15-16 gen (paylaşılan hesapta paralel şeritler de harcıyor; get_balance iş sonu: 9872).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) | Boy (pivot üstü) |
|---|---|---|---|---|
| iso_tree_oak_grown_se_v2.png | 112x160 | 56 | 24 | 128 |
| iso_tree_pine_grown_se_v2.png | 112x160 | 56 | 24 | 131 |
| iso_tree_pine_young_se_v2.png | 112x160 | 56 | 24 | 78 |
| iso_tree_apple_grown_se_v2.png | 96x136 | 48 | 24 | 97 |
| iso_tree_apple_young_se_v2.png | 96x136 | 48 | 24 | 62 |
| iso_tree_apple_fruit_se_v2.png | 96x136 | 48 | 24 | 97 |

TESLİM EDİLMEDİ: `iso_tree_oak_young_se_v2.png` (`art/results/772-red/`): ölçek denetimi KALDI, 56 px (genç yaban 60-85). 2. deneme de tükendi (maksimum 2). Ham çıktıda gövde tabanı yerden 22 px yukarıdaydı, görsel aşağı 22 px kaydırıldı (yalnızca öteleme); yine de boy yetmedi. Çam olgun gövde tabanı pivotun ~18 px altına sarkıyor (kök yayılımı dahil), yerinde bırakıldı.

## Doğrulama
| Dosya | validate_iso | Renk | Stil (palete / doygunluk) | Ölçek |
|---|---|---|---|---|
| oak_grown | geçti | 29 | %100 / x1.16 GEÇTİ | TAMAM |
| pine_grown | geçti | 42 | %100 / x1.14 GEÇTİ | TAMAM |
| pine_young | geçti | 22 | %100 / x1.15 GEÇTİ | TAMAM |
| apple_grown | geçti | 41 | %100 / x1.15 GEÇTİ | TAMAM |
| apple_young | geçti | 35 | %100 / x1.15 GEÇTİ | TAMAM |
| apple_fruit | geçti | 43 | %100 / x1.15 GEÇTİ | TAMAM (olgunla aynı boy) |

Doygunluk ham çıktıda x0.42-0.90 (soluk), `fit` ile aileye çıkarıldı. Meyveli elmanın tacı olgun elmadan hafif farklılaşabilir (edit); gözle kontrol gerekir.

## Sorular / öneri
- Genç meşe için üçüncü deneme isteniyor mu (maske üstü ~y40)? Şimdilik 772b genç ağaçları için maske üst sınırı ve gövde tabanını sabitleyen kılavuz önerilir.
