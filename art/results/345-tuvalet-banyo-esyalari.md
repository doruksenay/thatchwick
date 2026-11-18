unity-import: pending

# 345 Lazımlık, yıkanma kulübesi, havluluk (toplu işçi, 2026-11-18)

Claude kullanılmadı: `tools/art/batch_worker.py`, PixelLab REST `POST /map-objects` (kılavuz + maske, 301 yöntemi).
Harcama: **8 generation** (bakiye 4427.9 -> 4419.9, `GET /balance`). Çağrı: 5. Teslim: 2/3. Yan yana: [345-yanyana.png](345-yanyana.png).
Masaüstü kopyası: yerel (betik yapmaz).

## Parçalar

| Parça | Deneme | Sonuç | Stil | Ölçüler | Hata |
|---|---|---|---|---|---|
| chamber_pot | 1 | red | KALDI | boyut 80x64, kutu x19-61, y8-52, boy 45, renk 25, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %66, sağ %69, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az; stil kapısı KALDI |
| chamber_pot | 2 | TESLİM | UYARI | boyut 80x64, kutu x12-67, y5-46, boy 42, renk 34, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %88, sağ %88, tabana değiyor evet, alt satıra değme hayır |  |
| bath_hut | 1 | TESLİM | UYARI | boyut 112x128, kutu x8-103, y8-120, boy 113, renk 25, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %100, sağ %100, tabana değiyor evet, alt satıra değme hayır |  |
| towel_rack | 1 | red | GEÇTİ | boyut 80x88, kutu x24-55, y15-76, boy 62, renk 23, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %41, sağ %31, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |
| towel_rack | 2 | red | GEÇTİ | boyut 80x88, kutu x15-68, y7-78, boy 72, renk 35, yarı saydam 0, maske dışı 0, kılavuz kalıntısı 0, ayak izi kaplaması sol %78, sağ %91, tabana değiyor evet, alt satıra değme hayır | ayak izinin bir yanı %80'den az |

## Teslim
| Dosya | Pivot (x, y ALTTAN) |
|---|---|
| Assets/Art/Iso/_aday/iso_item_chamber_pot_se.png | 40, 24 |
| Assets/Art/Iso/_aday/iso_item_bath_hut_1x2_se.png | 56, 32 |

## İncele (otomatik ölçülemeyen)
- Tanınırlık, istenmeyen ayrıntı (zemin lekesi, gölge, ekstra nesne), ışık yönü, ayna uygunluğu: yan yana sayfasında kullanıcı karar verir.

<details><summary>GET /balance ham yanıt</summary>

```
{"credits": {"type": "usd", "usd": 0.0}, "subscription": {"type": "generations", "status": "active", "plan": "Tier 2: Pixel Artisan", "generations": 4419.9, "total": 5000.0}}
```
</details>
