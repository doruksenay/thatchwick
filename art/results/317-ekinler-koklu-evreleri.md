unity-import: pending

# 317 Kök ekinleri evreleri (rapor, 2026-11-20)

Sonuç: **13/15 teslim** (`Assets/Art/Iso/_aday/iso_crop_<ad>_<1-3>_se.png`). Teslim edilmeyen: **havuç 2 ve havuç 3** (iki denemede de tanınmadı; `art/results/317-red/`). Yan yana sayfası: [317-yanyana.png](317-yanyana.png). Ham çıktılar `art/results/317-ham/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`footprint_guide.py --w 80 --h 72 --n 1`, referanssız; evre yüksekliği 10/22/32), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ortak ek ("muted green and earthy colours", "no soil"). Tuval 80x72 (310 ile aynı), pivot x=40, y(alttan)=24. Tarifteki 64x64 yerine 80x72: 1x1 elması 64 px, 310'daki gerekçe.
- 15 ilk çağrı. 3'ü (sarımsak 1, sarımsak 3, pancar 3) hız sınırına takıldı, bekleyip yeniden gönderildi (pancar 3 iki kez).
- İkinci deneme (4 çağrı; açıklamaya "no grass tile / no dirt / only leaves" gibi kısıtlar): havuç 2 (hâlâ toprak bloğu), havuç 3 (hâlâ çim karosu, mor gövde), soğan 3 (ilkinde zemin parçası vardı, ikincide temiz, teslim), sarımsak 2 (ilkinde iki baş çıkmıştı, ikincide yapraklar, teslim).
- **Maliyet:** 19 çağrı. Bakiye 2891,9 (310 sonrası) → 2838,9, ancak diğer şeritler aynı anda harcadığından fark bu işe ayrıştırılamaz. Tahmini harcama 19 (çağrı başı 1, `cost_est` 15, `cost_max` 24). 310'da çağrı başı ~1,5 ölçülmüştü; öyleyse ~28 olabilir (cost_max aşımı olasılığı, kullanıcı bilsin).
- Doygunluk: ham çıktılar x1,2-3,2 çok doygundu; `style.py fit --family bitki` ile aileye indirildi (çarpan x0,36-0,79). Bitki ailesinin kilidi kurulmadı, palet uyumu uyarıdır.

## Dosyalar (pivot x=40, y alttan=24; boyut 80x72)
| Dosya | Opak sınır kutusu (x, y alttan) | Boy px | Renk | Not |
|---|---|---|---|---|
| parsnip_1 | x30-49, y23-48 | 26 | 35 | turp/turpçuk gibi, kökü küçük |
| parsnip_2 | x10-70, y14-61 | 48 | 53 | renk >48 (yalnız rapor) |
| parsnip_3 | x10-67, y8-64 | 57 | 42 | krem kök, iyi |
| carrot_1 | x31-47, y26-49 | 24 | 43 | küçük, iyi |
| carrot_2, carrot_3 | - | - | - | **teslim edilmedi** |
| onion_1 | x27-48, y7-48 | 42 | 28 | pırasa gibi, kabul edilebilir |
| onion_2 | x10-69, y9-62 | 54 | 41 | ⚠ yeşil sivri yapraklar, mısır/kaktüs gibi okunabilir |
| onion_3 | x24-62, y22-69 | 48 | 45 | 2. deneme, temiz |
| garlic_1 | x29-49, y8-49 | 42 | 38 | |
| garlic_2 | x10-68, y9-61 | 53 | 45 | 2. deneme, yaprak demeti (bir sivri sap) |
| garlic_3 | x16-63, y10-64 | 55 | 33 | beyaz baş, iyi |
| beet_1 | x24-56, y20-44 | 25 | 13 | ⚠ iki yaprak, defne yaprağı gibi, damarlar kırmızı |
| beet_2 | x10-69, y14-61 | 48 | 49 | renk >48 (yalnız rapor) |
| beet_3 | x28-51, y23-60 | 38 | 43 | mor pancar, iyi |

(y aralıkları alttan; boy opak sınır kutusu yüksekliği.)

## Doğrulama
- `validate_iso.py --kind object --max-colors 48 --footprint 8,32,64,32`: 11 geçti; parsnip_2 (53 renk) ve beet_2 (49 renk) yalnız renk sayısında aştı (tarif: fazlaysa raporla). Yarı saydam 0, alt satıra değme yok (alt boşluk 7-26 satır).
- `style.py check --family bitki` (fit sonrası): garlic_3 ve onion_3 GEÇTİ; diğer 11 UYARI (palet dışı, kilit yok; doygunluk x1,09-1,23). Ham hâller doygunluktan KALDI, fit ile düzeltildi.
- **Evre boyu sırası:** parsnip 26<48<57 ✓, garlic 42<53<55 ✓. **onion 42<54>48 ✗** (olgun evre bulb ekranda ikinci evreden kısa; yapraklar uzun), **beet 25<48>38 ✗** (olgun evre yapraksız tek kök). Havuçta yalnız evre 1 var.
- Aynı bitkinin evreleri arasında taban hizası: pivot sabit; ayrıntılı taban ölçümü yapılmadı. Doku/ürün ikonu (316) ve 310 ekinleriyle yan yana ayrım kıyası yapılmadı; havuç ↔ yaban havucu karşılaştırılamadı (havuç 2-3 yok).
- Döşeme testi: zemin değil, yapılmadı. Gölge pişirilmemiş, ışık sol-üstten (gözle).

## Sorunlar
- Havuç 2 ve 3 teslim edilmedi: model toprak bloğu ve çim karosu çıkarıyor ("no soil" tutmuyor). Öneri: ayrı iş, kılavuz yerine daha küçük maske ya da referans karo.
- Soğan 3 ve pancar 3 evre boyu ikinci evreden kısa; karar kullanıcıda.
- Araçta seed yok; ikinci deneme yalnızca açıklama kısıtlarıyla yapıldı.
