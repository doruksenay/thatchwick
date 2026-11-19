---
id: "004d"
title: Toprak yol - çim geçiş seti, kalan karolar ve süsler
status: pending
approved_by: pending     # 004c pilotu kullanıcı tarafından accepted olunca "user" yapılır
family: zemin
style_gate: false
cost_est: 30
cost_max: 55
depends_on: ["004c"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/iso_path_dirt_fill_b.png, _c.png
  - Assets/Art/Iso/iso_path_dirt_wang_00.png ... _15.png (004c'de üretilen 01, 03, 07 hariç)
  - Assets/Art/Iso/iso_path_decor_*.png (6-8 süs)
created: 2026-11-19
---
# 004d Toprak yol – kalan karolar

Tarif, palet ve doğrulama: `art/queue/004c-toprak-yol-gecis-seti.md` (tam set tarifi). 004c'de kabul edilen dört karo
stil referansıdır (aynı palet, aynı kenar dili); PixelLab'e referans görsel olarak verilir.

## Doğrulama
- `validate_iso.py --kind ground` her karoda.
- Tam dikiş sayfası (`art/results/004d-yanyana.png`): 2 ve 3 karoluk düz yol, L, T, kavşak, tek karoluk patika, yol sonu,
  kıvrılan yol; karo sınırı görünmemeli.
