unity-import: pending

# 581 Sofra takımı, baharat rafı, salça tepsisi, piknik, pazar tezgâhı (rapor): 11/11 teslim, review

Yan yana sayfası: [581-yanyana.png](581-yanyana.png). Reddedilen ilk denemeler ve ham hâller: `art/results/581-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, referanssız kılavuz + maske (küçük kılavuzlar `art/tmp581/mk.py` ile; tepsi, örtü ve tezgâh `footprint_guide.py` ile), `high top-down` (baharat rafı `side`), `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 11 çağrı + 3 ikinci deneme (sepet, kirli tabaklar, baharat rafı: ilk deneme tanınmadı, aynı kılavuz ve maske) = 14 generation (cost_max 22). `get_balance` sonrası: 1873,7 (paralel şeritler de harcıyor).
- Ayak izleri: masa üstü eşyalar 32x32 tuval, 16x8 taban elması; kirli tabaklar 32x36; sepet 40x40 (24x12 taban); salça tepsisi 1x1 80x56; piknik örtüsü 2x2 düz 144x80; tezgâh 2x1 112x136; baharat rafı duvar paralelkenarı 48x56.
- Doygunluk `fit`: tahta tabak dolu (x0.69) ve salça tepsisi (x0.70); hamları `581-red/` içinde. Baharat rafı ilk deneme `fit` ile de KALDI, ikinci deneme fit'siz GEÇTİ.

## Dosyalar (Assets/Art/Iso/_aday/), pivot ALTTAN
| Yol | Piksel | Pivot x | Pivot y |
|---|---|---|---|
| iso_item_plate_wood_full.png | 32x32 | 16 | 5 |
| iso_item_plate_wood_empty.png | 32x32 | 16 | 5 |
| iso_item_bowl_clay_full.png | 32x32 | 16 | 5 |
| iso_item_bowl_clay_empty.png | 32x32 | 16 | 5 |
| iso_item_cup_wood.png | 32x32 | 16 | 5 |
| iso_item_dirty_dishes.png | 32x36 | 16 | 5 |
| iso_item_spice_rack_se.png | 48x56 | 24 | 28 (duvar eşyası, merkez) |
| iso_item_paste_tray_se.png | 80x56 | 40 | 24 |
| iso_item_picnic_basket_se.png | 40x40 | 20 | 5 |
| iso_item_picnic_cloth_se.png | 144x80 | 72 | 40 |
| iso_item_food_stall_se.png | 112x136 | 56 | 32 |

## Doğrulama (validate_iso, --max-colors 48) ve stil denetimi (esya)
| Dosya | Renk | Yarı saydam | validate | Palete yakın | Doygunluk | Stil |
|---|---|---|---|---|---|---|
| tabak dolu (fit) | 8 | 0 | geçti | %100 | x1.17 | GEÇTİ |
| tabak boş | 7 | 0 | geçti | %100 | x0.78 | GEÇTİ |
| kase dolu | 7 | 0 | geçti | %100 | x0.85 | GEÇTİ |
| kase boş | 7 | 0 | geçti | %100 | x0.97 | GEÇTİ |
| bardak | 7 | 0 | geçti | %100 | x0.58 | UYARI (soluk) |
| kirli tabaklar (2. deneme) | 20 | 0 | geçti | %100 | x1.34 | UYARI (doygun) |
| baharat rafı (2. deneme) | 41 | 0 | geçti | %90 | x0.98 | GEÇTİ |
| salça tepsisi (fit) | 50 | 0 | renk 50 > 48 (yalnızca raporlandı, 301 kuralı) | %94 | x1.25 | GEÇTİ |
| sepet (2. deneme) | 16 | 0 | geçti | %93 | x1.10 | GEÇTİ |
| piknik örtüsü | 31 | 0 | geçti | %93 | x1.30 | UYARI (palete uzak, doygun) |
| tezgâh | 33 | 0 | geçti | %100 | x0.79 | GEÇTİ |

Hepsinde alta değme ve boşluk uygun. Döşeme testi eşya olduğu için uygulanmadı. Gölge pişirilmemiş, ışık sol-üst (gözle).

## Gözlem ve sorunlar
- Salça tepsisi: ham hâli magenta (palet dışı); fit sonrası koyu kırmızı. Renk sayısı 50 (sınır 48).
- Sepet: ikinci denemede kapaklı kasa gibi, üstünde bez ve ekmek; örme doku zayıf. Kullanıcı göz kararı versin.
- Kirli tabaklar: kaşıklar seçilmiyor, istif okunuyor.
- Tezgâh: tente çizgili değil, ahşap tonda; kapalı çömlek ve kase yığını var.
- Baharat rafı: duvar direği yüksek, raf küçük; çuvallar ayırt edilmiyor, otlar altta asılı.
- Dolu ve boş tabak/kase farkı küçük pencerede zor okunur.
- Öneri: `footprint_guide.py` küçük taban desteklemiyor (`--base-w` eklenmeli).
