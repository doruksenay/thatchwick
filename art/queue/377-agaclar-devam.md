---
id: "377"
title: Ağaçlar devam (olgun meşe, olgun huş, genç söğüt, genç erik)
status: pending
priority: 10            # başlangıç haritasındaki 14 ağaç meşe
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 (onay grubu 1: "Kabul + eksikler için iş")
family: bitki
style_gate: false
cost_est: 4
cost_max: 14
depends_on: ["372"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_tree_oak_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_birch_grown_se.png
  - Assets/Art/Iso/_aday/iso_tree_willow_young_se.png
  - Assets/Art/Iso/_aday/iso_tree_plum_young_se.png
created: 2026-11-18
---
# 377 Ağaçlar devam

## Amaç
372'den gelmeyen dört ağaç. En önemlisi **olgun meşe**: başlangıç haritasındaki 14 ağacın hepsi meşe ve hâlâ yer tutucu.

372 raporunun bulgusu: referanssız kılavuzdaki düz renkli taban elması, büyük ağaçlarda (meşe, huş) ve erikte modeli taban parçası (çim ya da toprak elması) çizmeye itiyor. Metin bunu bastıramadı.

## Tarif
Yöntem **372 ile aynı**: `create_map_object` stil eşleme, bitki referansları, açıklama eki, üretimden sonra `style.py fit --family bitki`, ardından `check`. Tablo da 372'deki gibi:
- `oak_grown` 88;
- `birch_grown` 76;
- `willow_young` 40;
- `plum_young` 44.

Farklar:
1. **Kılavuz:** taban elması yerine yalnızca gövde genişliğinde küçük bir işaret. 96x112 tuvalin pivotunda (40,24) 12x6 piksellik koyu kahve bir elips; geri kalanı saydam. Maske eskisi gibi (pivotun üstündeki alan). Kılavuz betikle üretilir (`footprint_guide.py` değil, `uv run --with pillow` ile birkaç satır). Betik `art/tmp/` altına yazılır.
2. **Açıklamaya ek:** `, the trunk ends directly on the transparent background, no base plate, no turf, no diamond`.
3. **İkinci deneme hâlâ tabanlıysa** tek bir `inpaint` ya da `edit_image` çağrısıyla taban parçası saydamla değiştirilir (mask: pivotun altındaki ve gövde dışındaki alan). Bu ikinci denemenin yerine geçer, ek değildir.
4. Olgun meşenin yüksekliği genç meşenin (372, `iso_tree_oak_young_se.png`) yaklaşık 2 katı; ölçek sayfası için o kullanılır.

- Maliyet 4 × 1 gen. İkinci deneme ya da inpaint payı: `cost_max` 14.

## Doğrulama
- 372 doğrulaması.
- Ek kontroller:
  - tabanda çim ya da toprak elması yok (çimen karosu üstünde sayfa);
  - olgun meşe köylünün yaklaşık 1,5 katı;
  - genç meşe ile olgun meşe yan yana aynı türün iki evresi gibi.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
