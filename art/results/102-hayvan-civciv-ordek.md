unity-import: pending

# 102 Hayvanlar 2: civciv + ördek — BLOCKED (döndürme yapılamadı)

## Tuval hesabı (Kural T, r=0,90, 101 raporu)
Civciv 13/0,90=14,4 → alt sınır 16. Ördek 21/0,90=23,3 → 24.

## Üretim
8 x `create_image_pixen` (1 gen): no_background, south, high top-down, tek renk siyah dış hat, medium detail. Üç çağrı (duck_3, duck_4) 10/10 iş hız sınırına takıldı; bekleyip yeniden denendi (duck_3 iki kez takıldı, sonra geçti). Toplam 8 gen. `get_balance` öncesi 4904,30, sonrası 4883,30 (diğer şeritler de harcıyor).

## Güney kontrolü (görsel)
Hepsi tam önden; ayak altında zemin/gölge yok; arka plan saydam.
| Aday | Tuval | bbox (en x boy) | Doluluk |
|---|---|---|---|
| chick_1 | 16 | 12x14 | 0,88 |
| chick_2 | 16 | 10x12 | 0,75 |
| chick_3 | 16 | 12x12 | 0,75 |
| chick_4 | 16 | 10x12 | 0,75 |
| duck_1 | 24 | 14x20 | 0,83 |
| duck_2 | 24 | 14x21 | 0,88 |
| duck_3 | 24 | 18x20 | 0,83 |
| duck_4 | 24 | 14x21 | 0,88 |
Civciv hedefi 13: chick_1 14 (+8%), diğerleri 12 (-8%). Ördek hedefi 21: 20-21 (en çok -5%).

## Engel
v3 döndürme referansı ≥32 px ister; 16 ve 24 px reddedildi (ücret yok). 8 yön üretilmedi, `Assets/Art/Iso`'ya dosya konmadı, stil denetimi ve yan yana sayfası yapılmadı. Ayrıntı: iş dosyası `## Sorular`.
Tek kareler repoya kopyalanamadı (kabuk kopyası onay istedi); PixelLab job id'leriyle alınabilir (`.../mcp/images/<id>/download`): chick_1 7359da98-ce91-494d-8dbe-c697e4ec1c61, chick_2 0b240c68-e903-4896-a592-c0831ffe1772, chick_3 3a2e8c84-5543-408e-b560-ec10d703bcb1, chick_4 84f586d1-b62e-4d8b-a0f8-6125b6aac3dd, duck_1 7173cef7-2d32-47ee-bfb3-9013e3e650e7, duck_2 28f75550-1004-4805-9fe0-22dec266ee12, duck_3 d9c90ae3-f908-4f0f-ac54-661889e88258, duck_4 f32ea482-5904-43bd-baa3-01a5bcc3de48. Masaüstü kopyası: yerel.
