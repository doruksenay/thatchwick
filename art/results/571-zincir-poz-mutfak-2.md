unity-import: pending

# 571 Zincir pozları, mutfak 2/2: pour, reach, carry, kindle, peel (84 px, erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün PixelLab döndürme görseli (rotations/<yön>.png). `end_frame` tek yön kabul ettiği için her yön ayrı çağrı (aynı klip grubuna `animation_group_id` ile eklendi). Karakterler: `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8), `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d); ikisi `completed`, 8 rotasyon.
- Eylem metinleri iş dosyasındaki gibi (pour, reach, carry, kindle, peel). Aynalama yok.
- Maliyet: çağrı başına 1 generation (makbuz), 40 çağrı = 40 generation (tahmin 40, tavan 52). Tek deneme; üretim hatası yok. Hesapta 10 iş yuvası sınırı vardı (diğer şeritlerle paylaşımlı); "need 1 job slots" ile reddedilen çağrılar para harcamadı, slot açılınca aynı çağrı bir kez yeniden gönderildi. Her klip için `get_character` ile grup/yön sayısı denetlendi: 10 grup x 4 yön, çift üretim yok.
- `get_balance` sonrası kalan: 1383,7 (diğer şeritler aynı anda harcıyor, sayı yalnızca bu işe ait değil).
- Ham çıktı 84x112 ya da daha geniş (en çok 120x112), 9 kare. İşleme (018c/018d kuralları): yalnızca kaydırma; her karenin en alt opak satırı y=81, yatay ortalama; kırpılan opak piksel 0, ölçekleme yok. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` 32 rengi, en yakın RGB, alfa ikili. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 kare. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: [571-yanyana.png](571-yanyana.png) (2 referans + SE kare 00 ve 03, 20 yeni). GIF (4 yön yan yana, 2x): `art/results/571-<man|woman>-<anahtar>.gif`.
- Dosya adı: `Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`. Toplam 323 PNG, hepsi 84x84, pivot x=42, pivot y=2 (alttan).
- masaüstü kopyası: yerel

## Dosyalar

| Anahtar | Erkek (yön x kare) | Kadın (yön x kare) |
|---|---|---|
| pour | 4 x 8 | 4 x 8 |
| reach | se 8, sw 8, nw 8, ne 8 | 4 x 8 |
| carry | se 9, sw 8, nw 8, ne 8 | 4 x 8 |
| kindle | 4 x 8 | 4 x 8 |
| peel | se 8, sw 9, nw 8, ne 8 | se 9, sw 8, nw 8, ne 8 |

9 kareli klipler (kapanış yakın değil, 08'in başa geçişine bakılmalı): man carry se (IoU 0,906), man peel sw (0,925), woman peel se (0,882).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (323 dosya) | 323/323 geçti |
| style.py check karakter | KALDI 0, UYARI 72, GEÇTİ 251. UYARI yalnızca "ailesinden doygun" (erkek, x1,26-1,35); palet yakınlığı %100. Aile kilidi kurulmadı (referans < 3), palet uyumu uyarı düzeyi; `fit` uygulanmadı. |
| Ayak satırı | her karede en alt opak satır y=81 |
| Tuval taşması / kırpma | 0 opak piksel kırpıldı, 40 klibin hepsi 84x84'e sığdı |
| Tepe | en küçük tepe y: woman peel se 0 (küreğin ucu üst kenara değiyor), man reach se / man peel sw / woman reach se / woman peel sw 1; diğerleri >= 2. Kırpma yok ama kenara yakın. |
| Ardışık IoU (en düşük) | woman peel sw 0,75; diğerleri >= 0,80 |
| Döşeme testi | zemin işi değil, uygulanmadı |

## El konumları
`hand_anchors.txt` için ilk karedeki tutan el konumu otomatik ölçülmedi (kare 0 durma pozu; el konumları klibin ortasında değişiyor). Kod/kullanıcı gerektiğinde `018d3-el-konumu` yöntemiyle ölçmeli.

## Sorunlar ve notlar
- Kimlik (yüz, giysi, saç) ve elde tutulan nesnenin okunurluğu yalnızca yan yana sayfasından gözle bakıldı: pour (testi), reach (kalkan kol), kindle (eğilme, kıvılcım) ve peel (uzun kürek) okunuyor. **`carry`: göğse tutulan büyük nesne PixelLab çıktısında görünmüyor**, yalnızca iki kolla duruş var (eylem metni nesne çizdirmedi). Oyunda taşınan eşya el çapasına ayrıca çizildiği için sorun değil, ama kullanıcı GIF'ten teyit etsin.
- Otomatik ölçülmeyen: kimlik kayması, GIF akıcılığı.
- Öneri: eşzamanlı iş yuvası (10) diğer şeritlerle paylaşıldığı için 40 çağrılık işler birkaç turda gönderildi; iş akışı bunu bilmeli.
