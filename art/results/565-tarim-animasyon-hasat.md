unity-import: pending

# 565 Tarım animasyonları 1/2: reap, scythe, pick, bend, thresh (84 px, erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün rotasyon görseli (018d3/018d4 ile aynı yöntem). Karakterler man_r84 (c63fe39d-b168-4732-bf11-b476172c98e8), woman_r84 (135cb0b1-bd46-4514-8c87-0834b96ef80d), 84x84, 8 yön, `completed`.
- Çağrı: 5 anahtar x 2 karakter x 4 yön = 40 çağrı, her biri 1 generation (araç çıktısındaki "cost: 1 generation"), toplam 40 gen (`cost_est` 40, tavan 52). Tek deneme.
- Slot sınırı: PixelLab 10 eşzamanlı iş verdi; "need 1 job slots but only 0 available" hatası alan çağrılar boşluk açılınca aynı parametrelerle yeniden gönderildi (hata üretim başlatmadı, harcama yok). Başarısız üretim yok.
- `get_balance` sonrası: 1495,7 kalan (Tier 2, plan `art/budget.md` ile aynı; başlangıçta 1543,7; fark 48 diğer şeritlerin harcamasını da içerir).
- İşleme (018c ile aynı): ham kare 84x112 ve üzeri (92-136 geniş/yüksek). Yalnızca kaydırma: her karenin en alt opak satırı y=81'e, yatayda ham tuval ortası x=42'ye; ölçekleme yok. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` renkleri, en yakın RGB, alfa ikili. Kare 0 ve 8 IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 kare.
- Yan yana sayfası: `art/results/565-yanyana.png` (2 referans + 11 yeni kare). GIF: `art/results/565-<man|woman>-<anahtar>.gif` (yönler yan yana, 2x).
- masaüstü kopyası: yerel

## Teslim edilen (30 klip, 243 PNG)
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (ALTTAN).

| karakter | anahtar | yön | kare | ham tuval | IoU ilk-son | en düşük ardışık IoU | taşan piksel |
|---|---|---|---|---|---|---|---|
| man | bend | ne | 9 | 84x112 | 0.923 | 0.59 | 0 |
| man | bend | nw | 8 | 84x112 | 0.999 | 0.66 | 0 |
| man | bend | se | 8 | 84x116 | 0.991 | 0.68 | 0 |
| man | bend | sw | 8 | 84x116 | 0.999 | 0.66 | 0 |
| man | pick | ne | 8 | 92x112 | 0.997 | 0.86 | 0 |
| man | pick | nw | 8 | 84x112 | 1.0 | 0.85 | 0 |
| man | pick | se | 8 | 84x120 | 1.0 | 0.82 | 0 |
| man | pick | sw | 8 | 84x112 | 0.999 | 0.84 | 0 |
| man | reap | ne | 8 | 96x112 | 0.999 | 0.69 | 0 |
| man | reap | nw | 8 | 92x112 | 0.989 | 0.75 | 0 |
| man | reap | se | 8 | 112x124 | 0.999 | 0.7 | 0 |
| man | reap | sw | 8 | 84x112 | 0.998 | 0.67 | 0 |
| man | scythe | ne | 9 | 104x116 | 0.927 | 0.73 | 0 |
| man | scythe | nw | 8 | 120x112 | 1.0 | 0.67 | 0 |
| man | thresh | ne | 8 | 96x112 | 0.998 | 0.66 | 0 |
| man | thresh | nw | 8 | 84x112 | 0.998 | 0.87 | 0 |
| woman | bend | ne | 8 | 88x112 | 1.0 | 0.67 | 0 |
| woman | bend | nw | 8 | 84x112 | 0.993 | 0.76 | 0 |
| woman | bend | se | 8 | 92x120 | 0.968 | 0.6 | 0 |
| woman | bend | sw | 8 | 84x112 | 1.0 | 0.72 | 0 |
| woman | pick | ne | 8 | 88x112 | 0.999 | 0.76 | 0 |
| woman | pick | nw | 8 | 84x112 | 1.0 | 0.85 | 0 |
| woman | pick | se | 8 | 88x116 | 0.969 | 0.86 | 0 |
| woman | reap | ne | 8 | 84x112 | 0.999 | 0.75 | 0 |
| woman | reap | nw | 8 | 92x112 | 0.999 | 0.79 | 0 |
| woman | reap | se | 8 | 96x120 | 1.0 | 0.77 | 0 |
| woman | reap | sw | 8 | 84x112 | 1.0 | 0.76 | 0 |
| woman | scythe | nw | 9 | 120x112 | 0.841 | 0.66 | 0 |
| woman | thresh | ne | 8 | 112x112 | 0.999 | 0.6 | 0 |
| woman | thresh | nw | 8 | 96x116 | 0.999 | 0.71 | 0 |

## Teslim edilmeyen (10 klip: 84x84'e sığmadı)
Bu klipler tuvalin dışına taşıyor (uzun saplı tırpan ve tokmak/döven savrulması); sığdırmak opak piksel kırpmak olurdu (kural: içerik kırpma yok). Kaydırılmış (kırpılmış) hâlleri `art/results/565-red/` altında. Eksik yönler: man scythe se/sw, man thresh se/sw, woman pick sw, woman scythe se/sw/ne, woman thresh se/sw. Kod, eksik yönde çapa pozuna düşer. Çözüm önerisi (kullanıcı/kod oturumu karar versin): bu yönler için 96x96 ya da 112x112 tuval kararı ve yeniden üretim, ya da daha kısa salınım tarifi. Yeni üretim yapılmadı (tek deneme).

| karakter | anahtar | yön | kare | ham tuval | IoU ilk-son | en düşük ardışık IoU | taşan piksel |
|---|---|---|---|---|---|---|---|
| man | scythe | se | 9 | 112x132 | 0.809 | 0.64 | 52 |
| man | scythe | sw | 9 | 108x124 | 0.841 | 0.67 | 4 |
| man | thresh | se | 9 | 96x132 | 0.883 | 0.61 | 56 |
| man | thresh | sw | 9 | 84x124 | 0.923 | 0.76 | 4 |
| woman | pick | sw | 9 | 84x132 | 0.901 | 0.79 | 15 |
| woman | scythe | ne | 9 | 96x124 | 0.854 | 0.64 | 5 |
| woman | scythe | se | 8 | 112x136 | 1.0 | 0.61 | 85 |
| woman | scythe | sw | 9 | 108x132 | 0.864 | 0.67 | 48 |
| woman | thresh | se | 8 | 104x132 | 1.0 | 0.64 | 32 |
| woman | thresh | sw | 8 | 84x136 | 0.996 | 0.55 | 83 |

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (243 dosya) | 243/243 geçti (84x84, <=32 renk, yarı saydam 0, alt boşluk) |
| style.py check karakter | KALDI 0, UYARI 46, GEÇTİ 197. UYARI yalnızca "ailesinden doygun" (en çok x1,35, erkek SE/SW ağırlıklı); palet yakınlığı %100. Aile kilidi kurulmadı (referans < 3), palet uyumu uyarı düzeyi. |
| Ayak satırı | her karede en alt opak satır y=81 |
| Alet okunurluğu | yan yana sayfasında orak kısa ve kavisli, tırpan uzun saplı okunuyor; meyve (elma) ve eğilip toplama pozları okunuyor. Tokmak (thresh) küçük, GIF'e bakılmalı |
| Döngü kapanışı | 9 kareli klipler (iyi kapanmayanlar): man bend ne 0,923, man scythe ne 0,927, woman scythe nw 0,841 |

## Sorunlar ve notlar
- Kimlik (yüz, giysi rengi) kaymasına yalnızca yan yana sayfası ve GIF'lerden gözle bakıldı; otomatik ölçülmedi.
- Tırpan ve tokmak klipleri en çok taşan klipler (10 eksik klibin 9'u bu iki anahtar).
- Düşük ardışık IoU (en düşük 0,55: woman thresh sw, kırmızıda): hızlı salınım.
