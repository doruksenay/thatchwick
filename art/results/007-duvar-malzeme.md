unity-import: pending
# 007 Duvar malzeme örneği: sonuç

Not: `unity-import` için içe aktarılacak oyun varlığı yok. Bu iş yalnızca örnek görsel üretti; Unity tarafının bir şey yapması gerekmiyor.

- İş: 007, durum `review` (görsel onay kullanıcıda).
- Tarih: 2026-11-08.

## Araç ve ayarlar
`create_building_kit` aracı kullanıldı: `tile_type=isometric`, `tile_size=64`, `layout=materials`, `wall_tiles=2`. Diğer ayarlar varsayılanda bırakıldı: `outline_mode=outline`, `roof_profile=gable`, `tile_view_angle` = 30 (2:1).
- `wall_description` tarifteki metinle aynı: "medieval village cottage wall, exposed timber frame with plaster infill, muted warm tones, flat few-tone pixel art, no roof, no shadow".
- `floor_description` = "packed earth floor, muted brown". Bu alan zorunlu ama tarifte yoktu. Zemin paletiyle (toprak) uyumlu, nötr bir seçim yaptım. Örnek yalnızca duvar malzemesini göstermek için olduğundan sonucu etkilemez.
- Çağrıdan önce okunan maliyet: aracın açıklamasında "layout='materials' is 10 generations at every type and every tile_size" yazıyor. `cost_est` 10 ile uyumlu.
- Tile id: `8af51dea-ef01-449d-a3b4-2884862907dd`. 80 parça, her biri 102x115 tuvalde.

## Harcama
| Çağrı | Generation |
|---|---|
| create_building_kit (materials, 64) | 10 (araç çıktısı: "charged: 10 generations") |

`get_balance` iş öncesinde 1965, iş sonrasında 1950 gösterdi. Fark 15, bu işin ücreti ise 10. Aynı anda çalışan zemin oturumu 5 generation harcamış olabilir; bunu doğrulayamadım.

## Dosyalar
| Yol | Boyut | Not |
|---|---|---|
| `art/results/007-duvar-malzeme-ornek.png` | 2040x230 | Örnek şerit, en yakın komşu ile 2 kat, #333333 zemin. Soldan sağa: zemin (0), kenar duvar N (1), kenar duvar W (4), köşe NW (8), sütun (31), kapı N (32), kapı W (38), kapalı kutu NESW (53), üst kat/düz çatı (44), piramit çatı (78) |

Oyun varlığı değil, bu yüzden `Assets/Art/Iso`'ya konmadı ve pivot verilmedi. 008 kabul edilirse parçalar oradan üretilecek.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Yarı saydam piksel (seçilen 10 parça) | 0 |
| Renk sayısı (10 parçanın toplamı) | 32. Kutu (53) tek başına 24, zemin (0) 5 |
| `validate_iso.py --kind object` (53 ve 0) | ikisi de geçti |
| Işık (kutu 53'te ortalama parlaklık) | üst 159.4 > sol 151.8 > sağ 108.3. Işık sol-üstten, kurala uygun. Sol ile üst arasındaki fark küçük |
| Gölge | Pişirilmiş gölge yok |
| Düz, az tonlu | Evet. Sıva açık krem, 2-3 ton ve hafif benekli doku. Ahşap çatkı orta-koyu kahve, çapraz ve yatay kirişler okunuyor |
| Palet uyumu (görsel not) | Toprak tonlarında, ılık ve sönük. Köylünün kahve/bej paletiyle uyumlu. Zemin karolarıyla yan yana denenmedi |

## Sorunlar ve notlar
- Kenar duvarlar ince ve elmas kenarına oturuyor; duvar yüksekliği 2 kat (64 px). Kit parçaları 64x32 hücreye kesilmez, ortak 102x115 tuvalde bütün olarak yerleştirilir. Bu, 008'de adlandırma ve pivot kararını etkiler.
- Zemin parçası (0) opak alanı 64x33 (y 73-105). Elmas tam 2:1'den 1 satır uzun. 008'de zemin kullanılacaksa maskeyle kırpılmalı.
- Bir odayı parçalardan kurmayı denedim. Kenar ve köşe parçalarının hücre içindeki konumunu doğru eşleyemedim (köşe 8 L değil, sütun gibi duruyor). Bu yüzden örnekte oda yok, yalnızca parça şeridi var. 008'den önce kit yerleşim kuralının (`get_tiles_pro` placement_rules) iyice incelenmesi gerekiyor.
- Aracın çıktısında `view: low top-down` yazıyor. Kit isometric 30 derece (2:1) parametresiyle üretildi ve zemin elması 64 px genişliğinde. Bu etiket büyük olasılıkla yalnızca bir metaveri.
