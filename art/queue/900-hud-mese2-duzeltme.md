---
id: "900"
title: "HUD meşe düzeltmeleri: para, görev şeridi, kart, Yapı düğmesi, rozet, 12 px ihtiyaç ikonları"
status: review
claimed_at: 2026-11-20T23:54Z
approved_by: user        # kullanıcı kararı 2026-11-21: "düzelt" (HUD pilotu sonuç sayfası, 420_hud_pilot.png)
priority: 1
family: arayuz
style_gate: false
cost_est: 14
cost_max: 25
depends_on: ["810", "811", "812"]
max_attempts: 2
pixellab: true
yon: mese2
outputs:
  - Assets/Art/Iso/_aday/ui_hud2_mese2_money.png
  - Assets/Art/Iso/_aday/ui_hud2_mese2_tracker_v2.png
  - Assets/Art/Iso/_aday/ui_hud2_mese2_card_v2.png
  - Assets/Art/Iso/_aday/ui_hud2_mese2_{btn_build_v2,btn_build_down_v2,btn_book_down_v2,badge_v2}.png
  - Assets/Art/Iso/_aday/ui_hud2_mese2_{need_hunger_v2,need_energy_v2,need_warmth_v2,need_fun_v2,need_hygiene_v2,need_social_v2,need_toilet_v2}.png
created: 2026-11-21
---
# 900 HUD meşe düzeltmeleri

## Neden
810-812 sahnede birlikte görüldü (`art/results/420_hud_pilot.png`; kod oturumu `ui_mockup.py hud hud_v2 --yon mese2`). Kullanıcı: "düzelt". Sorunlar:
1. `money` 810'da iki denemede de tutmadı (yuvalar üst üste; ikon yerleri saydam delik).
2. `tracker` uzatılınca ortada dikey kırılma görünüyor: orta bölge düz tekrarlanmıyor.
3. `card`: krem iç alan ahşap çerçevenin üst kenarını örtüyor, çerçeve iç alana göre kaymış (kullanıcı: "kaymış").
4. `btn_build`: çekicin yanında mala yerine yeşil yaprak var. Basılı hâller (`btn_build_down`, `btn_book_down`) normalden ayırt edilmiyor.
5. `badge`: ünlem silik.
6. Kart 7 ihtiyaç satırını taşıyacak kadar uzun değil; 16 px ikonlar üst üste biniyor. Karar: ihtiyaç ikonları **12x12** olur (kart boyu aynı kalır). Bu turda `need_hygiene` ve `need_social` da okunur hâle gelir.

Var olan dosyaların üzerine yazılmaz; düzeltmeler `_v2` adıyla teslim edilir (`ui_mockup.py` en yeni `_vN` sürümünü kullanır). `money` 810'da teslim edilmediği için yeni addır.

## Ortak kurallar (810'daki gibi)
- Arayüz paleti `art/style/palette_arayuz.json` (`color_image` olarak 128 renk), `seed=0`, renkli koyu dış kontur (saf siyah yok), ışık sol üstten, yazı/harf/rakam yok. Ölçek düşürülmez; kılavuz yalnızca `init_image`.
- **Arka plan silme dersi (810):** `no_background=true` iç krem alanı ve ikon yuvalarını saydam yaptı. Bu iş, iç alanı olan parçalarda `no_background=false` + düz zemin kullanır, sonra **yalnızca kenara bağlı tek renkli zemin** maskeyle silinir (810 `card` ikinci denemesindeki yöntem; içerik boyanmaz).
- `create_image_pixflux` en küçük kenar 32: küçük parçada tuval 32, kılavuz native kutuya ortalı, yalnızca native kutu kırpılır. `create_image_pixen` 16 tuval; 12 ve 10 px parçalar ortadan kırpılır.
- Meşe metin eki: `carved dark oak UI piece, leaf and vine carving on the rim (not geometric), muted moss green leaves, small wrought iron rivets at the corners, inner cream card for blank text, dark warm brown outline, pixel art game HUD, no text, no letters, no numbers`
- Denetim: her parça `python tools/art/style.py check --family arayuz` GEÇMELİ; geçmeyen ya da ölçüte uymayan parça için **en fazla bir** yeniden deneme. Yine olmazsa teslim edilmez, raporda yazılır.
- Bütçe `cost_max: 25`. Aynı anda en çok 3 PixelLab çağrısı.

