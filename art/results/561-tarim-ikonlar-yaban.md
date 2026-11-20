unity-import: pending

# 561 Tarım ikonları 6/6: yaban ürünleri: sonuç (durum: review)

Yan yana sayfası: [561-yanyana.png](561-yanyana.png) (arayuz ailesinde referans yok, yalnızca 19 yeni görsel). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 19 x `create_map_object` basic mod, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (417/559 yöntemi; pixen yüz riski nedeniyle baştan bu araç), açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". **19 gen** (cost_est 19, cost_max 38).
- Hız sınırı ("rate limit exceeded", paylaşılan hesap) birkaç çağrıda alındı; bu çağrılar harcama yapmadan reddedildi, 15-30 sn sonra yeniden gönderildi (tarifin tek istisnası).
- Renk indirgeme: >16 renkli çıktılar Pillow median-cut ile 16 renge indi (dither yok, alfa 0/255); yeniden boyama yok. Ham hâller `art/tmp561/raw/` (commit edilmez).
- `get_balance` sonrası: **1676,7** (paralel şeritler dahil).

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_{acorn, bilberry, chanterelle, dandelion, licorice, mallow, morel, porcini, salep, sloe, sorrel, sumac}: **12 dosya teslim**.

## Teslim edilmeyenler (7)
Stil denetiminde KALDI ("palet dışı renkler"); 16 renge indirilmiş hâlleri `art/results/561-red/` içinde: caper (%84), hawthorn (%76), juniper (%65), marsh_mallow_root (%63), samphire (%0, ortalama uzaklık 28,9), st_johns_wort (%50), wild_pear (%70). Şekiller yan yana sayfasında tanınıyor (juniper, wild_pear, hawthorn, caper iyi); sorun palet uyumu. `style.py fit` arayuz ailesinde referans olmadığı için çalışmadı. Yeniden üretim önerisi: 7 gen, daha az doygun açıklamalarla ("muted", "dusty", "dull").

## Doğrulama
| Dosya | Renk | Palete yakın | Stil |
|---|---|---|---|
| acorn, dandelion, licorice, porcini, sloe, sorrel | ≤16 | %100 | GEÇTİ |
| chanterelle, mallow, morel | ≤16 | %100 | UYARI (palete uzak) |
| bilberry, salep, sumac | ≤16 | %81 / %81 / %96 | UYARI (palete uzak) |
| caper, hawthorn, juniper, marsh_mallow_root, samphire, st_johns_wort, wild_pear | ≤16 | %0-84 | KALDI (teslim edilmedi) |

Boyut 32x32 (19/19), yarı saydam piksel 0. `validate_iso.py` uygulanmadı (UI ikonu, izometrik değil). Yan yana sayfasına göz atıldı: yüz/göz yok.

## Sorunlar
- 7 ikon KALDI, yukarıda. dandelion ince ve soluk (okunabilirlik düşük), licorice küçük; kullanıcı kontrolünde.
