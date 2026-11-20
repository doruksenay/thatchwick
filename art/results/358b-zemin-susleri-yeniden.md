unity-import: pending

# 358b Zemin süsleri yeniden (bulut şeridi 2)

- Araç: `create_map_object`, stil eşleme + özel maske, 8 çağrı paralel, çağrı başına 1 gen (toplam 8). `get_balance`: 2475,9 → 2461,9 (düşüş 14, paralel şeritlerin harcamaları dahil).
- Kalan (`get_balance`): 2461,9.
- Fark (358'e göre): önceki turda saydam tuval kullanılmıştı, model altına plaka pişirmişti. Bu turda arka plan olarak **opak çimen**: 001 çimen karosunun dörtlü döşemesinden kesilmiş 32x32 parça (`background_image`), maske ortada 24x22 oval. Model süsü çimenin içine çizdi, çıktı saydam arka planlı geldi; plaka/kare yok. Lineless, basic shading, low detail; tarife göre çakıl yalnızca seçici kontur.
- Kırpma: araç en az 32 px istiyor; sonuç maske bölgesine (y 6-29) kırpıldı → 32x24. İçerik tamamen bu bölgede, yeniden boyama yok.
- Doygunluk: 7 görsel "ailesinden çok doygun" → `style.py fit` ile aileye indirildi (x0,36-0,75). Ham hâlleri `art/358b-red/`de. Molehill fit'siz (UYARI).
- Masaüstü kopyası: yerel.

## Teslim edilenler (`Assets/Art/Iso/_aday/`, 32x24, pivot x 16, y 8 ALTTAN)
iso_decal_daisies_v2, buttercups_v2, clover_v2, pebbles_v2, grass_tuft_v2, fern_v2, leaves_autumn_v2, molehill_v2 (8/8).

## Doğrulama
Yan yana sayfası (358'in reddedilen çıktısı da referans olarak 358-yanyana'da): `art/results/358b-yanyana.png`
Çimen üstü serpme testi (8x8 çimen, 28 rastgele süs, 2x): `art/results/358b-serpme.png`

| Dosya | Palete yakın | Doygunluk/aile | Sonuç | validate (32x24, yarı saydam 0) | Yükseklik |
|---|---|---|---|---|---|
| daisies | %30 | x1,15 | UYARI (palet) | geçti | 19 px |
| buttercups | %37 | x1,14 | UYARI | geçti | 19 px |
| clover | %8 | x1,15 | UYARI | geçti | 21 px |
| pebbles | %100 | x1,15 | GEÇTİ | geçti | 20 px |
| grass_tuft | %28 | x1,15 | UYARI | geçti | 20 px |
| fern | %52 | x1,15 | UYARI | geçti | 18 px |
| leaves_autumn | %91 | x1,15 | GEÇTİ | geçti | 19 px |
| molehill | %100 | x1,34 | UYARI (doygun) | geçti | 22 px |

'bitki' kilidi kurulmadı: palet uyumu uyarı. Döşeme testi zemin karosu olmadığından uygulanmadı; yerine serpme testi.

## Sorunlar
- Süsler tarifteki 10/14 px yükseklik sınırını aşıyor (18-22 px, 32x24 tuvalin tamamına yakın). Kod tarafında küçültme ya da ölçek düşünülebilir (358'de de 17 px'e kadar çıkmıştı).
- Molehill dairesel kahverengi bir tepecik, etrafında koyu halka var; serpmede diğerlerinden belirgin. Kullanıcı kararı.
- Clover öbek biçiminde; beyaz yonca çiçekleri çok küçük kaldı.
- Öneri (`tools/art` değişmedi): bu "opak çimen arka plan + oval maske" yöntemi plakayı önlüyor; diğer zemin süsleri için kullanılabilir.
