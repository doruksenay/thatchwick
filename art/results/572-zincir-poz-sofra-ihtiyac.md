unity-import: pending

# 572 Zincir pozları, sofra ve ihtiyaç

Durum: `review`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true` (9 kare). Erkek `c63fe39d-…` (man_r84), kadın `135cb0b1-…` (woman_r84); ikisi de 84x84, `completed`.
- 5 anahtar x 2 karakter x 4 yön (SE, SW, NW, NE) = 40 çağrı, 1 gen/yön (makbuz), toplam **40 gen** (`cost_est` 40, sapma yok). En çok 3 eşzamanlı çağrı.
- **Sapma:** tarifteki `end_frame` (durma karesi) verilmedi. Bulutta yükleme yolu yok (`get_upload_url` + dosya gönderimi mümkün değil, curl yok); başlangıç karesi zaten yönün rotasyonu. Döngü kapanışı bu yüzden kare 0 ile son kare IoU'suna göre denetlendi.
- Gruplar: erkek sit bf69a759, sit_eat 8f5603eb, drink 80aa679a, warm_hands 44262725, wash 443328eb; kadın sit 526f7ad4, sit_eat e1bb5e07, drink c779503e, warm_hands 9a621e31, wash 9ba4cdb9.
- `get_balance` sonrası kalan: 9615 / 10000 (öncesi 9671; fark başka şeritlerin harcamasını da içerir).

## İşleme
Ham kare 112x112 (karakter 84 px merkezde). Yalnızca kaydırma ve merkez kırpma (14,14,98,98); ölçekleme yok. Her karenin en alt opak satırı y=81. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` 32 rengi, en yakın RGB, alfa ikili. Kare 0 ile 8 IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 kare. Pivot x=42, y=2 (ALTTAN).

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<sit|sit_eat|drink|warm_hands|wash>_<se|sw|nw|ne>_<NN>.png`: 337 PNG, 84x84, pivot (42, 2 alttan). 39 klip teslim.

## Teslim edilmeyen
- **erkek wash SE**: 9 karenin birinde ya da birkaçında 312 opak piksel 84x84 dışına taşıyor (kural: içerik kırpma yok). Ham işlenmiş kareler `art/results/572-red/`. Yeniden üretim ya da 96x96 tuval kararı gerekir.

## Doğrulama
| Denetim | Sonuç |
|---|---|
| `validate_iso.py` (her klibin 00 karesi, 39) | hepsi geçti; 84x84, 32 renk, alt boşluk 2 satır |
| `style.py check --family karakter` (337 kare) | KALDI 0, UYARI 63, GEÇTİ 274. UYARI yalnızca doygunluk/palet sapması; aile kilidi kurulmamış |
| Ayak satırı | her karede en alt opak satır y=81 (inşa gereği) |
| Tepe kırpılması | teslim edilenlerde yok |
| Yan yana sayfası | `art/results/572-yanyana.png` (39 klibin 00 karesi, referanslarla) |
| GIF | `art/results/572-<man|woman>-<anahtar>.gif` (4 yön yan yana) |

## Sorunlar (kullanıcı görsel kararı için)
- **`sit` klipleri oturmuyor:** karakter ayakta duruyor, küçük solunum hareketi var. Pixminimax tabure çizmedi; kod bu pozu bankta/sandalyede oynatırsa bacaklar havada kalır. Yeniden deneme (tek deneme kuralı) yapılmadı.
- **Kareye gömülü mobilya:** erkek `sit_eat` SE ve SW'de masa+sandalye, kadın `wash` ve erkek `wash` kimi yönlerde leğen, kare içine çizilmiş. Oyunda ayrı eşya olarak çizildiği için çift görünür; bu kareler büyük olasılıkla temizlenmeli (maskeyle mobilya silme ayrı iş).
- Tutan el konumları (`hand_anchors.txt`) hesaplanmadı; elin tek renk ayrımı yok, görsel tespit gerek. Ayrı iş önerisi.
- Pozlar tarifteki "elde tutulan nesne küçük olmalı" ölçütünü karşılıyor (kaşık/fincan çok küçük, okunurluğu sınırda).
- Aile kilidi kurulmadığı için doygunluk uyarıları teslimi engellemedi; `fit` gerekmedi.
- Öneri (`tools/art` yazılmadı): `end_frame` yükleme için bulutta bir yardımcı.
