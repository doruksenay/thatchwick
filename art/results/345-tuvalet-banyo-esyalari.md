unity-import: pending

# 345 Lazımlık, yıkanma kulübesi, havluluk (toplu işçi, 2026-11-18)

Claude kullanılmadı: `tools/art/batch_worker.py`, PixelLab REST `POST /map-objects` (kılavuz + maske, 301 yöntemi).
Harcama: **1 generation** (bakiye 4378.9 -> 4377.9, `GET /balance`). Çağrı: 1. Teslim: 1/1. Yan yana: [345-yanyana.png](345-yanyana.png).
Masaüstü kopyası: yerel (betik yapmaz).

## Parçalar

| Parça | Deneme | Sonuç | Stil | Ölçüler | Hata |
|---|---|---|---|---|---|
| towel_rack | 1 | TESLİM | GEÇTİ (doygunluk x0.76) | boyut 80x88, kutu x14-65, y7-80, boy 74, renk 35, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %81, sağ %81, tabana değiyor evet, alt satıra değme hayır |  |

## Teslim
| Dosya | Pivot (x, y ALTTAN) |
|---|---|
| Assets/Art/Iso/_aday/iso_item_towel_rack_se.png | 40, 24 |

## İncele (otomatik ölçülemeyen)
- Tanınırlık, istenmeyen ayrıntı (zemin lekesi, gölge, ekstra nesne), ışık yönü, ayna uygunluğu: yan yana sayfasında kullanıcı karar verir.

<details><summary>GET /balance ham yanıt</summary>

```
{"credits": {"type": "usd", "usd": 0.0}, "subscription": {"type": "generations", "status": "active", "plan": "Tier 2: Pixel Artisan", "generations": 4377.9, "total": 5000.0}}
```
</details>
