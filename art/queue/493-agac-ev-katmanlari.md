---
id: "493"
title: "Ağaç ev katmanları: 5 tür, harap hâlleri, yapım evreleri, kar, Çınar Aşevi"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026, Project belgesi 52 (5 tür onaylı, köşk 1.0'da)
priority: 10
family: yapi
style_gate: false
cost_est: 16
cost_max: 32
depends_on: ["492a", "492c"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_treehouse_{nest,lookout,fort,hut,kiosk}_se.png
  - Assets/Art/Iso/_aday/iso_treehouse_{nest,lookout,fort,hut,kiosk}_ruin_se.png
  - Assets/Art/Iso/_aday/iso_treehouse_site_{1,2,3}_se.png
  - Assets/Art/Iso/_aday/iso_treehouse_snow_se.png
  - Assets/Art/Iso/_aday/iso_treehouse_snow_big_se.png
  - Assets/Art/Iso/_aday/iso_treehouse_inn_se.png
created: 2026-11-20
---
# 493 Ağaç ev katmanları

## Amaç
Belge 52 §4: ulu ağacın üstüne çizilen ağaç ev. Ağaç ve ev ayrı katmandır: aynı ev görseli dört 2x2 ulu ağacın (meşe, ıhlamur, ceviz, kestane) tacına oturur. Kod: `GameRootTrees.TreehouseArtKey` (yapımda `site_1..3`, bitince tür, harapta `_ruin`, kışın üstüne `snow`). Ev içi görünmez (köşkün içi oyunda ayrı kat olarak çizilir; bu görsel köşkün dış görünüşü).

## Tarif
Yapı ailesinin izometrik tarifi (`create_image_pixflux`, izometrik görünüm, `no_background=true`), **tuval 160x224, pivot (80, 36)** (492a ile aynı, 20.11 kararıyla küçüldü; çınar aşevi 192x192, pivot (96, 48)).
- **Yalnızca ağaç ev çizilir, ağaç çizilmez.** Yapı, tuvalde pivotun 95-175 px üstünde, tacın dalları arasına oturacak yerdedir; gövdeye inen ip merdiven ya da tahta basamaklar pivotun hemen sol üstüne (ağacın önüne) iner.
- Hizalama: üretimden sonra `uv run --with pillow` ile her görseli 492a'nın yaz ve kış görsellerinin üstüne bindir (`art/results/493-bindirme/`); platform dala oturmuyorsa görseli dikey kaydırarak düzelt (en çok ±12 px), yeniden üretme.
- Dönem: ortaçağ köyü; ahşap, keten ip, saman, kiremit. Çivi başları görünmez, boya yok (köşkte koyu cila olabilir).

| Ad | Açıklama metni |
|---|---|
| `nest` (Dal Yuvası) | `small rough plank platform wedged between tree boughs, low rail of sticks, rope ladder hanging down, children's treehouse` |
| `lookout` (Gözcü Kulesi) | `tall narrow wooden lookout platform high in a tree, railing of poles, tiny pointed shingle roof, long ladder` |
| `fort` (Tahta Kale) | `small wooden fort on a tree platform with crenellated plank walls and a little cloth pennant flag on a pole` |
| `hut` (Saz Damlı Kulübe) | `tiny closed wooden hut with a steep thatched straw roof sitting on a tree platform, small round window, rope ladder` |
| `kiosk` (Ağaç Köşkü) | `elegant half-timbered tree pavilion with carved railings and a red clay tile roof on a broad platform, wooden stairs, like the 17th century Pitchford Hall treehouse` |
| `<tür>_ruin` | aynı yapı (`init_image` olarak kendi görseli), kırık tahtalar, sarkmış ip, eğik çatı, yosun |
| `site_1` | `a few planks and a ladder leaning against tree boughs, coil of rope, construction start` |
| `site_2` | `half-built bare plank platform frame in tree boughs, loose planks, ladder` |
| `site_3` | `nearly finished plank platform with partial walls of a small treehouse, scaffolding ladder` |
| `snow` | `snow caps on a small treehouse roof and platform edges only, white snow shapes` (yalnızca kar katmanı, 2x2 evlerin hepsine uyacak genişlikte) |
| `snow_big` | `snow` gibi, köşk ve aşevi için daha geniş |
| `inn` (Çınar Aşevi, 256x320) | `large half-timbered tree pavilion kitchen with a smoke hole, long balcony with benches, hanging lanterns, wide wooden stairs down the trunk, village communal kitchen in a great plane tree` |

- Maliyet ≈ 16 gen.

## Doğrulama
`validate_iso.py --kind object`, `style.py check --family yapi`. Ek: bindirme sayfası (`art/results/493-bindirme/`): 5 tür × 4 ulu ağaç; platform dalda, merdiven ağacın önünde. Fener içeren `inn` için `check`'e `--light`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
