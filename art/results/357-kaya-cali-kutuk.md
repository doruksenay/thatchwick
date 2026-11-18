unity-import: pending

# 357 Arazi engelleri (kaya, çalı, devrik kütük, eski kök kütüğü) (toplu işçi, 2026-11-18)

Claude kullanılmadı: `tools/art/batch_worker.py`, PixelLab REST `POST /map-objects` (kılavuz + maske, 301 yöntemi).
Harcama: **10 generation** (bakiye 4893.3 -> 4883.3, `GET /balance`). Çağrı: 5. Teslim: 1/5. Yan yana: [357-yanyana.png](357-yanyana.png).
Masaüstü kopyası: yerel (betik yapmaz).

## Parçalar

| Parça | Deneme | Sonuç | Stil | Ölçüler | Hata |
|---|---|---|---|---|---|
| boulder_small | 1 | red | KALDI | boyut 80x56, kutu x12-67, y13-43, boy 31, renk 25, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %88, sağ %88, tabana değiyor evet, alt satıra değme hayır | stil kapısı KALDI |
| boulder_small | 2 | red | KALDI | boyut 80x56, kutu x12-67, y8-47, boy 40, renk 40, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %88, sağ %88, tabana değiyor evet, alt satıra değme hayır | stil kapısı KALDI |
| bramble | 1 | red | UYARI (doygunluk x0.37) | boyut 72x64, kutu x12-63, y7-52, boy 46, renk 53, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %88, sağ %75, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| bramble | 2 | red | UYARI (doygunluk x0.37) | boyut 72x64, kutu x12-63, y7-52, boy 46, renk 52, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %88, sağ %75, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| hawthorn | 1 | red | UYARI (doygunluk x0.60) | boyut 72x80, kutu x13-60, y12-63, boy 52, renk 35, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %81, sağ %59, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| hawthorn | 2 | red | UYARI (doygunluk x0.59) | boyut 72x80, kutu x13-60, y12-64, boy 53, renk 54, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %81, sağ %59, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| fallen_log | 1 | TESLİM | UYARI | boyut 112x80, kutu x8-103, y7-72, boy 66, renk 42, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |
| old_stump | 1 | red | KALDI | boyut 80x56, kutu x12-67, y8-47, boy 40, renk 39, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %88, sağ %88, tabana değiyor evet, alt satıra değme hayır | stil kapısı KALDI |
| old_stump | 2 | red | KALDI | boyut 80x56, kutu x12-67, y8-47, boy 40, renk 45, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %88, sağ %88, tabana değiyor evet, alt satıra değme hayır | stil kapısı KALDI |

## Teslim
| Dosya | Pivot (x, y ALTTAN) |
|---|---|
| Assets/Art/Iso/_aday/iso_terrain_fallen_log_2x1_se_nw.png | 56, 32 |

## İncele (otomatik ölçülemeyen)
- Tanınırlık, istenmeyen ayrıntı (zemin lekesi, gölge, ekstra nesne), ışık yönü, ayna uygunluğu: yan yana sayfasında kullanıcı karar verir.

<details><summary>GET /balance ham yanıt</summary>

```
{"credits": {"type": "usd", "usd": 0.0}, "subscription": {"type": "generations", "status": "active", "plan": "Tier 2: Pixel Artisan", "generations": 4883.3, "total": 5000.0}}
```
</details>
