# Performans ve mimari değişiklikleri (20.11.2026)

Kod incelemesinden çıkan beş başlığın uygulaması. Çekirdek testleri ve Game katmanı derleme denetimi (`core/`) geçiyor;
Unity'de **gözle doğrulanması gerekenler** en altta listeli (bu değişiklikler Unity çalıştırılmadan yazıldı).

## 1. Sabit adımlı simülasyon

- `Core/Time/FixedStepper`: mantık saniyede 10 adım işler (kare başına en çok 4 adım; takılan karenin fazlası atılır).
  Mobil 30 FPS ile masaüstü 60 FPS aynı sonucu verir.
- `GameRoot.SimulateTick` yalnızca mantığı yürütür; `RenderFrame(alpha)` karakter ve hayvanları iki adım arasında ara
  değerler (`SmoothPosition`). Büyük sıçrama (bölge değişimi, ileri sarma) ara değerlenmez.
- Seyrek sistemler: dekor kararması ve kandil yağı ~15 oyun dakikasında bir (`LifeSystems.CoarseStepDays`); kayıttan
  önce birikim boşaltılır.
- Bozulma: envanter değişmediyse parti eşitlemesi yapılmaz (`Household.InventoryVersion`); 866 tanım yerine yalnızca
  bozulabilen 103 tanım gezilir.

## 2. Otonomi ve dizinler

- Giderecek bir şey bulamayan otonomi ~3 oyun dakikası yeniden aramaz (`JobRunner.AutonomyRetryDays`). Kritik
  ihtiyaç (açlık, enerji, ısınma) bekletilmez.
- `WorldState.GetItem(id)` sözlükten, `GetItemAt(karo)` tembel karo dizininden okur.
- A*: ikili yığın ve iş parçacığı başına tampon. Sıralama kuralı aynı olduğu için yollar birebir aynıdır
  (`PathfinderHeapTests` eski uygulamayla karşılaştırır). Düğüm sınırı bilerek eklenmedi: çiftlikten şehre uzun yollar var.

## 3. Görünümler

