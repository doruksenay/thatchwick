---
id: "350b"
title: Soy Mührü ve yadigâr ikonları yeniden (renk azaltma ve iki ikon yeniden)
status: review
claimed_at: 2026-11-19T12:16Z
approved_by: user        # kullanıcı onayı sohbette 2026-11-19 ("önerdiklerini yap": 350 için yeniden deneme)
family: arayuz
style_gate: false
cost_est: 3
cost_max: 8
depends_on: ["210"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_legacy_{seal,heirloom,descendants,married_out}.png
  - Assets/Art/Iso/_aday/ui_legacy_seal_large.png
  - Assets/Art/Iso/_aday/iso_fx_heirloom_glint.png
created: 2026-11-19
---
# 350b Soy Mührü ve yadigâr ikonları yeniden

## Amaç
350 hiçbir dosya teslim etmedi (`art/results/350-soy-muhru-yadigar-ikonlari.md`):
- Dört görsel iyiydi ama renk sınırını aşıyordu (16'lıkta en çok 8, 64'te en çok 16 renk): büyük mühür, parıltı, evlendirme ve soy.
- Mühür ve yadigâr ikonlarının 16x16'da şekli tutmadı.

## Tarif
1. **Renk azaltma (yeniden üretim yok):** `art/results/350-red/` altındaki dört görsel `reduce_colors` ile sınıra indirilir (çağrı başına 0,1 gen):
   - `ui_legacy_seal_large.png` (64, en çok 16 renk);
   - `iso_fx_heirloom_glint.png`;
   - `ui_icon_legacy_married_out.png` (16, en çok 8 renk);
   - `ui_icon_legacy_descendants.png` (16, en çok 8 renk).

   Şekil bozulmadıysa `cp -n` ile teslim edilir.
2. **Mühür ve yadigâr (yeniden üretim):** 212'nin yeni yöntemiyle. 32x32 tek ikon (`create_map_object` ya da `create_image_pixen` 32x32), en yakın komşuyla 16x16'ya indirme, `reduce_colors` ile en çok 8 renk. Açıklamalar 350 tarifindeki gibi:
   - mühür: kırmızı balmumu mühür, saz çatı ve köklü ağaç; 16'lıkta siluet ve bordo-kırmızı ton;
   - yadigâr: koyu ahşap sandık ya da balta sapı üzerinde küçük altın yıldız.

   Okunmazsa tek bir ikinci deneme.
3. Stil: 210 arayüz pilotunun paleti ve kontur kalınlığı. Ailede kabul referansı yok, kapı yalnızca uyarı verir.

- Maliyet: renk azaltma yaklaşık 0,4, iki ikon 2, ikinci deneme payı. `cost_est` 3, `cost_max` 8.

## Doğrulama
350 doğrulaması: renk sayısı sınırı, yarı saydam piksel 0, 16'lıkta okunurluk. Yan yana sayfası: altı görsel, 16'lıklar 4x büyütülmüş ve gerçek boyda.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
