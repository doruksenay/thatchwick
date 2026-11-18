---
id: "018"
title: Animasyon planı, 84 px gerçekçi karakter (yürüme + etkileşim; pilot ve set iş dosyalarını yazar)
status: review
claimed_at: 2026-11-18T18:46Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["012b"]
max_attempts: 1
pixellab: false
outputs:
  - art/queue/018a-*.md ... art/queue/018z-*.md
  - art/results/018-etkilesim-animasyonlari-plani.md
created: 2026-11-17
---
# 018 Animasyon planı: 84 px gerçekçi karakter

## Karar (kullanıcı, 18.11.2026)
- 012 (56 px, chibi) reddedildi; **012b kabul edildi**, animasyon tabanı **84 px** adaylar:
  - erkek: `Assets/Art/Iso/_aday/iso_char_man_r84_cand_se.png` (doygunluk `fit` ile düşürülmüş hâli teslim edilen)
  - kadın: `Assets/Art/Iso/_aday/iso_char_woman_r84_cand_se.png`
- 012b raporu: PixelLab `create_character` v3, 8 yön üretildi, yalnızca SE teslim edildi. Karakter kimlikleri `art/results/012b-karakter-gercekci-oran-pilotu.md`'de ya da PixelLab hesabında; kalan yönler oradan alınır.
- 013/014/015 yürüme işleri eski 56 px köylü içindi; yeni karakterde **yürüme yeniden** yapılır.

## Tarif
1. **Yönler:** 012b karakterlerinin 8 yönünü PixelLab'den indir (yeni üretim yok, 0 gen). Yoksa `## Sorular`.
2. **Animasyon anahtarlarını koddan çıkar** (okumak serbest, yazmak yasak):
   - `Assets/Scripts/Core/Life/ActivityDescriber.cs`: sleep, eat, warm, social, fun, chop, farm, cook, craft, work, walk, care, love, idle, dead;
   - kazı işleri: `Assets/Scripts/Core/Jobs/IAnimalEffects.cs` `TileAction` (Dig, Support, Bail; Sow, Water, Till);
   - `items.json` etkileşimlerinin `skill` alanları.
   Her anahtar için kare sayısı, döngü/tek seferlik ve hangi anahtarların aynı animasyonu paylaşacağı (ör. Till ve farm).
3. **Yöntem:** 013 raporundaki seçim (`animate_character`, pixminimax) 84 px'te yeniden denenir; pilot bunu doğrular.
4. **İş dosyalarını yaz** (`art/TEMPLATE.md`, `approved_by: user`, `family: karakter`, her biri `cost_max <= 60`):
   - **018a pilot:** yürüme, erkek 84, SE. `style_gate: true`, `depends_on: ["012b"]`.
   - **018b:** yürüme, erkek ve kadın, 8 yön. `depends_on: ["018a"]`.
   - **018c...:** etkileşim anahtarları × (erkek, kadın). `depends_on: ["018a"]`. Çocuk ve yaşlı tabanı yok (sonraki karar); raporda not et.
   - **Kazma, kova, destek:** yeni iş açma; mevcut `368-kazma-animasyonu.md`'nin tarifini 84 px tabana göre güncelle ve `depends_on`'u `["018a"]` yap, `status: pending`.
5. **Rapor:** `art/results/018-etkilesim-animasyonlari-plani.md`: anahtar listesi, iş tablosu, toplam maliyet ve aylık bütçeye göre takvim.

## Doğrulama
- `queue_ready.py --all` yeni işleri ayrıştırıyor mu, numara çakışması yok mu?
- `cost_max <= 60`.

## Notlar
- Katmanlı karakter (016) kararı varsa animasyon kareleri katmanlarla uyumlu olmalı; raporda belirt.
- 84 px karakter oyunda 56 px'in yerine geçer: `IsoArt` ve ölçek değişikliği kod oturumunun işi, işçi yalnızca raporda not eder.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

<!-- 2026-11-18 kullanıcı kararı (sohbet): 012 reddedildi; 012b kabul, taban 84 px. Plan buna göre yeniden yazıldı. -->
