unity-import: pending

# 778 Simge yapılar (619'un toplu işçiyle yeniden çalışması; 9 eşya) (toplu işçi, 2026-11-20)

Claude kullanılmadı: `tools/art/batch_worker.py`, PixelLab REST `POST /map-objects` (kılavuz + maske, 301 yöntemi).
Harcama: **10 generation** (bakiye 458.60000000000036 -> 448.60000000000036, `GET /balance`). Çağrı: 10. Teslim: 9/9. Yan yana: [778-yanyana.png](778-yanyana.png).
Masaüstü kopyası: yerel (betik yapmaz).

## Parçalar

| Parça | Deneme | Sonuç | Stil | Ölçüler | Hata |
|---|---|---|---|---|---|
| boundary_stone | 1 | TESLİM | UYARI | boyut 80x112, kutu x11-68, y7-104, boy 98, renk 28, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %91, sağ %91, tabana değiyor evet, alt satıra değme hayır |  |
| lightning_oak | 1 | TESLİM | GEÇTİ (doygunluk x0.72) | boyut 160x192, kutu x11-135, y18-169, boy 152, renk 16, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %56, sağ %61, tabana değiyor evet, alt satıra değme hayır |  |
| hunter_hut_ruin | 1 | TESLİM | GEÇTİ | boyut 160x144, kutu x9-134, y14-136, boy 123, renk 29, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %98, sağ %98, tabana değiyor evet, alt satıra değme hayır |  |
| sunken_boat | 1 | TESLİM | GEÇTİ | boyut 112x80, kutu x8-97, y13-70, boy 58, renk 51, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %88, tabana değiyor evet, alt satıra değme hayır |  |
| broken_boardwalk | 1 | TESLİM | GEÇTİ | boyut 112x72, kutu x26-88, y9-56, boy 48, renk 26, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %62, sağ %69, tabana değiyor evet, alt satıra değme hayır |  |
| watchtower_ruin | 1 | TESLİM | GEÇTİ (doygunluk x0.72) | boyut 160x224, kutu x12-135, y22-216, boy 195, renk 59, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %94, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |
| old_quarry_cleft | 1 | TESLİM | UYARI | boyut 160x128, kutu x10-133, y7-120, boy 114, renk 37, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %97, sağ %97, tabana değiyor evet, alt satıra değme hayır |  |
| vineyard_house_ruin | 1 | TESLİM | UYARI | boyut 160x160, kutu x8-135, y24-152, boy 129, renk 56, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |
| beacon_fire_place | 1 | red | KALDI | boyut 80x96, kutu x9-70, y17-84, boy 68, renk 34, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 35, ayak izi kaplaması sol %97, sağ %97, tabana değiyor evet, alt satıra değme hayır | kılavuz rengi kalıntısı 35 px; stil kapısı KALDI |
| beacon_fire_place | 2 | TESLİM | GEÇTİ | boyut 80x96, kutu x9-70, y18-78, boy 61, renk 43, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %97, sağ %97, tabana değiyor evet, alt satıra değme hayır |  |

## Teslim
| Dosya | Pivot (x, y ALTTAN) |
|---|---|
| Assets/Art/Iso/_aday/iso_item_boundary_stone_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_lightning_oak_2x2_se.png | 72, 40 |
| Assets/Art/Iso/_aday/iso_item_hunter_hut_ruin_2x2_se.png | 72, 40 |
| Assets/Art/Iso/_aday/iso_item_sunken_boat_2x1_se_nw.png | 56, 32 |
| Assets/Art/Iso/_aday/iso_item_broken_boardwalk_2x1_se_nw.png | 56, 32 |
| Assets/Art/Iso/_aday/iso_item_watchtower_ruin_2x2_se.png | 72, 40 |
| Assets/Art/Iso/_aday/iso_item_old_quarry_cleft_2x2_se.png | 72, 40 |
| Assets/Art/Iso/_aday/iso_item_vineyard_house_ruin_2x2_se.png | 72, 40 |
| Assets/Art/Iso/_aday/iso_item_beacon_fire_place_se.png | 40, 24 |

## İncele (otomatik ölçülemeyen)
- Tanınırlık, istenmeyen ayrıntı (zemin lekesi, gölge, ekstra nesne), ışık yönü, ayna uygunluğu: yan yana sayfasında kullanıcı karar verir.

<details><summary>GET /balance ham yanıt</summary>

```
{"credits": {"type": "usd", "usd": 0.0}, "subscription": {"type": "generations", "status": "active", "plan": "Tier 2: Pixel Artisan", "generations": 448.60000000000036, "total": 5000.0}}
```
</details>
