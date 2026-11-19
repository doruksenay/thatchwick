unity-import: pending

# 419b Ürün ikonları B: ekin, tohum, fidan, meyve, toplanan (durum: review)

Yan yana sayfası: [419b-yanyana.png](419b-yanyana.png) (arayuz ailesinde kabul referansı yok, yalnızca yeni görseller). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 19 x `create_map_object` basic mod, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (417/418a yöntemi), her açıklamanın sonunda `, simple still-life object, no face, no eyes, no creature, no person`. **19 gen** (cost_est 19, cost_max 38).
- Hız sınırı: paylaşılan hesapta eşzamanlı iş sınırı ("rate limit exceeded") alındı (nettle, elderflower, wild_strawberry, chestnut, cress; wild_strawberry, chestnut, cress ikinci kez de); başarısız çağrılar harcama yapmadı, bekleyip yeniden gönderildi (tarifin tek istisnası).
- **Renk indirgeme:** 3 ikon ham çıktıda 16'dan fazla renkti (chestnut 20, sapling_apple 18, wild_strawberry 25): Pillow median-cut 16 renk, dither yok, alfa 0/255; yeniden boyama yok. Ham hâller `art/tmp419b/` altında (commit edilmez).
- `get_balance` sonrası: **3886,5** (paralel şeritlerin harcaması dahil).

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_{seed_spinach, apple, sapling_apple, pear, sapling_pear, cherry, sapling_cherry, plum, sapling_plum, wild_garlic, nettle, elderflower, wild_strawberry, chestnut, rosehip, pinecone, cress, sapling_birch, sapling_pine}.png: 19 dosya, hepsi teslim edildi.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (19/19) |
| Yarı saydam piksel | 0 |
| Renk sayısı | <=16 (19/19, indirgeme sonrası) |
| `style.py check --family arayuz` | KALDI yok; 2 GEÇTİ (chestnut, pinecone), 17 UYARI (kilit kurulmadı / palet dışı) |
| Yüz/göz/figür | Sayfada görülmedi (19/19 temiz) |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

Teslim edilmeyen: yok.

## Sorunlar (göz kontrolü, kullanıcı kararı)
- `chestnut`: kestane yerine kesilmiş kütük parçalarına benziyor; yeniden üretim önerilir.
- `elderflower`: mor/lavanta sapına benziyor (beyaz çiçek kümesi değil); yeniden üretim önerilir.
- `nettle`: tek bir iri yaprak (ısırgan demeti değil); `rosehip` tek meyve (çoğul değil); `wild_garlic` zor tanınıyor, kök/yaprak demeti.
- `sapling_plum` ve `sapling_pear`: fidan yerine meyve çizdi (erik ve armut gibi görünüyor), tarif "fidan" demişti; yeniden üretim önerilir. `sapling_apple`/`cherry` dalında meyveli, fidan olarak okunabilir. `sapling_birch` çıplak dal; `sapling_pine` küçük çam (toprak topağı belirgin değil).
- `sapling_*` ailesinde "toprak topaklı" kuralı çoğunda uygulanmadı.
- Yeniden üretim için 6 ikon = 6 gen önerisi.
