---
id: "007"
title: Duvar malzeme örneği
status: review
approved_by: auto
family: yapi
style_gate: true
cost_est: 10
cost_max: 15
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - art/results/007-duvar-malzeme-ornek.png
created: 2026-11-08
---
# 007 Duvar malzeme örneği

## Amaç
Duvar parçalarını (008) üretmeden önce malzeme görünümünü kullanıcıya göstermek: ahşap çatkı + sıva orta çağ kulübesi. Çıktı oyun varlığı değil, örnek; `Assets/Art/Iso`'ya konmaz.

## Tarif
- Araç: `create_building_kit`, isometric 64, `layout=materials`, `wall_tiles=2`.
- Açıklama: medieval village cottage wall, exposed timber frame with plaster infill, muted warm tones, flat few-tone pixel art, no roof, no shadow.
- Çağrıdan önce aracın açıklamasındaki maliyeti yaz (`layout=materials` için 10 bekleniyor).

## Doğrulama
Palet köylü ve zemin paletiyle uyumlu mu (görsel not), düz/az tonlu mu, ışık sol-üstten mü.

## Notlar
Örneği `art/results/007-duvar-malzeme-ornek.png` olarak kaydet. Kullanıcı kabul ederse 008 başlar.
