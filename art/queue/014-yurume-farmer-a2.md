---
id: "014"
title: Yürüme animasyonu farmer_a2_56 (8 yön, pixminimax)
status: pending
approved_by: pending     # kullanıcı bütçe günü onaylayacak (planlanan: 2026-11-09)
cost_est: 8
cost_max: 10
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_farmer_a2_56_walk_<yön>_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_farmer_a2_56_idle_<yön>.png
  - art/results/014-yurume-farmer-a2.md
created: 2026-11-08
---
# 014 Yürüme animasyonu: farmer_a2_56, 8 yön

## Amaç
013 pilotundan sonra kullanıcı şu kararları verdi (2026-11-08):
- Yöntem pixminimax, 8 karelik döngü.
- Tuval 64x64.
- Sabit ayak satırı ve tek pivot.
- Aynalama karakter bazında. farmer_a2_56 kese ve omuz kayışı taşıdığı için **8 yön üretilir, aynalama yok**.

Bu iş, 015a-c'deki karakter seti için **ortak işleme kurallarının** kaynağıdır. 015 işleri aşağıdaki "Ortak işleme" bölümüne atıf yapar.

## Tarif

### Hedef
`farmer_a2_56`, PixelLab karakter `37cebe59-b761-4dd8-b23c-b11f60f58294` (60x60, 8 rotasyon). 013'te bu karaktere eklenmiş `walk_pilot_*` animasyonları var; onlara dokunma, silme.

### Üretim (8 × 1 gen)
- Ön kontrol (ücretsiz): `get_character`. Karakter `completed` olmalı ve 8 rotasyonu bulunmalı.
- 8 yönün her biri için ayrı `animate_character` çağrısı:
  - `mode=pixminimax`, `directions=[<yön>]`, `frame_count=8`, `keep_first_frame=true`
  - `action_description="walking forward at a calm pace, one full step cycle"` (013 ile aynı)
  - `end_frame_url`: o yönün **PixelLab rotasyon görüntüsü**. `get_character`'daki rotasyon bağlantısından indir, `get_upload_url` ile yükle. Doğal boyutundadır; başlangıç karesiyle aynı boyutta olmalı.
  - `animation_name=walk`. İlk çağrıdan sonra kalan yönleri `animation_group_id` ile aynı gruba ekle, `animation_name=walk` yine verilir.
- Yönler, sırasıyla: south-east, south, east, north-east, north, north-west, west, south-west.
- Maliyet kuralları:
  - Her çağrının maliyet satırı 1 olmalı. Farklıysa dur ve raporla.
  - Toplam 10'u geçemez.
  - Eşzamanlı iş sınırı çıkarsa 30 saniye bekle, bir kez daha dene. Yeniden üretim yok.
