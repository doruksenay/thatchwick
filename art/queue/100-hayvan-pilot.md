---
id: "100"
title: Hayvan pilotu (koyun + tavuk, 4 aday, araç karşılaştırması)
status: running
approved_by: user        # kullanıcı onayı 2026-11-08 (sohbet)
cost_est: 8
cost_max: 12
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_sheep_1_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_sheep_2_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_chicken_1_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_chicken_2_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-08
---
# 100 Hayvan pilotu

## Amaç
Köy hayvanlarının (çiftlik, yaban, yavrular) tasarım adayları için hangi PixelLab yolunun uygun olduğunu ucuza sınamak. İki tür (koyun, tavuk), dört aday. Her aday ayrı bir yolu deniyor. Kullanıcı Unity'de bakıp yolu ve ölçeği seçecek; Öncelik 1 toplu işleri (101+) bu işin `accepted` olmasından sonra yazılır.

Hayvanlar köylüyle aynı dünyada durur. Referans köylü: `Assets/Art/Iso/iso_char_villager_se.png`. Gövdesi ~45 px; karo 64x32.

## Tarif

### Ortak kurallar
- `view="high top-down"` (2:1'e en yakın, ~35°). `outline="single color black outline"`.
- Açıklama metinlerinin hepsine şu ek yazılır: `, earthy muted medieval palette, flat simple shading, light from top-left, no cast shadow, no background, no dithering, no noise`.
- **Güneye bakış kontrolü (B ve B' yolları):** v3 döndürmeden önce tek kareye bak. Hayvan izleyiciye **dümdüz önden** bakmıyorsa (3/4 dönük, yandan vb.) döndürme yapma. Tek kareyi `_ham` olarak sakla, raporda "güney değil, döndürülmedi" yaz, o aday için devam etme. Yeniden deneme yok.
- **v3 döndürme:** `create_character`, `mode="v3"`, `n_directions=8`, `reference_image_url` (görselin URL'si; `get_image`'in verdiği URL ya da `get_upload_url` ile yükleme). 32 px'ten büyük görseli inline base64 ile gönderme. `size` verme, çıktı referans boyutunda kalsın. Maliyet `ceil(w*h*8/65536)` = 1 gen (≤64x64).
- **Yön eşlemesi:** south=s, south-east=se, south-west=sw, west=w, north-west=nw, north=n, north-east=ne, east=e.
- **Ölçek:** yeniden örnekleme (büyütme, küçültme) yok. Boy hedefi tutmazsa yalnızca ölç ve raporla.

### Adaylar
| Aday | Yol | Çağrılar ve ayarlar | Açıklama metni (+ ortak ek) | Gen |
|---|---|---|---|---|
| sheep_1 | A | `create_character` `mode="standard"`, `body_type="quadruped"`, `template="dog"`, `n_directions=8`, `size=40`, `detail="medium detail"`, `shading="basic shading"` | `small medieval farm sheep, thick white wool, black face and black legs, standing on all four legs` | 1 |
| sheep_2 | B | 1) `create_image_pixflux` `width=48`, `height=48`, `no_background=true`, `direction="south"`, `color_image_base64` = `iso_char_villager_se.png` (palet zorlama), `detail="medium detail"`, `shading="basic shading"`. 2) v3 döndürme, `body_type="quadruped"`, `template="dog"` | `small medieval farm sheep seen straight from the front facing the viewer, brownish grey wool, dark brown face, standing on all four legs` | 1 + 1 |
| chicken_1 | B' | 1) `create_image_pixen` `width=32`, `height=32`, `no_background=true`, `direction="south"`, `detail="medium detail"`. 2) v3 döndürme, `body_type="humanoid"` | `small white farm hen seen straight from the front facing the viewer, red comb and wattle, yellow beak and legs, standing` | 1 + 1 |
| chicken_2 | B | 1) `create_image_pixflux` `width=32`, `height=32`, `no_background=true`, `direction="south"`, `color_image_base64` = `iso_char_villager_se.png`, `detail="medium detail"`, `shading="basic shading"`. 2) v3 döndürme, `body_type="humanoid"` | `small brown speckled farm hen seen straight from the front facing the viewer, red comb, yellow beak and legs, standing` | 1 + 1 |

