---
id: "612"
title: "Köy meydanı onarımı: 7 yapının yıkık ve onarılmış hâli, iki iskele katmanı"
status: running
claimed_at: 2026-11-20T14:26Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("köy meydanının onarımı ... bu olsun"), Project belgesi 64c
priority: 15
family: yapi
style_gate: false
cost_est: 18
cost_max: 36
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_square_{well,hearth,cellar,millwheel,belltower,market_hall,bridge}_se.png
  - Assets/Art/Iso/_aday/iso_square_{well,hearth,cellar,millwheel,belltower,market_hall,bridge}_ruin_se.png
  - Assets/Art/Iso/_aday/iso_square_scaffold_{small,large}_se.png
created: 2026-11-20
---
# 612 Köy meydanı onarımı yapıları

## Amaç
Belge 64c §3: köyün ana hedefi meydanın onarımı. Oyun başında meydanda yedi yapı yıkık durur; her köy projesi birini onarır ve yapı haritada kalıcı olarak değişir. Her yapının iki hâli (yıkık, onarılmış) ve onarım sürerken üstüne binen ortak bir iskele katmanı gerekir. Kod `iso_square_<ad>` (onarılmış), `iso_square_<ad>_ruin` (yıkık) ve proje başlayınca yıkık hâlin üstüne `iso_square_scaffold_<boy>` çizer.

## Tarif
Yapı ailesinin izometrik tarifi (`create_image_pixflux`, izometrik görünüm, `no_background=true`), yalnızca SE yönü (meydan yerleşimi dönmez). Ayak izi ve pivot `tools/art/footprint_guide.py` ile; tuval ve pivot aşağıda.

Önce **onarılmış** hâl üretilir; **yıkık** hâl kendi onarılmış görseli `init_image` olarak verilerek üretilir (aynı yapının harap hâli, ayak izi aynı kalır).

| Ad | Ayak izi | Tuval | Onarılmış (açıklama metni) | Yıkık (ek metin) |
|---|---|---|---|---|
| `well` | 2x2 | 128x128 | `village square well with a round dressed-stone curb, wooden roof frame on two posts, windlass with rope and wooden bucket` | `collapsed curb stones, broken roof posts lying on the ground, no rope, weeds` |
| `hearth` | 2x2 | 128x128 | `communal open hearth in a village square: low round stone ring, iron cauldron on a tripod, log benches around` (sönük; alev çizilmez) | `scattered fire-blackened stones, cracked ring, no cauldron, ash` |
| `cellar` | 2x1 | 128x96 | `stone-arched entrance of a communal earth cellar dug into a grassy mound, heavy plank double door, steps going down` | `half-collapsed arch, door hanging off one hinge, earth spilling over the steps` |
| `millwheel` | 2x2 | 128x160 | `large wooden undershot water wheel in a timber frame with a short stone race wall` (değirmen binasına bitişik konur; bina çizilmez) | `wheel with missing paddles and broken spokes, tilted, stuck` |
| `belltower` | 2x2 | 128x256 | `tall square timber bell tower with a stone base, shingled pyramid roof, single bronze bell visible in the open belfry, rope hanging` | `roofless stone stump with broken timber frame, bell lying on the ground at the base` |
| `market_hall` | 3x3 | 192x192 | `open-sided medieval market hall: timber posts on stone pads, steep clay tile roof, no walls, two empty trestle stalls underneath` | `only some posts standing, roof collapsed in a heap of broken tiles and timber` |
| `bridge` | 3x2 | 192x128 | `arched stone footbridge wide enough for an ox cart, low parapet walls` (yalnızca köprü; su çizilmez, oyun ırmak karolarının üstüne koyar) | `narrow makeshift plank footbridge laid over the broken stumps of a stone bridge` |
| `scaffold_small` | 2x2 | 128x192 | `wooden construction scaffolding of lashed poles and planks, a ladder, a pile of stones and a rope pulley` (yalnızca iskele) | — |
| `scaffold_large` | 3x3 | 192x256 | aynı, daha geniş ve yüksek | — |

- Dönem: ortaçağ köyü; taş, ahşap, saman, kiremit, bronz çan. Boya, cam, metal çatı yok.
- Işık sol üstten; gölge pişirilmez.
- Maliyet ≈ 18 gen (16 yapı görseli + 2 iskele).

## Doğrulama
`python tools/art/validate_iso.py --kind object <dosya> ...`, `python tools/art/style.py check --family yapi <dosya> ...`. Ek:
- Yıkık ve onarılmış hâllerin taban elması aynı piksellerde (yan yana sayfa: `art/results/612-yanyana.png`, her satırda yıkık | onarılmış | yıkık + iskele bindirmesi).
- `bridge`: köprü uçları tuvalin sol ve sağ köşelerine kadar uzanır (yol karosuna bağlanacak), altı saydam.
- Dönem kuralları (CLAUDE.md, Project belgesi 20).

## Notlar
- Yerleşim ve kod: belge 64a Aşama E (meydan) ile birlikte; konumlar `markers.json`'a `square:<ad>` olarak girer.
- Eşleme gelene kadar kod yıkık hâl için yer tutucu çizmez; yapı yoksa onarım projesi listede görünür ama haritada işaret olmaz.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
