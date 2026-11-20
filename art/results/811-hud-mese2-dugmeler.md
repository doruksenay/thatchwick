unity-import: pending

# 811 HUD meşe2: Yapı ve Defter düğmeleri, rozet

- Araç: create_image_pixflux 32x32 x2 (btn_build, btn_book), create_image_pixen 16x16 x1 (badge); seed 0, saydam zemin, single color outline. Toplam 3 gen (başlangıç bakiyesi 9997; sonrası okunmadı, defter 9994).
- Kılavuz: `ui_hud2_mese_btn_*` 774 parçaları depoda yok, bu yüzden `init_image` kullanılmadı; yalnızca metin tarifi.
- `_down` hâlleri Python ile (0 gen, tarifin yedek yolu): iç daire %18 koyulaştırıldı, 20x20 ikon bölgesi 1 px aşağı kaydırıldı.
- Rozet: 16x16 çıktının ortasından 10x10 kırpıldı.
- Yan yana: art/results/811-yanyana.png (referans bulunamadı, 0 referans); düzen 1x/2x/3x + rozetli kitap: art/results/811-duzen.png
- Masaüstü kopyası: yerel.

| Dosya | Boyut | Pivot x, y (alttan) | Stil denetimi |
|---|---|---|---|
| ui_hud2_mese2_btn_build.png | 32x32 | 16, 0 | GEÇTİ (%99, 7.7) |
| ui_hud2_mese2_btn_build_down.png | 32x32 | 16, 0 | GEÇTİ (%99, 8.5) |
| ui_hud2_mese2_btn_book.png | 32x32 | 16, 0 | GEÇTİ (%100, 6.6) |
| ui_hud2_mese2_btn_book_down.png | 32x32 | 16, 0 | GEÇTİ (%100, 5.7) |
| ui_hud2_mese2_badge.png | 10x10 | 5, 0 | GEÇTİ (%90, 9.8) |

## Sorunlar
- btn_build: çekiçle birlikte mala yerine yaprak benzeri yeşil şekil çıktı; tanınırlığı kullanıcı değerlendirsin.
- Rozette ünlem 10x10 kırpmada zayıf okunuyor.
- btn_book ortası koyu; kenar oyması belirgin değil.
- validate_iso yalnızca btn_build için çalıştırıldı (geçti).
