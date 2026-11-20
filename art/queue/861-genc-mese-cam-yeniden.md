---
id: "861"
title: "Genç meşe ve genç çam yeniden (772 stilinde, _v2)"
status: pending
approved_by: user        # kullanıcı 21.11.2026 02:29: "ağaçlar çok tutarsız, birbirine benzemiyor; daha cozy ağaçlar"
priority: 10
family: bitki
style_gate: false
cost_est: 6
cost_max: 14
depends_on: ["772"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_oak_young_se_v2.png
  - Assets/Art/Iso/_aday/iso_tree_pine_young_se_v3.png
  - art/results/861-*.md
created: 2026-11-21
---
# 861 Genç meşe ve genç çam yeniden

## Amaç
772 pilotunun iki eksiği: genç meşe teslim edilmedi (56 px, ölçek KALDI); genç çam ötekilerden kopuk (düz koyu, kümesiz, iğne dokusu yok; 772 stil sayfasında en sağdaki). Pilotun kalanıyla aynı aileden görünmeleri gerek.

## Tarif
772 ile aynı yöntem ve stil çapası (`art/queue/772-agaclar-yaban-olcek-v2.md` "Stil çapası" ve "Tarif"), ama referans/stil kaynağı **772'nin teslim ettiği görseller**: `iso_tree_oak_grown_se_v2.png` (meşe), `iso_tree_pine_grown_se_v2.png` (çam).
- Tuval 112x160, pivot (56, 24). Genç ağaçta **maskenin üst sınırı y≈40** (pivot üstü ~70 px), gövde tabanı pivotta sabit (772 raporundaki öneri).
- genç meşe: `young oak tree, slender trunk, small rounded crown of a few soft leaf clusters, same style as the mature oak`
- genç çam: `young scots pine, small layered tiers of soft rounded needle clusters, same cluster style and palette as the mature pine, thin reddish trunk`
- Ortak ek 772'deki gibi. Renk: `style.py fit --family bitki`.

## Doğrulama
`validate_iso.py --kind object`, `style.py check --family bitki`, `scale_check.py` (genç yaban 60-85 px; KALDI teslim edilmez). Stil sayfası `art/results/861-stil.png`: 772'nin 6 görseli + bu ikisi aynı taban çizgisinde.

## Notlar
Genç çam `_v3` adıyla gelir (772'nin `_v2`'si yerinde kalır, kullanıcı seçer).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
