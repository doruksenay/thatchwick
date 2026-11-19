unity-import: pending

# 018b2 Yürüme seti yeniden (sekmesiz): kısmi teslim, 4/16 yön

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = yönün durma karesi (idle PNG, `get_upload_url` ile yüklendi), metin 018a3 aday d (aynen), yön başına tek çağrı, tek deneme. Ham çıktı 84x116, 9 kare/yön.
- Harcanan: 16 generation (16 çağrı x 1; tahmin 40, tavan 60). `get_balance` sonrası: 4025,7. Defter: art/ledger/018b2.md.
- Eşzamanlılık: ilk dalgada 6 kadın çağrısı "0 job slots" verdi; ~45 sn sonra yuva açılınca aynı çağrılar bir kez yeniden gönderildi (hız sınırı istisnası).
- Masaüstü kopyası: yerel
- **Teslim edilen: 4 yön x 8 kare = 32 PNG** (erkek SE, E, W; kadın E). Kalan 12 yön sekme ölçütünü geçmediği için teslim edilmedi (işlenmiş kareleri `art/results/018b2-red/`).

## İşleme (ücretsiz düzeltme)
1. Her kare, en alt opak satırı (yere basan ayak) durma karesinin ayak satırına gelecek şekilde bütün olarak dikey kaydırıldı; yatay: yönün birleşik kutu ortası durma karesininkine. 84x84'e maskeyle kırpma (ölçekleme yok).
2. Palet: yönün durma karesinin renkleri, en yakın RGB; alfa ikili.
3. Son kare (08) 00'la neredeyse aynı poz (IoU > 0,93) ise atıldı; teslim edilen 4 yönde hepsinde atıldı (8 kare, 00-07).

## Sekme ölçütü (kafa tepesi kareler arası farkı, ≤ 3 px, kırpma yok): yön x karakter
Ölçüm ayak hizalamasından sonra, tuval dışına taşan satırlar dahil (gerçek kafa tepesi, kare 00-08).
| Yön | Erkek | Kadın |
|---|---|---|
| S | 4 (kırpıldı) KALDI | 4 (kırpıldı) KALDI |
| SE | **3 GEÇTİ** | 7 KALDI |
| E | **1 GEÇTİ** | **2 GEÇTİ** |
| NE | 6 (kırpıldı) KALDI | 5 (kırpıldı) KALDI |
| N | 4 (kırpıldı) KALDI | 6 (kırpıldı, 222 px) KALDI |
| NW | 6 KALDI | 5 KALDI |
| W | **2 GEÇTİ** | 4 KALDI |
| SW | 7 KALDI | 3 ama kafa tepesi 14 px kırpıldı, KALDI |

Ayak satırı sabit (hizalama gereği). Gözlem: pixminimax ham karelerde yürürken poz yüksekliği 4-7 px değişiyor (adımda bacaklar açılıp gövde alçalıyor, kafa tepesi 0'dan -4'e kadar yükseliyor); ayak hizalaması bunu gidermiyor, sekme dikey kaymadan değil poz boyundan geliyor. S ve N yönlerinde en kötü.

## Dosyalar (Assets/Art/Iso/_aday/anim/, 84x84, pivot x=42)
| Yol | Kare | Pivot y (alttan) |
|---|---|---|
| iso_char_man_r84_walk2_se_00..07.png | 8 | 2 |
| iso_char_man_r84_walk2_e_00..07.png | 8 | 3 |
| iso_char_man_r84_walk2_w_00..07.png | 8 | 3 |
| iso_char_woman_r84_walk2_e_00..07.png | 8 | 2 |

Pivot y yönün durma karesinin ayak boşluğudur (018b ile aynı kural).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (nesne) | 32/32 geçti |
| style.py check karakter | 0 KALDI, 13 UYARI ("ailesinden doygun", erkek x1,25-1,33; kadın hepsi GEÇTİ x0,82-0,91), 19 GEÇTİ. Aile kilidi kurulmadığından palet uyumu uyarı düzeyinde; `fit` uygulanmadı (UYARI teslim edilir) |
| Tepe kırpılması (teslim edilenler) | 0 piksel |
| Kapanış (08 atıldı, IoU 08-00) | erkek SE 0,999, E 0,965, W 0,999; kadın E 0,999 |

## Görseller
- Yan yana (durma referansları + kare 00 ve 04): art/results/018b2-yanyana.png
- GIF (x2, 110 ms): art/results/018b2-yurume-man.gif (SE, E, W), art/results/018b2-yurume-woman.gif (E)

## Sorunlar / öneri
- 12/16 yön teslim edilmedi. 018b'deki zıplama kısmen sürüyor: kafa oynaması pozdan geliyor. Yeni deneme istenirse: skeleton-v3 ya da şablon (018a3 a/c adayları kafa 0 px ölçtü, topallama riski vardı) ya da `direction_prompts` ile yöne özel metin. Aynı yöntemle yeniden çağrı yapılmadı (tek deneme).
- Reddedilen yönlerin işlenmiş kareleri `art/results/018b2-red/` içinde; ham kareler yalnız PixelLab'de (işin grup kimlikleri tool çıktısında).
- Kimlik (yüz, giysi): durma karesi paleti kullanıldığı için renkler tutarlı; otomatik ölçülmedi.
