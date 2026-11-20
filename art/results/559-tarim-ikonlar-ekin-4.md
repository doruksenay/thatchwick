unity-import: pending

# 559 Tarım ikonları 4/6: sonuç (durum: review)

Yan yana sayfası: [559-yanyana.png](559-yanyana.png) (arayuz ailesinde referans yok, yalnızca yeni 14 görsel). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 16 x `create_map_object` basic mod, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (417 yöntemi; pixen yüz riski olduğu için baştan bu araç), açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". **16 gen** (cost_est 16, cost_max 32).
- Hız sınırı ("rate limit exceeded", paylaşılan hesap) birkaç çağrıda alındı; bu çağrılar harcama yapmadan reddedildi, boşalınca yeniden gönderildi (tarifin tek istisnası).
- Renk indirgeme: >16 renkli çıktılar (anise, coriander, dry_pea, linseed, nigella, seed_anise, seed_calendula, seed_cumin, seed_nigella ya da benzeri) Pillow median-cut ile 16 renge indi (dither yok, alfa korunur); yeniden boyama yok.
- `get_balance` sonrası: **1742,7** (paralel şeritler dahil).

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_{seed_rose, seed_calendula, cumin, seed_cumin, coriander, seed_coriander, anise, seed_anise, nigella, seed_nigella, purslane, dry_pea, coarse_fibre, linseed}: **14 dosya teslim**.

## Teslim edilmeyenler (2)
`icon_item_rose_petals.png` ve `icon_item_calendula.png`: stil denetiminde KALDI ("palet dışı renkler", palete yakın %57 / %42). `style.py fit` arayuz ailesinde referans olmadığı için çalışmadı. Ham (16 renge indirilmiş) hâlleri `art/results/559-red/` içinde. Yeniden üretim önerisi: 2 gen, daha az doygun açıklama ("muted"/"dusty pink", "dull orange") ile.

## Doğrulama
| Dosya | Renk | Palete yakın | Stil |
|---|---|---|---|
| anise, coarse_fibre, coriander, cumin, linseed, nigella, seed_calendula, seed_coriander, seed_cumin | ≤16 | %100 | GEÇTİ |
| dry_pea, purslane, seed_anise, seed_nigella, seed_rose | ≤16 | %100 | UYARI (palete uzak) |
| calendula, rose_petals | ≤16 | %42 / %57 | KALDI (teslim edilmedi) |

Boyut 32x32 (16/16), yarı saydam piksel 0. `validate_iso.py` uygulanmadı (UI ikonu, izometrik değil). Gözle kontrol yapılmadı: yalnızca ilk görsel (rose_petals) görüldü; tohum kesesi ailesi tutarlılığı ve baharat kesesi şekilleri kullanıcı kontrolünde.

## Sorunlar
- 2 ikon KALDI, yukarıda.
- Kese stili 316'daki v3 kese ile birebir aynı olmayabilir (init görseli desteklenmiyor).
