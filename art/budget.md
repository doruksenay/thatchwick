# PixelLab bütçesi

Bu dosya işçinin freni ve harcama defteridir. İşçi her iş öncesi okur, her iş sonrası günceller.

## Plan
- Plan: Tier 1 (Pixel Apprentice), aylık 2000 generation.
- Dönem: 2026-11-08 → yenilenme 2026-12-08.
- Başlangıç bakiyesi (2026-11-08, tur 4 Faz A `get_balance`): 1999.

## Kurallar
| Kural | Değer |
|---|---|
| Onaysız tek iş tavanı (`cost_max`) | 25 generation. Üstü için iş dosyasında `approved_by: user` gerekir |
| Çalışma başına tavan | 60 generation, en çok 3 iş |
| Günlük tavan | 150 generation |
| Rezerv | Kalan bakiye (`get_balance`), işten sonra 200'ün altına düşecekse işçi durur |
| Maliyet sapması | Gerçek maliyet `cost_est`'in 2 katını aşarsa işçi durur, `status: failed` yazar |
| Yeniden deneme | Yok. Hata, Tier kısıtı ya da ayak izi/2:1 doğrulaması tutmazsa durur, hatayı aynen yazar |
| Doğruluk kaynağı | Bakiye için her zaman `get_balance`. Bu dosyadaki "kalan" sütunu yalnızca defterdir; çelişirse `get_balance` geçerlidir |

Kuralları gevşetmek ya da sıkılaştırmak kullanıcının kararıdır; işçi bu tabloyu değiştirmez.

## Günlük defter
| Tarih | İş | Araç | Harcanan | Kalan (get_balance) |
|---|---|---|---|---|
| 2026-11-08 | (devir) tur 3-4 Faz A | — | 1 | 1999 |
| 2026-11-08 | 007 duvar malzeme | create_building_kit (materials, 64) | 10 | 1950 |
| 2026-11-08 | 009 katman fizibilite | — (rapor, PixelLab yok) | 0 | 1950 |
| 2026-11-08 | 010 karakter boy testi | create_character v3 (72/84/96) | 7 | 1928 |
| 2026-11-08 | 011 karakter boy testi 2 | create_character v3 (52/60/68) + reduce_colors (32 renk, 3 çağrı) | 6,3 | 1896,7 |
| 2026-11-08 | 100 hayvan pilotu | create_character standard + create_image_pixflux x2 + create_image_pixen + create_character v3 ref x2 + reduce_colors (32 renk, 1 çağrı) | 6,1 | 1890,6 |
| 2026-11-08 | 012 karakter seti K2 | create_character v3 x8 (60/56/40) + reduce_colors (32 renk, 8 çağrı) | 16,8 | 1873,8 |
| 2026-11-08 | 013 yürüme pilotu | animate_character SE: template 1 + v3 1 + skeleton-v3 3 + pixminimax 1 | 6 | 1867,8 |
| 2026-11-08 | 300 eşya grup 1 | create_map_object stil eşleme x14 (8 tabureli + 6 kılavuzlu ikinci deneme); 6/8 teslim | 14 | 1853,8 |
