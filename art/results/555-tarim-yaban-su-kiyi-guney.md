unity-import: pending

# 555 Yaban bitkileri 3/3: ırmak, kıyı ve Güney Yamaçları (rapor)

Sonuç: **3/9 teslim** (`Assets/Art/Iso/_aday/`). Yan yana sayfası: [555-yanyana.png](555-yanyana.png). Teslim edilmeyenler: `art/results/555-red/` (ham ve fit). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme; `footprint_guide.py` ile 1x1 kılavuz + maske (80x72, pivot 40,24; maske yükseklikleri 22/26/24/6/12/14/26/16/12), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki açıklama + 373 eki. Çağrı başı 1 gen, 9 çağrı = **9 generation** (cost_max 18). Kenger ve zahter ilk gönderimde hız sınırına (rate limit) takıldı, ücretsiz reddedildi; birkaç sn sonra bir kez yeniden gönderildi. İkinci deneme yapılmadı. Hepsine `style.py fit --family bitki` uygulandı (doygunluk: meyan x0,48; su kestanesi x0,70; deniz börülcesi x0,37; söğüt x0,37; hatmi x0,73). `get_balance` sonrası: 1966,7 (paralel şeritler de harcıyor).

## Teslim edilenler
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Renk | Stil (bitki) | validate_iso |
|---|---|---|---|---|---|---|
| iso_forage_licorice_se.png | 80x72 | 40 / 24 | x18-58, y11-61 | 46 | UYARI (palet %95, uzaklık 10,9) | geçti |
| iso_forage_water_chestnut_se.png | 80x72 | 40 / 24 | x24-55, y26-47 | 45 | GEÇTİ (%100) | geçti |
| iso_forage_samphire_se.png | 80x72 | 40 / 24 | x20-59, y22-53 | 33 | GEÇTİ (%100) | geçti |

Yarı saydam piksel 0; gölge pişirilmemiş. Bitki zemin parçası olmadığından döşeme testi yok.

## Teslim EDİLMEYEN (6/9)
| Dosya | Stil | Neden |
|---|---|---|
| iso_forage_marsh_mallow_se | GEÇTİ (%96) | altında çim elması (zemin parçası), "no grass patch" tutmadı |
| iso_forage_willow_bark_se | GEÇTİ (%99) | söğüt gibi değil: yuvarlak yeşil topuzlu saplar, soyulmuş kabuk okunmuyor |
| iso_forage_caper_se | — | toprak/kaya bloğu üstünde yeşil top; kapari belli değil |
| iso_forage_myrtle_se | — | çalı değil ağaç + çim elması |
| iso_forage_kenger_se | — | toprak bloğu, mor çiçekli gül/şakayık gibi, dikenli kenger değil |
| iso_forage_zahter_se | — | tam toprak-çim bloğu, bitki ayrı okunmuyor |

## Sorunlar
- Altı görsel tek denemede biçim hatasıyla elendi (çoğu zemin bloğu); tarif gereği ikinci deneme yapılmadı. Meyan kökü biraz doygun/pembe-mor, uyarıyla teslim.
- Öneri: elenenler için yeni iş; tanımda "floating alone, no base, no soil, no grass" vurgusu, myrtle/kapari/zahter için daha küçük maske yüksekliği.
