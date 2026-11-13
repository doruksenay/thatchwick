---
id: "000"
title: Kısa ad
status: pending          # pending | running | review | accepted | rejected | failed | blocked
approved_by: auto        # auto (cost_max <= 25) | user (daha pahalı ya da kararı kullanıcıya ait) | pending (kullanıcı kararı bekleniyor, çalışmaz)
family: esya             # zemin | yapi | esya | bitki | karakter | hayvan | arayuz  (art/style/references.json)
style_gate: false        # true: ailenin stil pilotu (türevleri buna bağımlı yazılır)
cost_est: 10             # generation
cost_max: 15             # bunu aşarsa dur
depends_on: []           # örn. ["001"]: hepsi accepted olmadan başlamaz
max_attempts: 1
pixellab: true           # false ise PixelLab çağrılmaz (betik ya da rapor işi)
outputs:
  - Assets/Art/Iso/iso_ground_grass_a.png
created: 2026-11-08
---
# 000 Kısa ad

## Amaç
Bu varlık oyunda ne işe yarıyor, nerede görünecek.

## Tarif
Araç, ayarlar, ayrıntılı açıklama metni, referans dosyalar. Stil eşlemesi kullanan araçlarda, tarif başka referans yazmıyorsa ailenin `art/style/references.json` görselleri referanstır. İşçi bunu olduğu gibi uygular; araç açıklamasıyla çelişki varsa durur ve `## Sorular` ekler.

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family <aile> <dosya> ...` (KALDI teslim edilmez, UYARI raporda yazılır) ve işe özgü ek kontroller (döşeme testi, ayak izi, ışık sırası). Başarı ölçütleri burada yazılır.

## Notlar
Dosya adı kuralı, pivot, bağımlılık açıklaması.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
