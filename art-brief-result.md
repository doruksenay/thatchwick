# Art Brief Sonucu: İzometrik varlık seti (A1, tur 3)

## Faz 1: Köylü ve zemin (2026-11-06)

Durum: **Köylü geçti. Çimen karosu başarısız, projeye konmadı.** Onay bekleniyor; Faz 2'ye geçilmedi.

### Harcama

| Çağrı | Araç | Generation |
|---|---|---|
| 1 | `create_character`, `v3` + referans, 8 yön (`06605af9-15dc-42de-b0b3-4294b2bb3bf3`) | 1 |
| 2 | `create_isometric_tile`, 64, `thin tile` (`f7012fe2-d902-497e-ab9b-fc2248a83e65`) | 1 |
| **Faz 1 toplamı** | | **2** |

Hesapta kalan: 30 (`get_balance` ile doğrulandı). Bu işe başlarken 32 vardı; brief "yaklaşık 35" diyordu.

### Dosyalar

| Yol | Piksel boyutu | Pivot x | Pivot y (ALTTAN) |
|---|---|---|---|
| `Assets/Art/Iso/iso_char_villager_se.png` | 68x68 | 34 | 12 |

- Pivot ölçümü: en alt opak satır y=55 (üstten, 0 tabanlı), altında 12 boş satır var. Pivot y bu satırın alt kenarıdır. Unity normalize değeri yaklaşık (0,5, 0,176).
- Pivot x: iki ayağın kapladığı x=27–41 aralığının ortası, aynı zamanda tuval merkezi.
- Ayaklar aynı satırda bitmiyor: sol ayak y=55'te, sağ ayak y=53'te bitiyor.

### Köylü doğrulaması

| Kontrol | Sonuç |
|---|---|
| Boy (44–52 px) | 45 px (y=11–55), geçti; genişlik 17 px |
| Yön | Ekranda sağ-aşağı bakıyor, SE ile uyumlu |
| Saydamlık | Tam saydam 4039, tam opak 585, yarı saydam 0 |
| Pişmiş gölge | Görsel incelemede yok |
| Palet | 32 renk (referans south 17 renk); ek renklerin çoğu çok az pikselde. "Az tonlu" hedefinden biraz uzak |

### Çimen karosu doğrulaması (`iso_ground_grass_a.png` TESLİM EDİLMEDİ)

| Kontrol | Sonuç |
|---|---|
| Ham boyut | 64x64, 64x32 değil; üst yüze ek olarak yan yüzler ve taşan çimen tutamları var |
| Yarı saydam / anti-alias | Yarı saydam 0; renk düzeyinde anti-alias ayrıca ölçülmedi |
| Maske (64x32, 2 px yatay / 1 px dikey basamak) | Maske doğrulandı; maske dışında piksel kalmıyor |
| En iyi konum, y=22 | 31 delik + 3 çimen dışı piksel (%3,1); üst uç satırı tamamen boş |
| İlk deliksiz konum, y=24 | 0 delik, 63 çimen dışı piksel (%5,8); yan yüzün koyu şeridi maske içine giriyor |
| Dikiş testi (ortalama parlaklık sıçraması) | y=22: içeride 32,9, dikişte 59,3 (1,80 kat). y=24: 32,2 ve 56,4 (1,75 kat) |
| Görsel dikiş testi | İki konumda da çapraz çizgiler net görünüyor; dikişsiz değil |
| Stil | Çok gürültülü, 25–28 renk; "düz, az tonlu" değil |
| Işık | Yan yüzler atıldığı için tek üst yüz kalıyor; sol/sağ yüz sıralaması bu karo için uygulanmıyor |

Karar: Maskeleme yetmedi; brief içerik boyamayı yasakladığı için düzeltme yapılmadı.

Maskeleme notu: Tam 2:1, 64 px genişliğindeki elmaslar (32, 16) adımıyla döşendiğinde boşluk kalmıyor, ama komşu karolar kenarda 2 px örtüşüyor (2048 piksellik örnekte 128 piksel). Oyunun çizim sırası bunu örtmelidir.

