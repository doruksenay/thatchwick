unity-import: pending

# 018a3 Yürüme adayları, erkek 84 px SE (kullanıcı seçer)

- Karakter: man_r84 (c63fe39d-b168-4732-bf11-b476172c98e8), yalnızca SE. Dört aday, hepsi tek denemeyle ve aynı anda gönderildi.
- Harcanan: 6 generation (a: 1, b: 1, c: 3, d: 1; tahmin 8, üst sınır 20). `get_balance` sonrası: 4088,7 (başka şeritler de harcıyor, sayı yalnızca bu işe ait değil). Defter: art/ledger/018a3.md.
- Teslim: ölçütten bağımsız bütün adaylar `Assets/Art/Iso/_aday/anim/` altında (31 PNG). Seçim kullanıcının.
- masaüstü kopyası: yerel

## Adaylar
| Harf | Yöntem | Grup | Kare | Ham boyut | Not |
|---|---|---|---|---|---|
| a | `animate_character` mode=template `walking-8-frames` | 722f3381 | 8 | 84x84 | |
| b | mode=template `walking-6-frames` | 4cd44fcc | 6 | 84x84 | 6 kare tam çift adım DEĞİL (aşağıya bak) |
| c | mode=skeleton-v3, `walking-8-frames` (013'teki `skel` yöntemi) | 8687de3d | 8 | 112x112 | |
| d | mode=pixminimax, `frame_count=8`, tarifteki metin | 223e6402 | 9 (0 = başlangıç) | 112x112 | |

Hepsi kullanılabilirdi, atlanan şablon yok.

## İşleme
- c ve d 112x112 geldi; ofsetli kırpma ile 84x84 (c: x16, y15; d: x15, y15). Kırpılan opak piksel yok, yeniden boyama ve ölçekleme yok. a ve b zaten 84x84.
- Bütün kare dizisi aynı ofsetle taşındı (yere basan ayak aynı satırda; ayak satırı kuralı bu işte yok).
- Doygunluk: ham kareler `style.py check` ile "ailesinden çok doygun" (x1,54-1,74) yüzünden KALDI (aile kilidi kurulmadı). Şekil sorunu olmadığı için `style.py fit --family karakter` uygulandı; 31/31 GEÇTİ. d_08 x0,69 (hedef 0,40). Ham kareler: art/results/018a3-red/.
- Pivot x=42, y=2 (ALTTAN); alt boşluk 3 satır (a/b/c kare 00).

## Doğrulama
| Aday | Kare | Kafa oynaması (tepe y) | Alt satır (kare kare) | Karar |
|---|---|---|---|---|
| a | 8 | 0 px | 80,77,76,76,80,81,81,80 | kafa geçti |
| b | 6 | 0 px | 80,75,75,80,80,80 | kafa geçti |
| c | 8 | 0 px (tepe y=3) | 80,77,76,77,80,81,81,81 | kafa geçti |
| d | 9 | 1 px | 80,78,77,77,76,77,79,82,83 | kafa geçti |

Kafa ≤ 3 px ölçütünü dördü de geçti; "zıplıyor" işareti yok.

### Adım simetrisi (kontak sayfası: art/results/018a3-adaylar.png, satırlar a-d)
Ölçüm kaba: opak bbox genişliği ve en alçak ayak satırı. Kesin karar GIF'e bakılarak verilmeli.
- **a:** ayak kalkması kare 1-3 (alt 77,76,76), geniş adım kare 5-6 (bbox x 22-68). İki adım birbirine benziyor ama ilk adımda arka ayak kalkıyor (3 kare), ikincisinde ön ayak öne uzanıyor (2 kare). Kare sayısı 3 ve 2: **kısmen eşit değil, hafif topallama riski**.
- **b:** 6 karede tek kalkma (1-2) ve tek geniş adım (kare 4): çift adımın yalnızca bir yarısı gibi görünüyor, **topallıyor** diye işaretlendi (018a2'nin sorunu aynı şablonun 6 karelik hâli).
- **c:** a'ya çok benzer (alt 77,76,77 / 80,81,81,81); bbox x 19-68, ön ayak uzanımı en geniş. Ayak ucu x farkı sol-sağ için kare 1-3 ≈ 61, kare 5-6 ≈ 67-68: adım boyu farkı ~6-7 px, **hafif eşitsiz**.
- **d:** alt satır 76'dan 83'e iniyor, son kareler (07, 08) yere 2-3 px gömülü (82, 83) ve karakter ileri kayıyor; döngü kapanmıyor (kare 00 ile 08 farklı poz). **topallıyor ve döngü kapanmıyor** olarak işaretlendi. Kimlik (yüz, giysi) a-d arasında korunmuş görünüyor; d'de kese ve kemer biraz değişmiş.

Bu işaretler otomatik bir ölçü değil, bbox ve kontak sayfasından okunan gözlemdir.

## Dosyalar (hepsi `Assets/Art/Iso/_aday/anim/`, 84x84, pivot x=42 y=2 alttan)
| Yol | Kare |
|---|---|
| iso_char_man_r84_walk3a_se_00..07.png | 8 |
| iso_char_man_r84_walk3b_se_00..05.png | 6 |
| iso_char_man_r84_walk3c_se_00..07.png | 8 |
| iso_char_man_r84_walk3d_se_00..08.png | 9 |

## Görseller
- GIF (x3, 110 ms): art/results/018a3-walk3a.gif, 018a3-walk3b.gif, 018a3-walk3c.gif, 018a3-walk3d.gif
- Referans GIF: art/results/018a-yurume.gif (018a, zıplıyor). 018a2 için GIF üretilmedi, ham şablon kareleri art/results/018a2-red/.
- Bütün adaylar tek sayfada (her satır bir aday): art/results/018a3-adaylar.png
- Aile yan yana sayfası (kare 00'lar, referanslarla): art/results/018a3-yanyana.png

## Stil denetimi (fit sonrası)
31/31 GEÇTİ, palete yakın %100, UYARI yok. Aile kilidi kurulmadığı için palet uyumu yalnızca uyarı düzeyindedir. validate_iso örnek karelerde geçti (opak renk sayısı fit öncesi 65).

## Sorunlar
- Renk sayısı 32'nin üstünde (65). 018a'daki gibi durma karesinin 32 rengine eşleme yapılmadı; seçilen aday için 018b2 öncesi yapılmalı (kullanıcı seçince).
- Adım simetrisi gözlemleri yukarıda; GIF'lerle onay kullanıcıda.
- Öneri: 8 karelik şablon (a) ve skeleton (c) arasında seçim; b 6 karede çift adım vermiyor, d döngüyü kapatmıyor.
