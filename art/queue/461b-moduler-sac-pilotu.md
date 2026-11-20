---
id: "461b"
title: "P2b modüler saç: ortadan ayrık ön, at kuyruğu, tek örgü (5 yön)"
status: pending
approved_by: pending     # kullanıcı kararı 2026-11-19: kota sıfırlanınca (2026-12-18); o gün user yap
family: karakter
style_gate: false
cost_est: 30
cost_max: 45
depends_on: ["461"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/katman/lay_front_parted_<yön>_fr.png
  - Assets/Art/Iso/katman/lay_extra_ponytail_<yön>_{fr,bk}.png
  - Assets/Art/Iso/katman/lay_extra_braid_<yön>_{fr,bk}.png
  - art/results/461b-moduler-sac-pilotu.md
created: 2026-11-20
---
# 461b P2b modüler saç

## Amaç
Belge 43b §2 ve §6: saç kütle + ön + eklenti olarak ayrı parçalardır. 461'in iki kütlesi (short, shoulder) ve iki önü (straight, parted) ile iki eklenti (ponytail, braid) birleşince 8 saç çıkar; dikiş görünmemeli. Tutmazsa geri dönüş: saç tam stil olarak üretilir, sayı ~40-50 stile düşer.

## Tarif
461 ile aynı yöntem ve renk kuralı. Tabanlar: kadın mankenin 461 `mass shoulder` uygulanmış durma kareleri (5 yön).
- `front parted`: `hair parted in the middle, framing the face`
- `extra ponytail`: `hair gathered into a high ponytail at the back of the head`
- `extra braid`: `a single long braid hanging down the back`
Çıkarmada `--base` kütlesiz manken, `--src` kütleli kare kullanılır (yalnız yeni eklenen saç parça olsun).

## Doğrulama
- `ramps.py check --ref hair` GEÇTİ.
- 8 birleşimin (2 kütle × 2 ön × 2 eklenti, uyumsuz olan short + braid hariç) yan yana sayfası: dikiş görünmüyor, her biri tek parça çizilmiş gibi.
- Unity'de (kullanıcı): yaratma ekranında "Ayrıntılı" açıkken birleşimler yürürken doğru.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
