---
id: "004"
title: Toprak yol seti
status: pending
approved_by: pending
cost_est: 10
cost_max: 15
depends_on: ["003"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_path_dirt_00.png ... iso_ground_path_dirt_17.png
created: 2026-11-08
---
# 004 Toprak yol seti

## Amaç
Köy yolları. Eski plan: 001-003 geçerse ayrı onayla yapılacaktı. `approved_by: pending`: kullanıcı onaylayana kadar çalışmaz; 003 kabul edildikten sonra `approved_by: user` yap.

## Tarif
- Araç: `create_path_tiles`, isometric 64, açı 30, derinlik 0 (18 karo; açı 90 olursa maliyet 15).
- Palet: kabul edilmiş toprak ve çimen karolarının renkleri. Düz, az tonlu, gölge yok.

## Doğrulama
`validate_iso.py --kind ground` (her karo), düz/köşe/T/kavşak karolarını döşeyip dikiş testi.

## Notlar
Adlar `iso_ground_path_dirt_<nn>.png` (brief'te ad yoktu, öneri). Karoların hangi yol topolojisine denk geldiğini rapora yaz.
