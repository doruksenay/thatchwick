---
id: "015c"
title: Yürüme animasyonu seti 3/3 (boy_a, girl_a)
status: pending
approved_by: pending     # kullanıcı bütçe günü onaylayacak
family: karakter
style_gate: false
cost_est: 10
cost_max: 25
depends_on: ["014"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_<ad>_56_walk_<yön>_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_<ad>_56_idle_<yön>.png
  - art/results/015c-yurume-set-c.md
created: 2026-11-08
---
# 015c Yürüme animasyonu seti 3/3 (boy_a, girl_a)

## Amaç
K2 karakter setinin (012, accepted) yürüme animasyonu. Toplu iş 25 gen tavanı nedeniyle üç parçaya bölündü: 015a, 015b ve 015c. Yöntem ve işleme kuralları 014 ile birebir aynı: pixminimax, 8 karelik döngü, 64x64 tuval, ayak satırı y=61, tek pivot (32, 2 ALTTAN).

## Tarif

### Karakterler
| Karakter | PixelLab id | Native tuval | Üretilen yönler | Ayna | Gerekçe | Gen |
|---|---|---|---|---|---|---|
| boy_a | 264e22ee-3ff5-42e9-9dc8-006b13b9b051 | 40 | se, s, e, ne, n | **evet** (sw←se, w←e, nw←ne) | simetrik; tek taraflı aksesuar yok | 5 |
| girl_a | d84f5155-87ee-482c-acce-8a521c508acb | 40 | se, s, e, ne, n | **evet** (sw←se, w←e, nw←ne) | simetrik; tek taraflı aksesuar yok | 5 |
| **Toplam** | | | | | | **10** (aynalama kontrolü 8 yöne çıkarırsa en çok 16) |

Karakterler sırayla işlenir. Bir karakterin bütün yönleri bitmeden sonrakine geçilmez.

### Üretim
- Her üretilen yön için 014 "Üretim" bölümündeki çağrının aynısı yapılır:
  - `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`
  - aynı `action_description`
  - `end_frame_url`: o yönün PixelLab rotasyonu, native boyutta
  - `animation_name=walk`; ilk çağrıdan sonra aynı `animation_group_id`
- Yön başına maliyet 1 gen. Maliyet satırı farklıysa dur.
- **Aynalama kontrolü (ücretsiz, üretimden önce):** "Ayna: evet" olan karakterin 8 rotasyonuna bak. Belirgin tek taraflı bir aksesuar görürsen (kese, çanta, alet, sepet, silah) o karakteri aynalama; 8 yön üret ve nedenini raporla. Ancak bunu yalnızca toplam `cost_max`'i aşmıyorsa yap; aşıyorsa `## Sorular` ekle, `status: blocked` yap ve dur.
- "Ayna: hayır" olan karakteri aynalama.

### İşleme
`014-yurume-farmer-a2.md` "Ortak işleme" maddeleri 1-9, aynen uygulanır (betik `tools/art/anim_walk.py`). Aynalanan karakterde sw, w ve nw yürüme ve durma kareleri ayna kopyadır (madde 7).

### Native boyut
Çocuk (40) ve yaşlı (56) karakterlerin PixelLab kareleri native tuvalde gelir. Ölçeklenmez, yalnızca 64x64'e dolgu yapılır; ayak satırı y=61, pivot x=32. Böylece set, boy oranları korunarak tek pivotta hizalanır.

## Doğrulama
- **Karakter ve yön başına tablo:** 014 "Doğrulama" bölümünün aynısı. Ek olarak aynalanan yönlerin kaynağı belirtilir (ör. sw ← se).
- **Aksesuar tarafı:** karakter karakter yazılır. Aksesuar hangi elde ya da yanda? Ayna yönlerde taraf değişti mi? Değişen karakterin listesi rapora girer.
- **Yön kayması ve kimlik:** 014'teki gibi kontrol edilir.
- **Doğrulayıcı:** her işlenmiş kare için `validate_iso.py --kind object --max-colors 32`; pivot (32, 2) kontrolü.

## Önizleme
- **Sayfa:** karakter başına `art/results/015c-<ad>-sayfa.png` (8 yön × 8 kare, 4 kat büyütme, sabit pivotlu 64x32 elmas).
- **GIF:** karakter başına `art/results/015c-<ad>-x4.gif`. 8 yön aynı anda oynar, köylü yanda durur.
- **Toplu GIF:** `art/results/015c-toplu-se-x4.gif`. Bu işteki karakterlerin SE yürüyüşü yan yana.
- **Masaüstü kopyası** (kullanıcının brief'indeki açık istisna):
  - `C:\Users\User\OneDrive\Desktop\art\karakter\<çalışma tarihi>_<ad>_yurume\` (işlenmiş ve ham kareler, durma kareleri, sayfa ve GIF).
  - Toplu GIF karakter klasörüne değil, `C:\Users\User\OneDrive\Desktop\art\karakter\` altına kopyalanır.
  - `_indeks.md` sonuna karakter başına bir satır. Generation sütununa yalnızca sayı yazılır.
  - Silme ve üzerine yazma yok.

## Notlar
- 014 `accepted` olmadan başlamaz. Böylece işleme düzeni önce tek karakterde onaylanır.
- Rapor: `art/results/015c-yurume-set-c.md`, başta `unity-import: pending`.
- Seti oluşturan işler: 015a (woman_a, elder_m, elder_f), 015b (woodcutter_a, farmer_f, craftsman_a), 015c (boy_a, girl_a). Set toplamı 58 gen.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
