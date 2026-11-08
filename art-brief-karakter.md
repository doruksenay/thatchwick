# Art Brief: Karakter tasarımları (A1/A2 hazırlığı, paralel oturum)

Bu dosya, ZEMİN/EŞYA işini yürüten oturumdan AYRI, ikinci bir Claude Code oturumu içindir. Yalnızca karakter işini yap. Zemin, eşya ve duvarlara (art-brief.md) dokunma. Zemin oturumunun sonuç dosyası art-brief-result.md; senin sonuç dosyan art-brief-karakter-result.md (üst üste yazma).

## Amaç
Mevcut köylü (iso_char_villager_*, 68x68 tuval, boy ~45 px) yeterince detaylı görünmüyor. PixelLab ile daha detaylı ve birbirinden FARKLI karakterler tasarla. Mevcut köylüye dokunma, silme, üzerine yazma; stil ve ölçek referansıdır.

## Bağlayıcı kurallar
- Şartname: docs/gorsel-yonerge.md (2:1 dimetrik izometrik, ışık sol-üstten, gölge pişirilmez, saydam arka plan, anti-alias yok, ad kuralı iso_<tür>_<ad>_<yön>.png, yönler se, sw, nw, ne + s, w, n, e). Çelişki olursa yönerge geçerlidir, çelişkiyi bildir. İstisna: yönergedeki 'boy 44-52 px' ve 'az tonlu' hedefi kullanıcı tarafından gözden geçiriliyor; bu brief'teki boy testi (Faz K1) onu belirleyecek.
- Kamera: önceki turdaki gibi en yakın seçenek (high top-down, 2:1 için ~30 derece). Seçtiğin ayarı ve gerekçeyi yaz.
- Detay hedefi: daha okunur yüz (kaş, burun, ağız çizgisi), saç tutamları ve ışık/gölge ayrımı, tunikte kat/yaka/kol ağzı, kemer ve toka, küçük aksesuarlar (kese, bıçak kını, çanta), diz sargısı, bağcıklı ayakkabı. Ama stil temiz kalsın: net siyah dış hat, gürültü ve dithering yok, renk sayısı en fazla 32, topraksı ve soluk orta çağ paleti.
- Her karakter birbirinden silüet ve renkle ayırt edilebilmeli (kıyafet, saç, boy, yaş, ten rengi).
- Aynı anda tek iş sınırı çıkarsa (çağrı reddedilirse, ücret yazılmaz) 30 saniye bekleyip bir kez daha dene. Diğer oturum da PixelLab kullanıyor olabilir.

## Hak bütçesi
- Hesap Tier 1 (aylık 2000, yenilenme 2026-12-08). get_balance ile doğrula ve raporun başına yaz. Hesap diğer oturumla ortak.
- Bu brief için üst sınır: 120 generation. Her çağrıdan ÖNCE maliyeti, bu işte harcanan toplamı ve kalan bakiyeyi yaz. Aşılacaksa sor.
- Hata ya da plan/Tier kısıtında tekrar deneme, hatayı aynen bildir.
- Fazlar arasında DUR, onay bekle.

## Faz K0: Plan (harcama yok)
Karakter araçlarının (create_character v3 metinden ve referansla, standard, pro, create_character_pro_flash) güncel maliyetlerini, boyut seçeneklerini ve izometrik/2:1 uygunluğunu araç açıklamalarından çıkar. 6 satırlık plan yaz: hangi aracı hangi iş için seçtin ve neden, tahmini generation. DUR.

