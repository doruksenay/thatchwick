unity-import: pending

# 600 Mutfak, pişirme kapları (rapor)

Yan yana sayfası: [600-yanyana.png](600-yanyana.png) (12 yeni + 7 referans). Ham/elenen hâller: `art/results/600-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (Python ile `art/tmp600/gen.py`; taban elması alt kenardan 4 px yukarıda, maske yükseklik kadar yukarı + 1 px), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki (koyu kahverengi ahşap ibaresi metal/kil kapları için çıkarıldı). 12 çağrı x 1 gen + 1 ikinci deneme (kil tepsi: ilk çıktı kırmızımsı, topaç/koni gibi, tanınmadı) = 13 generation (cost_max 24). Kılavuz tuvalleri: tencere/kazan 48x48, sac 48x40, tava 56x40, güveçler 56x44, şiş takımları 64x48, tepsiler 48x36, sacayağı 40x36.
`get_balance` iş sonu: 1835,7 (paralel şeritler de harcıyor; bu işin payı 13).

## Sonuç özeti
| Eşya | Durum | Not |
|---|---|---|
| pot_clay | teslim | GEÇTİ |
| cauldron_iron | teslim (UYARI) | ailesinden soluk (x0.41, siyah demir) |
| griddle_iron | teslim (UYARI) | soluk (x0.47); düz koyu disk, halkalar zor seçilir |
| pan_copper | teslim | GEÇTİ; sap küçük pencerede zor seçilir |
| guvec_plain | teslim (UYARI) | ham KALDI (palet %36, x1.77 kırmızı); `fit` ile doygunluk x0.65, stil GEÇTİ; opak renk 61 > 48 (301 kuralı: azaltılmadı, yalnızca raporlandı), validate_iso renk sınırında BAŞARISIZ |
| guvec_glazed | teslim | GEÇTİ |
| guvec_fine | teslim (UYARI) | palete uzak (12.6); mavi-beyaz desen okunuyor |
| spit_iron | teslim | GEÇTİ; iki çatal ayak okunuyor, şişler ince |
| spit_steel | teslim (UYARI) | soluk (x0.57) |
| baking_tray_clay | teslim (2. deneme, UYARI) | 1. deneme koni/topaç biçimli (`600-red/`); 2. deneme tepsi gibi, ailesinden doygun x1.33 |
| baking_tray_copper | teslim (UYARI) | palete uzak, doygun x1.50 |
| trivet_iron | teslim (UYARI) | soluk (x0.48), morumsu; halka ince |

Güveç glazed/fine/plain aynı kılavuzdan, siluetleri aynı (opak 1044 px).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_pot_clay_se.png | 48x48 | 24 | 11 |
| iso_item_cauldron_iron_se.png | 48x48 | 24 | 11 |
| iso_item_griddle_iron_se.png | 48x40 | 24 | 11 |
| iso_item_pan_copper_se.png | 56x40 | 28 | 12 |
| iso_item_guvec_plain_se.png | 56x44 | 28 | 12 |
| iso_item_guvec_glazed_se.png | 56x44 | 28 | 12 |
| iso_item_guvec_fine_se.png | 56x44 | 28 | 12 |
| iso_item_spit_iron_se.png | 64x48 | 32 | 14 |
| iso_item_spit_steel_se.png | 64x48 | 32 | 14 |
| iso_item_baking_tray_clay_se.png | 48x36 | 24 | 12 |
| iso_item_baking_tray_copper_se.png | 48x36 | 24 | 12 |
| iso_item_trivet_iron_se.png | 40x36 | 20 | 10 |

Pivot, ayak izi elmasının merkezidir (referans `copper_pot`: 40x40, pivot 20,10 ile uyumlu).

## Doğrulama (validate_iso, `--max-colors 48`)
Hepsi geçti (opak sınır kutusu tuval içinde, yarı saydam 0, alta değme yok); tek istisna guvec_plain: 61 renk. Renk sayıları: tray_clay 10, tray_copper 7, cauldron 16, griddle 6, guvec_fine 35, guvec_glazed 35, guvec_plain 61, pan 9, pot 12, spit_iron 33, spit_steel 22, trivet 13.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Sonuç | Not |
|---|---|---|
| pot_clay, pan_copper, spit_iron, guvec_glazed, guvec_plain (fit) | GEÇTİ | |
| baking_tray_clay | UYARI | doygun x1.33 |
| baking_tray_copper | UYARI | palete uzak, x1.50 |
| cauldron_iron, griddle_iron, spit_steel, trivet_iron | UYARI | soluk (x0.41-0.57) |
| guvec_fine | UYARI | palete uzak 12.6 |

## Sorunlar / öneriler
- Işık/gölge: kullanıcı yan yana sayfasında sol-üst ışığı kontrol etmeli (alev yok).
- guvec_plain renk sayısı 61: kullanıcı kabul ederse kod oturumu `reduce_colors` uygulayabilir.
- `footprint_guide.py` küçük (32-64 px) eşyaları desteklemiyor; öneri: `--base-w` seçeneği.
- Bakır kazan yeniden üretilmedi (`iso_item_copper_pot_se` var).
