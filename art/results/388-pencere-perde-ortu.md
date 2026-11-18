unity-import: pending

# 388 Pencere içi, perdeler, örtüler ve süsler (rapor): 10/13 teslim, review

Yan yana sayfası: [388-yanyana.png](388-yanyana.png). Teslim edilmeyenler: `art/results/388-red/` (ham ve fit hâlleri). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object` stil eşleme, referanssız kılavuz + maske (301/306/381 yöntemi). Duvar eşyaları `side`, zemin eşyaları `high top-down`; hepsi `low detail`, `basic shading`, `single color outline`; açıklama + takım eki + (duvar eki) + ortak ek (tekstilde "dark brown wood" çıkarıldı). 13 çağrı x 1 gen = **13 generation**. Tek deneme; çelenk çağrısı iki kez "base64 kesildi" hatasıyla üretim yapmadan reddedildi (harcama yok, kılavuz base64'ü kısaltılıp gönderildi).
- Kılavuzlar `art/tmp388/` (commit edilmez): duvar eşyaları "/" eğimli paralelkenar (duvar rengi 184,160,122), örtüler `footprint_guide.py --n 2 --axis x --height 3` (112x64), küçük eşyalar 32x32 tuvalde taban elması (PixelLab en az 32 px istiyor) ve sonra istenen tuvale kırpıldı (maske dışı silinen piksel 0, yalnızca tuval kırpması: sill 48x24, heybe 24x32, yastık 32x24, ibrik 20x32).
- **Doygunluk:** 13 çıktının 10'u ailesinden çok doygundu. `style.py fit --family esya` ile (x0,62–x0,89) indirildi, teslim edilenler fit hâlleridir (ibrik ve heybe x1,00, değişmedi).
- get_balance sonrası: 4607,9 / 5000 (paralel şeritler de harcıyor).

## Dosyalar (`Assets/Art/Iso/_aday/`; duvar eşyası pivotu = merkez, ALTTAN)
| Dosya | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_wall_item_window_sill_board_sw_ne.png | 48x24 | 24 | 12 |
| iso_wall_item_linen_curtain_sw_ne.png | 48x56 | 24 | 28 |
| iso_wall_item_wool_curtain_sw_ne.png | 48x56 | 24 | 28 |
| iso_wall_item_harvest_wreath_sw_ne.png | 40x40 | 20 | 20 |
| iso_wall_item_embroidery_panel_sw_ne.png | 40x48 | 20 | 24 |
| iso_item_tablecloth_linen_se.png | 112x64 | 56 | 32 |
| iso_item_tablecloth_embroidered_se.png | 112x64 | 56 | 32 |
| iso_item_table_carpet_se.png | 112x64 | 56 | 32 |
| iso_item_heybe_se.png | 24x32 | 12 | 4 (en alt opak satırın hizası) |
| iso_item_copper_ewer_se.png | 20x32 | 10 | 3 |

Teslim edilmedi: `iso_item_big_cushion_se` (KALDI, palet dışı renkler, %74), `iso_item_sedir_cover_kilim_se` (fit sonrası da KALDI, %76 palet), `iso_wall_item_kilim_curtain_sw_ne` (fit sonrası KALDI, %50 palet; kırmızı-çivit bantları paleti aşıyor).

## Doğrulama (`validate_iso.py --max-colors 48`: 10/10 geçti; yarı saydam 0)
| Dosya | Renk | Stil denetimi (fit sonrası) |
|---|---|---|
| sill board | 11 | UYARI (%100, 5.9, x1.28 ailesinden doygun) |
| linen curtain | ≤41 | GEÇTİ (%100, 7.5, x1.22) |
| wool curtain | ≤25 | GEÇTİ (%100, 9.8, x1.15) |
| harvest wreath | ≤27 | GEÇTİ (%97, 7.0, x1.15) |
| embroidery panel | 11 | GEÇTİ (%100, 7.5, x1.16) |
| tablecloth linen | 20 | GEÇTİ (%100, 6.0, x1.16) |
| tablecloth embroidered | 29 | UYARI (%97, 10.5, palete uzak) |
| table carpet | 15 | UYARI (%100, 10.9, palete uzak) |
| heybe | 7 | GEÇTİ (%100, 8.8, x0.78) |
| copper ewer | 12 | GEÇTİ (%100, 7.4, x1.10) |

## Sorunlar / gözlem
- Döşeme testi örtülere uygulanmadı (zemin değil, 2x1 üst eşya).
- Örtüler 112x64 tuvalde 96x48 elmas dolduruyor; kod eşyanın üstündeki karolara ölçekleyecek (tarif).
- Heybe kancada asılı değil, 24x32 taban elmasında duruyor; ibrik küçük (20x32'de ~16 px gövde). Gözle bakılmalı.
- Doygunluk düzeltmesi (fit) renkleri soldurdu; masa halısı ve sedir kilimi canlılığını kısmen kaybetti.
- Yeniden üretim önerisi (kullanıcı kararıyla): yastık, sedir örtüsü, kilim perde; tarife "muted ochre, faded red, no bright colours" eklenerek.
- Pencere açıklığı (330) ölçüsüne göre doğrudan örtüşme denenmedi; pivot ve yerleşim kod/kullanıcı tarafında.
- `tools/art`'a duvar ve küçük eşya kılavuzu betiği eklenebilir (`art/tmp388/mk.py`).
