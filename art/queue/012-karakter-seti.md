---
id: "012"
title: Karakter seti K2 (8 karakter, 56 px)
status: review
approved_by: user        # kullanıcı onayı 2026-11-08 (sohbet: boy 56, K2 başla)
family: karakter
style_gate: true
cost_est: 16.8
cost_max: 20
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_char_woman_a_56_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_elder_m_56_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_elder_f_56_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_boy_a_56_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_girl_a_56_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_woodcutter_a_56_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_farmer_f_56_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_craftsman_a_56_cand_se.png
created: 2026-11-08
---
# 012 Karakter seti K2 (8 karakter, 56 px)

## Amaç
`art-brief-karakter.md` Faz K2. Kullanıcının boy kararı: **56** (60x60 tuval, yetişkinde gerçek boy ~58 px). Temel tasarım `farmer_a2_56` (011): `Assets/Art/Iso/_aday/iso_char_farmer_a2_56_cand_*.png`. 48 ve 64 bırakıldı. Sekiz yeni karakter aynı stil ve ölçekte üretilir. Her biri silüet ve renkle ayırt edilebilir olmalı.

## Tarif

### Ortak ayarlar
- **Araç:** `create_character`, `mode=v3`, `body_type=humanoid`, `view=high top-down`, `outline=single color black outline`, `detail=high detail`. 8 yön.
- **Ortak açıklama eki:** her karakterin metninin sonuna şunu ekle:
  ", earthy muted medieval village palette, desaturated ochre, brown, olive and grey cloth, clean pixel art, crisp black outline, readable face with brows nose and mouth line, no dithering, no noise, no cast shadow, transparent background"
- **Çalışma tavanı:** 20 gen (kullanıcı kararı; `budget.md`'deki 60'ın yerine yalnızca bu çalışma için). Tahmin 8 × 2 + 8 × 0,1 = 16,8.
- **Eşzamanlı iş sınırı:** çağrı reddedilirse (ücret yazılmaz) 30 saniye bekle, bir kez daha dene; ret sürerse dur. Karakterler sırayla üretilir.

### Boy ve tuval
- **Yetişkinler** (woman_a, woodcutter_a, farmer_f, craftsman_a): `size=60`. Hedef gerçek boy 58 ±3 px.
- **Yaşlılar** (elder_m, elder_f): `size=56`. Hedef 54 ±3 px, hafif kısa ve kambur. Çıktı 56x56'dır; şeffaf dolguyla 60x60'a getir.
- **Çocuklar** (boy_a, girl_a): `size=40`. Hedef ~39 px, yani yetişkinin %65-70'i. Çıktı 40x40'tır; 60x60'a getir.
- **Dolgu kuralı:** ölçekleme yok, yalnızca şeffaf piksel eklenir. Karakter yatayda ortalanır. Ayak altı, aynı yönün farmer_a2_56 karesindeki ayak altıyla aynı satıra gelir (dikeyde alttan hizalama). Böylece tüm set 60x60 tuvalde, aynı pivot düzeninde olur.

### Karakterler (ad = dosyadaki `<ad>`)
1. **woman_a:** "adult village woman, linen apron over a faded green-brown wool dress, hair tied back in a bun, light-medium skin, small cloth pouch at belt, laced shoes"
2. **elder_m:** "old man villager, grey beard, slightly hunched posture, holding a wooden walking stick, worn brown wool tunic and grey hood on shoulders, light skin, leg wraps, shoes"
3. **elder_f:** "old woman villager, grey hair under a wool headscarf and shawl, slightly hunched, long muted dark-red brown dress, light skin, small basket or none"
4. **boy_a:** "young village boy, child proportions, short messy hair, torn collar on a plain beige tunic, patched brown trousers, cloth leg wraps, small shoes, light skin"
5. **girl_a:** "young village girl, child proportions, braided hair, simple muted blue-grey wool dress with belt, light skin, small shoes"
6. **woodcutter_a:** "young strong woodcutter man, broad shoulders, dark skin, short black hair, dark olive wool tunic with rolled sleeves, leather belt with axe sheath and axe at hip, leg wraps, boots"
7. **farmer_f:** "young farmer woman, wide straw hat, medium skin, muted ochre blouse and brown skirt, carrying a woven basket, apron, laced shoes"
8. **craftsman_a:** "slim adult male craftsman, leather work apron with tools in pocket, dark hair, clean shaven, light skin, rolled grey-brown sleeves, belt, shoes"

