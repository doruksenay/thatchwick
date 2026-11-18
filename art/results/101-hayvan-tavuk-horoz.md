unity-import: pending

# 101 Hayvanlar 1: tavuk + horoz: BLOKE (kısmi)

**Sonuç:** 3 tavuğun 8 yön döndürmesi PixelLab'te üretildi, ama bulut işçisinde PNG'leri diske indirecek araç yok (`curl` onay istiyor, MCP araçları dosya yazmıyor). Hiçbir aday `Assets/Art/Iso/_aday/` altına konamadı. Horozlar üretilmedi (kural R döndürülmüş tavuk ölçümü gerektiriyor; teslim de aynı nedenle imkansız, gen israfı önlendi).

## Çağrılar
| Çağrı | Gen |
|---|---|
| pixen 28 chicken_3 | 1 |
| pixen 28 chicken_4 | 1 |
| pixen 32 chicken_5 | 1 |
| pixen 32 chicken_6 | 1 |
| v3 döndürme chicken_3 (28 px kare, saydam dolgu ile 32'ye) | 1 |
| v3 döndürme chicken_4 (aynı) | 1 |
| v3 döndürme chicken_5 | 1 |

Toplam 7 gen. `get_balance` sonrası (diğer şeritlerin harcamasıyla karışık): 1805,8.

## Güney kontrolü (tek kare)
| Aday | Tuval | bbox | Karar |
|---|---|---|---|
| chicken_3 | 28 | x6-22, y2-26 (boy 24, doluluk 0,86) | önden, ortalı; döndürüldü |
| chicken_4 | 28 | x4-23, y2-26 (boy 24, doluluk 0,86) | önden (kanat/kuyruk sola taşıyor, gövde ortalı); döndürüldü |
| chicken_5 | 32 | x5-26, y0-31 (boy 31, doluluk 0,97) | önden ama tuvale tam dayalı (ibik ve ayak kırpılmış olabilir), duruşu insansı; döndürüldü |
| chicken_6 | 32 | x3-30, y0-32 | 3/4 dönük, gövde sağa >3 px kayık: **güney değil**, döndürülmedi |

Ham tek kareler repoya konamadı (dosya kopyalama onay istedi); PixelLab galerisinde: chicken_3 `d52d3cdb-b694-5032-be12-877c7f6e933f`, chicken_4 `8cf3a098-568b-58b2-aa0e-fa37e71f0006`, chicken_5 `2b1fc892-0863-55fb-8ad3-2f15977e047a`, chicken_6 `958013fb-991f-5e50-9213-94a3955c0efa`.

## Döndürme notları
- v3 referansı en az 32 px istiyor: 28 px tavuklar yeniden örnekleme olmadan, ortalanmış (2 px) saydam dolguyla 32x32'ye taşındı. İlk iki çağrı bu yüzden hata verdi (ücretsiz).
- Base64 gönderimi bir kez bozuldu; yeniden kodlanmış PNG ile geçti.

## Karakter kimlikleri (yerelden indirilecek)
- chicken_3: `abbef112-83dd-4be0-82ae-ddf337f2a323`
- chicken_4: `494b2ba3-ceac-46a0-ab92-f2a3236f33fb`
- chicken_5: `58abcd4a-9594-4a6a-bf27-f63645357afc`

İndirme: `https://api.pixellab.ai/mcp/characters/<id>/download` (zip; `rotations/<yön>.png`).

## Sıradaki adım
Yerel işçi kimlikleri indirir, 8 yönü ölçer ve doğrular (doluluk r, renk sayısı, zemin lekesi), `Assets/Art/Iso/_aday/iso_animal_chicken_{3,4,5}_cand_<yön>.png` olarak koyar; r ile horoz tuvalini belirleyip rooster_1-4'ü (ve isterse chicken_6'yı) üretir. Doğrulama tabloları ve yan yana sayfası bu yüzden yok.

Masaüstü kopyası: yerel.
