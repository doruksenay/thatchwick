---
id: "770"
title: "Geçiş seti: çim ↔ çayır (koyu çim öbekleri)"
status: failed
claimed_at: 2026-11-21T00:05Z
approved_by: user        # otomatik akış; kullanıcı 21.11.2026 (Project belgesi 66, 66b)
priority: 10
family: zemin
style_gate: false
cost_est: 6
cost_max: 10
depends_on: ["613g"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_trans_grass_meadow_{01,03,05,07}_{a,b}.png
created: 2026-11-21
---
# 770 Geçiş seti: çim ↔ çayır

## Amaç
Tester raporu 66 §1.5: çayır karoları (`iso_ground_meadow_a/b/c`) çimde sert kenarlı, basamaklı elmas lekeler gibi duruyordu.
Kod artık çayırı **yalnızca bu geçiş seti depodayken** çizer (`IsoArt.IsMeadow`, ilk görsel `iso_trans_grass_meadow_01_a`) ve
köşelere dual-grid geçişi koyar (`WorldView.TransitionPairs` içinde `("grass", "meadow")`). Set gelene kadar çimde çayır yok.

## Tarif
613g ile aynı yöntem ve ayarlar (`create_tiles_pro`, isometric 64 px, 004e/613g paleti), birinci zemin `iso_ground_grass_a`,
ikinci zemin `iso_ground_meadow_a`. Maskeler 01, 03, 05, 07; iki değişke (a, b). Kenar düzensiz ve yumuşak: çayırın koyu
tutamları çime dağılarak geçer, düz çizgi ya da simetri yok.

## Doğrulama
`validate_iso.py --kind ground`; 8x8 karoluk gürültü lekesi önizlemesi (kenarda basamak görünmemeli).

## Sonuç (işçi, 2026-11-21)
`create_tiles_pro` (tileset, isometric 64, açı 30, depth 0, segmentation, amplitude 0.3, boundary_seed 51, seed 7701; palet: grass_a 4 renk + meadow_a'dan #6e8d49 #658242 #407437 #628d4c) tek çağrı, tile id `b39a7c03-34ab-42de-9a30-95cbaa8ba5ae`. Çağrı ~40 dk boyunca "processing 5%" durumunda kaldı (ETA ~420 sn'de sabit; aynı hesapta başka işler tamamlandı). Tek deneme kuralı gereği yeniden denenmedi, doğrulama/teslim yapılmadı. Bakiye üretimden önce 9731; ücret kesildiyse 10 gen. Tile tamamlanırsa `get_tiles_pro` ile alınıp işlenebilir; aksi halde işi `pending`'e çekip yeniden kuyruğa almak kullanıcı kararıdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
