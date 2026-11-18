unity-import: pending

# 368 Kazı animasyonları: kazma (dig), kova (bucket), destek (support), 84 px erkek ve kadın

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün PixelLab döndürme görseli (her yön ayrı çağrı). Karakterler: `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8), `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d), ikisi `completed`, 8 rotasyon.
- **Kare sayısı:** tarif kazma ve destek için 6 kare diyordu; `pixminimax` yalnızca 4'ün katını kabul ettiği için 8 kullanıldı (1 gen/çağrı, maliyet aynı). Kova zaten 8.
- Eylem metinleri tarifteki gibi. Çağrı başına maliyet 1 gen (makbuz); 24 çağrı = 24 gen (tahmin 24, tavan 36). Eşzamanlı iş slotu sınırı ("need 1 job slots", 10/10) nedeniyle 4 çağrı reddedildi (para harcanmadı), slot açılınca aynı çağrılar bir kez yeniden gönderildi; çift üretim yok.
- `get_balance` sonrası kalan: 3996,5 (diğer şeritler aynı anda harcıyor).
- İşleme (018 ortak kuralları): ham 84x112-120 kare; her karenin en alt opak satırı y=81'e kaydırıldı (yatay ortalama, ölçekleme yok), palet `iso_char_<cinsiyet>_r84_cand_se.png` 32 renk en yakın RGB, alfa ikili. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı (8 kare); değilse 9 kare. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: art/results/368-yanyana.png. GIF'ler: art/results/368-<man|woman>-<dig|bucket|support>.gif (dört yön yan yana, 2x).
- masaüstü kopyası: yerel

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_mine_<dig|bucket|support>_<se|sw|nw|ne>_<NN>.png`, hepsi 84x84, pivot x=42, y=2 (alttan). Toplam 188 PNG.

| Anahtar | Erkek (se/sw/nw/ne) | Kadın (se/sw/nw/ne) |
|---|---|---|
| dig | 8/8/8/8 | 8/8/8/8 |
| bucket | 9/8/8/8 | 8/8/8/8 |
| support | 8/9/8/8 | 9/8/9/**teslim edilmedi** |

## Teslim edilmeyen
- **woman support ne (8 kare):** kirişi başın üstüne kaldırınca üst kenar taşıyor; ayak satırı y=81'e çekilince 386 opak piksel tuval dışına çıkıyor (tepe kırpılması). Ham hâli `art/368-red/` altında. Başka tuval ya da yeniden üretim gerekir (kullanıcı kararı).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (--kind object, 196 dosya işlendi) | 196/196 geçti |
| style.py check karakter | KALDI 0; GEÇTİ ve UYARI. UYARI yalnızca erkek karelerinde "ailesinden doygun" (x1,26-1,32, palet yakınlığı %100). Aile kilidi kurulmadı (referans < 3), palet uyumu uyarı düzeyi; `fit` uygulanmadı |
| Aynı kare tekrarı | yok (klip içinde birebir aynı kare yok) |
| Ayak satırı | kare başına en alt opak satır y=81 |
| Tepe kırpılması | woman support ne: 386 px (teslim edilmedi). Teslim edilenlerde: man support sw 13 px, woman support nw 3 px kırpıldı (ucu kesik kiriş olabilir, GIF'ten bakılsın); diğerleri 0 |
| Kapanış (kare 0 / son kare IoU) | 9 kareli klipler: man bucket se 0,915, man support sw 0,889, woman support se 0,905, woman support nw 0,89; woman bucket nw 0,95 sınırda (8 kare); diğerleri >0,98 |

## Sorunlar ve notlar
- Kimlik ve nesne okunurluğu yalnızca yan yana sayfasından bakıldı; kazma (kazma aleti), kova ve kiriş okunurluğunu kullanıcı GIF'lerden teyit etsin. Kimlik kayması otomatik ölçülmedi.
- 9 kareli kliplerde döngü geçişi (08 -> 00) gözle bakılmalı.
- Öneri: tepe kırpılması için ham çıktı 84x120; 84x84 tuval yerine daha yüksek tuval düşünülebilir (kullanıcı kararı).
