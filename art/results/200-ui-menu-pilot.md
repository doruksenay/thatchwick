unity-import: pending

# 200 Arayüz menü pilotu
Araç: `create_ui_asset` 256x256, no_background, max_colors 48, seed 0, stil görseli iso_char_villager_se.png, palet tarifteki gibi, 6 parçalı şablon. 2 çağrı x 10 = 20 gen. get_balance sonrası: 4904,3 (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel (bulutta yapılamaz).

Yan yana sayfası: art/results/200-yanyana.png (aile `arayuz`: referans yok, kilit kurulmadı).

| Dosya | Boyut | Pivot | Renk | Saydam / opak / yarı | Köşeler | Stil denetimi |
|---|---|---|---|---|---|---|
| Assets/Art/Iso/_aday/ui_menu_parsomen_1.png | 256x256 | UI, yok | 48 | 11660 / 53876 / 0 | saydam | GEÇTİ (palet %100, uzaklık 9.6) |
| Assets/Art/Iso/_aday/ui_menu_ahsap_1.png | 256x256 | UI, yok | 48 | 29407 / 36129 / 0 | saydam | GEÇTİ (palet %100, uzaklık 5.5) |

Görsel not: yalnızca parşömen çıktısına bakıldı (ahşap önizlemesi araç çıktısında görüldü). Ahşap panel dikey dikdörtgen, kenarlarda saydam boşluk var; parşömen tuvali dolduruyor. Başlık levhası ve 4 düğme şablon yerlerinde, boş; yazı benzeri şekil görülmedi; yarı saydam piksel yok. Ahşap ve parşömende dithering belirgin değil. Kullanıcı yan yana sayfasına bakıp karar verir.
Sorun: yok. validate_iso bu iş için gerekmez.
