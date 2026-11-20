unity-import: pending

# 342 Gölet, balık sepeti, çıkrık, dokuma tezgâhı, imbik, iksir rafı (toplu işçi, 2026-11-20)

Claude kullanılmadı: `tools/art/batch_worker.py`, PixelLab REST `POST /map-objects` (kılavuz + maske, 301 yöntemi).
Harcama: **18 generation** (bakiye 2964.9 -> 2946.9, `GET /balance`). Çağrı: 8. Teslim: 4/6. Yan yana: [342-yanyana.png](342-yanyana.png).
Masaüstü kopyası: yerel (betik yapmaz).

## Parçalar

| Parça | Deneme | Sonuç | Stil | Ölçüler | Hata |
|---|---|---|---|---|---|
| pond | 1 | red | KALDI | boyut 176x112, kutu x10-165, y17-104, boy 88, renk 35, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %98, sağ %98, tabana değiyor evet, alt satıra değme hayır | stil kapısı KALDI |
| pond | 2 | red | KALDI | boyut 176x112, kutu x8-167, y17-104, boy 88, renk 27, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır | stil kapısı KALDI |
| fish_trap | 1 | TESLİM | GEÇTİ (doygunluk x0.72) | boyut 80x80, kutu x12-67, y19-65, boy 47, renk 43, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %88, sağ %88, tabana değiyor evet, alt satıra değme hayır |  |
| spinning_wheel | 1 | TESLİM | GEÇTİ | boyut 80x96, kutu x15-62, y22-87, boy 66, renk 29, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %78, sağ %69, tabana değiyor evet, alt satıra değme hayır |  |
| loom | 1 | TESLİM | GEÇTİ | boyut 112x120, kutu x14-101, y23-106, boy 84, renk 32, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %88, sağ %94, tabana değiyor evet, alt satıra değme hayır |  |
| alembic | 1 | red | GEÇTİ (doygunluk x0.72) | boyut 112x112, kutu x26-85, y25-98, boy 74, renk 40, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %62, sağ %62, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| alembic | 2 | red | GEÇTİ | boyut 112x112, kutu x23-88, y26-92, boy 67, renk 47, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %67, sağ %69, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| potion_shelf | 1 | TESLİM | UYARI (doygunluk x0.76) | boyut 80x96, kutu x8-71, y15-88, boy 74, renk 33, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |

## Teslim
| Dosya | Pivot (x, y ALTTAN) |
|---|---|
| Assets/Art/Iso/_aday/iso_item_fish_trap_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_spinning_wheel_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_loom_2x1_se.png | 56, 32 |
| Assets/Art/Iso/_aday/iso_item_potion_shelf_se.png | 40, 24 |

## İncele (otomatik ölçülemeyen)
- Tanınırlık, istenmeyen ayrıntı (zemin lekesi, gölge, ekstra nesne), ışık yönü, ayna uygunluğu: yan yana sayfasında kullanıcı karar verir.

<details><summary>GET /balance ham yanıt</summary>

```
{"credits": {"type": "usd", "usd": 0.0}, "subscription": {"type": "generations", "status": "active", "plan": "Tier 2: Pixel Artisan", "generations": 2946.9, "total": 5000.0}}
```
</details>
