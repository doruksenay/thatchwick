unity-import: pending

# 494 Ağaç ev iskeleti ve ceviz ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/494-yanyana.png` (arayuz ailesinde kabul referansı yok; yalnızca yeni 6 görsel).

## Araç ve ayarlar
- `create_map_object` basic mod, 32x32, `low detail`, `basic shading`, `single color outline`, `high top-down`. 420a dersi (pixen yüz üretiyor) nedeniyle baştan map_object kullanıldı; açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person" eklendi.
- 6 çağrı x 1 gen = **6 gen** (`cost_est` 6, `cost_max` 12). Paralel gönderildi, hız sınırı hatası olmadı.
- Renk: ham çıktılar 16-31 renkliydi; median-cut ile 16'ya indirildi (dither yok, alfa 0/255, yeniden boyama yok). Ham hâller teslim edilmedi.
- `get_balance` sonrası: **2474,9** (paralel şeritlerin harcaması dahil).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
| Dosya | Renk |
|---|---|
| icon_item_th_kit_nest | 15 |
| icon_item_th_kit_lookout | 15 |
| icon_item_th_kit_fort | 12 |
| icon_item_th_kit_hut | 15 |
| icon_item_th_kit_kiosk | 15 |
| icon_item_walnut | 15 |

## Doğrulama
| Dosya | Palete yakın | Ort. uzaklık | Sonuç |
|---|---|---|---|
| th_kit_fort | %97 | 6.4 | GEÇTİ |
| th_kit_hut | %87 | 8.1 | GEÇTİ |
| th_kit_kiosk | %100 | 5.4 | GEÇTİ |
| th_kit_lookout | %97 | 5.6 | GEÇTİ |
| th_kit_nest | %100 | 5.8 | GEÇTİ |
| walnut | %93 | 11.7 | UYARI (palete uzak; kilit kurulmadı) |

Boyut 32x32 (6/6), yarı saydam piksel 0, renk ≤16. `validate_iso.py` uygulanmadı (UI ikonu).

## Sorunlar
- Kullanıcı gözüyle bakmalı: `nest` istenen "halat merdiven" ayrıntısı küçük boyutta belirsiz (tahta yığını okunuyor); `lookout` direk/tahta demeti olarak tanınıyor, kiremit ayrıntısı seçilmiyor; `walnut` cevizler tanınıyor, kırık olan ve çekirdek net değil.
- Hiçbir ikon reddedilmedi, 6/6 teslim.
