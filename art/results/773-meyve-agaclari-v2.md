unity-import: pending

# 773 Meyve ağaçları (_v2), KISMİ teslim (5 / 12)

Yan yana: [773-yanyana.png](773-yanyana.png). Masaüstü kopyası: yerel. Stil sayfası `773-stil.png` yapılmadı (yan yana sayfası olgun çapaları içeriyor).

## Araç ve ayarlar
772 yöntemi: `create_map_object` stil eşleme, özel maske, `high top-down`, `single color outline`, `basic shading`, `medium detail`, tuval 96x136, taban elması (48,112) merkezli. Olgun maske: elmas + dikdörtgen (x4-90, y14-112); genç maske: elmas + (x22-72, y50-112). Renk: `style.py fit --family bitki` (ham x0.39-1.0 → aile). Araç stil görseli almadığı için stil yalnızca metinden geldi (772 ile aynı sınır).
Çağrılar: 12 `create_map_object` (olgun 4 + armut yeniden 1 [ilk denemede armutlar taçta meyve olarak çıktı] + genç 4 + genç yeniden 3 [armut, kiraz, zeytin]). Tahmini harcama ≈ 12-16 gen (hesap paylaşımlı). `get_balance` sonrası: 9694 / 10000.
Meyveli (`_fruit`) 4 düzenleme YAPILMADI: `edit_image_pro_flash` 136 px kaynakta tanesi 6 gen (24 gen); tarifin tahmini 12-16'nın üstünde ve `cost_max` 28'i aşar. Çağrılmadı.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) | Boy |
|---|---|---|---|---|
| iso_tree_pear_grown_se_v2.png | 96x136 | 48 | 24 | 96 |
| iso_tree_cherry_grown_se_v2.png | 96x136 | 48 | 24 | 97 |
| iso_tree_plum_grown_se_v2.png | 96x136 | 48 | 24 | 95 |
| iso_tree_plum_young_se_v2.png | 96x136 | 48 | 24 | 58 |
| iso_plant_olive_grown_se_v2.png | 96x136 | 48 | 24 | 96 |

## Doğrulama
| Dosya | validate_iso | Renk | Stil (palete / doygunluk) | Ölçek |
|---|---|---|---|---|
| pear_grown | geçti | 43 | %100 / x1.15 GEÇTİ | TAMAM |
| cherry_grown | geçti | 61 | %89 / x1.15 GEÇTİ | TAMAM |
| plum_grown | geçti | 49 | %100 / x1.15 GEÇTİ | TAMAM |
| plum_young | geçti | 45 | %96 / x1.06 GEÇTİ | TAMAM |
| olive_grown | geçti | 20 | %100 / x1.15 GEÇTİ | `scale_check` tabloda `iso_plant_*` "çalı" sınıfında (35-70) KALDI görünür; iş tarifi ağaç boyu (95-115) ister, 96 px uyar. Tablo düzeltmesi önerisi aşağıda |

Notlar: armut/kiraz/erik/zeytin olgunları çapa stilinde (kabarık kümeli taç). Kirazın tacında sarımsı vurgu lekeleri var, meyve gibi okunabilir; gözle kontrol gerekir. Genç erikte kırmızı noktalar (meyve gibi) var; genç evre için istenmeyen olabilir.

## Teslim EDİLMEDİ (`art/results/773-red/`)
- `iso_tree_pear_young`, `iso_tree_cherry_young`: iki denemede de gövdesiz alçak çalı (≈35-45 px) çıktı, ölçek ve biçim KALDI. İkinci deneme tükendi.
- `iso_plant_olive_young`: ikinci denemede dallı yapı ama fit sonrası stil KALDI (palete yakın %73, palet dışı renkler, turkuaz tonlu yapraklar).
- Meyveli 4 görsel (yukarıda neden).

## Sorular
1. Meyveli düzenlemeler (4 x 6 gen = 24) için onay var mı? `cost_max` 28 yetmiyor; 773b olarak yazılabilir.
2. Genç armut/kiraz/zeytin: stil görseli alan araç ya da ağaç-gövde kılavuzlu (gövde çizili) maske gerekli; öneri 772c ile birlikte ele alınsın. Genç eriğin maske başarısı (x22-72, y50-112) tekrar denenebilir, ama aynı maske armut ve kirazda çalışmadı.
3. `scale_check.py` tablosu `iso_plant_olive_*` dosyalarını ağaç sınıfına almalı (araç değişikliği bu işçide yasak, öneri).
