---
id: "492c"
title: "Ulu çınar (kasaba meydanı, 3x3), dört mevsim"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026, Project belgesi 52 (karar 0-6: hazır aşevi)
priority: 10
family: bitki
style_gate: false
cost_est: 4
cost_max: 10
depends_on: ["492a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_greattree_plane_{spring,summer,autumn,winter}_se.png
created: 2026-11-20
---
# 492c Ulu çınar

## Amaç
Belge 52 §2 ve §5.4: şehir meydanının güneydoğusundaki ulu çınar. Tacında meyhanecinin işlettiği Çınar Aşevi köşkü (493'teki `iso_treehouse_inn_se`) durur; mevsim sonu Bereket Sofrası bu ağacın altında kurulur.

## Tarif
492a yöntemi, farkları:
- **Ayak izi 3x3, tuval 256x320, pivot (128, 56)**, yükseklik 270-300 px.
- Yaz açıklaması: `ancient colossal oriental plane tree, enormous pale mottled trunk with patches of peeling cream and grey-green bark, very broad dome crown of large maple-like leaves, isometric view, no ground`.
- Mevsimler: ilkbahar açık yeşil; yaz koyu yeşil; güz pas turuncusu; kış çıplak dallarda asılı kalmış top meyveler.
- Maliyet ≈ 4 gen.

## Doğrulama
492a doğrulaması (`--kind object`, 3x3 taban). Ek: 492a meşeyle yan yana, çınar daha iri ve kabuğu alacalı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
