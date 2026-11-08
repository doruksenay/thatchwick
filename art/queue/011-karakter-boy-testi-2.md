---
id: "011"
title: Karakter boy testi 2 (K1 tekrar, hedef boylar)
status: review
approved_by: user        # kullanıcı onayı 2026-11-08 (sohbet, seçenek b)
cost_est: 6.3
cost_max: 8
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_char_farmer_a2_48_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_farmer_a2_56_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_farmer_a2_64_cand_se.png
created: 2026-11-08
---
# 011 Karakter boy testi 2 (K1 tekrar)

## Amaç
010'da v3 tuvali neredeyse tamamen doldurdu: 72/84/96 tuvalde gövde boyları 70/81/86 çıktı, hedef 48/56/64'tü. Bu iş boy testini hedef boylarda tekrarlar, renk sayısını 32'ye indirir ve karşılaştırma önizlemesi üretir. Kullanıcı bunun üzerinden boyu seçecek; K2 (8 karakter) bu karara bağlı. Referans: `art-brief-karakter.md` Faz K1, `art/results/010-karakter-boy-testi.md`.

## Tarif

### 1. Üretim (6 gen)
- Araç: `create_character`, `mode=v3`, `body_type=humanoid`, `view=high top-down`, `outline=single color black outline`, `detail=high detail`.
- Üç çağrı, sırayla:
  - `size=52`: hedef gövde 48 px, 2 gen
  - `size=60`: hedef 56 px, 2 gen
  - `size=68`: hedef 64 px, 2 gen
- `name=farmer_a2_<boy>`.
- Açıklama: 010 ile birebir aynı metin. Tutarlılık için üçünde de değiştirmeden kullan:
  "adult male medieval peasant villager, standing idle, yellow-brown wool tunic with collar, folds and cuffs, leather belt with buckle and small pouch, wrapped cloth leg bindings, laced leather shoes, dark brown short hair with visible strands, light stubble beard, fair skin, readable face with brows nose and mouth line, earthy muted palette, clean pixel art, crisp black outline, no dithering, no noise, no cast shadow, transparent background"
- **Referansla üretim yok.** v3 referans modu verilen sprite'ı olduğu gibi döndürür, yeniden çizmez. Ayrıca referansın güneye (S) bakması gerekir; SE verilirse tüm yön etiketleri 45° kayar. Bu yüzden 48'lik karakter referans verilirse 56 ve 64 aynı 48 px'lik karakterin dolgulu kopyası olur. Ölçeklemek de yönergeye aykırı. Üç boy da metinden üretilir; tutarlılık aynı açıklama metniyle sağlanır.
- Eşzamanlı iş sınırı çıkarsa (çağrı reddedilir, ücret yazılmaz) 30 saniye bekleyip bir kez daha dene; ret sürerse dur.

### 2. Ham çıktıyı sakla
- Her boyun 8 yönünü önce `Assets/Art/Iso/_aday/_ham/iso_char_farmer_a2_<boy>_cand_<yön>.png` olarak kaydet.
- Yön eşlemesi: south=s, south-east=se, east=e, north-east=ne, north=n, north-west=nw, west=w, south-west=sw.

### 3. Renk azaltma (3 × 0,1 gen)
- Araç: `reduce_colors`. Her boy için **bir çağrı**, o boyun 8 yönü birlikte (ortak palet).
- Ayarlar: `num_colors=32`, `dithering=none`.
- Sonucu `Assets/Art/Iso/_aday/iso_char_farmer_a2_<boy>_cand_<yön>.png` olarak kaydet.
- Saydamlığın korunduğunu kontrol et: alfa kanalı ham çıktıyla aynı olmalı, yarı saydam piksel 0. Bozulduysa nedenini yaz, elle düzeltme.

### 4. Önizleme (betikle, gen yok)
- Her boy için `_onizleme_x4.png`. Koyu gri (#333333) zemin, en yakın komşu ile 4 kat büyütme. Üst satır:
  1. `Assets/Art/Iso/iso_char_villager_se.png` (salt okunur referans)
  2. adayın renk azaltılmış SE karesi
  3. adayın ham SE karesi
- Her karakter kendi 64x32 elmas konturunun (1 px açık gri çizgi, dolgusuz) üzerinde durur. Ayak izi merkezi (pivot) elmasın merkezine oturur. Karakter konturun önünde çizilir.
- Alt satır: renk azaltılmış 8 yön yan yana.
- Faz toplusu: üç boy ve köylü aynı elmas ızgarasında tek satırda yan yana, 4 kat büyütülmüş. Ad: `2026-11-08_K1b_toplu_x4.png`.
- Önizleme betiğini `tools/art/` altına koyabilirsin (ör. `tools/art/char_preview.py`).

### 5. Masaüstü kopyası (kullanıcının brief'indeki açık istisna, yalnızca bu iş için)
- `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_farmer_a2_<boy>\` klasörüne:
  - 8 yön (renk azaltılmış)
  - `_ham\` alt klasörüne 8 ham yön
  - `_onizleme_x4.png`
- Faz toplusu: `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_K1b_toplu_x4.png`.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna her boy için bir satır ekle. Generation sütununa yalnızca sayı yaz, ölçümleri not sütununa yaz:
  `2026-11-08 | karakter | karakter/2026-11-08_farmer_a2_<boy> | create_character v3 + reduce_colors | <gen> | aday | boy <h> px, <renk> renk`
- Masaüstünde silme ve üzerine yazma yok.

## Doğrulama
- **Ölçüm tablosu:** her boyun SE yönü için hem ham hem renk azaltılmış dosyada ölç:
  - gerçek gövde boyu (opak bbox yüksekliği)
  - genişlik
  - renk sayısı
  - yarı saydam piksel
  - pivot x ve y ALTTAN (010'daki yöntemle)
- **Başarı ölçütü:**
  - Gerçek boy hedefin ±3 px içinde olmalı.
  - Renk ≤32, yarı saydam 0.
  - Tutmazsa tekrar deneme, ölçümü yaz.
- **8 yön tablosu:** her yön için renk sayısı, yarı saydam piksel ve pivot y.
- **Doğrulayıcı:** her SE dosyası için `validate_iso.py <dosya> --kind object --max-colors 32`.
- **Detay:** boylar arası detay farkını 010'daki başlıklarla yaz (yüz, eller, kemer/toka/kese, sargı, ayakkabı bağı, oran). Renk azaltmanın yüz ve aksesuar okunurluğunu bozup bozmadığını da yaz.
- **Tutarlılık:** üç boy arasında tasarım tutarlılığını yaz (tunik rengi, saç, aksesuar).

## Notlar
- Ad `farmer_a2`: 010'un `farmer_a_*` dosyaları duruyor, üzerine yazılmaz.
- Rapor: `art/results/011-karakter-boy-testi-2.md`, başta `unity-import: pending`. Kod oturumu sonucu `art-brief-karakter-result.md`'ye "Faz K1b" olarak kopyalar.
- K2 (karakter seti) bu işin sonucuna ve kullanıcının boy kararına bağlı; ayrı iş olarak açılacak.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
