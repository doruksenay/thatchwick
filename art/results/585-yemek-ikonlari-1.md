unity-import: pending

# 585 Yemek ikonları 1: sonuç (durum: review, 12/16 teslim)

Yan yana: `art/results/585-yanyana.png` (arayuz ailesinde referans yok). x4 önizleme (parşömen ve koyu zemin): `art/results/585-ikonlar-x4.png`.

## Araç ve ayarlar
- 316 yöntemi: `create_image_pixen` 32x32, `no_background`, `single color outline`, `low detail`, her açıklamanın sonuna "no face, no eyes, no creature, no person"; 16 çağrı = 16 gen.
- Pixen yüz/yaratık üreten 6 ikon (pestil, saffron_bun, saffron_pilaf, soup_tarhana, soup_wild_garlic, rabbit_stew) tarif gereği `create_map_object` 32x32 (high top-down, low detail) ile yeniden üretildi: 6 çağrı. Pixen hamları `art/tmp585/ham-pixen/` (commit dışı).
- Toplam ≈ 22 gen (`cost_est` 16, `cost_max` 32 içinde). Hız sınırı (10/10 iş, paylaşılan hesap) nedeniyle çağrılar boşalan yerlere yeniden gönderildi (tarifin tek istisnası). get_balance sonrası: 1609,7 (diğer şeritlerin harcaması dahil).
- Renk indirgeme: ham 11-63 renk, Pillow median-cut ile 16'ya (dither yok, alfa 0/255). Yeniden boyama yok.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
borek_cheese, fish_fry, goose_roast, kome, lamb_tandir, rabbit_stew, saffron_bun, saffron_pilaf, sarma_leaf, soup_tarhana, spinach_saute, venison_roast (hepsi `icon_item_` önekli): 12 dosya.

## Teslim edilmeyenler (`art/results/585-red/`)
`style.py check` KALDI (palet dışı renkler; doygunluk sorunu değil, bu yüzden `fit` uygulanmadı): fish_grill (%71), gozleme (%79), pestil (%22), soup_wild_garlic (%80). Yeniden üretim ayrı iş olmalı (max_attempts 1).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut 32x32, yarı saydam piksel 0 | 16/16 |
| Renk ≤16 | 16/16 (11-16) |
| style check | GEÇTİ 4 (kome, sarma_leaf, spinach_saute, venison_roast); UYARI 8 (borek, fish_fry, goose, lamb, rabbit_stew, saffron_bun, saffron_pilaf, soup_tarhana); KALDI 4 (yukarıda) |
| validate_iso | uygulanmadı (UI ikonu) |

## Sorunlar
1. saffron_bun düz sarı disk (11 renk), ayırt edilebilirliği zayıf; borek peynirinde göz benzeri iki nokta olabilir. Kullanıcı gözle karar versin.
2. Pixen çıktısı (fish_*, goose, lamb, spinach) ile map_object çıktıları (kaseler) arasında perspektif/stil farkı var.
3. Kalan 4 ikon yeniden üretim bekliyor.