### Kamera / view seçimi

`high top-down` (yaklaşık 35°) seçildi. 26,6° ekrandaki kenar eğimidir; 2:1 elmas veren gerçek kamera yüksekliği yaklaşık 30°'dir ve PixelLab da isometric 30'u 2:1 olarak tanımlar. 35°, 30°'ye 20°'den daha yakındır. `oblique` yalnızca 4 yönlüdür. Karakter tam 30° ile çizilemiyor, bu yüzden açı farkı yaklaşık 5° kalıyor.

### Araç sınırları ve notlar

- `v3` + referans aynı 1 generation ile 8 yön verdi. Kalan 7 yön PixelLab'de hazır, Faz 2 onayına kadar kopyalanmadı; bu yönler için ek generation gerekmiyor.
- Kalan yönler incelenmedi.
- `create_isometric_tile` maliyeti araç açıklamasında yok; 1 generation bilgisi PixelLab yardımından geldi ve bakiye ile doğrulandı.
- Yatak dosya adı için yönerge örneği `iso_item_bed_straw.png`, brief `iso_item_bed_straw_2x1.png` diyor. Yönergenin `_2x1` kuralı gereği ikincisi kullanılacak.
- `.meta` yazılmadı, commit yapılmadı.

## Faz 2 (yalnızca köylü yönleri) ve Faz 3 başlangıcı (2026-11-08)

Durum: **Köylünün 7 yönü kopyalandı. Yatak denemesi başarısız, projeye konmadı.** Onay bekleniyor.

Kullanıcı kararı: zemin karoları (çimen a/b/c, toprak, geçişli set) kodla yapılacak; Faz 2'nin zemin kısmı atlandı. Faz 1'deki zemin seçenekleri artık geçersiz.

### Harcama

| Çağrı | Araç | Generation |
|---|---|---|
| 3 | Köylü yönleri: Faz 1'deki 8 yönlü üretimden indirildi | 0 |
| 4 | `create_map_object`, temel mod, 96x72, `high top-down`, 1 yön (`a517c4c0-96d4-4fe7-8534-465292beb1bd`) | 1 |
| **Bu bölüm toplamı** | | **1** |
| **İş toplamı** | | **3** |

Hesapta kalan: 29 (`get_balance` ile doğrulandı). `create_map_object` temel modun 1 generation tuttuğu da böylece doğrulandı.

### Dosyalar

Pivot y ALTTAN piksel. Tuval 68x68; pivot y = 68 − (en alt opak satır + 1). Pivot x = tuval merkezi 34.

| Yol | Piksel boyutu | Pivot x | Pivot y (ALTTAN) | Boy |
|---|---|---|---|---|
| `Assets/Art/Iso/iso_char_villager_se.png` (Faz 1) | 68x68 | 34 | 12 | 45 |
| `Assets/Art/Iso/iso_char_villager_s.png` | 68x68 | 34 | 13 | 44 |
| `Assets/Art/Iso/iso_char_villager_sw.png` | 68x68 | 34 | 12 | 45 |
| `Assets/Art/Iso/iso_char_villager_w.png` | 68x68 | 34 | 11 | 44 |
| `Assets/Art/Iso/iso_char_villager_nw.png` | 68x68 | 34 | 12 | 45 |
| `Assets/Art/Iso/iso_char_villager_n.png` | 68x68 | 34 | 13 | 45 |
| `Assets/Art/Iso/iso_char_villager_ne.png` | 68x68 | 34 | 12 | 45 |
| `Assets/Art/Iso/iso_char_villager_e.png` | 68x68 | 34 | 11 | 44 |

