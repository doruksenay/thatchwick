unity-import: pending

# 468b Karakter yaratma ekranı ikonları (12)

- Araç: `create_map_object` 32x32 x12 (side, single color outline, basic shading, medium detail; 407y yöntemi). Hız sınırı hatası (429): 3 çağrı 15-30 sn sonra bir kez yeniden gönderildi.
- İkinci deneme (`create_image_pixflux` 32x32 no_background) x3: `adorn` (yalnızca küpe çıkmıştı), `family` (eller koyu, okunmuyordu), `night` (mum yoktu). Ham ilk sürümler teslim edilmedi.
- Harcanan: 15 generation (12 + 3). Kalan (get_balance, işten sonra): 2376,9 (paylaşılan hesap, başka şeritler de harcıyor).
- Hepsi yerel `quantize(16)` ile 16 renge indirildi, alfa eşiği 128 (yarı saydam 0). `reduce_colors` çağrılmadı.
- Yan yana: `art/results/468b-yanyana.png` (12 yeni + `ui_icon_ui_lock` + `ui_icon_top_undo`; üst iki satır koyu, alt iki satır keten zeminde, x3).
- Masaüstü kopyası: yerel. `Assets/Resources/UI/Icons/` kopyası yapılmadı (`cloud_guard` bu yolu reddeder); kabulden sonra yerel işçi ya da kod oturumu `_aday/`dan kopyalar.

| Dosya | Boyut | Pivot x | Pivot y (alttan) | Renk | Stil denetimi | Not |
|---|---|---|---|---|---|---|
| ui_icon_cc_person | 32x32 | 16 | 0 | 7 | GEÇTİ | |
| ui_icon_cc_face | 32x32 | 16 | 0 | 15 | GEÇTİ | kapüşonlu keten maske |
| ui_icon_cc_hair | 32x32 | 16 | 0 | 15 | GEÇTİ | tarak ve saç lülesi |
| ui_icon_cc_clothes | 32x32 | 16 | 0 | 11 | UYARI (palet dışı; kilit yok) | kırmızı şeritli tunik |
| ui_icon_cc_adorn | 32x32 | 16 | 0 | 14 | UYARI (palete uzak) | pixflux 2. deneme; boncuk kolye, küpe yok |
| ui_icon_cc_voice | 32x32 | 16 | 0 | 15 | GEÇTİ | çan; ses çizgileri belirgin değil |
| ui_icon_cc_family | 32x32 | 16 | 0 | 7 | UYARI (palete uzak) | pixflux 2. deneme; iki el AYRI duruyor, kavuşmuyor |
| ui_icon_cc_dice | 32x32 | 16 | 0 | 13 | GEÇTİ | beş benek net okunmuyor |
| ui_icon_cc_redo | 32x32 | 16 | 0 | 15 | GEÇTİ | sağa dönen ok; `top_undo` ile tam eş değil |
| ui_icon_cc_portrait | 32x32 | 16 | 0 | 15 | GEÇTİ | |
| ui_icon_cc_elder | 32x32 | 16 | 0 | 7 | GEÇTİ | ince ve küçük; keten zeminde okunur |
| ui_icon_cc_night | 32x32 | 16 | 0 | 10 | UYARI (palete uzak) | pixflux 2. deneme; yanan mum, AY YOK |

Doğrulama: 32x32, yarı saydam 0, ≤16 renk; `validate_iso --kind object` örnek ikonda geçti (betik tek dosya alıyor, 12'sinin hepsi çalıştırılmadı); stil denetiminde KALDI yok. Göz kontrolü koyu ve keten zeminde yapıldı.

## Sorunlar
- `family` eller kavuşmuyor, `night` ayda yok, `dice` beş beneği net vermiyor. Gerekirse bu üçü için yeniden iş istenir (kalan cost_max 11).
- `arayuz` stil kilidi kurulmadı; UYARI'lar bundan.
- `tools/art`'a dokunulmadı. Öneri: `validate_iso.py` birden çok dosya almıyor; toplu çağrı desteği işe yarar.
