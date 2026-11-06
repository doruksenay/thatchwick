# Art Brief: Köylü yürüme animasyonu ve yaş varyantları (A1, tur 2)

Bu dosya Claude Code için iş emridir. Sırayla uygula. Sınırların dışına çıkma.

## Bağlam
- Oyun: Thatchwick, orta çağ köyünde canlı yaşam simülasyonu. Piksel sanat, yukarıdan 3/4 bakış (view: high top-down).
- Boyutlar: karakter 48 px (standard mod, tuval yaklaşık 68x68), zemin karosu 32 px. Unity import: PPU 32, Point filtre, sıkıştırma yok.
- Referans karakter: önceki oturumda ürettiğin 48 px köylü (Assets/Art/Test/koylu48_south.png ve diğer yönler; kahverengi tunik, kısa koyu saç). Bu karakterin PixelLab character_id değerini bul (önceki üretim çıktısından ya da PixelLab galerisinden). Bulamazsan DUR, bana söyle. Yeni bir köylü üretme.

## Hak (generation) bütçesi
- Hesap: ücretsiz deneme, 40 hak. Yaklaşık 36 kaldı.
- Bu iş emri için üst sınır: 8 generation. Her PixelLab çağrısından ÖNCE araç açıklamasındaki maliyeti ve bu çağrıdan sonra harcanmış toplamı yaz. Toplam 8'i aşacaksa çağrıyı yapma, bana sor.
- Bir çağrı hata verir ya da Tier/plan kısıtı çıkarsa tekrar deneme, hatayı aynen bana bildir.

## Görev 1: Yürüme animasyonu (4 yön)
- Kaynak: Referans köylü (48 px, 4 yön).
- Araç: animate_character, template modu, yürüme (walk) animasyonu, her yön için (south, east, north, west). Beklenen maliyet: yön başına 1 generation, toplam 4.
- Önce yalnızca south yönünü üret, kareleri incele (tutarlı mı, karakter kayıyor mu, hat bozuk mu). Sorun yoksa kalan 3 yönü üret. Sorun varsa DUR ve bana göster.
- Kare sayısını ve önerdiğin fps değerini raporla.

## Görev 2: Yaş varyantları (statik, 4 yön)
Aynı stil, aynı palet ailesi, aynı kamera (high top-down), aynı çizgi kalınlığı. Her biri tek generation:
- Çocuk köylü: yetişkinin yaklaşık %65-70 boyunda. Standard create_character, 4 yön, tuval 48 px civarı. Sade açık renkli tunik, kısa saç.
- Yaşlı köylü: hafif kambur duruş, gri saç/sakal, soluk renkli tunik. Standard create_character, 48 px, 4 yön.
Her ikisinde yetişkin köylüyle yan yana durduğunda aynı oyundan göründüğünü kontrol et. Tutarsızsa bana göster, tekrar üretmeden önce sor.

## Dosya konumu ve adlandırma (Unity projesi içinde)
- Kök: Assets/Art/Test/ (stil testi klasörü). Yeni klasörler: Walk/, Child/, Elder/.
- Adlar küçük harf, ASCII, alt çizgi:
  - Yürüme: Assets/Art/Test/Walk/koylu48_walk_south_00.png, koylu48_walk_south_01.png, ... (east, north, west için aynı biçim).
  - Çocuk: Assets/Art/Test/Child/cocuk_south.png, cocuk_east.png, cocuk_north.png, cocuk_west.png.
  - Yaşlı: Assets/Art/Test/Elder/yasli_south.png, yasli_east.png, yasli_north.png, yasli_west.png.
- PNG dosyalarını kopyala. .meta dosyası YAZMA. İçe aktarma ayarlarını (Point, sıkıştırma yok, PPU 32, pivot Bottom Center) ben Unity'de yapacağım.

## Yapma
- Git commit ya da push yapma.
- Assets/Scripts, Packages, ProjectSettings ve mevcut Assets/Art/Test dosyalarına dokunma. Var olan dosyaların üzerine yazma.
- PixelLab dışında başka bir üretim aracı kullanma.
- 8 generation sınırını aşma.

## Teslim
İşin sonunda proje kökünde art-brief-result.md dosyası yaz. İçeriği:
- Harcanan toplam generation ve çağrı başına döküm.
- Tüm dosyaların listesi (yol, boyut).
- Referans karakterin character_id değeri.
- Yürüme animasyonu: kare sayısı, önerdiğin fps, döngü uygun mu.
- Dikkat çeken sorunlar (tutarsızlık, kayma, hata, yapamadıkların).
- Kalan hak tahmini.
Sonra kısa bir özet ver. Bunun dışında hiçbir şeyi değiştirme.
