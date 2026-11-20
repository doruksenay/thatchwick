# Yayın altyapısı

20.11.2026'da eklendi (üçüncü inceleme, Project belgeleri 63 ve 63b). Oyunun mağazaya çıkmadan önce ihtiyaç duyduğu
ortak parçalar: hata toplama, başarımlar, mağaza katmanı, masaüstü ayarları, derleme hattı. Bu belge neyin nerede
olduğunu ve senin (kullanıcının) yapacağı adımları anlatır.

**Durum:** kod Unity'siz ortamda yazıldı; çekirdek testlerden, Game katmanı derleme denetiminden geçti. Unity'de
henüz denenmedi (sondaki liste).

## 1. Hata mekanizması

| Parça | Yer | Ne yapar |
|---|---|---|
| `ErrorJournal` | `Core/Diagnostics` | Günlük dosyası: aynı hatayı tekilleştirir (ilk 3 tam, sonra her 200'de bir sayaç), 512 KB'ta `.1` olarak döndürür, bir oturumda en çok 200 farklı hata yazar |
| `SessionMarker` | `Core/Diagnostics` | Yarım kalan oturumu (çökme, zorla kapatma) bir sonraki açılışta bildirir |
| `ErrorReporter` | `Game` | Unity'nin yakaladığı bütün hata ve istisnaları (arayüz, Update, arka plan), `SimGuard`'ı ve kayıt hatalarını tek günlükte toplar |
| Bildirimler | `GameRootPlatform.cs` | Yeni hata olunca oyuncuya tek satır (30 saniyede en çok bir kez); kayıt yazılamazsa uyarı ve 20 saniye sonra yeniden deneme |
| Ayarlar | `GameRootPlatform.cs` | "Hata raporunu kopyala" (panoya: sürüm, cihaz, günlüğün sonu) ve "Günlük klasörünü aç" |

Dosyalar `Application.persistentDataPath` altında (Windows: `%USERPROFILE%\AppData\LocalLow\Doruk Senay\Thatchwick\`):

- `thatchwick_errors.log`, `thatchwick_errors.log.1`: hata günlüğü
- `thatchwick_session.lock`: oturum işareti (düzgün kapanışta silinir)
- `thatchwick_profile.json`: başarım profili
- `thatchwick_save.json`, `thatchwick_save_2.json`, `thatchwick_save_3.json` (+ `.bak`): kayıt yuvaları

Koddan hata bildirmek: `ErrorReporter.Report("kaynak", ex)`. `Debug.LogError` ve yakalanmamış istisnalar zaten
günlüğe düşer; ayrıca bildirmek gerekmez.

**Eksik:** hatalar oyuncunun cihazında kalır. Oyuncudan geliştiriciye kendiliğinden gönderim (Unity Cloud Diagnostics
ya da Sentry) hesap ve paket gerektirir; karar bekliyor.

## 2. Başarımlar

- Tanımlar: `Core/Progress/Achievements.cs` > `AchievementCatalog.Default` (22 başarım).
- Profil kayıt yuvasından bağımsızdır: bir yuvada açılan başarım her yerde açık kalır; ilerleme düşmez.
- Oyun rozetlerinden (`BadgeDef`) ayrıdır: rozet oyun içi ödül verir ve kayda yazılır; başarım ödül vermez.
- Koşullar 2 saniyede bir denetlenir (`CheckAchievements`): hesap sayaçları (`AccountProgress.Record`), hesap
  seviyesi, kuşak, geçen yıl, yadigâr sayısı.
- Liste: Ayarlar > "Başarımlar" (UI Toolkit penceresi, `Ui/AchievementsPanel.cs`).
- +18 paketine bağlı başarım yoktur (paket ayrı satılır).

Yeni başarım eklemek:
1. `AchievementCatalog.Default`'a satır (`ach.<kısa_ad>`; kimlik yayından sonra değişmez).
2. `ui.achievements.en.json` ve `ui.achievements.tr.json`'a `ui.ach.<kısa_ad>.name` ve `.desc`.
3. Aşağıdaki tabloya ve Steamworks'e API adı (`ACH_<KISA_AD>`).
İlk ikisini test denetler (`Basarim_Katalog_KimliklerTekil_MetinlerIkiDildeVar`).

### Steamworks'e girilecek API adları

| API adı | Ad (EN) | Koşul |
|---|---|---|
| `ACH_FIRST_HARVEST` | First Harvest | 1 hasat |
| `ACH_HARVEST_100` | Full Granary | 100 hasat |
| `ACH_HARVEST_1000` | Bread for the Village | 1.000 hasat |
| `ACH_CHOP_50` | Seasoned Woodcutter | 50 ağaç kesme |
| `ACH_FIRST_ROOM` | A Roof Overhead | ilk kapalı oda |
| `ACH_FIRST_CONTRACT` | A Deal Is a Deal | 1 köy işi |
| `ACH_CONTRACTS_25` | Pillar of the Village | 25 köy işi |
| `ACH_CHAT_100` | Good Neighbour | 100 sohbet |
| `ACH_PET_25` | Gentle Hand | 25 hayvan sevme |
| `ACH_FIRST_RIDE` | In the Saddle | ilk biniş |
| `ACH_MARRIAGE` | Wedding Day | evlenme teklifi |
| `ACH_FIRST_HEIR` | A New Head of the House | 2. kuşak |
| `ACH_GENERATION_3` | Three Generations | 3. kuşak |
| `ACH_GENERATION_5` | Old Family | 5. kuşak |
| `ACH_GENERATION_10` | Dynasty | 10. kuşak |
| `ACH_FIRST_HEIRLOOM` | Handed Down | ilk yadigâr |
| `ACH_YEAR_1` | Four Seasons | 1 yıl |
| `ACH_YEAR_10` | Ten Winters | 10 yıl |
| `ACH_YEAR_25` | Quarter of a Century | 25 yıl |
| `ACH_LEVEL_5` | Finding Your Feet | hesap seviyesi 5 |
| `ACH_LEVEL_10` | Seasoned Hand | hesap seviyesi 10 |
| `ACH_LEVEL_20` | Master of the Craft | hesap seviyesi 20 |

## 3. Mağaza katmanı (Steam)

`Game/Platform/`: `IPlatform` arayüzü, `NullPlatform` (mağazasız) ve `SteamPlatform` (yalnızca `STEAMWORKS_NET`
sembolüyle derlenir). Oyun kodu mağazayı doğrudan çağırmaz; Game Center ve Play Games ileride aynı arayüze takılır.

Steam'i bağlamak için (Steamworks hesabı açıldıktan sonra):
1. Unity > Package Manager > "Add package from git URL": Steamworks.NET deposunun güncel sürümü
   (`https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net#<sürüm>`). Paket
   `STEAMWORKS_NET` sembolünü kendisi ekler.
2. `Assets/Scripts/Game/Thatchwick.Game.asmdef` > Assembly Definition References'a `com.rlabrecque.steamworks.net`.
3. `Game/Platform/Platform.cs` > `PlatformConfig.SteamAppId` ve `SteamAdultDlcId` (0 iken Steam başlatılmaz).
4. Editörde denemek için proje köküne `steam_appid.txt` (içinde uygulama kimliği); repoya eklenmez.
5. Steamworks > Stats & Achievements: yukarıdaki tablo.
6. Steamworks > Cloud > Auto-Cloud: kök `WinAppDataLocalLow`, alt klasör `Doruk Senay/Thatchwick`, desenler
   `thatchwick_save*.json` ve `thatchwick_profile.json`.

+18 paketi: `AdultPackAvailable` artık `PlatformServices.Current.OwnsDlc("adult")` sorar. Mağazasız geliştirme
derlemesinde paket açıktır; Steam derlemesi Steam'e bağlanamazsa kapalıdır; mobilde hiç yoktur.
İçerik hâlâ ana derlemenin içindedir; mobil derlemeden tümüyle çıkarmak ayrı iştir (belge 49 §4).

## 4. Masaüstü ayarları

Ayarlar ekranına eklendi (yalnızca masaüstü oyuncusunda görünür): tam ekran / pencere, çözünürlük, dikey
eşitleme. `Esc`: açık pencereyi kapatır; pencere yokken ayarları açar (kaydet, ana menü, çıkış burada).
Sürüm numarası ayarların altında (`ProjectSettings > bundleVersion`, şu an `0.1.0`).

**Eksik:** tuş atama ve kumanda desteği yok. Kumanda gezinmesi IMGUI panelleri UI Toolkit'e taşınmadan yapılamaz.

## 5. Derleme ve test hattı

| İş akışı | Ne zaman | Gereken |
|---|---|---|
| Core tests | her PR; `main`'e kod push'u (yalnız `art/`, `audio/`, `Assets/Art/` push'larında çalışmaz) | yok |
| Unity tests | Unity tarafına dokunan her PR; elle | `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` |
| Unity build | elle; `v*` etiketi | aynı secret'lar |

Secret'lar yokken Unity işleri hata vermeden atlanır. Eklendiği an kendiliğinden çalışır.
Lisans dosyasını üretmek Unity Hub kurulu bir makine ister (GameCI "activation" belgesi).

Koruma testleri (`KorumaTests`):
- **Sabit metin sayacı:** `Assets/Scripts` altında dosya başına sabit Türkçe metin satırı sayısı
  `tools/loc/sabit_metin_tabani.json`'daki tabanı aşamaz. Yeni metin anahtarla girer. Metin taşıdıkça tabanı düşür:
  `THATCHWICK_SABIT_METIN_YAZ=1 tools/offline-tests/run.sh KorumaTests`.
- **Dönem kuralı:** içerik adlarında (İngilizce tablo) yasak sözcük olamaz; bilinçli istisnalar testte listelidir.

## 6. `main` koruması

Bu oturum repo ayarlarını değiştiremiyor; kurallar hazır, içe aktarması sende:
GitHub > Settings > Rules > Rulesets > New ruleset > **Import a ruleset**.

1. `.github/rulesets/main-gecmis.json`: `main` silinemez, zorla push edilemez. Hiçbir akışı bozmaz; hemen açılabilir.
2. `.github/rulesets/main-pr-test.json`: `main`'e yalnızca PR ile ve "test" yeşilken girilir. **Kapalı gelir.**
   Açılırsa bulut işçilerinin (Art worker, Batch worker, Ses üretimi) doğrudan push'ları da reddedilir: GitHub,
   Actions'ın kendi anahtarını kural dışı tutmaya izin vermiyor. Açmadan önce işçilerin ayrı bir anahtarla
   (deploy key) push etmesi ve o anahtarın kuralın "Bypass list"ine eklenmesi gerekir. O güne kadar kural
   `CLAUDE.md`'de yazılıdır ("Dallar ve main").

## 7. Unity'de bakılacaklar

- Açılış: konsolda "Platform: yok" satırı; hata yok.
- Ayarlar > "Başarımlar": pencere açılıyor, sayfalar dönüyor, `Esc` ve "Kapat" kapatıyor. Başlık ekranında da.
- İlk hasatta "Başarım açıldı" mesajı; oyunu kapatıp açınca listede açık.
- Ayarlar > "Hata raporunu kopyala": panoda sürüm, cihaz ve günlük.
- Bilerek hata: bir panelde geçici `throw` → oyun sürüyor, tek satır uyarı, günlükte tek kayıt.
- Derlemede (editör dışı): tam ekran / pencere ve çözünürlük düğmeleri; pencere yeniden boyutlanıyor.
- `Esc`: oyun içinde ayarları açıp kapatıyor; canlılık evi açıkken onu kapatıyor.
