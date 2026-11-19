---
id: "423b"
title: Yaşlı erkek SE yönünde kalan küreği sil (423 devamı)
status: review
claimed_at: 2026-11-19T12:24Z
approved_by: user        # kullanıcı 19.11.2026: "kabul ediyorum" (423 önerisi)
priority: 60
family: karakter
style_gate: false
cost_est: 5
cost_max: 12
depends_on: ["423"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_elder_m_r84_idle_se_v3.png
  - art/results/423b-yasli-erkek-se-kurek.md
created: 2026-11-19
---
# 423b Yaşlı erkek SE: küreği sil

423'te se yönünde kutu maske (26,62,42,84) küreğin bıçağını kapsamadı. 423 sonuç dosyasındaki öneriyle yeniden:
- Kaynak: `iso_char_elder_m_r84_idle_se.png` (415 orijinali, v2 değil).
- Maske kutusu x 24-46, y 60-84; tuval alttan genişletilerek 96x96 (bıçak alt kenara değiyor), aynı ofsetle 84x84'e kırp.
- Aynı istem ve araç (423). Çıktı `_se_v3.png`; maske dışı piksel farkı 0.
- Önce/sonra: `423b-oncesonra.png` (415 se, 423 v2 se, v3 se; 4 kat).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
