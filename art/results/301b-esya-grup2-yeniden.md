unity-import: pending

# 301b Ev eşyaları grup 2, yeniden (rapor)

Yan yana sayfası: [301b-yanyana.png](301b-yanyana.png). Önce/sonra: [301b-once-sonra.png](301b-once-sonra.png) (4 kat; soldan: 301 raf, 301 tezgâh, yeni raf deneme 1, deneme 2, yeni tezgâh). Teslim edilmeyen: `art/results/301b-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`; bardak için 32x32 dekor kılavuzu, taban 12x6 elmas alt kenardan 4 px yukarıda, maske 12 px yukarı + 1 px), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 6 çağrı x 1 gen = 6 generation (raf ve bardak ikinci deneme; kazan ve tezgâh ilk denemede tuttu). `get_balance` iş sonu: 836,1 (paralel şeritler de harcıyor; bu işin payı 6).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | raf | TESLİM EDİLMEDİ | deneme 1: doygunluk x0.40 (⚠ soluk, ölçüt 0.6-1.3 dışı), ton 6° (masa 16°); deneme 2: ton 348° (mor-pembe kayma, ölçüt ±8° dışı), doygunluk x0.74. İki denemede de raf yerine kapalı gövdeli dolap/sandık gibi, iki tahta aralığı net değil |
| 2 | pazar tezgâhı | teslim | tente bej (ort. ton 27°, açık piksellerde kırmızı/turuncu %3,7), ahşap ton 14° (masa 16°), doygunluk x0.87 |
| 3 | kazan | teslim | siyah demir kazan, üç ayak, açık boş koyu iç, ahşap yok |
| 4 | bardak | teslim (2. deneme, 2 px kaydırma) | |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_market_stall_2x1_se_nw_v2.png | 112x136 | 56 | 32 |
| iso_item_cooking_pot_se_v2.png | 80x68 | 40 | 24 |
| iso_item_clay_cup_se.png | 32x32 | 16 | 4 (küçük taban elmasının merkezi: x16, y alttan ~7) |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Maske dışı | Kılavuz kalıntısı | Sonuç |
|---|---|---|---|---|---|---|---|
| tezgâh | x9-102, y3-127 | 13 | 0 | yok | 0 | 0 | geçti |
| kazan | x19-60, y14-51 | 8 | 0 | yok | 0 | 0 | geçti (ayak izi kutusu 8-71 x 28-59; kazan 42 px geniş, taban kutusunun ~%65'i, dar tabanlı) |
| bardak (kaydırma sonrası) | x9-22, y11-25 | 5 | 0 | yok | 0 | 0 | geçti |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| tezgâh | %100 | 9.1 | x0.87 | GEÇTİ |
| kazan | %100 | 7.5 | x0.84 | GEÇTİ |
| bardak | %100 | 7.5 | x0.80 | GEÇTİ (ilk deneme x1.89 KALDI idi, 2. deneme geçti) |
| raf d1 (teslim değil) | %100 | 5.6 | x0.40 | UYARI |
| raf d2 (teslim değil) | %100 | 10.7 | x0.74 | UYARI (palete uzak) |

## Ek ölçümler
- Kazan: içi koyu (ort. parlaklık 9), boş; üç ayak görünüyor; ahşap yok. Sol/sağ parlaklık 22/24 (çok koyu, ışık yönü ayrışmıyor).
- Tezgâh: sol/sağ yüz 60/59 (ayrışmıyor, ⚠ düz).
- Bardak: ilk deneme en alt opak satır y=23 (taban elmasının alt yarısı y 25-27), ikinci deneme de y=23. **Yalnızca dikey 2 piksel aşağı kaydırıldı** (piksel taşıma): en alt satır y=25. Maske dışı taşma 0.
- Raf yüzey yuvası ölçümü yapılmadı (teslim edilmedi).

## Sorunlar
- Raf iki denemede de tutmadı (tarifteki "raf olarak okunmuyorsa teslim etme" ve ton/doygunluk ölçütleri). Yeni tarif ya da farklı yaklaşım gerekir (ör. açık iskelet raf, ton ipucu "warm orange-brown oak"); kullanıcı kararı.
- `seed` yok; ikinci denemeler yeniden gönderim.
