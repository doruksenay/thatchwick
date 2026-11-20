unity-import: pending

# 740 Muhabbet pozları 1: laugh_smile, laugh_big, giggle, listen_nod, listen_arms, shrug (84 px, erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = karakterin ilgili yön rotasyonu (durma karesi). end_frame tek yönlü olduğu için her çağrı tek yön: 6 x 2 x 4 = 48 çağrı, 1 gen/çağrı = **48 gen** (cost_est 48, tavan 60). Aynalama yok.
- Karakterler: man_r84 `c63fe39d-b168-4732-bf11-b476172c98e8`, woman_r84 `135cb0b1-bd46-4514-8c87-0834b96ef80d`.
- Hız sınırı: hesapta 10 eşzamanlı iş sınırı vardı ("10/10 used", diğer şeritlerle paylaşılıyor); reddedilen çağrılar (harcama yapmaz) slot açılınca yeniden gönderildi. Hiçbir klip iki kez üretilmedi.
- `get_balance` sonrası: 602,6 (Tier 2; paylaşılan hesap, sayı yalnızca bu işe ait değil).
- İşleme (018c ile aynı): ham kare 84x112; alfa ikili, palet `iso_char_<g>_r84_cand_se.png` renklerine en yakın RGB, ayak satırı y=81'e kaydırma, yatay ortalama (kare 0'a göre tek ofset), ölçekleme ve kırpma yok (taşan piksel 0). Kare 0 - son kare IoU > 0,95 ise son kare atıldı (8 kare); aksi 9 kare (man listen_arms sw IoU 0,876, woman listen_arms ne 0,923). Pivot x=42, y=2 (ALTTAN).
- Yan yana: `art/results/740-yanyana.png` (referanslar + 12 poz SE). GIF (4 yön yan yana, 2x): `art/results/740-<man|woman>-<poz>.gif` (12 dosya).
- masaüstü kopyası: yerel

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<poz>_<se|sw|nw|ne>_<NN>.png`, 386 PNG, hepsi 84x84, pivot x=42, y=2 (alttan). Her poz/yön 8 kare (00-07); iki istisna 9 kare (00-08): man listen_arms sw, woman listen_arms ne.

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (386 dosya) | 386/386 geçti |
| style.py check karakter | KALDI 0, UYARI 99, GEÇTİ 287. UYARI yalnızca "ailesinden doygun" (x1,2-1,3, erkek ağırlıklı); aile kilidi kurulu değil, palet yakınlığı %100 |
| Boyut / ayak satırı | 386/386 84x84, en alt opak satır y=81 |
| Kare 0 - son kare IoU | 0,996-1,0; istisna man listen_arms sw 0,876 ve woman listen_arms ne 0,923 (9 kare) |
| Taşma | 0 piksel |

## Sorunlar ve notlar
- Kimlik (yüz, giysi, saç) yan yana sayfasında tutarlı; poz okunurluğu (kahkaha, el ağızda, kollar kavuşuk, omuz silkme) SE'de net. Diğer yönler yalnızca GIF'lerden gözle bakılmalı.
- İki 9 kareli klipte döngü kapanışı yakın değil; 08'den 00'a geçiş gözle kontrol edilmeli.
- `shrug` tarifte "short" (tek seferlik); klip 8 kare, döngü zorunlu değil.
- Geçici dosyalar `art/tmp/` altında (git dışı); `tools/art` değişmedi.
