---
id: "820b"
title: "İç Sesler nakış yaması pilotu, ikinci deneme: üç tam yama, kurdele, dört kanaviçe ikon"
status: running
claimed_at: 2026-11-21T00:14Z
approved_by: user        # kullanıcı 21.11.2026 03:04: 820 sonucu gösterildi, "baştan ürettir parçaları" (kodla çizim önerisi reddedildi)
priority: 1
family: arayuz
style_gate: true
cost_est: 16
cost_max: 30
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_patch_{single,double,gold}.png
  - Assets/Art/Iso/_aday/ui_patch_ribbon_v2.png
  - Assets/Art/Iso/_aday/ui_icon_voice_{good_meal,nice_chat,grief,inspired}.png
created: 2026-11-21
---
# 820b İç Sesler nakış yaması pilotu, ikinci deneme

## Amaç
820'nin yerine (820 `failed`; rapor `art/results/820-yama-pilotu.md`, reddedilenler `art/820-red/`). Karakter kartında her iç ses bir **nakış yaması** olacak (Project belgesi 68a). Yamanın kumaş rengini kod hâle göre **palet değişimiyle** boyar; bu yüzden yama nötr keten renginde ve az tonla çizilir.

## 820'den dersler (bağlayıcı)
1. **Saydam dikiş katmanı istenmez.** PixelLab katmanı çerçeveli kare çiziyor. Bunun yerine **üç tam yama** üretilir: aynı yama, kenarda tek dikiş / çift dikiş / sırma dikiş.
2. 32x32 tuvalde nesne tuvali dolduruyor. Yama **24x24'lük alana**, ikon **16x16'ya** iner (aşağıda).
3. Yama tabanı minder ya da parşömen gibi çıktı (kabarık, gölgeli). İstenen: **düz, ince kumaş parçası**, üstten bakış, kabarıklık ve iç gölge yok.
4. Kurdele parşömen tomarı gibi çıktı. İstenen: **iki ucu V kesikli (çatal) düz kumaş şerit**.
5. İkonların rengi ve okunurluğu kullanıcının beğendiği yöndeydi; eksik olan **işleme (kanaviçe) hissi**, küçüklük ve az renk.

## Tarif
Her parça **ayrı çağrı**. Ortak ayarlar: arayüz paleti `art/style/palette_arayuz.json`, saydam zemin, `flat shading`, `low detail`, tek renk koyu ılık kontur, ışık sol üstten, saf siyah yok.

### A. Üç yama (24x24)
- Araç: `create_image_pixen` 32x32, ardından **içerik kutusu 24x24'e** kırpılır. Nesne 24'ten büyük çıkarsa en yakın komşuyla 24x24'e indirilir (yalnızca bu iş için izinli; 212'deki ikon yöntemi).
- Ardından `reduce_colors` (ya da yerel Pillow) ile **en çok 6 renk**: keten kumaş için **en çok 3 ton**, 1 kontur, dikiş ipliği için 1-2 renk. Palet değişimi keten tonlarını hâl rengine eşleyecek; keten tonları iplik renginden açıkça ayrı olmalı.
- Ortak açıklama: `a flat square patch of plain undyed linen cloth seen straight from above, thin fabric not a pillow, no puffiness, no inner shadow, slightly frayed edges, cut corners, dark warm brown outline, empty plain center`
  | Dosya | Ek |
  |---|---|
  | ui_patch_single | `, one row of short cream running stitches along the border just inside the edge` |
  | ui_patch_double | `, two parallel rows of short cream running stitches along the border` |
  | ui_patch_gold | `, one row of shiny gold thread stitches along the border and a tiny gold knot at each corner` |
- Üçünün **kumaş kısmı aynı** görünmeli (aynı ton, aynı doku); fark yalnızca dikişte. Gerekirse ilk yamayı `init_image` olarak öbür ikisine ver (güç 300-450).

### B. Kurdele (64x16)
- Araç: `create_image_pixflux` 64x32, içerik kutusu kırpılır (yükseklik 14-18).
- `a flat horizontal ribbon banner made of plain undyed linen cloth, both ends cut into a V-shaped swallowtail fork, a row of small running stitches along the top and bottom edge, empty middle for text, seen straight from the front, no scroll, no rolled ends, no paper`
- En çok 6 renk (keten en çok 3 ton). Dosya adı `ui_patch_ribbon_v2` (820'nin `ui_patch_ribbon`'unun üzerine yazılmaz).

### C. Dört ikon (16x16, kanaviçe)
- Araç: `create_image_pixen` 32x32; 32'lik hâli `_32` ekiyle teslim edilir; 16x16 hâli en yakın komşuyla indirilir; **en çok 8 renk**.
- Ortak ek: `, cross-stitch embroidery style where every pixel reads as a small thread stitch, simple bold shape, centered, small, no frame, no background, no letters`
  | Dosya | Açıklama |
  |---|---|
  | ui_icon_voice_good_meal | `a steaming clay bowl of stew` (820'deki tencerenin rengi iyiydi; daha sade ve küçük) |
  | ui_icon_voice_nice_chat | `two overlapping speech bubbles, one cream and one wheat gold` |
  | ui_icon_voice_grief | `a single wilted flower with its head bowed down and one fallen petal`, **dik gül değil**, soluk renkler |
  | ui_icon_voice_inspired | `a small lit candle in a clay holder with a bright flame` |

## Doğrulama
- Yamalar 24x24, ≤6 renk, keten ≤3 ton; kurdele genişlik 56-64, yükseklik 14-18, ≤6 renk; ikonlar 16x16 (+ `_32`), ≤8 renk, yarı saydam 0.
- `python tools/art/style.py check --family arayuz <dosya> ...`; `inspired` için `--light`.
- **Palet değişimi testi:** keten tonlarını sekiz hâl rengine (Sakin #A89B86, Huzurlu #7E9B66, Neşeli #D6A542, Şevkli #4E9A8F, Gururlu #8A5A8C, Gergin #B8803A, Kederli #6C8299, Hırçın #A84A3E) eşleyen küçük bir Pillow betiğiyle boyanmış hâlleri üret; her birinin üstüne bir ikon koy.
- Önizleme `art/results/820b-yama-pilotu-x6.png`: üç yama, kurdele, dört ikon ve sekiz renkli boyalı yamalar ikonlarıyla; krem (#F3EAD6) ve koyu (#3A2C24) zeminde, 6 kat.
- Göz kontrolü: yama düz kumaş gibi mi (minder/parşömen değil); üç dikiş birbirinden ayırt ediliyor mu; ikon yamanın üstünde okunuyor mu; yas ikonu solgun ve boynu bükük mü.

## Notlar
- Kullanıcı onay isteyen görselleri sohbette görmek istiyor; önizleme sayfası bu yüzden zorunlu.
- Pilot kabul edilirse sonraki iş: bütün iç ses ikonları bu dille (~310), 60'lık parçalara bölünerek; 791 o dile göre yeniden yazılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
