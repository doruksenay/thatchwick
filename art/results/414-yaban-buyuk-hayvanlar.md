unity-import: pending

# 414 Yaban büyük hayvanlar: 1/5 teslim (tilki), review

Not: iş dosyası `414-yaban-buyuk-hayvanlar.md`, ama içindeki `id` alanı "410" (410-kervan ile çakışıyor; kod oturumu düzeltmeli). Bu iş 410 numaralı görevle sahiplenildi.
Yan yana sayfası (tilki SE, S, NE; referanslar koyun ve domuz): [414-yanyana.png](414-yanyana.png). Teslim edilmeyen tek kareler: `art/results/414-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
375 ile aynı: `create_image_pixflux`, `high top-down`, black outline, `direction=south`, `no_background`, medium detail, basic shading, palet zorlaması `iso_animal_sheep_1_cand_se.png` (10 renk şerit), açıklama eki `..., desaturated`. Tuvaller: karaca 48, tilki 38, kurt 44, domuz 44, ayı 56. Döndürme: `create_character` v3, quadruped, template dog (yalnızca tilki), referans = pixflux çıktısı. `style.py fit` her yönde x1.00 (değişiklik yok).

## Tek kare kontrolü
| Tür | Tuval | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|---|
| roe_deer | 48 | yandan/3/4, gövde kayık | yok | döndürülmedi |
| fox | 38 | önden, oturan | yok | döndürüldü, teslim |
| wolf | 44 | 3/4 yandan, gövde kayık | yok | döndürülmedi |
| boar | 44 | önden | VAR (açık gri disk) | döndürülmedi |
| bear | 56 | önden | VAR (açık gri disk) | döndürülmedi |

Öneri: karaca ve kurt için "seen straight from the front, head-on, symmetrical" vurgusu; domuz ve ayı için daha güçlü "no ground disc" ya da palet zorlamasız deneme (375'te de aynı zemin lekesi). Yeni iş gerekir.

## Generation
pixflux x5 = 5; v3 döndürme x1 (tilki) = 1; toplam 6 (cost_max 18). `get_balance` sonrası: 3092,9 (paralel şeritler dahil).

## Dosyalar (`Assets/Art/Iso/_aday/iso_wild_fox_cand_<yön>.png`, 40x40)
| Yön | Opak kutu | Renk | Yarı saydam | Pivot x | Pivot y (alttan) |
|---|---|---|---|---|---|
| s | x9-38, y2-38 | 10 | 0 | 21,5 | 2 |
| se | x11-29, y2-38 | 17 | 0 | 20,0 | 2 |
| sw | x8-38, y2-38 | 16 | 0 | 21,1 | 2 |
| w | x7-38, y2-38 | 16 | 0 | 22,5 | 2 |
| nw | x10-28, y2-38 | 13 | 0 | 18,5 | 2 |
| n | x3-29, y2-38 | 12 | 0 | 16,5 | 2 |
| ne | x2-32, y2-38 | 16 | 0 | 17,4 | 2 |
| e | x2-31, y2-38 | 16 | 0 | 17,9 | 2 |

Pivot y yönler arası oynamıyor. Renk 32'nin altı, azaltma gerekmedi.

## Doğrulama
`validate_iso.py --kind object` SE: geçti. Boy 36 px (tuval 40, doluluk 0,9; tilki oturan poz). Gölge pişirilmemiş, ışık sol-üstten, zemin lekesi yok.

### Stil denetimi (`check --family hayvan`)
Hayvan ailesi kilidi kurulmadı; palet uyumu uyarı. 8 yönün hepsi UYARI (ailesinden soluk, x0,32-0,50), KALDI yok.

## Sorunlar
- 4/5 tür teslim edilmedi (yukarıda); yeni iş gerekir.
- Tilki oturuyor (v3 şablon dog); 8 yön tutarlı.
- Çift `id: "410"` (iş dosyası 414).
