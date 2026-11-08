---
id: "106"
title: Hayvanlar 6: köpek + kedi
status: pending
approved_by: pending     # kullanıcı çalıştırma onayı bekleniyor (plan 2026-11-08'de onaylandı)
cost_est: 17
cost_max: 20
depends_on: ["100"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_dog_1_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_dog_2_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_dog_3_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_dog_4_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cat_1_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cat_2_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cat_3_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cat_4_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-08
---
# 106 Hayvanlar 6: köpek + kedi

## Amaç
Ev ve çiftlik hayvanları. Yol B; köpekte dog, kedide cat şablonu. Her tür 4 aday; hepsi aynı çizim stili ve kamera. Kullanıcı Unity'de bakıp seçecek.

## Tarif

### Adaylar
Her aday 2 gen (tek kare 1 + v3 döndürme 1) + gerekirse renk azaltma 0,1. 8 aday ≈ 16-17 gen. Güney kontrolünden dönen aday 1 gen harcar.

| Aday | Yol | Tuval ve ayar | Açıklama metni (+ ortak ek) |
|---|---|---|---|
| dog_1 | B | pixflux 38x38, template dog | `medieval farm dog seen straight from the front facing the viewer, short tan coat, floppy ears, standing on all four legs` |
| dog_2 | B | pixflux 38x38, template dog | `medieval sheepdog seen straight from the front facing the viewer, black and white shaggy coat, standing on all four legs` |
| dog_3 | B | pixflux 38x38, template dog | `sturdy medieval guard dog seen straight from the front facing the viewer, brindle brown coat, broad head, standing on all four legs` |
| dog_4 | B | pixflux 38x38, template dog | `medieval hunting hound seen straight from the front facing the viewer, wiry grey coat, long legs, standing on all four legs` |
| cat_1 | B | pixflux 32x32, template cat | `small farm cat seen straight from the front facing the viewer, brown tabby striped coat, standing on all four legs` |
| cat_2 | B | pixflux 32x32, template cat | `small farm cat seen straight from the front facing the viewer, black coat, standing on all four legs` |
| cat_3 | B | pixflux 32x32, template cat | `small farm cat seen straight from the front facing the viewer, white coat with orange patches, standing on all four legs` |
| cat_4 | B | pixflux 32x32, template cat | `small farm cat seen straight from the front facing the viewer, grey coat, standing on all four legs` |

**Tuvaller:** köpek 38 (hedef 30 / 0,79). Kedi 32: hedef 21 için 27 gerekirdi ama pixflux'ın en küçük alanı 32x32; kedinin ~25 px çıkması beklenir (tahmin). Küçültme yapma, ölç ve raporla; rapora kedi için alternatif öner (ör. pixen 28, palet zorlamasız).

### Ortak kurallar (100 pilotu ve kullanıcı kararları, 2026-11-08)
- **Kamera ve hat:** `view="high top-down"`, `outline="single color black outline"`.
- **Açıklama eki** (her açıklama metninin sonuna, aynen): `, small black eyes, earthy muted medieval palette, flat simple shading, light from top-left, no ground, no shadow, no dirt under feet, transparent background, no dithering, no noise`.
- **Yol B (dört ayaklılar):**
  1. `create_image_pixflux`: `width=height=<tuval>`, `no_background=true`, `direction="south"`, `view="high top-down"`, `outline="single color black outline"`, `detail="medium detail"`, `shading="basic shading"`, `color_image_base64` = `Assets/Art/Iso/_aday/iso_char_farmer_a2_56_cand_se.png` (palet zorlama; K2 köylüsü).
  2. v3 döndürme: `body_type="quadruped"`, `template=<şablon>`.
- **Yol B' (kuşlar):**
  1. `create_image_pixen`: `width=height=<tuval>` (4'ün katı, kare), `no_background=true`, `direction="south"`, `view="high top-down"`, `outline="single color black outline"`, `detail="medium detail"`.
  2. v3 döndürme: `body_type="humanoid"`.
- **v3 döndürme:** `create_character`, `mode="v3"`, `n_directions=8`, `reference_image_url` (`get_image` URL'si ya da `get_upload_url` ile yükleme; 32 px üstünü inline base64 ile gönderme), `size` verme. Maliyet `ceil(w*h*8/65536)`; 90x90'a kadar 1 gen. Çağrıdan önce araç açıklamasındaki maliyeti oku.
- **Güney kontrolü (kullanıcı toleransı):** döndürmeden önce tek kareye bak. Yüz tam önden bakmalı; gövdenin dikey eksenden kayması **≤3 px kabul**. Ötesi (3/4 dönük, yandan, gövde >3 px kayık): döndürme yapma, tek kareyi masaüstü `_ham\` altına `..._cand_tek_guney_degil.png` adıyla koy, raporda yaz. Yeniden deneme yok.
- **Zemin/gölge kontrolü:** tek karede ya da 8 yönde ayak altında opak toprak, gölge, çimen ya da zemin lekesi varsa adayı Assets'e koyma (maskeyle silme yok). Yalnızca masaüstünde dursun, raporda "zemin lekesi" diye işaretle.
- **Renk azaltma:** her adayın 8 yön ham renk sayısını say. 32'den fazlaysa `reduce_colors`, 8 kare tek çağrıda birlikte, `num_colors=32`, `dithering="none"` (0,1 gen). 32 ya da altıysa yapma, "gerekmedi" yaz. Ham 8 yön masaüstünde `_ham\` altında kalır; Assets'e azaltılmış hâli (gerekmediyse ham) gider. Bozulma denetimi: ham ve azaltılmış SE'yi 8 kat büyüterek karşılaştır; göz, gaga, ibik, boynuz ya da dış hat bozulduysa **⚠ azaltma bozdu** diye işaretle.
- **Ölçek:** yeniden örnekleme (büyütme ya da küçültme) yok. Boy hedefi tutmazsa yalnızca ölç, raporla, gelecek için tuval öner (tahmin olarak).
- **Yön eşlemesi:** south=s, south-east=se, south-west=sw, west=w, north-west=nw, north=n, north-east=ne, east=e.
- **Teslim:** `Assets/Art/Iso/_aday/iso_animal_<tür>_<n>_cand_<yön>.png`, `cp -n`; `.meta` yazma; var olan dosyanın üzerine yazma. Aday numaraları tabloda; o adla dosya zaten varsa dur ve `## Sorular`'a yaz.

### Masaüstü kopyası (kullanıcının açık izni)
Hiçbir şeyi silme ya da üzerine yazma. Klasör tarihi üretim günü (`yyyy-mm-dd`).
- `C:\Users\User\OneDrive\Desktop\art\hayvan\<yyyy-mm-dd>_<tür>\`: her adayın 8 yön PNG'si (Assets'teki adlarla; reddedilen, zemin lekeli ve döndürülemeyenler dahil). `_ham\` alt klasörü: ham 8 yön ve tek kare referanslar. Aynı tür için o günün klasörü varsa içine ekle, mevcut dosyalara dokunma.
- Her tür klasöründe `_onizleme_x4.png` (o işin adayları; aynı adla dosya varsa `_onizleme_x4_<iş>.png`):
  - Tüm adayların SE yönü yan yana, en yakın komşu ile 4 kat, `#333333` zemin, her adayın altında aday numarası.
  - Ölçek referansı: `Assets/Art/Iso/_aday/iso_char_farmer_a2_56_cand_se.png` (58 px köylü), aynı 4 kat. Her hücrede 64x32 karo elmas konturu (1 px açık gri, 4 kat), ayak altı elmas merkezine oturur. Hücreler alt hizalı.
- Toplu önizleme: `C:\Users\User\OneDrive\Desktop\art\hayvan\<yyyy-mm-dd>_<iş>_toplu_x4.png`; her tür bir satır, satır başında köylü referansı ve karo konturu.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna her aday için bir satır: `tarih | hayvan | klasör | araç | generation | aday | not`.
- Python'da Pillow yok: `uv run --with pillow ...`. Betikleri geçici klasörde tut, repoya ekleme.

## Doğrulama
- `python tools/art/validate_iso.py <SE dosyası> --kind object` (her teslim edilen aday).
- Her dosya (8 yön x aday) için tablo: piksel boyutu, opak bbox yüksekliği (boy) ve genişliği (uzunluk), renk sayısı (ham / azaltılmış), yarı saydam piksel sayısı, pivot x ve pivot y (ALTTAN; 010'daki ayak bandı yöntemi). Pivot y'nin yönler arası oynamasını ayrıca yaz.
- Hedef boydan sapma yüzde olarak. Tuval başına doluluk oranı (SE boy / tuval yüksekliği) ayrıca yazılır; sonraki işler bunu kullanır.
- Gölge pişirilmemiş, zemin lekesi yok, arka plan saydam, ışık sol-üstten. 8 yön tutarlılığı (aynı hayvan, doğru yönler). Kuşlarda humanoid iskeletin hayvanı insanlaştırıp insanlaştırmadığı not edilir.

## Rapora eklenecek
- Çağrı başına generation, `get_balance` öncesi/sonrası.
- Her aday için tek satır özet: yol, tuval, güney kontrolü, SE boy / uzunluk ve hedef sapması, ham / azaltılmış renk, zemin lekesi, ⚠ işaretleri.
- Tuval başına doluluk oranı ve sonraki işler için tuval önerisi (tahmin olarak).

## Notlar
- **Git (kullanıcı kuralı):** `git pull --rebase --autostash`. `git add art Assets/Art/Iso tools/art` kullanılmaz; yalnızca bu işte oluşturulan/değiştirilen dosyalar tek tek eklenir (bu işin `iso_animal_*` PNG'leri, `art/results/<iş>-*`, `art/budget.md`, bu iş dosyası). `.meta` ve başka oturumların dosyaları eklenmez; commit öncesi `git diff --cached --name-only` ile kontrol. Başka oturumlara ait commit edilmemiş değişiklik görülürse dur ve bildir (kullanıcı o çalışma için istisnaları ayrıca söyler).
- Ölçek hedefleri (kullanıcı, K2 köylüsü 58 px, 2026-11-08): civciv ~13, tavuk ~24, horoz ~28, ördek ~21, kaz ~30, kedi ~21, köpek ~30, koyun ~36, domuz ~36, keçi ~36, inek ~48 (uzunluk ~70), at ~60 (uzunluk ~70). Kabul edilen referans: `iso_animal_sheep_2_cand_*` (pixflux 48 → SE boy 38, doluluk 0,79).
- Varsayılan tuvaller doluluk oranlarından (pixflux ~0,79, pixen ~0,81) hesaplandı; tahmindir.
- Onaylananlar sonra `iso_animal_<tür>_<ad>_<yön>.png` adını alır (kullanıcı işi). İmport ve pivot `Assets/Editor/IsoImportPostprocessor.cs` ile otomatik.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
