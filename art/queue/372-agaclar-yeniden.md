---
id: "372"
title: Ağaçlar yeniden (meşe, huş olgun, çam genç, söğüt, erik genç)
status: accepted
claimed_at: 2026-11-18T12:40Z
priority: 10            # başlangıç haritasında görünenler önce (görsel analiz)
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 ("yap bunu": yeniden deneme işleri)
family: bitki
style_gate: false
cost_est: 7
cost_max: 14
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_oak_{young,grown}_se.png
  - Assets/Art/Iso/_aday/iso_tree_birch_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_pine_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_willow_{young,grown}_se.png
  - Assets/Art/Iso/_aday/iso_tree_plum_young_se.png
created: 2026-11-18
---
# 372 Ağaçlar yeniden

## Amaç
**Meşe hiç üretilmedi.** Başlangıç haritasındaki 14 ağaç meşe (`item.tree_oak`) ve hepsi hâlâ "lolipop" yer tutucu. 320 ve 319'un kurtarılamayan ağaçları da burada yeniden üretilir:
- huş olgun: tuhaf biçim, 52 renk;
- çam genç: çam değil, yuvarlak çalı;
- söğüt genç: tanınmıyor, çıplak ve zeminli;
- söğüt olgun: tabanında çim parçası;
- erik genç: çıplak dallar.

Kurtarılabilenler 371'de. Harita kenarı ormanı (370) bu işe bağlıdır.

## Tarif
Yöntem **320 ile aynı** (301 yöntemi, `create_map_object` stil eşleme, referanssız kılavuz + maske, tuval 96x112, `--n 1`, pivot 40,24). Farklar:

1. **Referanslar:** bitki ailesinin yeni referansları (`references.json`, 2026-11-18). Özellikle `iso_tree_plum_grown_se`: bu aileden kapıyı geçen tek ağaç; renk düzeyi ve gölgeleme bu.
2. **Açıklama eki** (her metnin sonuna): `, muted desaturated natural greens, earthy palette, simple shading, light from top-left, single trunk base centred on the tile, no ground, no grass, no soil patch, no shadow, transparent background`
3. **Üretimden sonra her dosyaya** `python tools/art/style.py fit --family bitki <ham> <çıktı>`. Ardından `check`. 48 renkten fazlaysa yerel 48 renk nicemleme (titreme yok).
4. **İkinci deneme:** yalnızca biçim tutmazsa (tür tanınmıyor, zemin var). Renk ya da doygunluk için ikinci deneme yapılmaz; `fit` bunu çözer.

| Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|
| `iso_tree_oak_young_se.png` | 44 | `young oak tree with a short sturdy grey-brown trunk and a small rounded crown of lobed oak leaves` |
| `iso_tree_oak_grown_se.png` | 88 | `mature broad oak tree with a thick gnarled grey-brown trunk, spreading branches and a wide rounded crown of dense lobed oak leaves` |
| `iso_tree_birch_grown_se.png` | 76 | `mature silver birch tree with a slender white trunk with black marks and a light airy crown of small pale green leaves` |
| `iso_tree_pine_young_se.png` | 44 | `young Scots pine sapling, narrow pointed conical shape, dark green needle tiers, thin brown trunk` |
| `iso_tree_willow_young_se.png` | 40 | `young willow tree with a short trunk and thin drooping branches of narrow grey-green leaves` |
| `iso_tree_willow_grown_se.png` | 84 | `mature weeping willow with a thick trunk and long hanging curtains of narrow grey-green leaves` |
| `iso_tree_plum_young_se.png` | 44 | `young plum tree with a thin trunk and a small round crown of green leaves, no fruit` |

- Maliyet 7 × 1 gen. İkinci deneme payı 7: `cost_max` 14.

## Doğrulama
- 301 tablosu, ayrıca her dosyada `fit` öncesi ve sonrası doygunluk.
- Yan yana sayfası. Olgunlar bir satırda: meşe, huş, çam (371'den), söğüt, erik (319). Gençler ikinci satırda.
- Ayırt edilme kontrolü:
  - türler silüetten tanınıyor mu: meşe geniş yuvarlak, huş beyaz gövdeli seyrek, çam sivri, söğüt sarkık;
  - meşe olgun köylünün yaklaşık 1,5 katı mı.
- Çimen karosu üstünde sayfa: tabanda zemin ya da çim lekesi yok.

## Notlar
- Oyun olgun görseli `IsoArt.Plant`'tan alır. Genç evre için kod tarafı, iş review'a geçince yazılır.
- Dönem: hepsi Eski Dünya türü.

## Sorular
- 2026-11-18 bulut şeridi 1: `get_balance` hesabı Tier 2 (Pixel Artisan), 4999/5000 gen gösteriyor; `art/budget.md` ise Tier 1 / 2000 gen yazıyor. Bütçe kuralı "plan Tier 1 değilse dur" olduğundan üretim yapılmadı (0 gen). Kullanıcı `art/budget.md` planını güncelleyip işi `pending` yapmalı.
- 2026-11-18 (kod oturumu): çözüldü. Plan `art/budget.md`'de Tier 2'ye güncellendi (kullanıcı onayı); iş `pending`e alındı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-18 kullanıcı kararı (sohbet, onay grubu 1): gelen üçü kabul (genç meşe, genç çam, olgun söğüt). Eksik dört ağaç 377 ile.
