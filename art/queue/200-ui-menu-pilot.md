---
id: "200"
title: Arayüz menü stili pilotu (parşömen + ahşap, ana menü paneli)
status: pending
approved_by: pending     # kullanıcı onayı 2026-11-08 (sohbet). 012 review'a geçip kilit kalkınca kod oturumu "user" yapar; o zamana kadar işçi almaz
cost_est: 20
cost_max: 25
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_menu_parsomen_1.png
  - Assets/Art/Iso/_aday/ui_menu_ahsap_1.png
created: 2026-11-08
---
# 200 Arayüz menü stili pilotu

## Amaç
Oyunun menü sayfaları için 6 görsel yön kıyaslanacak; bu iş ilk 2 yönün pilotu. Her yön aynı ekranı gösterir: ana menü paneli. Panelde üstte oyun adı için boş başlık levhası, altında Devam, Yeni Hanedan, Köy Ziyareti ve Ayarlar için 4 boş düğme var. Yazılar sonra ayrı bir font işiyle eklenecek. Kullanıcı pilotu beğenirse 201 (taş + kumaş) ve 202 (koyu sade + ocak) gelir; diğer ekranlar (karakter yaratma, yapı modu, envanter, ayarlar, duraklat) seçilen yön için ayrı işlerde yapılır.

Ölçü kararı (kullanıcı): yalnızca menü paneli, 256x256, saydam arka plan. Panel Unity'de ekranın ortasına konur; yatay ve dikey yönde aynı panel kullanılır. Tam sayı ölçekle büyütülür.

## Tarif

### Araç ve ortak ayarlar (iki aday için aynı)
- `create_ui_asset`. Maliyet (araç açıklaması): 256x256'ya kadar 10 generation. İki çağrı, toplam 20.
- `width=256`, `height=256`, `no_background=true`, `max_colors=48`.
- `style_image_base64` = `Assets/Art/Iso/iso_char_villager_se.png` dosyasının base64'ü (68x68, yaklaşık 1,6 KB). Palet ve dış hat köylü ve zeminlerle aynı dünyaya bağlansın diye.
- `color_palette` = `"earthy muted medieval browns, cream, dark iron grey"`.
- `seed=0`.
- `pieces`: iki adayda **aynı** şablon. Koordinatlar 512x512 sanal tuvalde:
  ```json
  [
    {"id": "frame",   "kind": "rounded_rect", "x": 24,  "y": 16,  "w": 464, "h": 480, "radius": 24, "label": "menu panel"},
    {"id": "title",   "kind": "rounded_rect", "x": 88,  "y": 44,  "w": 336, "h": 96,  "radius": 12, "label": "blank title plaque"},
    {"id": "btn1",    "kind": "rounded_rect", "x": 136, "y": 172, "w": 240, "h": 58,  "radius": 10, "label": "blank button"},
    {"id": "btn2",    "kind": "rounded_rect", "x": 136, "y": 248, "w": 240, "h": 58,  "radius": 10, "label": "blank button"},
    {"id": "btn3",    "kind": "rounded_rect", "x": 136, "y": 324, "w": 240, "h": 58,  "radius": 10, "label": "blank button"},
    {"id": "btn4",    "kind": "rounded_rect", "x": 136, "y": 400, "w": 240, "h": 58,  "radius": 10, "label": "blank button"}
  ]
  ```
- `elements` verme.
- Açıklama metinlerinin hepsinin sonuna şu ek yazılır: `, pixel art game main menu panel, earthy muted medieval palette, flat simple shading, clean dark outline, light from top-left, no dithering, no noise, no text, no letters, no numbers, all plaques and buttons blank`.

