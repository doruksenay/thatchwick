unity-import: pending

# 812 HUD meşe2 ikonlar

- Araç: `create_image_pixen` 16x16, seed 0 (yeniden denemede seed 1), `no_background`, outline "single color outline". Pixen `init_image`/palet almaz; kılavuzsuz, yalnızca metinle üretildi (tarifteki kılavuz adımı uygulanamadı). Hız ve mevsim ikonları 16 tuvalde ortadaki 12x12'ye kırpıldı (maske dışı silme, yeniden boyama yok).
- Harcanan: 17 gen (15 + 2 yeniden deneme: need_hygiene ve season_autumn ilk denemede palet KALDI, ikincisi GEÇTİ). Kalan get_balance: 9966 (diğer şeritlerle paylaşımlı).
- Yan yana: art/results/812-yanyana.png; 1x/2x/3x: art/results/812-ikonlar-x3.png
- Stil denetimi (arayuz): 13 GEÇTİ, 2 UYARI (season_spring ortalama uzaklık 12.2; speed_3 12.2, %95); KALDI yok.
- Sorun: season_winter içeriği 12x12'ye sığmadı (y 1-15), kırpılmadı, 16x16 teslim. Pivot tarifte yok (UI ikonu): pivot merkez.
- Ham ilk denemeler (hygiene, autumn) teslim edilmedi. Tanınırlık kullanıcının görsel kararına bağlı.
- Masaüstü kopyası: yerel.

| Dosya | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/ui_hud2_mese2_need_energy.png | 16x16 | 8 | 8 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_need_fun.png | 16x16 | 8 | 8 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_need_hunger.png | 16x16 | 8 | 8 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_need_hygiene.png | 16x16 | 8 | 8 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_need_social.png | 16x16 | 8 | 8 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_need_toilet.png | 16x16 | 8 | 8 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_need_warmth.png | 16x16 | 8 | 8 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_season_autumn.png | 12x12 | 6 | 6 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_season_spring.png | 12x12 | 6 | 6 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_season_summer.png | 12x12 | 6 | 6 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_season_winter.png | 16x16 | 8 | 8 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_speed_1.png | 12x12 | 6 | 6 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_speed_2.png | 12x12 | 6 | 6 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_speed_3.png | 12x12 | 6 | 6 |
| Assets/Art/Iso/_aday/ui_hud2_mese2_speed_pause.png | 12x12 | 6 | 6 |
