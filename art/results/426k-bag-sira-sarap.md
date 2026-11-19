unity-import: pending

# 426k Üzüm asması evreleri, üzüm teknesi ve bağ ürünleri ikonları (durum: review)

Yan yana sayfası: [426k-yanyana.png](426k-yanyana.png) (7 esya referansı + 9 yeni; asma evreleri fit sonrası hâlleriyle). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 9 x `create_map_object` basic (kılavuz/maske yok), `high top-down`, `low detail`, `basic shading`, `single color outline`. Asmalar 64x64 / 64x80, tekne 64x64, ikonlar 32x32 (423k yöntemi). Gerekçe: tek deneme, kılavuzsuz basic mod en ucuz yol; iso ayak izi 2:1 kılavuzla zorlanmadı. 9 çağrı x 1 gen = **9 gen** (cost_est 11, cost_max 22). Hız sınırı hatası yok.
- Asma young ve fruit'e `style.py fit --family bitki` uygulandı (doygunluk x0,63 ve x0,67 ile; ham hâller `426k-red/*_ham.png`).
- `get_balance` sonrası: **3509,9** (paralel şeritler dahil).

## Dosyalar (`Assets/Art/Iso/_aday/`; pivot x merkez, y alttan)
| Dosya | Boyut | Pivot x / y | Renk | Stil denetimi | validate_iso |
|---|---|---|---|---|---|
| iso_plant_grape_young_se.png | 64x64 | 32 / 11 | 19 (ham) | UYARI (fit sonrası; kilit yok) | geçti |
| iso_plant_grape_grown_se.png | 64x80 | 32 / 12 | 17 | UYARI (x1,23) | geçti |
| iso_plant_grape_fruit_se.png | 64x80 | 32 / 9 | 31 (ham) | UYARI (fit sonrası; kilit yok) | geçti |
| icon_item_sapling_grape.png | 32x32 | 16 / 0 | 13 | UYARI | - |
| icon_item_grape.png | 32x32 | 16 / 0 | 9 | UYARI | - |
| icon_item_must.png | 32x32 | 16 / 0 | 14 | UYARI | - |
| icon_item_wine_new.png | 32x32 | 16 / 0 | 11 | UYARI | - |
| icon_item_wine_aged.png | 32x32 | 16 / 0 | 13 | UYARI | - |

Yarı saydam piksel 0; gölge pişirilmemiş; yüz/göz/figür yok; cam yok. Zemin parçası olmadığından döşeme testi yok.

## Teslim edilmeyen
- **iso_item_grape_tub_se.png** (64x64): `style.py check --family esya` KALDI (palet dışı, ailesinden doygun x1,25; esya kilitli olduğundan `fit` uygulanmaz). Görsel olarak tanınıyor (üzümlü ahşap tekne); kullanıcı isterse el ile kabul edebilir: `art/results/426k-red/`.

## Sorunlar
- **young** asma palmiye/genç ağaç gibi okunuyor (uzun gövde, tepede yaprak), çatal destekli asma değil; tanınırlığı zayıf. Yükseklik hedefi (371-374 ölçeği) kılavuzsuz ölçülmedi.
- grown tek gövdeli yeşil yapraklı sırık gibi; fruit'te salkım gövdeye göre çok büyük. Üç evrenin ölçeği tutarsız (evre dizisi için kılavuzlu yeniden iş önerilir).
- İkonlar sade ve tanınıyor; wine_new/aged ayrımı küçük (amphora vs sürahi).
- Palet uyarıları kilit kurulmadığı için bilgi.
- Öneri: kılavuz+maskeli (`footprint_guide.py`) yeni iş ile asma evreleri ve tekne yeniden.
