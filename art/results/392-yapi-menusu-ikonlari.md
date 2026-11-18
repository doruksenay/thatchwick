unity-import: pending
# 392 Yapı menüsü ikonları (kısmi teslim: 12/28)

- Araç: `create_image_pixen` 32x32, no_background, single color outline, low detail, seed 0, her ikon ayrı çağrı (tarifteki gibi). 28 çağrı x 1 = **28 generation**. 11 çağrı ilk gönderimde eşzamanlılık sınırına (10/10) takıldı (ücret düşmedi), bekleyip bir kez yeniden gönderildi.
- `get_balance` sonrası: 4427.9 (hesap ortak, diğer şeritler de harcıyor; bu işin payı 28).
- Yan yana: `art/results/392-yanyana.png` (teslim edilen 12; arayuz ailesinde referans yok). Tüm 28'in etiketli önizlemesi: `art/results/392-onizleme-tum.png`.
- Masaüstü kopyası: yerel.

## Teslim edilen (12): `Assets/Art/Iso/_aday/`, 32x32, pivot x=16, y=0 (alttan, ortalı)
- ui_icon_bgroup_floor.png
- ui_icon_bgroup_garden.png
- ui_icon_bgroup_plot.png
- ui_icon_bgroup_wall.png
- ui_icon_bgroup_water.png
- ui_icon_sgroup_decor.png
- ui_icon_sgroup_furniture.png
- ui_icon_sgroup_household.png
- ui_icon_sgroup_outdoor.png
- ui_icon_top_level_ground.png
- ui_icon_ui_new.png
- ui_icon_ui_search.png

## Teslim EDİLMEYEN (16): model tarifi izlemedi, ikon yerine yaratık/karakter üretti ya da nesne yanlış
Ham dosyalar `art/392-red/`. Tek deneme kuralı gereği yeniden denenmedi.
- tab_build
- tab_items
- tab_inventory
- top_level_basement
- top_level_upper
- top_walls_up
- top_walls_cut
- top_walls_down
- top_roof
- top_eyedropper
- top_sledgehammer
- top_rooms
- top_undo
- ui_lock
- ui_comfort
- ui_filter

## Doğrulama
- Boyut 32x32, yarı saydam piksel 0: 28/28 geçti.
- Renk sayısı (alfa>0): 24-64, hedef ≤12 hepsinde aşıldı ⚠.
- Kapsama ⚠: ui_search tuvali tamamen dolduruyor, bgroup_floor ve ui_new da 24-26 px hedefinin üstünde.
- `style.py check --family arayuz`: kilit kurulmadı (palet uyarısı). GEÇTİ: floor, search; kalanı UYARI. KALDI yok.

## Öneri
- Pixen 32x32 küçük tuvalde tarif edilen nesneyi sıklıkla yaratığa çeviriyor (16/28). Alternatif: 64x64 üretip küçültme ya da `create_image_pro`; 16 eksik ikon için yeni iş.
