unity-import: pending

# 424k Zeytin ağacı, pres, küp, süzgeç ve çay öbekleri (rapor)

Sonuç: **9/12 teslim** (3 eşya stil denetiminde KALDI). Yan yana: [424k-yanyana.png](424k-yanyana.png) (bitki ailesi: 3 zeytin evresi + 4 öbek, fit sonrası), [424k-yanyana-esya.png](424k-yanyana-esya.png) (esya: ezme yalağı, pres, küp, süzgeç; ham). Teslim edilmeyen ve ham hâller: `art/results/424k-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme; `footprint_guide.py` kılavuzu + maske (80x72, 1x1, pivot 40,24, yükseklik 34), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarif açıklamaları + `, no face, no eyes, no creature, no person`. İkon: basic mod 32x32, aynı ayarlar (423k yöntemi). 12 çağrı x 1 gen = **12 generation** (cost_est 12, cost_max 24). Hız sınırı hatası olmadı. `get_balance` sonrası: **3656,5** (paralel şeritler de harcıyor).
Bitki/öbek görsellerine `style.py fit --family bitki` uygulandı (x0,38-0,93; 374/422k ile aynı yol; `bitki` kilidi kurulmadı). `esya` ailesi kilitli olduğundan eşyalara fit uygulanmadı.

## Teslim edilenler (Assets/Art/Iso/_aday/)
| Dosya | Boyut | Pivot x / y (alttan) | Renk | Stil | validate_iso |
|---|---|---|---|---|---|
| iso_plant_olive_young_se.png | 80x72 | 40 / 24 | 47 | UYARI (bitki, fit x1,14, %80) | geçti |
| iso_plant_olive_grown_se.png | 80x72 | 40 / 24 | 36 | UYARI (fit x1,15, %65) | geçti |
| iso_plant_olive_fruit_se.png | 80x72 | 40 / 24 | 49 | UYARI (fit x1,15, %86) | geçti |
| iso_item_olive_crusher_se.png | 80x72 | 40 / 24 | 47 | GEÇTİ (esya, %90, x0,68) | geçti |
| iso_forage_chamomile_se.png | 80x72 | 40 / 24 | 26 | UYARI (fit x1,19, %69) | geçti |
| iso_forage_linden_se.png | 80x72 | 40 / 24 | 42 | UYARI (fit x1,15, %67) | geçti |
| iso_forage_sage_se.png | 80x72 | 40 / 24 | 47 | UYARI (fit x1,15, palet %2) | geçti |
| iso_forage_mint_se.png | 80x72 | 40 / 24 | 41 | UYARI (fit x1,15, %57) | geçti |
| icon_item_tea_rosehip.png | 32x32 | 16 / 0 | - | UYARI (arayuz kilidi yok, %30) | - |

Yarı saydam piksel 0, gölge pişirilmemiş. Fit öncesi (ham) denetimde ağaç evreleri ve linden/nane doygunluk yüzünden KALDI'ydı; fit sonrası UYARI.

## Teslim EDİLMEYEN (`424k-red/`)
| Dosya | Stil (esya) | Neden |
|---|---|---|
| iso_item_olive_press_se | KALDI (palet dışı, %53, uzaklık 20,4) | pembe-turuncu pagoda gibi; ahşap/taş değil |
| iso_item_brine_crock_se | KALDI (%23, x1,96) | turuncu doygun küp, kapak kırmızı; kil tonu değil |
| iso_item_honey_strainer_se | KALDI (%61, x1,90) | sarı doygun; torba tanınıyor ama palet dışı |
Eşya ailesi kilitli olduğundan fit uygulanmadı; yeniden deneme yok kuralı gereği ikinci deneme yapılmadı. Öneri: bu üçü için "wood/clay/stone, muted brown" vurgulu yeni iş.

## Sorunlar
- Zemin parçası olmadığından döşeme testi yok. Genç ve meyveli zeytinin altında küçük zemin elması var (kilavuz kaynaklı); kod tarafı gerekirse kırpar.
- Ezme yalağında tarife uygun ama ince direk; okunabilir.
- Opak yükseklikler kılavuzdan büyük (ağaçlar ~55-60 px): ölçek kod tarafında ayarlanabilir.
