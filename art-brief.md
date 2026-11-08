# Art Brief: İzometrik varlık seti, Tier 1 ile (A1, tur 4)

Bu dosya Claude Code için iş emridir. Önceki brief art-brief-tur3.md olarak saklandı (artık geçersiz). Eski 3/4 brief'i art-brief-eski-34.md GEÇERSİZDİR.

## Durum ve kararlar
- Kullanıcı PixelLab **Tier 1** plan aldı. Pro araçlar artık açık olmalı (create_tiles_pro, animasyon araçları vb.). Tier 2 gerektiren araçlar (create_room, extend map v2) bu işte gerekmiyor.
- Zemin artık KODLA DEĞİL, PixelLab ile üretilecek (kullanıcı kararı). Önceki turdaki 'kodla yap' notu geçersiz.
- Köylü: 8 yön tamam, Assets/Art/Iso altında. Pivot ortak (34, 12). Dokunma. Aynı karakter stil referansıdır.
- Bağlayıcı şartname: docs/gorsel-yonerge.md (zemin karosu 64x32 elmas, tam 2:1 kenar, anti-alias yok, saydam arka plan, ışık sol-üstten, gölge pişirilmez, ad kuralı iso_<tür>_<ad>_<yön>.png). Çelişki olursa yönerge geçerlidir.
- Daha önce görülenler: deneme hesabındaki temel iso karo aracı yan yüzlü kalın blok verdi (64x64), maske ile düzeltilemedi, dikiş çıktı. Pro araçlarla bunun giderilmesi hedefleniyor, garanti değil.

## Hak bütçesi
- Önce get_balance ile hesabı doğrula: plan (Tier 1 mi), aylık limit, kalan bakiye. Bunu raporun başına yaz.
- Bu işin toplam üst sınırı: 250 generation. Plan limiti daha düşük çıkarsa (ör. Tier 1 tahminen 2000) ya da bakiye 250'den azsa dur ve bana söyle.
- Her PixelLab çağrısından ÖNCE aracın açıklamasındaki maliyeti, bu işte harcanmış toplamı ve kalan bakiyeyi yaz.
- Hata ya da plan/Tier kısıtı çıkarsa tekrar deneme, hatayı aynen bildir.
- Fazları sırayla yap. Her fazın sonunda DUR, raporla, onayımı bekle.

## Faz A: Durum doğrulama (harcama yok)
1. get_balance, plan/Tier, kalan hak.
2. Pro araçların gerçekten kullanılabilir olduğunu araç açıklamalarından doğrula (create_tiles_pro, create_building_kit, create_object_pro_flash, create_8_direction_object vb.). Çağrı yapma.
3. Assets/Art/Iso klasöründe şu an olan dosyaları listele. Özellikle iso_item_bed_straw_2x1.png ve iso_item_hearth_stone.png: bunlar teslim edildi mi, ne zaman ve hangi araçla üretildi, doğrulamayı geçtiler mi (boyut, saydamlık, ayak izi, 2:1, ışık)? Bu dosyalara DOKUNMA, silme, üzerine yazma. Yalnızca durumlarını raporla.
4. Her varlık türü için (zemin varyasyonu, geçiş seti, 2x1 yatak, 1x1 eşya, duvar) hangi aracı seçeceğini ve tahmini maliyeti 6-8 satırlık bir planla yaz.
DUR ve planı onaya sun.

## Faz B: Zemin
Hepsi izometrik, 64x32 elmas, yalnızca üst yüz (yan yüz ya da kalınlık YOK; araçta kalınlık/yükseklik seçeneği varsa sıfır ya da en ince). Düz, az tonlu (3-5 ton), gürültüsüz.
1. create_tiles_pro (varyasyon): çimen karosu a, tek bir deneme. Doğrula (aşağıdaki Doğrulama). Geçerse b ve c varyasyonlarını üret (aynı stil referansı). Toprak karosu için de 2 varyasyon (iso_ground_dirt_a, iso_ground_dirt_b).
2. create_tiles_pro (tileset): çimen <-> toprak geçiş seti. Taban karo olarak 1. adımdaki çimen ve toprak karolarını referans ver (zincirleme). Kenar kıvrımlılığı orta, arazi yüksekliği sıfır. Karo sayısını ve hangi geçişleri verdiğini yaz. Dikişsiz olmalı.
3. (Yalnızca 1 ve 2 geçerse, ayrı onayımla) create_path_tiles ile toprak yol seti.
Dosya adları: iso_ground_grass_a/b/c.png, iso_ground_dirt_a/b.png, geçiş seti için iso_ground_grass_dirt_<n>.png (n = karo no).
DUR, raporla.

## Faz C: Eşyalar
Ayak izi kuralı: 1x1 nesne 64x32 elmasa oturur; 2x1 nesne iki karo (iki eksenden birinde), tabanı yaklaşık 96x48 px paralelkenar. İki eksen için ayrı dosya gerekir (SE-NW ve SW-NE); yansıtma YAPMA (ışık yönü bozulur).
1. Aracı seç (Faz A planına göre). Her nesnede ÖNCE tek yön ve tek eksenle başla, ayak izini ölç, doğrula, ancak sonra diğerlerine geç.
2. Sıra: yatak (2x1, battaniye ve yastık net okunmalı, kısa ayaklı puf gibi görünmemeli), tabure (1x1), ocak taşı (1x1, zaten var ise Faz A raporuna göre yeniden değerlendir).
3. Ad: iso_item_bed_straw_2x1_<yön>.png, iso_item_stool_<yön>.png, iso_item_hearth_stone_<yön>.png.
Ayak izi tutmazsa tekrar tekrar deneme: DUR ve göster.

