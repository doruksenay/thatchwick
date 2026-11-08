# Art Brief Karakter: Sonuç

## Faz K0: Plan (2026-11-08, harcama 0)

**Bakiye (`get_balance`, 13:37):** 1975 / 2000 generation, Tier 1 (Pixel Apprentice), yenilenme 2026-12-08. Bu brief'te harcanan: 0 / 120.

### Referans ölçüm: mevcut köylü (`iso_char_villager_*`, dokunulmadı)
| Yön | Tuval | Gövde boyu | Genişlik | Renk | Yarı saydam | Ayak altı boşluğu |
|---|---|---|---|---|---|---|
| se | 68x68 | 45 | 17 | 32 | 0 | 12 |
| s | 68x68 | 44 | 20 | 17 | 0 | 13 |
| e | 68x68 | 44 | 15 | 32 | 0 | 11 |
| n | 68x68 | 45 | 20 | 22 | 0 | 13 |

Gövde/tuval oranı yaklaşık 0,66. Hedef boylar için tuval: 48 px boy → 72, 56 px → 84, 64 px → 96. Gerçek boy üretimden sonra ölçülecek.

### Araçlar (açıklamalardan, 2026-11-08)
| Araç | Maliyet (8 yön) | Boyut | Detay ve kontrol | 2:1 uygunluğu |
|---|---|---|---|---|
| `create_character` v3, metinden | 2 (≤90 px), 3 (91-128) | kare, 256'ya kadar | En detaylı. Seed yok. `max_colors` ve palet yok. Dış hat yalnızca ipucu | `view` seçenekleri: low (~20°), **high top-down (~35°)**, side. 30°'ye en yakın high |
| `create_character` v3, referansla | ceil(w·h·8/65536), 64x96 için 1 | referansın boyutu | Verilen güney sprite'ını 8 yöne döndürür | Güney sprite'ı bizim çizmemiz gerekir |
| `create_character` standard | 1 | 128'e kadar (tuval ~%40 büyük) | Şablon iskelet, en az detay. `max_colors` yok | low/high/side |
| `create_character` pro | 10 (≤85), 15 (86-113), 25 (114-128) | 128'e kadar | `max_colors`, `style_character_id` ile set tutarlılığı. Pahalı | high |
| `create_character_pro_flash` | 6 (16-88), 7 (92-96), 8 (100-128). Ücretsiz sorguyla doğrulandı: 84x84 = 5 görüntü + 1 döndürme = 6 | 4'ün katı, özel boyut beta | Seed var, `max_colors` var, `style_image` ile set boyunca stil çapası | low/high/side |

### Plan
1. **K1 aracı:** `create_character` v3, `view=high top-down`, `outline=single color black outline`, `detail=high detail`. Gerekçe: en detaylı çıktı en ucuz fiyata. Brief'in ana hedefi detay, maliyet pro_flash'ın üçte biri.
2. **K1 boyutları:** tuval 72, 84 ve 96. Maliyet 2 + 2 + 3 = **7 generation**. Bu, brief'teki 120'lik tavanın %6'sı.
3. **K1 sınırı:** v3 seed almıyor. Üç boyut üç ayrı çizim olur, aynı tarifle ama yüz ve kıyafet birebir aynı çıkmaz. Boy karşılaştırması için yeterli, piksel düzeyinde birebir karşılaştırma değil.
4. **32 renk sınırı:** v3 bunu zorlayamaz. Her çıktıda renk sayısı ölçülecek. Aşılırsa elle renk azaltma yapılmayacak, rapora yazılacak. K2'de aşım sürerse `pro_flash` + `max_colors=32` seçeneği masada.
5. **K2 aracı (boy kararından sonra):** varsayılan v3, 8 karakter × 2-3 = **16-24 generation**. Seçilen K1 karakteri stil sapması yaparsa fallback: `create_character_pro_flash` + `style_image` = onaylı K1 SE sprite'ı + `max_colors=32`. Bu yolda 8 × 6-7 = **48-56 generation**. İki yol da 120 tavanı altında.
6. **Çocuk ve yaşlı:** v3'te `size` tuvali belirliyor, gövdeyi değil. Kısa boy yalnızca tarifle ("small child, about two thirds adult height") istenecek, sonuç ölçülecek. Tutmazsa çocuk daha küçük tuvalde üretilip seçilen tuvale ortalanarak yerleştirilir: piksel ölçeklemesi yok, yalnızca dolgu.

### Süreç: kuyruğa taşıma ve çelişkiler (karar senin)
- `CLAUDE.md`'ye göre kod oturumu PixelLab'de üretim yapmaz, iş kuyruktan `pixellab-artist` ile yürür. K0'da yalnızca ücretsiz okuma yaptım (`get_balance`, `get_pro_flash_capabilities`). K1'i **`art/queue/010-karakter-boy-testi.md`** olarak ekledim: `approved_by: pending`, onaylamadan çalışmaz.
- **Çelişki 1:** Brief "commit/push yapma" diyor. `CLAUDE.md` ise kuyruk düzeninin bu kısıtın yerine geçtiğini söylüyor; işçi kendi yollarını push eder.
- **Çelişki 2:** Brief masaüstü kopyası istiyor (`C:\Users\User\OneDrive\Desktop\art`). İşçi yalnızca `art/`, `Assets/Art/Iso/` ve `tools/art/` altına yazabiliyor. 010'un tarifine, senin brief'ine dayanarak, açık istisna olarak yazdım.
- **Çelişki 3:** İşçi kuyruğu numara sırasıyla işliyor. 001-008 (zemin, eşya, duvar) hâlâ `pending`. Öte yandan diğer oturum zemini eski brief düzeniyle yürütüyor (`_aday` altında çimen ve toprak adayları var). İşçiyi başlatırsam önce 001 ve 005'i alır ve diğer oturumun işini çift harcamayla tekrarlar.
