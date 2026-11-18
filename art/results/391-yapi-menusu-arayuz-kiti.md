unity-import: pending

# 391 Yapı menüsü arayüz kiti

- Yön: `parsomen` (210'da `yon:` satırı yok, varsayılan). Stil referansı `ui_panel_durum_parsomen_1.png`, 416x192'den 208x96'ya yarıya indirildi, 16 renge nicemlendi (istek yükü küçük kalsın diye).
- Araç: `create_ui_asset` x3, 256x256, `no_background=true`, `max_colors=32`, `seed=0`, marka paleti. Her çağrı 10 gen, toplam 30 (cost_est 30). Panel çağrısı ilk denemede hız sınırına (10/10 iş) takıldı, ~60 sn sonra bir kez yeniden denendi.
- Sapma: `badge_price` için tarifteki `ellipse` şemada yok; eşdeğer `circle` (x=52, y=268, r=36) kullanıldı.
- Kesim: aracın verdiği kutular içinde saydam kenar kırpıldı (yeniden boyama yok).
- get_balance sonrası: 4427,9 (hesap paylaşımlı, öteki şeritler de harcıyor; bu işin payı 30).
- Yan yana: art/results/391-yanyana.png (arayuz ailesinin kabul referansı yok, 0 referans). 9-slice önizleme: art/results/391-9slice-x2.png
- Masaüstü kopyası: yerel.

## Dosyalar (Assets/Art/Iso/_aday/, UI parçası: pivot yok)
| Dosya | Boyut | Önerilen border L/R/T/B |
|---|---|---|
| ui_kit_panel_sheet / button_sheet / misc_sheet | 256x256 | - |
| ui_kit_panel | 113x113 | 14/14/14/14 |
| ui_kit_inset | 113x57 | 10/10/10/10 |
| ui_kit_tab_on, tab_off | 53x29 | 10/10/10/8 |
| ui_kit_cell, cell_sel, cell_lock | 65x65 | 10/10/10/10 |
| ui_kit_btn, btn_hover, btn_press | 225x37 | 12/12/12/12 |
| ui_kit_btn_sel | 227x40 | 12/12/12/12 |
| ui_kit_btn_off | 223x35 | 12/12/12/12 |
| ui_kit_field | 161x29 | 8/8/8/8 |
| ui_kit_tooltip | 161x51 | 10/10/10/10 |
| ui_kit_scroll_track, scroll_thumb | 17x145, 17x49 | 6/6/8/8 |
| ui_kit_badge_price | 37x37 | - |
| ui_kit_badge_new | 62x26 | - |
| ui_kit_badge_lock | 39x38 | - |
| ui_kit_close | 37x37 | - |

Border değerleri gözle seçildi (köşe süsü + dikiş); Unity'de gerekirse 1-2 px ayarlanır. Sayfalar 256 olduğundan parçalar tarifin 512 tuvaline göre yarı ölçektedir; GUISkin'de 2x NEAREST ile kullanılır.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Sayfalar 256x256, saydam zemin | evet |
| Yarı saydam piksel | 0 (üç sayfa) |
| 20 parça çıktı | evet |
| Yazı/harf benzeri şekil | yok (badge_lock'ta asma kilit simgesi, close'ta çarpı) |
| 9-slice x2/x3 testi | dikiş kopmuyor; btn_off kenar noktaları soluk |
| style.py check --family arayuz | GEÇTİ 15, UYARI 5 (kilit kurulmadı: palet), KALDI 0 |

UYARI: badge_price (palete uzak, 12.8), btn (10.3), btn_hover (%99 palet dışı, 16.6), btn_sel (%97, 12.8).

## Sorunlar
- `cell` (normal) koyu çıktı, `cell_sel` açık krem; `cell_lock` koyu + çapraz çizgili. Normal ile kilitli arasındaki fark küçük; Unity'de kilitli hücreye ek karartma gerekebilir.
- `btn_off` üst/alt kenar çizgisi kesik kesik (soluk dikiş).
- Parçalar tarifin 512 tuvaline göre yarı ölçekte.
