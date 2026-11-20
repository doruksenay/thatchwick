unity-import: pending

# 344 Yayık, taş yontma tezgâhı, tuğla fırını, boya kazanı ve 7 düzenek (toplu işçi, 2026-11-20)

Claude kullanılmadı: `tools/art/batch_worker.py`, PixelLab REST `POST /map-objects` (kılavuz + maske, 301 yöntemi).
Harcama: **15 generation** (bakiye 2935.9 -> 2920.9, `GET /balance`). Çağrı: 15. Teslim: 8/11. Yan yana: [344-yanyana.png](344-yanyana.png).
Masaüstü kopyası: yerel (betik yapmaz).

## Parçalar

| Parça | Deneme | Sonuç | Stil | Ölçüler | Hata |
|---|---|---|---|---|---|
| churn | 1 | TESLİM | GEÇTİ | boyut 80x96, kutu x22-65, y21-88, boy 68, renk 32, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %56, sağ %56, tabana değiyor evet, alt satıra değme hayır |  |
| stonecutter | 1 | TESLİM | GEÇTİ | boyut 112x104, kutu x9-102, y19-96, boy 78, renk 31, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %94, sağ %98, tabana değiyor evet, alt satıra değme hayır |  |
| kiln | 1 | TESLİM | GEÇTİ | boyut 144x136, kutu x8-135, y15-128, boy 114, renk 11, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |
| dye_vat | 1 | red | KALDI | boyut 80x88, kutu x9-70, y19-80, boy 62, renk 49, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %91, sağ %91, tabana değiyor evet, alt satıra değme hayır | stil kapısı KALDI |
| dye_vat | 2 | TESLİM | GEÇTİ | boyut 80x88, kutu x8-71, y19-80, boy 62, renk 43, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |
| windmill | 1 | red | GEÇTİ | boyut 144x184, kutu x19-115, y19-150, boy 132, renk 17, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %47, sağ %45, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| windmill | 2 | red | UYARI | boyut 144x184, kutu x24-132, y29-151, boy 123, renk 47, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %75, sağ %80, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| water_gate | 1 | TESLİM | UYARI | boyut 80x80, kutu x9-70, y15-72, boy 58, renk 26, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %97, sağ %97, tabana değiyor evet, alt satıra değme hayır |  |
| noria | 1 | red | UYARI | boyut 144x144, kutu x22-110, y19-121, boy 103, renk 25, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %78, sağ %61, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| noria | 2 | red | GEÇTİ | boyut 144x144, kutu x36-112, y21-108, boy 88, renk 35, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %56, sağ %64, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| feed_mixer | 1 | red | GEÇTİ | boyut 80x96, kutu x15-64, y21-81, boy 61, renk 17, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %69, sağ %69, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| feed_mixer | 2 | red | GEÇTİ | boyut 80x96, kutu x14-64, y21-82, boy 62, renk 19, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %81, sağ %78, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| harvest_rake | 1 | TESLİM | UYARI | boyut 80x88, kutu x8-71, y19-80, boy 62, renk 12, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |
| seeder | 1 | TESLİM | GEÇTİ | boyut 80x88, kutu x10-69, y19-76, boy 58, renk 25, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %94, sağ %94, tabana değiyor evet, alt satıra değme hayır |  |
| ice_well | 1 | TESLİM | UYARI | boyut 144x120, kutu x25-118, y28-99, boy 72, renk 47, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %73, sağ %73, tabana değiyor evet, alt satıra değme hayır |  |

## Teslim
| Dosya | Pivot (x, y ALTTAN) |
|---|---|
| Assets/Art/Iso/_aday/iso_item_churn_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_stonecutter_2x1_se.png | 56, 32 |
| Assets/Art/Iso/_aday/iso_item_kiln_2x2_se.png | 72, 40 |
| Assets/Art/Iso/_aday/iso_item_dye_vat_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_water_gate_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_harvest_rake_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_seeder_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_ice_well_2x2_se.png | 72, 40 |

## İncele (otomatik ölçülemeyen)
- Tanınırlık, istenmeyen ayrıntı (zemin lekesi, gölge, ekstra nesne), ışık yönü, ayna uygunluğu: yan yana sayfasında kullanıcı karar verir.

<details><summary>GET /balance ham yanıt</summary>

```
{"credits": {"type": "usd", "usd": 0.0}, "subscription": {"type": "generations", "status": "active", "plan": "Tier 2: Pixel Artisan", "generations": 2920.9, "total": 5000.0}}
```
</details>
