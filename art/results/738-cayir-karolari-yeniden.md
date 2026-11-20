unity-import: pending

# 738 Çayır karoları yeniden (ana çimle aynı ton)

İş dosyası: `art/queue/738-cayir-karolari-yeniden.md` (aynı numarayı taşıyan ikinci iş `738-kavga-ve-sohbet-pozlari-yasli` bu turda işlenmedi; numara çakışması aşağıda).

## Araç ve ayarlar
- `create_tiles_pro`, stil modu (style_images: `iso_ground_grass_a/b/c`, base64 satır içi), isometric, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`, 64x32. 004e/612 ayarları, aynalama ve döndürme yok.
- Tek çağrı, 16 karo döndü (tile id `70d0cd59-186c-451d-8397-b54f47698c30`), 10 generation (araç açıklaması ve `cost_est` ile uyumlu). Bakiye: 9808 -> 9752 (aradaki fark diğer şeritlerin harcaması; bu işin payı 10).
- 16 karodan, çim tonu referansa en yakın ve çiçeği okunan 3'ü seçildi: a = tile_0 (beyaz çiçek), b = tile_3 (pale sarı), c = tile_12 (beyaz). Yeniden boyama yok, `style.py fit` gerekmedi.

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/iso_ground_meadow_v2_a.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_ground_meadow_v2_b.png | 64x32 | 32 | 16 |
| Assets/Art/Iso/iso_ground_meadow_v2_c.png | 64x32 | 32 | 16 |

Yan yana sayfası (referanslarla): `art/results/738-yanyana.png`
Döşeme testi (6x6, ortada 3x3 çayır): yeni `art/results/738-doseme-v2.png`, karşılaştırma için 612'nin eski çayırı `art/results/738-doseme-eski.png`.

## Doğrulama
| Ölçüt | a | b | c | Sonuç |
|---|---|---|---|---|
| validate_iso --kind ground (opak 1024, maske dışı 0, boşluk 0) | geçti | geçti | geçti | GEÇTİ |
| Çim ortalaması (R,G,B); referans grass_a (115,4; 178,8; 95,0) | 116,7; 177,2; 97,2 | 116,4; 176,8; 96,1 | 116,4; 176,1; 97,3 | GEÇTİ (kanal farkı en çok 2,7, sınır ±5) |
| Kenar/iç parlaklık oranı (0,97-1,03) | 0,996 | 1,001 | 0,997 | GEÇTİ |
| style.py check --family zemin | GEÇTİ, doygunluk x1,02 | GEÇTİ, x1,01 | GEÇTİ, x1,00 | GEÇTİ (UYARI yok) |
| Gölge pişirilmemiş, ışık sol-üst | evet | evet | evet | GEÇTİ |

Eski çayır ortalaması (106,6; 139,0; 74,0) idi; yeşil kanal -39 sapıyordu, şimdi -1,6 ile -2,7.
Döşeme testinde 3x3 çayır öbeğinin kenarı ızgara gibi seçilmiyor, yalnızca çiçekler fark ediliyor (eski çayırlı sayfada öbek koyu yama olarak görülür).

## Sorunlar / notlar
- `art/queue/` altında iki dosya `id: "738"` taşıyor (`738-cayir-karolari-yeniden` ve `738-kavga-ve-sohbet-pozlari-yasli`). İşlenen çayır işidir (ART_JOB ortamı okunamadı, ama son sahiplenme commit'i `b68e6c05` ve `claimed_at 2026-11-20T23:54Z` bunu gösteriyor). Yaşlı pozları işi `running` kaldı (claimed_at 20:44Z, 3 saatten eski): sahipsiz olabilir, kullanıcı/kod oturumu numarayı çözmeli (ör. birine yeni numara).
- Kod işi (kod oturumu): `IsoArt.MeadowTiles` `iso_ground_meadow_v2_a/b/c`'yi kullansın.
- Masaüstü kopyası: yerel.
