unity-import: pending

# 783 Kedi ve küçük köpek animasyonları: kedi teslim, köpek yapılmadı

Sonuç: `status: review` (kısmi). Kedi 5 klip (sneak, pounce, run_fish, walk: 4 yön; box: 2 yön), 18 klip-yön x 9 kare = 162 PNG teslim. Küçük köpek (`howl`, `walk`) yapılmadı: 780 köpek karakterini üretmedi (kimlik yok), tarif gereği çağrı yapılmadı. Masaüstü kopyası: yerel.
Yan yana sayfası (referanslar + her klibin se ve nw 00 karesi, box se): `art/results/783-yanyana.png`. Klip GIF'leri: `art/results/783-cat_1_v2_<klip>_<yön>.gif` (18 adet).

## Araç ve ayarlar
- `animate_character`, mod v3, `frame_count=8`, `keep_first_frame=true` (9 kare), yön başına 1 çağrı, en çok 3-4 eşzamanlı. Karakter: `cat_1_v2` 191be777-5901-4865-8226-00cd100e45eb. Eylem metinleri tarifteki gibi. Hız sınırı hatası olmadı. Çağrı başına 1 gen.
- Ham kare tuvali: se/sw 32x32, nw/ne 36x36. Her karenin durma karesiyle hizası rotasyon üzerinden alfa ile doğrulandı (alfa hatası 0), 780'in 26x25 durma tuvaline kırpıldı: ayak satırı ve pivot sütunu aynı, tuval yalnızca yukarı ve yanlara gerektiği kadar genişletildi (nw/ne klipleri 26-30 x 25). Yeniden örnekleme yok.
- Renk: ham doygunluk x1,56-1,66 ile KALDI; kare başına `style.py fit --family hayvan` (doygunluk x0,7) sonrası 162/162 GEÇTİ. Ham kareler `art/tmp783/` altında (commit edilmez).
- `box`: durma tuvali, üst %55 (12 px) kalır, en alt opak satır alttan 2. satıra taşındı.
- walk ve diğer klipler 9 kare (son kare kapanış sayılır).

## Doğrulama
| Kontrol | Sonuç |
|---|---|
| `style.py check --family hayvan` | 162/162 GEÇTİ (fit sonrası) |
| `validate_iso.py --kind object` | 162/162 geçti |
| `scale_check.py` (00 kareleri, box dahil) | 18/18 TAMAM (se/sw 19 px, ne/nw 23 px; kedi 16-24) |
| Ayak satırı kayması | ≤1 px (pounce, run_fish, walk se/sw ve sneak sw: 1; diğerleri 0) |

Tuval: se/sw 26x25; sneak/pounce/walk ne 28x25, sneak nw 28x25, pounce nw ve walk nw 26x25, run_fish nw 30x25, ne 28x25. Pivot x: tuval ortası (durma karesi pivot sütunu), pivot y alttan 1.

## Dosyalar (`Assets/Art/Iso/_aday/`)
`iso_animal_cat_1_v2_{sneak,pounce,run_fish,walk}_{se,sw,nw,ne}_00..08.png` (16 x 9) ve `iso_animal_cat_1_v2_box_{se,sw}_00..08.png` (2 x 9) = 162 PNG.

## Sorunlar
- Köpek (`dog_lapdog_v2` `howl` se/sw, `walk` 4 yön; 6 çağrı): 780'de köpek karakteri üretilemedi. 780b gelince 783b olarak yazılmalı. `supersedes` listesindeki köpek uluma maddesi arşivlenmemeli.
- Kuyruklu kedi hareketleri (sıçrama, balıkla kaçma) görsel olarak kullanıcı kararı bekler; GIF'lere bakılmalı. Balığın görünüp görünmediği ve sıçramanın okunurluğu elle bakılmadı (yalnızca ölçütler).
- `walk` v3 ile üretildi (şablon yürüme klibi `walk-8-frames` da var; beğenilmezse ucuz alternatif).

## Generation
| Çağrı | Gen |
|---|---|
| animate_character v3 x18 | 18 |
| Toplam | 18 (cost_max 36) |

`get_balance` sonrası: 9419 (paralel şeritlerin harcaması dahil).
