---
id: "612"
title: "Takı katmanı pilotu: küpe ve gerdanlık (karakterin üstünde)"
status: pending
approved_by: user        # kullanıcı 20.11.2026 sohbet: "aç buna iş" (karakterin üstünde takı görünsün)
priority: 30
family: karakter
style_gate: true         # türevi 613 buna bağlı
cost_est: 20
cost_max: 40
depends_on: ["460a"]     # P1 yetişkin mankeni (460a `approved_by: pending`, kullanıcı kararı: 18.12.2026'da üretilecek); o iş review olmadan bu iş başlamaz
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/katman/lay_jw_earring_<yön>_fr.png
  - Assets/Art/Iso/katman/lay_jw_necklace_<yön>_fr.png
  - art/results/612-taki-katmani-pilotu.md
created: 2026-11-20
---
# 612 Takı katmanı pilotu: küpe ve gerdanlık

## Amaç
Project belgesi 62 §2: takılar artık takılıyor ve etki veriyor ama karakterin üstünde görünmüyor. Soru: 84 piksellik karakterde takı okunuyor mu? Pilot iki parçayla yanıtlar: küpe (baş) ve gerdanlık (boyun). Yüzük, hızma ve nazar boncuğu bu ölçekte bir pikselin altında kalır; çizilmez.

## Tarif
- Yetişkin mankeninin (460a) 5 benzersiz yönü (s, se, e, ne, n; batı aynası), yalnızca ön parça (`_fr`).
- **Küpe (`lay_jw_earring`)**: baş parçası sözleşmesi, PartCanvas 64x96, çapa (32,24). Kulak memesinde 2x2 ya da 2x3 piksellik halka; s yönünde iki kulak, se ve e'de tek kulak, ne ve n'de saçın altında kalıyorsa boş kare. Başlık ve saç maskesiyle çakışmasın.
- **Gerdanlık (`lay_jw_necklace`)**: gövde tuvali, omuz çapası (`ClothingMasks.Shoulder`: gövde açıklığı ortası, omuz satırı). Boyun altında 1 piksel kalınlığında yay, ortada 2x2 taş. n yönünde yalnızca ensede kısa çizgi.
- **Anahtar renk:** metal kısım kumaş referans grisi (#9A9A9A, `KeyColors.ReferenceCloth`) ki kod bakır, gümüş ve altın rampasına çevirebilsin. Taş sabit koyu kırmızı (#A8322E), tek renk koyu kontur yok (bu ölçekte kontur takıyı yutar; yalnızca alt kenarda 1 piksel gölge).
- **Önce betik:** bu boyutta PixelLab yerine `tools/art` betiğiyle nokta basmak daha tutarlı olabilir. İşçi önce betikle (0 gen) dener, yan yana sayfasına koyar; okunmuyorsa ya da biçim tutmuyorsa PixelLab ile çizer (`cost_max` 40).

## Doğrulama
- Yan yana sayfası x4: takısız manken, yalnız küpe, yalnız gerdanlık, ikisi birlikte; üç metal rampasıyla (bakır, gümüş, altın). Oyun yakınlığında (x2) da bir satır.
- Ölçüt: x2'de gerdanlık bir bakışta seçiliyor; küpe en az s ve se yönlerinde seçiliyor; yürüme karelerinde parça gövdeden kopmuyor.
- Rapor (`art/results/612-...md`) açık bir sonuç yazar: **okunuyor** (613 çalışır) ya da **okunmuyor** (öneri: takı yalnızca portrede ve karakter kartında gösterilsin, 613 iptal).
- `python tools/art/ramps.py check --ref cloth <dosya>`.

## Notlar
- Kod tarafı **hazır değil**: `LayerComposer`'da takı yuvası yok. Pilot kabul edilirse kod oturumu `jw` yuvasını ekler (küpe baş çapasına, gerdanlık omuz çapasına) ve takılı takıyı `ClothingService.WornJewelry`'den okur.
- Dosya adı kuralı: `lay_jw_<kimlik>_<yön>_fr.png` (baş parçası adlandırmasıyla aynı).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
