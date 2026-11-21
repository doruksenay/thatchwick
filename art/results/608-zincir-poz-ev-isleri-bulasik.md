unity-import: pending

# 608 Zincir pozları, ev işleri ve bulaşık

Durum: `review`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true` (9 kare). Erkek `c63fe39d-…` (man_r84), kadın `135cb0b1-…` (woman_r84); ikisi de 84x84, 8 rotasyon, `completed`.
- 5 anahtar x 2 karakter x 4 yön = 40 çağrı, 1 gen/yön (makbuz), toplam **40 gen** (`cost_est` 40, sapma yok). En çok 3 eşzamanlı çağrı; 429 hatası olmadı.
- Gruplar: erkek sweep 241bd4a4, scrub fa87fb38, wash_dishes 6ef1fa29, whitewash 24f21f91, tread f4202313; kadın sweep f30cc14a, scrub 6b7fc83c, wash_dishes 10e64c10, whitewash 28da333d, tread 79db4784.
- **Sapma:** `end_frame` (durma karesi) verilmedi (bulutta yükleme yolu yok; 572 ile aynı). Başlangıç karesi yönün rotasyonu. Döngü kapanışı kare 0 / son kare IoU'suna göre denetlendi: IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 kare. Birçok klipte IoU düşük (özellikle scrub 0,35-0,62): döngü tam kapanmıyor, görsel karar için not.
- `get_balance` sonrası kalan: 9252 / 10000 (başka şeritlerin harcamasını da içerir).

## İşleme
Ham kare 112x112, karakter 84 px. Her karede en alt opak satır y=81'e kaydırıldı (dikey kaydırma), (14,14,98,98) merkez kırpma; ölçekleme yok. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` 32 rengi, en yakın RGB, alfa ikili. Pivot x=42, y=2 (ALTTAN).

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<sweep|scrub|wash_dishes|whitewash|tread>_<se|sw|nw|ne>_<NN>.png`: 316 PNG, 84x84, pivot (42, 2 alttan). 36 klip teslim.

## Teslim edilmeyen (84x84 dışına taşma; içerik kırpma yok)
Ham işlenmiş kareler `art/results/608-red/` (35 kare):
- erkek wash_dishes SE (2 px taşma), erkek wash_dishes NE (96 px)
- kadın sweep SW (22 px), kadın tread SW (14 px)
Yeniden üretim ya da 96x96 tuval kararı gerekir. Eksik yönler: erkek wash_dishes SE/NE, kadın sweep SW, kadın tread SW.

## Doğrulama
| Denetim | Sonuç |
|---|---|
| `validate_iso.py` (her klibin 00 karesi, 36) | hepsi geçti; 84x84, 32 renk, alt boşluk 2 satır |
| `style.py check --family karakter` (316 kare) | KALDI 0, UYARI 79, GEÇTİ 237 (UYARI doygunluk/palet sapması; aile kilidi kurulmamış, teslimi engellemedi) |
| Ayak satırı | her karede en alt opak satır y=81 (inşa gereği; çömelme ve çiğnemede kare başına hizalandı) |
| Yan yana sayfası | `art/results/608-yanyana.png` (36 klibin 00 karesi + referanslar) |
| GIF | `art/results/608-<man|woman>-<anahtar>.gif` (yönler yan yana; eksik yönler yok) |

## Sorunlar (kullanıcı görsel kararı için)
- Elde tutulan nesnenin okunurluğu, kareye gömülü leğen/mobilya ve döngü kapanışı GIF'lerden gözle denetlenmeli; otomatik denetim bunları ölçmedi.
- Düşük IoU'lu klipler (scrub) döngüde sıçrama gösterebilir.
- `hand_anchors.txt` hesaplanmadı (elde tek renk ayrımı yok; görsel tespit gerek, ayrı iş önerisi).
- Öneri (`tools/art` yazılmadı): `end_frame` yükleme için bulutta bir yardımcı.
