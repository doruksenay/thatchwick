# Art Brief: İzometrik (2:1 dimetrik) varlık seti (A1, tur 3)

Bu dosya Claude Code için iş emridir. Eski 3/4 brief'i (art-brief-eski-34.md) GEÇERSİZDİR, uygulama. Eski 3/4 testleri Assets/Art/Test_34/ altına taşındı (Unity ile, GUID'ler korundu), onlara dokunma.

## 0. Önce yap (üretimden önce)
1. `git pull --rebase` çalıştır. docs/gorsel-yonerge.md GitHub'da var, yerel klasörde henüz yoktu.
2. docs/gorsel-yonerge.md dosyasını baştan sona oku. O dosya bağlayıcı şartnamedir; bu brief onu hak bütçesi ve sıra açısından tamamlar. Çelişki olursa yönerge geçerlidir, çelişkiyi bana bildir.
3. PixelLab araçlarının güncel açıklamalarından, her varlık türü için hangi aracın izometrik çıktı verdiğini ve maliyetini çıkar (ör. create_isometric_tile, create_path_tiles, create_building_kit, create_character view seçenekleri, nesne araçları). Harcama yapmadan 6 satırlık bir plan yaz: varlık, seçtiğin araç, tahmini generation.

## Hak bütçesi (ÖNEMLİ)
- Hesap: ücretsiz deneme. Kalan yaklaşık 35 generation.
- Her PixelLab çağrısından ÖNCE araçtaki maliyeti ve bu işte harcanmış toplamı yaz.
- Kalan hak, o fazı bitirmeye yetmeyecekse çağrıyı yapma. Bana sor (hesaba paket alınabilir).
- Tier/plan kısıtı ya da hata çıkarsa tekrar deneme, hatayı aynen bana bildir.
- Fazları sırayla yap. Her fazın sonunda DUR, sonucu ve harcamayı raporla, onayımı bekle.

## Faz 1: Köylü ve zemin (ilk bakış, en kritik)
Amaç: görsel dilin izometrik 2:1 ile tutarlı çıktığını ucuza doğrulamak.
1. Köylü: referans görünüm (kahverengi tunik, kısa koyu saç, düz az tonlu piksel stili; eski karakter 48 px, character_id b88f12d1-67da-4ff8-b36b-3c772038f57a stil referansıdır). Önce SE yönünü tek başına üret (ya da aracın sunduğu en ucuz yolla). Kamera açısı 2:1 elmasa uymalı. Araçtaki seçenekler (low top-down ~20, high top-down ~35, oblique) 2:1 (yaklaşık 26,6 derece zemin eğimi) ile birebir eşleşmeyebilir; en yakınını seç ve nedenini yaz. Boy 44-52 px.
2. Zemin karosu: 64x32 elmas, dikişsiz. İlk olarak yalnızca tek bir saf çimen karosu (iso_ground_grass_a.png).
3. DUR. Aşağıdaki doğrulamayı yap ve raporla.

### Doğrulama (her zemin ve duvar dosyası için)
- Gerçek piksel boyutu 64x32 mi? Arka plan tamamen saydam mı? Yarı saydam ya da anti-alias piksel var mı? (Küçük bir betikle say.)
- Elmasın kenarları tam 2:1 mi (2 piksel yatay, 1 piksel dikey)? Köşe ve tepe pikselleri bu elmas maskesinin içinde mi?
- Çıktı tam 2:1 değilse: tamamen başarısız sayma. Karoyu tam 2:1 elmas maskesiyle kırp (yalnızca maske dışını sil, içeriği yeniden boyama) ve bunu raporda açıkça belirt. Maskeleme yetmeyecek kadar bozuksa durup bana göster.
- Işık: üst yüz en açık, sol yüz orta, sağ yüz en koyu. Gölge sprite'a pişirilmemeli.

## Faz 2: Zemin seti ve karakter yönleri
- Çimen <-> toprak geçişli iso tileset (64x32 elmas, dikişsiz) ve 2 çimen varyasyonu (iso_ground_grass_b.png, iso_ground_grass_c.png). Önce kenar geçiş mantığını kısaca açıkla (kaç karo, hangi geçişler), sonra üret.
- Köylünün kalan yönleri: SW, NW, NE (en az 4 yön: SE, SW, NW, NE). 8 yön yalnızca toplamda ek hak gerektirmiyorsa ya da ucuzsa; aksi halde 4 yönle bırak ve söyle. Yeni yürüme animasyonu bu iş emrinde YOK.

## Faz 3: Eşyalar
- Yatak (2x1 karo, iso_item_bed_straw_2x1.png), ocak taşı (iso_item_hearth_stone.png), tabure (iso_item_stool.png, 1x1).
- Taban ayak izine uyar, yükseklik serbest. Önce bir tanesini üret ve doğrula, sonra diğerlerine geç. Her eşya için yön sayısını ve maliyeti söyle (tek yönle başlamak yeterliyse öyle yap).

## Faz 4: Duvarlar
- Her iki eksen için (SE-NW ve SW-NE) düz duvar ve köşe parçası (iso_wall_straight_se_nw.png, iso_wall_straight_sw_ne.png, iso_wall_corner_*.png).
- Orta çağ kulübe malzemesi (ahşap/sıva, saz çatı değil, yalnızca duvar). Plan yetmeyecekse bu fazı bana bırak, maliyet tahminini yaz.

## Dosya konumu ve adlandırma
- Hepsi Assets/Art/Iso/ altına. Alt klasör kullanma, doğrudan ad kuralı:
  iso_<tür>_<ad>_<yön>.png (örn. iso_char_villager_se.png, iso_ground_grass_a.png). Yönler: se, sw, nw, ne. Çok karolu nesne: _2x1 eki.
- PNG dosyalarını kopyala. .meta dosyası YAZMA. Import ayarlarını (PPU 32, Point filtre, sıkıştırma yok, pivot) ben Unity'de yapacağım.

## Pivot kuralı
- Yönergeye göre pivot ayak izi taban elmasının merkezi (karakterde ayakların altı). Ben import ederken kullanacağım için her dosya için piksel olarak ölç ve raporla: (sprite genişliği x yüksekliği, pivot x, pivot y; pivot y üstten değil ALTTAN piksel olarak yaz ve hangisini kullandığını belirt).

## Yapma
- Git commit ya da push yapma (git pull --rebase hariç).
- Assets/Scripts, Packages, ProjectSettings ve Assets/Art/Test_34 içindeki dosyalara dokunma. Mevcut dosyaların üzerine yazma.
- PixelLab dışında başka üretim aracı kullanma.
- Onay beklemeden bir sonraki faza geçme.

## Teslim
Her faz sonunda ve işin sonunda proje kökünde art-brief-result.md dosyasını GÜNCELLE (üzerine ekle). İçeriği:
- Faz başına ve çağrı başına harcanan generation, toplam, kalan tahmini.
- Dosya listesi: yol, piksel boyutu, pivot (px).
- Doğrulama sonucu (2:1, saydamlık, anti-alias, ışık yönü), maskeleme yaptıysan hangi dosyalarda.
- Seçtiğin kamera/view ayarı ve gerekçesi, aracın sınırları, yapamadıkların.
Sonra kısa bir özet ver ve onay bekle.
