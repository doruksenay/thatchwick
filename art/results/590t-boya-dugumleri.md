unity-import: pending

# 590t Boya düğümleri (rapor)

Sonuç: **2/3 teslim** (`Assets/Art/Iso/_aday/`), likenli kaya teslim edilmedi. Yan yana sayfası: [590t-yanyana.png](590t-yanyana.png) (ısırgan, otlar, `iso_terrain_boulder_se` ve ham liken denemesiyle). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, 373 yöntemi: 80x72 tuval, 1x1 kılavuz + maske (`footprint_guide.py`, pivot x=40 / y alttan 24), `high top-down`, `low detail`, `basic shading`, `single color outline`. 6 çağrı x 1 gen = 6 generation (3 ilk deneme + 3 ikinci deneme). `get_balance` sonrası: 1844,7 (paralel şeritler de harcıyor).
- İlk deneme (tarifteki açıklama): üçü de zemin elmasıyla çıktı (cehri: çimenli elmas üstünde ağaç; ceviz: koyu zemin karosu; liken: toprak/çim bloğu üstünde kaya). Ham: `590t-red/*1.png`.
- İkinci deneme (yalnızca biçim; açıklama "yüzen nesne, zemin yok, ağaç değil" diye yeniden yazıldı): cehri ve ceviz zemini attı. Liken yine taş çerçeveli çimenli kare çıktı. Deneme hakkı bitti.
- Teslim edilenlere `style.py fit --family bitki` uygulandı: cehri doygunluk x0.52, ceviz x0.47.

## Dosyalar ve doğrulama
| Dosya | Boyut | Pivot x / y(alttan) | Renk | validate_iso | Stil denetimi (fit sonrası) |
|---|---|---|---|---|---|
| iso_forage_buckthorn_se | 80x72 | 40 / 24 | 49 | geçti | GEÇTİ (palet %100, x1.15) |
| iso_forage_walnut_hull_se | 80x72 | 40 / 24 | 59 | geçti | GEÇTİ (palet %100, x1.15) |
| iso_forage_lichen_se | teslim yok | | 31 | geçti | KALDI (palet %65, doygun x1.28), ham: `590t-red/lichen2.png` |

## Sorunlar
- **Likenli kaya teslim yok:** iki denemede de zemin/çerçeve kareli çıktı, kaya değil. Öneri: ayrı iş; `iso_terrain_boulder_se` (357) üstüne liken lekesi boyayan `pixellab: false` iş ya da kılavuzsuz tarif.
- Cehri yüksekliği ~49 px (tarif 26), yaprak kümesi geniş (60 px); çalı olarak okunuyor ama hazel/mürver kadar büyük. Meyveler sarı-turuncu noktalar.
- Ceviz yığını ~37 px yüksek (tarif 8); yeşil kabuklar yıldız biçimli, kahverengi ceviz ve kuru yapraklar var. Okunabilir ama kabuklar yaprak gibi duruyor; kullanıcı karar versin.
- Döşeme testi: zemin işi değil, yapılmadı.
