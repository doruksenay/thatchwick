unity-import: pending

# 378 Toplama düğümleri devam (rapor)

Sonuç: **1/3 teslim** (kestane v2). Fındık ve böğürtlen v2 tutmadı, teslim edilmedi. Yan yana sayfası: [378-yanyana.png](378-yanyana.png) (kestane, böğürtlen fit, fındık ham). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme; saydam 64x64 tuval, maske tuvalin tamamı, pivot (32,16). `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifin açıklamaları. 3 çağrı x 1 gen = 3 gen. İlk gönderimde 3 çağrı "rate limit exceeded" verdi (üretim başlamadı); 45 sn beklenip bir kez yeniden denendi. `get_balance` sonrası: 3745,5 (paralel şeritler de harcıyor).

## Dosyalar ve doğrulama
| Dosya | Boyut | Pivot x / y(alttan) | Renk | validate_iso | Stil denetimi |
|---|---|---|---|---|---|
| iso_forage_chestnut_v2_se | 64x64 | 32 / 16 | 37 | geçti | UYARI (palet %96, doygunluk x1.15; fit x0.48 uygulandı) |

Bitki kilidi kurulmadı; palet uyumu uyarı. Kestane kahverengi-kırmızı, yeşil kabuklu, mor yok.

## Sorunlar
- **fındık teslim yok:** yine ağaç (gövde, yuvarlak taç, altında çim). Ham: `378-red/hazel_raw.png`.
- **böğürtlen v2 teslim yok:** koyu mor/magenta yumru; diken, kamış, yaprak yok. Ham ve fit (x0.37, hâlâ mor): `378-red/berry_v2_raw.png`, `berry_v2_fit.png`. Şekil sorunu fit ile çözülmez.
- Tek deneme kuralı gereği ikinci deneme yapılmadı. Öneri: fındık için dal üstünde kabuklu fındık salkımı ikonu (çalı yerine); böğürtlen için küçük "black berries on thorny cane" salkımı, "purple/magenta" yasağıyla.
- Döşeme testi: zemin işi değil.
