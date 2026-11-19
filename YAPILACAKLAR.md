# Yapılacaklar

Son güncelleme: 2026-11-19

## Kod
- [ ] 018d4 (boş elle yeme, kalan 4 yön) gelince `art/results/018d4-hand-anchors.txt` satırlarını `Assets/Resources/Content/hand_anchors.txt` sonuna ekle.
- [ ] Madencilik işlerini ayır, böylece her biri kendi animasyonunu oynatsın: kazma (`mine_dig`), kova taşıma (`mine_bucket`), destek kurma (`mine_support`). Şu an hepsi kazma oynatıyor.
- [ ] 018c (tarla, odun) kabul edilince animasyonlarını `GameRoot.ActionKeys`'e bağla.
- [ ] 018b4 kabul edilince yürümede eski `walk` setine düşen yön kalmadığını kontrol et.
- [ ] Unity'de dene:
  - [ ] Elde yiyecek: elma, yumurta, sepetten armut ve erik.
  - [ ] Zıplamasız yürüme (walk2).
  - [ ] Sohbet, bakım, sevgi, keyif ve balık animasyonları.
  - [ ] Kazma animasyonu.
  - [ ] Yıkanma ve tuvalette keyif animasyonunun oynamadığını kontrol et.
- [ ] (Öneri, karar bekliyor) Elde tutulan aletleri de el konumu yöntemiyle göster (balta, çapa, kazma…).

## Sanat (bulut işçisi üretir, sonra kullanıcı karar verir)
- [ ] 018c: tarla ve odun animasyonları (üretimde)
- [ ] 018b4: yürüme, son 5 yön (erkek sw; kadın n, ne, se, sw)
- [ ] 018d4: boş elle yeme, kalan 4 yön + el konumu (erkek sw; kadın se, ne, sw)
- [ ] 395e: kömür yığını (395d aday 2'nin renk düzeltmesi)
- [ ] Hâlâ eksik kalan yönler olursa yeni tur iş dosyası yaz.

## Biten (19.11.2026)
- [x] Yemekte elde yenen yiyecek: kod (#60), el konumları oyunda (#73)
- [x] Kabul edildi:
  - [x] 018b2 (4 yön), 018b3 (7 yön)
  - [x] 018d3 (4 yön)
  - [x] 018e
  - [x] 368, 368b
  - [x] 395d (çatkı, kandil)
- [x] Oyuna bağlandı:
  - [x] walk2 yürüme
  - [x] 018e sosyal ve bakım animasyonları
  - [x] 368 kazma
