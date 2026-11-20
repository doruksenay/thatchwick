unity-import: pending

# 018c Etkileşim animasyonları: idle, chop, hoe, sow, water (84 px, erkek ve kadın)

- **Bu çalışmada yeni PixelLab çağrısı yapılmadı (0 gen).** `get_character` iki karakterde de 5 anahtarın 4 yönünü (SE, SW, NW, NE; `animate_character` pixminimax, 8 kare + `keep_first_frame` = 9 kare) zaten gösteriyordu; tarihleri 2026-11-18, yani bu işin önceki sahiplenmesinde üretilmiş ama kuyruğa işlenmemiş çıktılar. Çift harcama olmasın diye aynı klipler yeniden üretilmedi; ham kareler (PixelLab animation URL'leri, `{i}.png` 0-8) indirilip işlendi. Bu üretimin maliyeti 11-18 harcamasında kalır (tahmin 40, tavan 52); `art/ledger/018c.md`'de bu çalışmaya 0 yazıldı.
- Karakterler: `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8), `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d), 84x84, 8 yön, `completed`.
- `get_balance`: 3092,9 kalan (Tier 2, plan `art/budget.md` ile aynı; diğer şeritler de harcıyor, sayı bu işe ait değil).
- İşleme (014/018d kuralları): ham kare 84x112 (bazı kliplerde 92-120 geniş, 124-148 yüksek). Yalnızca kaydırma: her karenin en alt opak satırı y=81'e çekildi, geniş karelerde yatay ortalama; ölçekleme yok. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` renkleri, en yakın RGB, alfa ikili. Kare 0 ile 8 IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 kare. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: `art/results/018c-yanyana.png` (2 referans + SE kare 00 ve 03, 18 yeni). GIF: `art/results/018c-<man|woman>-<anahtar>.gif` (yönler yan yana, 2x).
- masaüstü kopyası: yerel

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam 301 PNG.

| Anahtar | Erkek (yön x kare) | Kadın (yön x kare) |
|---|---|---|
| idle | 4 x 8 | 4 x 8 |
| chop | se 8, nw 8, ne 9 (sw YOK) | sw 8, nw 8 (se, ne YOK) |
| hoe | 4 x 8 | 4 x 8 |
| sow | se 8, sw 8, nw 8, ne 9 | 4 x 8 |
| water | se 9, sw 8, nw 9, ne 9 | 4 x 8 |

## Teslim edilmeyenler (84x84'e sığmadı)
Üç chop klibi (baş üstü balta salınımı) tuvalin üstünden taşıyor; 84x84'e sığdırmak opak piksel kırpmak olurdu (kural: içerik kırpma yok), bu yüzden teslim edilmedi: erkek chop SW (ham 120x148, 307 px taşma), kadın chop SE (96x124, 16 px), kadın chop NE (92x136, 27 px). Kaydırılmış (kırpılmış) hâlleri `art/results/018c-red/` altında. Çözüm önerisi: bu üç yön için tek yönlü yeniden üretim (daha küçük salınım) ya da 96x96 tuval kararı; kullanıcı/kod oturumu karar versin. Yeni üretim yapılmadı (tek deneme kuralı).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (301 dosya) | 301/301 geçti (84x84, <=32 renk, alt boşluk 2, yarı saydam 0) |
| style.py check karakter | KALDI 0, UYARI 62, GEÇTİ 239. UYARI yalnızca "ailesinden doygun" (x1,26-1,35; erkek SE/SW ağırlıklı); palet yakınlığı %97-100. Aile kilidi kurulmadı (referans < 3), palet uyumu uyarı düzeyi. |
| Ayak satırı | her karede en alt opak satır y=81 |
| Kare 0 - son kare IoU | idle 0,999-1,0; hoe 0,966-0,999; sow 0,919-1,0 (man sow ne 9 kare); water 0,908-1,0; chop 0,949-1,0 (man chop ne 9 kare) |
| Ardışık IoU (en düşük) | man chop nw 0,71, man chop ne 0,78, woman chop nw 0,68 (hızlı salınım); man water se 0,78; diğerleri >= 0,83 |
| Tepe kırpılması | teslim edilenlerde yok (en küçük tepe y: 2) |

## Sorunlar ve notlar
- Kimlik (yüz, giysi rengi) ve elde tutulan nesne (balta, çapa, kova, tohum) okunurluğu yalnızca yan yana sayfasından ve GIF'lerden gözle bakılmalı; otomatik ölçülmedi.
- Erkek chop NE, sow NE, water SE/NW/NE 9 kare: döngü kapanışı yakın değil, 08'in başa geçişine bakılmalı.
- 018c'nin tam teslimi için 3 chop yönü eksik.
