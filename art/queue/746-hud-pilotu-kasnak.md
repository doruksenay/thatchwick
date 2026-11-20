---
id: "746"
title: "HUD pilotu, yön C: Kasnak ve kumaş yama"
status: rejected
claimed_at: 2026-11-20T20:35Z
approved_by: user        # kullanıcı kararı 2026-11-20 (Project belgesi 66 §5 S3-S4: önce PixelLab HUD pilotu, üç yön birden)
priority: 1
family: arayuz
style_gate: false
cost_est: 20
cost_max: 30
depends_on: []
max_attempts: 1
pixellab: true
yon: kasnak
outputs:
  - Assets/Art/Iso/_aday/ui_hud2_kasnak_sheet1.png
  - Assets/Art/Iso/_aday/ui_hud2_kasnak_sheet2.png
  - Assets/Art/Iso/_aday/ui_hud2_kasnak_sheet3.png
  - Assets/Art/Iso/_aday/ui_hud2_kasnak_{clock_hoop,clock_band,money,tracker,btn_build,btn_build_down,btn_book,btn_book_down,badge,frame,bar,bar_fill_green,bar_fill_amber,bar_fill_red}.png
  - Assets/Art/Iso/_aday/ui_hud2_kasnak_{need_hunger,need_energy,need_warmth,need_fun,need_hygiene,need_social,need_toilet}.png
  - Assets/Art/Iso/_aday/ui_hud2_kasnak_{speed_pause,speed_1,speed_2,speed_3,season_spring,season_summer,season_autumn,season_winter}.png
created: 2026-11-20
---
# 746 HUD pilotu, yön C: Kasnak ve kumaş yama

## Amaç
Project belgesi 66: HUD'un amatör görünmesinin kökü sistem yokluğu (üç ayrı görsel dil). Bu iş, yeni HUD'un (66 §3) bütün parçalarını **tek bir malzeme diliyle** üretir. 744, 745 ve 746 aynı parça listesini üç farklı malzemeyle üretir; kullanıcı yan yana bakıp birini seçer. Üç iş bağımsızdır, paralel çalışır.

## Yön: Kasnak ve kumaş yama
Parçalar ketene dikilmiş kumaş yamalar; kenarda görünür teyel ilmekleri. P1 nakış kasnağı (ahşap halka + üstte küçük vida), çemberin içinde 4 mevsim simgesi işlenmiş, ortada saat için boş alan; altında gün/yıl için kumaş şerit (`clock_hoop` + `clock_band`). Düğmeler yuvarlak işlemeli rozet. P8 dolgusu ilmek ilmek dolan iplik dokusu. Logoyla aynı aile (Project belgesi 17). **Okunurluk:** ilmek dokusu 2 px'ten ince olmaz; ikon silüetleri dokusuz düz renk.

**Açıklama metni eki (her çağrıya):** `fabric patch UI piece sewn onto linen, visible running-stitch border (stitches at least 2 px), embroidery hoop motif, soft cloth texture only on the border, flat blank text area, pixel art game HUD, transparent background, no text, no letters, no numbers`

