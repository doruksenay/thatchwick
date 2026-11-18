unity-import: pending

# 105 Hayvanlar 5: domuz + keçi

**Sonuç:** 8 aday üretildi (16 gen). Stil denetiminden yalnızca **pig_2** (8/8 yön) geçti ve `Assets/Art/Iso/_aday/` altına teslim edildi. Öteki 7 aday KALDI; kural gereği teslim edilmedi, ham halleri `art/results/105-ham/` altında duruyor (kullanıcı görsel karar için bakabilir). Masaüstü kopyası: yerel (bulut işçisi).

Yan yana sayfası: `art/results/105-yanyana.png` (aile referansı + 8 adayın SE yönü).

## Çağrılar
| Çağrı | Ayar | Gen |
|---|---|---|
| create_image_pixflux x8 | 46x46, no_background, south, high top-down, black outline, medium detail, basic shading, `color_image_url` = K2 köylüsü (upload) | 8 |
| create_character v3 ref x8 | n_directions=8, quadruped, template bear (domuz) / dog (keçi), referans = pixflux çıktısı, size verilmedi (çıktı 48x48) | 8 |
| reduce_colors | gerekmedi (tüm yönler ≤32 renk) | 0 |

Hız sınırı: v3 çağrılarında 429 (8 eşzamanlı iş, Tier 1) pig_2 ve pig_3 için geldi; slot açılınca her biri yeniden gönderildi (pig_3 için ikinci deneme de 429 aldı, ardından bir slot açılınca üçüncü gönderim kabul edildi; reddedilen çağrılar ücretlenmedi). Başka şeritler aynı anda çalıştığı için `get_balance` ortak: 1761,8.

## Stil denetimi (SE; 8 yön için sonuç aynı yönde)
| Aday | Sonuç | Bulgu |
|---|---|---|
| pig_1 | KALDI (8/8) | palete uzak, ailesinden çok doygun (x1,6-2,0) |
| pig_2 | **GEÇTİ (8/8)** | doygunluk x1,13-1,19 |
| pig_3 | KALDI (8/8) | palete uzak, çok doygun (x1,9) |
| pig_4 | KALDI (8/8) | palete uzak, çok doygun |
| goat_1 | KALDI (8/8) | çok doygun (x1,7-2,0) |
| goat_2 | KALDI (8/8) | çok doygun |
| goat_3 | KALDI (7/8; yalnız s geçti) | palet dışı renkler; ayrıca **zemin lekesi** (ayak altı kahverengi toprak) |
| goat_4 | KALDI (8/8) | çok doygun |

## Ölçümler (SE; hedef boy 36)
| Aday | Boyut | SE boy | SE uzunluk | Sapma | Doluluk (boy/48) | Renk (ham=teslim) | Pivot x / y (alttan) |
|---|---|---|---|---|---|---|---|
| pig_1 | 48x48 | 39 | 33 | +8% | 0,81 | 31 | 24,4 / 4 |
| pig_2 | 48x48 | 36 | 34 | 0% | 0,75 | 30 | 23,7 / 6 |
| pig_3 | 48x48 | 40 | 36 | +11% | 0,83 | 29 | 21,9 / 5 |
| pig_4 | 48x48 | 38 | 30 | +6% | 0,79 | 32 | 24,9 / 5 |
| goat_1 | 48x48 | 25 | 19 | -31% | 0,52 | 15 | 22,8 / 11 |
| goat_2 | 48x48 | 39 | 22 | +8% | 0,81 | 30 | 23,8 / 4 |
| goat_3 | 48x48 | 42 | 29 | +17% | 0,88 | 31 | 24,1 / 3 |
| goat_4 | 48x48 | 40 | 28 | +11% | 0,83 | 29 | 24,5 / 4 |

Tüm yönlerde yarı saydam piksel 0, renk ≤32 (azaltma gerekmedi, "azaltma bozdu" yok). Pivot y yön oynaması: pig_2 6-7, pig_1 4, pig_3 4-5, pig_4 5, goat_1 11, goat_2 4, goat_3 2-3, goat_4 4-5.

## Teslim edilen dosyalar (pig_2)
`iso_animal_pig_2_cand_{se,sw,nw,ne,s,w,n,e}.png`, hepsi 48x48; SE pivot x 23,7, pivot y alttan 6 (s, w, e yönlerinde 7). `validate_iso.py --kind object`: SE ve N geçti (alt boşluk 6, 30 / 17 renk). Gölge pişirilmemiş, arka plan saydam, ışık sol-üstten.

## Sorunlar
- **pig_2 "beyaz kuşak"**: tarifteki omuz kuşağı beyaz bant olarak değil, ince mavimsi bir çizgi (kolye gibi) çıktı. Palet zorlaması beyazı vermedi. Kullanıcı bunu bilerek seçmeli.
- Yön ve zemin: yalnızca SE yan yana sayfasına ve ölçümlere baktım; 8 yönü tek tek göz kontrolünden geçirmedim. Güney kontrolünde (tek kare) hepsi önden bakıyordu, gövde kayması gözle ≤3 px. Zemin lekesi yalnız goat_3'te görüldü.
- Bear şablonu domuzu ayıya çevirmedi (kulak sivri-küçük, burun uzun, kuyruk görünmüyor); dog şablonu keçide dik ve dar gövde verdi (uzunluk boydan çok kısa), keçi için koyun gibi yatay gövde çıkmadı.
- goat_1 hedeften çok küçük (25 px, -31%): pixflux tuvalde hayvanı küçük çizdi.
- Domuz için tuval 46 yaklaşık doğru (pig_2 boy 36); keçi için tuval önerisi (tahmin) 46 kalsın ama palet zorlaması aşırı doygun sonuç verdi, stil kapısını geçmek için referans olarak koyun/aile görseli denenebilir.
- Zemin döşeme testi: zemin işi değil, uygulanmadı.
