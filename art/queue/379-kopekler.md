---
id: "379"
title: Köpekler (375'in devamı, zemin lekesiz)
status: pending
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 (onay grubu 1: eksikler için iş)
family: hayvan
style_gate: false
cost_est: 9
cost_max: 16
depends_on: ["375"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_dog_{1,2,3,4}_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-18
---
# 379 Köpekler

## Amaç
375'te dört köpek adayı da teslim edilmedi:
- köpek 1, 2 ve 3'ün altında gri zemin lekesi çıktı;
- köpek 4 güneye değil yana bakıyordu.

375 raporuna göre leke koyun paletindeki griden gelmiş olabilir. Kediler kabul edildi.

## Tarif
Tarif **375 ile aynı** (106 tarifi, `fit --family hayvan`, teslim kuralı). Farklar:
1. **Palet zorlaması yok.** Renk `fit` ile aileye çekilir. Leke paletten geliyorsa böylece kaybolur.
2. **Açıklama ekine:** `, standing or sitting directly on transparent background, no ground disc, no floor shadow, no grey patch under the paws`.
3. **Köpek 4:** `facing the viewer (south), front view` vurgusu. Güney kontrolü önce yapılır; tutmazsa döndürme yapılmaz, aday raporlanır.
4. **Zemin kontrolü:** pençelerin altında 3 pikselden kalın opak gri şerit varsa aday teslim edilmez.

- Maliyet: 4 aday × 2 gen + azaltma ≈ 9. Güney kontrolünden dönen aday 1 gen. `cost_max` 16.

## Doğrulama
375 doğrulaması. Yan yana sayfası: kabul edilen kediler (`cat_1`, `cat_2`), koyun 1 ve domuz 2 ile.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
