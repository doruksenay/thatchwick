unity-import: pending

# 015a Yürüme seti, yaşlı erkek ve kadın (84 px): kısmi teslim, 9/16 yön

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `custom_start_frame_url` ve `end_frame_url` = yönün durma karesi (elder_m için 423/423b `_v2`/`_v3`, elder_f için 415), yön başına tek çağrı (özel kare tek yön kabul ediyor). Metin 018b2 ile aynı + `, slightly slower shorter steps of an old person`.
- Harcanan: 26 generation (16 ilk deneme + 10 ikinci deneme; 1 gen/çağrı; tahmin 24, tavan 40). `get_balance` sonrası: 3455,9 (hesap geneli, paralel şeritler dahil). Defter: art/ledger/015a.md.
- Eşzamanlılık: 10/10 iş yuvası dolunca elder_f çağrıları reddedildi (ücret yok), yuva açılınca aynı çağrılar yeniden gönderildi (hız sınırı istisnası).
- İkinci denemede PixelLab aynı metni önceki klip sayıp yeniden üretmedi; metnin sonuna yalnızca bir nokta eklendi (`...of an old person.`). Başka fark yok.
- Masaüstü kopyası: yerel
- **Teslim edilen: 9 yön x 8 kare = 72 PNG.** elder_m: S, E, N, NW, W. elder_f: S, E, W, SW. Kalan 7 yön sekme ölçütünü iki denemede de geçmedi, teslim edilmedi (işlenmiş kareleri `art/results/015a-red/`, `a1_` ilk, `a2_` ikinci deneme).
- **elder_m SE:** `_v3` durma karesi (423b) kullanıldı, ama yürüme SE ölçütü geçmedi (aşağıda). Kürek hiçbir teslim edilen karede görünmüyor.

## İşleme (018b2 ile aynı, ücretsiz)
1. Her kare, en alt opak satır durma karesinin ayak satırına gelecek şekilde bütün olarak dikey kaydırıldı; yatay: yönün birleşik kutu ortası durma karesininkine. 84x84'e maskeyle kırpma (ölçekleme yok).
2. Palet: yönün durma karesinin renkleri, en yakın RGB; alfa ikili.
3. Kare 08, 00'a IoU > 0,93 ise atıldı (teslim edilenlerin hepsinde atıldı, 8 kare 00-07).

## Sekme ölçütü (kafa tepesi kareler arası farkı, ≤ 3 px, tepe kırpılmadan): yön x karakter
Her hücre: ilk deneme / ikinci deneme.
| Yön | elder_m | elder_f |
|---|---|---|
| S | 4 / **3 GEÇTİ** | **2 GEÇTİ** |
| SE | 4 / 5 KALDI | 5 / 6 KALDI |
| E | **2 GEÇTİ** | **2 GEÇTİ** |
| NE | 4 / 5 (kırpıldı) KALDI | 5 / 5 (kırpıldı) KALDI |
| N | **2 GEÇTİ** | 4 (kırpıldı) / 3 (kırpıldı) KALDI |
| NW | 5 / **2 GEÇTİ** | 4 / 7 (kırpıldı) KALDI |
| W | 4 / **3 GEÇTİ** | **1 GEÇTİ** |
| SW | 7 / 5 KALDI | **3 GEÇTİ** |

Ayak satırı sabit (hizalama gereği). 018b2'deki gözlem sürüyor: kafa oynaması poz boyundan geliyor (adımda gövde alçalıyor).

## Dosyalar (Assets/Art/Iso/_aday/anim/, 84x84, pivot x=42)
Pivot y (alttan) yönün durma karesinin ayak boşluğudur: S 4, E 3, N 5, NW 4, W 3 (elder_m); S 3, E 3, W 3, SW 2 (elder_f). İş dosyası (42, 2) yazıyor; 018b2 kuralı gibi durma karesinin ayak boşluğu esas alındı.
| Yol | Kare |
|---|---|
| iso_char_elder_m_r84_walk2_{s,e,n,nw,w}_00..07.png | 8 x 5 yön |
| iso_char_elder_f_r84_walk2_{s,e,w,sw}_00..07.png | 8 x 4 yön |

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (nesne, 32 renk) | 72/72 geçti |
| style.py check karakter | 0 KALDI, 12 UYARI (hepsi "ailesinden soluk": elder_f W x0,51-0,55; elder_m E 02, NW 02-04, W 03 x0,58-0,60), 60 GEÇTİ. Aile kilidi kurulmadığından palet uyumu uyarı düzeyinde; `fit` uygulanmadı. Soluk görünüm yaşlı paletinden (durma karesiyle aynı renkler) |
| Kürek / alet | Teslim edilen 5 elder_m yönünün hiçbirinde görünmüyor (yan yana sayfasında bakıldı; küçük ölçek, kullanıcı GIF'ten teyit etsin) |
| Tepe kırpılması (teslim edilenler) | 0 piksel |
| Kapanış (IoU 08-00) | hepsi > 0,96, 08 atıldı |

## Görseller
- Yan yana (referanslar + kare 00 ve 04): art/results/015a-yanyana.png
- GIF (110 ms): art/results/015a-yurume-elder_m.gif (5 yön), art/results/015a-yurume-elder_f.gif (4 yön), art/results/015a-yurume-karsilastirma.gif (yetişkin erkek 018b2 E, elder_m E, elder_f E: hız ve boy)

## Sorunlar / öneri
- 7/16 yön teslim edilmedi: elder_m SE, NE, SW; elder_f SE, NE, N, NW. Bunlar için kodda durma karesi + sallantı sürer (iş notu).
- Yeni deneme istenirse: skeleton-v3 ya da şablon (018b2 önerisiyle aynı), ya da `direction_prompts` ile yöne özel metin. Aynı yöntemle üçüncü deneme yapılmadı (yön başına en çok 2).
- Ham çıktı PixelLab'de: elder_m grupları 56765837-1458-4a98-9f02-ba2c2ca34e84 (ilk) ve yön başına walk2b grupları; elder_f aa7f64f6-d7e2-403b-9003-fd6a127f2335 (ilk) ve walk2b grupları.
