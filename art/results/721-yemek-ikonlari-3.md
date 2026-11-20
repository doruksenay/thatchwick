unity-import: pending

# 721 Yemek ikonları 3: sonuç (durum: review, 7/14 teslim)

Yan yana sayfası: `art/results/721-yanyana.png` (arayuz ailesinde referans yok). Toplu önizleme x4 (parşömen + koyu): `art/results/721-ikonlar-x4.png` (14 ikonun hepsi, sıra: alfabetik).

## Araç ve ayarlar
- `create_image_pixen` 32x32, `no_background`, `single color outline`, `low detail`; ikon başına 1 çağrı (316 yöntemi), açıklama sonuna "no face..." eki.
- 14 çağrı x 1 gen = **14 gen** (`cost_est` 14). `get_balance` sonrası: 1563,7 (paralel şeritler dahil).
- Hız sınırı (10/10 iş) ilk gönderimde 13 çağrıyı reddetti (harcama yok); boşalan slotlara küçük gruplarla bir kez yeniden gönderildi.
- Renk: ham 23-108 renk; Pillow median-cut 16 renk, dither yok, alfa 0/255. Ham hâller `art/tmp721/` (commit edilmez).
- Masaüstü kopyası: yerel.

## Teslim edilenler (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0)
| Dosya | Stil denetimi | Not |
|---|---|---|
| icon_item_paste_stew | GEÇTİ | |
| icon_item_fish_steam | GEÇTİ | |
| icon_item_radish_salad | GEÇTİ | |
| icon_item_cress_salad | GEÇTİ | |
| icon_item_porridge_barley | UYARI (palete uzak, %83) | teslim |
| icon_item_rye_gruel | GEÇTİ | kaseden çok sepete benziyor |
| icon_item_salt_fish_boil | GEÇTİ | kase yok, balık ve havuç düz duruyor |

## Teslim edilmeyenler (`art/results/721-red/`)
| Dosya | Neden |
|---|---|
| icon_item_cabbage_paste | KALDI (palet dışı %61) + yüz çizilmiş |
| icon_item_pea_mash | KALDI (palet dışı %69) + yüz çizilmiş |
| icon_item_eel_pie | KALDI (palet dışı %78); dilim belirsiz |
| icon_item_mussel_pilaf | KALDI (palet dışı %79) |
| icon_item_turnip_mash | UYARI ama yüz/göz çizilmiş |
| icon_item_pumpkin_stuffed | GEÇTİ ama göz benzeri noktalar, yüzlü yaratık okunuyor |
| icon_item_broad_bean_oil | GEÇTİ ama ortadaki bakla yüz gibi okunuyor |

Palet KALDI'ları doygunluk değil palet dışı renk sorunu; görevdeki `fit` kuralı yalnızca doygunluk için, bu yüzden uygulanmadı. Tek deneme kuralı gereği yeniden üretilmedi.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut 32x32 | 14/14 |
| Yarı saydam piksel | 0 (14/14) |
| Renk <= 16 | 14/14 (indirgeme sonrası) |
| validate_iso | uygulanmadı (UI ikonu) |

## Sorunlar / öneri
1. Yüz eğilimi sürüyor (5/14). Önerilen 2. tur: `create_map_object` 32x32 (401/417 yöntemi) ile eksik 7 ikon (cabbage_paste, pea_mash, eel_pie, mussel_pilaf, turnip_mash, pumpkin_stuffed, broad_bean_oil), yaklaşık 7-14 gen; yeni iş dosyası kullanıcı onayında.
2. `arayuz` ailesinde kilit/referans yok, palet denetimi zayıf bir ölçüt.
