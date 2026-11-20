unity-import: pending

# 618k Kaya sırtı ve kenar dekoru (bulut şeridi 3, 2026-11-20)

Araç: `create_map_object` (MCP), stil eşleme: `tools/art/footprint_guide.py` kılavuzu + özel maske, `high top-down`, `basic shading`, `low detail`, `single color outline` (357 yöntemi). 10 çağrı, 10 ürün, yeniden deneme yok (base64 aktarım hatası alan 3 istek üretime girmedi, 0 gen).
Harcama: tahmini **~10-12 gen** (10 çağrı; `cost_est` 12). Hesap paylaşımlı olduğundan `get_balance` farkı (9943 -> 9872) başka şeritleri de içerir; kalan: **9872**.
Teslim: 6/10. Yan yana: [618k-yanyana.png](618k-yanyana.png). Reddedilenler: `art/results/618k-red/`. Masaüstü kopyası: yerel.

Sapma: kök yığını ve kütük için 80x64 kılavuz base64 aktarımında bozulduğundan, 80x72 kılavuzla (boulder_cluster ile aynı) üretildi; üstteki 8 boş satır silinip 80x64'e inildi (içerik kesilmedi).

## Stil denetimi ve sonuç
| Parça | Boyut | Stil | Sonuç | Not |
|---|---|---|---|---|
| ridge_straight_2x1_se_nw | 112x96 | GEÇTİ (%85, x0.88) | TESLİM | |
| ridge_corner_se | 80x96 | KALDI (palet dışı, %79) | red | dik prizma gibi, köşe okunmuyor |
| ridge_end_se | 80x88 | GEÇTİ (%94) | TESLİM | |
| ridge_tall_straight_2x1_se_nw | 112x128 | GEÇTİ (%100) | TESLİM | tabanda çim elması var |
| ridge_tall_corner_se | 80x128 | GEÇTİ (%91) | TESLİM | |
| boulder_cluster_se | 80x72 | KALDI (palet dışı, %67) | red | şekil tanınıyor, sorun doygunluk değil (x1.08), `fit` uygulanmadı |
| boulder_flat_se | 80x56 | UYARI (ailesinden soluk x0.34) | TESLİM | |
| boulder_mossy_se | 80x72 | ham KALDI (x1.38); `fit` sonrası UYARI (x1.16) | TESLİM (düzeltilmiş, doygunluk x0.83) | ham hâl red klasöründe |
| root_mound_se | 80x64 | KALDI (palet, doygun x1.29) | red | iki ekstra ağaç, kök yığını okunmuyor |
| fallen_log_short_se | 80x64 | KALDI (palet, doygun x1.38) | red | dik kütük bloğu, yatan kütük değil |

## Teslim (Assets/Art/Iso/_aday/)
| Dosya | Piksel | Pivot (x, y ALTTAN) |
|---|---|---|
| iso_terrain_ridge_straight_2x1_se_nw.png | 112x96 | 56, 32 |
| iso_terrain_ridge_end_se.png | 80x88 | 40, 24 |
| iso_terrain_ridge_tall_straight_2x1_se_nw.png | 112x128 | 56, 32 |
| iso_terrain_ridge_tall_corner_se.png | 80x128 | 40, 24 |
| iso_terrain_boulder_flat_se.png | 80x56 | 40, 24 |
| iso_terrain_boulder_mossy_se.png | 80x72 | 40, 24 |

`validate_iso.py`: hepsi geçti (yarı saydam 0, alt boşluk 7-17). Tam 2:1 kırpma gerekmedi.

## Sorunlar
- Alçak sırt köşesi reddedildi: alçak sırt seti (düz, uç) var, köşe yok. Kesintisiz sırt birleşme testi köşe eksik olduğundan yapılamadı.
- Kök yığını, yatan kütük ve üçlü kaya kümesi yeniden üretilmeli (yeni iş). Öneri: kök ve kütükte "weathered grey limestone" son eki kullanılmamalı; 80x64 kılavuz için MCP'de dosya yolu yok, base64 aktarımı uzun dizgilerde bozuluyor (batch_worker/REST yolu daha güvenli).
- Işık ve tanınırlık göz kararı: yan yana sayfasında kullanıcı karar verir.