### Renk azaltma (kullanıcı kararı: evet)
- Her aday için 8 yönün ham renk sayısını say. 32'den fazlaysa `reduce_colors` uygula: 8 kare **tek çağrıda birlikte**, `num_colors=32`, `dithering="none"`. Maliyet 0.1 gen. 32 veya altındaysa azaltma yapma, raporda "gerekmedi" yaz.
- Ham 8 yön masaüstünde `_ham\` altında kalır. `Assets/Art/Iso/_aday/`'a azaltılmış hâli gider (azaltma gerekmediyse ham hâli).
- **Bozulma denetimi:** ham ve azaltılmış SE'yi 8 kat büyütüp karşılaştır. Dış hat kopması, göz kaybı, tavukta gaga ya da ibik kaybı, koyunda yüz/bacak siyahının yünle karışması varsa adayı raporda **⚠ azaltma bozdu** diye işaretle ve neyin bozulduğunu yaz. Ham hâli yine de masaüstünde dursun.

### Teslim
- `Assets/Art/Iso/_aday/iso_animal_<tür>_<n>_cand_<yön>.png`, `cp -n` ile; `.meta` yazma. Mevcut dosyaların üzerine yazma.

### Masaüstü kopyası (kullanıcının açık izni)
Hiçbir şeyi silme, üzerine yazma. Klasör tarihi üretim günüdür (`yyyy-mm-dd`).
- `C:\Users\User\OneDrive\Desktop\art\hayvan\<yyyy-mm-dd>_sheep\` ve `..._chicken\`: adayların 8 yön PNG'leri (Assets'tekiyle aynı adlar; reddedilen ya da döndürülemeyenler dahil) ve `_ham\` alt klasörü (ham 8 yön, tek kare referanslar).
- Her tür klasöründe `_onizleme_x4.png`:
  - Tüm adayların SE yönü yan yana, en yakın komşu ile 4 kat büyük, `#333333` zemin, her adayın altında aday numarası.
  - **Ölçek referansı:** adayların yanında `Assets/Art/Iso/iso_char_villager_se.png`, aynı 4 kat ölçekte (ayrıca ölçekleme yok). Her hücrede 64x32 çimen karosu elmas konturu (1 px açık gri çizgi, 4 kat), sprite'ın ayak altı elmas merkezine oturacak şekilde. Bütün hücreler alt hizalı.
- Toplu önizleme: `C:\Users\User\OneDrive\Desktop\art\hayvan\<yyyy-mm-dd>_100_toplu_x4.png`. Her tür bir satır; satır başında köylü referansı ve karo konturu.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna her aday için bir satır: `tarih | hayvan | klasör | araç | generation | aday | not`.
- Python'da Pillow yok: `uv run --with pillow ...`. Betikleri geçici klasörde tut, repoya ekleme.

## Doğrulama
- `python tools/art/validate_iso.py <SE dosyası> --kind object` (her aday).
- Her dosya (8 yön × 4 aday) için tablo: piksel boyutu, opak bbox yüksekliği (boy) ve genişliği (uzunluk), renk sayısı (ham / azaltılmış), yarı saydam piksel sayısı, pivot x ve pivot y (ALTTAN; 010'daki ayak bandı yöntemi).
- Hedef boylar (yaklaşık): koyun 28-30 px (uzunluk ~40), tavuk 18-20 px. Sapmayı yüzde olarak yaz. Hedefe ulaşmak için tuval/size önerisini tahmin olarak yaz.
- Gölge pişirilmemiş, arka plan saydam, ışık sol-üstten. 8 yön tutarlılığı (aynı hayvan, doğru yönler). Kuşlarda humanoid iskeletle döndürmenin hayvanı insanlaştırıp insanlaştırmadığı (kol, dik gövde vb.) ayrıca not edilir.

## Rapora eklenecek öneriler
1. **Yol önerisi:** dört ayaklılar ve kuşlar için hangi yol (A, B, B'), gerekçesiyle. B tutmadıysa C (`create_character_pro_flash`, 6 gen) ya da D (`create_8_direction_object`, 10 gen) gerekip gerekmediği.
2. **Toplu işlerin günlere bölünmesi:** Öncelik 1 (12 tür × 4-5 aday) için iş listesi ve gün planı. Her iş `cost_max` ≤ 25. Çalışma tavanı 60 gen ve 3 iş, günlük 150. O günün diğer oturum harcamalarına (zemin, eşya, karakter) pay bırakılmalı; öneri olarak hayvan işlerine günde en fazla ~60-75 gen. Bütçe kuralı gevşetme önerisi varsa `## Sorular` altına yaz.

## Notlar
- Tür adları İngilizce küçük harf ASCII. Onaylananlar sonra `iso_animal_<tür>_<ad>_<yön>.png` adını alır (kullanıcı işi).
- Pivot ve import ayarlarını `Assets/Editor/IsoImportPostprocessor.cs` `iso_animal_` önekiyle otomatik yapıyor; işçi `.meta` yazmaz.
- **Tür listesi (sonraki işler için, kullanıcı onaylı):**
  - Öncelik 1: chicken, rooster, chick, sheep, cow, pig, goat, horse, dog, cat, duck, goose.
  - Öncelik 2: donkey, ox, rabbit, sparrow, crow, fox, wolf (sert ton), deer, mouse, pigeon, owl, hedgehog.
  - Öncelik 3 (yavrular, yetişkinin ~%60'ı): lamb, calf, foal, kitten, puppy, kid (oğlak), piglet, duckling, gosling.
  - Arı sprite olarak yapılmaz; kovan eşyası ve sürü efekti ayrı iş.
- Ölçek hedefleri (yaklaşık): civciv 10-12, tavuk 18-20, horoz 22-24, ördek 16-18, kaz 24, küçük kuş 10-12, kedi 16-18 (uzunluk ~22), köpek 22-26, koyun 28-30, domuz 28, keçi 28-30, inek 36-40 (uzunluk ~56), at 48 (uzunluk ~56), eşek 40, tavşan 12-14, tilki 20, kurt 28, geyik 40.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
