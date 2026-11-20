---
id: "414t"
title: "Yaban hayvanları tamamlama: kızıl tilki, karaca, kurt, yaban domuzu, ayı (8 yön)"
status: running
claimed_at: 2026-11-20T10:33Z
approved_by: user        # kullanıcı kararı 2026-11-20 (sohbette hayvan incelemesi)
priority: 16
family: hayvan
style_gate: true
cost_est: 35
cost_max: 60
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wild_fox_v2_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_roe_deer_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_wolf_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_boar_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_bear_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-20
---
# 414t Yaban hayvanları tamamlama

## Neden
414'te (dosya `414-yaban-buyuk-hayvanlar.md`, iş kimliği 410) yalnız tilki teslim edilmişti ve tilki griydi. Karaca, kurt, yaban domuzu ve ayının birer yönü stil kapısında kalmış, türler bütünüyle atılmıştı. Kullanıcı 414'ü reddetti (2026-11-20).

## Tarif
414'teki tarifin aynısı (375 tarifi, 8 yön, v3 döndürme, `style.py fit --family hayvan`, açıklama eki `earthy muted medieval palette, desaturated`). Farklar:
- **Tilki (v2):** **kızıl tilki**, gri değil. Turuncu-kızıl sırt, beyaz göğüs ve kuyruk ucu, siyah bacak altları ve kulak arkası. Oturur değil, dört ayak üstünde ayakta (karaca ve kurtla aynı duruş).
- **Karaca:** ince bacaklı, kızıl-kahve, beyaz kuyruk aynası, erkekte kısa üç çatallı boynuz.
- **Kurt:** gri-kahve, köpeklerden (379) belirgin iri, uzun bacaklı, kuyruk aşağıda.
- **Yaban domuzu:** koyu kahve-siyah kıllı, yüksek omuz, küçük dişler. Domuz 2'den belirgin farklı: daha kıllı, daha uzun burunlu.
- **Ayı:** boz ayı, iri ve kahve, omuz kamburu. Ölçek: köylünün iki katından iri.
- **Stil kapısı (bu işe özel):** bir türün bir yönü KALDI ise tür bütünüyle atılmaz. Yalnızca o yön v3 döndürmeyle en çok 2 kez yeniden denenir. Yine kalırsa `style.py fit --family hayvan` ile kurtarma denenir. O da kalırsa geçen yönler teslim edilir, kalan yön raporda ve `## Notlar`da yazılır. (Kullanıcı kararı 2026-11-20: oyunda inek, at, keçi ve yaban hayvanları için hiç sprite yoktu.)

## Doğrulama
414 doğrulaması. Yan yana sayfası: `art/results/414t-yanyana.png`. Her türün SE yönü, köylü ve köpek 1 ile aynı satırda.

## Notlar
