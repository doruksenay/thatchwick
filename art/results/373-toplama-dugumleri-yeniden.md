unity-import: pending

# 373 Toplama düğümleri yeniden (rapor)

Sonuç: **7/8 teslim** (`Assets/Art/Iso/_aday/`), hazel teslim edilmedi. Yan yana sayfası: [373-yanyana.png](373-yanyana.png). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, referanssız 1x1 kılavuz + maske (`footprint_guide.py`, tuval 80x72, pivot x=40 / y alttan 24), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ek. Çağrılar: 8 ilk deneme + 2 ikinci deneme (berry, hazel; yalnızca biçim) = 10 generation. `get_balance` sonrası: 4826,9 (paralel şeritler de harcıyor). Üretimden sonra `style.py fit --family bitki` (doygunluk: herbs x0.55, elder x0.40, çilek x0.41, kestane x1.00, kuşburnu x0.73, ısırgan x0.66, böğürtlen x0.98).

## Dosyalar ve doğrulama
| Dosya | Boyut | Pivot x / y(alttan) | Renk | validate_iso | Stil denetimi |
|---|---|---|---|---|---|
| iso_forage_berry_bush_se | 80x72 | 40 / 24 | 45 | geçti | UYARI (palet %41) |
| iso_forage_herbs_se | 80x72 | 40 / 24 | 31 | geçti | UYARI (%50) |
| iso_forage_elderflower_se | 80x72 | 40 / 24 | 31 | geçti | UYARI (%80) |
| iso_forage_strawberry_se | 80x72 | 40 / 24 | 25 | geçti | UYARI (%100) |
| iso_forage_chestnut_se | 80x72 | 40 / 24 | 52 | geçti (araç; 48 üstü, nicemleme yapılmadı) | UYARI (%81) |
| iso_forage_rosehip_se | 80x72 | 40 / 24 | 19 | geçti | UYARI (%59) |
| iso_forage_nettle_se | 80x72 | 40 / 24 | 44 | geçti | UYARI (%41) |

Bitki ailesi kilidi kurulmadı (referans < 3): palet uyumu yalnızca uyarı.

## Sorunlar
- **hazel teslim yok:** iki denemede de ağaç + çim elması çıktı (ham: `373-red/hazel_raw1.png`, `hazel_raw2.png`). Berry ilk denemede de ağaç + toprak bloğu çıktı (`373-red/berry_bush_raw1.png`).
- Berry ikinci deneme: zemin yok ama yaprak yelpazesi eğrelti gibi, diken ve böğürtlen belli değil. Biçim zayıf, kullanıcı karar versin.
- Herbs: altında küçük çim lekesi var ("no grass" tutmadı). Elder çiçekleri büyük (kala zambağı gibi). Kestane dikenli kabuk/fındık yerine koyu toplar, 52 renk.
- Döşeme testi: zemin işi değil, yapılmadı. Çimen üstü 6x6 sayfa yapılmadı.
- Öneri: hazel için ayrı tarif (kılavuzsuz ya da daha küçük tuval).