- Ölçülen değerler yönler arasında 11–13 arasında değişiyor. Yön değiştirirken karakter 1 px zıplamasın istenirse 8 dosyanın hepsine ortak **(34, 12)** önerilir; sapma en fazla 1 px.
- Dört ara yön dışındaki S, W, N, E dosyaları için yönerge ad önermiyor. Kısaltma olarak `s`, `w`, `n`, `e` kullanıldı.
- Tüm yönlerde yarı saydam piksel 0. Renk sayısı 17–32. Görsel kontrolde yüz, saç ve tunik 8 yönde tutarlı; bakış yönleri dosya adlarıyla uyumlu.
- Dosyalar indirilen PNG'lerle bayt bayt aynı. Var olan dosyanın üzerine yazılmadı (`cp -n`). `.meta` yazılmadı. `iso_char_villager_se.png.meta` Unity tarafından oluşturulmuş; ona dokunulmadı.

### Yatak doğrulaması (`iso_item_bed_straw_2x1.png` TESLİM EDİLMEDİ)

| Kontrol | Sonuç |
|---|---|
| Boyut / saydamlık | 96x72, arka plan saydam, yarı saydam 0, 13 renk (düz, az tonlu: geçti) |
| Ayak izi | **Kare (1x1) bir eşkenar dörtgen, 2x1 değil.** Taban 72 px geniş, yaklaşık 36 px yüksek, simetrik |
| Okunurluk | Battaniye ve yastık yok. Yatak değil, kısa ayaklı bir minder/puf gibi okunuyor |
| Kenar eğimi | Yaklaşık 2:1, ama basamaklar düzensiz (üst-sol kenar: 2,1,1,2,3,2,1,2,…) |
| Işık | Üst 196, sol yüz 102, sağ yüz 78 (ortalama parlaklık): sıralama doğru |
| Pişmiş gölge | Yok |
| Pivot (bilgi için) | Taban elmasının merkezi yaklaşık x=48, y=25 alttan |

Karar: 2x1 ayak izi şartını karşılamıyor. Maskeleme ayak izini değiştiremez, bu yüzden teslim edilmedi.

### Araç sınırları

- `create_map_object` izometrik görünüm sunmuyor (yalnızca `low top-down`, `high top-down`, `side`). Açıklamadaki "2x1, uzun kenar sol-üstten sağ-alta" ifadesi dikkate alınmadı.
- `create_1_direction_object` 10–15 generation tutuyor ve izometrik görünümü de yok.

## Faz 3: Eşyalar, kılavuzlu üretim (2026-11-08)

Durum: **Yatak, ocak taşı ve tabure teslim edildi; her biri tek yön ve 1 deneme.** Onay bekleniyor; Faz 4'e geçilmedi.

Köylü pivotu: kullanıcı 8 köylü dosyasının tümünde **(34, 12)** pivotunu kullanıyor (x=34, y=12 alttan). Önceki tablodaki yön bazlı 11–13 değerleri bilgi amaçlıdır.

### Yöntem (kılavuzlu üretim)

