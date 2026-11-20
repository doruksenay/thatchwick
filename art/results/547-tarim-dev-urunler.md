unity-import: pending

# 547 Görünür dev ürünler (rapor)

Yan yana sayfaları: tarla [547-yanyana.png](547-yanyana.png), ikonlar [547-ikonlar-yanyana.png](547-ikonlar-yanyana.png) (arayüz ailesinde referans yok). Reddedilen denemeler ve teslim edilmeyen ikonlar: `art/results/547-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- Tarla: `create_map_object`, stil eşleme, 2x2 kılavuz + maske (`footprint_guide.py --w 144 --h 120 --n 2 --m 2 --axis x --height 44`, ham betik çıktıları `art/tmp547/`, commit dışı), `high top-down`, `low detail`, `basic shading`, `single color outline`, 374 açıklama eki. `style.py fit --family bitki` her çıktıya (doygunluk x0,43-0,63). Pivot x 72, pivot y 40 (alttan).
- İkon: `create_image_pixen` 32x32, `no_background`, `single color outline`, `low detail` (316 ile aynı). Ham çıktılar 21-147 renkti; Pillow median-cut ile 16 renge indirildi (dither yok, alfa 0/255).
- Çağrılar: ilk tur 14 + ikinci tur 7 (tepsi: şalgam, turp, pancar tarlası; yüz/şekil: lahana, kabak, kavun, pancar ikonu) = **21 gen** (`cost_max` 28). Lahana tarlası ilk çağrıda hız sınırı hatası aldı (harcama yok), ~1 dk sonra yeniden gönderildi. `get_balance` sonrası: 2061,7 (paralel şeritler dahil).

## Teslim edilenler (`Assets/Art/Iso/_aday/`)
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Renk | Stil denetimi | validate_iso |
|---|---|---|---|---|---|---|
| iso_crop_giant_turnip_2x2_se.png (2. deneme) | 144x120 | 72 / 40 | x51-107, y14-90 | 48 | GEÇTİ | geçti |
| iso_crop_giant_cabbage_2x2_se.png | 144x120 | 72 / 40 | x20-122, y18-88 | 42 | GEÇTİ | geçti |
| iso_crop_giant_pumpkin_2x2_se.png | 144x120 | 72 / 40 | x18-127, y14-100 | 41 | GEÇTİ | geçti |
| iso_crop_giant_melon_2x2_se.png | 144x120 | 72 / 40 | x44-102, y15-79 | 27 | UYARI (palete uzak) | geçti |
| iso_crop_giant_watermelon_2x2_se.png | 144x120 | 72 / 40 | x30-113, y12-94 | 40 | GEÇTİ | geçti |
| iso_crop_giant_radish_2x2_se.png (2. deneme) | 144x120 | 72 / 40 | x48-89, y3-78 | 39 | GEÇTİ | geçti |
| iso_crop_giant_beet_2x2_se.png (2. deneme) | 144x120 | 72 / 40 | x29-111, y13-90 | 51 | GEÇTİ | geçti |
| icon_item_giant_turnip.png | 32x32 | ikon | x2-28 | 15 | UYARI (palete uzak) | geçti |
| icon_item_giant_cabbage.png (2. deneme) | 32x32 | ikon | tam | 15 | UYARI | geçti |
| icon_item_giant_watermelon.png | 32x32 | ikon | x1-30 | 16 | UYARI | geçti |
| icon_item_giant_radish.png | 32x32 | ikon | x1-30 | 15 | UYARI | geçti |
| icon_item_giant_beet.png (2. deneme) | 32x32 | ikon | tam | 16 | GEÇTİ | geçti |

Yarı saydam piksel 0, gölge pişirilmemiş. Bitki zemin parçası olmadığından döşeme testi yok. Karpuz ikonu yan yana sayfasına girmedi (süzgeç hatası), dosya teslim edildi; denetimi yukarıdaki tabloda (UYARI).

## Teslim edilmeyenler
- `icon_item_giant_melon.png` ve `icon_item_giant_pumpkin.png`: şekil tanınıyor (ikinci denemede yüz de yok) ama `style.py check --family arayuz` "KALDI, palet dışı renkler" (%76 ve %75). Arayüz ailesinde referans olmadığı için `fit` kullanılamadı. 16 renkli hâlleri `art/results/547-red/icon16_*_palet_kaldi.png`; istenirse kullanıcı kabul edebilir. Yeni deneme `max_attempts` (2) dolduğu için yapılmadı.

## Sorunlar
- **Tarla ölçüsü:** model maske yüksekliğine (44 px) uymadı. Opak yükseklikler: şalgam 77, lahana 71, kabak 87, kavun 65, karpuz 83, turp 76, pancar 78 px. Karpuz, kavun ve turp 2x2 ayak izinin yalnızca bir kısmını dolduruyor (turp ~41 px geniş); kod tarafı ölçek gerekirse uygular.
- Turp tarlada siyah değil pembe-mor. Kavun tarlasında asma yaprağı yok. İlk denemelerde şalgam, turp ve pancar toprak tepsili çıktı; "floating alone, no soil, no dirt" ekiyle giderildi (ham hâller `547-red/`). İlk deneme lahana/kavun/pancar ikonlarında yüz vardı (yasak), reddedildi.
- Şalgam ve turp ikonlarında toprak izi var (tarifte "half out of the soil").
- Paylaşılan hesapta başka şeritlerin işleri de çalıştığı için `get_balance` farkı 21'den büyük; bu işin kendi çağrıları 21 gen.
