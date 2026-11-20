---
id: "797"
title: "Servis kapları, katmanlı: boş kap ve dört doluluk katmanı (10 birleşim)"
status: running
claimed_at: 2026-11-20T23:09Z
approved_by: user        # kullanıcı 21.11.2026: 578-580 "4'ü kabul + düz kaplar katmanlı" (seçmeli liste)
priority: 5          # kullanıcı 21.11 02:02: "işçiye ürettir" (mutfak işleri önce)
family: esya
style_gate: false
cost_est: 44
cost_max: 60
depends_on: ["578", "579", "580"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_serve_vessel_board.png
  - Assets/Art/Iso/_aday/iso_serve_vessel_platter.png
  - Assets/Art/Iso/_aday/iso_serve_vessel_bowl_wood.png
  - Assets/Art/Iso/_aday/iso_serve_food_<yemek>_{4,3,2,1}.png
  - art/results/797-*.md
created: 2026-11-21
---
# 797 Servis kapları, katmanlı (10 birleşim)

## Amaç
578-580'de "kabı aynı bırak, yalnızca içini değiştir" yöntemi düz tahta, tepsi ve tabaklarda tutmadı: model boş kap yerine dolu nesne çizdi, kap silueti `_4` ile eşleşmedi (`art/results/578-*.md`, `579-*.md`, `580-*.md`). Kullanıcı kararı: bu birleşimlerde **kap tek görsel, yemek ayrı katman**. Oyun boş kabı çizer, üstüne doluluğa göre yemek katmanını koyar; kabı beş karede eşitleme sorunu ortadan kalkar.

Kabul edilen dört birleşim (güveç çömleği, kazan çorba, dolma tepsisi, komposto kasesi) beş karelik hâliyle kalır.

## Tarif
**1. Boş kaplar.** Var olanlar yeniden üretilmez:

| Kap | Kaynak |
|---|---|
| sırlı kil çömlek | kabul edilmiş `iso_serve_pot_stew_0.png` (boş) |
| bakır tepsi | kabul edilmiş `iso_serve_tray_dolma_0.png` (boş) |
| `iso_serve_vessel_board.png` | yeni: `rectangular wooden serving board, empty, lying flat` |
| `iso_serve_vessel_platter.png` | yeni: `long oval wooden serving platter, empty, lying flat` |
| `iso_serve_vessel_bowl_wood.png` | yeni: `large round wooden serving bowl, empty, shallow, seen from above at an angle` |

Yeni kaplar 578'deki gibi masa üstü eşyası (meyve sepeti `iso_item_fruit_basket` ölçüsü, 27 §7.7 "K boy"), `create_map_object` stil eşleme, aile `esya`, ortak ek `, medieval tableware, seen from the isometric game angle, no hands, no people, no text`.

**2. Yemek katmanları.** Her birleşim için `_4` dolu, `_3` dörtte üç, `_2` yarım, `_1` çeyrek; `_0` yok (boş kap). Katman kapla **aynı tuval boyutunda**, şeffaf zemin, yalnızca yemek pikselleri; kabın üstüne aynı (0,0) konumunda bindirilince kabın içine ya da yüzeyine oturur.

Yöntem: kabın görseli arka plan, kabın iç ya da üst yüzeyi maske; açıklama yalnızca yemeği ister (`… only the food, …`). Çıktıdan kap pikselleri (arka planla aynı olan pikseller, eşik ±8) çıkarılarak katman elde edilir. Kap pikselleri boyanmış çıkarsa o kare katmana alınmaz, rapora yazılır. Önce `_4`; `_3…_1` `_4` katmanı arka planla birlikte verilip azaltılarak.

| Katman | Kap | Yemek (dolu hâl) |
|---|---|---|
| `iso_serve_food_pilaf_<n>.png` | bakır tepsi | `golden bulgur pilaf heaped in a dome` |
| `iso_serve_food_soup_red_<n>.png` | kil çömlek | `red-orange soup surface` |
| `iso_serve_food_roast_<n>.png` | bakır tepsi | `a roast joint of meat sliced on a bed of onions` |
| `iso_serve_food_fish_<n>.png` | oval tabak | `whole grilled fish laid side by side` |
| `iso_serve_food_pie_<n>.png` | tahta | `a large golden layered pie cut in squares` |
| `iso_serve_food_bread_<n>.png` | tahta | `round flatbreads stacked and a sliced dark loaf` |
| `iso_serve_food_sweet_<n>.png` | tahta | `small honey-glazed sweet pieces arranged in rows` |
| `iso_serve_food_salad_<n>.png` | tahta kase | `chopped green leaves and radish` |
| `iso_serve_food_breakfast_<n>.png` | oval tabak | `cheese wedges, olives, a honeycomb piece and bread slices` |
| `iso_serve_food_fry_<n>.png` | oval tabak | `small golden fried pieces piled up` |

Bütçe: 3 kap + 10 × 4 katman = 43 çağrı. Önce tek birleşim (pilav) uçtan uca denenir; katman çıkarma tutmazsa iş durur, `## Sorular` eklenir ve kalan çağrılar yapılmaz.

## Doğrulama
- `python tools/art/validate_iso.py`, `python tools/art/style.py check --family esya` (kap ve kap+katman birleşimi üzerinde).
- Her birleşim için şerit: kap + `_4…_1` + boş kap (`art/results/797-serit-<yemek>.png`), masa üstünde deneme görüntüsü.
- Katmanda kap pikseli kalmamalı (kapla birleşik hâl ile kap arasında katman dışı fark 0).

## Notlar
Kod tarafı (kod oturumu, kabulden sonra): servis kabı çizimi "kap + doluluk katmanı" desteği kazanır; beş kareli dört birleşim aynen çalışır. Yemek → birleşim eşlemesi 578-580 ile aynı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
