unity-import: pending

# 319 Meyve ağaçları (rapor)

Yan yana sayfası: [319-yanyana.png](319-yanyana.png) (elma 3 hal + erik olgun, bitki ailesi referanslarıyla). Teslim edilmeyen 11 görsel: `art/results/319-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`tools/art/footprint_guide.py --w 96 --h 112 --n 1`, yükseklik 85 olgun/meyveli, 44 genç; ayak izi x 8-71, y 72-103, pivot (40, 24 alttan)), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. Araçta `seed` yok.
Çağrılar: 12 × 1 gen = 12 generation (cost_max 20 içinde). 5 çağrı bozuk base64 aktarımıyla üretime girmeden reddedildi (ücret yok), yeniden kodlanıp gönderildi. İkinci deneme yapılmadı: tarifteki ikinci deneme koşulları (ayak izi, boş çıktı, tanınmama) oluşmadı; red nedeni stil denetimi.
`get_balance` iş sonu: 931,70 (paralel şeritler de harcıyor; bu işin payı 12).

## Sonuç özeti
| Ağaç | Hal | Durum | Not |
|---|---|---|---|
| elma | genç / olgun / meyveli | TESLİM EDİLMEDİ | stil denetimi KALDI (doygunluk x1.96 / x2.40 / x2.00); genç 49 renk > 48 |
| armut | genç / olgun / meyveli | TESLİM EDİLMEDİ | stil KALDI (x1.66 / x1.77 / x2.57); genç 51 renk > 48 |
| kiraz | genç / olgun / meyveli | TESLİM EDİLMEDİ | stil KALDI (x1.54 / x1.99 / x2.48); meyveli 56 renk > 48 |
| erik | genç | TESLİM EDİLMEDİ | stil KALDI (palete %79) |
| erik | olgun | teslim (UYARI) | palete %100, doygunluk x0.49 (soluk) |
| erik | meyveli | TESLİM EDİLMEDİ | stil KALDI (palete %77, x0.63) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_tree_plum_grown_se.png | 96x112 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48 --footprint 8,72,64,32`)
| Dosya | Opak sınır kutusu | Boy | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|---|
| apple_young | x23-71, y28-96 | 69 | 49 | 0 | yok | renk ⚠ |
| apple_grown | x8-70, y12-95 | 84 | 23 | 0 | yok | geçti |
| apple_fruit | x11-71, y9-97 | 89 | 33 | 0 | yok | geçti |
| pear_young | x27-67, y31-86 | 56 | 51 | 0 | yok | renk ⚠ |
| pear_grown | x19-71, y14-97 | 84 | 37 | 0 | yok | geçti |
| pear_fruit | x12-70, y13-98 | 86 | 35 | 0 | yok | geçti |
| cherry_young | x25-70, y32-84 | 53 | 18 | 0 | yok | geçti |
| cherry_grown | x10-71, y8-96 | 89 | 27 | 0 | yok | geçti |
| cherry_fruit | x10-70, y13-90 | 78 | 56 | 0 | yok | renk ⚠ |
| plum_young | x19-61, y32-83 | 52 | 29 | 0 | yok | geçti |
| plum_grown | x20-70, y14-94 | 81 | 24 | 0 | yok | geçti |
| plum_fruit | x12-68, y20-95 | 76 | 29 | 0 | yok | geçti |

Tam 2:1 çıktı sorunu yok, kırpma yapılmadı. Genç ağaç boyları tarifteki 40-48'in üstünde (52-69, ⚠). Olgun ↔ meyveli farkının yalnızca meyve noktaları olması sağlanamadı: her hal ayrı üretim, taç şekli hallere göre değişiyor. Döşeme testi ağaç için gerekmez. Maske dışı piksel ve kılavuz rengi ayrıca ölçülmedi.

## Stil denetimi (`style.py check --family bitki`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| apple_fruit | %35 | 35.2 | x2.00 | KALDI |
| apple_grown | %21 | 44.4 | x2.40 | KALDI |
| apple_young | %36 | 37.1 | x1.96 | KALDI |
| cherry_fruit | %21 | 41.7 | x2.48 | KALDI |
| cherry_grown | %39 | 25.0 | x1.99 | KALDI |
| cherry_young | %55 | 16.5 | x1.54 | KALDI |
| pear_fruit | %22 | 40.3 | x2.57 | KALDI |
| pear_grown | %27 | 34.5 | x1.77 | KALDI |
| pear_young | %16 | 33.6 | x1.66 | KALDI |
| plum_fruit | %77 | 15.0 | x0.63 | KALDI |
| plum_grown | %100 | 10.2 | x0.49 | UYARI |
| plum_young | %79 | 11.3 | x1.07 | KALDI |

## Sorunlar
- Çıktıların çoğu bitki ailesi paletinden çok doygun (yaprak yeşili parlak); sonraki denemede açıklamaya "muted desaturated green" ve referans kılavuzu eklenmeli. Yeni iş ya da karar kullanıcıda.
- Meyveli/olgun piksel piksel aynı olmalı koşulu için tek bir olgun görselden `inpaint_image_pro_flash` ile meyve eklemek gerekir; bu işte yapılmadı.
- Teslim edilmeyen PNG'ler `319-red/` altında (görsel karar için).
