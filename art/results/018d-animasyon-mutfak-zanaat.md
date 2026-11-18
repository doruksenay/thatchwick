unity-import: pending

# 018d Etkileşim animasyonları: cook, craft_hammer, craft_hand, work, eat (84 px, erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün PixelLab döndürme görseli (rotations/<yön>.png, https). `end_frame` tek yön kabul ettiği için her yön ayrı çağrı. Karakterler: `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8), `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d); ikisi de `completed`, 8 rotasyon.
- Çağrı başına maliyet: 1 generation (makbuz), toplam 40 çağrı = 40 generation (tahmin 40, tavan 52). Tek deneme; hata yok. Eşzamanlı iş slotu sınırı (10/10, "need 1 job slots") nedeniyle birkaç çağrı reddedildi; para harcanmadı, slot açılınca aynı çağrı bir kez yeniden gönderildi (çift üretim yok).
- `get_balance` sonrası kalan: 4170,9 (diğer şeritler aynı anda harcıyor, sayı yalnızca bu işe ait değil).
- Ham çıktı 84x112, 9 kare. İşleme (014 kuralları): yalnızca kaydırma. Her karenin en alt opak satırı durma karesinin en alt satırına (y=81) çekildi; yatay ofset karakter/yön başına kare 0 ayak merkezinden hesaplandı (çoğu 0; man craft_hammer sw -2, nw -4; woman craft_hammer se -4). Kırpılan opak piksel 0, ölçekleme yok. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` 32 renk, en yakın RGB, alfa ikili.
- Kapanış: kare 0 ile son kare IoU > 0,95 ise son kare atıldı (00-07, 8 kare); değilse 9 kare (00-08) teslim: man craft_hammer sw (IoU 0,815), man work se (0,882), man work ne (0,949). Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: art/results/018d-yanyana.png (iki eski referans + 12 yeni kare). GIF'ler (4 yön yan yana, 2x): art/results/018d-<man|woman>-<anahtar>.gif.
- masaüstü kopyası: yerel

## Dosyalar
Yol: `Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam 323 PNG.

| Anahtar | Yönler x kare (erkek) | Yönler x kare (kadın) |
|---|---|---|
| cook | 4 x 8 | 4 x 8 |
| craft_hammer | se 8, sw 9, nw 8, ne 8 | 4 x 8 |
| craft_hand | 4 x 8 | 4 x 8 |
| work | se 9, sw 8, nw 8, ne 9 | 4 x 8 |
| eat | 4 x 8 | 4 x 8 |

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (323 dosya) | 323/323 geçti (84x84, 32 renk, alt boşluk 2, yarı saydam 0) |
| style.py check karakter | KALDI 0; GEÇTİ ve UYARI. UYARI yalnızca erkek SE/SW karelerinde "ailesinden doygun" (x1,26-1,35, palet yakınlığı %100); kadın ve erkek NE/NW hepsi GEÇTİ. Aile kilidi kurulmadı (referans < 3), palet uyumu uyarı düzeyi. |
| Tepe kırpılması | yok (en küçük tepe y: 0 woman craft_hand se, diğerleri 1-4; kare 0 ve diğerleri üstten kesilmedi, ama woman craft_hand se kolları üst kenara değiyor) |
| Palet dışı oran | %0 |
| Ayak satırı | kare başına en alt opak satır y=81 (durma karesiyle aynı) |
| Ardışık IoU (en düşük) | man work se 0,58; woman work sw 0,57; woman work se 0,66; man craft_hammer sw 0,71; diğerleri 0,72-1,0 |

## Sorunlar ve notlar
- Kimlik ve nesne okunurluğu yalnızca yan yana sayfasından gözle bakıldı: kuyruk kaşığı (man cook), çekiç (man ve woman craft_hammer) okunuyor; `eat` karelerinde ekmek parçası küçük, kullanıcı GIF'ten teyit etsin. `work` (eğilip yerden alma) karelerinde gövde büyük hareket ediyor; düşük IoU bundan, 8 kareyle sıçrama olabilir.
- `craft_hammer` ve `work` bazı yönlerde 9 kare (kapanış yakın değil): döngü için 08'in başa geçişine kullanıcı bakmalı.
- Erkek SE/SW doygunluk uyarısı x1,3 civarı; kilit kurulmadığı için `fit` uygulanmadı.
- Kimlik kayması (yüz, giysi rengi) otomatik ölçülmedi; kullanıcı gözüyle onaylanmalı.
- Öneri: `end_frame_url` ile rotasyon görseli doğrudan kullanılabildi (upload gerekmedi).
