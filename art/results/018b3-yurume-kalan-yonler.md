unity-import: pending

# 018b3 Yürüme seti, kalan 12 yön (sekmesiz): kısmi teslim, 7/12 yön

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = yönün durma karesi (idle PNG, `get_upload_url` ile yüklendi), yön başına tek çağrı. Ham çıktı 84x112, 9 kare/yön. İşleme 018b2 ile aynı (ayak satırı hizası, durma karesi paleti, alfa ikili, kapanışta 08 atılır).
- **Metin değişikliği (gerekçe):** 018b2'nin metni aynen gönderilince PixelLab aynı isteği 018b2'deki klibe eşleyip yeni üretim yapmadı (ücretsiz, "already queued or complete"). Bu 12 yön 018b2'de zaten ölçütü geçmemişti, aynı klibi yeniden işlemek anlamsızdı. Bu yüzden 1. deneme metni sonuna "same body height in every frame" eklenerek yeniden gönderildi. 2. denemede metin: `walking with short even steps at a calm pace, legs alternate left and right, torso and head stay at exactly the same height in every frame, no up and down motion`. Tarif "aynen" derken amaç yeni üretimdi.
- Harcanan: 12 (1. deneme) + 8 (2. deneme, yalnız 1. denemede kalan yönler) = **20 generation** (tahmin 40, tavan 55). `get_balance` sonrası: 3737,5 (diğer şeritler eşzamanlı harcıyor, bu sayı yalnızca bu işe ait değil). Defter: art/ledger/018b3.md.
- Eşzamanlılık: 1. dalgada kadın SW ve W "0 job slots" verdi, kadın SW yuva açılınca, W bir sonraki yuvada bir kez yeniden gönderildi (hız sınırı istisnası).
- Masaüstü kopyası: yerel
- **Teslim edilen: 7 yön x 8 kare = 56 PNG** (erkek N, NE, NW, S; kadın NW, S, W). Teslim edilmeyen 5 yön: erkek SW; kadın N, NE, SE, SW (iki denemede de sekme ölçütü geçmedi). 2. deneme kareleri `art/results/018b3-red/` altında (41 PNG).

## Sekme ölçütü (kafa tepesi kareler arası farkı, ≤ 3 px, kırpma yok)
Ölçüm ayak hizalamasından sonra, tuval dışına taşan satırlar dahil, kare 00-07 (08 atılan).
| Yön | Karakter | 1. deneme | 2. deneme | Sonuç |
|---|---|---|---|---|
| N | erkek | 4, 7 px kırpıldı KALDI | 2 | **GEÇTİ (2. deneme)** |
| NE | erkek | 3 | - | **GEÇTİ** |
| NW | erkek | 5 KALDI | 3 | **GEÇTİ (2. deneme)** |
| S | erkek | 3 | - | **GEÇTİ** |
| SW | erkek | 7 KALDI | 6 KALDI | teslim edilmedi |
| N | kadın | 5, 144 px kırpıldı KALDI | 3 ama 20 px kırpıldı KALDI | teslim edilmedi |
| NE | kadın | 5 KALDI | 5 KALDI | teslim edilmedi |
| NW | kadın | 4 KALDI | 3 | **GEÇTİ (2. deneme)** |
| S | kadın | 2 | - | **GEÇTİ** |
| SE | kadın | 5 KALDI | 6, 7 px kırpıldı KALDI | teslim edilmedi |
| SW | kadın | 4 KALDI | 5, 7 px kırpıldı KALDI | teslim edilmedi |
| W | kadın | 3 | - | **GEÇTİ** |

Gözlem: 018b2 ile aynı; kafa oynaması pozdan geliyor (adımda gövde alçalıyor), özellikle SW/SE/N yönlerinde. Bu yönler için pixminimax ile iki deneme yetmedi. Öneri: skeleton-v3 ya da şablon (`walking-8-frames`) ya da `direction_prompts` ile yöne özel metin, ya da bu yönlerde (SW, SE) yan görünüşün aynalanması (SW=SE aynası, kabul edilen erkek SE'den). Karar kullanıcıda.

## Dosyalar (Assets/Art/Iso/_aday/anim/, 84x84, pivot x=42)
| Yol | Kare | Pivot y (alttan) |
|---|---|---|
| iso_char_man_r84_walk2_n_00..07.png | 8 | 5 |
| iso_char_man_r84_walk2_ne_00..07.png | 8 | 4 |
| iso_char_man_r84_walk2_nw_00..07.png | 8 | 4 |
| iso_char_man_r84_walk2_s_00..07.png | 8 | 4 |
| iso_char_woman_r84_walk2_nw_00..07.png | 8 | 4 |
| iso_char_woman_r84_walk2_s_00..07.png | 8 | 2 |
| iso_char_woman_r84_walk2_w_00..07.png | 8 | 2 |

Pivot y yönün durma karesinin ayak boşluğudur (018b ile aynı kural). Var olan 4 yönün dosyalarına (erkek SE/E/W, kadın E) dokunulmadı.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (nesne) | 56/56 geçti |
| style.py check karakter | 0 KALDI, 8 UYARI ("ailesinden doygun", yalnız erkek, x1,25-1,26; kadın hepsi GEÇTİ x0,75-0,99), 48 GEÇTİ. Aile kilidi kurulmadığından palet uyumu uyarı düzeyinde; `fit` uygulanmadı |
| Tepe kırpılması (teslim edilenler) | 0 piksel |
| Kapanış (08 atıldı, IoU 08-00) | 0,990-1,000, hepsinde 08 atıldı (8 kare, 00-07) |
| Döşeme testi | uygulanmaz (zemin değil) |

## Görseller
- Yan yana (referans + kare 00 ve 04): art/results/018b3-yanyana.png
- GIF (x2, 110 ms, 8 yön yan yana: S SE E NE N NW W SW; teslim edilmeyen yönler boş): art/results/018b3-yurume-man.gif (SW boş), art/results/018b3-yurume-woman.gif (SE, NE, N, SW boş). Var olan kabul edilmiş yönler (erkek SE/E/W, kadın E) de aynı GIF'te.

## Sorunlar / öneri
- 5/12 yön yine teslim edilmedi: erkek SW, kadın N, NE, SE, SW. Yeni deneme istenirse farklı yöntem gerekir (yukarıya bak); aynı yöntemle üçüncü deneme yapılmadı (yön başına en çok 2 deneme).
- Kadın SE, SW (ve erkek SW) için yön aynalama seçeneği kullanıcı kararına bırakıldı; aynalama yapılmadı.
- Kimlik (yüz, giysi): durma karesi paleti kullanıldığı için renkler tutarlı; otomatik ölçülmedi.
- Yardımcı betikler (yükleme, indirme, işleme) `art/tmp/` altında, git yok sayar; `tools/art` değişmedi.
