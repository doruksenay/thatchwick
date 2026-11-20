unity-import: pending

# 560 Tarım ikonları 5/6: meyve ve fidan: sonuç (durum: review, kısmi: 14/19)

Yan yana sayfası: `art/results/560-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni görseller).

## Araç ve ayarlar
- 1. tur: `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, ikon başına bir çağrı (316 yöntemi), her açıklamanın sonuna `, simple still-life object, no face, no eyes, no creature, no person`. 19 çağrı = 19 gen. Hız sınırı (10/10 iş, paylaşılan hesap) nedeniyle bazı çağrılar bekleyip bir kez yeniden gönderildi.
- 2. tur (iş dosyasındaki yedek yöntem): pixen 1. turda 6 ikonu yüzle/göz ile çizdi (bitter_orange, lemon, medlar, peach, quince, scion) ve carob kara bir top gibi çıktı. Bunlar için `create_map_object` 32x32, `low detail`, aynı açıklama sonekiyle 7 çağrı = 7 gen. 1 ve 2. tur toplamı **26 gen** (`cost_est` 19, `cost_max` 38; sınır içinde). Hız sınırı nedeniyle 3 çağrı bekleyip yeniden gönderildi.
- Renk indirgeme: ham çıktılar 18-61 renk (ölçüt ≤16); Pillow median-cut 16 renk, dither yok, alfa 0/255. Yeniden boyama yok. Ham hâller `art/tmp560/` altında (commit edilmedi).
- `style.py fit` kullanılamadı ("arayuz ailesinin referansı yok").
- Masaüstü kopyası: yerel.
- `get_balance` sonrası: 1674,7 (paralel şeritler dahil).

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
| Dosya | Kaynak | Stil denetimi |
|---|---|---|
| icon_item_almond | pixen | UYARI (tek badem, çevresinde açık hale; "almonds" tam karşılanmadı) |
| icon_item_apricot | pixen | UYARI |
| icon_item_fig | pixen | GEÇTİ |
| icon_item_henna_leaf | pixen | UYARI |
| icon_item_mulberry | pixen | UYARI |
| icon_item_raspberry | pixen | UYARI |
| icon_item_sapling_citrus | pixen | UYARI |
| icon_item_sapling_fruit_tree | pixen | GEÇTİ |
| icon_item_sour_cherry | pixen | GEÇTİ |
| icon_item_walnut | pixen | GEÇTİ (kestaneye benziyor; "çatlak yarım" yok) |
| icon_item_carob | map_object | GEÇTİ |
| icon_item_lemon | map_object | UYARI |
| icon_item_medlar | map_object | UYARI (erik/elmaya benziyor) |
| icon_item_scion | map_object | UYARI (tek dal; "bağlı demet" zayıf) |

## Teslim edilmeyenler (5)
| Dosya | Neden |
|---|---|
| bitter_orange | map_object çıktısı yüzsüz ve tanınıyor ama stil denetimi KALDI (palet dışı renkler, %69); pixen çıktısı yüzlü. Ham hâli `art/560-red/map_icon_item_bitter_orange.png` |
| peach | map_object çıktısı KALDI (%66); pixen çıktısı yüzlü. `art/560-red/map_icon_item_peach.png` (elma gibi görünüyor) |
| quince | map_object çıktısı KALDI (%79); pixen çıktısı yüzlü. `art/560-red/map_icon_item_quince.png` (elma gibi) |
| cornel | pixen çıktısı kiraz gibi, yüzsüz; stil denetimi KALDI (%68). `art/560-red/icon_item_cornel.png` |
| currant | pixen çıktısı tanınıyor, yüzsüz; stil denetimi KALDI (%66). `art/560-red/icon_item_currant.png` |

Not: 560-red altındaki dosyalar yalnızca incelemek içindir; `Assets/Art/Iso/`ya konmadı. Palet denetimi "kilit kurulmadı" durumunda; kullanıcı bu 5'ini görsel olarak kabul etmek isterse 560-red'den alabilir. `art/560-red/` içindeki `icon_item_lemon.png` ve `icon_item_peach.png`, `icon_item_bitter_orange.png` pixen (yüzlü) ham dosyalarıdır, kullanılmamalı.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (14/14) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (indirgeme sonrası) |
| `style.py check --family arayuz` | 14 teslim: 5 GEÇTİ, 9 UYARI; 5 KALDI teslim edilmedi |
| `validate_iso.py` | uygulanmadı (UI ikonu) |
| Yüz/göz | teslim edilenlerde yok (göz kontrolü) |

## Sorunlar / öneri
1. 5 ikon eksik: bitter_orange, cornel, currant, peach, quince. Yeniden üretim ya da kullanıcı kararıyla 560-red'den kabul.
2. almond, walnut, medlar, scion tanınırlığı zayıf; kullanıcı yan yana sayfasına bakarak karar versin.
3. Aile kilidi kurulu olmadığından palet denetimi gevşek; `fit` çalışmıyor.
