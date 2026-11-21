unity-import: pending

# 784 At, inek, eşek (durma): teslim yok

Sonuç: **failed**. Hiçbir PNG `Assets/Art/Iso/` altına konmadı. Ham çıktılar `art/results/784-red/` altında.

## Araç ve ayarlar
- `create_image_pixflux`, `direction=south`, `view=high top-down`, `outline=single color black outline`, `detail=medium detail`, `shading=basic shading`, `no_background=true`; palet `color_image_base64` = `iso_animal_ox_v2_cand_se.png`. Tuvaller tarifteki gibi: at 80, inek 64, eşek 68. Açıklama = tablo metni + 106 ortak eki + 375 eki (`desaturated` dahil).
- v3 döndürme (yalnızca inek): `create_character` `mode=v3`, `body_type=quadruped`, `template=horse` (tarif), referans = inek tek karesi (`upload:` ile; inline base64 "broken data stream" verdi, aynı bayt yüklemeyle çalıştı).

## Çağrılar (generation)
| Çağrı | Gen | Sonuç |
|---|---|---|
| pixflux at (1) | 1 | yandan duruyor (sola bakan profil) → güney kontrolü KALDI |
| pixflux inek | 1 | önden, kayma yok (bbox 24x41, orta sütun 32) → geçti |
| pixflux eşek (1) | 1 | yandan profil → KALDI |
| pixflux at (2, `head and chest toward the camera, body foreshortened behind`) | 1 | hâlâ 3/4 yandan (baş sola dönük, gövde sağa uzanıyor) → KALDI, döndürülmedi |
| pixflux eşek (2, aynı ek) | 1 | hâlâ yandan; yalnızca baş öne dönük, gövde profilde → KALDI, döndürülmedi |
| v3 döndürme inek | 1 | 8 yön geldi; boy tuttu değil (aşağıda) |
| **Toplam** | **7** | `cost_est` 6, `cost_max` 14 |

`get_balance` sonrası: 9546 / 10000 (paralel şeritler aynı anda harcadığı için bu iş tek başına 7).

## Karar
- **At (`horse_medium`) ve eşek:** iki denemeden sonra da güney karesi yandan/3-4; 486 dersine göre bir yeniden deneme yapıldı, tutmadı, gövde teslim edilmez (yeniden deneme hakkı bitti). Döndürme yapılmadı (gen harcanmadı).
- **İnek:** güney karesi doğru, 8 yön döndü, ama `scale_check.py` sığır sınıfı (56-68) dışı:

| Yön | Boy (px) | Sınıf |
|---|---|---|
| se | 43 | KALDI (56-68) |
| s | 41 | KALDI |
| e | 43 | KALDI |
| diğer yönler | 43-46 (bbox) | KALDI |

Neden: pixflux 64 tuvalde inek yalnız 41 px boy çizdi (doluluk 0,64); tarifin "boy ≈ tuval x 0,91" tahmini yanlış (ox_v2'de 68 tuval → 62). Yeniden örnekleme yasak olduğu için büyütme yapılmadı; tek deneme kuralı gereği yeniden üretilmedi. Stil denetimi, doğrulama ve yan yana sayfası teslim edilen dosya olmadığı için üretilmedi.

## Öneri (tahmin)
- İnek için tuval ≈ 64 x 56/41 ≈ **88-92**; pixflux alan sınırı 400x400 içinde. At için 80 tuvalde at yandan çıkıyor: yüz önden zor; tek kare yerine `create_image_pro_flash` ya da 3/4 kare + `mode=pro` döndürme ya da `create_8_direction_object` denenebilir (maliyetlerini çağrıdan önce okumak gerekir). Eşek için aynı.
- Karakter kimliği: inek v3 `0f32ba38-bf82-4337-b645-3f504e6fc7d5` (boy dışı, 785/786 kullanmamalı). At ve eşek için karakter üretilmedi.

## Dosyalar
Teslim edilen yok. Reddedilenler `art/results/784-red/`: `cow_tek.png`, `iso_animal_cow_v2_{se,sw,nw,ne,s,w,n,e}.png` (64x64, pivot x 32, boy 41-46), `horse_medium_tek{1,2}_*.png`, `donkey_tek{1,2}_*.png`.

Masaüstü kopyası: yerel.

## Sorular
- İnek tuvali büyütülüp (≈90) yeniden denensin mi, at ve eşek için hangi yol (pro_flash / pro döndürme)? Bu iş `failed`; yeni deneme için yeni iş dosyası gerekir.
