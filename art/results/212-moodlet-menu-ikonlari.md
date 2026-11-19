unity-import: pending

# 212 Moodlet, menü ve panel ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/212-yanyana.png` (arayuz ailesinin kabul referansı yok; yalnızca 8 örnek yeni ikon, `style.py sheet` bu kadar kabul etti). Toplu önizleme x4: `art/results/212-ikonlar-x4.png` (parşömen zemin), `art/results/212-ikonlar-x4-koyu.png` (#333333 zemin).

## Araç ve ayarlar
- `create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; ikon başına 1 çağrı = **31 gen** (cost_est 31, cost_max 40 içinde). Gerekçe: tarif kararı (32 tek tek, sonra 16'ya indirme).
- 32x32 ham çıktı alfa ikili hâle getirilip `_32` ekiyle konuldu. 16x16: en yakın komşu indirme, sonra **yerel Pillow ile 12 renge** kuantizasyon (mediancut, dither yok). PixelLab `reduce_colors` çağrılmadı (31 x 0,1 gen tasarruf); sonuç 10-12 renk.
- Hız sınırı: PixelLab 10 eşzamanlı iş sınırı (diğer şeritlerle paylaşılıyor) 14 çağrıyı reddetti (harcama yok); işler bitince aynı çağrılar bir kez yeniden gönderildi.
- Yön: 210'da `yon:` yok, varsayılan `parsomen`. Stil eşlemesi yok (arayuz ailesinin referansı yok).
- `get_balance` sonrası: 3580,5 (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`; 16x16, pivot x 8, pivot y 0 alttan; ikon, pivot önemsiz; her biri ayrıca `_32` 32x32)
ui_icon_moodlet_{good_meal,rested,clean,inspired,had_fun,nice_chat,newlywed,new_baby,grief,harmony,bright_room,good_harvest,proud,pet_love,animal_died,crop_frozen,sharp_tools,slaughtered,injured,gift,festival}, ui_icon_menu_{build,family,village,market}, ui_icon_ui_{close,play,diamond,dice,undo} (16 ve _32), ui_icon_ui_lock_32 (yalnızca 32).
- **ui_icon_ui_lock.png (16x16) teslim edilmedi**: `_aday/` altında 399a'dan zaten 32x32 bir `ui_icon_ui_lock.png` var, üzerine yazılmaz. Yeni 16x16 kilit commit edilmedi; kullanıcı isterse farklı adla eklenir.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 16x16 (30 dosya) + 31 adet 32x32 `_32` |
| Yarı saydam piksel | 0 (alfa 0/255) |
| Renk (opak) | 10-12, hepsi ≤12 |
| `validate_iso.py` | 2:1 zemin/duvar denetimi ikonlara uygulanmaz; atlandı |
| Stil denetimi | KALDI yok; 2 GEÇTİ (menu_build, new_baby), diğerleri UYARI (kilit kurulmadı, palet uyumu) |
| Okunurluk (3x, koyu/açık) | Göz kontrolü: çoğu okunur (damla, elmas, çarpı, üçgen, kese, kandil, buğday, ev, bıçak). Zayıf: had_fun ve ui_dice (zar belirsiz), menu_family (yüzler detaylı), harmony (tüm tuvali dolduruyor), ui_undo, rested, nice_chat |

Stil denetimi tablosu (kilit dahil 31 ikon):

Not: 'arayuz' ailesinin kilidi kurulmadı (kabul referans < 3); palet uyumu yalnızca uyarı.
| Dosya | Palete yakın | Ortalama uzaklık | Doygunluk / aile | Sonuç | Bulgu |
|---|---|---|---|---|---|
| ui_icon_menu_build.png | %83 | 9.5 | - | GEÇTİ |  |
| ui_icon_menu_family.png | %87 | 14.6 | - | UYARI | palete uzak |
| ui_icon_menu_market.png | %96 | 14.2 | - | UYARI | palete uzak |
| ui_icon_menu_village.png | %72 | 15.3 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_animal_died.png | %64 | 11.4 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_bright_room.png | %56 | 13.8 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_clean.png | %27 | 49.2 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_crop_frozen.png | %49 | 26.1 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_festival.png | %48 | 29.6 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_gift.png | %39 | 29.2 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_good_harvest.png | %67 | 14.6 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_good_meal.png | %81 | 11.3 | - | UYARI | palete uzak |
| ui_icon_moodlet_grief.png | %80 | 19.7 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_had_fun.png | %35 | 26.9 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_harmony.png | %71 | 23.0 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_injured.png | %85 | 11.6 | - | UYARI | palete uzak |
| ui_icon_moodlet_inspired.png | %58 | 16.5 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_new_baby.png | %100 | 7.4 | - | GEÇTİ |  |
| ui_icon_moodlet_newlywed.png | %64 | 16.5 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_nice_chat.png | %61 | 23.0 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_pet_love.png | %100 | 11.0 | - | UYARI | palete uzak |
| ui_icon_moodlet_proud.png | %65 | 17.6 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_rested.png | %40 | 22.8 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_sharp_tools.png | %32 | 19.9 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_moodlet_slaughtered.png | %75 | 18.8 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_ui_close.png | %79 | 12.7 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_ui_diamond.png | %30 | 38.2 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_ui_dice.png | %55 | 22.7 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_ui_lock.png | %48 | 20.1 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_ui_play.png | %59 | 21.5 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |
| ui_icon_ui_undo.png | %63 | 14.7 | - | UYARI | palet dışı renkler (kilit kurulmadı: uyarı) |


## Sorunlar
1. `ui_mockup.py mockup durum` önizlemesi (212-durum-onizleme-x2.png) üretilmedi: moodlet kartlarına ikon bağlayan düzen bu işte yok; yan yana ve x4 sayfaları sunuldu.
2. 16'ya komşu indirme ince ayrıntıyı kaybediyor; zayıf ikonlar için `_32` sürümleri 32x32 arayüzde kullanılabilir ya da yeniden üretilir.
3. ui_lock adı çakışması (yukarıda).