`create_map_object` stil eşleme modu (`background_image` + `inpainting` `type: mask`) kullanıldı.
- **Kılavuz görüntü:** Saydam tuval; ayak izi tam 2:1 maskeyle çizilmiş düz, tek renkli (#786046) bir taban elmasından ibaret. 2x1 için 64x32 elmaslardan iki tane, +x ekseni boyunca.
- **Maske (beyaz = üretim alanı):** Taban elması, eşyanın yüksekliği kadar yukarı uzatıldı (yatak 22 px, ocak 14 px, tabure 22 px) ve 1 px genişletildi.
- **Kontrol sonucu:** Üç sonuçta da maske dışında opak piksel 0. Araç arka planı saydam döndürdü. Taban rengi çıktıda kalmadı.
- Betik: scratchpad `tools/footprint_guide.py`.

### Harcama

| Çağrı | Araç | Generation |
|---|---|---|
| 5 | Yatak, stil eşleme, 112x84, 2x1 maske (`8e9cc13d-e975-4a27-b4aa-570f470d0a8d`) | 1 |
| 6 | Ocak taşı, stil eşleme, 80x56, 1x1 maske (`eda28f7e-9da1-494d-8436-da05f2366306`) | 1 |
| 7 | Tabure, stil eşleme, 80x64, 1x1 maske (`7c4c1b38-65d3-4a45-8788-42de06875dc2`) | 1 |
| **Faz 3 toplamı** | | **3** |
| **İş toplamı** (başarısız ilk yatak dahil) | | **7** |

- Yatak çağrısından sonra deneme hesabında 28 kaldı. Stil eşleme modunun maliyeti araç açıklamasında yoktu; bakiyeyle 1 olarak doğrulandı.
- **Hesap değişikliği:** Ocak taşından sonra hesap **Tier 1 aboneliğe (Pixel Apprentice)** geçmiş. Tabureden sonraki bakiye: bu dönemde 2000 hak, 1 kullanılmış (tabure), kalan 1999, yenilenme 2026-12-08.

### Dosyalar

Pivot = ayak izi taban elmasının merkezi; y ALTTAN piksel. Kılavuz maskeden hesaplandı.

| Yol | Piksel boyutu | Pivot x | Pivot y (ALTTAN) | Ayak izi (taban elması, tuvalde) |
|---|---|---|---|---|
| `Assets/Art/Iso/iso_item_bed_straw_2x1.png` | 112x84 | 56 | 32 | x 8–103, y 28–75 (96x48) |
| `Assets/Art/Iso/iso_item_hearth_stone.png` | 80x56 | 40 | 24 | x 8–71, y 16–47 (64x32) |
| `Assets/Art/Iso/iso_item_stool.png` | 80x64 | 40 | 24 | x 8–71, y 24–55 (64x32) |

Unity normalize pivot: yatak (0,5, 0,381); ocak (0,5, 0,429); tabure (0,5, 0,375).

### Doğrulama

| Kontrol | Yatak | Ocak taşı | Tabure |
|---|---|---|---|
| Ayak izi | 2x1, uzun kenar sol-üstten sağ-alta; ayaklar taban köşelerinde (±1 px) | 1x1; taş halka 60x38, tabana oturuyor | 1x1 içinde; ayaklar 40 px aralıkta, tabandan dar |
| Kenar basamakları | Üst kenarlar 2 px yatay / 1 px dikey, düzenli | 2/1, düzenli | Oturak kenarı 2/1, düzenli |
| Saydamlık / yarı saydam | Saydam / 0 | Saydam / 0 | Saydam / 0 |
| Maske dışı piksel | 0 | 0 | 0 |
| Renk sayısı | 28 | 62 (ateş yüzünden) | 41 |
| Işık (ortalama parlaklık) | Üst 122 > sol 77 > sağ 35: doğru | Yan yüzler 2–3 px; sol 86, sağ 85, fark belirsiz | Oturak 87 > sol 61 > sağ 57: doğru ama fark zayıf |
| Pişmiş gölge | Yok | Yok | Yok |
| Okunurluk | Yatak: krem yastık sol-üst uçta, koyu kırmızı-kahve battaniye alt üçte ikide, ahşap çerçeve | Taş ocak | Dört ayaklı ahşap tabure |

### Notlar ve sınırlar

- **Ocak taşı:** Açıklamada "ateş yok, parıltı yok" dendiği halde **yanan bir ateşle** geldi. Alevler sprite'a sabit çizili; renk sayısının 62'ye çıkmasının nedeni bu. Söndürülmüş bir sürüm ya da ayrı bir ateş efekti gerekiyorsa yeni bir üretim (1 generation) gerekir. Var olan dosyanın üzerine yazılmayacağı için bu, yeni bir adla ya da kullanıcı dosyayı sildikten sonra yapılabilir.
- **Tabure:** Görsel taban merkezi (ayak uçları) footprint merkezinin yaklaşık 3 px altında; ayak uçlarına göre pivot yaklaşık (40, 21). Raporlanan (40, 24) yönergeye göre karo merkezidir.
- **Palet:** Yatak koyu kırmızı-kahve ve krem, eski köylü paletine yakın. Ocak ve tabure daha çok renkli ve "düz, az tonlu" hedefinden biraz uzak.
- Hepsi tek yön. Yön başına yeni üretim aynı yöntemle yaklaşık 1 generation tutar; ayna (x çevirme) Unity'de ücretsizdir.
- `.meta` yazılmadı, commit yapılmadı, var olan dosyaların üzerine yazılmadı.

---

# Tur 4: İzometrik varlık seti, Tier 1 (A1)

## Faz A: Durum doğrulama (2026-11-08, harcama 0)

### Hesap (`get_balance`)

| Alan | Değer |
|---|---|
| Plan | **Tier 1: Pixel Apprentice**, aktif abonelik |
| Aylık limit | 2000 generation; yenilenme 2026-12-08 |
| Kalan | 1999 (1 kullanıldı: tur 3'teki tabure) |
| Bu işin üst sınırı | 250. Limit ve bakiye bunu karşılıyor, durmaya gerek yok |

### Pro araçların kullanılabilirliği (çağrı yapılmadan)

PixelLab yardımına göre (`search_help`): Tier 1, `create_tiles_pro` (`tile_feature="tileset"` dahil), `create_building_kit` ve `create_path_tiles` için yeterli. `create_object_pro_flash`, `create_8_direction_object` ve `create_map_object` stil eşleme modu deneme hesabında bile açık. Listedeki araçların hiçbiri Tier 2 gerektirmiyor.

Araç açıklamalarındaki maliyetler:

| Araç | Ayar | Maliyet |
|---|---|---|
| `create_tiles_pro`, bağımsız karolar | izometrik, 64 px, açı 30, derinlik 0 | 10 (en fazla 16 karo) |
| `create_tiles_pro`, `tileset` | 64 px, 16 karoluk köşe geçiş seti | 10 |
| `create_tiles_pro`, stil modu | 64x32 stil görüntüsü | 10 |
| `create_path_tiles` | izometrik 64 px | 10 (açı 90 olursa 15); 18 karo |
| `create_building_kit` | `layout=materials` | 10 |
| `create_building_kit` | izometrik 64 px `grid` | 25 (`wall_tiles=1` ise 15) |
| `create_object_pro_flash` | 1 yön, 112x84 | 6 (`get_pro_flash_capabilities` teklifi) |
| `create_8_direction_object` | 64 px'e kadar | 10 |
| `create_map_object` | stil eşleme + maske | 1 (tur 3'te bakiyeyle doğrulandı) |

### `Assets/Art/Iso` içeriği (dokunulmadı)

- `iso_char_villager_{s,sw,w,nw,n,ne,e,se}.png` + `.meta`: 8 yön, 68x68, ortak pivot (34, 12).
- **`iso_item_bed_straw_2x1.png`**: 112x84. Tur 3, Faz 3'te 2026-11-08'de teslim edildi. Araç `create_map_object` stil eşleme + kılavuz 2x1 maske (`8e9cc13d-e975-4a27-b4aa-570f470d0a8d`), 1 generation. Doğrulamayı geçti:
  - Arka plan saydam, yarı saydam 0, maske dışı piksel 0.
  - Ayak izi 2x1, uzun eksen +x (SE-NW yönü).
  - Kenarlar düzenli 2:1.
  - Işık üst 122 > sol 77 > sağ 35.
  - Yastık ve battaniye net. Pivot (56, 32 alttan).
- **`iso_item_hearth_stone.png`**: 80x56. Aynı gün, aynı araç ve yöntemle (`eda28f7e-9da1-494d-8436-da05f2366306`), 1 generation.
  - Geçenler: ayak izi 1x1, 2:1 kenarlar, saydamlık, maske dışı 0. Pivot (40, 24 alttan).
  - **Sorunlar:** "ateş yok" denmesine rağmen yanan ateş sprite'a çizili ve 62 renk var. Yan yüzler 2–3 px olduğu için ışık sırası ölçülemiyor (sol 86, sağ 85).
- `iso_item_stool.png`: 80x64, aynı yöntem (`7c4c1b38-65d3-4a45-8788-42de06875dc2`). Geçti; ışık farkı zayıf. Pivot (40, 24 alttan).
- Üç eşya dosyası indirilen PNG'lerle bayt bayt aynı, değişmemiş. `.meta` dosyaları Unity'de oluşmuş; şu an PPU 100 ve pivot merkezde (0,5, 0,5). Import ayarı henüz yapılmamış görünüyor (bilgi için).

### Brief / yönerge ile çelişkiler ve sınırlar

1. **Zincirleme referans mümkün değil:** `create_tiles_pro` `tile_feature="tileset"`, `style_images` ile birlikte kullanılamıyor (araç açıklaması). Geçiş setine çimen/toprak karosu referans olarak verilemez. Yerine onaylı çimen ve toprak karolarının renkleri `palette` olarak verilecek; her pikseli bu renklerden olur.
2. **"Kenar kıvrımlılığı orta" için parametre yok:** Ancak açıklama metniyle istenebilir. "Arazi yüksekliği sıfır" `tile_depth_ratio=0` ile sağlanır.
3. **Eşya adlandırması:** Yeni kural `iso_item_<ad>_<yön>.png`. Var olan üç dosyada yön eki yok ve dokunulmayacak. Öneri: kullanıcı Unity'de yeniden adlandırır (GUID korunur):
   - `iso_item_bed_straw_2x1.png` → `iso_item_bed_straw_2x1_se.png` (uzun eksen SE-NW)
   - ocak → `iso_item_hearth_stone_se.png`
   - tabure → `iso_item_stool_se.png`

   Böylece yalnızca eksik yönler üretilir. Aksi halde aynı eşyanın ikinci kopyası üretilir.
4. **Karo ucu:** `tile_flat_top_px` 2 ("klasik") seçilecek. Yönerge yalnızca 2:1 basamak istiyor; doğrulama maskesi çıktının gerçek ucuna göre ayarlanır.

### Plan (onay bekliyor)

| # | Varlık | Araç ve ayar | Tahmini generation |
|---|---|---|---|
| 1 | Çimen a (aday seti) | `create_tiles_pro` bağımsız; isometric, `tile_size=64`, `tile_view_angle=30`, `tile_depth_ratio=0`, `tile_flat_top_px=2`, `outline_mode=segmentation`, `max_colors` 5; "flat short grass" adaylarından en iyisi seçilir | 10 |
| 2 | Çimen b, c + toprak a, b | `create_tiles_pro` stil modu, `style_images` = onaylı çimen a (64x32); numaralı 1) çimen b 2) çimen c 3) toprak a 4) toprak b | 10 |
| 3 | Çimen ↔ toprak geçiş seti | `create_tiles_pro` `tile_feature="tileset"` (16 karoluk köşe seti); 1. adımdaki ayarlar, `palette` = onaylı karoların renkleri | 10 |
| 4 | (Ayrı onayla) toprak yol | `create_path_tiles` isometric 64, açı 30, derinlik 0 (18 karo) | 10 |
| 5 | Yatak 2x1, SW-NE ekseni | `create_map_object` stil eşleme + 2x1 kılavuz maske (tur 3'te çalışan yöntem); SE-NW ekseni zaten var (3. madde) | 1 (en fazla 2) |
| 6 | Tabure, ocak 1x1 | Aynı yöntem; tek yönle başla. Ocak için "söndürülmüş" yeni sürüm, kullanıcı isterse | 1–2 her biri |
| 7 | Duvarlar | `create_building_kit` isometric 64, `layout=materials` (ahşap çatkı + sıva), `wall_tiles=2`; parçalar onaydan sonra seçilir | 10 (grid gerekirse +25) |
| | **Toplam** | | **yaklaşık 55; en kötü durum yaklaşık 85 (üst sınır 250)** |

Neden `create_map_object` ve `create_object_pro_flash` değil: Pro Flash ilk kareyi her zaman güneye bakar çizer ve ayak izi kontrolü vermez; ayrıca 5–6 generation tutar. Maskeli yöntem 1 generation ile ayak izini kesin tutturdu.