- Kareleri `get_character` bağlantılarından indir (ücretsiz, 013'teki gibi).

### Ortak işleme (014 ve 015a-c için bağlayıcı; betikle, ücretsiz)
Betik: `tools/art/anim_walk.py`. 013'teki `anim_pilot.py`'den türetilebilir.

1. **Döngü.** pixminimax 9 kare verir (00 = rotasyon, 01-08 üretilmiş). 08 durma pozuna döndüğü için **atılır**. Döngü 00-07, 8 kare.
2. **Tuval 64x64.** Ölçekleme yok. Kare 64x64 tuvale şeffaf dolgu ile ya da şeffaf kenar kırpılarak getirilir.
3. **Ayak satırı.** En alt opak satır her karede ve her yönde **y=61** olur. Bu, tuvalin alt satırından 2 px yukarısıdır; pivot y = 2 (ALTTAN). Bunun için kare başına tamsayı dikey kaydırma yapılır. Karakter böylece kendi içinde dikey olarak oynayabilir; buna dokunulmaz.
4. **Yatay konum: tek pivot.**
   - Her yönün 8 karesinde ayak bandı merkezi ölçülür: en alttaki gövde boyunun %10'u kadar satırda opak sütunların ortası.
   - Bu 8 değerin **ortalaması** alınır.
   - Yönün bütün karelerine **tek bir tamsayı yatay kaydırma** uygulanır; ortalama x=32'ye gelir.
   - Kare başına ayrı ortalama YAPILMAZ. Karakter kendi içinde yatay olarak oynayabilir.
   - Sonuç: pivot x = 32, her karede, her yönde ve her karakterde aynı.
5. **Durma karesi.** `iso_char_<ad>_56_idle_<yön>.png`, 64x64. Kare 00'dır (rotasyon), o yönün yürüme kareleriyle **aynı kaydırmayla** yerleştirilir; böylece duruştan yürümeye geçişte sıçrama olmaz. Mevcut 60x60 `_cand_` dosyalarına dokunulmaz.
6. **Palet.** Her piksel karakterin kendi 32 renklik paletine en yakın renge eşlenir (RGB uzaklığı). Palet, o karakterin `Assets/Art/Iso/_aday/iso_char_<ad>_56_cand_*.png` dosyalarındaki renklerin birleşimidir. Alfa ikili kalır (0 ya da 255).
7. **Aynalama** (yalnızca 015'te aynalanan karakterler için):
   - sw = yatay ayna(se), w = ayna(e), nw = ayna(ne).
   - Ayna, 64 px'lik tuvalin tamamında alınır (x → 63-x). Pivot x=32 korunur.
   - Aynalanan karakterde durma kareleri de aynadır. Üretilmiş sw, w ve nw rotasyonları kullanılmaz ama dosyaları silinmez. Böylece duruş ve yürüme aynı kimlikte kalır.
8. **Tepe taşması.** Bir kare 64x64'e sığmıyorsa (y<0) kırpılan satır ve piksel sayısını raporla. Kırpma dışında düzeltme yapma.
9. **Adlar** (`<NN>` = 00-07):
   - İşlenmiş: `Assets/Art/Iso/_aday/anim/iso_char_<ad>_56_walk_<yön>_<NN>.png`
   - Ham: `Assets/Art/Iso/_aday/anim/_ham/iso_char_<ad>_56_walk_<yön>_<NN>.png` (aracın tuvalinde, 08 dahil)
   - Ayna yönlerde ham dosya yoktur.
   - 013'ün `walk_pmm_se` dosyalarına dokunma. Var olan dosyanın üzerine yazma, `.meta` yazma.

## Doğrulama
- **Yön başına tablo:**
  - kare sayısı (8)
  - boy aralığı
  - hizalamadan önceki ayak satırı aralığı
  - uygulanan yatay kaydırma
  - kare başına ayak bandı merkezinin 32'den sapması (min/max)
  - ardışık IoU min/ortalama
  - döngü kapanışı (07 → 00 IoU)
  - renk ham → işlenmiş, yarı saydam
- **Yön kayması:** 013'te pixminimax'in son karelerinde karakterin öne döndüğü gözlendi. Her yön için 01-07 karelerinin bakış yönü durma karesiyle aynı mı? Gözle ve sayıyla bak: genişlik oranı (kare genişliği / durma genişliği) ve 00 ile IoU. Ayrışan kareleri listele.
- **Tutarlılık:** her yönde kese, kayış, yüz ve tunik tonu durma karesiyle aynı mı?
- **Doğrulayıcı:** her işlenmiş kare için `uv run --with pillow python tools/art/validate_iso.py <dosya> --kind object --max-colors 32`. Pivot (32, 2 ALTTAN) her karede doğrulanır.

## Önizleme
- **Sayfa:** `art/results/014-yurume-farmer-a2-sayfa.png`. 8 yön satır, 8 kare sütun. 4 kat büyütme, #333333 zemin. Her kare 64x32 elmas konturunun üzerinde; elmasın merkezi sabit pivottadır (32, 61). Kareler pivotla yerleştirilir, kare başına yeniden ortalanmaz.
- **GIF:** `art/results/014-yurume-farmer-a2-x4.gif`. 8 yön yan yana aynı anda oynar, köylü (`iso_char_villager_se.png`) solda sabit durur. 8 fps (125 ms; GIF 120 ms yazarsa belirt), sonsuz döngü.
- **Masaüstü kopyası** (kullanıcının brief'indeki açık istisna):
  - `C:\Users\User\OneDrive\Desktop\art\karakter\<çalışma tarihi>_farmer_a2_56_yurume\`: işlenmiş ve ham kareler, durma kareleri, sayfa, GIF.
  - `_indeks.md` sonuna bir satır: `<tarih> | animasyon | karakter/<tarih>_farmer_a2_56_yurume | animate_character pixminimax | 8 | aday | 8 yön × 8 kare, pivot (32,2)`.
  - Silme ve üzerine yazma yok.

## Notlar
- Rapor: `art/results/014-yurume-farmer-a2.md`, başta `unity-import: pending`. Unity için yazılacak pivot: (32, 2) piksel, yani normalize (0,5, 0,03125).
- 015a, 015b ve 015c bu işe bağlıdır: 014 `accepted` olmadan başlamazlar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
