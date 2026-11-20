# Görsel yönerge: İzometrik (2:1 dimetrik) varlıklar

Bu dosya, varlık üreten yapay zekâ (PixelLab, Claude Code oturumu) için bağlayıcı şartnamedir.
Oyun artık **izometrik**. Eski "yüksek top-down / 3/4" testleri (Assets/Art/Test) **geçersizdir**.

## Projeksiyon
- 2:1 dimetrik. Zemin karosu bir **elmas: 64 px geniş x 32 px yüksek**. Unity PPU = 32 (1 dünya birimi = 32 px).
- Eksenler: ızgara +x ekranda sağ-yukarı (Iso.GridToWorld ile aynı), +y sol-yukarı. Yukarı yön kuzey-batı/kuzey-doğu köşeleri.
- Kenar çizgileri tam 2:1 (2 piksel yatay, 1 piksel dikey) basamaklı olmalı; yumuşatma (anti-alias) yok.
- Işık sol-üstten: üst yüz en açık, sol yüz orta, sağ yüz en koyu. **Gölge sprite'a pişirilmez** (oyun ekler).
- Arka plan tamamen saydam. Düz, az tonlu pikselart; önceki test karakterleriyle aynı stil/palet ailesi.

## Palet ve kontur kilidi (kullanıcı kararı 2026-11-20, Project belgesi 64c §1)
1. **İki donmuş palet.** Dünya görselleri (zemin, yapı, eşya, bitki, hayvan) `art/style/palette.json`'daki 112 rengi; ikon ve arayüz görselleri (`icon_*`, `ui_*`, `arayuz` ailesi) `art/style/palette_arayuz.json`'daki 128 rengi kullanır. İkisi de kabul edilmiş işlerin çıktılarından üretildi ve **donduruldu**: elle düzenlenmez, kendiliğinden yeniden üretilmez.
2. **Yeni renk yalnızca kullanıcı kararıyla.** Karar verilince `python tools/art/style.py palette --set dunya|arayuz --yeniden "<tarih, karar, gerekçe>"`; not dosyaya yazılır. Paletten kalan bir görseli geçirmek için palet değiştirilmez, görsel düzeltilir.
3. **Karakterler** rampa kilidinde kalır (aşağıda "Katmanlı karakter"); `ramps.py check` bağlayıcıdır.
4. **Kontur.** Dış kontur koyu ve **renkli**: nesnenin en koyu tonunun bir basamak koyusu; saf siyah (#000) kontur yok. İç çizgiler dış konturdan bir basamak açık. Zemin karoları kontursuz; duvar ve yapı parçalarında yalnızca dış kenarda kontur.
5. **Doygunluk** aileye göre sınırlı (`style.py check`: aile referanslarının 1,5 katını aşan KALIR). Çim ve yaprak yeşili toprak tonuna yakın; limon yeşili yok.
6. **Işık** sol üstten; alev ve ışık içeren görselde denetim `check --light` ile yapılır.
7. **Kapı:** `style.py check`'ten KALAN görsel teslim edilmez. Kilitten önce kabul edilmiş görsellerin taraması: `art/results/palet-kilidi-tarama.md` (yeniden üretim kullanıcı onayıyla).

## Teslimat (Assets/Art/Iso/ altına)
1. **Zemin seti**: çimen <-> toprak geçişli iso tileset, her karo 64x32 elmas, dikişsiz. + 2 çimen varyasyonu.
2. **Köylü**: 1 karakter, 8 yön (en az 4: SE, SW, NW, NE). Boy aşağıdaki ölçek tablosuna göre (yetişkin 76 px). Ayak altı karo merkezinde.
3. **Eşyalar**: yatak (2x1 karo), ocak taşı, tabure (1x1). Taban ayak izine uyar, yükseklik serbest.
4. **Duvar parçaları**: her iki eksen için (SE-NW ve SW-NE yönlü) düz duvar + köşe.

## Ölçek referansı (tester raporu 66, 21.11.2026)
Her görselin boyu tek bir insana göre ayarlanır. Ölçü **ayak noktasının (pivot) üstündeki dolu yükseklik**: yetişkin karakter
76 px ≈ 1,7 m, yani 1 m ≈ 45 px. Eşyada pivot ayak izi elmasının ortasıdır; elmasın arka yarısı (1x1'de 16 px, 2x1'de 24 px)
ölçüye girer, aralıklar bunu içerir. Denetim: `python tools/art/scale_check.py <png>...` (aralık dışı KALIR; teslim edilmez).

| Sınıf | Pivot üstü (px) | Gerçek boy karşılığı |
|---|---|---|
| Yetişkin karakter (84 px tuval) | 72-80 | 1,6-1,8 m |
| Çocuk (64 px tuval) | 52-62 | 1,2-1,4 m |
| Yaban ağacı, olgun (meşe, huş, çam, söğüt, ceviz, ıhlamur, kestane) | 120-140 | insanın ~1,7 katı (oyun sıkıştırması) |
| Yaban ağacı, genç | 60-85 | |
| Meyve ağacı, olgun ve meyveli evre | 95-115 | insanın ~1,4 katı; meyveli evre olgunla aynı gövde ±4 px |
| Meyve ağacı, genç | 50-72 | |
| Çalı ve bodur bitki | 35-70 | |
| Ulu ağaç 2x2 (160x224 tuval, kullanıcı kararı 20.11) | 185-215 | insanın ~2,6 katı, yaban ağacının ~1,5 katı |
| Ulu çınar 3x3 (192x192 tuval, kullanıcı kararı 20.11) | 165-190 | boy yerine genişlikle "ulu" okunur |
| Sandalye (sırt dahil) | 46-66 | 0,85-1,05 m + elmas derinliği |
| Tabure | 22-38 | 0,35-0,5 m |
| Kedi / küçük köpek / köpek | 16-24 / 18-26 / 24-36 | |
| Tavuk / kaz, ördek / küçük kuş / tavşan | 15-22 / 17-26 / 9-16 / 11-17 | |
| Koyun / keçi / domuz / sığır / at | 30-40 / 32-42 / 28-38 / 56-68 / 60-82 | |
| Ayı / kurt / tilki / karaca | 44-58 / 28-36 / 19-26 / 38-48 | |

- Tuval ölçüsü ölçekten ayrıdır: olgun yaban ağacı 112x160, meyve ağacı 96x136, pivot (tuval/2, 24); ulu ağaç 160x224 pivot (80, 36), çınar 192x192 pivot (96, 48).
- Var olan görselin üzerine yazılmaz: yeniden ölçeklenen ağaç `_v2` ekiyle gelir (`iso_tree_oak_grown_se_v2`); oyun `_v2`'yi
  kendiliğinden tercih eder (`IsoArt.GetPlant`).
- 21.11.2026 ölçümü: ağaçların hepsi (58-81 px), kedi, tavuk, güvercin, tavşan, kaz, tilki, martı ve küçük köpek aralık dışında;
  sandalye ve masalar aralıkta (ilk tester raporundaki "sandalye insan boyunda" ölçüsü elmas derinliğini saymıyordu).

## Adlandırma
`iso_<tür>_<ad>_<yön>.png` (ör. `iso_char_villager_se.png`, `iso_item_bed_straw.png`, `iso_ground_grass_a.png`).
Yönler: se, sw, nw, ne. Çok karolu nesne: `_2x1` eki.

## Pivot kuralı
- Pivot her zaman **ayak izi taban elmasının merkezi** (karakterde ayakların altı, karo merkezi).
- 1x1 nesnede: yatay merkez, dikeyde elmasın orta noktası. Sprite boyutunu ve pivotu raporla (px).

## Kurallar
- Eski 3/4 testlerini `Assets/Art/Test_34/` altına taşı; silme.
- `Assets/Scripts` altına dokunma.
- İş bitince her dosyanın piksel boyutunu ve pivot (px) değerini listele.

## Yön kuralı (yerleşim sistemi, belge 44)
Tek tanım: `Assets/Scripts/Core/Layout/Facing.cs` (testli). Izgaraya göre dört mantıksal yön:

| Yön | Izgara | Ekranda | Dosya eki | Kamera |
|---|---|---|---|---|
| N | -y | sağ-aşağı | `_se` | ön yüz görünür |
| E | +x | sağ-yukarı | `_ne` | arkası dönük |
| S | +y | sol-yukarı | `_nw` | arkası dönük |
| W | -x | sol-aşağı | `_sw` | ön yüz görünür |

- Bir yön ancak o yönün görseli (`items.json` → `art.n/e/s/w`) ya da sol/sağ simetrik eşyada (`art.mirror`) aynasının görseli
  varsa geçerlidir. Ayna yalnızca se ↔ sw (N ↔ W) ve ne ↔ nw (E ↔ S) çevirir; **arkası dönük görünüm aynayla elde edilmez**,
  ayrı görsel ister. Görseli olmayan yön seçilemez (yapı modunda Döndür yalnızca geçerli yönler arasında döner).
- Ön yüzü olan eşyada (oturma, sandık/dolap, ocak/tezgâh) dönüş 0'ın yönü: çok karoluda ayak izinden (yatay eşya N, dikey eşya W),
  tek karoda kendi görselinin yönü. Her dönüş saat yönünde bir adım (N → E → S → W).
- Ön yüzü olmayan eşya eksen görselleriyle çizilir (`art.alongX` çift dönüş, `art.alongY` tek dönüş).
- Görsel boyutu (`art.pxW/pxH`) ve pivot (`art.pivotX/pivotY`, alttan) ayak izinden (`width/height`) ayrı tutulur. Görsel adındaki ölçü
  ayak iziyle uyuşmuyorsa görsel eşlenmez (örn. 1x1 üretilmiş kurutma askısı, 2x1 tanım).
- Kameraya dönük yönler N ve W olduğundan oturma eşyası ve dolaplar odanın arka duvarlarına (doğu +x ve güney +y) sırt verir;
  ön duvarlar (kuzey ve batı) kesitte saydamlaşır.

## Katmanlı karakter (belge 43, 43b; 20.11.2026)
Karakter yaratmadaki seçimler (saç, sakal, başlık, renkler, beden) dünyada katmanlarla çizilir. Bağlayıcı sözleşme:

- **Manken:** her animasyon karesi kel bir taban karakterdir, anahtar renklerde çizilir (`tools/art/ramps.py` KEYS; `LayerTypes.cs` KeyColors). `tools/art/manken.py regions` kareyi bölge haritasına çevirir: `Assets/Art/Iso/katman/lay_body_<gövde>_<anim>_<yön>_<NN>.png`.
  - Kanallar: kırmızı = bölge kodu, yeşil = ton (0 koyu - 3 açık), mavi = çapa işareti (1 baş, 2 göz), alfa = örtü.
  - Gövde: `m` / `f` (yetişkin), `cm` / `cf` (çocuk), ardından `_ince`, `_orta`, `_gurbuz`, `_dolgun`. Ergen ve yaşlı yetişkin mankenini kullanır (kullanıcı kararı 19.11.2026).
  - Ham kaynaklar `Assets/Art/Iso/_ham/katman/lay_src_...` (kütüphaneye girmez).
- **Baş parçası:** saç kütlesi, saç önü, eklenti, süs, bıyık, sakal, favori, başlık yalnız 5 yönde çizilir (s, se, e, ne, n); batı yönleri aynadır.
  - Tuval 64x96; çapa (32, 24) = kel kafanın en üst pikseli. Ad: `lay_<yuva>_<kimlik>_<yön>_fr.png` (ön) ve isteğe bağlı `_bk.png` (gövdenin arkasında kalan kısım, ör. sırta dökülen saç).
  - Saç ve sakal referans kahve rampada, başlık ve süs referans gri rampada çizilir (`ramps.py` REFERENCE); oyun seçilen renge çevirir. Rampaya uymayan pikseller (metal, çiçek) olduğu gibi kalır.
- **Palet kilidi (kullanıcı kararı 19.11.2026):** ten, saç, göz ve kumaş rampaları `Assets/Resources/Content/character_ramps.json`'da kilitlidir (`ramps.py build` üretir, elle düzenlenmez). Karakter işi teslimden önce `ramps.py check` ile denetlenir; KALDI alan kabul edilmez.
- `Assets/Art/Iso/katman/` altındaki dokular okunabilir (Read/Write) içe aktarılır; oyun piksel piksel okur.
