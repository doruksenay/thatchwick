unity-import: pending

# 018b4 Yürüme seti, son 5 yön (sekmesiz): 5/5 yön teslim

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = yönün durma karesi (idle PNG, yüklendi), yön başına tek çağrı. `action_description`: `walking slowly with small steps, upper body and head stay perfectly level, no bouncing, feet stay close to the ground` (işin tarifi, aynen). Ham çıktı 84x112, 9 kare/yön. İşleme 018b2/018b3 ile aynı (ayak satırı hizası, durma karesi paleti, alfa ikili, kapanışta 08 atıldı).
- Kimlikler: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84).
- Harcanan: 5 (ilk dalga) + 1 (kadın NE 2. deneme) = **6 generation** (tahmin 20, tavan 35). `get_balance` sonrası: 3559,5 (diğer şeritler eşzamanlı harcıyor). Defter: art/ledger/018b4.md.
- Kadın NE 2. deneme metni: aynı metne ", body stays inside the frame" eklendi (aynı metin PixelLab'de eski klibe eşlenip yeni üretim yapmayabilir, 018b3 deneyimi).
- Masaüstü kopyası: yerel
- **Teslim edilen: 5 yön x 8 kare = 40 PNG** (erkek SW; kadın N, NE, SE, SW). Var olan yönlere dokunulmadı. Önceki yöntemden (018b3) farkı yalnız `action_description`; sekmeyi bastıran metin 5/5 yönde ölçütü geçti.

## Sekme ölçütü (kafa tepesi kareler arası farkı, ≤ 3 px, kırpma yok)
Ölçüm ayak hizalamasından sonra, tuval dışı satırlar dahil, kare 00-08.
| Yön | Karakter | 1. deneme | 2. deneme | Sonuç |
|---|---|---|---|---|
| SW | erkek | 0 px, kırpma 0 | - | **GEÇTİ** |
| N | kadın | 1 px, kırpma 0 | - | **GEÇTİ** |
| NE | kadın | 3 px ama 5 px kırpıldı KALDI | 0 px, kırpma 0 | **GEÇTİ (2. deneme)** |
| SE | kadın | 0 px, kırpma 0 | - | **GEÇTİ** |
| SW | kadın | 0 px, kırpma 0 | - | **GEÇTİ** |

Hareket gerçek: kare 00'a göre maske farkı SW erkek en çok 165 px, kadın N 192, NE 604, SE 231, SW 97; yani bacak adımı var, kare donuk değil. Reddedilen kadın NE 1. deneme karelerinin ham hâli yalnız PixelLab'de (group c5f58755-249b-40d4-bb57-2d4a3f7803ad); `018b4-red/` gerekmedi.

## Dosyalar (Assets/Art/Iso/_aday/anim/, 84x84, pivot x=42)
| Yol | Kare | Pivot y (alttan) |
|---|---|---|
| iso_char_man_r84_walk2_sw_00..07.png | 8 | 2 |
| iso_char_woman_r84_walk2_n_00..07.png | 8 | 5 |
| iso_char_woman_r84_walk2_ne_00..07.png | 8 | 4 |
| iso_char_woman_r84_walk2_se_00..07.png | 8 | 2 |
| iso_char_woman_r84_walk2_sw_00..07.png | 8 | 2 |

Pivot y yönün durma karesinin ayak boşluğudur (018b ile aynı kural).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (nesne) | 40/40 geçti |
| style.py check karakter | 0 KALDI, 7 UYARI (erkek SW kare 01-07 "ailesinden doygun", x1,36-1,37; kare 00 GEÇTİ), 33 GEÇTİ (kadın hepsi, x0,79-1,03). Aile kilidi kurulmadığından palet uyumu uyarı düzeyinde; `fit` uygulanmadı (UYARI teslim edilir; erkek karakterde önceki yönlerde de x1,25-1,33 görüldü) |
| Tepe kırpılması | 0 piksel (hepsi) |
| Kapanış (08 atıldı, IoU 08-00) | 0,999-1,000, hepsinde 08 atıldı (8 kare, 00-07) |
| Döşeme testi | uygulanmaz (zemin değil) |

## Görseller
- Yan yana (referans + kare 00 ve 04): art/results/018b4-yanyana.png
- GIF (x2, 110 ms, 8 yön yan yana: S SE E NE N NW W SW; kabul edilmiş yönlerle birlikte, boş yön yok): art/results/018b4-yurume-man.gif, art/results/018b4-yurume-woman.gif. Yürüme seti artık erkek ve kadın için 8/8 yönde tamam.

## Sorunlar / öneri
- Erkek SW doygunluk uyarısı (x1,36): önceki erkek yönlerin düzeyine yakın, kimlik tutarlı. İstenirse `style.py fit` ile indirilir.
- Yardımcı betikler (yükleme, indirme, işleme) `art/tmp/` altında, git yok sayar; `tools/art` değişmedi.
