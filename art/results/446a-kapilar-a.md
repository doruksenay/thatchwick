unity-import: pending

# 446a Kapı varyantları A (bulut işçisi, şerit 2, 2026-11-19)

Yan yana sayfası: [446a-yanyana.png](446a-yanyana.png) (soldaki 3 referans; sonra red sw_ne, red se_nw, blue sw_ne, blue se_nw, green sw_ne, green se_nw)

## Araç ve ayarlar
- 329 ile aynı yöntem: `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`; 6 çağrı x 5 gen = **30 generation** (araç açıklaması: 96px'e kadar 5, cost_est ile uyumlu).
- Kaynak: `iso_wall_door_sw_ne/se_nw.png`, 96x96 kırpma (ofset sw_ne (0,4), se_nw (6,4)); maske yalnızca kapı kanadı (sw_ne x44 y41 17x39, se_nw x36 y41 17x39). Çıktı aynı ofsetle 102x115'e geri konuldu; maske dışı değişen piksel 0.
- İlk üç çağrı base64 çözme hatasıyla reddedildi (ücret yok); paletli yeniden kodlamayla yeniden gönderildi (329'daki gibi). Hız sınırı hatası olmadı.
- `get_balance` sonrası kalan: **3282.9**. Masaüstü kopyası: yerel.

## Teslim edilenler (102x115)
| Dosya | Pivot (x, y ALTTAN) |
|---|---|
| iso_wall_door_red_sw_ne.png | 32, 19 |
| iso_wall_door_red_se_nw.png | 70, 19 |

## Teslim edilmeyenler (`art/results/446a-red/`)
blue ve green (4 dosya): kapı kalas kapıya değil, tahtasız düz boyalı bayrak/pankart gibi çıktı (V biçimli alt kenar, kalas ve kasa izi yok); ayrıca stil denetimi KALDI. Tek deneme kuralı gereği yeniden denenmedi.

## Doğrulama
`validate_iso.py`: tüm dosyalar geçti (kırmızılar 46 renk, mavi se_nw 32).

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| red_sw_ne | %97 | 5.8 | x1.12 | GEÇTİ |
| red_se_nw | %100 | 5.8 | x1.26 | UYARI (ailesinden doygun) |
| blue_sw_ne | %73 | 12.8 | x1.02 | KALDI (palet dışı) |
| blue_se_nw | %69 | 12.9 | x1.23 | KALDI (palet dışı) |
| green_sw_ne | %93 | 7.6 | x0.97 | GEÇTİ (ama şekil bayrak, teslim edilmedi) |
| green_se_nw | %77 | 6.9 | x0.98 | KALDI (palet dışı) |

## Sorunlar / öneri
- Mavi ve yeşil için ayrı iş gerekir; tarifte "plank door" kalas dokusunu vurgulayan açıklama (ör. `vertical wooden planks with visible plank gaps, painted indigo blue`) ve kuyrukta bir kaynak ayrımı denenebilir. Palet dışı boya rengi stil kilidiyle doğal çakışır; kullanıcı kararı gerekebilir.
- Dikişsiz yan yana ve ışık yönü gözle denetlenmedi; kullanıcı sayfaya baksın.
