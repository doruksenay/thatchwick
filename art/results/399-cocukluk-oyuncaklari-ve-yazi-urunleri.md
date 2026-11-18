unity-import: pending

# 399 Çocukluk oyuncakları ve yazı/mum/malt ikonları (durum: review, kısmen teslim)

Yan yana sayfası: [399-yanyana.png](399-yanyana.png) (yalnızca oyuncaklar; ikonlar için arayuz ailesinde referans yok). Teslim edilmeyenler: `art/results/399-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
- Oyuncaklar: `create_map_object`, stil eşleme, referanssız kılavuz + maske (301 yöntemi; kılavuz `art/tmp399/g.py` ile üretildi, `footprint_guide.py` yuva/dekor modu olmadığı için), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. **Tuval 32x32 (kite 32x40):** araç alt sınırı 32 px; tarifteki 16x20, 20x12, 16x12 tuvalleri bu yüzden büyütüldü, taban elması yuva için 12-14 px, zemin için 24 px, alt kenardan 4 px yukarıda. Maske eşya yüksekliği kadar yukarı +1 px.
- İkonlar: `create_image_pixen` 32x32, `no_background`, `single color outline`, `low detail` (316 yöntemi), teslim edilenler 16 renge median-cut (dither yok). 
- 13 çağrı x 1 gen = **13 gen** (cost_est 13, cost_max 18). Bir pixen çağrısı hız sınırına (10/10) takıldı, bekleyip yeniden gönderildi (ücret yok). `get_balance` sonrası: 4364,9 (paralel şeritler dahil).

## Sonuç özeti
| # | Öğe | Durum | Not |
|---|---|---|---|
| 1 | sapan | teslim | ince, tanınır; küçük (8x18 px) |
| 2 | atlama ipi | teslim | iplik halka; sap belirgin değil |
| 3 | çember | teslim | tek çember, sopa yok (⚠) |
| 4 | oyma bıçağı | TESLİM EDİLMEDİ | bıçak yok, yalnızca iki çubuk ve talaş |
| 5 | bilye | teslim (⚠) | kese olarak okunuyor, bilyeler seçilmiyor |
| 6 | uçurtma | teslim (UYARI) | tanınır; zemin lekesi dahil (açık yeşil, küçük); doygunluk x1,40 |
| 7 | hokkabaz topları | TESLİM EDİLMEDİ | tek top, üç değil |
| 8 | icon_tallow | TESLİM EDİLMEDİ | kasede kırmızı göz benzeri benekli toplar, iç yağı olarak tanınmıyor |
| 9 | icon_parchment | teslim | |
| 10 | icon_oak_gall | TESLİM EDİLMEDİ | kuş yüzü ve gözlü toplar, "no eyes" çiğnendi |
| 11 | icon_ink | TESLİM EDİLMEDİ | hokka yerine insan figürü çıktı |
| 12 | icon_blank_book | teslim (⚠) | kalın kitap/sandık gibi, üstü beyaz sayfa; cilt doku koyu mavi, renk uyumu zayıf |
| 13 | icon_malt | TESLİM EDİLMEDİ | renkli (mor/yeşil) kaotik, tane yığını değil |

## Dosyalar (pivot x, pivot y ALTTAN)
| Yol | Piksel | Pivot x | Pivot y |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_item_slingshot_se.png | 32x32 | 16 | 7 (taban elması merkezi) |
| Assets/Art/Iso/_aday/iso_item_jump_rope_se.png | 32x32 | 16 | 7 |
| Assets/Art/Iso/_aday/iso_item_hoop_se.png | 32x32 | 16 | 10 |
| Assets/Art/Iso/_aday/iso_item_marbles_se.png | 32x32 | 16 | 7 |
| Assets/Art/Iso/_aday/iso_item_kite_se.png | 32x40 | 16 | 10 |
| Assets/Art/Iso/_aday/icon_parchment.png | 32x32 | 16 | 0 (ikon) |
| Assets/Art/Iso/_aday/icon_blank_book.png | 32x32 | 16 | 0 (ikon) |

Not: iş dosyası ikonları `Assets/Art/Icons/` altına yazmayı söylüyordu; işçi yolları dışında olduğundan (316'daki gibi) `_aday/` altına kondu.

## Doğrulama (validate_iso, `--kind object`, `--max-colors 48`)
| Dosya | Opak sınır | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| sapan | x12-19, y8-25 | 8 | 0 | geçti |
| ip | x8-23, y12-27 | 9 | 0 | geçti |
| çember | x4-27, y4-27 | 9 | 0 | geçti |
| bilye | x11-21, y13-24 | 5 | 0 | geçti (alt boşluk 7 satır) |
| uçurtma | x6-25, y3-36 | 15 | 0 | geçti |
| bıçak (teslim değil) | x8-23, y17-26 | 12 | 0 | geçti, ama bıçak yok |

## Stil denetimi (`style.py check`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| sapan | %100 | 4.5 | x0.71 | GEÇTİ |
| ip | %100 | 4.7 | x0.91 | GEÇTİ |
| çember | %100 | 6.0 | x0.95 | GEÇTİ |
| bıçak | %100 | 4.9 | x0.74 | GEÇTİ |
| bilye | %100 | 6.6 | x0.80 | GEÇTİ |
| uçurtma | %95 | 10.7 | x1.40 | UYARI (palete uzak, doygun) |
| icon_parchment | %94 | 12.1 | - | UYARI (arayuz kilidi yok) |
| icon_blank_book | %91 | 10.9 | - | UYARI (arayuz kilidi yok) |

Döşeme testi gerekmedi (zemin parçası yok). Gölge pişirilmemiş; ışık yönü bu boyutta ölçülemedi.

## Sorunlar
- Oyuncaklar çok küçük ve ince çizgili (tuval alt sınırı 32); asıl ölçek/yuva uyumu için kullanıcı bakmalı.
- 6 görsel teslim edilmedi (bıçak, hokkabaz topları, tallow, oak_gall, ink, malt); tek deneme kuralı gereği yeniden denenmedi. Yeniden üretim için öneri: ikonlarda "no face, no eyes, no creature" ve daha sade tarif; pixen yerine ikonları `create_map_object` 32x32 ya da farklı araçla denemek.
- Dönem kuralı: bilye kil, uçurtma keten tarifine uyuldu; uçurtma bezi kırmızı değil, krem.
