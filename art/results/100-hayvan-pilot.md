unity-import: pending
# 100 Hayvan pilotu: sonuç

- İş: 100, durum `review`. Kullanıcı yolu ve ölçeği seçecek; 101+ işleri bu işin `accepted` olmasından sonra yazılır.
- Tarih: 2026-11-08.
- Harcanan: **6,1 generation** (cost_est 8, cost_max 12). `get_balance` öncesi 1896,7, sonrası **1890,6**; fark tam olarak bu işin çağrıları (eşzamanlı harcama yok).
- Bugünkü toplam kullanım (dönem bugün başladı): 109,4 / 150 günlük tavan.

## Araç ve ayarlar
Ortak: `view="high top-down"` (2:1'e en yakın), `outline="single color black outline"`, her açıklamaya ortak ek (`, earthy muted medieval palette, flat simple shading, light from top-left, no cast shadow, no background, no dithering, no noise`). Maliyetler çağrıdan önce araç açıklamalarından okundu, `cost_est` ile uyumluydu.

| Aday | Yol | Çağrı | Ayar | Id | Gen |
|---|---|---|---|---|---|
| sheep_1 | A | create_character standard | quadruped, template dog, 8 yön, size 40 (tuval 56x56), medium detail, basic shading | a4dcd840-8612-4a08-a664-b103121744a3 | 1 |
| sheep_2 | B | create_image_pixflux | 48x48, no_background, direction south, color_image_base64 = iso_char_villager_se.png, medium detail, basic shading | dec2669b-bf88-45bf-8a47-83af2ec4166a | 1 |
| sheep_2 | B | create_character v3 (referans) | quadruped, template dog, 8 yön, reference_image_url = pixflux indirme URL'si, size verilmedi (48x48 kaldı) | 8cb0487f-51f1-45da-9832-48de507c5009 | 1 |
| chicken_1 | B' | create_image_pixen | 32x32, no_background, direction south, medium detail | 9572bd07-196d-4aa1-8055-8c9ba2050c4b | 1 |
| chicken_1 | B' | create_character v3 (referans) | humanoid, 8 yön, reference_image_url = pixen indirme URL'si, size verilmedi (32x32) | d2dbaa1a-ef09-459e-9dca-8feb347cfbe9 | 1 |
| chicken_1 | B' | reduce_colors | 8 yön tek çağrıda, num_colors 32, dithering none | a40fa261-b69f-4586-9531-e1a6e8bc9024 | 0,1 |
| chicken_2 | B | create_image_pixflux | 32x32, no_background, direction south, color_image_base64 = iso_char_villager_se.png, medium detail, basic shading | 4a61dada-99ad-4aa3-879f-a45deefaa1b2 | 1 |
| chicken_2 | B | v3 döndürme | **yapılmadı** (güney kontrolü) | — | 0 |
| **Toplam** | | | | | **6,1** |

## Güney kontrolü (B ve B')
| Aday | Tek kare | Sonuç |
|---|---|---|
| sheep_2 | Yüz, gözler ve kulaklar simetrik, tam önden; gövde başın arkasında hafif sağa taşıyor, üç bacak görünüyor. | **Geçti** (yakın önden). Not: gövde ~3 px sağa kayık; döndürme sonucu 8 yön doğru etiketli çıktı (aşağıda). |
| chicken_1 | Tam önden, simetrik (ibik, gaga, sakal, iki ayak). | **Geçti.** |
| chicken_2 | Yandan (doğuya bakan) tavuk, 3/4 bile değil. | **Güney değil, döndürülmedi.** Tek kare `_ham` olarak masaüstünde. Yeniden deneme yok. |

## Renk azaltma
| Aday | Ham ortak renk (8 yön) | İşlem | Sonuç |
|---|---|---|---|
| sheep_1 | 10 | gerekmedi | — |
| sheep_2 | 19 | gerekmedi | — |
| chicken_1 | 60 | reduce_colors, 8 kare birlikte, 32 renk, dithering yok | ortak palet 32; kare başına 22-30 renk; alfa 8 karenin hepsinde hamla bayt bayt aynı; değişen opak piksel kare başına 164-246 |
| chicken_2 | 18 (tek kare) | döndürülmediği için yapılmadı | — |

**Bozulma denetimi (chicken_1, SE ham ve azaltılmış 8 kat):** dış hat kesintisiz, göz (siyah + beyaz parlama) duruyor, gaga turuncu, ibik ve sakal kırmızısı ayrı. Yakın krem tonlar birleşti; banding yok. **Azaltma bozmadı.**

## Dosyalar
Hiçbir `.meta` yazılmadı. Proje kopyası `cp -n`.

**Proje (`Assets/Art/Iso/_aday/`), 16 dosya:**
| Dosya | Boyut | Pivot x (SE) | Pivot y (SE, ALTTAN) |
|---|---|---|---|
| iso_animal_sheep_1_cand_{se,sw,nw,ne,s,w,n,e}.png (ham = teslim, azaltma gerekmedi) | 56x56 | 29.0 | 14 |
| iso_animal_sheep_2_cand_{se,sw,nw,ne,s,w,n,e}.png (ham = teslim, azaltma gerekmedi) | 48x48 | 23.0 | 5 |

**Teslim edilmeyenler:**
- `iso_animal_chicken_1_cand_*`: **teslim edilmedi.** Tek karede ve döndürülmüş 8 yönün hepsinde ayakların altında opak, pişmiş bir kahverengi toprak/gölge lekesi var (yaklaşık 20x3 px). Yönerge gölge pişirilmemiş, arka plan saydam ister; maske dışı silme dışında düzeltme yetkim yok ve leke opak sprite'ın parçası. Masaüstünde (azaltılmış + ham) duruyor. Bkz. Sorular 1.
- `iso_animal_chicken_2_cand_*`: döndürülmedi (güney değil).

Pivot yöntemi 010/011 ile aynı (`tools/art/char_preview.py measure`): y = tuval yüksekliği - 1 - en alt opak satır; x = en alttan boyun %10'u kadar bantta opak sütunların ortası.

**Masaüstü (kullanıcının açık izni; silme ya da üzerine yazma yok):**
- `C:\Users\User\OneDrive\Desktop\art\hayvan\2026-11-08_sheep\`: sheep_1 ve sheep_2 8 yön; `_ham\` altında ham 8 yön (ikisi) ve `iso_animal_sheep_2_cand_tek_referans.png`; `_onizleme_x4.png`.
- `C:\Users\User\OneDrive\Desktop\art\hayvan\2026-11-08_chicken\`: chicken_1 azaltılmış 8 yön; `_ham\` altında chicken_1 ham 8 yön, `iso_animal_chicken_1_cand_tek_referans.png`, `iso_animal_chicken_2_cand_tek_guney_degil.png`; `_onizleme_x4.png`.
- `C:\Users\User\OneDrive\Desktop\art\hayvan\2026-11-08_100_toplu_x4.png`: her tür bir satır, satır başında köylü SE.
- Önizlemeler: `#333333` zemin, en yakın komşu 4 kat, her hücrede 64x32 elmas konturu (1 px açık gri), sprite'ın ayak altı (pivot) elmas merkezinde, hücreler alt hizalı, altında aday adı. chicken_2 için SE olmadığından tek kare (doğuya bakan) kondu ve etiketlendi.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md`: sonuna 4 satır (her aday bir satır).
- Betikler geçici klasörde; repoya eklenmedi.

## Doğrulama: SE ve hedef
`uv run --with pillow python tools/art/validate_iso.py <SE> --kind object`: sheep_1 SE, sheep_2 SE, chicken_1 SE (ham ve azaltılmış) **geçti** (yarı saydam 0). Not: validator chicken_1'deki toprak lekesini yakalamaz; görsel denetimle bulundu.

| Aday | SE boy (px) | Hedef boy | Sapma | SE uzunluk (px) | Hedef uzunluk | Sapma | Tuval/size önerisi (tahmin) |
|---|---|---|---|---|---|---|---|
| sheep_1 (A) | 25 | 28-30 (29) | **-14%** | 28 | ~40 | **-30%** | size ~46-48 (standard'da boy size'ın ~%62'si); uzunluk yine kısa kalabilir |
| sheep_2 (B) | 38 | 28-30 (29) | **+31%** | 34 | ~40 | -15% | pixflux tuvali 36x36 ya da 40x40 (boy tuvalin ~%79'u); E/W uzunluk 38 → ~29-31 |
| chicken_1 (B') | 28 (leke dahil; gövde ~26) | 18-20 (19) | **+47%** (gövde ~+37%) | 21 | — | — | pixen 24x24 (pixen 32 altında kare ister: 24 ya da 20) |
| chicken_2 (B, tek kare) | 31 | 18-20 | +63% | 26 | — | — | pixflux 32 alt sınır alan 32x32; daha küçük için 24x48 gibi dikdörtgen gerekir, ya da pixen |
| köylü (ref.) | 45 | | | 17 | | | 68x68, pivot (34.5, 12) |

Yeniden örnekleme yapılmadı.

## Doğrulama: 8 yön
Her hücre: boy / genişlik / renk / yarı saydam / pivot x, pivot y (alttan).

| Aday | se | sw | nw | ne | s | w | n | e |
|---|---|---|---|---|---|---|---|---|
| sheep_1 (56x56) | 25/28/10/0/29,14 | 25/28/10/0/28,15 | 35/30/10/0/39,9 | 35/30/10/0/17,9 | 31/16/10/0/28,15 | 26/36/10/0/34,15 | 37/16/10/0/28,8 | 26/35/10/0/22,15 |
| sheep_2 (48x48) | 38/34/18/0/23,5 | 38/39/15/0/25.5,5 | 38/34/15/0/25,5 | 38/36/15/0/22.5,5 | 38/32/15/0/26,5 | 38/38/17/0/25.5,5 | 38/30/12/0/24,5 | 38/38/18/0/22.5,5 |
| chicken_1 ham (32x32) | 28/21/50/0/16,2 | 28/21/47/0/16,2 | 28/20/35/0/15.5,2 | 28/20/36/0/16,2 | 28/23/49/0/16,2 | 28/22/48/0/16,2 | 28/19/34/0/15.5,2 | 28/22/44/0/16,2 |
| chicken_1 azaltılmış | 28/21/30/0/16,2 | 28/21/27/0/16,2 | 28/20/22/0/15.5,2 | 28/20/23/0/16,2 | 28/23/27/0/16,2 | 28/22/30/0/16,2 | 28/19/23/0/15.5,2 | 28/22/28/0/16,2 |

**Gözlem:**
- **sheep_1 (A):** 8 yön aynı koyun, yönler doğru. Boy yöne göre 25-37 oynuyor (n/ne/nw'de koyun dikine uzanıyor), pivot y 8-15 arası: tek pivotla arka yönler 5-7 px kayar. Çok az detay (10 renk), yün düz beyaz; köylünün detay seviyesinin altında. Gölge yok, zemin saydam, ışık belirsiz (düz).
- **sheep_2 (B):** 8 yön aynı koyun, yönler doğru (s önden, se/sw 3/4, e/w profil, n arkadan). Boy ve pivot y 8 yönde sabit (38, 5): Unity'de tek pivot yeter. Palet zorlama köylüyle uyumlu toprak tonları verdi. Sorunlar: gözler **sarı** (parlayan göz etkisi; "dark brown face" isteğine rağmen), n/nw/ne'de kuyruk yerine kontur içinde bir halka çizgisi var. Gölge yok, zemin saydam, ışık üst-solda (yün üst ve sol açık).
- **chicken_1 (B'):** 8 yön aynı tavuk, yönler doğru. Boy ve pivot sabit (28, 2). **Insanlaşma yok:** humanoid iskelet kol, dik gövde ya da insan bacağı eklemedi; gövde yuvarlak, kanat gövdeye yapışık, kuyruk arkada. Tek sorun pişmiş toprak lekesi (yukarıda).
- **chicken_2 (B):** palet zorlama ile pixflux, "seen straight from the front" ve `direction=south`a rağmen yandan çizdi.

## Rapora eklenecek öneriler
### 1. Yol önerisi
- **Dört ayaklılar: B** (pixflux + palet zorlama + v3 referans, quadruped). Gerekçe: detay ve renk köylüyle uyumlu, 8 yönde boy/pivot sabit, 2 gen. Ölçek tuvalle ayarlanmalı (koyun için 36-40). Tek örnekte güney kontrolü geçti ama sınırdaydı; toplu işte başarısızlık payı bırakılmalı. A (1 gen) ucuz ama çok küçük, detaysız ve pivotu yöne göre 7 px oynuyor; yalnızca uzak/küçük yaban hayvanları için yedek.
- **Kuşlar: B'** (pixen + v3 humanoid). Gerekçe: pixen tam önden poz verdi, humanoid iskelet kuşu insanlaştırmadı, 8 yön tutarlı. B (pixflux + palet) kuşta yandan çizdi (1 örnek). Pixen'in palet zorlaması yok; renk uyumu reduce_colors (32) ile sağlanıyor. Çözülmesi gereken: ayak altı toprak lekesi (Sorular 1).
- **C (`create_character_pro_flash`, 6 gen) ya da D (`create_8_direction_object`, 10 gen):** bu pilotta gerekmedi. B/B' toplu işte güney kontrolünde sık düşerse (ör. adayların %30'undan fazlası), dört ayaklılarda C'nin bir türde denenmesi önerilir. D şimdilik gereksiz.

### 2. Toplu işlerin günlere bölünmesi (Öncelik 1: 12 tür)
Varsayım: aday başına ~2,1 gen (1 tek kare + 1 v3 + 0,1 azaltma); güneyde düşen aday 1 gen yakar. Tür başına 4 aday ≈ 8,4 gen; 2 tür/iş ≈ 17 gen, `cost_max` 20 (≤ 25).

| İş | Türler | Yol | Aday | cost_est | cost_max |
|---|---|---|---|---|---|
| 101 | chicken, rooster | B' | 4+4 | 17 | 20 |
| 102 | chick, duck | B' | 4+4 | 17 | 20 |
| 103 | goose, sheep | B' / B | 4+4 | 17 | 20 |
| 104 | cow, horse | B (tuval 64, v3 yine 1 gen) | 4+4 | 17 | 20 |
| 105 | pig, goat | B | 4+4 | 17 | 20 |
| 106 | dog, cat | B | 4+4 | 17 | 20 |

- **Gün 1:** 101, 102, 103 (tek çalışma: 3 iş, ~51 gen ≤ 60).
- **Gün 2:** 104, 105, 106 (~51 gen).
- Günlük hayvan payı ~51 gen; 150 günlük tavanın ~100'ü diğer oturumlara (zemin, eşya, karakter) kalır. 5 aday istenirse tür başına ~10,5 gen, iş başına ~21 (cost_max 25) olur; o zaman günde 2 iş (~42 gen) ve 3 gün.
- Bütçe kuralı gevşetme önerisi yok; mevcut tavanlar bu plana yetiyor.

## Sorular
1. **chicken_1 toprak lekesi:** pixen tek karesi ayak altında opak kahverengi bir toprak/gölge lekesi çizdi; v3 bunu 8 yöne taşıdı. Yönerge gereği Assets'e teslim etmedim (masaüstünde var). Leke piksellerini maske ile silmeme (yeniden boyamadan, yalnızca lekeyi saydam yapma) izin verilir mi? Ya da 101+ işlerinde pixen açıklamasına "no ground, no shadow under feet" eklensin mi?
2. **sheep_2 sarı gözler** ve arka yönlerdeki halka çizgisi kabul edilebilir mi, yoksa 101+ için açıklamaya "small black eyes" eklensin mi?
3. **sheep_2 güney kontrolü sınırdaydı** (yüz tam önden, gövde hafif sağa). Bu toleransı doğru buldum; toplu işlerde aynı ölçüt mü uygulansın, yoksa gövde de simetrik olmalı mı?
4. Hedef boy için tuval önerileri (koyun pixflux 36-40, tavuk pixen 24) tahmindir; 101'de tek bir türde doğrulanması önerilir.

## Sorunlar
- Yeniden deneme yapılmadı. Hiçbir çağrı hata vermedi.
- chicken_2 güney kontrolünden döndü (1 gen harcandı, döndürme yapılmadı).
