---
id: "004b"
title: Toprak yol seti, dış çizgisiz (otomatik karo)
status: superseded   # 004c bunun yerine geçti
approved_by: user        # kullanıcı: yollar ızgara gibi görünüyordu (18.11.2026)
family: zemin
style_gate: false
cost_est: 10
cost_max: 15
depends_on: ["004"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_path_dirt_00.png ... iso_ground_path_dirt_17.png (004'ün üstüne yazar)
created: 2026-11-18
---
# 004b Toprak yol seti, dış çizgisiz

## Neden
004 seti `create_path_tiles` varsayılan outline moduyla üretildi: her karonun elmas kenarında koyu bir çizgi var
(ölçüm: kenar parlaklığı 0,31, iç 0,47). Yan yana döşenince yol ızgara/zincir gibi görünüyor. Kod şimdilik yolları
dikişsiz `iso_ground_dirt_a/b` ile çiziyor (`IsoArt.PathSetSeamless = false`).

## Tarif
- Araç: `create_path_tiles`, isometric 64, açı 30, derinlik 0; **outline: none / kapalı** (dış çizgi yok).
- Palet: `iso_ground_grass_a` ve `iso_ground_dirt_a` renkleri; toprak tonu `iso_ground_dirt_a` ile aynı olmalı ki
  tam karo (17) ile toprak zemin aynı görünsün.
- Kenar geçişi yumuşak: yol ile çimen arasında 2-3 piksellik düzensiz geçiş, sert çizgi yok.
- Aynı topoloji ve dosya numaraları (004 sonuç raporundaki tablo; bit 0=N 1=E 2=S 3=W).

## Doğrulama
- `validate_iso.py --kind ground` her karoda.
- Dikiş testi: 2 karo genişliğinde düz yol, L dönüş, T ve kavşak döşenip yan yana sayfasında bakılır; karo sınırı görünmemeli.
- Ölçüm: elmas kenar piksellerinin ortalama parlaklığı iç piksellerle ±0,03 içinde (outline yok).

## Kabul edilince
`Assets/Scripts/Game/IsoArt.cs` içinde `PathSetSeamless = true` yapılır; `WorldView.PathArt` komşu maskesine göre
düz, köşe, T, kavşak ve yol sonu karolarını seçer. 2 karoluk yolların iç kenar karoları için gerekirse ayrıca
"yol kenarı" (yarım toprak) varyantları istenir.
