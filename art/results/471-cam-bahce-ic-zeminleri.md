unity-import: pending
# 471 Cam bahçe iç zeminleri

- Araç: create_image_pixflux x3 (paralel), 160x96, view=side, outline=lineless, no_background=false; 1 gen/çağrı, toplam 3. Kalan: 3027,9. Masaüstü kopyası: yerel.
- Yan yana: art/results/471-yanyana.png

| Dosya | Boyut | Stil denetimi | Sonuç |
|---|---|---|---|
| viv_bg_moist.png | 160x96 | UYARI (palete uzak, x1.22) | teslim: Assets/Art/Iso/_aday/ |
| viv_bg_dry.png | 160x96 | KALDI (x1.68; fit sonrası da palet dışı) | teslim edilmedi, art/results/471-red/ |
| viv_bg_salt.png | 160x96 | KALDI (x1.52; fit sonrası da palet dışı) | teslim edilmedi, art/results/471-red/ |

Pivot: yok (arka plan). Zemin döşeme testi: uygulanmaz.

## Sorunlar
- moist: tavanda bir lamba ve ince çerçeve var (tarif çerçevesiz istiyordu); zemin şeridi çakıllı. Kabul kararı kullanıcıda.
- dry ve salt: `fit` (x0.68 / x0.75) sonrası da palet dışı renk nedeniyle KALDI. Tek deneme kuralı gereği yeniden üretilmedi. salt'ta ayrıca arkada yosun ve koyu çerçeve var. Yeniden deneme için yeni iş gerekir.
- Pivot ve izometri doğrulaması yan görünümde anlamsız.