### Adaylar
| Aday | Yön | `name` | Açıklama metni (+ ortak ek) | Gen |
|---|---|---|---|---|
| ui_menu_parsomen_1 | 1 parşömen ve el yazması | `thatchwick_menu_parsomen_1` | `aged cream-tan parchment manuscript sheet with rolled scroll ends at top and bottom, dark brown ink border lines, a blank parchment title plaque at the top, four blank parchment button strips below it, small simple ink flourishes in the corners` | 10 |
| ui_menu_ahsap_1 | 2 ahşap pano, demir bantlar | `thatchwick_menu_ahsap_1` | `medieval wooden notice board made of dark oak planks with riveted dark iron bands along the edges and corners, a blank wooden title plaque at the top, four blank plank buttons with small iron studs below it` | 10 |

### Teslim
- `Assets/Art/Iso/_aday/ui_menu_<yön>_1.png`, `cp -n` ile; `.meta` yazma. Mevcut dosyaların üzerine yazma. Kullanıcı sonra `Assets/Art/UI/_aday/` altına taşıyacak; işçi `Assets/Art/UI/` altına yazmaz.
- Çıktı 256x256 değilse ya da arka plan saydam değilse: yeniden örnekleme ya da boyama yok. Ölç, raporla, adayı yine de teslim et ve "⚠" ile işaretle.

### Masaüstü kopyası (kullanıcının açık izni, yalnızca bu iş için)
Hiçbir şeyi silme, üzerine yazma. Klasör tarihi üretim günüdür (`yyyy-mm-dd`).
- `C:\Users\User\OneDrive\Desktop\art\arayuz\<yyyy-mm-dd>_parsomen\` ve `..._ahsap\`: adayın PNG'si (Assets'tekiyle aynı ad) ve `_onizleme_x2.png`. Önizleme: aday en yakın komşu ile 2 kat büyük, `#333333` zemin üzerinde.
- Toplu görsel: `C:\Users\User\OneDrive\Desktop\art\arayuz\<yyyy-mm-dd>_toplu.png`. Pilotun iki yönü yan yana, 2 kat, `#333333` zemin, aralarında 16 px boşluk. Altı yönün tamamı 202'de ayrı tarihli bir toplu dosyada birleşir; bu dosya ona kadar pilot topludur.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna her aday için bir satır: `tarih | arayüz | arayuz/<yyyy-mm-dd>_<yön> | create_ui_asset 256x256 | <gen> | aday | <renk sayısı, not>`.
- Python'da Pillow yok: `uv run --with pillow ...`. Betikleri geçici klasörde tut, repoya ekleme.

## Doğrulama
- Her dosya için tablo: piksel boyutu, renk sayısı (opak), tam saydam / tam opak / **yarı saydam piksel sayısı**. Köşe pikselleri saydam mı? (Arka plan saydam.)
- Ölçütler:
  - 256x256
  - renk ≤ 48
  - arka plan saydam
  - yarı saydam piksel sayısını yaz (0 beklenir; değilse ⚠ ile işaretle, düzeltme yok)
- Görsel kontrol, 4 kat büyütüp:
  - **yazı ya da harfe benzer şekil var mı** (olmamalı)
  - dithering ya da gürültü var mı
  - dış hat net ve koyu mu
  - başlık levhası ve 4 düğme şablondaki yerlerde ve boş mu
  - ışık sol-üstten mi
  - bilinen bir oyunun arayüzüne ya da logosuna benzerlik var mı
- `tools/art/validate_iso.py` bu iş için gerekmez (izometrik değil).

## Notlar
- Yön adları ASCII: `parsomen`, `ahsap`. Sonraki işlerde `tas`, `kumas`, `koyu_sade`, `ocak`.
- Pivot ve import ayarı UI klasöründe kullanıcının editör betiğinde; işçi `.meta` yazmaz.
- Telif: bilinen oyunların arayüzü, logosu ya da çerçeve motifi kopyalanmaz; açıklamada oyun adı geçmez.
- Bu iş 012'den (karakter oturumu) sonra çalışır. Sıra kod oturumu tarafından `approved_by` ile tutulur.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
