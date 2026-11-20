unity-import: pending

# 486 At tipleri, eşek, katır, bardo: teslim yok (failed)

Sonuç: `status: failed`. 7 pixflux tek karesi üretildi; hepsi güney kontrolünde elendi (yandan duruş: baş sola dönük, gövde yan profil), üçünde ayrıca zemin lekesi var. Tek deneme kuralı gereği döndürme (v3) yapılmadı ve yeniden denenmedi. Assets'e dosya konmadı. Masaüstü kopyası: yerel (bulut modu).
Tek kareler yan yana (x4): `art/results/486-yanyana.png`. Ham kareler: `art/results/486-red/`.

## Araç ve ayarlar
375 tarifi: `create_image_pixflux`, `view=high top-down`, black outline, `direction=south`, `no_background`, medium detail, basic shading. Palet zorlaması: koyun 1 SE'nin 10 rengi (şerit PNG). Açıklamaya 375 eki, `desaturated` ve 486 eki (`neutral grey-brown coat, plain coat...`) eklendi. Tuvaller: at 3 tip + katır 56, midilli ve eşek 44, bardo 48. Hız sınırı hatası olmadı.

## Tek kare kontrolü
| Gövde | Tuval | Boy x en (opak) | Duruş | Zemin lekesi | Karar |
|---|---|---|---|---|---|
| horse_light | 56 | 50 x 52 | yandan | yok | döndürülmedi |
| horse_medium | 56 | 49 x 38 | yandan | yok | döndürülmedi |
| horse_heavy | 56 | 46 x 50 | yandan | yok | döndürülmedi |
| pony | 44 | 39 x 29 | yandan/3-4 | var (açık gri leke) | döndürülmedi |
| donkey | 44 | 31 x 23 | önden/3-4 karışık, küçük | var (gri leke) | döndürülmedi |
| mule | 56 | 51 x 41 | 3-4 | var (gri leke) | döndürülmedi |
| hinny | 48 | 43 x 40 | yandan | yok | döndürülmedi |

Stil denetimi ve `validate_iso` çalıştırılmadı (teslim edilecek dosya yok).

## Generation
pixflux x7 = 7. `get_balance` sonrası: 2583,9.

## Sorunlar ve öneri
- 104'teki gibi at gövdesi `direction=south` ile önden gelmiyor; v3 döndürme önden referans ister, yandan referans her yönü kaydırır.
- Öneri (tahmin): yol B yerine `create_character mode=pro` (quadruped, template horse; yandan referansı döndürebilir, 10 gen/8 yön) ya da pixflux yerine `create_image_pro_flash` ile önden çizim, sonra `mode=pro` döndürme. Kullanıcı kararı gerekir; maliyet kalemi 28'i aşabilir.
- Zemin lekesi (midilli, eşek, katır) tarife eklenen "no ground disc" ifadesine rağmen çıktı; palet zorlamasının gri tonları neden olabilir (375 notu aynı).
