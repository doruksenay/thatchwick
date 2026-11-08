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

## Faz K1: Boy ve detay testi (2026-11-08, kuyruk işi 010, status: review)

Ayrıntılı rapor: `art/results/010-karakter-boy-testi.md` (commit de5b254). Araç: `create_character` v3, `view=high top-down`, `outline=single color black outline`, `detail=high detail`.

### Generation dökümü
| Çağrı | Generation |
|---|---|
| v3 size 72 | 2 |
| v3 size 84 | 2 |
| v3 size 96 | 3 |
| **Bu brief toplam** | **7 / 120** |

Kalan bakiye (`get_balance`, iş sonrası): 1928. Hesap ortak: aynı aralıkta 007 (10 gen) ve paralel oturumlar da harcadı.

### SE ölçümleri (kod oturumunda yeniden ölçüldü, işçi raporuyla aynı)
| Hedef boy | Tuval | Gerçek gövde boyu | Genişlik | Renk | Yarı saydam | Pivot x | Pivot y (alttan) |
|---|---|---|---|---|---|---|---|
| 48 | 72x72 | **70** | 38 | 63 | 0 | 36,5 | 1 |
| 56 | 84x84 | **81** | 42 | 47 | 0 | 45,0 | 1 |
| 64 | 96x96 | **86** | 43 | 64 | 0 | 50,5 | 4 |

8 yönde renk sayıları: 48 için 48-64, 56 için 32-48, 64 için 53-64. Yarı saydam piksel hiçbir dosyada yok. `validate_iso.py --kind object` üç SE dosyasında da geçti.

### Detay farkı
| Detay | 48 | 56 | 64 |
|---|---|---|---|
| Yüz | Kaş ve göz okunuyor; burun yalnızca gölgeyle belli; ağız sakalda kayboluyor | Kaş, burun ve ağız çizgisi seçiliyor | Göz bebeği, kaş, burun ve ağız ayrı ayrı okunuyor; en iyisi |
| Eller | Yumruk lekesi | Parmak ipucu var | 2-3 parmak seçiliyor |
| Kemer, toka, kese | Var | Var, ayrıca omuz kayışı | Var, ayrıca omuz kayışı ve bileklik |
| Bacak sargısı | Zayıf okunuyor | Belirgin | Belirgin, şerit dokusu görünüyor |
| Ayakkabı bağı | Yok | Yok | Yok |
| Oran | İri baş, chibi'ye yakın | Gerçekçi | Gerçekçi |

Her boy kendi içinde 8 yönde tutarlı. Ancak seed olmadığı için üç boy birebir aynı tasarım değil; örneğin 64'te tunik daha turuncu, saç daha uzun ve kızıl.

### Sorunlar
1. **Boy hedefi tutmadı (K0 planındaki hata).** K0'da gövde/tuval oranını mevcut köylüden 0,66 varsaydım. v3 ise tuvali neredeyse tamamen dolduruyor (oran 0,90-0,97). Bu yüzden üç aday da hedefin çok üstünde: 70, 81 ve 86 px.
   - Mevcut köylü (45 px) ve 64x32 elmas ölçeğinde bu adaylar çok büyük kalır.
   - 48/56/64 gövde boyu için tuval yaklaşık 52, 60 ve 68 olmalı (tahmin).
   - O boylarda yeniden test: 2 + 2 + 2 = 6 gen, yeni karakter işi 011.
2. **Renk sayısı 32'nin üstünde** (yalnızca 56'nın S yönü 32'de kalıyor). 32 renk şartı K2'de de geçerli kalacaksa iki yol var:
   - `create_character_pro_flash` ile `max_colors=32`: 6 gen/karakter.
   - Ayrı bir renk azaltma adımı.
3. **Pivot y yöne göre 1-3 px değişiyor.** Arka yönler (n, nw, ne) daha yukarıda bitiyor. Unity'de yön başına pivot gerekir.
4. **Ayakkabı bağı** hiçbir boyda çizilmedi.

### Dosyalar
- Proje: `Assets/Art/Iso/_aday/iso_char_farmer_a_{48,56,64}_cand_{se,sw,nw,ne,s,w,n,e}.png` (24 PNG). Paralel oturumun 5703540 commit'i bunları `.meta`'larıyla birlikte erkenden commit etti; içerik işçinin ürettiğiyle bayt bayt aynı.
- Masaüstü:
  - `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_farmer_a_{48,56,64}\` (her birinde 8 PNG ve `_onizleme_x4.png`)
  - `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_K1_toplu_x4.png`
  - `_indeks.md`'ye 3 satır eklendi. Bu satırlardaki "38 gen." gibi notlar genişlik demek, generation değil.
