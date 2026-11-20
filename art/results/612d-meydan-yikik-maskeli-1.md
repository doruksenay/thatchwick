unity-import: pending

# 612d Köy meydanı: yıkık hâller (maskeli düzenleme)

Araç: `inpaint_image_pro_flash` (Modify current layer), onarılmış `_aday/iso_square_*_se.png` kaynak, `art/tmp/` altında Python ile dikdörtgen maske. Gerekçe: iş dosyası önce bunu istiyor, tek çağrı 5-9 gen. Her yapı için tek deneme (4 çağrı paralel).

Yan yana sayfası (referanslarla): `art/results/612d-yanyana.png`. Onarılmış | yıkık | maske sayfası: `art/results/612d-onar-yikik-maske.png`. Ham çıktılar: `art/results/612d-ham/`.

## Çağrılar
| Yapı | Kaynak boyutu | Maske (x0,y0,x1,y1) | Generation |
|---|---|---|---|
| kuyu | 128x128 | 10,0,118,80 | 6 |
| kiler | 128x96 | 48,14,110,78 | 6 |
| çan kulesi | 128x256 | 0,0,128,168 | 9 |
| pazar çardağı | 192x192 | 0,0,192,128 | 6 |

Toplam 27 gen (cost_est 24, cost_max 44). `get_balance` sonrası kalan: 815,6 (başka şeritler aynı anda harcıyor olabilir; sonuç satırı defterde).

## Dosyalar (`Assets/Art/Iso/_aday/`)
| Dosya | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_square_well_ruin_v2_se.png | 128x128 | 64 | 16 |
| iso_square_cellar_ruin_v2_se.png | 128x96 | 64 | 6 |
| iso_square_belltower_ruin_v2_se.png | 128x256 | 64 | 7 |
| iso_square_market_hall_ruin_v2_se.png | 192x192 | 96 | 15 |

Pivot = tuval yatay ortası, alttan boş satır sayısı (kaynaklarla aynı tuval ve yerleşim).

## Doğrulama
| Dosya | validate_iso (object, 48 renk) | Maske dışı fark | Stil denetimi (yapi) |
|---|---|---|---|
| kuyu | geçti (42 renk) | 0 | GEÇTİ, palete %91, doygunluk x0.96 |
| kiler | geçti (48 renk, ham 84) | 0 | GEÇTİ, %91, x1.17 |
| çan kulesi | geçti (48 renk, ham 69) | 0 | UYARI: ailesinden soluk (x0.55); şekil olarak yapı onarılmışla aynı |
| pazar çardağı | geçti (48 renk, ham 54) | 0 | GEÇTİ, %100, x1.06 |

Renk sınırı: model üç çıktıda 48 rengi aştı (kaynaklar 31-40 renk). Yalnızca maske içi pikselleri, kaynak renkleri artı en sık geçen yeni renkler (toplam 48) paletine en yakın renge eşledim (değişen piksel: kiler 523, çan kulesi 659, pazar çardağı 227); yeniden çizim yok. Ham hâller `612d-ham/`de. Maske dışı fark üç dosyada da çıktıda zaten 0 çıktı (araç maske dışını byte'a kadar koruyor); yine de maske dışını kaynaktan geri kopyaladım.

## Sorunlar / kullanıcı bakışı için
- Çan kulesi: kiremit çatı ve çan katı gitti, kırık direk uçları var; ama **çan kaidenin dibinde görünmüyor** ve kiremit kırıkları çok az. Maske y=168'de bitiyor, bu yüzden duvar ile kaide arasında yatay bir dikiş (açık gri bant) var.
- Pazar çardağı: yarı çökük çatı ve devrik direk okunuyor; yerdeki moloz küçük ve seyrek. Tezgâhlar kırık okunmuyor (maske y=128'de kesiyor).
- Kuyu: çatı ve kova yok, bilezik taşları dağılmış, yabani ot var; direk dibi sol-altta kaldı (maske dışı).
- Kiler: kemer yarı çökük, kapı sarkıyor, toprak basamaklarda. İyi.
- Zemin döşeme testi bu iş için gerekmiyor (nesne). Gölge pişirilmemiş, ışık sol-üstten.
- Masaüstü kopyası: yerel.
