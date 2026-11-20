unity-import: pending

# 473a Cam bahçe canlıları A (8)

- Araç: `create_image_pixen`, `no_background=true`, `view=side`, `direction=east`. **Tariften sapma:** tarif pixflux diyor ama pixflux alan tabanı 32x32 (16x16 reddedilir); araç açıklaması küçük spritelar için pixen öneriyor, maliyet aynı (1 gen). 8 çağrı paralel, 8 generation, yeniden deneme yok.
- Kalan (`get_balance`): 3011,9 (diğer şeritlerin harcaması dahil).
- Yan yana: [473a-yanyana.png](473a-yanyana.png). Masaüstü kopyası: yerel.
- 471 nemli zemini üzerinde 2x doğrulama sayfası üretilmedi; yalnızca stil yan yana sayfası var.

## Dosyalar (pivot alttan ortada: x = genişlik/2, y = 0)
| Dosya | Boyut | Pivot | Durum |
|---|---|---|---|
| viv_aphid.png | 16x16 | 8,0 | teslim (UYARI: palete uzak) |
| viv_snail.png | 16x16 | 8,0 | teslim (UYARI: palete uzak) |
| viv_woodlouse.png | 16x16 | 8,0 | teslim (UYARI: ailesinden soluk, x0.48) |
| viv_butterfly.png | 24x24 | 12,0 | teslim edilmedi (KALDI) |
| viv_caterpillar.png | 16x16 | 8,0 | teslim edilmedi (KALDI) |
| viv_firefly.png | 16x16 | 8,0 | teslim edilmedi (KALDI) |
| viv_ladybird.png | 16x16 | 8,0 | teslim edilmedi (KALDI) |
| viv_worm.png | 16x16 | 8,0 | teslim edilmedi (KALDI) |

Teslim: `Assets/Art/Iso/_aday/`. Reddedilenler: `art/results/473a-red/`.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Doygunluk | Sonuç |
|---|---|---|---|
| aphid | %91 | x0.95 | UYARI |
| snail | %100 | x0.77 | UYARI |
| woodlouse | %100 | x0.48 | UYARI |
| butterfly | %63 | x1.64 | KALDI (palet dışı, çok doygun) |
| caterpillar | %53 | x0.90 | KALDI (palet dışı) |
| firefly | %78 | x0.89 | KALDI (palet dışı) |
| ladybird | %67 | x1.03 | KALDI (palet dışı) |
| worm | %64 | x1.06 | KALDI (palet dışı) |

`style.py fit` 5 KALDI dosyaya denendi: kelebek doygunluğu x0.70'e indi ama hepsi yine "palet dışı renkler" ile KALDI kaldı; düzeltilmiş hâller teslim edilmedi (`fit` palet sorununu çözmüyor).

`validate_iso.py --kind object`: 8 dosyanın hepsi geçti (ayak alt kenara 1-4 satır boşluk).

## Sorular
- 5 canlı palet denetimini geçemedi. Doğal renkli küçük canlılar (kırmızı uğur böceği, pembe solucan, yeşil tırtıl, turuncu kelebek, sarı ateş böceği) için `esya` paletinde istisna ya da ayrı bir canlı paleti gerekebilir; kullanıcı kararı. Reddedilen 5 görsel `473a-red/` içinde.
- 471 zemini üzerinde 2x doğrulama sayfası yapılmadı.
