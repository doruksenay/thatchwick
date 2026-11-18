unity-import: pending

# 416 Çocuk karakter, gerçekçi oran, 64 px (erkek ve kız, 8 yön durma)

Durum: review. 16/16 dosya teslim edildi (`Assets/Art/Iso/_aday/anim/`). Yan yana: [416-yanyana.png](416-yanyana.png) (yaşlı erkek, yetişkin erkek, yetişkin kadın 84 SE ve yeni oğlan, kız SE; stil referansları sayfada). **Boy ölçütü erkek çocukta tutmadı** (aşağıda).

## Araç ve ayarlar
- `create_character` v3, humanoid, `high top-down`, siyah tek renk dış çizgi, high detail, size 64, 8 yön (012b/415 ile aynı); tarifteki oran metni, kıyafet ve 012b ortak eki aynen. Çağrılar paralel. Araç stil referansı almadığı için kimlik referansı kullanılmadı.
  - boy: 6940a34e-75c4-4cb6-b88d-8ef7e0e7719e (2 gen)
  - girl: 9d58ae77-0243-403e-84d3-33799a2f7e70 (2 gen)
- `reduce_colors` 32 renk, dithering none, karakter başına 8 yön tek çağrı: 2 x 0,1 gen.
- Toplam harcanan: 4,2 gen. `get_balance` sonrası: 4005,5 (paralel şeritler dahil).
- Her kare yalnızca dikey kaydırıldı (içerik yeniden çizilmedi) ki alt boşluk 2 px olsun; kayma -2..+1 px.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/anim/iso_char_{boy,girl}_r64_idle_{s,se,e,ne,n,nw,w,sw}.png`, 64x64, pivot x=32, y=2 alttan)
| Yön | boy opak (g x y) | girl opak (g x y) |
|---|---|---|
| s | 34x61 | 33x58 |
| se | 29x62 | 26x58 |
| e | 30x62 | 25x57 |
| ne | 28x60 | 27x56 |
| n | 33x59 | 31x56 |
| nw | 29x60 | 26x57 |
| w | 30x62 | 25x57 |
| sw | 30x62 | 25x58 |

Yarı saydam piksel 0; renk 31-32 (girl n/ne/nw 26-27). Alt boşluk hepsinde 2.

## Doğrulama
- `validate_iso --kind object`: SE örnekleri geçti (alt boşluk 2).
- `style.py check --family karakter`: 16/16 GEÇTİ (palete yakın %84-98, doygunluk x0.74-1.13). Aile kilidi kurulmadı; palet uyumu uyarı niteliğinde.
- **Boy ölçütü (SE opak boy / 84 yetişkin opak boyu 78 erkek, 80 kadın; hedef %62-72):**

| Karakter | Boy | Oran | Sonuç |
|---|---|---|---|
| boy | 62 | %79 (erkeğe göre) | tutmadı (çok uzun) |
| girl | 58 | %72,5 (kadına göre) | sınırda |

- Boy/kafa oranı (hedef 4,5-5,5) ölçülmedi; kafa yüksekliği elle ölçülmedi, yan yana sayfasına bakılmalı.

## Sorunlar
- Oğlan, yetişkin erkeğe göre hedeften uzun (%79). Kullanıcı sayfaya bakıp karar versin; gerekirse `size=56` ile yeniden iş yazılır.
- Tek deneme kuralı gereği yeniden üretim yapılmadı.
