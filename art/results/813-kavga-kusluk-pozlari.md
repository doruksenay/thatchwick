unity-import: pending

# 813 Kavga ve küslük pozları, yetişkin

Durum: `review`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true` (9 kare). Erkek `c63fe39d-…` (man_r84), kadın `135cb0b1-…` (woman_r84); ikisi de 84x84, 8 rotasyon, `completed`.
- 6 anahtar x 2 karakter x 4 yön (SE, SW, NW, NE) = **48 gen** (`cost_est` 48, sapma yok; çağrı başına 4 yön, 1 gen/yön). Aynı anda en çok 3 çağrı; eşzamanlılık hatası olmadı.
- Sapma: `end_frame` (durma karesi) verilmedi; bulutta yükleme yolu yok (572 ile aynı). Başlangıç karesi yönün rotasyonu.
- Gruplar (erkek / kadın): argue 016bc33b / c5a8e045, fight_ready 02614ea9 / 5da9df51, fight_win 1452f161 / 86741326, fight_lose 378b3eb1 / 77e74461, sulk 34402421 / 3b70be46, apologize e404f9ab / 5101cfc7.
- `get_balance`: başlangıç 9596, sonra 9513 (fark başka şeritleri de içerir).

## İşleme
Ham kare 112x112; yalnızca merkez kırpma (14,14,98,98), ölçekleme yok. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` 32 rengi, en yakın RGB, alfa ikili. Kare 0 ile 8 IoU > 0,95 ise son kare atıldı (argue man SE, argue woman NE: 8 kare), diğerleri 9 kare. Pivot x=42, y=2 (ALTTAN). Ayak satırı: ayakta pozlarda SE/SW y=81, NW/NE y=79 (rotasyonun kendi alt satırı, kare boyunca sabit). `fight_lose` oturma kareleri: kalça/alt satır y=81..83 (SE/SW), 79..82 (NW/NE) arasında değişir; yerde oturma gereği.

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<argue|fight_ready|fight_win|fight_lose|sulk|apologize>_<se|sw|nw|ne>_<NN>.png`: 412 PNG, 84x84, pivot (42, 2 alttan). 46 klip teslim (48 üretildi).

## Teslim edilmeyen
- **man ve woman `fight_lose` SE** (18 kare): oturan karakter 84x84 dışına taşıyor (erkek 25, kadın 24 opak piksel). İçerik kırpma yok kuralı. Ham işlenmiş kareler `art/results/813-red/`. Yeniden üretim ya da büyük tuval kararı gerekir.

## Doğrulama
| Denetim | Sonuç |
|---|---|
| `validate_iso.py` (örnek 00 kareleri) | geçti; 84x84, 32 renk, alt boşluk 2 satır |
| `style.py check --family karakter` (412 kare) | KALDI 0, UYARI 102 (yalnızca "ailesinden doygun", x1,27-1,37; aile kilidi kurulmamış), kalanı GEÇTİ |
| Ayak satırı | yukarıda |
| Yan yana sayfası | `art/results/813-yanyana.png` (46 klibin 00 karesi + referanslar) |
| GIF | `art/results/813-<man|woman>-<anahtar>.gif` (yönler yan yana) |

## Sorunlar (görsel karar için)
- Pixminimax pozları (kollar kavuşturma, yerde oturma, el göğüste) tek denemede okunur mu, kullanıcı GIF'lerden bakmalı; 572'de `sit` oturmamıştı, `fight_lose` oturma kalitesi özellikle denetlenmeli (IoU 0,3-0,45: poz belirgin değişiyor).
- Tutan/vuran el konumu ya da toz bulutu bu işte yok (814).
- Öneri (`tools/art` yazılmadı): `end_frame` yükleme yardımcısı.
