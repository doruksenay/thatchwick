unity-import: pending

# 418b Ürün ikonları A parça 2/3 (durum: review)

Yan yana sayfası: [418b-yanyana.png](418b-yanyana.png) (arayuz ailesinde kabul referansı yok, yalnızca yeni görseller). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 19 x `create_map_object` basic mod, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (401/417 yöntemi), her açıklamanın sonuna "simple still-life object, no face, no eyes, no creature, no person". 19 çağrı = **19 gen** (cost_est 19, cost_max 38).
- Hız sınırı: paylaşılan hesapta 429 ("rate limit exceeded") alındı; başarısız çağrılar harcama yapmadı, boşalınca yeniden gönderildi (tarifin tek istisnası). Her ikon tam bir kez üretildi.
- **Renk indirgeme:** ham çıktılar 5-41 renk (ölçüt ≤16); 14 dosya Pillow median-cut ile 16 renge indirildi (dither yok, alfa 0/255), yeniden boyama yok. Ham hâller `art/tmp418b/raw/` altında (commit edilmedi).
- `get_balance` sonrası: **3934,5** (paralel şeritler dahil).

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_{salted_fish, smoked_fish, bread, ale, cider, mead, yogurt, butter, ayran, boza, serbet, vinegar, tarhana, soup_bean, soup_fish, stew_meat, soup_chicken, saute_mushroom, kaygana}.png: 19 dosya, hepsi teslim edildi.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (19/19) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (19/19, indirgeme sonrası) |
| Yüz/göz/figür | göz kontrolünde yok (19/19) |
| `style.py check --family arayuz` | KALDI yok; GEÇTİ 3 (mead, saute_mushroom, yogurt), UYARI 16 (kilit kurulmadı / palete uzak) |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

## Sorunlar
1. `salted_fish` kırmızı-turuncu taze balık gibi, "tuzlanmış kurutulmuş" okunmuyor. Palete en uzaklar: smoked_fish (%32), salted_fish (%48), tarhana (%55).
2. `ale` yalnızca 5 renk, koyu kahve düz kupa; `cider` ve `ale` kil dokusu zayıf. Kullanıcı gözle karar vermeli.
3. Kase/çanak perspektifi ve boyutu ikonlar arasında biraz değişiyor (soup_fish diğerlerinden büyük).
4. Teslim edilmeyen ikon yok.