Listede belirsizlik (ör. tırpan ya da sepet) kullanıcının brief'inde zaten "ya da" diye verilmiş; burada seçildi: farmer_f sepet taşır, tırpan yok. Bunun dışında bir belirsizlik görürsen tahmin yürütme: `## Sorular` ekle, `status: blocked` yap ve dur.

### Kayıt ve renk azaltma
1. **Ham çıktı:** her karakterin 8 yönünü (dolgudan sonra, 60x60) `Assets/Art/Iso/_aday/_ham/iso_char_<ad>_56_cand_<yön>.png` olarak kaydet. Yön eşlemesi: south=s, south-east=se, east=e, north-east=ne, north=n, north-west=nw, west=w, south-west=sw.
2. **Renk azaltma:** `reduce_colors`, her karakter için **bir çağrı**, o karakterin 8 yönü birlikte. `num_colors=32`, `dithering=none`. Dosyaları `image_urls` ile ver (`get_upload_url` ile yükle, base64 kullanma).
3. **Sonuç:** `Assets/Art/Iso/_aday/iso_char_<ad>_56_cand_<yön>.png`. Alfa kanalı hamla aynı olmalı.
4. **Yasak:** Var olan dosyaların üzerine yazma. `Assets/Art/Iso/` köküne bir şey koyma; onay gelmeden yalnızca `_aday` kullanılır.

### Önizleme
- `tools/art/char_preview.py` (011'de yazıldı) ile her karakter için `_onizleme_x4.png`:
  - **Üst satır:** köylü (`iso_char_villager_se.png`), farmer_a2_56 SE (stil referansı), adayın renk azaltılmış SE'si ve ham SE'si. Her biri kendi 64x32 elmas konturunun üzerinde, 4 kat büyütülmüş, #333333 zemin.
  - **Alt satır:** renk azaltılmış 8 yön.
- **Faz toplusu:** köylü, farmer_a2_56 ve 8 karakterin SE kareleri tek satırda, aynı elmas ızgarasında, 4 kat. Ad: `2026-11-08_K2_toplu_x4.png`.

### Masaüstü kopyası (kullanıcının brief'indeki açık istisna, bu iş için)
- **Karakter klasörü:** `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_<ad>\` içine renk azaltılmış 8 yön, `_ham\` altına 8 ham yön ve `_onizleme_x4.png`.
- **Faz toplusu:** `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_K2_toplu_x4.png`.
- **İndeks:** `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna karakter başına bir satır ekle. Generation sütununa yalnızca sayı (2,1) yaz:
  `2026-11-08 | karakter | karakter/2026-11-08_<ad> | create_character v3 + reduce_colors | 2,1 | aday | boy <h> px, tunik <ton>`
- Masaüstünde silme ve üzerine yazma yok.

## Doğrulama
- **Karakter başına tablo (SE):**
  - gerçek boy (opak bbox yüksekliği), hedef ±3 tuttu mu
  - genişlik
  - renk sayısı, ham → azaltılmış
  - yarı saydam piksel
  - pivot x ve y ALTTAN (010/011 yöntemiyle)
- **Pivot y, yön başına:** her karakter için 8 değer (s, se, e, ne, n, nw, w, sw).
- **8 yön:** renk sayısı ≤32 ve yarı saydam 0 olmalı.
- **Doğrulayıcı:** her SE için `uv run --with pillow python tools/art/validate_iso.py <dosya> --kind object --max-colors 32`.
- **Tunik tonu:** her karakterin ana giysi rengini yaz: baskın hex ve kısa ad (ör. #8a6a3a okra-kahve). Köylü paletinden belirgin sapma varsa (doygun ya da parlak renk) not et.
- **Ayırt edilebilirlik:** toplu görselde silüet, renk, boy ve yaş farkını kısaca yaz. Birbirine fazla benzeyen çiftleri belirt.
- **Tutarlılık:** her karakterin yüzü, kıyafeti ve aksesuarı 8 yönde tutarlı mı? Değişen aksesuarı (ör. değnek bir yönde yok) yaz.
- Başarısız karakteri yeniden üretme. Tek deneme kuralı geçerli: ölçümü ve sorunu yaz.

## Notlar
- **Rapor:** `art/results/012-karakter-seti.md`, başta `unity-import: pending`. Kod oturumu özeti `art-brief-karakter-result.md`'ye "Faz K2" olarak ekler.
- **Onay sonrası:** onaylanan karakterler kullanıcı onayından sonra `Assets/Art/Iso/iso_char_<ad>_<yön>.png` olarak taşınır. Bu iş taşımaz.
- **Yürüme animasyonu** bu işte yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
