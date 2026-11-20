unity-import: pending

# 589 Zincir pozları, mutfak 1/2: stir, chop_knife, taste, ladle, knead (84 px, erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün rotasyon karesi (durma karesi). Karakterler `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8) ve `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d), ikisi de `completed`, 8 rotasyon. Aynalama yok.
- Maliyet: 40 çağrı x 1 gen = **40 gen** (tahmin 40, tavan 52). Her çağrı öncesi araç yanıtındaki maliyet satırı okundu (1/yön, `cost_est` ile uyumlu).
- `get_balance` sonrası: **1201,7** (Tier 2; plan `art/budget.md` ile aynı). Diğer şeritler de harcadığı için bu sayı yalnızca bu işe ait değil.
- Hız sınırı: 10 iş yuvası ortak kullanıldığı için bazı çağrılar "10/10 used" ile reddedildi (üretim başlamadı, ücret yok); `wait_for_jobs` sonrası aynı çağrı bir kez yeniden gönderildi. Hiçbir klip iki kez üretilmedi.
- İşleme (018c ile aynı): ham kare 84x112 (bazılarında geniş/yüksek). Yalnızca kaydırma: her karenin en alt opak satırı y=81; geniş karelerde yatay ortalama; ölçekleme yok. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` renkleri, en yakın RGB, alfa ikili. Kare 0 ile 8 IoU > 0,95 ise son kare atıldı (8 kare), değilse 9. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: `art/results/589-yanyana.png` (2 referans + SE kare 00 ve 03, 10 klip). GIF: `art/results/589-<man|woman>-<anahtar>.gif` (4 yön yan yana, 2x).
- masaüstü kopyası: yerel

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam **314 PNG, 39 klip** (40 üretildi).

| Anahtar | Erkek (yön x kare) | Kadın (yön x kare) |
|---|---|---|
| stir | se 8, sw 9, nw 8, ne 8 | 4 x 8 |
| chop_knife | 4 x 8 | 4 x 8 |
| taste | 4 x 8 | 4 x 8 |
| ladle | 4 x 8 | se 8, sw 9, nw 8, ne 8 |
| knead | se 8, nw 8, ne 8 (**sw YOK**) | 4 x 8 |

## Teslim edilmeyen
Erkek knead SW: ham kare 120x148, 84x84 tuvale sığmıyor (552 opak piksel taşıyor); içerik kırpmak kuralı gereği teslim edilmedi. Kaydırılmış (kırpık) hâli `art/results/589-red/` altında (9 kare). Kare 0 - 8 IoU 0,44: hareket de çok geniş. Kod bu yön için `knead` yerine eldeki en yakın poza düşer. Öneri: bu tek yön için yeniden üretim (1 gen) ya da 96x96 tuval kararı. Tek deneme kuralı gereği yeniden üretilmedi.

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (314 dosya) | 314/314 geçti (84x84, <=32 renk, yarı saydam 0) |
| style.py check karakter | KALDI 0, UYARI 70, GEÇTİ 244. UYARI "ailesinden doygun" ağırlıklı; palet yakınlığı %100 (palet doğrudan karakter paletinden). Aile kilidi kurulmadı, palet uyumu uyarı düzeyi. Ayrıntı: yerel `style.py check` çıktısı. |
| Ayak satırı | her karede en alt opak satır y=81 |
| Tepe kırpılması | teslim edilenlerde piksel kırpılması 0 |
| Kare 0 - son kare IoU | 0,977-1,0 (8 kare olanlar); 9 kareli: man stir sw 0,752, woman ladle sw 0,887 (döngü kapanışı yakın değil, 08'den 00'a geçişe bakılmalı) |

## Sorunlar ve notlar
- Kimlik (yüz, saç, giysi) yan yana sayfasında durma karesiyle tutarlı göründü; elde tutulan nesne (kaşık, bıçak, kepçe, kase) okunurluğu ve hareketin doğruluğu GIF'lerden gözle bakılmalı (otomatik ölçülmedi). Kullanıcı kabul kararını bununla verir.
- El konumları (`hand_anchors.txt` için ilk karedeki tutan el): **ölçülmedi**; otomatik çıkarım güvenilir değil, kod/kullanıcı oturumu GIF'ten ya da karelerden okumalı.
- 9 kareli iki klip (man stir sw, woman ladle sw) döngü kapanışı zayıf.
- Giysi katmanı aktarımı (509) ayrı iştir.
