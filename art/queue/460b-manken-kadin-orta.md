---
id: "460b"
title: "P1 manken: kadın Orta, anahtar renkli, durma + yürüme, 8 yön"
status: pending
approved_by: pending     # kullanıcı kararı 2026-11-19: karakter seti PixelLab kotası sıfırlanınca (2026-12-18) üretilecek; o gün user yap
family: karakter
style_gate: false
cost_est: 45
cost_max: 60
depends_on: ["460a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_ham/katman/lay_src_f_orta_idle_<yön>_00.png
  - Assets/Art/Iso/_ham/katman/lay_src_f_orta_walk2_<yön>_<NN>.png
  - Assets/Art/Iso/katman/lay_body_f_orta_idle_<yön>_00.png
  - Assets/Art/Iso/katman/lay_body_f_orta_walk2_<yön>_<NN>.png
  - art/results/460b-manken-kadin-orta.md
created: 2026-11-20
---
# 460b P1 manken: kadın Orta

## Amaç
Katmanlı karakterin tabanı (Project belgesi 43 §4.3 ve §6 P1, 43b). Oyunda saç, sakal, renk ve beden seçimi bu mankenin üstüne çizilir (`LayeredCharacterArt`, `LayerComposer`). Manken görseli gelene kadar oyun bugünkü 84 px görseli kullanır; bu iş tutarsa karakter yaratma ekranındaki her seçim dünyada gerçekten görünür.

## Tarif
- **Referans (oran ve stil):** kabul edilmiş 84 px kadın taban `Assets/Art/Iso/_aday/anim/iso_char_woman_r84_idle_<yön>.png` (8 yön). Manken aynı tuval, aynı boy, aynı baş boyutu ve aynı ayak satırında olmalı.
- **Karakter:** `create-character-pro`, `rotate_character`, referans `iso_char_woman_r84_idle_se.png`, 8 yön. Açıklama:
  `same woman, same body proportions and pose, completely bald head with no hair, no beard, no hat, flat colour-key clothing: plain red tunic #D83030, blue sleeve cuffs and hem #3050D8, green trousers #30A848, cyan belt #28C0C0, bright yellow shoes #D8D800, skin #D9A877, eyes with white sclera #F0F0F0 and magenta iris #E040E0, violet eyebrows #7030C0, dark outline #2A1E1E, pixel art, 3-4 tones per colour, no dithering, no gradients, no background`
- **Yürüme:** 018b2 yöntemi aynen (d): `animate_character`, `pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` o yönün durma karesi, metin: `walking at a calm steady pace, a full stride cycle with the left foot then the right foot stepping equally, head and shoulders level, no bouncing, no limping`. 018b2'deki ücretsiz ayak hizalaması ve kapanış karesi kuralı aynen.
- **İşleme (0 gen):** `python tools/art/manken.py regions Assets/Art/Iso/_ham/katman/lay_src_f_orta_*.png --out-dir Assets/Art/Iso/katman`
  - Bölge haritaları çapalarıyla (mavi kanal) yazılır. `KALDI` alan kare (anahtar renge oturmayan piksel > %3) teslim edilmez.
  - `python tools/art/manken.py preview <kare> --out art/results/460b-onizleme-<yön>.png` ile 4 yön önizleme.

## Doğrulama
- `python tools/art/ramps.py check --ref body Assets/Art/Iso/_ham/katman/lay_src_f_orta_*.png` (GEÇTİ şart).
- `manken.py regions`: her kare GEÇTİ; `manken.py anchors Assets/Art/Iso/katman/lay_body_f_orta_*.png --out art/results/460b-capalar.json`: her karede baş çapası var; G, GD, D, GB, B yönlerinde iki ya da bir göz bulunur, K yönünde göz yok.
- Sekme ölçütü 018b2 ile aynı (kafa ≤ 3 px). Baş çapası kareler arasında en çok 3 px oynar (saç bu çapaya oturur).
- Yan yana: 84 px kadın taban (R) ve manken (Y), `style.py sheet` ile.

## Notlar
- Dosya adı: `lay_src_f_orta_<anim>_<yön>_<NN>.png` (ham), `lay_body_...` (işlenmiş). Yönler: e, ne, n, nw, w, sw, s, se.
- Etkileşim kareleri (018c-e eşi) bu pilot kabul edilince ayrı işlerde.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
