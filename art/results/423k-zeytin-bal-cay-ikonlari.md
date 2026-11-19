unity-import: pending

# 423k Zeytin, bal, çay ürün ikonları (durum: review)

Yan yana sayfası: [423k-yanyana.png](423k-yanyana.png) (arayuz ailesinde kilit yok, yalnızca yeni görseller). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 19 x `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (419c yöntemi), açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 19 çağrı x 1 gen = **19 gen** (cost_est 19, cost_max 38).
- Hız sınırı: 8 çağrı "rate limit exceeded" aldı (harcama yok); diğer çağrılar bitince bir kez yeniden gönderildi (tarifin tek istisnası).
- `get_balance` sonrası: **3672,5** (paralel şeritler dahil).
- Renk: 6 ham çıktı 16'dan fazla opak renk içeriyordu (chamomile 19, honey_strained 25, leek_olive_oil 24, mint 19, olive_fine 17, olive_oil 20, olive_oil_virgin 23, olives_brined 39); Pillow median-cut ile 16'ya (en çok 14) indirildi, dither yok, alfa ikili, yeniden boyama yok. Ham hâller `art/tmp423k/raw` (commit edilmemeli).

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_ önekiyle 19 dosya:
- `icon_item_chamomile.png`
- `icon_item_honey_strained.png`
- `icon_item_leek_olive_oil.png`
- `icon_item_linden.png`
- `icon_item_mint.png`
- `icon_item_olive.png`
- `icon_item_olive_fine.png`
- `icon_item_olive_oil.png`
- `icon_item_olive_oil_virgin.png`
- `icon_item_olive_paste.png`
- `icon_item_olives_brined.png`
- `icon_item_pomace.png`
- `icon_item_sage.png`
- `icon_item_sapling_olive.png`
- `icon_item_soap_olive.png`
- `icon_item_tea_chamomile.png`
- `icon_item_tea_linden.png`
- `icon_item_tea_mint.png`
- `icon_item_tea_sage.png`

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (19/19) |
| Renk | ≤16 opak (19/19), yarı saydam piksel 0 |
| `style.py check --family arayuz` | KALDI yok; 3 GEÇTİ (olive_paste, pomace, tea_linden), 16 UYARI (kilit kurulmadı / palet dışı / palete uzak). En uzak: mint (%0 palete yakın), linden (%14), soap_olive (%19) |
| Yüz/göz/figür | Yok (gözle yan yana sayfasında) |
| Teslim edilmeyen | Yok |

## Sorunlar
- pomace koyu mor salkım üzüm gibi okunuyor (küme biçimi); zeytin küspesi kekleri olarak zayıf.
- olive_fine ve olive_oil_virgin tarifin yeşil/altın tonunu yansıtmıyor: olive_fine morumsu zeytinler, olive_oil_virgin sarı yağlı küp-testi; olive_oil sade.
- sapling_olive ince, çok küçük; olive_oil_virgin ile olive_oil küp biçimleri farklı (aile birliği orta).
- tea_mint gözle kontrol: yaprak çifti yüz gibi okunuyorsa reddedilmeli (koyu konturlu).
- Palet uyarıları kilit kurulmadığı için yalnızca bilgi.
- `validate_iso.py` uygulanmadı (UI ikonu).
