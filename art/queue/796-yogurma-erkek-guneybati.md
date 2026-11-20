---
id: "796"
title: "Zincir pozu tamamlama: erkek güneybatı yoğurma (1 klip)"
status: failed
claimed_at: 2026-11-20T23:08Z
approved_by: user        # kullanıcı 21.11.2026: 589 "kabul + eksik yoğurma yeniden" (seçmeli liste)
priority: 5          # kullanıcı 21.11 02:02: "işçiye ürettir" (mutfak işleri önce)
family: karakter
style_gate: false
cost_est: 1
cost_max: 2
depends_on: ["589"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_man_r84_knead_sw_<NN>.png
  - art/results/796-*.md
created: 2026-11-21
---
# 796 Erkek güneybatı yoğurma (1 klip)

## Amaç
589'da bu tek klip teslim edilmedi: ham kare 120x148, 84x84 tuvale sığmadı ve hareket çok genişti (`art/results/589-zincir-poz-mutfak-1.md`, `589-red/`). Kod şu an bu yönde en yakın poza düşüyor.

## Tarif
589 ile aynı karakter ve ayarlar: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), `animate_character`, yön `south-west`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Tek çağrı.

Eylem metni hareketi dar tutar: `standing close to a waist-high trough, elbows bent and close to the body, kneading dough with both hands in small pushes and folds, upper body stays upright, rhythmic loop`.

## Ortak işleme
589 ile aynı (018c "Ortak işleme": 84x84 tuval, ayak satırı y=81, pivot, 012b paleti). İçerik kırpılmaz: sığmazsa teslim edilmez, rapora yazılır.

## Doğrulama
018a ölçütleri; aynı karakterin diğer üç yön yoğurma klibiyle yan yana GIF (`art/results/796-man-knead.gif`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
