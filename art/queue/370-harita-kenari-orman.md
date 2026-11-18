---
id: "370"
title: Harita kenarı ormanı (ağaç öbekleri ve çalılık şeritleri)
status: running
claimed_at: 2026-11-18T13:10Z
approved_by: user        # otomatik akış; kullanıcı onayı sohbette 2026-11-18 ("yap bunu", görsel analiz partisi)
family: bitki
style_gate: false
cost_est: 6
cost_max: 12
depends_on: ["372"]   # 320 başarısız oldu; ağaç türleri 372 ile yeniden (2026-11-18)
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_edge_trees_a.png
  - Assets/Art/Iso/_aday/iso_edge_trees_b.png
  - Assets/Art/Iso/_aday/iso_edge_trees_c.png
  - Assets/Art/Iso/_aday/iso_edge_trees_d.png
  - Assets/Art/Iso/_aday/iso_edge_undergrowth_2x1_a.png
  - Assets/Art/Iso/_aday/iso_edge_undergrowth_2x1_b.png
created: 2026-11-18
---
# 370 Harita kenarı ormanı

## Amaç
Görsel analiz (Project `26-gorsel-analiz.md`, P1 madde 9): harita kenarı sert, koyu bir boşluk. Kod partisinde kenara dışa doğru solan bir çimen şeridi eklendi (`WorldView.BuildFringe`). Bu iş, şeridin üstüne sık dizilecek, oynanamayan orman sınırı görsellerini üretir. Öbekler yalnızca haritanın dışında durur; eşya değildir.

## Tarif
Yöntem **301 ile aynı** (referanssız kılavuz, `create_map_object` stil eşleme, ikinci deneme kuralı, masaüstü kopyası). Farklar:
- Referans: 372 ve 371'in ağaçları (meşe, huş, çam). 320 başarısız olduğu için ağaçlar orada yeniden üretilir. Kenar öbekleri o ağaçlardan **daha koyu ve ayrıntısız** olmalı: oyun alanındaki etkileşimli ağaçlarla karışmasın, arka plan gibi okunsun.
- Ortak ek: `, dense medieval European forest edge seen from high top-down isometric view, dark muted greens, simple shading, single dark outline, light from top-left, no ground tile, no shadow, transparent background`
- Öbekler:

| # | Dosya (`_aday/`) | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama metni (+ ortak ek) |
|---|---|---|---|---|---|---|
| 1 | `iso_edge_trees_a.png` | 1x1 | 80x112 | `--n 1` | 76 | `cluster of two overlapping oak tree crowns, dark green, trunks barely visible` |
| 2 | `iso_edge_trees_b.png` | 1x1 | 80x112 | `--n 1` | 80 | `cluster of three tall dark pine trees close together` |
| 3 | `iso_edge_trees_c.png` | 1x1 | 80x112 | `--n 1` | 70 | `mixed clump of a beech and a birch crown, dark green with a few pale birch trunks` |
| 4 | `iso_edge_trees_d.png` | 1x1 | 80x112 | `--n 1` | 64 | `low dense clump of young oak and hazel trees, dark green` |
| 5 | `iso_edge_undergrowth_2x1_a.png` | 2x1 | 112x80 | `--n 2 --axis x` | 22 | `strip of dense dark undergrowth: brambles, ferns and young saplings` |
| 6 | `iso_edge_undergrowth_2x1_b.png` | 2x1 | 112x80 | `--n 2 --axis x` | 18 | `strip of low hedge and bracken, dark green with some brown` |

- Maliyet 6 × 1 gen; ikinci deneme payı 6.

## Doğrulama
301 tablosu. Ek kontroller:
- Sayfa: 12x6 karolık kenar şeridi, öbekler sık ve rastgele dizilmiş, önünde bir 372 meşesi ve köylü. Öbekler arka plan gibi mi okunuyor; etkileşimli ağaçtan ayırt ediliyor mu.
- Öbekler birbirine bindiğinde dikiş ya da çerçeve görünmüyor.

## Notlar
- Dönem: meşe, huş, kayın, çam, fındık, eğrelti; hepsi Eski Dünya türü.
- Kod tarafı (kenar şeridine öbek dizme, kamera sınırı) bu iş review'a geçince yazılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