## Faz K1: Boy ve detay testi
Aynı karakteri (yetişkin erkek köylü, mevcut köylüden FARKLI yeni tasarım: sarı-kahve yün tunik, kemer ve kese, sarma bacaklık, koyu kumral saç ve hafif sakal, açık ten) üç boyutta üret. Hedef GÖVDE boyu: 48 px, 56 px, 64 px (tuval boyutunu araçla bu boy çıkacak şekilde seç ve gerçek boyu ölç). 8 yön tek çağrıda geliyorsa hepsini sakla, incelemeye yalnızca SE yönünü kopyala.
- Boyutlar arası detay farkını yaz (yüz, eller, aksesuarlar seçiliyor mu).
- Her boyutta renk sayısı, yarı saydam piksel, pivot (ayak izi merkezi, ALTTAN piksel) ölç.
DUR. Boyuna ben karar vereceğim (kullanıcıyla Unity'de 64x32 elmasın üzerinde yan yana bakarım).

## Faz K2: Karakter seti (boy kararından sonra)
Seçilen boyutta, aşağıdaki karakterleri üret. Her biri ayrı tasarım; yalnızca SE yönünü kopyalayıp onay bekle, sonra kalan yönleri kopyala (ek generation gerekmiyorsa).
1. Yetişkin kadın (önlük, bağlı saç, açık-orta ten)
2. Yaşlı erkek (gri sakal, değnek, hafif kambur)
3. Yaşlı kadın (başörtüsü/yün şal, gri saç)
4. Erkek çocuk (kısa boy, yırtık yaka, yamalı pantolon)
5. Kız çocuk (örgülü saç, basit elbise)
6. Genç adam / oduncu (geniş omuz, balta kını, koyu ten)
7. Genç kadın / çiftçi (hasır şapka, tırpan ya da sepet, orta ten)
8. Yetişkin erkek, ince yapılı (zanaatkar önlüğü, koyu saç, açık ten)
Çocuk ve yaşlı için boy oranını yetişkine göre ayarla: çocuk yaklaşık %65-70, yaşlı hafif kısa ve kambur. Aynı çağrıyla yön tutarlılığını kontrol et (yüz, kıyafet ve aksesuar 8 yönde tutarlı mı).
Maliyet tahmini ~2-4 generation / karakter. DUR ve rapor ver.

## Dosya konumu ve adlandırma
Proje içinde (Unity projesi, .meta YAZMA, import ayarlarını ben yapacağım):
- Adaylar ve test: Assets/Art/Iso/_aday/ altına, ad iso_char_<ad>_<boyut>_cand_<yön>.png (ör. iso_char_farmer_a_56_cand_se.png).
- Onaylananlar: Assets/Art/Iso/ altında, ad iso_char_<ad>_<yön>.png. Onay gelmeden buraya KOYMA.
- Karakter adları: farmer_a, woman_a, elder_m, elder_f, boy_a, girl_a, woodcutter_a, farmer_f, craftsman_a gibi kısa, küçük harf, ASCII.

## ÖNEMLİ: Masaüstü art klasörü kopyası
Kullanıcı üretilen her şeyi düzenli kontrol etmek için masaüstüne bir klasör açtı:
C:\Users\User\OneDrive\Desktop\art
Bundan sonra ürettiğin HER dosya hem projede hem burada olacak:
- Yapı: art\karakter\<yyyy-mm-dd>_<ad>\ altına tüm yön PNG'leri (özgün boyut, aynı adlarla).
- Her karakter için bir önizleme görseli: art\karakter\<yyyy-mm-dd>_<ad>\_onizleme_x4.png (8 yön yan yana, en yakın komşu ile 4 kat büyütülmüş, saydam yerine koyu gri zemin üzerinde). Her faz sonunda tüm adaylar için tek bir toplu görsel: art\karakter\<yyyy-mm-dd>_<faz>_toplu_x4.png.
- Adayları ve reddedilenleri de kopyala (adı cand kalsın), silme.
- art\_indeks.md dosyasına her üretim için bir satır EKLE (varsa eski satırları silme): tarih | kategori | dosya/klasör | araç | generation | durum (aday, onaylı, reddedildi) | not.
Kopyalama komutu (Windows'ta): robocopy ya da PowerShell Copy-Item; klasör yoksa oluştur. Masaüstü OneDrive altında, yol aynen yukarıdaki.

## Yapma
- Git commit ya da push yapma (git pull --rebase hariç, o da yalnızca gerekirse).
- Assets/Scripts, Packages, ProjectSettings, Assets/Art/Test_34 ve mevcut Assets/Art/Iso dosyalarına dokunma, üzerine yazma.
- Zemin, eşya ve duvar üretme. Yürüme animasyonu bu brief'te YOK (karakterler onaylanınca ayrı iş).
- PixelLab dışında üretim aracı kullanma.
- Onay beklemeden bir sonraki faza geçme.

## Teslim
Her faz sonunda art-brief-karakter-result.md dosyasına yeni bölüm EKLE (eskiyi silme): generation dökümü (çağrı başına, toplam, kalan), dosya listesi (yol, piksel boyutu, pivot x ve y ALTTAN), ölçümler (boy, renk sayısı, yarı saydam piksel), seçtiğin araç/ayar ve gerekçe, sorunlar. Sonra kısa bir özet ver, masaüstü klasöründe ne bulunduğunu yaz ve onay bekle.
