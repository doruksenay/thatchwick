unity-import: pending

# 492a Ulu meşe (rapor): BAŞARISIZ, teslim yok

Yan yana sayfası: [492a-yanyana.png](492a-yanyana.png). Aday: `492a-red/iso_greattree_oak_summer_se.png`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, özel maske (160x224; 2x2 elmas ayak izi merkezi (80,188), pivot (80,36) alttan; gövde-taç bandı x 4-156, y 10-190 maskeli), `high top-down`, `low detail`, `basic shading`, `single color outline`. Açıklama tarifteki aynen. 1 çağrı = 1 generation (kılavuz/maske art/tmp'de). get_balance sonrası 869,6 (paralel şeritler de harcıyor).

## Sonuç
Yaz görseli üretildi ve teknik kapıları geçti, fakat içerik hatası var; ayrıca kalan üç mevsim bu araçla tarifteki gibi üretilemiyor. Tek deneme kuralı gereği `failed`.

| Dosya | Piksel | Pivot x | Pivot y (alttan) | Durum |
|---|---|---|---|---|
| iso_greattree_oak_summer_se.png | 160x224 | 80 | 36 | teslim edilmedi |

## Doğrulama
| Denetim | Sonuç |
|---|---|
| validate_iso (object) | geçti; 22 renk, yarı saydam 0, opak x 7-154 y 10-204 (boy 195, taç ~147 px) |
| style.py check --family bitki | GEÇTİ: palete %100, uzaklık 8.1, doygunluk x0.93 |
| Gölge/ışık | zemin gölgesi yok, ışık sol-üstten |

## Sorunlar
1. **Ağaç evi platformu çizilmiş:** açıklamadaki "boughs low enough to hold a treehouse platform" ifadesi modelin sağ dalda korkuluklu bir ahşap platform ve kulübe çizmesine yol açtı. Platform 493 katmanının işi; ulu ağaçta olmamalı, kış/güz hallerinde de çıplak ağaçta olmamalı.
2. **Mevsim varyantları:** tarif "ilk yaz görseli `init_image`" diyor, `create_map_object`'in `init_image` parametresi yok (yalnızca `background_image` + maske). Gövde piksel piksel aynı kalan dört mevsim bu araçla garanti edilemez; seçenek: `edit_image_pro_flash` ile yazdan renk/yaprak düzenlemesi (ek maliyet, kullanıcı onayı gerek).
3. Ayrıca gövde tabanı ayak izi elmasına göre biraz sağda/yukarıda; alta boşluk 19 satır (hedef 8-16).

## Öneri (kullanıcı kararı)
Açıklamadan treehouse ifadesini çıkarıp ("a few broad horizontal boughs") yeni deneme; mevsimler için `edit_image_pro_flash` yaklaşımı. Maliyet ~6 gen, cost_max 10 içinde.
