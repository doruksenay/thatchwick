unity-import: pending

# 357 Arazi engelleri (kaya, çalı, devrik kütük, eski kök kütüğü) (toplu işçi, 2026-11-18)

Claude kullanılmadı: `tools/art/batch_worker.py`, PixelLab REST `POST /map-objects` (kılavuz + maske, 301 yöntemi).
Harcama: **10 generation** (bakiye 4955.0 -> 4945.0, `GET /balance`). Çağrı: 14. Teslim: 2/7. Yan yana: [357-yanyana.png](357-yanyana.png).
Masaüstü kopyası: yerel (betik yapmaz).

## Parçalar

| Parça | Deneme | Sonuç | Stil | Ölçüler | Hata |
|---|---|---|---|---|---|
| boulder_small | 1 | red | - |  | GET /map-objects/6495b804-c5b6-4ef5-9f0d-0f9028f0a8ed -> HTTP 423: {"detail":"Map object is still being generated (3% complete). ETA: 86 seconds."} |
| boulder_small | 2 | red | KALDI | boyut 80x56, kutu x12-67, y8-47, boy 40, renk 47, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %88, sağ %88, tabana değiyor evet, alt satıra değme hayır | stil kapısı KALDI |
| boulder | 1 | red | - |  | GET /map-objects/180c6a03-7b9b-483a-8ce5-b04989c4b72b -> HTTP 423: {"detail":"Map object is still being generated (3% complete). ETA: 86 seconds."} |
| boulder | 2 | TESLİM | UYARI | boyut 80x72, kutu x8-71, y7-64, boy 58, renk 12, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |
| boulder_large | 1 | red | - |  | GET /map-objects/bc41aec7-60dd-4520-8fb6-2f3bbc68ca3f -> HTTP 423: {"detail":"Map object is still being generated (3% complete). ETA: 87 seconds."} |
| boulder_large | 2 | TESLİM | UYARI | boyut 112x96, kutu x8-103, y7-88, boy 82, renk 17, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |
| bramble | 1 | red | - |  | GET /map-objects/9a9c08fb-028f-40bc-8284-3e2a561f1c60 -> HTTP 423: {"detail":"Map object is still being generated (3% complete). ETA: 87 seconds."} |
| bramble | 2 | red | - |  | GET /map-objects/ba8e84f9-61f2-4d82-bb5a-de842a76cde5 -> HTTP 423: {"detail":"Map object is still being generated (4% complete). ETA: 86 seconds."} |
| hawthorn | 1 | red | - |  | GET /map-objects/eba4c4fd-e8a7-4f39-b874-6a9fcaf435e1 -> HTTP 423: {"detail":"Map object is still being generated (2% complete). ETA: 87 seconds."} |
| hawthorn | 2 | red | - |  | GET /map-objects/657600dd-4aa2-433a-b16d-aac794ab8fac -> HTTP 423: {"detail":"Map object is still being generated (4% complete). ETA: 86 seconds."} |
| fallen_log | 1 | red | - |  | GET /map-objects/5b7d1dac-9e6c-4b7f-86fb-2df82691f9ed -> HTTP 423: {"detail":"Map object is still being generated (2% complete). ETA: 87 seconds."} |
| fallen_log | 2 | red | - |  | GET /map-objects/4da9bb14-9ebe-41df-9ce9-4148be05a9f1 -> HTTP 423: {"detail":"Map object is still being generated (4% complete). ETA: 86 seconds."} |
| old_stump | 1 | red | - |  | GET /map-objects/6b0e5196-1659-4e3e-84df-1c67c9d70ab2 -> HTTP 423: {"detail":"Map object is still being generated (2% complete). ETA: 87 seconds."} |
| old_stump | 2 | red | - |  | GET /map-objects/06b1dd2c-7a6c-4061-b05c-2183f79dec5e -> HTTP 423: {"detail":"Map object is still being generated (4% complete). ETA: 86 seconds."} |

## Teslim
| Dosya | Pivot (x, y ALTTAN) |
|---|---|
| Assets/Art/Iso/_aday/iso_terrain_boulder_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_terrain_boulder_large_2x1_se_nw.png | 56, 32 |

## İncele (otomatik ölçülemeyen)
- Tanınırlık, istenmeyen ayrıntı (zemin lekesi, gölge, ekstra nesne), ışık yönü, ayna uygunluğu: yan yana sayfasında kullanıcı karar verir.

<details><summary>GET /balance ham yanıt</summary>

```
{"credits": {"type": "usd", "usd": 0.0}, "subscription": {"type": "generations", "status": "active", "plan": "Tier 2: Pixel Artisan", "generations": 4945.0, "total": 5000.0}}
```
</details>
