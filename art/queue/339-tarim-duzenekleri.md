---
id: "339"
title: Tarım düzenekleri ve saraç (5)
status: accepted
claimed_at: 2026-11-18T11:50Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
priority: 20
family: esya
style_gate: false
cost_est: 8
cost_max: 12
depends_on: ["301"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_canal_se.png
  - Assets/Art/Iso/_aday/iso_item_water_wheel_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_millstone_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_tanning_rack_2x1_se.png
  - Assets/Art/Iso/_aday/iso_item_saddler_bench_2x1_se.png
created: 2026-11-17
---
# 339 Tarım düzenekleri ve saraç

## Amaç
Ortaçağ mekanizmaları (katalogda "Tarım düzenekleri"): kuyudan çekilen **kanal** tarlayı sular, kanala değen **su çarkı** yanındaki **değirmen taşını** çevirir ve taş her sabah arpayı yeme öğütür. Deri zinciri: kesimden çıkan ham deri **tabak germesinde** deri olur, **saraç tezgâhında** eyer dikilir.

## Tarif
Ayrıntılı yöntem **301'deki ile aynıdır** (`create_map_object` stil eşleme, ayak izi kılavuzu, ikinci deneme kuralı). Ahşap olanlar ham, açık tonlu.
- kanal (1x1) = short straight stone-lined irrigation channel segment with water, flush with the ground (yan yana dizilince birleşmeli: uçları açık); su çarkı (2x2) = wooden undershot water wheel on a stone base; değirmen taşı (2x2) = horizontal millstone on a wooden frame with gears; tabak germesi (2x1) = wooden frame with a stretched hide; saraç tezgâhı (2x1) = leatherworker bench with tools and a saddle in progress.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`, ayak izi kılavuzuyla örtüşme (301 ölçütleri). Kanal parçası iki yanından komşusuna ekleşmeli (yan yana iki kopyayla bak).

- 2026-11-18 (işçi): kanal teslim edilmedi (su yok, kırmızı dal benzeri çıktı); diğer 4 teslim. Ayrıntı: art/results/339-tarim-duzenekleri.md

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-18: yeniden `pending` (Claude, kurtarma): 2026-11-18T09:53Z sahiplenmesinden sonuç gelmedi.
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