## Parçalar
| Dosya | Native (tuval) | Araç | Kılavuz | Ölçüt |
|---|---|---|---|---|
| `money.png` | 72x20 (72x32) | pixflux | `ui_hud2_mese2_tracker` ve `ui_hud2_mese_money` (774) | **Tek satır**, iki koyu ahşap yuva **yan yana** (x 3-35 ve x 37-69). Her yuvanın solunda 10x10 **opak** koyu ikon yeri, sağında boş sayı alanı. Kılavuzda ikon yerleri orta ahşap tonunda opak boyanır. Saydam delik yok |
| `tracker_v2.png` | 48x18 (48x32), 9-slice köşe 6 | pixflux | `ui_hud2_mese2_tracker` | Uçlarda yaprak oyma yalnızca sol ve sağ 6 px içinde. **x 6-41 arası her sütun aynı** (üst kenar, krem şerit, alt kenar düz). Teslimden önce Python denetimi: orta bölgenin sütunları birbirinin aynısı değilse yeniden dene. 9-slice testini 48, 120, 200 genişlikte yeniden çiz |
| `card_v2.png` | 132x112, sabit | pixflux, `no_background=false` | `ui_hud2_mese2_card` | Ahşap çerçeve **dört kenarda da tam görünür**, en az 8 px kalın; krem iç alan çerçevenin içinde kalır ve **x 10-122, y 10-102 düz krem** (810'da x 13-117, y 13-98 idi). Sarmaşık ve yapraklar çerçevenin üstünde, iç alana taşmaz. Teslimden önce Python: iç dikdörtgenin %95'i aynı krem tonu olmalı |
| `btn_build_v2.png` | 32x32 | pixflux | `ui_hud2_mese2_btn_build` | Krem daire üstünde **çapraz demir çekiç ve üçgen uçlu duvarcı malası** (mala: düz metal üçgen ağız, ahşap sap). Yeşil ya da yaprak yok |
| `btn_build_down_v2.png`, `btn_book_down_v2.png` | 32x32 | Python (0 gen) | `btn_build_v2`, `ui_hud2_mese2_btn_book` | Basılı hâl: kalkanın içi (krem daire ve ikon) paletten **iki** basamak koyu, ikon 1 px aşağı, dış alt gölge satırı silinir. 1x'te normalden açıkça ayrılmalı |
| `badge_v2.png` | 10x10 (pixen 16) | pixen | `ui_hud2_mese2_badge` | Bordo daire, **kalın krem ünlem** (2 px genişlik, gövde + nokta), ince demir halka. 1x'te ünlem okunmalı |
| `need_*_v2.png` (7) | 12x12 (pixen 16) | pixen, her biri ayrı çağrı | ilgili `ui_hud2_mese2_need_*` | Açlık: somun ekmek. Enerji: hilal. Isınma: alev. Eğlence: topaç. **Temizlik: büyük mavi su damlası** (sabun yok, tek şekil). **Sosyal: iki baş ve omuz silüeti yan yana, ten rengi yüz, kahve saç** (gri leke değil). Tuvalet: kapaklı ahşap oturak. Hepsi 12x12 içinde, 1x'te tanınır |

## Teslim
- `art/results/900-yanyana.png`: her parçanın eski hâli (varsa) ve yenisi yan yana, 1x ve 3x.
- `art/results/900-9slice-test.png` (`tracker_v2`), `art/results/900-ikonlar-x3.png` (7 ihtiyaç ikonu 1x, 2x, 3x).
- Kompozisyon bu işte yapılmaz; kod oturumu `ui_mockup.py` ile `420_hud_pilot.png`'yi yeniden çıkarır.

## Sonuç

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
