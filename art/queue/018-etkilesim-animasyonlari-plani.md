---
id: "018"
title: Etkileşim animasyonları planı (pilot + set iş dosyalarını yazar)
status: blocked   # 012 karakter oranı reddedildi; yürüme/animasyon 012b pilotunun kabulünü bekler, iş yeni karaktere göre yeniden yazılacak
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["013", "014"]
max_attempts: 1
pixellab: false
outputs:
  - art/queue/018a-*.md ... art/queue/018z-*.md
  - art/results/018-etkilesim-animasyonlari-plani.md
created: 2026-11-17
---
# 018 Etkileşim animasyonları planı

## Amaç
Karakterler yer, uyur, odun keser, eker, pişirir, sohbet eder. Şu an yalnızca yürüme pilotu var (013, 014, 015a-c).

Proje belgeleri (`09` Riskler, `12` Eksikler 8) etkileşim animasyonu listesinin eksik olduğunu yazıyor. Bu plan işi listeyi koddan çıkarır, bir pilot ve set iş dosyalarını yazar.

## Tarif
1. **Animasyon anahtarlarını koddan çıkar:** `Assets/Scripts/Core/Life/ActivityDescriber.cs` (etkinlik anahtarları: sleep, eat, warm, social, fun, chop, farm, cook, craft, work, walk, care, love, idle, dead). Ek olarak `items.json` etkileşimlerinin `skill` alanları.
   - Okumak serbest, yazmak yasak.
   - Her anahtar için kaç kare ve döngü mü, tek seferlik mi olacağını öner.
2. **Yöntemi seç:** 013 ve 014 raporlarından en iyi animasyon yöntemi (`animate_character` şablonu, v3, skeleton) ve kare başına maliyet.
3. **İş dosyalarını yaz:**
   - Format `art/TEMPLATE.md`; `approved_by: user`, `family: karakter`.
   - **018a pilot:** `chop` (odun kesme), farmer_a2, SE yönü. `style_gate: true`, `depends_on: ["014"]`.
   - **018b...:** her anahtar × karakter tabanı (yetişkin erkek, yetişkin kadın, çocuk, yaşlı).
     - `depends_on: ["018a"]`; her iş `cost_max <= 60`.
     - Yön sayısı 012/014 ile aynı.
4. **Rapor:** `art/results/018-etkilesim-animasyonlari-plani.md`. İçerik: liste, iş tablosu, toplam maliyet ve aylık bütçeye göre takvim.

## Doğrulama
- `queue_ready.py --all` yeni işleri ayrıştırıyor mu?
- `cost_max <= 60`.
- Numara çakışması yok.

## Notlar
- Katmanlı karakterde animasyon kareleri katmanlarla uyumlu olmalı. 016 raporunda bir yöntem önerildiyse (`transfer_outfit` gibi) tariflere ekle.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

<!-- 2026-11-18 kullanıcı kararı (sohbet): 012 karakter oranı reddedildi; yürüme/animasyon 012b pilotunun kabulünü bekler, iş yeni karaktere göre yeniden yazılacak -->
