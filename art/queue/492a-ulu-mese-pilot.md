---
id: "492a"
title: "Ulu meşe, dört mevsim (ulu ağaç stil pilotu)"
status: pending
claimed_at: 2026-11-20T10:46Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamasını tek pr'da hallet, gereken assetleri sıraya koy"), Project belgesi 52
priority: 10
family: bitki
style_gate: true
cost_est: 4
cost_max: 10
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_greattree_oak_{spring,summer,autumn,winter}_se.png
created: 2026-11-20
---
# 492a Ulu meşe (pilot)

## Amaç
Belge 52 §2: haritada kesilemeyen, yüzyıllık ulu ağaçlar. Arsadaki kesin ulu ağaç meşedir. Üstüne ağaç ev katmanı (493) oturur, köşk (5x5) bu ağacın tacına kurulur. Kod görseli ada göre bulur (`GameRootTrees.AddGreatTree`): `iso_greattree_<tür>_<mevsim>_se`; mevsim görseli yoksa yaz görseline, o da yoksa yer tutucuya düşer.

## Tarif
319'daki ağaç yöntemi (`create_map_object` stil eşleme, bitki ailesi referansları), farkları:
- **Ayak izi 2x2** (gövde tabanı iki karoluk elmasın ortasında), **tuval 160x224, pivot (80, 36)** (kullanıcı kararı 2026-11-20 21:16: seçenek (b) "Stil eşlemeyle 160x224 ... bu olsun"; stil eşleme en çok 192x192 = 36.864 px alır, 160x224 = 35.840 px). Yükseklik 190-215 px: sıradan meşeden (`iso_tree_oak_grown_se`, 96x112) belirgin biçimde büyük ve yaşlı görünmeli.
- Açıklama metni (yaz): `ancient enormous oak tree, very thick gnarled trunk with deep bark furrows and exposed roots, wide spreading crown of dark green leaves, a few broad horizontal boughs low enough to hold a treehouse platform, isometric view, no ground`.
- Mevsimler aynı gövde ve dal iskeletiyle (ilk yaz görseli `init_image` olarak verilir, gövde piksel piksel aynı kalmalı):
  - ilkbahar: taze açık yeşil, seyrek yaprak;
  - yaz: koyu dolgun yeşil;
  - güz: altın sarısı ve pas kahvesi yaprak;
  - kış: yapraksız, çıplak kalın dallar (köşk ve platform dalların arasında görünecek).
- Taç ağacın en geniş yerinde ~150 px; taç ortası pivotun ~130 px üstünde (ağaç ev katmanı 493 buraya hizalanır).
- Maliyet ≈ 4 gen.

## Doğrulama
301 doğrulama tablosu (`validate_iso.py --kind object`, `style.py check --family bitki`). Ek: dört mevsim yan yana; gövde ve dallar dördünde aynı yerde mi; sıradan meşe ile yan yana boy oranı (ulu ağaç en az 2 kat).

## Notlar
Pilot: 492b ve 492c bunun stilini izler.

## Sorular
- `create_map_object` stil eşleme modu en çok 36.864 px alan kabul ediyor (`error: 192x256 too large for inpainting (max 192x192 = 36,864px)`); tarifteki 192x256 tuval (49.152 px) bu yöntemle üretilemez. Hiç generation harcanmadı.
- Seçenekler: (a) basit mod 192x256, ama 40.000 px üstünde düz renk zemin gelir, `remove_background` ek maliyet; (b) stil eşlemede 160x224 (35.840 px) tuval, taç ~150 px; (c) 192x192 tuvalde daha alçak ağaç. Tuval ve pivot kararı kullanıcıda; 492b/492c de aynı sınıra takılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
