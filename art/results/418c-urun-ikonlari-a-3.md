unity-import: pending

# 418c Ürün ikonları A parça 3/3 (durum: review)

Yan yana sayfası: [418c-yanyana.png](418c-yanyana.png) (arayuz ailesinde kilit yok, yalnızca yeni görseller). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 17 x `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (401/417 yöntemi), açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 17 çağrı x 1 gen = **17 gen** (cost_est 17).
- Hız sınırı (paylaşılan hesap): birçok çağrı "rate limit exceeded" aldı, harcama yapmadı; bekleyip yeniden gönderildi (tarifin tek istisnası).
- `get_balance` sonrası: **3925,5** (paralel şeritler dahil).
- Renk: 8 ham çıktı 16'dan fazla opak renk içeriyordu (breakfast 31, kete_meat 17, pilaf 21, porridge_wheat 17, rusk 23, sarma 23, stew_game 41, wrap_fish 35); Pillow median-cut ile 16'ya indirildi (dither yok, alfa ikili, yeniden boyama yok). Ham hâller `art/tmp418c/raw` (commit edilmedi).

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_{breakfast,pilaf,sarma,hosaf,helva,pekmez,stew_game,wrap_fish,borek_leek,kete_meat,tart_apple,simit,rusk,bread_rye,bread_maslin,porridge_oats,porridge_wheat}.png: 17 dosya.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (17/17) |
| Renk | ≤16 opak (17/17), yarı saydam piksel 0 |
| `style.py check --family arayuz` | KALDI yok; 2 GEÇTİ (helva, porridge_oats), 15 UYARI (kilit kurulmadı / palet dışı). En uzak: wrap_fish (%21 palete yakın), pekmez (%36) |
| Yüz/göz/figür | Gözle yok (17/17) |
| Teslim edilmeyen | Yok |

## Sorunlar
- borek_leek küçük bir ekmek topuna benziyor (pırasa belirsiz); porridge_oats düz bir kase/tabak gibi okunuyor. Kullanıcı görsel onayında bakmalı.
- Palet uyarıları kilit kurulmadığı için yalnızca bilgi.
