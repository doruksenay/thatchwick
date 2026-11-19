unity-import: pending

# 420b Ürün ikonları C parça 2/4 (durum: review)

Yan yana sayfası: [420b-yanyana.png](420b-yanyana.png) (arayuz ailesinde kilit yok, yalnızca yeni görseller). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 15 x `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (401/417/419c yöntemi), açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 15 çağrı x 1 gen = **15 gen** (cost_est 15, cost_max 30).
- Hız sınırı (paylaşılan hesap): birçok çağrı "rate limit exceeded" aldı, harcama yapmadı; bekleyip yeniden gönderildi (tarifin tek istisnası; birkaç çağrı iki üç kez gerekti, hepsi harcamasız hata). Bir indirme 423 Locked verdi, 20 sn sonra alındı.
- `get_balance` sonrası: **3818,5** (paralel şeritler dahil).
- Renk: 5 ham çıktı 16'dan fazla opak renk içeriyordu (animal_salve 22, basket 19, potion_calm 25, potion_growth 29, potion_warmth 20, yarn 19: toplam 6); Pillow median-cut ile 14'e indirildi, dither yok, alfa ikili, yeniden boyama yok. Ham hâller `art/tmp420b/raw` (commit edilmedi).

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_{yarn,cloth,blanket,basket,potion_energy,potion_warmth,potion_calm,potion_focus,potion_growth,animal_salve,clay,iron_ore,iron_bar,horseshoe,gear}.png: 15 dosya.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (15/15) |
| Renk | ≤16 opak (15/15), yarı saydam piksel 0 |
| `style.py check --family arayuz` | KALDI yok; 4 GEÇTİ (animal_salve, basket, blanket, gear), 11 UYARI (kilit kurulmadı / palet dışı / palete uzak). En uzak: yarn (%14 palete yakın), cloth (%27), potion_warmth (%40), potion_calm (%58) |
| Yüz/göz/figür | Yok (15/15) |
| Teslim edilmeyen | Yok |

## Sorunlar
- `animal_salve` küçük saksıda bitki gibi okunuyor (merhem kavanozu değil); gözle onayda bakılmalı, gerekirse yeniden üretilir.
- `cloth` canlı pembe, `yarn` mor: yün için doğal ton beklenirdi. Biçim tanınıyor, renk reddedilebilir.
- İksir şişeleri kil değil cam/ince şişe gibi; `potion_focus` yeşil tıpalı küçük kil kap olarak okunuyor, `potion_energy` kırmızı tıpalı kahverengi; kil şişe ailesi birliği orta. Renk-tıpa ayrımı (kırmızı, turuncu, mavi, yeşil, yaprak) okunuyor ama turuncu/kırmızı yakın.
- `iron_bar` küçük ve soluk, külçe gibi; `iron_ore` mora yakın koyu.
- Palet uyarıları kilit kurulmadığı için yalnızca bilgi. `validate_iso.py` uygulanmadı (UI ikonu).
