unity-import: pending

# 418a Ürün ikonları A: sonuç (durum: review)

Yan yana sayfası: [418a-yanyana.png](418a-yanyana.png) (arayuz ailesinde kabul referansı yok, yalnızca yeni görseller). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 19 x `create_map_object` basic mod, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (417/406 yöntemi), her açıklamanın sonunda `, simple still-life object, no face, no eyes, no creature, no person`. **19 gen** (cost_est 19, cost_max 38).
- Hız sınırı: paylaşılan hesapta eşzamanlı iş sınırı ("rate limit exceeded") 6 çağrıda alındı (salted_poultry, smoked_cheese, pickled_carrot, pickled_radish ve iki tekrar); başarısız çağrı harcama yapmadı, 15-30 sn sonra bir kez yeniden gönderildi (tarifin tek istisnası).
- **Renk indirgeme:** 9 ikon ham çıktıda 17-40 renkti (ölçüt ≤16): Pillow median-cut 16 renk, dither yok, alfa 0/255; yeniden boyama yok. Ham hâller `art/tmp418a/` altında (commit edilmedi).
- `get_balance` sonrası: **3925,5** (paralel şeritlerin harcaması dahil).

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_{milk, salted_meat, salted_poultry, pastirma, smoked_meat, smoked_poultry, cheese, brined_cheese, smoked_cheese, pickled_carrot, pickled_beet, pickled_onion, pickled_radish, dried_apple, dried_pear, dried_plum, dried_mushroom, jam_berry, jam_plum}.png: 19 dosya, hepsi teslim edildi.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (19/19) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (19/19, indirgeme sonrası) |
| `style.py check --family arayuz` | KALDI yok; 2 GEÇTİ (brined_cheese, smoked_poultry), 17 UYARI (kilit kurulmadı, palet dışı) |
| Yüz/göz/figür | Sayfada görülmedi (19/19 temiz) |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

Teslim edilmeyen: yok.

## Sorunlar
- Aile dili göz kontrolünde: `salted_poultry` ve `smoked_poultry` kümes hayvanı olarak zor tanınıyor (salted_poultry kırmızı, belirsiz şekil); `salted_meat`/`pastirma` biçim olarak benzer; `dried_apple` yalnızca tek halka (dizi yerine); `cheese` düz sarı disk. Kullanıcı kararı gerekirse bu 4 ikon için yeniden üretim önerilir (4 gen).
- `pickled_*` kavanozları cam görünümlü; tarif "kavanoz" dediği için aynen bırakıldı (dönem notu: cam eşya yalnızca cam zinciri ürünlerinde, kullanıcı kontrol etmeli).
