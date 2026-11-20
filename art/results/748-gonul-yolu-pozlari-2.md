unity-import: pending

# 748 Gönül yolu pozları 2 (84 px, erkek ve kadın)

- Karakterler: man_r84 `c63fe39d-b168-4732-bf11-b476172c98e8`, woman_r84 `135cb0b1-bd46-4514-8c87-0834b96ef80d`. Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = karakterin aynı yöndeki rotasyon görseli (durma karesi; `get_character` https adresi, yükleme gerekmedi). 6 anahtar x 2 karakter x 4 yön = 48 çağrı, çağrı başına 1 gen, **toplam 48 gen** (tahmin 48, sapma yok). Aynalama yok.
- Hız sınırı: PixelLab 10 iş yuvası şeritler arasında paylaşılıyordu; "need 1 job slots" hatası alan çağrı, yuva boşalınca aynen yeniden gönderildi (hatalı çağrı iş başlatmadı, harcama yok).
- `get_balance` sonrası: 448,6 (diğer şeritler de harcıyor, sayı yalnızca bu işe ait değil).
- İşleme (018c kuralları): ham kare 84x112 (bazı kliplerde 92-120 geniş, 116-152 yüksek). Yalnızca kaydırma: ham tuval 84 genişliğe ortalanır, en alt opak satır y=81'e çekilir; ölçekleme yok. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` 32 rengi, en yakın RGB, alfa ikili. Kare 0 ile 8 IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 kare. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: `art/results/748-yanyana.png` (2 referans + 22 yeni, SE kare 00 ve 03). GIF: `art/results/748-<man|woman>-<anahtar>.gif` (SE, SW, NW, NE yan yana, 2x).
- masaüstü kopyası: yerel

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam 357 PNG, 44 klip.

| Anahtar | Erkek (yön x kare) | Kadın (yön x kare) |
|---|---|---|
| put_ring | se 8, sw 8, nw 9, ne 8 | se 8, sw 8, nw 9, ne 8 |
| cut_ribbon | 4 x 8 | 4 x 8 |
| henna | 4 x 8 | 4 x 8 |
| halay | 4 x 8 | se 8, nw 8, ne 9 (sw YOK) |
| procession | 4 x 8 | 4 x 8 |
| throw_jar | se 8, nw 9, ne 8 (sw YOK) | nw 8, ne 9 (se, sw YOK) |

## Teslim edilmeyenler (84x84'e sığmadı; içerik kırpılmadı)
4 klip: erkek throw_jar SW (ham 116x152, 10 px taşma), kadın throw_jar SE (116x140, 7 px), kadın throw_jar SW (1 px), kadın halay SW (1 px). Kaydırılmış (kırpılmış) hâlleri `art/results/748-red/` altında. Çözüm önerisi: bu 4 yön için tek yönlü yeniden üretim ya da 96x96 tuval kararı (018c'deki chop sorunuyla aynı). Tek deneme kuralı gereği yeniden üretilmedi.

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (357 dosya) | 357/357 geçti |
| style.py check karakter | KALDI 0, UYARI 108, GEÇTİ 249 (tam çıktı: yan yana sayfası; UYARI yalnızca doygunluk/palet ölçüsü, aile kilidi kurulmadı) |
| Ayak satırı | her karede en alt opak satır y=81 |
| Kare 0 - kare 8 IoU | 0,93-1,0 (9 kareli: put_ring nw 0,943/0,934, throw_jar erkek nw 0,941, kadın ne 0,879, halay kadın ne 0,925); döngü kapanışı bu yönlerde gözle bakılmalı |
| Tepe kırpılması | teslim edilenlerde yok |

## Sorunlar ve notlar
- Kimlik, elde tutulan nesne (yüzük, makas+kurdele, kına kasesi, testi) okunurluğu ve `halay`'da kolların omuz hizasında yan komşuya uzanması yalnızca GIF ve yan yana sayfasından gözle denetlenmeli; otomatik ölçülmedi.
- Betikler `art/tmp/` altında (commit edilmez).