## Faz D: Duvarlar
- Orta çağ kulübe malzemesi (ahşap ya da sıva), yalnızca duvar (çatı yok).
- create_building_kit (iso) ile önce malzeme örneği (materials, maliyetini yaz), onayımla sonra parçalar: her iki eksen için düz duvar ve köşe (SE-NW, SW-NE).
- Ad: iso_wall_straight_se_nw.png, iso_wall_straight_sw_ne.png, iso_wall_corner_<yön>.png.
DUR, raporla.

## Doğrulama (her zemin ve duvar dosyası)
- Gerçek piksel boyutu 64x32 mi (zemin)? Arka plan tamamen saydam mı? Yarı saydam ya da anti-alias piksel var mı? (Küçük bir betikle say.)
- Kenarlar tam 2:1 mi (2 piksel yatay, 1 piksel dikey)? Köşe ve tepe pikselleri maskenin içinde mi?
- Döşeme testi: aynı karoyu (ve varyasyonlarını) 8x8 elmas ızgarada (32,16 adımıyla) döşe, bir PNG olarak kaydet (scratchpad). Komşu karolar arasında görünür çapraz çizgi, parlaklık sıçraması ya da boşluk var mı? Ortalama parlaklık sıçramasını sayıyla yaz.
- Işık: üst yüz en açık, sol yüz orta, sağ yüz en koyu. Gölge pişirilmemiş olmalı.
- Çıktı tam 2:1 değilse: tamamen başarısız sayma, tam 2:1 elmas maskesiyle kırp (yalnızca maske dışını sil, içeriği yeniden boyama) ve raporda belirt. Maske yetmeyecek kadar bozuksa DUR.

## Dosya konumu ve pivot
- Hepsi Assets/Art/Iso/ altında, alt klasör yok, ad kuralı yukarıdaki gibi. PNG kopyala. .meta YAZMA. Import ayarlarını (PPU 32, Point, sıkıştırma yok, pivot) ben Unity'de yapacağım.
- Pivot: ayak izi taban elmasının merkezi. Her dosya için piksel olarak ölç ve raporla (sprite boyutu, pivot x, pivot y ALTTAN; zeminde pivot elmas merkezi).

## Yapma
- Git commit ya da push yapma (git pull --rebase hariç).
- Assets/Scripts, Packages, ProjectSettings, Assets/Art/Test_34 ve mevcut Assets/Art/Iso dosyalarına dokunma. Var olan dosyanın üzerine yazma.
- PixelLab dışında üretim aracı kullanma. Zemini kodla üretme.
- Onay beklemeden bir sonraki faza geçme.
- Bu işte köylü animasyonu, çocuk ve yaşlı varyantı YOK (sonraki tur).

## Teslim
Her faz sonunda art-brief-result.md dosyasına yeni bir bölüm EKLE (eskiyi silme): faz başına ve çağrı başına generation, toplam, kalan; dosya listesi (yol, piksel boyutu, pivot); doğrulama sonuçları; seçtiğin araç ve ayarlar ile gerekçe; sorunlar. Sonra kısa bir özet ver ve onay bekle.

## ÖNEMLİ (eklendi): Masaüstü art klasörü kopyası
Kullanıcı üretilen her şeyi düzenli kontrol etmek için masaüstüne bir klasör açtı: C:\Users\User\OneDrive\Desktop\art
Bundan sonra ürettiğin HER dosya (adaylar, onaylananlar, reddedilenler dahil) hem projede hem burada olacak:
- Yapı: art\zemin\<yyyy-mm-dd>_<ad>\, art\esya\<yyyy-mm-dd>_<ad>\, art\duvar\<yyyy-mm-dd>_<ad>\ (aynı dosya adlarıyla, özgün boyut).
- Her aday kümesi ve her faz sonu için büyütülmüş (x4, en yakın komşu) bir önizleme: art\<kategori>\<yyyy-mm-dd>_<ad>\_onizleme_x4.png. Zemin için 8x8 döşeme önizlemesini de buraya koy.
- art\_indeks.md dosyasına her üretim için bir satır EKLE (eskiyi silme): tarih | kategori | dosya/klasör | araç | generation | durum (aday, onaylı, reddedildi) | not.
- Şu ana kadar üretilen ve projede duran dosyalar için (çimen adayları, yatak, tabure, ocak, köylü yönleri, reddedilen karolar) geriye dönük kopyala ve indekse yaz. Karakter klasörünü (art\karakter) ayrı oturum yönetiyor, karıştırma; köylü yönlerini art\karakter\2026-11-06_villager\ altına kopyala, bunu yalnızca bir kez yap ve diğer oturum aynı klasörü kullanıyorsa üzerine yazma.
- İkinci bir oturum paralel olarak karakter tasarlıyor (art-brief-karakter.md). PixelLab hesabı ortak: çağrı 'aynı anda tek iş' diye reddedilirse 30 saniye bekle ve bir kez daha dene. Karakter oturumunun dosyalarına (iso_char_*, art-brief-karakter-result.md) dokunma.
