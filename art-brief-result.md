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
