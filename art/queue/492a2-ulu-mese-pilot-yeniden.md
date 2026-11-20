---
id: "492a2"
title: "Ulu meşe, dört mevsim (pilot, yeniden: ağaç evsiz, mevsimler düzenlemeyle)"
status: running
claimed_at: 2026-11-20T23:20Z
approved_by: user        # otomatik akış (art/budget.md); kullanıcı 21.11.2026: "bulgularının tamamını tek prda yap" (Project belgesi 66b)
priority: 10
family: bitki
style_gate: true
cost_est: 6
cost_max: 12
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_greattree_oak_{spring,summer,autumn,winter}_se.png
created: 2026-11-21
---
# 492a2 Ulu meşe (pilot, yeniden)

## Amaç
492a'nın yeniden denemesi (492a `failed`: yaz görseli teknik kapıları geçti ama ağaç evi platformu çizildi; `create_map_object`'te
`init_image` olmadığı için mevsimler aynı gövdeyle üretilemedi). Arsadaki ulu meşe şu an küçük, koyu yeşil bir lolipop yer tutucu
(tester raporu 65 C1, 66 §1.3). 492b ve 492c bu pilota bağlanır.

## Tarif
- **Tuval ve ölçü 492a ile aynı, kullanıcı kararı 20.11.2026 21:16 seçenek (b):** stil eşleme, tuval 160x224, pivot (80, 36),
  ayak izi 2x2. Ölçek tablosu (`docs/gorsel-yonerge.md` "Ölçek referansı"): pivot üstü **185-215 px**, yeniden ölçeklenen sıradan
  meşenin (772, 120-140 px) en az 1,4 katı.
- Yaz görseli: `create_map_object`, bitki ailesi referansları. Açıklama (492a'dan **"treehouse platform" çıkarıldı**):
  `ancient enormous oak tree, very thick gnarled trunk with deep bark furrows and exposed roots, wide spreading crown of dark green leaves, a few broad horizontal boughs, isometric view, no ground, no buildings, no platform, no ladder`.
- Öteki üç mevsim yaz görselinden **`edit_image_pro_flash`** ile (görsel girdi = yaz görseli; gövde ve dal iskeleti piksel piksel
  aynı kalmalı, yalnızca yaprak örtüsü değişir; işçinin 492a önerisi):
  - ilkbahar: `same tree, fresh light green sparse spring leaves`;
  - güz: `same tree, golden yellow and rust brown autumn leaves, a few fallen leaves on the roots`;
  - kış: `same tree, bare leafless thick branches, light snow on the boughs`.
- Maliyet ≈ 1 + 3 düzenleme ≈ 6 gen.

## Doğrulama
`validate_iso.py --kind object`, `style.py check --family bitki`, `python tools/art/scale_check.py <png>...` (185-215 px; KALDI
teslim edilmez). Dört mevsim yan yana: gövde ve dallar dördünde aynı yerde mi. Sıradan meşe (`iso_tree_oak_grown_se`, 772 gelmişse
`_v2`) ile yan yana boy oranı.

## Notlar
- 492b ve 492c bu işe bağlanır (`depends_on` 492a2). 493t (ağaç ev katmanları) tacın ortasına hizalanır; platform burada çizilmez.
- Kod: `ItemViews.AddGreatTree` adı `iso_greattree_<tür>_<mevsim>_se`, pivot (80, 36). Kod değişikliği gerekmez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
