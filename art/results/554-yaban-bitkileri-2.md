unity-import: pending

# 554 Yaban bitkileri 2/3: çayır ve tepe (rapor)

Sonuç: **5/9 teslim** (`Assets/Art/Iso/_aday/`). Yan yana sayfası: [554-yanyana.png](554-yanyana.png) (referanslar + 9 yeni, hepsi fit sonrası). Teslim edilmeyenler: `art/results/554-red/` (ham ve fit). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuzu + maske (80x72, pivot x=40 / y alttan 24; yükseklikler 12/16/10/8/20/32/26/16/18), `high top-down`, `low detail`, `basic shading`, `single color outline`, 373 eki + `, no face, no eyes, no creature, no person`. Çağrı başı 1 gen = **9 generation** (cost_max 18), ikinci deneme yapılmadı. Sumac çağrısı hız sınırı (rate limit) hatası verdi, ~30 sn sonra bir kez yeniden gönderildi (ilk çağrı başlamadı, ücret yok). `get_balance` sonrası: 1966,7 (paralel şeritler de harcıyor). Hepsine `style.py fit --family bitki` uygulandı (doygunluk x0,37-0,86).

## Teslim edilenler (5/9)
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Renk | Stil (bitki) | validate_iso |
|---|---|---|---|---|---|---|
| iso_forage_sorrel_se | 80x72 | 40 / 24 | x20-59, y21-52 | 35 | GEÇTİ (%97) | geçti |
| iso_forage_mallow_se | 80x72 | 40 / 24 | x20-60, y15-50 | 55 | UYARI (%90, palete uzak) | geçti |
| iso_forage_dandelion_se | 80x72 | 40 / 24 | x28-52, y22-47 | 56 | GEÇTİ (%100) | geçti |
| iso_forage_wild_asparagus_se | 80x72 | 40 / 24 | x24-55, y15-52 | 42 | GEÇTİ (%100) | geçti |
| iso_forage_sumac_se | 80x72 | 40 / 24 | x23-56, y15-56 | 37 | GEÇTİ (%90) | geçti |

Yarı saydam piksel 0, gölge pişirilmemiş. Bitki zemin parçası olmadığından döşeme testi yok.

## Teslim EDİLMEYEN (4/9)
| Dosya | Stil | Neden |
|---|---|---|
| madımak (knotweed) | GEÇTİ (%100) | **zemin bloğu**: tüm ayak izini kaplayan çimli toprak karosu, bitki değil |
| ardıç (juniper) | GEÇTİ (%100) | koyu çam benzeri ağaç, kahverengi toprak karosu üstünde; alçak çalı değil |
| salep orkidesi | KALDI (%79, palet dışı renkler) | şekil iyi (mor başak) ama mor çiçek aile paletinde yok; `fit` yalnızca doygunluğu düzeltir, palet sorunu değil |
| sarı kantaron | KALDI (%77, palet dışı renkler) | şekil kabul edilebilir (sarı yıldız çiçekleri) ama sarı palet dışı; aynı gerekçe |

## Sorunlar
- Madımak ve ardıç için yeni iş gerekir ("floating alone, no soil, no ground tile" vurgusu, daha küçük tuval ya da kılavuzsuz); orkide ve kantaron için aile paletine çiçek renklerinin eklenmesi (kullanıcı kararı) ya da vurgu paleti.
- Sumac opak yüksekliği 42 px (hedef 32), kuşkonmaz 38 (hedef 20), kuzukulağı 32 (hedef 12): kod tarafı ölçek gerektirebilir. Sumac kırmızı, ağaççık gibi okunuyor.
- Ebegümeci palete uzak uyarısı aldı (pembe çiçekler).
