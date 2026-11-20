unity-import: pending

# 541 Yeni ekinler 2/7: pamuk, kavun, karpuz, hıyar, şerbetçiotu (rapor)

Sonuç: **13/15 teslim** (`Assets/Art/Iso/_aday/iso_crop_<ad>_<1|2|3>_se.png`). Teslim edilmeyen: **şerbetçiotu 2** ve **karpuz 2**. Yan yana sayfası: [541-yanyana.png](541-yanyana.png). Reddedilen denemeler: `art/results/541-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, 374 yöntemi: 1x1 kılavuz + maske (`footprint_guide.py`, tuval 80x72, pivot 40,24; maske yüksekliği 10/22/32, hıyar 3 ve şerbetçiotu 3 için 40), `high top-down`, `low detail`, `basic shading`, `single color outline`, 374'teki açıklama eki. Ardından `style.py fit --family bitki` ve `check`. Her çıktıya fit uygulandı (doygunluk x0,36-0,74).
- Çağrı: 15 ilk deneme + 7 ikinci deneme (zemin/toprak karosu çıkan pamuk 3, kavun 2, kavun 3, karpuz 2, hıyar 3, şerbetçiotu 2, şerbetçiotu 3; metne "floating alone, no soil, no dirt, no ground tile" eklendi) = **22 gen** (cost_max 30). Hız sınırı hatası ilk turda 4, ikinci turda 3 çağrıda çıktı (üretim başlamadı, ücret yok); her biri 30-60 sn sonra yeniden gönderildi. Hıyar 3'ün ilk gönderimi iki kez hız sınırına takıldı.
- `get_balance` sonrası: 2201,7 (paralel şeritler de harcıyor; bu işin payı 22).

## Teslim edilenler (hepsi 80x72, pivot x 40 / y 24 alttan)
| Dosya | Opak sınır (x0,y0,x1,y1) | Yükseklik | Renk | Stil denetimi | validate_iso | Not |
|---|---|---|---|---|---|---|
| iso_crop_cotton_1_se | 20,21-60,48 | 27 | 37 | UYARI (palet dışı, kilit yok) | geçti | iki geniş yaprak |
| iso_crop_cotton_2_se | 12,12-68,60 | 48 | 39 | UYARI | geçti | beyaz pamuk topları gösteriyor, 3. evreyle karışabilir |
| iso_crop_cotton_3_se (2. deneme) | 22,11-64,55 | 44 | 44 | UYARI (ailesinden doygun x1,27) | geçti | açık koza kırmızı uçlu, zemin yok |
| iso_crop_melon_1_se | 22,26-57,55 | 29 | 55 | UYARI | renk >48 (yalnız rapor) | |
| iso_crop_melon_2_se (2. deneme) | 19,10-61,46 | 36 | 16 | UYARI | geçti | sarı çiçekli, iyi |
| iso_crop_melon_3_se (2. deneme) | 16,12-72,56 | 44 | 37 | UYARI | geçti | iki sarı-yeşil kavun, iyi |
| iso_crop_watermelon_1_se | 19,25-61,57 | 32 | 21 | UYARI (palete uzak) | geçti | dört koyu tomurcuk, fide gibi okunmuyor ⚠ |
| iso_crop_watermelon_3_se | 18,10-63,55 | 45 | 22 | UYARI | geçti | çizgili karpuz, tanınıyor |
| iso_crop_cucumber_1_se | 18,23-63,56 | 33 | 41 | UYARI | geçti | |
| iso_crop_cucumber_2_se | 23,11-59,57 | 46 | 17 | UYARI | geçti | sırığa sarılı |
| iso_crop_cucumber_3_se (2. deneme) | 28,4-52,48 | 44 | 39 | UYARI | geçti | sırıksız asılı dikey sarmaşık; hıyar meyveleri küçük |
| iso_crop_hops_1_se | 22,21-54,52 | 31 | 50 | UYARI | renk >48 (yalnız rapor) | |
| iso_crop_hops_3_se (2. deneme) | 27,6-54,65 | 59 | 50 | UYARI | renk >48 (yalnız rapor) | sırıklı, kozalaklı; tabanda küçük çim yaması kaldı ⚠ |

Yarı saydam piksel 0. Gölge pişirilmemiş, ışık sol-üstten. Bitki zemin parçası olmadığından döşeme testi yok.

## Teslim edilmeyenler
- **karpuz 2:** ilk deneme toprak karoslu; ikincide karpuz yığını (yanlış nesne). `541-red/`.
- **şerbetçiotu 2:** ilk deneme toprak karoslu; ikincide yalnızca küçük bir spiral. `541-red/`.
- Öneri: ayrı iş (kılavuza sırık çizilmiş ya da farklı yaklaşım).

## Sorunlar
- Yükseklikler tarif hedefinin dışında (374'teki gibi, model maske yüksekliğine uymuyor): ölçülen 27-59 px. Evre sırası kavun ve hıyarda tutarlı; pamukta 2 ve 3 benzer. Kodda ölçek gerekebilir.
- Dönem kuralları: Yeni Dünya bitkisi yok (pamuk, kavun, karpuz, hıyar, şerbetçiotu ortaçağ).
- Bitki ailesinin stil kilidi kurulmadı, palet uyumu uyarıdır.
- Öneri (`tools/art` değişmedi): kılavuz aracına sırık/çit eklemek yeniden denemeleri azaltır.
