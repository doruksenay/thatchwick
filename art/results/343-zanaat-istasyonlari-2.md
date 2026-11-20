unity-import: pending

# 343 Taş ocağı, demir ocağı, çömlekçi çarkı, küp, mayalama fıçısı, mum tezgâhı, av pususu, rüzgâr fırıldağı, yazı tahtası, kopuz (toplu işçi, 2026-11-20)

Claude kullanılmadı: `tools/art/batch_worker.py`, PixelLab REST `POST /map-objects` (kılavuz + maske, 301 yöntemi).
Harcama: **11 generation** (bakiye 2946.9 -> 2935.9, `GET /balance`). Çağrı: 11. Teslim: 10/10. Yan yana: [343-yanyana.png](343-yanyana.png).
Masaüstü kopyası: yerel (betik yapmaz).

## Parçalar

| Parça | Deneme | Sonuç | Stil | Ölçüler | Hata |
|---|---|---|---|---|---|
| quarry | 1 | TESLİM | GEÇTİ | boyut 144x112, kutu x9-134, y15-104, boy 90, renk 21, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %98, sağ %98, tabana değiyor evet, alt satıra değme hayır |  |
| forge | 1 | red | KALDI | boyut 144x128, kutu x37-106, y21-114, boy 94, renk 10, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %55, sağ %55, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az; stil kapısı KALDI |
| forge | 2 | TESLİM | GEÇTİ | boyut 144x128, kutu x10-133, y15-120, boy 106, renk 44, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %97, sağ %97, tabana değiyor evet, alt satıra değme hayır |  |
| pottery_wheel | 1 | TESLİM | GEÇTİ (doygunluk x0.70) | boyut 80x88, kutu x8-71, y20-80, boy 61, renk 46, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |
| storage_jar | 1 | TESLİM | GEÇTİ (doygunluk x0.69) | boyut 80x88, kutu x9-70, y17-74, boy 58, renk 25, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %97, sağ %97, tabana değiyor evet, alt satıra değme hayır |  |
| brew_barrel | 1 | TESLİM | GEÇTİ | boyut 80x88, kutu x9-70, y17-80, boy 64, renk 29, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %97, sağ %97, tabana değiyor evet, alt satıra değme hayır |  |
| chandler_table | 1 | TESLİM | GEÇTİ | boyut 80x88, kutu x10-69, y17-78, boy 62, renk 44, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %81, sağ %81, tabana değiyor evet, alt satıra değme hayır |  |
| hunting_blind | 1 | TESLİM | UYARI | boyut 80x96, kutu x9-70, y19-82, boy 64, renk 21, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %91, sağ %81, tabana değiyor evet, alt satıra değme hayır |  |
| weather_vane | 1 | TESLİM | GEÇTİ | boyut 80x120, kutu x16-63, y16-99, boy 84, renk 37, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %9, sağ %9, tabana değiyor evet, alt satıra değme hayır |  |
| slate | 1 | TESLİM | GEÇTİ | boyut 80x96, kutu x14-66, y15-88, boy 74, renk 43, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %62, sağ %84, tabana değiyor evet, alt satıra değme hayır |  |
| lute | 1 | TESLİM | GEÇTİ | boyut 80x96, kutu x23-53, y26-86, boy 61, renk 25, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %53, sağ %44, tabana değiyor evet, alt satıra değme hayır |  |

## Teslim
| Dosya | Pivot (x, y ALTTAN) |
|---|---|
| Assets/Art/Iso/_aday/iso_item_quarry_2x2_se.png | 72, 40 |
| Assets/Art/Iso/_aday/iso_item_forge_2x2_se.png | 72, 40 |
| Assets/Art/Iso/_aday/iso_item_pottery_wheel_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_storage_jar_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_brew_barrel_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_chandler_table_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_hunting_blind_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_weather_vane_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_slate_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_lute_se.png | 40, 24 |

## İncele (otomatik ölçülemeyen)
- Tanınırlık, istenmeyen ayrıntı (zemin lekesi, gölge, ekstra nesne), ışık yönü, ayna uygunluğu: yan yana sayfasında kullanıcı karar verir.

<details><summary>GET /balance ham yanıt</summary>

```
{"credits": {"type": "usd", "usd": 0.0}, "subscription": {"type": "generations", "status": "active", "plan": "Tier 2: Pixel Artisan", "generations": 2935.9, "total": 5000.0}}
```
</details>
