---
id: "774"
title: "HUD meşe: 745 sayfa 1'den parça kesimi"
status: review
claimed_at: 2026-11-20T21:45Z
approved_by: user        # kullanıcı kararı 2026-11-21: HUD yönü B (oyma meşe), Project belgesi 66 §5 S5
priority: 1
family: arayuz
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["745"]
max_attempts: 1
pixellab: false
outputs:
  - Assets/Art/Iso/_aday/ui_hud2_mese_clock.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_money.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_tracker.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_frame.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_bar.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_bar_fill_{green,amber,red}.png
  - Assets/Art/Iso/_aday/ui_hud2_mese_window.png
created: 2026-11-21
---
# 774 HUD meşe: 745 sayfa 1'den parça kesimi

## Amaç
Kullanıcı HUD malzeme dili olarak **B, oyma meşe**yi seçti (2026-11-21). 745'in sayfa 1'indeki parçalar (`Assets/Art/Iso/_aday/ui_hud2_mese_sheet1.png`, 256x192) birbirinden saydam boşlukla ayrık ve beğenildi. Yeniden üretmek yerine **olduğu boyutta** kesilir. PixelLab çağrılmaz.

## Karar: native boyut kuralı değişti
66 §6'daki küçük native boyutlar (96x40, 24x24 ...) PixelLab araçlarının alt sınırlarıyla tutmadı. Yeni kural: **parça, sayfada çizildiği boyutta native kabul edilir; ölçek düşürme ve büyütme yok.** HUD düzeni (kod tarafı) parça boyutlarına uyar, oyunda tam sayı büyütme (1× ya da 2×, ekran çözünürlüğüne göre) kullanılır.

## Tarif
1. `ui_hud2_mese_sheet1.png`'i aç. Saydam olmayan pikselleri bağlı bileşenlere ayır (alfa > 0, 8-komşuluk; zincir halkaları levhaya bağlı değilse en yakın bileşene kat). Her bileşenin sıkı sınır kutusunu al, 1 px saydam pay bırak.
2. Bileşenleri şu adlarla kaydet (sayfadaki yerine göre):
   | Sayfadaki parça | Dosya | Not |
   |---|---|---|
   | Sol üst: zincirle asılı, ortası yuvarlak koyu boşluklu oyma levha | `clock` | Ortadaki koyu yuva mevsim/saat ikonu alanı. Zincirler dahil |
   | Sol orta: iki koyu yuvalı perçinli panel | `money` | Üst yuva akçe, alt yuva elmas satırı |
   | Sağ orta: kenarı yapraklı ince krem şerit | `tracker` | 9-slice: sol ve sağ yapraklı uçlar köşe, orta krem şerit tekrarlanır. Köşe genişliğini uç süslerin bittiği yerden ölç, raporda yaz |
   | Sağ üst: köşeleri perçinli krem kart | `frame` | 9-slice: köşe = perçin + çıta kalınlığı (ölç, raporda yaz) |
   | Alt: uzun koyu çubuk, uçları perçinli | `bar` | 9-slice, yatay: sol ve sağ perçinli uç köşe |
3. `bar_fill_green/amber/red`: `bar`'ın iç boşluğunun yüksekliğinde (ölç), 1 px genişlikte dikey şerit; renkler `art/style/palette_arayuz.json`'dan: yeşil (>60), hardal (30-60), kırmızı (<30). Her şeritte üst 1 px bir ton açık (ışık sol üstten).
4. `window`: 745 sayfa 2'deki sarmaşıklı büyük çerçeve (`ui_hud2_mese_sheet2.png`, tek parça). Defter penceresinin çerçevesi olarak kullanılacak; 9-slice köşe = köşe perçini + sarmaşık kıvrımının bittiği yer (ölç, raporda yaz). Ortadaki krem alanın kenarındaki yırtık kâğıt efekti korunur.
5. Kesimden sonra hiçbir piksel boyanmaz, yeniden renklendirilmez.

## Doğrulama
- `python tools/art/style.py check --family arayuz <her dosya>`: GEÇMELİ.
- `art/results/774-9slice-test.png`: `tracker`, `frame`, `bar` ve `window` parçaları kodla 3 farklı boya gerilmiş (ör. `tracker` 1×, 2×, 3× genişlik; `frame` 64x40, 160x64, 240x120; `window` 320x200, 480x300), kenarda dikiş ya da kırık desen görünmemeli. Görünüyorsa raporda yaz, düzeltme denemesi yapma.
- `art/results/774-yanyana.png`: bütün parçalar 3× büyütülmüş, adları ve piksel boyutlarıyla.
- Rapor: her parçanın boyutu ve 9-slice kenar değerleri (sol, sağ, üst, alt piksel) tablo olarak. Kod oturumu bu değerleri `UiSkin`/USS'e yazacak.

## Notlar
- Masaüstü kopyası: çıktılar `C:\Users\User\OneDrive\Desktop\art\hud_pilot\` altına da kopyalanır (yerel işçide; bulut işçisi atlar).
- 775 (düğmeler) ve 776 (ikonlar) bu işe bağlı değil, paralel çalışabilir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
