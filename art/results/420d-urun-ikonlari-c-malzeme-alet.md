unity-import: pending

# 420d Ürün ikonları C parça 4/4 (durum: review)

Yan yana sayfası: [420d-yanyana.png](420d-yanyana.png) (arayuz ailesinde kilit yok, yalnızca yeni görseller). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 14 x `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (420b yöntemi), açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 14 çağrı x 1 gen = **14 gen** (cost_est 14, cost_max 28).
- Hız sınırı (paylaşılan hesap): ilk turda 7 çağrı "rate limit exceeded" aldı (harcamasız), ilk işler bitince bir kez yeniden gönderildi (tarifin tek istisnası).
- `get_balance` sonrası: **3785,5** (paralel şeritler dahil).
- Renk: yalnızca `linen_yarn` 17 opak renkliydi, median-cut ile 14'e indirildi (dither yok, alfa ikili, yeniden boyama yok). Diğerleri 7-14 renk. Ham hâller `art/tmp420d/raw` (commit edilmedi).

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_{rye_flour,linen_yarn,linen,broken_pottery,broken_glass,ash,fine_sand,clear_glass,mirror_sphere,mirror_glass,hunting_bow,snare,tin,mussel_shell}.png: 14 dosya.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (14/14) |
| Renk | ≤16 opak (14/14), yarı saydam piksel 0 |
| `style.py check --family arayuz` | KALDI yok; 3 GEÇTİ (broken_pottery, hunting_bow, mussel_shell), 11 UYARI (kilit kurulmadı / palete uzak). En uzak: clear_glass (%4 palete yakın), linen_yarn (%41), broken_glass (%50), mirror_glass (%51) |
| Yüz/göz/figür | Yok (14/14) |
| Teslim edilmeyen | Yok |

## Sorunlar
- `tin` sarı, altın külçe gibi okunuyor (kalay gümüş-gri beklenirdi); `linen_yarn` canlı pembe (keten için doğal bej beklenirdi); `linen` beyaz rulo, mor kontur.
- `broken_pottery` tek kil topağı gibi, parçalı çanak olarak zor okunuyor; `ash` mor-gri dağınık leke.
- `mussel_shell` koyu çift kabuk, göz gibi okunabilir ama yüz değil; gözle onayda bakılmalı.
- `mirror_sphere` ile `mirror_glass` ayırt ediliyor (küre mor, ayna mavi çerçeveli). `hunting_bow` yay yerine düz sopa/kürek gibi: yay eğrisi yok, reddedilmesi muhtemel.
- `validate_iso.py` uygulanmadı (UI ikonu).
