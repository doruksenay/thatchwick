---
id: "018d3"
title: Yemek animasyonu boş elle + her karede el konumu (erkek ve kadın, 4 yön)
status: pending
approved_by: user        # kullanıcı 18.11.2026: "bunları yap"
priority: 10
family: karakter
style_gate: false
cost_est: 10
cost_max: 20
depends_on: ["012b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_eat3_{se,sw,nw,ne}_<NN>.png
  - art/results/018d3-hand-anchors.txt
  - art/results/018d3-yemek-bos-el.md
created: 2026-11-19
---
# 018d3 Yemek: boş el + el konumu kaydı

## Amaç
018d2 (kabul) ekmeği resmin içine çiziyor. Kullanıcı: "ne yiyorsa o gözüksün, elmaysa elma armutsa armut". Bunun için:
1. Aynı hareket, ama elde hiçbir şey yok (`eat3`).
2. Her karede yiyeceği tutan elin piksel konumu. Oyun yenen yiyeceğin küçük ikonunu o noktaya çizer (`Assets/Scripts/Game/HandAnchors.cs`).

## Tarif
018d2'nin karakter kimlikleri, yönleri (se, sw, nw, ne), kare sayısı ve işleme kuralları aynen geçerli. Dosya adı `eat3`.
- `action_description="standing upright, head stays still and level, raises one empty hand cupped as if holding a small fruit up to the mouth, takes a bite, lowers the hand back to waist, the head does not lean forward, nothing in the hand"`.

## El konumu dosyası (`art/results/018d3-hand-anchors.txt`)
Her kare için bir satır:
```
iso_char_man_r84_eat3_se 0 47 38
iso_char_man_r84_eat3_se 1 48 44
iso_char_man_r84_eat3_nw 3 30 50 b
```
- Sütunlar sırasıyla önek, kare numarası, x ve y. İsteğe bağlı son sütun `b` olabilir.
- Kare numarası, dosya adındaki `NN` ile aynıdır.
- x ve y, 84×84 tuvalde avuç içinin ortasıdır. x soldan, y alttan sayılır (Unity yönü).
- `b`: el gövdenin arkasında kalıyorsa yazılır (genellikle nw/ne). Oyun ikonu karakterin arkasına çizer.
- Konum otomatik bulunur: bir önceki karede el olmayan bölgeye göre ten rengi uzantının ucu alınır. Bulunamayan kare elle işaretlenir.
- Her yön/cinsiyet için tüm kareler yazılır; eksik kare olamaz.

## Doğrulama
- 018d2'deki baş ölçütü burada da bağlayıcıdır: kafa farkı ≤ 2 px. El en az bir karede ağız hizasına çıkmalıdır.
- Elde ekmek ya da başka nesne olmamalıdır. Elin rengi ten rengi olmalıdır.
- Yan yana sayfası hazırlanır. Her karede el konumu 3×3 kırmızı noktayla işaretlenir. Aynı sayfaya elma ikonunun (`item.apple`) o noktaya %35 ölçekle bindirilmiş hali eklenir. GIF de üretilir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
