---
id: "356"
title: Hayvan sağlığı ve komşu hane ikonları
status: review
claimed_at: 2026-11-18T13:06Z
approved_by: user        # otomatik akış: kullanıcı onayı (art/budget.md)
family: arayuz
style_gate: false
cost_est: 4
cost_max: 8
depends_on: ["315"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_animal_{sick,cold,cured}.png
  - Assets/Art/Iso/_aday/ui_icon_house_{head,heir,extinct,wedding,our_blood}.png
created: 2026-11-18
---
# 356 Hayvan sağlığı ve komşu hane ikonları

## Amaç
18.11.2026 partisiyle (dal `feat/yarim-kalanlar`) gelen iki sistem şimdilik yalnızca metinle görünüyor:
- **Hayvan sağlığı (Adım 42):** `Animal.HealthText` ("Hasta (2 gün)", "Üşüyor"). Hayvanın başında ve yapı modundaki hayvan satırında gösterilecek küçük ikonlar: `sick` (hasta), `cold` (üşüyor), `cured` (iyileşti, kısa süreli).
- **Komşu hanedanlar (Adım 55):** Köy paneli "Haneler" sekmesi ve soy ağacı. İkonlar: `head` (hane başı, şu an ★), `heir` (varis), `extinct` (soyu tükenen hane), `wedding` (köy düğünü bildirimi), `our_blood` (oyuncunun soyundan gelen kişi).

Toplam 8 ikon, 16x16 tuval.

## Tarif
315 ile aynı: `create_image_pixflux` ya da `create_map_object`, 210'un arayüz yönü, tek renk koyu kontur, düz gölge, ışık sol-üstten, 3-5 renk. Anlam şekilden okunmalı:
- `sick`: kıvrık yaprak ve küçük damla (otacı merhemi çağrışımı); `cold`: kar tanesi; `cured`: yeşil tomurcuk.
- `head`: basit taç değil, **anahtar** (hane başı evin anahtarını taşır; dönem uygun); `heir`: küçük anahtar; `extinct`: sönmüş mum; `wedding`: iki halka; `our_blood`: işleme (cross-stitch) dokulu küçük kalp, marka diliyle uyumlu bordo.

## Doğrulama
16x16, yarı saydamlık yok, renk sayısı ≤12, 3x ölçekte koyu (#333333) ve açık zemin üstünde okunur; `python tools/art/style.py check --family arayuz <dosyalar>`. Toplu önizleme x4.

## Notlar
Anahtar → dosya adı: `ui_icon_animal_<durum>`, `ui_icon_house_<anahtar>`. Bağlama (`GameRoot` hayvan etiketi, `VillagePanel.DrawHouses`) kabulden sonra yapılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
