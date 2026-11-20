---
id: "754"
title: "HUD karma yön: ihtiyaç, hız ve mevsim ikonları (parça başına)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-21: meşe + ilmek karışık; ikonlar parça başına
priority: 1
family: arayuz
style_gate: false
cost_est: 15
cost_max: 30
depends_on: []
max_attempts: 2
pixellab: true
yon: karma
outputs:
  - Assets/Art/Iso/_aday/ui_hud2_karma_{need_hunger,need_energy,need_warmth,need_fun,need_hygiene,need_social,need_toilet}.png
  - Assets/Art/Iso/_aday/ui_hud2_karma_{speed_pause,speed_1,speed_2,speed_3,season_spring,season_summer,season_autumn,season_winter}.png
created: 2026-11-21
---
# 754 HUD karma yön: ikonlar (parça başına)

## Amaç
753 ile aynı karar (karma yön, 2026-11-21). 744-746'da ikon sayfası (pixflux 128×64 ızgara) tutmadı; ikonlar ya okunmaz ya eksik çıktı. Bu iş **her ikonu ayrı çağrıyla** üretir. 745 sayfa 3'teki tanınan ikonlar (ekmek, hilal, alev, topaç, su damlası, filiz, kar tanesi) stil kılavuzudur.

## Araç
- `create_image_pixen` (1 gen/çağrı), tuval 16×16. 12×12 ikonlarda (hız, mevsim) içerik ortadaki 12×12'ye sığacak şekilde tarif edilir, sonra o bölge kırpılır (içerik taşarsa kırpma yapılmaz, raporda yazılır, 16×16 teslim edilir).
- Varsa 745 sayfa 3'teki karşılık gelen ikon kırpılıp `init_image` olarak verilir (yalnızca kılavuz, teslim edilmez).
- Ayarlar: arayüz paleti, `seed=0`, saydam zemin. **Dokusuz düz renk silüet**, 1 px renkli koyu kontur, saf siyah yok, ışık sol üstten. Açıklama eki: `single flat pixel art UI icon, simple readable silhouette, flat colors, 1px dark warm outline, transparent background, no text, no letters, no numbers`

## İkonlar
| Dosya eki | Native | Tarif |
|---|---|---|
| `need_hunger` | 16×16 | Somun ekmek |
| `need_energy` | 16×16 | Hilal ay |
| `need_warmth` | 16×16 | Alev |
| `need_fun` | 16×16 | Ahşap topaç |
| `need_hygiene` | 16×16 | Su damlası ve küçük sabun kalıbı |
| `need_social` | 16×16 | Yan yana iki baş silüeti |
| `need_toilet` | 16×16 | Ahşap oturak (kapaklı kova) |
| `speed_pause` | 12×12 | İki dikey çubuk |
| `speed_1` | 12×12 | Tek sağa üçgen |
| `speed_2` | 12×12 | İki sağa üçgen |
| `speed_3` | 12×12 | Üç sağa üçgen |
| `season_spring` | 12×12 | Yeşil filiz (iki yaprak) |
| `season_summer` | 12×12 | Sarı güneş |
| `season_autumn` | 12×12 | Turuncu yaprak |
| `season_winter` | 12×12 | Açık mavi kar tanesi |

Hız ikonları ahşap/krem tonlu olsun (meşe düğme yüzeyi), sembol koyu kahve.

## Denetim ve teslim
- Her ikon `python tools/art/style.py check --family arayuz <dosya>` GEÇMELİ; geçmeyen için **en fazla bir** yeniden deneme.
- `art/results/754-ikonlar-x3.png`: hepsi 2× ve 3× büyütülmüş, adlarıyla; her biri tek bakışta tanınmalı. Tanınmayan ikon için yeniden deneme hakkı kullanılabilir (aynı tek deneme sınırı içinde).
- Bütçe: `cost_max: 30`; 753 + 754 ≤ 50. Aşılacaksa dur, `## Sorular`'a yaz.

## Sonuç

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
