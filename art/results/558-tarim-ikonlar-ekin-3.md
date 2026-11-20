unity-import: pending

# 558 Tarım ikonları 3/6: sonuç (durum: review, 13/16 teslim)

Yan yana sayfası: `art/results/558-yanyana.png` (arayuz ailesinde referans yok, yalnızca yeni görseller; 16 görselin hepsi, teslim edilmeyenler dahil). Teslim edilmeyen 3 ham dosya: `art/results/558-red/`.

## Araç ve ayarlar
- `create_map_object` basic 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline`; her açıklamanın sonunda "simple still-life object, no face, no eyes, no creature, no person". Gerekçe: 556'da pixen 16 ikonun 11'ini yüzlü/yaratık verdi, bu yüzden tarifteki yedek yöntem doğrudan ilk deneme olarak seçildi (pixen atlandı, boşa harcama olmasın diye). 16 çağrı x 1 gen = **16 gen** (`cost_est` 16, `cost_max` 32).
- Hız sınırı: ilk gönderimde 6 çağrı "rate limit exceeded" aldı (ücret yok); işler bitince bekleyip yeniden gönderildi (tarifin tek istisnası).
- Renk indirgeme: ham çıktılar 16 renge median-cut (dither yok, alfa 0/255); yeniden boyama yok.
- `get_balance` sonrası: **1737,7** (paralel şeritler dahil).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
| Dosya | Stil denetimi |
|---|---|
| icon_item_artichoke | UYARI (palete uzak) |
| icon_item_asparagus | UYARI (palete uzak) |
| icon_item_cauliflower | GEÇTİ |
| icon_item_celery | UYARI (palete uzak) |
| icon_item_rocket | GEÇTİ |
| icon_item_seed_artichoke | GEÇTİ |
| icon_item_seed_asparagus | UYARI (palete uzak) |
| icon_item_seed_cauliflower | GEÇTİ |
| icon_item_seed_celery | GEÇTİ |
| icon_item_seed_chard | GEÇTİ |
| icon_item_seed_okra | GEÇTİ |
| icon_item_seed_parsley | UYARI (palete uzak) |
| icon_item_seed_rocket | GEÇTİ |

## Teslim edilmeyenler (KALDI)
- `icon_item_okra` (%8 palete yakın), `icon_item_chard` (%50), `icon_item_parsley` (%67): canlı yeşil, aile paletinden uzak. Şekilleri tanınıyor (okra yaprak/kapsül biçiminde, tam bamya baklası gibi değil). `style.py fit` arayuz ailesinde referans olmadığı için çalışmadı. Ham hâller `art/results/558-red/`. Tek deneme kuralı gereği yeniden üretilmedi; öneri: ayrı iş (3 gen) ya da arayuz referans kilidi kurulunca `fit`.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (13/13) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 |
| `style.py check --family arayuz` | 7 GEÇTİ, 6 UYARI, KALDI yok (teslim edilenlerde); 3 KALDI teslim edilmedi |
| Yüz/göz | Yan yana sayfasında görülmedi |
| `validate_iso.py` | uygulanmadı (UI ikonu, izometrik değil) |

## Sorunlar
1. 3 ikon eksik (yukarıda). 2. `.meta` yazılmadı. 3. Tohum keseleri 316 kese ailesiyle biçimce tam aynı değil (map_object perspektifi); kullanıcı yan yana sayfasına bakmalı. 4. seed_asparagus "kese içinde kök taç" yerine bitki gibi okunuyor, kullanıcı gözüyle kontrol edilmeli.
5. Öneri: PixelLab görseli kaydetmek için `art/tmp` altında geçici betik kullanıldı; `tools/art` değişmedi.