- `ItemViews` (GameRoot'tan ayrıldı): eşya başına görünüm kovası. `WorldState.TakeChanges` yalnızca değişen eşyaları
  verir; onlar yerinde yeniden kurulur. Kaynağı belirsiz değişiklik, kat, duvar kipi, bölge, mevsim ve halı sırası
  değişiminde bütün bölge kurulur. Diğer bölgelerin eşyası hiç kurulmaz.
- `AnimalViews` (GameRoot'tan ayrıldı): adımda eşitlenir, karede ara değerlenir.
- Zemin: karo başına GameObject yerine kat başına Tilemap (`GroundLayer`). Yalnızca değişen karo, toplu `SetTiles` ile
  yazılır. `GroundLayer.UseTilemap = false` eski SpriteRenderer yoluna döner (karşılaştırma için).

## 4. Atlas ve katman önbelleği

- **Thatchwick > Görsel atlasını kur**: görseller `Assets/Art/IsoAtlas/iso_atlas_NN.png` sayfalarına (2048 px) piksel
  tam dizilir; oyun yapısından (build) önce kendiliğinden de çalışır. Atlas kurulana kadar oyun eski dokularla çalışır;
  sonradan eklenen görsel de bir sonraki kuruluma kadar kendi dokusuyla çalışır.
- Atlas dışı: `katman/` (oyunda piksel piksel okunur), `icon_`, `viv_`, `ui_` (IMGUI dokunun tamamını çizer).
- `LayeredCharacterArt`: LRU önbellek (dolunca yalnızca en eski kare atılır); görünüm anahtarı karakter başına saniyede
  bir yenilenir.

## 5. GameRoot ve arayüz

- GameRoot'tan ayrılanlar: `ItemViews`, `AnimalViews`, `Shadows`, `Ui/HudBar`. Kalan büyük parçalar: kurulum (Awake),
  kayıt/yükleme, köylü programları, etkileşim menüsü.
- UI Toolkit altyapısı: `Ui/UiHost` (sahne ve asset gerektirmez; arayüz kitinin dokularını kullanır), `KitButton`,
  `HudBar`. **F4** küçük HUD'u UI Toolkit sürümüne çevirir (tercih saklanır); varsayılan eski IMGUI HUD'dur, çünkü yeni
  HUD Unity'de henüz gözle doğrulanmadı.
- Küçük HUD'un metinleri iki sürümde de `ui.hud.*` ve `ui.season.*` anahtarlarından gelir.
- +18 paketi mobilde açılamaz (`AdultPackAllowed`); düğmesi yalnızca masaüstünde görünür.

### Arayüz geçişinin kalanı

Paneller hâlâ IMGUI: BuildMode, CharacterCreator, FamilyPanel, StatusHud, HomePanel, RequestPanel, VillagePanel,
VivariumView, ShopPanel, MarketPanel, FishingView, MinePanel, StoragePanel, KeepsakePanel, LanguageMenu, etkileşim
menüsü, konuşma balonları, eşya etiketleri ve HUD'un "Ayrıntı" bölümü. Sıra önerisi: önce her karede çizilenler
(balonlar, etiketler, StatusHud), sonra küçük paneller, en son BuildMode ve CharacterCreator. Her panel taşınırken
metinleri `ui.*.json` anahtarlarına geçer.

## Unity'de doğrulanacaklar

1. Zemin (Tilemap): çim, yol, su, kıyı, zemin stilleri, bodrum ve üst kat, harita kenarı şeridi eskisiyle aynı yerde ve
   renkte mi. Sorun varsa `GroundLayer.UseTilemap = false` ile karşılaştır.
2. Yürüme: karakter ve hayvanlar 1x, 2x, 3x hızda akıcı mı; bölge geçişinde kayma var mı.
3. Eşya: koyma, taşıma, kaldırma, masaya dekor, halı sırası, duvar kipi, kat değişimi sonrası görünüm doğru mu;
   büyüyen bitkinin boyu ve gölgesi doğru mu.
4. Atlas: menüden kur; eşya, duvar, karakter ve hayvan görsellerinde kenar taşması var mı.
5. F4: yeni HUD görünüyor mu, yazı tipi doğru mu, düğmeler tıklanıyor mu, HUD üstüne tıklama dünyaya geçmiyor mu.
6. Profiler: kare başı GC, `GameRoot.Update` ve `OnGUI` süreleri (önce/sonra).

---

# İkinci tur (20.11.2026): ölçümlü düzeltmeler

İkinci inceleme gerçek başlangıç dünyasında ölçüm yaparak ilerledi. Ölçüm aracı repoda: `tools/offline-tests/bench.sh`
(Release derlemesi; 1.494 eşya, 30 karakter, 3x hız, 2 oyun günü, saniyede 10 adım). Sayılar bu sunucunun
masaüstü işlemcisinden; telefonda birkaç kat büyür. Tekrarlar arasında ±%15 oynar.

| Ölçüm | Önce | Sonra |
|---|---|---|
| Çekirdek simülasyon, gerçek saniye başına | 16-17 ms | 5-6 ms |
| Karakterler (otonomi), adım başına | 0,86 ms | 0,16 ms |
| Isı, adım başına | 0,44 ms | 0,008 ms |
| Bellek ayırma | 519 KB/sn | 95 KB/sn |
| Yol bulma, 108 karoluk yol | 14-23 ms, 1,1 MB | 8-11 ms, 4 KB |
| Yol bulma, 167 karoluk yol | 20-28 ms, 2,8 MB | 19-24 ms, 13 KB |
| Bir duvar parçası değişince oda haritası | 8-9 ms | 4-5 ms |
| Kayıt dosyası | 1.695 KB | 824 KB |

## Yapılanlar

- **Otonomi:** `WorldState.NeedItemsFor(need)` ve `ItemDef.AutonomyInteractions(need)`. Arama yalnızca o ihtiyacı
  giderebilecek eşyaları gezer; sıra korunduğu için seçim aynıdır. Bekleme süresi gerçek zamana bağlı (1,5 sn).
- **Isı:** oda sınırları oda haritası değişince bir kez çıkarılır; adım yalnızca iç odalar üzerinden yürür. Yalıtım
  katsayıları harita ya da eşya değişince yenilenir. `IsiOnbellekTests` eski uygulamayla karşılaştırır.
- **Kenarlar:** `GridMap` kenar türlerini düz dizilerde de tutar; `GetEdge`, `CanStep` ve oda tespiti sözlük yerine
  diziden okur. `OdaHaritasiHizliTests` gerçek dünyada ve rastgele haritalarda eski uygulamayla karşılaştırır.
- **Yol bulma:** düğüm nesnesi yerine karo indeksli diziler; yollar aynı. Süre kazancı sınırlı kaldı (aşağıda).
- **Kayıt (sürüm 3):** karo dizileri koşu uzunluğuyla (`TileRle`), sıcaklık oda başına. Sürüm 1-2 kayıtları okunur.
- **Otomatik kayıt:** gün dönümünden 3 sn sonra (günlük işlerle aynı kareye binmez).
- **Hata yalıtımı:** `SimGuard`; her sistem ve her karakter kendi korumasında. Günlük:
  `persistentDataPath/thatchwick_errors.log`.
- **Metne bağlı mantık:** tilki ve yaban domuzu uyarı düzeyi sonuçtan; stil seti kimlik sabitleri (`ItemCatalog`).
- **Arayüz alan dosyaları:** `ui.<alan>.<dil>.json`. Yeni metin kendi alan dosyasına yazılır (çakışmayı azaltır).
- **Testler:** `tools/offline-tests/run.sh` CI ile aynı xunit kurallarında; `XunitShim.cs` gerçek xunit'te olmayan
  imza içermez.

## Çözülmeyenler

- **Uzun yollarda süre:** asıl maliyet genişletilen düğüm sayısı. Yol tercihli kestirim (her karo kaldırım hızında
  varsayılır) aramayı haritanın büyük bölümüne yayıyor. Kalıcı çözüm bölge geçitleri üzerinden iki aşamalı arama ya
  da aramayı adımlara yaymak; ikisi de `JobRunner`'daki 14 çağrı yerini etkiler.
- **Gün dönümü:** günlük işler tek adımda ~30-50 ms (masaüstü). Kayıt ayrıldı ama günlük işler yayılmadı; sıraları
  birbirine bağlı ve testler tek adımda bitmesini bekliyor.
- **Yeni dünya kurulumu ~1 sn:** süre yerleşim kurallarında (oda döşeme, doğrulama); oda tespiti ve yol bulma değil.
- **Oda haritası:** hâlâ bütün dünya yeniden hesaplanıyor (4-5 ms); yalnızca değişen odaları güncellemek ayrı iş.
