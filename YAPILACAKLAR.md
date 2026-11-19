# Yapılacaklar

Son güncelleme: 2026-11-18

## 1. Yemekte elde tutulan yiyecek (dal: `feat/yemek-elde`, commit edilmedi)

Amaç: Yeme animasyonunda karakterin elinde ne yiyorsa o görünsün (elma → elma, armut → armut).

Kod tarafı yazıldı:
- `Core/Life/HeldFood.cs`
- `DecorSystem.NextToEat`
- `Game/HandAnchors.cs`
- `IsoArt.HeldItem`
- `GameRoot` içindeki Held sprite'ı
- `UnityStubs` düzeltmeleri

Kalanlar:
- [x] `HeldFoodTests` meyve sepeti testini düzelt. Sepet yere konamıyor, bir masanın üstüne konmalı (`PlaceOnSurface`). Düzeltme yarıda kesildi; dosyanın son halini kontrol et.
- [x] Çekirdek testlerini çalıştır. Hepsinin geçtiğini doğrula (önceki çalıştırma: 871 geçti, 1 kaldı).
- [x] GameCheck derlemesini tekrar doğrula.
- [x] Commit at, dalı push et, PR aç.
- [x] Sanat işi `art/queue/018d3` yaz. İçeriği:
  - `eat3`: boş elle yeme. Erkek ve kadın, 4 yön (se, sw, nw, ne). Hareket 018d2 ile aynı.
  - Her kare için el konumu (`prefix kare x y [b]`): x soldan, y alttan; "b" el gövdenin arkasındaysa. Çıktı `art/results` altına.
- [ ] İş dosyasını `main`'e al ve `pixellab-artist` işçisini başlat.
- [ ] 018d3 teslim edilince el konumlarını `Assets/Resources/Content/hand_anchors.txt` dosyasına kopyala. İstenirse bunun için bir `tools/art` betiği yaz.
- [ ] Unity'de dene: elma, yumurta, sepetten armut ve erik.

## 2. İzlenecek sanat işleri

Her biri teslim edilince kullanıcıya gösterilecek, kabul ya da ret kullanıcıda.

- [ ] 018b2: yürüme seti (d yöntemi ve serbest ayak hizası)
- [ ] 018c: tarla ve odun animasyonları
- [ ] 018e: sosyal ve bakım animasyonları
- [ ] 368: madencilik animasyonu
- [ ] 395c: 4. tur (çatkı, kandil, kömür yığını; kireç ocağı referansıyla)

## 3. Açık öneriler (kullanıcı kararı bekliyor)

- [ ] Aynı el konumu yöntemiyle elde tutulan aletleri de göster (balta, çapa, kazma…).

## 4. Test ortamı notu

- [ ] Yerel test düzeneği (`/home/claude/harness`) yalnızca `items.json` ve `life.json` kopyalıyor. "bilinmeyen beceri 'Athletics'" hataları eski derleme çıktısından geliyordu; temiz derlemede sorun çıkmadı.
