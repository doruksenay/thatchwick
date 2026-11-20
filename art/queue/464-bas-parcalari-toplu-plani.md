---
id: "464"
title: "Baş parçaları toplu parti planı (iş dosyalarını yazar; 43b §3)"
status: pending
approved_by: user        # plan işi, gen harcamaz; 461 ve 461b review olunca çalışır
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["461", "461b"]
max_attempts: 1
pixellab: false
outputs:
  - art/queue/464a-....md (ve devamı)
  - art/results/464-bas-parcalari-toplu-plani.md
created: 2026-11-20
---
# 464 Baş parçaları toplu parti planı

## Amaç
Plan işi: PixelLab çağrılmaz. 461 ve 461b pilotlarının yöntemiyle 43b §3'teki bütün dünya baş parçaları için **iş dosyası yazar**. Kullanıcı kararı (19.11.2026): sayılar 43b'deki gibi; üretim kota sıfırlanınca (18.12) ayrı parti.

## Tarif
1. **Kapsam** (kimlikler `Assets/Resources/Content/appearance.json`, 461/461b'de üretilmiş olanlar hariç):
   - saç kütlesi `hairStyles` (bald hariç 15);
   - saç önü `bangs` (none hariç 13);
   - eklenti `hairExtras` (12);
   - süs `hairOrnaments` (10);
   - bıyık `mustaches` (14), sakal `beards` (18), favori `sideburns` (5);
   - başlık `hats` (16; `tag` = örtü: full, top, none).
2. **Tarif kaynağı:** 461 ve 461b iş dosyaları ve sonuç raporları; çıkan sorunlar (maske boyu, dikiş, ön/arka ayrımı) yeni tariflere işlenir. Her parça 5 yön (s, se, e, ne, n), `manken.py extract` ile çıkarılır, `ramps.py check` GEÇTİ şart.
   - Açıklamalar `displayName`'den İngilizceye çevrilir. Dönem kuralı: ortaçağ, hayali yerleşim.
   - Sarık, yaşmak, şaperon: döneme uygun kumaş başlık olarak tarif edilir.
3. **Bölme:** her iş en çok `cost_max: 60`, ölçülen birim maliyete göre (461 raporu) 1-2 parça. `approved_by: user`, `depends_on: ["461", "461b"]`. İş kimlikleri 464a, 464b, ...
4. **Sıra (priority):** önce hazır stillerde (`hairPresets`) en çok kullanılan kütle ve önler, sonra eklentiler, sakal, başlık, en son süs.
5. **Rapor:** `art/results/464-bas-parcalari-toplu-plani.md`: iş tablosu ve toplam gen. Ay bütçesini aşan kısım frenlerle sonraki döneme kalır.

## Doğrulama
- Yazılan iş dosyaları `python tools/art/queue_ready.py --all` ile okunuyor; hiçbir kimlik çakışmıyor.
- Kapsamdaki her kimliğin bir işi var (rapordaki tablo `appearance.json` ile karşılaştırılır).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
