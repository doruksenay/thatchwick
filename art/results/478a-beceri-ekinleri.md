unity-import: pending

# 478a Beceri ekinleri ve yer mantarı (rapor)

Sonuç: **5/10 teslim** (`Assets/Art/Iso/_aday/`). Yan yana sayfası: [478a-yanyana.png](478a-yanyana.png). Reddedilen denemeler: `art/results/478a-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, referanssız 1x1 kılavuz + maske (`footprint_guide.py`, tuval 80x72, pivot 40,24; evre yükseklikleri 10/22/32; yer mantarı 10), `high top-down`, `low detail`, `basic shading`, `single color outline`, 374 ek metni. İkinci denemede (yalnızca biçim için) eke "floating alone, no soil, no dirt, no wooden box, no grass" eklendi. Çıktılara `style.py fit --family bitki` uygulandı.
Çağrılar: 10 ilk deneme + 8 ikinci deneme = **18 gen** (cost_max 22). Hız sınırı hatası yok. `get_balance` sonrası: 2492,9.

## Teslim edilenler
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Renk | Doygunluk (fit) | Stil denetimi | validate_iso |
|---|---|---|---|---|---|---|---|
| iso_crop_saffron_2_se.png | 80x72 | 40 / 24 | x24-56, y9-47 | 51 | x0,55 | UYARI (palet %40, kilit yok) | geçti |
| iso_crop_saffron_3_se.png (2. deneme) | 80x72 | 40 / 24 | x17-60, y5-52 | 47 | x0,53 | UYARI (%63) | geçti |
| iso_crop_vetch_1_se.png (2. deneme) | 80x72 | 40 / 24 | x25-54, y22-49 | 12 | x0,64 | UYARI (%75) | geçti |
| iso_crop_vetch_3_se.png (2. deneme) | 80x72 | 40 / 24 | x10-69, y8-55 | 54 | x0,51 | UYARI (%20) | geçti |
| iso_forage_truffle_se.png (2. deneme) | 80x72 | 40 / 24 | x23-57, y31-50 | 14 | x1,00 (fit gerekmedi) | GEÇTİ | geçti |

Yarı saydam piksel 0. Gölge pişirilmemiş. Bitki zemin parçası olmadığından döşeme testi yok. Bitki ailesi kilidi kurulmadı (referans < 3), palet uyumu yalnızca uyarı. Saffron 2 (51) ve vetch 3 (54) 48 rengin üstünde; nicemleme yapılmadı.

## Teslim edilmeyenler (5/10 değil: 5 eksik)
- `iso_crop_herb_bed_1_se`: ilk deneme ahşap kutu, ikinci deneme yeşil lekeler halinde, sprout olarak tanınmıyor.
- `iso_crop_herb_bed_2_se`: iki denemede de kutu (ahşap, sonra tuğla).
- `iso_crop_herb_bed_3_se`: bitki iyi (dereotu + lavanta) ama altında 64 px tam toprak elması var (zemin parçası), teslim edilmedi. Ham: `478a-red/herb3_deneme1_toprak_elmasi.png`.
- `iso_crop_saffron_1_se`: ilk deneme kutu, ikinci deneme saksıda sarı çiçek (yanlış nesne).
- `iso_crop_vetch_2_se`: iki denemede de çim bloğu zemini, sarmaşık biçimi tanınıyor.

## Sorunlar
- Saffron 3 ve vetch 3 ilk denemede zemin parçalıydı, ikinci deneme temiz. Vetch 3 mavi-pembe çiçekli çim demeti gibi; çiçek rengi mor olarak okunmuyor.
- Yükseklikler kılavuzun dışında kalabilir (374'te de böyleydi): vetch 1 = 28 px, saffron 3 = 48 px.
- Öneri: kalan dördü (herb 1-3'ten 1 ve 2, saffron 1, vetch 2) için kutusuz/zeminsiz ayrı tarif, ya da herb 3 / vetch 2'yi zemin maskesiyle (bitkiyi koru, zemini sil) kırpmak.
