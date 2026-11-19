unity-import: pending

# 420c Ürün ikonları C parça 3/4 (durum: review)

Yan yana sayfası: [420c-yanyana.png](420c-yanyana.png) (arayuz ailesinde kilit yok, yalnızca yeni görseller). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 15 x `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (420b yöntemi), açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 15 çağrı x 1 gen = **15 gen** (cost_est 15, cost_max 30).
- Hız sınırı: eşzamanlı iş tavanı nedeniyle birkaç çağrı "rate limit exceeded" aldı (harcamasız), bekleyip bir kez yeniden gönderildi (tarifin tek istisnası).
- `get_balance` sonrası: **3785,5** (paralel şeritler dahil).
- Renk: 5 ham çıktı 16'dan fazla opak renk içeriyordu (beeswax 21, dye_red 31, dye_yellow 23, remedy 17, straw 23); Pillow median-cut ile 14'e indirildi, dither yok, alfa ikili, yeniden boyama yok. Ham hâller `art/tmp420c/raw` (commit edilmedi).

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_{flour,beeswax,pelt,fur,remedy,brick,roof_tiles,cut_stone,dye_red,dye_yellow,dye_blue,soap,felt,manure,straw}.png: 15 dosya.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (15/15) |
| Renk | ≤16 opak (15/15), yarı saydam piksel 0 |
| `style.py check --family arayuz` | KALDI yok; 3 GEÇTİ (flour, manure, soap), 12 UYARI (kilit kurulmadı / palet dışı / palete uzak). En uzak: beeswax (%19 palete yakın), dye_blue (%27), dye_red (%45), cut_stone (%53) |
| Yüz/göz/figür | Yok (15/15) |
| Teslim edilmeyen | Yok |

## Sorunlar
- `pelt` koyu mor-bordo oval, ham deri yerine düz bir cisim gibi okunuyor; `fur` mor-gri katlı blok, kürk dokusu zayıf. Renk reddedilebilir.
- `felt` düz bordo levha (katlı kumaş olarak okunuyor, `cloth` ile benzeşebilir).
- `straw` demet yerine bitki/başak gibi duruyor (tabanda yuvarlak düğüm).
- `dye_*` pot yerine kavanoz/kova gibi; kırmızı/sarı/mavi ayrımı net.
- `validate_iso.py` uygulanmadı (UI ikonu).
