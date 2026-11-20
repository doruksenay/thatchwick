unity-import: pending
# 475 Cam bahçe süsleri (10)

- Araç: create_image_pixen x10 (paralel), no_background=true, view=side; 1 gen/çağrı = 10 gen. Hız sınırı yok. Kalan (get_balance): 2964,9 (diğer şeritlerin harcaması da içinde). Masaüstü kopyası: yerel.
- Sapma: Pixen 32 pikselin altında yalnızca kare kanvas kabul ediyor. 32x24 çağrısı doğrulamada reddedildi (ücret yok); büyük olanlar 32x32, küçükler 24x24 üretildi (tarifteki 32x24 / 24x16 yerine). Yeniden boyama ya da kırpma yapılmadı.
- Yan yana: art/results/475-yanyana.png
- Doygunluk `fit` uygulanmadı: KALDI olanların hepsi palet dışı (yalnız doygunluk değil).

| Dosya | Boyut | Pivot x / y (alttan) | Stil denetimi | Sonuç |
|---|---|---|---|---|
| viv_bark.png | 32x32 | 16 / 3 | UYARI (palete uzak %82, x1.27) | teslim |
| viv_driftwood.png | 32x32 | 16 / 7 | UYARI (x1.29) | teslim |
| viv_snail_shell.png | 24x24 | 12 / 2 | UYARI (%96, x1.48) | teslim |
| viv_whelk.png | 24x24 | 12 / 3 | UYARI (x1.32) | teslim |
| viv_mossy_stone.png | 32x32 | - | UYARI (%83, x0.48) | teslim edilmedi: şekil yüz/maske gibi (gözler, ağız), taş değil |
| viv_flat_stone.png | 32x32 | - | KALDI (palet dışı, %57) | teslim edilmedi: yassı levha değil, mavi küp |
| viv_pot_shard.png | 24x24 | - | KALDI (%53) | teslim edilmedi: palet dışı |
| viv_coral.png | 24x24 | - | KALDI (%52, x1.72) | teslim edilmedi: palet dışı ve çok doygun (şekil tanınıyor) |
| viv_river_pebble.png | 24x24 | - | KALDI (%54) | teslim edilmedi: palet dışı |
| viv_mussel_shell.png | 24x24 | - | KALDI (%70, x0.50) | teslim edilmedi: palet dışı, soluk |

validate_iso (--kind object): teslim edilen 4 dosya geçti. Teslim: Assets/Art/Iso/_aday/ (4 dosya). Ham reddedilenler: art/results/475-red/.

## Sorunlar
- 6 görsel teslim edilmedi (tek deneme, `max_attempts` dolu sayıldı: bu bulut turu tek deneme). Yeniden deneme yeni iş gerektirir: mossy_stone, flat_stone, pot_shard, river_pebble, mussel_shell için `esya` yerine renkli süs/doğa ailesi referansı (ya da `pixflux` ile lineless) önerilir; coral için palet genişletmesi.
- `esya` ailesi kahverengi ağırlıklı; renkli nesneler (mercan, çakıl, midye) bu yüzden palet dışı kalıyor. `style.py` için `doga` gibi ayrı aile önerisi.
