unity-import: pending

# 418p İkon palet uyumu (durum: review)

Yan yana sayfası: [418p-yanyana.png](418p-yanyana.png) (arayuz ailesinde referans yok, yalnızca yeni görseller; eski ve palete oturtulmuş hâller sayfaya eklenmedi). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 7 x `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (418b/418t yöntemi), her açıklamanın sonunda "simple still-life object, no face, no eyes, no creature, no person". 6 ilk çağrı + serbet için 1 yeniden deneme (`max_attempts: 2`) = **7 gen** (cost_est 6, cost_max 12).
- Hız sınırı: serbet çağrısı ilk gönderimde 429 aldı (harcama yok), bekleyip bir kez yeniden gönderildi.
- Serbet ilk denemesi stil denetiminde KALDI (palete yakın %70, palet dışı renkler); ikinci denemede ("flat shading", "small footed clay goblet bowl") GEÇTİ. İlk hâl `art/418p-red/` altında.
- Renk indirgeme: >16 renkli game_meat (18), jam_berry (19) ve serbet ilk (35) Pillow median-cut ile 16 renge indirildi, dither yok, alfa 0/255. Ham hâller `art/tmp418p/` (commit edilmedi).
- `get_balance` sonrası: **979,6** (paylaşılan hesap, paralel şeritler dahil; bu işin payı 7).

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan)
icon_item_{cloth, game_meat, jam_berry, meat, salted_meat, serbet}_v2.png: 6 dosya, hepsi teslim.

## Doğrulama
| İkon | Boyut | Palete yakın | Sonuç |
|---|---|---|---|
| cloth_v2 | 32x32 | %99 | GEÇTİ |
| game_meat_v2 | 32x32 | %85 | UYARI (palete uzak) |
| jam_berry_v2 | 32x32 | %89 | UYARI (palete uzak) |
| meat_v2 | 32x32 | %100 | UYARI (ort. uzaklık 12,6) |
| salted_meat_v2 | 32x32 | %99 | GEÇTİ |
| serbet_v2 | 32x32 | %96 | GEÇTİ (2. deneme) |

Yarı saydam piksel 0, renk sayısı ≤16, yüz/göz/figür yok. İş dosyası "GEÇMELİ" diyor; üç UYARI kalan ikon KALDI değil, kullanıcı gözle karar versin. `validate_iso.py` uygulanmadı (UI ikonu).

## Sorunlar
1. Üç UYARI: game_meat, jam_berry, meat palete biraz uzak.
2. Görsel içerik (tuz taneleri, yağ damarı, bez kapak/ip) 32x32'de ayrıntı sınırında; gözle bakılmalı.
3. Eski dosyaların üzerine yazılmadı; kod eşlemesi (`IsoArt.IconOverrides`) kabulden sonra `_v2`'ye çevrilecek.
