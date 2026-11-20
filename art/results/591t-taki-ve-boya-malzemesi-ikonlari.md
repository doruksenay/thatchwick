unity-import: pending

# 591t Takı, takı malzemesi ve boya malzemesi ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/591t-yanyana.png` (arayuz ailesinde referans yok, yalnızca yeni görseller). Önizleme: `art/results/591t-ikonlar-x4.png` (parşömen), `art/results/591t-ikonlar-x4-koyu.png` (koyu); sıra alfabetik (gemstone, gold_dust, jw_bracelet, jw_brooch, ...).

## Araç ve ayarlar
- `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; her açıklamanın sonuna "simple still-life object, no face, no eyes, ..." eklendi. 316 ile aynı yöntem (ikon başına bir çağrı).
- 20 çağrı x 1 gen = **20 gen** (`cost_est` 20, `cost_max` 40). Hız sınırı (10/10 eşzamanlı iş, paylaşılan hesap) 2 turda 6 çağrıda alındı; başarısız çağrı harcama yapmadı, işler boşalınca bir kez yeniden gönderildi (tarifin istisnası). `get_balance` sonrası: 1562,7 (paralel şeritlerin harcaması dahil).
- Renk indirgeme: ham çıktılar 32-58 renk; Pillow median-cut ile 16 renge indirildi (dither yok, alfa 0/255). Ham hâller `art/tmp591/` altında (commit edilmedi).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
19 teslim: icon_item_{gemstone, gold_dust, jw_bracelet, jw_brooch, jw_cord_necklace, jw_earring, jw_earring_gold, jw_evil_eye, jw_evil_eye_gem, jw_glass_beads, jw_necklace_gem, jw_nose_ring, jw_ring, jw_ring_gem, jw_ring_gold, jw_seal_ring, lichen, walnut_hull, weld_berry}.
**Teslim edilmedi:** `icon_item_jw_earring_gem.png` (style.py KALDI, palet dışı renkler %74); `art/591t-red/` içinde duruyor. Yeniden üretim önerilir (1 gen).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (20/20) |
| Yarı saydam piksel | 0 |
| Renk sayısı | 13-16 (≤16) |
| `style.py check --family arayuz` | 8 GEÇTİ, 11 UYARI (palete uzak), 1 KALDI (jw_earring_gem) |
| `validate_iso.py` | uygulanmadı (izometrik değil) |

## Sorunlar (göz kontrolü)
1. `jw_brooch` gri disk/kalkan gibi okunuyor (açık halka ve iğne yok, "bakır" yerine gri); `jw_evil_eye` düz mavi küre, eş merkezli halkalar ve ip zayıf; `jw_nose_ring` ve `jw_ring` ince ve küçük; `lichen` seçilmesi zor. Bu dördü için yeniden üretim önerilir.
2. Küpe çifti (`jw_earring`) iki ayrı halka olarak okunuyor, tek halka gibi görünmüyor: ölçüt sağlandı. Yüzük dörtlüsü (ring, seal, gold, gem) ayırt ediliyor; bakır yüzük diğerlerinden zayıf.
3. `jw_necklace_gem` ve `jw_evil_eye_gem` tarife uygun; nazarlık ortadaki kırmızı taşla ayrışıyor.
4. Yüz/göz çıkmadı.