## Ortak kurallar (744, 745, 746 aynı; Project belgesi 66 §6)
- **Palet:** donmuş arayüz paleti `art/style/palette_arayuz.json` (`create_ui_asset` / `create_image_pixflux` için `color_palette` olarak bu dosyanın renkleri). Kontur: renkli koyu dış kontur (en koyu tonun bir basamak koyusu), **saf siyah yok**; ışık sol üstten.
- **Stil görseli verilmez** (bulut işçisi base64 aktaramıyor, 211 notu); yön yalnızca açıklama metniyle tarif edilir. Üç yön aynı ayarlarla üretilir ki karşılaştırma adil olsun: `max_colors=32`, `seed=0`, saydam arka plan.
- **Yazı üretilmez** (Pixelify Sans kodla basılır). Yazı alanları boş, düz, krem ya da açık zemin bırakılır. Harf, rakam, sembol yok (ikon parçaları hariç).
- **Native boyut:** her parça aşağıdaki boyutta teslim edilir; oyunda 2× (telefonda 3×) tam sayı büyütmeyle gösterilir. Daha büyük üretilen çıktı **en yakın komşuyla değil**, sayfa üretimiyle native boyutta alınır (aşağıdaki sayfa yöntemi); ölçek düşürme yapılmaz.
- **9-slice parçalar** (P3, P6, P8 kılıfı): köşe 6 px (P8'de 3 px); kenar ve orta bölge düz tekrarlanabilir (desen ya da gradyan yok, kenarda dikiş görünmesin). Teslimle birlikte `art/results/<id>-9slice-test.png`: parçayı 3 farklı boya (ör. 48x18, 120x18, 200x40) kodla gerip yan yana çiz.
- **Sayfa yöntemi:** parçalar 3 sayfada üretilir (maliyet ve tutarlılık için), sonra `art/tmp/` altında Python ile native boyutlu PNG'lere kesilir. Sayfalar da teslim edilir (`..._sheet1/2/3.png`).
  - Sayfa 1 (`create_ui_asset`, 256x128): P1, P2, P3 kaynağı, P6 kaynağı, P8 kılıfı; parçalar arası en az 8 px saydam boşluk.
  - Sayfa 2 (`create_ui_asset`, 128x64): P4 Yapı normal + basılı, P4 Defter normal + basılı, P5.
  - Sayfa 3 (`create_image_pixflux`, 128x64, ızgara): P7 ×7 (16×16), P9 ×4 ve P10 ×4 (12×12). İkon silüetleri **dokusuz, düz renk**, 1 px renkli kontur; zemin saydam. Izgara kılavuzu `art/tmp/` altında çizilip `init_image` (güç ≤ 120) olarak verilebilir.
  - P8 dolgusu (3 renk) sayfa 1'de ya da kodla: dolgu 1×6 px dikey şerit yeterli; renkler yeşil `>60`, hardal `30-60`, kırmızı `<30` (paletten).
- **Denetim:** her parça `python tools/art/style.py check --family arayuz <dosya>...` GEÇMELİ. Geçmeyen parça için **en fazla bir** yeniden deneme (yalnızca o sayfa ya da parça); yine geçmezse teslim edilmez, raporda yazılır. `validate_iso` bu iş için kullanılmaz (izometrik değil).
- **Okunurluk:** P7, P9, P10 ikonları 2× ve 3× büyütülmüş hâlde `art/results/<id>-ikonlar-x3.png` sayfasında; her ikon 16×16 (ya da 12×12) içinde tek bakışta tanınmalı.
- **Bütçe:** iş tavanı `cost_max: 30`; üç iş toplam ≤ 90 (kullanıcı sınırı). Aşılacaksa dur, `## Sorular`'a yaz.

## Parça listesi
Ad biçimi: `Assets/Art/Iso/_aday/ui_hud2_<yon>_<parça>.png`

| # | Dosya eki | Native | Not |
|---|---|---|---|
| P1 | `clock` | 96×40 (kasnak yönünde: `clock_hoop` 56×56 + `clock_band` 64×20) | Mevsim ikonu yuvası (12×12) sol tarafta; gün/saat için boş yazı alanı |
| P2 | `money` | 72×20 | İki ikon yuvası (akçe, elmas; her biri 10×10) ve yanlarında iki boş sayı alanı |
| P3 | `tracker` | 48×18, 9-slice | Solda 12×12 parşömen ya da kitap ikonu yuvası, sağında boş yazı şeridi |
| P4 | `btn_build`, `btn_build_down`, `btn_book`, `btn_book_down` | 32×32 | Yapı: çekiç + mala; Defter: deri kaplı kitap. Basılı hâl 1 px aşağı, gölge az, iç biraz koyu |
| P5 | `badge` | 10×10 | Bordo zemin, krem ünlem |
| P6 | `frame` | 24×24, 9-slice | Widget'ların ortak ince kenarı, ortası boş krem |
| P7 | `need_hunger`, `need_energy`, `need_warmth`, `need_fun`, `need_hygiene`, `need_social`, `need_toilet` | 16×16 | Açlık (ekmek/kâse), Enerji (yastık ya da hilal), Isınma (alev), Eğlence (topaç), Temizlik (su damlası + sabun), Sosyal (iki baş), Tuvalet (oturak ya da kapı) |
| P8 | `bar`, `bar_fill_green`, `bar_fill_amber`, `bar_fill_red` | kılıf 40×8 (9-slice), dolgu 1×6 | |
| P9 | `speed_pause`, `speed_1`, `speed_2`, `speed_3` | 12×12 | Duraklat (iki çizgi), 1× (tek üçgen), 2×, 3× (iki ve üç üçgen) |
| P10 | `season_spring`, `season_summer`, `season_autumn`, `season_winter` | 12×12 | Filiz, güneş, yaprak, kar tanesi |

## Sonuç
- Bütün parçalar native boyutta; sayfalar; `art/results/<id>-yanyana.png` (bütün parçalar 3× büyütülmüş, adlarıyla); 9-slice testi; ikon sayfası.
- Kompozisyon (ev içi sahne üstüne `hud_v2` düzeni) bu işte yapılmaz: üç iş bitince kod oturumu `ui_mockup.py` ile birlikte çıkarır (`art/results/420_hud_pilot.png`).

## Sorular
Üç sayfa teslim edildi ama parçalar native boyutta kesilemedi (ayrıntı: `art/results/746-hud-pilotu-kasnak.md`). Parça başı üretim (ui_asset elements / pixen ikonlar) için devam işi açılsın mı?

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

Kullanıcı kararı 2026-11-21: **rejected.** HUD için yön B (oyma meşe) seçildi. Sayfa 2 düğmeleri silinmez; başka yerde kullanılabilir.
