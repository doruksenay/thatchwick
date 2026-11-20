unity-import: pending

# 015c Yürüme seti: çocuk erkek ve kız (64 px): kısmi teslim, 10/16 yön

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = yönün durma karesi (`get_upload_url` ile yüklendi), metin 018b2 ile aynen. Yön başına tek çağrı, 16 çağrı x 1 generation.
- Harcanan: 16 generation (tahmin 24, tavan 40). `get_balance` sonrası: 3465,9 (diğer şeritlerin eşzamanlı harcamasını da içerir). Defter: art/ledger/015c.md.
- Hız sınırı: ilk dalgada 14 çağrı "0 job slots (10/10)" verdi (başka şeritler yuvaları tutuyordu); yuva açıldıkça aynı çağrılar bir kez yeniden gönderildi.
- Yeniden deneme: kalan 6 yön için ikinci deneme yapılmadı. PixelLab aynı metinli isteği mevcut klibe eşliyor ("already queued or complete"), tarif metnin aynen kalmasını şart koşuyor; metni değiştirmek tarifi bozardı. Öneri: 015c2 ile `direction_prompts` ya da yön başına hafif farklı metin.
- Masaüstü kopyası: yerel
- Karakter kimlikleri: boy `6940a34e-...`, girl `9d58ae77-...` (ham klipler PixelLab'de, 9 kare/yön, 64x84..96).

## İşleme (ücretsiz düzeltme, 018b2 ile aynı)
1. Her kare, en alt opak satırı durma karesinin ayak satırına gelecek şekilde bütün olarak dikey kaydırıldı; yatay: yönün birleşik kutu ortası durma karesininkine. 64x64'e maskeyle kırpma (ölçekleme yok).
2. Palet: yönün durma karesinin renkleri, en yakın RGB; alfa ikili.
3. Son kare (08) 00 ile IoU > 0,93 ise atıldı (teslim edilen 10 yönde hepsinde atıldı, 8 kare 00-07).

## Sekme ölçütü (kafa tepesi kareler arası farkı, ≤ 2 px): yön x karakter
| Yön | Erkek | Kız |
|---|---|---|
| S | 1 GEÇTİ | 1 GEÇTİ |
| SE | 2 GEÇTİ | 4 KALDI |
| E | 2 GEÇTİ | 2 GEÇTİ |
| NE | 3 KALDI | 4 KALDI |
| N | 2 GEÇTİ | 1 GEÇTİ |
| NW | 3 KALDI | 1 GEÇTİ |
| W | 2 GEÇTİ | 2 GEÇTİ |
| SW | 3 KALDI | 3 KALDI (ayrıca döngü kapanmadı, IoU 0,90) |

Ayak satırı sabit (hizalama gereği, pivot y = 2 alttan). Teslim edilen yönlerde tepe kırpılması yok; erkek SE'de 6 piksel yan tuval dışına taştı (silindi).

## Dosyalar (Assets/Art/Iso/_aday/anim/, 64x64, pivot x=32, pivot y=2 alttan)
| Yol | Kare |
|---|---|
| iso_char_boy_r64_walk2_{s,se,e,n,w}_00..07.png | 5 yön x 8 |
| iso_char_girl_r64_walk2_{s,e,n,nw,w}_00..07.png | 5 yön x 8 |

Teslim edilmeyen (kareleri `art/results/015c-red/`): erkek NE, NW, SW; kız SE, NE, SW. Kodda bu yönlerde durma karesi + sallantı sürer.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (--kind object --max-colors 32) | 80/80 geçti |
| style.py check --family karakter | 80 GEÇTİ, 0 UYARI, 0 KALDI |
| Sekme | 10/16 yön geçti (yukarıdaki tablo) |

## Görseller
- Yan yana: art/results/015c-yanyana.png
- GIF (110 ms, 8 yön ızgara; kırmızı yönler durma karesi): art/results/015c-yurume-boy.gif, art/results/015c-yurume-girl.gif
- Yetişkin erkekle yan yana GIF üretilmedi (tarifte istenmişti; yetişkin 84 px kareleriyle ölçek karşılaştırması bir sonraki işte yapılabilir).

## Sorunlar
- 6/16 yön sekme ölçütünü geçemedi (kafa 3-4 px). Pixminimax poz boyundan kaynaklanıyor (018b2 ile aynı gözlem).
