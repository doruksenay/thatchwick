---
id: "462"
title: "P3 siluet ailesi: uzun entari, iki yöntemle (2 yön)"
status: pending
approved_by: pending     # kullanıcı kararı 2026-11-19: kota sıfırlanınca (2026-12-18); o gün user yap
family: karakter
style_gate: false
cost_est: 40
cost_max: 60
depends_on: ["460b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_ham/katman/lay_src_f_orta_entari_{a,b}_walk2_{se,s}_<NN>.png
  - art/results/462-entari-siluet-pilotu.md
created: 2026-11-20
---
# 462 P3 uzun entari: yöntem karşılaştırması

## Amaç
Belge 43 §4.2 katman C: silueti değiştiren giysi (uzun entari) her karede çizilmek zorunda. İki yolun maliyeti ve kalitesi karşılaştırılır; ucuz ve yeterli olan bütün siluet aileleri (tunik + pantolon, uzun entari, iş kıyafeti) için kullanılır.

## Tarif
Kadın Orta manken (460b), yalnız SE ve S yönü, yürüme (walk2) kareleri.
- **a:** `transfer-outfit-v2`; referans: mankenin SE durma karesinin üstüne inpaint ile çizilmiş ayak bileğine inen kırmızı anahtar renkli entari (`#D83030`, kenarı `#3050D8`, kemer `#28C0C0`); kareler 460b walk2 (istek başına en çok 3 kare).
- **b:** `create-character-pro` `rotate_character` entarili referansla + 018b2 yürüme yöntemi.
- Her iki sonuç `manken.py regions` ile işlenir (anahtar renge oturma oranı raporlanır).

## Doğrulama
- Rapor tablosu: yöntem × (harcanan gen, istek sayısı, anahtar renge oturma %, sekme ölçütü, göz kontrolü).
- Yan yana GIF: a ve b, iki yön.
- Kullanıcıya soru: hangi yöntem bütün aileler için kullanılsın.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
