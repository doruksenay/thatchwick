unity-import: pending

# 513 Easter egg hayvan pozları

İş: `art/queue/513-easter-egg-hayvan-pozlari.md`. Yan yana: [513-yanyana.png](513-yanyana.png) (referans + poz karesi 04). Tüm kareler: [513-kareler.png](513-kareler.png). GIF'ler: `art/results/513-gif/`. Masaüstü kopyası: yerel (bulut modu).

## Araç ve ayarlar
`animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `directions=[south-east, south-west]`; gövde başına 1 çağrı (2 iş), 4 gövde, tek deneme, hız sınırı yok. Pixminimax dört ayaklıda kabul edildi. Çağrı başına 2 generation, toplam **8**. `get_balance` sonrası: 1167,6 (paralel şeritler aynı anda harcıyordu; bu işin payı 8).

Karakter kimlikleri: dog_1 `2ae427e6-4a51-4005-98ae-0e4f7a7cefc0`, lapdog `fc163aab-ff7b-4be0-9bfc-dbd6ca69ad29` (adı "lapdog"; 484 raporundan ayrıca doğrulanamadı), cat_1 `11de8a77-8e44-43c3-b4ab-68f45412dcee` (`cat_1_375`; ikinci bir `cat_1` bef596ef… var, 375'te yeniden üretilen sürüm seçildi), öküz `ad449aaf-286d-457d-b2fe-60c0e8478d55` (48x48).

## Sonradan işlem (ücretsiz)
- Her çıktı 9 kare (0 = referans durma karesi, 1-8 hareket); adlar `_00` … `_08`.
- PixelLab tuvali gövdeden büyük (dog_1 ve lapdog 44, kedi 40, öküz 64). Kareler ölçeksiz, aynı gövdenin `_cand_<yön>` tuvaline (44/32/32/48) taşındı: kare 0'ın alt satırı ve yatay merkezi, durma görselinin alt satırı ve merkezine oturtuldu (animasyon boyunca aynı kaydırma). Opak piksel kaybı yok.
- Kedi `box`: kare 0'ın üst %55'i dışında kalan satırlar her karede silindi (yalnızca baş, boyun, omuz); kesim satırı durma görselinin alt satırına indirildi.
- dog_1 `howl`: doygunluk x1,4-1,5 idi (sw 02-08 KALDI); 18 karenin hepsi `style.py fit` ile x1,15'e indirildi. Ham hâl `art/tmp513/` altında (commit edilmez).

## Dosyalar (`Assets/Art/Iso/_aday/`, her biri 9 kare, `_00`-`_08`)
| Ad | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_animal_dog_1_howl_{se,sw}_NN | 44x44 | 19 | 4 |
| iso_animal_dog_lapdog_howl_{se,sw}_NN | 32x32 | 16 | 2 |
| iso_animal_cat_1_box_{se,sw}_NN | 32x32 | 15 | 2 |
| iso_animal_ox_stare_{se,sw}_NN | 48x48 | 28 | 12 |

## Doğrulama
`validate_iso.py --kind object`: 72/72 geçti. Stil denetimi (hayvan ailesi, kilit kurulmadı):
| Grup | Sonuç | Bulgu |
|---|---|---|
| cat_1_box se/sw | 18 UYARI | ailesinden soluk x0,29-0,31 |
| dog_1_howl se/sw | 18 GEÇTİ (fit sonrası x1,15-1,16) | ham: 16 UYARI + 7 KALDI |
| dog_lapdog_howl se/sw | 18 UYARI | soluk x0,26-0,28 |
| ox_stare se/sw | 18 UYARI | soluk x0,30-0,31 |

Işık sol-üstten, gölge pişirilmemiş, zemin lekesi yok; tüy rengi durma karesiyle aynı.

## Sorunlar
- Kare 0 referans durma karesidir (poz değil); döngü 0'dan başlarsa ilk kare ayakta/oturur görünür. Kod 01-08'i oynatmayı düşünebilir.
- Oturan pozlarda boy durma görselinden kısadır; ayak satırı ve yatay merkez durma görseline göre hizalı, pivot kodda kayıtlı olanla aynı kaldı.
- Kedi `box` kesimi yaklaşık %55 (tarif gereği); sandık üstünde görsel onay gerekir.
- Öküzün öbür tipleri ile kalan köpek ve kedi gövdeleri bu işin dışındadır (514+).
