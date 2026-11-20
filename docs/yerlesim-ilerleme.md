# Yerleşim sistemi: ilerleme ve devam notu

Görev: Project belgesi 44 (tek prompt, A'dan D'ye). Bağlam sıkışırsa buradan devam et. Her aşamanın sonunda bu dosya güncellenir.

## Ortam notu (19.11.2026 bulut oturumu)
- Bu oturumda depoya **push izni yok** (add_repo push reddedildi) ve kullanıcının bilgisayarı bağlı değil: MCP for Unity köprüsüyle
  **ekran görüntüsü alınamadı**. Commit'ler yerelde yapıldı; teslimde git paketi (bundle) ve uygulama komutları verildi.
- Çekirdek testleri: .NET 8 SDK + `tools/offline-tests/XunitShim.cs` ile çevrimdışı derlenip çalıştırıldı. Game katmanı
  `core/Thatchwick.GameCheck` (sahte Unity tipleri) ile derleme denetiminden geçirildi.
- Rapor aracı: `dotnet run --project tools/layout-report` (ayrıntı: sonuna `-- list` ya da `-- md`).

## Aşama A: İnceleme, hata envanteri, debug katmanı (tamam)

### İnceleme
**1. Eşya yönü.** `ItemDef`'te yön alanı yok. Yerleşim kaydında (`ItemInstance.Rotation`, 0-3) yalnızca ayak izi dönüşü var.
Mantıksal yön ile çizilen görsel arasında bağ yok: `IsoArt.Item` dönüşün yalnızca teklik/çiftliğine bakıyor (`Rotation % 2`).
Kare eşyada tek dönüşler aynalanıyor, 2 ve 3 dönüşleri 0 ve 1 gibi çiziliyor (arkası dönük görünüm hiç yok). Neredeyse her
eşya tek görselle çiziliyor (`_se`; tahta sandalye `_sw`). Ön yüz, etkileşim hücresi, ait olduğu oda, iç/dış bilgisi yok.

**2. Görsel envanteri.** IsoArt eşlemesinde 74 eşya var (tablo aşağıda). `Assets/Art/Iso/_aday` altında **225 görsel üretilmiş
ama eşlenmemiş** (bütün ağaçlar, merdivenler, kiler kapağı, sehpa masa, han bankı, ceviz masa, yer halıları, 24 duvar eşyası,
kuyu kovası hariç çoğu dış eşya). Bunlar oyunda kutu olarak çiziliyor: çiftlikte, şehirde ve evlerde 213 eşyanın (bitki hariç) 109'u kutu.
Ağaçların hepsi (erik hariç) kodla çizilen yer tutucu ağaç; "yapay görünüm"ün büyük kısmı buradan.

**3. Şehir, dükkânlar, evler.** Hepsi elle koordinat: `Regions.TownMap` (Painter.I/G/Wall/On) ve çiftlik için
`GameRoot.PlaceDemoItems`. 6 köylü evinde aynı üç eşya aynı köşeye. Ağaçlar sabit tohumlu rastgele (`Random(52)`) köşe
bölgelerine; yalnızca yol/taş/su atlanıyor. Tek denetim `WorldState.CanPlaceFloor` (harita, karo, çakışma, su, merdiven ucu,
kapalı oda). Kapı önü, yön, etkileşim, oda türü, iç/dış denetimi yok. Duvar eşyaları `Painter.Wall(..., 0|3)` ile ön duvarın iç
yüzüne asılıyor: yorum "görünen arka duvar" diyor ama kenar 0 ve 3 kameraya arkası dönük yüz, kesit kipinde çizilmiyor.

**4. Yapı modu (BuildService).** `CanPlaceFromInventory` → `WorldState.CanPlaceFloor/Rug/Wall/Ceiling/Surface` + arsa sahipliği,
kilit, envanter. Döndür düğmesi 0-1-2-3 serbest döner; geçerli yön kavramı yok.

**5. Kapı, pencere, duvar, oda.** `GridMap` kat başına kenar tutar (`EdgeKind`: Wall/Door/Window/HalfWall/Fence/Gate/Railing).
`RoomMap` taşma dolgusuyla odaları bulur (iç/dış, kapı ve pencere sayısı); oda türü yok (yalnızca satın alma filtresinde
`ItemDef.Rooms`).

**6. Katlar.** Kat sistemi var (`GridMap` -2..1, `SetTile`, merdiven eşyası 1x2 → `RebuildLinks`, otomatik çatı `RoofBuilder`,
`WorldView.ViewLevel`). `Pathfinder` `LinksFrom` ile katlar arası yol buluyor: NPC'ler merdivenle kat değiştirebilir.
Şehir binaları tek katlı (z≠0 karo yok). Köylü yerleri (`Regions.Places`) her yüklemede yeniden kuruluyor (kayda yazılmıyor).

**7. Bitki örtüsü ve bahçe.** Ağaç/çalı yerleşimi yukarıdaki gibi; bahçe/avlu kavramı yok (yalnızca otacının çitli ot bahçesi ve
ağıl `Painter.Fence`). Dekor ve meyve ağacı evlerin kenarına elle.

### Değişiklikler
- `Core/Layout/StarterWorld.cs`: yeni oyunun haritası ve başlangıç eşyaları GameRoot'tan Core'a taşındı (davranış aynı);
  doğrulayıcı ve testler aynı dünyayı Unity'siz kurar.
- `Core/Layout/PlacementValidator.cs`: doğrulayıcı iskeleti; kurallar: çakışma, sınır (duvar/çit üstünden taşan ayak izi,
  plan arsası dışı), kapı önü (içte 1, dışta 2 karo; bahçe kapısı ve oda arası kapı 1). İhlal: kural kodu + okunur neden.
  Oyuncu kipinde çakışma/sınır dışındaki kurallar uyarı.
- `Core/Layout/Facing.cs` (Dir4 ve ekran eşlemesi), `LayoutPlan.cs` (arsa ve oda bölgeleri), `Reach.cs` (ulaşılabilirlik).
- `Game/PlacementDebugOverlay.cs`: F3 (yalnızca editör/geliştirme derlemesi). Izgara, ayak izi, yön oku, kapı önü, etkileşim,
  engel, ulaşılamayan karo, ihlaller kırmızı; sol üstte kural sayıları.
- `tools/layout-report`: envanter raporu. Testler: `YerlesimDogrulayiciTests` (11).

### Hata envanteri (başlangıç, Aşama A kuralları + sezgisel denetimler)
Ekran görüntüsü alınamadı (bkz. ortam notu); envanter, oyunun yeni dünyası Unity'siz kurulup kodla çıkarıldı.
"yanlış yön", "yanlış yer", "etkileşim", "iç/dış", "görsel-iz" bu aşamada rapor aracındaki sezgisel denetimlerden geliyor
(Aşama B'de doğrulayıcının kendi kurallarıyla değişti).

```
Eşya: 1188, yerleştirilemeyen: 2, ihlal: 38 (hata 38)
  iç/dış mekân                         14
  yanlış yön                           11
  yanlış yer                            9
  etkileşim noktası kapalı              2
  kapı önü kapalı                       1
  gorsel-iz                             1
Yere göre:
  Balıkçı                               4   iç/dış mekân 1, yanlış yer 3
  Çiftlik                               1   iç/dış mekân 1
  Çiftlik evi                           2   yanlış yön 2
  Çiftlik evi (bodrum)                  1   iç/dış mekân 1
  Çiftlik evi (üst kat)                 1   yanlış yön 1
  Çömlekçi                              1   iç/dış mekân 1
  Değirmen ve fırın                     2   iç/dış mekân 2
  Demirci                               5   iç/dış mekân 4, yanlış yer 1
  Marangoz                              4   yanlış yer 1, yanlış yön 3
  Meyhane                               8   etkileşim noktası kapalı 2, yanlış yer 2, yanlış yön 4
  Orman                                 1   yanlış yön 1
  Otacı                                 2   gorsel-iz 1, yanlış yer 1
  Pazar (Kerem)                         1   iç/dış mekân 1
  Saraç                                 1   yanlış yer 1
  Şehir                                 3   kapı önü kapalı 1, iç/dış mekân 2
  Taşçı                                 1   iç/dış mekân 1
Görselsiz (kutu) eşya / toplam, yere göre (bitki hariç):
  Balıkçı                               5 / 9
  Çiftlik                               9 / 15
  Çiftlik evi                           6 / 18
  Çiftlik evi (bodrum)                  4 / 11
  Çiftlik evi (üst kat)                 3 / 12
  Çömlekçi                              4 / 8
  Değirmen ve fırın                     2 / 7
  Demirci                               6 / 7
  Güney Yamaçları                       0 / 2
  Hayvan pazarı                         7 / 7
  Irmak ve göl                          0 / 1
  Kâtip                                 6 / 12
  Köy yolu                              0 / 3
  Köylü evi 1                           0 / 3
  Köylü evi 2                           0 / 3
  Köylü evi 3                           0 / 3
  Köylü evi 4                           0 / 3
  Köylü evi 5                           0 / 3
  Köylü evi 6                           0 / 3
  Marangoz                              7 / 8
  Meyhane                              13 / 22
  Orman                                 1 / 2
  Otacı                                 8 / 11
  Pazar (Kerem)                         7 / 13
  Saraç                                 4 / 6
  Sazlık                                1 / 1
  Şehir                                 8 / 10
  Şehir meydanı                         1 / 5
  Taşçı                                 2 / 4
  Tepe ve taş ocağı                     3 / 4
  Terzi                                 7 / 10
```

| Yer | Konum | Eşya | Sorun | Neden | Kök neden |
|---|---|---|---|---|---|
| Balıkçı | (55,22) | Balık sepeti (tuzak) | iç/dış mekân | Balık sepeti (tuzak) dış mekân eşyası, kapalı odada. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Balıkçı | (54,18) | Kürek askısı | yanlış yer | Kürek askısı ön duvarın iç yüzüne asılı (kenar 3); kamera bu yüzü görmez. | Painter.Wall yan 0/3 (yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü) |
| Balıkçı | (55,18) | Kurutulmuş balık dizisi | yanlış yer | Kurutulmuş balık dizisi ön duvarın iç yüzüne asılı (kenar 0); kamera bu yüzü görmez. | Painter.Wall yan 0/3 (yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü) |
| Balıkçı | (58,18) | Ağ perde | yanlış yer | Ağ perde ön duvarın iç yüzüne asılı (kenar 0); kamera bu yüzü görmez. | Painter.Wall yan 0/3 (yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü) |
| Çiftlik | (34,35) | Tahta tabure | iç/dış mekân | Tahta tabure iç mekân eşyası, dışarıda. | GameRoot.PlaceDemoItems elle koordinat; eşyada iç/dış alanı yok |
| Çiftlik evi | (30,29) | Tahta sandalye | yanlış yön | Tahta sandalye bir masaya ya da odağa bakmıyor (sw). | GameRoot.PlaceDemoItems elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Çiftlik evi | (36,31) | Minderli koltuk | yanlış yön | Minderli koltuk bir masaya ya da odağa bakmıyor (se). | GameRoot.PlaceDemoItems elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Çiftlik evi (bodrum) | (36,26,kat -1) | Tohum kasası | iç/dış mekân | Tohum kasası dış mekân eşyası, kapalı odada. | GameRoot.PlaceDemoItems elle koordinat; eşyada iç/dış alanı yok |
| Çiftlik evi (üst kat) | (36,30,kat 1) | Minderli koltuk | yanlış yön | Minderli koltuk bir masaya ya da odağa bakmıyor (se). | GameRoot.PlaceDemoItems elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Çömlekçi | (43,44) | Tuğla fırını | iç/dış mekân | Tuğla fırını dış mekân eşyası, kapalı odada. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Değirmen ve fırın | (40,4) | Değirmen taşı | iç/dış mekân | Değirmen taşı dış mekân eşyası, kapalı odada. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Değirmen ve fırın | (46,4) | Tohum kasası | iç/dış mekân | Tohum kasası dış mekân eşyası, kapalı odada. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Demirci | (3,22) | Demir ocağı ve örs | iç/dış mekân | Demir ocağı ve örs dış mekân eşyası, kapalı odada. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Demirci | (3,26) | Su çekici | iç/dış mekân | Su çekici dış mekân eşyası, kapalı odada. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Demirci | (7,26) | Bileği taşı | iç/dış mekân | Bileği taşı dış mekân eşyası, kapalı odada. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Demirci | (8,24) | Çiçek ocağı | iç/dış mekân | Çiçek ocağı dış mekân eşyası, kapalı odada. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Demirci | (3,25) | Alet askısı | yanlış yer | Alet askısı ön duvarın iç yüzüne asılı (kenar 3); kamera bu yüzü görmez. | Painter.Wall yan 0/3 (yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü) |
| Marangoz | (5,31) | Alet askısı | yanlış yer | Alet askısı ön duvarın iç yüzüne asılı (kenar 0); kamera bu yüzü görmez. | Painter.Wall yan 0/3 (yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü) |
| Marangoz | (9,35) | Oymalı sandalye | yanlış yön | Oymalı sandalye bir masaya ya da odağa bakmıyor (se). | Regions.cs elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Marangoz | (8,35) | Boyalı sandalye | yanlış yön | Boyalı sandalye bir masaya ya da odağa bakmıyor (se, görsel yok). | Regions.cs elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Marangoz | (3,35) | Sandık-bank | yanlış yön | Sandık-bank bir masaya ya da odağa bakmıyor (se, görsel yok). | Regions.cs elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Meyhane | (20,12) | Taş ocak | etkileşim noktası kapalı | Taş ocak dört yandan kapalı; kullanılamaz. | Regions.cs elle koordinat; etkileşim hücresi tanımı yok |
| Meyhane | (29,12) | Mayalama fıçısı | etkileşim noktası kapalı | Mayalama fıçısı dört yandan kapalı; kullanılamaz. | Regions.cs elle koordinat; etkileşim hücresi tanımı yok |
| Meyhane | (23,12) | Maşrapa rafı | yanlış yer | Maşrapa rafı ön duvarın iç yüzüne asılı (kenar 0); kamera bu yüzü görmez. | Painter.Wall yan 0/3 (yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü) |
| Meyhane | (20,14) | Tabak rafı | yanlış yer | Tabak rafı ön duvarın iç yüzüne asılı (kenar 3); kamera bu yüzü görmez. | Painter.Wall yan 0/3 (yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü) |
| Meyhane | (21,14) | Uzun bank | yanlış yön | Uzun bank bir masaya ya da odağa bakmıyor (sw). | Regions.cs elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Meyhane | (21,16) | Uzun bank | yanlış yön | Uzun bank bir masaya ya da odağa bakmıyor (sw). | Regions.cs elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Meyhane | (25,14) | Uzun bank | yanlış yön | Uzun bank bir masaya ya da odağa bakmıyor (sw). | Regions.cs elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Meyhane | (25,16) | Uzun bank | yanlış yön | Uzun bank bir masaya ya da odağa bakmıyor (sw). | Regions.cs elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Orman | (28,34) | Ahşap bank | yanlış yön | Ahşap bank bir masaya ya da odağa bakmıyor (se, görsel yok). | Regions.cs elle koordinat; dönüş verilmiyor (0), eşyada yön/ön yüz verisi yok |
| Otacı | (10,6) | Kurutma askısı | gorsel-iz | Kurutma askısı: tanım 2x1, üretilen görsel 1x1 (IsoArt'ta bilinçli eşlenmedi). | görsel üretimi tanımla uyuşmuyor; ItemDef'te görsel boyutu ayrı tutulmuyor |
| Otacı | (6,8) | Kuru ot demeti | yanlış yer | Kuru ot demeti ön duvarın iç yüzüne asılı (kenar 3); kamera bu yüzü görmez. | Painter.Wall yan 0/3 (yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü) |
| Pazar (Kerem) | (40,14) | Tohum kasası | iç/dış mekân | Tohum kasası dış mekân eşyası, kapalı odada. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Saraç | (54,37) | Eyer askısı (duvar) | yanlış yer | Eyer askısı (duvar) ön duvarın iç yüzüne asılı (kenar 3); kamera bu yüzü görmez. | Painter.Wall yan 0/3 (yorum 'görünen arka duvar' diyor, aslında ön duvarın iç yüzü) |
| Şehir | (7,38) | Odun sepeti | iç/dış mekân | Odun sepeti iç mekân eşyası, dışarıda. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Şehir | (61,41) | Fıçı küvet | iç/dış mekân | Fıçı küvet iç mekân eşyası, dışarıda. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
| Şehir | (52,20) | - | kapı önü kapalı | Kapı önü suya açılıyor (Vertical(198,30)). | Regions.cs elle koordinat; kapı önü kontrolü yok |
| Taşçı | (20,42) | Taş yontma tezgâhı | iç/dış mekân | Taş yontma tezgâhı dış mekân eşyası, kapalı odada. | Regions.cs elle koordinat; eşyada iç/dış alanı yok |
Yerleştirilemeyenler:
  Başlangıç eşyası konamadı item.log (243,32,0): (243,32): Bu eşya yerleştirilmez.
  Başlangıç eşyası konamadı item.fish_basket_decor (288,15,0): (288,15): Bu eşya yere konamaz, bir yüzeye konur.


## Aşama B: Eşya metadata'sı, yön ve yerleşim kuralları (tamam)

### Yön
- `Core/Layout/Facing.cs`: N/E/S/W ve ekran eşlemesi tek yerde (`docs/gorsel-yonerge.md` "Yön kuralı"). N ve W kameraya dönük.
- `Core/Layout/ItemFacing.cs`: dönüş ↔ yön, geçerli yön (görsel ya da sol/sağ ayna; arkası dönük aynayla yok), çizilecek görsel,
  ön karolar. IsoArt artık yalnızca bunu çağırır; eski el tablosu (`IsoArt.Items`, `IsoArt.Plants`) `items.json`'a taşındı.
- Eski kayıtlar: tek karolu eşyada dönüş 0 kendi görselinin yönüdür (görünüş değişmez); 2-3 dönüşleri artık "görsel yok"
  sayılır ve yer tutucu çizilir (başka yönün görseli konmaz). Kayıt biçimi değişmedi, migration gerekmedi.

### ItemDef (var olan alanlar korundu, eklenenler)
`Art` (eksen/yön görselleri, lit, bitki evreleri, pivot, görsel boyutu `pxW/pxH`, ayna, izinli dönüşler), `FrontUse`
(sit/open/work/stand) ve `FrontDepth`, `Site` (indoor/outdoor/any), `Belongs` (oda türleri), `Trades` (meslek etiketleri),
`Anywhere` (bilinçli her yer: ışıklar, merdivenler), `Relation` (faceTable, bedside, nearHearth; uzun eşya kuralı `tall`'dan).
Veri `tools/layout/item_meta.py` ile dolduruldu (kategori varsayılanı + açık listeler; tekrar çalıştırılabilir, `--check`).

- 205 eşlenmemiş görsel bağlandı (bütün ağaçlar ve bitki evreleri, merdivenler, halılar, banklar, duvar eşyaları, toplama
  düğümleri); reddedilen işlerin çıktıları ve ayak iziyle uyuşmayan görseller (kurutma askısı 1x1/2x1, el merdiveni) alınmadı.
- 44 yeni eşya (Ek tablosu meslek eşyaları, komodin, sepet/çuval, dış mekân ve bitki örtüsü). Görselleri iş dosyalarında.

### Doğrulayıcı kuralları (`Core/Layout/LayoutRules.cs`)
çakışma, sınır (A) · kapı önü (A) · etkileşim (ön karo boş, duvar değil, kapıdan ulaşılabilir; ön yüzsüz eşyada en az bir yan) ·
oturma yönü (masaya ya da 3 karo içinde odağa bakar, duvara bakmaz) · ilişki (komodin yatak başında, ocak eşyası ocağın 2 karo
yanında, uzun eşya duvara sırt) · oda türü/meslek · iç/dış · görsel yönü (+ duvar eşyası ön duvarın iç yüzünde: uyarı).

### Yapı modu
Döndür yalnızca geçerli yönler arasında döner. Kapı önü, etkileşim ve yön ihlali: turuncu hayalet, ihlal karoları kırmızı,
panelde neden; yerleştirme yine yapılır. Çakışma ve sınır kesin engel (değişmedi).

### Başlangıç ihlalleri (eski yerleşim, bütün kurallar)
```
Eşya: 1188, yerleştirilemeyen: 2, ihlal: 64 (hata 55)
  etkileşim noktası kapalı             24
  ilişki                               14
  yön görseli yok                       9
  yanlış yön                            8
  yanlış yer                            4
  iç/dış mekân                          4
  kapı önü kapalı                       1
Yere göre:
```

### Emin olunamayanlar (veri)
```
EMİN DEĞİL: item.drying_rack: görsel iso_item_drying_rack_se 1x1, ayak izi 2x1: eşlenmedi (görsel boyutu ile ayak izi uyuşmuyor)
EMİN DEĞİL: item.ladder: görsel iso_item_ladder_se 1x1, ayak izi 1x2: eşlenmedi (görsel boyutu ile ayak izi uyuşmuyor)
EMİN DEĞİL: item.grave: oda türü yok (rooms boş), kural dışı kalır
EMİN DEĞİL: item.debris: oda türü yok (rooms boş), kural dışı kalır
```
- Pivotu raporda olmayan, varsayılan pivotla bağlanan görseller (ekranda kayma olabilir, bakılmalı): toplama düğümleri teresi,
  kuru dal, yabani kovan, mantar, yabani sarımsak; hamam kulübesi, lazımlık, süzgeç çukuru, mezar, mantar yatağı, zeytinyağı
  kandili, kalay testi, maşrapa, havluluk.
- Ayna bütün eşyalarda açık (hepsi sol/sağ simetrik varsayıldı); simetrik olmadığı görülen eşya `NO_MIRROR` listesine eklenmeli.

### PixelLab iş dosyaları (approved_by: pending)
- `art/queue/431-yerlesim-dukkan-esyalari.md` (30), `432-yerlesim-dis-mekan.md` (13), `433-oturma-arka-gorunum.md` (12, arkası dönük).

## Aşama C: Oda şablonları, dükkân katları ve iç mekân (tamam)

### Süreç (`Core/Layout/RoomFurnisher.cs`)
oda geometrisi → kapı, merdiven uçları, başlangıç noktaları (boş kalır) → her adım: ana eşya (duvara sırt / arka duvar / köşe /
orta / kapı yanı / uzak / tezgâh) → ilişkili eşya (`seatsAround`: oturak masaya bakar; `bedside`; `nearHearth`; `nearItem`) →
süs yalnızca kalan geçerli yerlere (yüzey yuvası, arka duvarın kameraya bakan yüzü, tavan, halı) → doğrulayıcı. Her eşya konmadan
önce dolaşım denetlenir (girişler, merdiven girişleri, bütün ön karolar ve "en az bir yanı" istekleri kapıdan ulaşılır kalmalı).
Doğrulayıcıdan geçmeyen isteğe bağlı eşya çıkarılır; zorunlu eşya geçmezse başka tohumla yeniden denenir (6 deneme).
Sabit tohum: oda tohumu = Seeds.Derive(oyun tohumu, bina:kat:oda); aynı tohum aynı odayı verir (testli).

### Veri (`Assets/Resources/Content/layouts.json`, kod içinde koordinat yok)
25 oda şablonu (köylü evi, üst kat ev, küçük ev, han odası, çiftlik evinin üç katı, 12 dükkân zemini, 6 bodrum) ve 14 bina şablonu
(Ek tablosu kat planı). Zorunlu meslek eşyaları zorunlu adım; şablon onlarsız geçmez (test: `Sehir_DukkanZemini_EkTablosundakiZorunluEsyalar`).

### Dükkân evleri (`Core/Layout/TownBuilder.cs`)
- Plan (bina dikdörtgeni, kapı) `Regions.TownBuildings` tablosuna alındı (belge 40 planı değişmedi); iç döşeme satırları silindi.
- Üst kat ve bodrum bina şablonundan: üst katta zemin kattaki kapı/pencere hizasında pencere, bodrumda düz duvar ve taş zemin;
  merdivenler ön duvarlara (batı/kuzey) dayalı, girişi ve inişi bina içinde, kapıdan uzak; üst kat merdiven boşluğu açık.
- Han üst katı: inişin sırası koridor, iki yanında dört oda (3 kiralık han odası + Şerife'nin odası), her odanın koridora kapısı.
- Tek katlı kalanlar: demirci, taşçı (Ek tablosu). Hayvan pazarı ağıl olarak kaldı (kulübe eklenmedi, bkz. kalanlar).
- Sahiplerin evi üst katta: `Regions.Places(map)`; Kerem, Hatice, Zeynep ve Ayşe'nin `home` anahtarı life.json'da dükkânın
  üst katına alındı. Üst katı olmayan eski haritada eski evine düşer (test). NPC yol bulma merdivenle (test: meydandan her
  sahibin üst kattaki evine yol var). `NearestWalkable` artık katı koruyor.
- Bina içinden geçen yol temizlendi (eski planda kuzey yolu meyhanenin içinden geçiyordu).

### Çiftlik evi
Belge 38'deki iki katlı ev aynı (yapı ve merdivenler), içi şablondan: zemin mutfak-yemek-oturma, üst kat üç yataklı yatak odası,
bodrum kiler ve atölye. Oyuncu ve Maud'un başlangıç karoları, kapı içi ve merdiven uçları boş (test). Oyuncunun evi yalnızca yeni
oyunda döşenir; eski kayıtların yerleşimine dokunulmaz (dünya yalnızca yeni oyunda kuruluyor).

### Sonuç (tohum 1; 20 farklı tohumda da 40/40)
```
Şablon: 40 oda, kusursuz 40, eksik zorunlu 0, doğrulayıcının çıkardığı isteğe bağlı 0
Eşya: 1375, yerleştirilemeyen: 2, ihlal: 4 (hata 4)
  sınır                                 1
  kapı önü kapalı                       1
  ilişki                                1
  iç/dış mekân                          1
Yere göre:
  Balıkçı                               1   kapı önü kapalı 1
  Marangoz / mobilyacı                  1   ilişki 1
  Saraç ve tabakhane                    1   iç/dış mekân 1
  Şehir                                 1   sınır 1
Görselsiz (kutu) eşya / toplam, yere göre (bitki hariç):
```
Kalan 4 ihlal dış mekân (Aşama D). Önce/sonra kat planları: `docs/yerlesim-kat-planlari.md` (ekran görüntüsü yerine ASCII).

## Aşama D: Dış mekân, bahçeler, bitki örtüsü ve yollar (tamam)

### Kurallar (`Core/Layout/OutdoorRules.cs`, doğrulayıcıya eklendi)
kapıdan yola patika ve yol ağı (meydan taşı ve köprü yürüyüş yolu sayılır; kopuk parça yok) · yol ve patika üstünde eşya yok ·
kuyu meydanın ortasında ya da yol kesişiminin 3 karo içinde · bank yola ya da meydana bakar, sırtı duvar/çit/alçak duvara ·
yağmur fıçısı bina köşesinde duvar dibinde · odun yığını yan ya da arka duvarda (kapı duvarında değil) · ağaç binaya en az 1 karo
boşluk, kameraya bakan cephenin (batı ve kuzey) 2 karo önüne konmaz · meslek dış eşyası kendi dükkânının 5 karo içinde ·
tabela kapının 1-2 karo yanında, yola değen yerde.

### Yerleşim (`Core/Layout/OutdoorBuilder.cs`, `Vegetation.cs`, layouts.json `gardens`)
- Plan korundu (belge 40); Regions'tan şehir ve köy yolunun bütün elle donatısı silindi. Her binanın kapı yönü plandan okunur.
- Ayrılan karolar: kapı önü (3 karo) ve iki yanı, bölge geçitleri, yol; bahçe bölgesi = binaya 1-4 karo, en yakın bina bu olan açık karo.
- Meydan: kuyu ortada (yanında kova ve çamaşır leğeni), banklar meydana bakar ve arkalarına alçak taş duvar, ilan tahtası, bekçi
  ateşi, Kerem'in tezgâhı meydan kenarında.
- Tabelalar önce (kapı yanını komşu bahçe kapmasın), sonra her binanın avlu/bahçe şablonu: köylü evlerinde çitli sebze tarhı (bahçe
  kapısı yola açılır), meyve ağacı, yağmur fıçısı, odun yığını, çamaşır ipi, bank, çiçek; meslek avluları (demirci sergisi, marangoz
  hızarı ve kereste, değirmen su çarkı ırmak kıyısında, çömlekçi fırını ve sergisi, terzinin boyalı kumaşları, balıkçının ağ askısı
  ve sepeti su kıyısında, saracın tabak çerçeveleri arkada, taşçının kireç ocağı, otacının çitli ot tarhı ve arı kovanı, hanın at
  bağlama direği, fıçıları ve avlu masası), hayvan pazarının ağıl içi.
- Yol kesişimlerinde yol tabelası; köy yolunda tüccar durağı (yola bakan bank, arkasında çit, fıçı ve tohum sandığı) ve toplama
  düğümleri (ıslak sevenler dere boyunda).
- Çiftlik girişi: kapı patikasından kuyuya kısa bir patika (T kavşak), kuyu kavşağın yanında; kütük yan duvarda, yağmur fıçısı
  köşede, saman arkada, avlu masası ve dokuz taş.
- Bitki örtüsü: 2x2 hücreli seyrek aday ızgarası, türe göre en az uzaklık (ağaç 3, çalı/saz 2, çiçek/çim 1), %45-60 kümelenme, yoğunluk
  haritası (kenar +, su +, meydan −, ana yol −), türler yere göre (su kıyısı söğüt ve saz, kenar meşe/huş/çam, duvar dibi çalı/çiçek,
  yol kenarı çim öbeği), %30 genç ağaç (farklı boy). Görseli henüz olmayan çalı, çiçek ve çim öbeği konmaz (görünmez engel olmasın);
  432 gelince yeni oyunda kendiliğinden çıkar.
- Yaban haritalarının elle konmuş eşyaları da aynı kurallardan geçer: yol üstündeki kütük, sarımsak ve orman bankı konmadı.

### Yollar
- Plan düzeltmeleri (yalnızca kural ihlali olanlar): kuzey yolu meyhanenin içinden geçiyordu, doğusundan ve kuzeyinden dolanır; köylü
  evi 2'nin patikası ana yola bağlandı; balıkçı bir karo doğuya (kapı önü suydu).
- Mantıksal yol ağı GridMap yol katmanında (görselden ayrı); bağlantı testi: her dış kapı ana ağa bağlı (test: `YolAgi_*`, `Sehir_*`).
- Görsel: 004e karoları depoda (review), dual-grid Wang çizimi (WorldView.RebuildPathLayer) zaten etkin; yol değişince katman yeniden
  kurulur. Geçici bayraklar kaldırıldı: `IsoArt.PathSetSeamless`, 4-bit `PathTile`/`PathTileByMask`, `WorldView.ProceduralPathFringe`,
  `PathFringe`, `RebuildPathFringe`. Wang seti yüklenmemişse yol dikişsiz toprak karoyla çizilir.

### Sonuç (tohum 1; 10 tohumda da 0 ihlal, 77/77 şablon)
```
Şablon: 77 oda, kusursuz 77, eksik zorunlu 0, doğrulayıcının çıkardığı isteğe bağlı 1
Eşya: 1461, yerleştirilemeyen: 4, ihlal: 0 (hata 0)
Yere göre:
Görselsiz (kutu) eşya / toplam, yere göre (bitki hariç):
```
- Şehirde ağaç 62 → 83, hepsi gerçek görselle (önce erik dışındakiler kodla çizilen yer tutucuydu).
- Kutu çizilen eşya: 213'ün 109'u (%51) → 490'ın 197'si (%40). Kalan kutuların çoğu yeni eklenen eşyalar (431, 432 bekliyor).

## Final: hata envanterinin kapanışı (Aşama A satırları)
| Yer | Konum | Eşya | Sorun | Durum |
|---|---|---|---|---|
| Balıkçı | (55,22) | Balık sepeti (tuzak) | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Balıkçı | (54,18) | Kürek askısı | yanlış yer | kapandı: duvar eşyası yalnızca kameraya bakan arka duvar yüzüne (kenar 1-2); oda türü/meslek kuralı |
| Balıkçı | (55,18) | Kurutulmuş balık dizisi | yanlış yer | kapandı: duvar eşyası yalnızca kameraya bakan arka duvar yüzüne (kenar 1-2); oda türü/meslek kuralı |
| Balıkçı | (58,18) | Ağ perde | yanlış yer | kapandı: duvar eşyası yalnızca kameraya bakan arka duvar yüzüne (kenar 1-2); oda türü/meslek kuralı |
| Çiftlik | (34,35) | Tahta tabure | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Çiftlik evi | (30,29) | Tahta sandalye | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Çiftlik evi | (36,31) | Minderli koltuk | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Çiftlik evi (bodrum) | (36,26,kat -1) | Tohum kasası | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Çiftlik evi (üst kat) | (36,30,kat 1) | Minderli koltuk | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Çömlekçi | (43,44) | Tuğla fırını | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Değirmen ve fırın | (40,4) | Değirmen taşı | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Değirmen ve fırın | (46,4) | Tohum kasası | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Demirci | (3,22) | Demir ocağı ve örs | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Demirci | (3,26) | Su çekici | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Demirci | (7,26) | Bileği taşı | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Demirci | (8,24) | Çiçek ocağı | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Demirci | (3,25) | Alet askısı | yanlış yer | kapandı: duvar eşyası yalnızca kameraya bakan arka duvar yüzüne (kenar 1-2); oda türü/meslek kuralı |
| Marangoz | (5,31) | Alet askısı | yanlış yer | kapandı: duvar eşyası yalnızca kameraya bakan arka duvar yüzüne (kenar 1-2); oda türü/meslek kuralı |
| Marangoz | (9,35) | Oymalı sandalye | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Marangoz | (8,35) | Boyalı sandalye | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Marangoz | (3,35) | Sandık-bank | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Meyhane | (20,12) | Taş ocak | etkileşim noktası kapalı | kapandı: ön karolar dolaşım denetimiyle boş ve kapıdan ulaşılır |
| Meyhane | (29,12) | Mayalama fıçısı | etkileşim noktası kapalı | kapandı: ön karolar dolaşım denetimiyle boş ve kapıdan ulaşılır |
| Meyhane | (23,12) | Maşrapa rafı | yanlış yer | kapandı: duvar eşyası yalnızca kameraya bakan arka duvar yüzüne (kenar 1-2); oda türü/meslek kuralı |
| Meyhane | (20,14) | Tabak rafı | yanlış yer | kapandı: duvar eşyası yalnızca kameraya bakan arka duvar yüzüne (kenar 1-2); oda türü/meslek kuralı |
| Meyhane | (21,14) | Uzun bank | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Meyhane | (21,16) | Uzun bank | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Meyhane | (25,14) | Uzun bank | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Meyhane | (25,16) | Uzun bank | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Orman | (28,34) | Ahşap bank | yanlış yön | kapandı: oturma eşyası şablonda seatsAround ile masaya bakarak, yalnızca görseli olan (kameraya dönük) yönde |
| Otacı | (10,6) | Kurutma askısı | gorsel-iz | kapanmadı (bilinçli): kurutma askısının görseli 1x1, tanım 2x1; görsel eşlenmedi, yeni görsel 2x1 üretilmeli |
| Otacı | (6,8) | Kuru ot demeti | yanlış yer | kapandı: duvar eşyası yalnızca kameraya bakan arka duvar yüzüne (kenar 1-2); oda türü/meslek kuralı |
| Pazar (Kerem) | (40,14) | Tohum kasası | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Saraç | (54,37) | Eyer askısı (duvar) | yanlış yer | kapandı: duvar eşyası yalnızca kameraya bakan arka duvar yüzüne (kenar 1-2); oda türü/meslek kuralı |
| Şehir | (7,38) | Odun sepeti | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Şehir | (61,41) | Fıçı küvet | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |
| Şehir | (52,20) | - | kapı önü kapalı | kapandı: balıkçı binası bir karo doğuya alındı, kapı önü 2 karo kuru |
| Taşçı | (20,42) | Taş yontma tezgâhı | iç/dış mekân | kapandı: eşyaya site (indoor/outdoor/any) verildi; atölye eşyaları bilinçli 'any'; dükkân içi şablondan, dış eşya avluda |

Aşama B'nin bütün kurallarıyla eski yerleşimde ölçülen 55 hata + 9 uyarı da kapandı: yeni dünyada doğrulayıcı 0 hata veriyor
(iç mekân 40 oda, dış mekân 37 bölüm; 20 tohumda iç mekân, 10 tohumda bütün dünya denendi).

### Görsel envanteri (IsoArt eşlemesi)
Yönler dosya adındaki ekten: se = N, sw = W (kameraya dönük), ne = E, nw = S (arkası dönük). Hiçbir eşyanın arkası dönük
görseli yok. Ayna yalnızca sol↔sağ (se↔sw); duvar eşyasında öbür duvar için kullanılıyor.

| Eşya | Ayak izi | Görsel (px) | Pivot | Mevcut yönler | Ayna | Not |
|---|---|---|---|---|---|---|
| item.bed_straw | 2x1 | ? | 56,32 | sw/ne, se/nw | sol↔sağ |  |
| item.hearth_stone | 1x1 | ? | 40,24 | se | sol↔sağ |  |
| item.stool_wood | 1x1 | ? | 40,24 | se | sol↔sağ |  |
| item.table_wood | 2x1 | ? | 56,32 | se/nw | sol↔sağ |  |
| item.shelf_wood | 2x1 | 112x96 | 56,32 | se/nw | sol↔sağ |  |
| item.chair_wood | 1x1 | ? | 40,24 | sw | sol↔sağ |  |
| item.cooking_pot | 1x1 | 80x68 | 40,24 | se | sol↔sağ |  |
| item.candle | 1x1 | ? | 16,7 | se | sol↔sağ |  |
| item.flower_pot | 1x1 | ? | 16,7 | se | sol↔sağ |  |
| item.chair_carved | 1x1 | 80x88 | 40,24 | se | sol↔sağ |  |
| item.chest_wood | 1x1 | 80x72 | 40,24 | se | sol↔sağ |  |
| item.chopping_block | 1x1 | 80x64 | 40,24 | se | sol↔sağ |  |
| item.market_stall | 2x1 | 112x136 | 56,32 | se/nw | sol↔sağ |  |
| item.clay_cup | 1x1 | 32x32 | 16,7 | se | sol↔sağ |  |
| item.bench_long | 2x1 | 112x80 | 56,32 | se/nw | sol↔sağ |  |
| item.mattress_straw | 1x1 | 80x56 | 40,24 | se | sol↔sağ |  |
| item.child_bed | 1x1 | 80x72 | 40,24 | se | sol↔sağ |  |
| item.stump_table | 1x1 | 80x64 | 40,24 | se | sol↔sağ |  |
| item.prep_counter | 2x1 | 112x88 | 56,32 | se/nw | sol↔sağ |  |
| item.armchair_cushion | 1x1 | 80x88 | 40,24 | se | sol↔sağ |  |
| item.brazier | 1x1 | 80x64 | 40,24 | se | sol↔sağ |  |
| item.stone_oven | 2x1 | 112x104 | 56,32 | se/nw | sol↔sağ |  |
| item.barrel | 1x1 | 80x72 | 40,24 | se | sol↔sağ |  |
| item.woodshed | 2x1 | 112x104 | 56,32 | se/nw | sol↔sağ |  |
| item.seed_crate | 1x1 | 80x64 | 40,24 | se | sol↔sağ |  |
| item.whetstone | 1x1 | 80x72 | 40,24 | se | sol↔sağ |  |
| item.basin_ewer | 1x1 | 80x72 | 40,24 | se | sol↔sağ |  |
| item.water_bucket | 1x1 | 80x56 | 40,24 | se | sol↔sağ |  |
| item.barrel_tub | 1x1 | 80x72 | 40,24 | se | sol↔sağ |  |
| item.outhouse | 1x1 | 80x120 | 40,24 | se | sol↔sağ |  |
| item.well | 1x1 | 80x112 | 40,24 | se | sol↔sağ |  |
| item.scarecrow | 1x1 | 80x112 | 40,24 | se | sol↔sağ |  |
| item.garden_bench | 2x1 | 112x88 | 56,32 | se/nw | sol↔sağ |  |
| item.child_chair | 1x1 | 80x64 | 40,24 | se | sol↔sağ |  |
| item.toy_chest | 1x1 | 80x72 | 40,24 | se | sol↔sağ |  |
| item.jug | 1x1 | 32x32 | 16,7 | se | sol↔sağ |  |
| item.salting_barrel | 1x1 | 80x72 | 40,24 | se | sol↔sağ |  |
| item.compost_heap | 1x1 | ? | 40,24 | se | sol↔sağ |  |
| item.seed_maker | 2x1 | ? | 56,32 | se | sol↔sağ |  |
| item.bucket_pulley | 1x1 | ? | 40,24 | se | sol↔sağ |  |
| item.smokehouse | 2x1 | 112x96 | 56,32 | se | sol↔sağ |  |
| item.cheese_press | 1x1 | 72x64 | 40,24 | se | sol↔sağ |  |
| item.forage_pinecone | 1x1 | ? | 40,24 | se | sol↔sağ |  |
| item.sedir | 2x1 | 112x88 | 56,32 | se | sol↔sağ |  |
| item.sedir_long | 3x1 | 144x96 | 72,40 | se | sol↔sağ |  |
| item.sedir_bed | 2x1 | 112x88 | 56,32 | se | sol↔sağ |  |
| item.floor_bed | 2x1 | 112x80 | 56,32 | se | sol↔sağ |  |
| item.yukluk | 2x1 | 112x136 | 56,32 | se | sol↔sağ |  |
| item.floor_cushion | 1x1 | 80x56 | 40,24 | se | sol↔sağ |  |
| item.box_bed | 2x1 | 112x96 | 56,32 | se | sol↔sağ |  |
| item.kilim_small | 2x1 | 112x72 | 56,32 | se | sol↔sağ |  |
| item.kilim_large | 3x2 | 176x96 | 88,48 | se | sol↔sağ |  |
| item.cicim | 2x2 | 144x88 | 72,40 | se | sol↔sağ |  |
| item.slingshot | 1x1 | ? | 16,7 | se | sol↔sağ |  |
| item.jump_rope | 1x1 | ? | 16,7 | se | sol↔sağ |  |
| item.kite | 1x1 | ? | 16,10 | se | sol↔sağ |  |
| item.hoop | 1x1 | 32x32 | 16,10 | se | sol↔sağ |  |
| item.marbles | 1x1 | 32x32 | 16,4 | se | sol↔sağ |  |
| item.whittling_knife | 1x1 | 32x32 | 16,7 | se | sol↔sağ |  |
| item.juggling_balls | 1x1 | 32x32 | 16,7 | se | sol↔sağ |  |
| item.book_almanac | 1x1 | ? | 16,7 | se | sol↔sağ | dosya yok |
| item.book_herbal | 1x1 | 32x32 | 16,7 | se | sol↔sağ |  |
| item.book_husbandry | 1x1 | 32x32 | 16,7 | se | sol↔sağ |  |
| item.book_smithing | 1x1 | 32x32 | 16,7 | se | sol↔sağ |  |
| item.bread_basket | 1x1 | 32x32 | 16,2 | se | sol↔sağ |  |
| item.fruit_basket | 1x1 | 32x32 | 16,2 | se | sol↔sağ |  |
| item.cheese_board | 1x1 | 32x32 | 16,5 | se | sol↔sağ |  |
| item.glass_jar | 1x1 | 32x32 | 16,5 | se | sol↔sağ |  |
| item.hourglass | 1x1 | 32x32 | 16,3 | se | sol↔sağ |  |
| item.copper_hand_mirror | 1x1 | 32x40 | 16,4 | se | sol↔sağ |  |
| item.flower_wreath | 1x1 | 40x40 | 20,20 | sw/ne | sol↔sağ | duvar: ayna öbür duvar için |
| item.harvest_wreath | 1x1 | 40x40 | 20,20 | sw/ne | sol↔sağ | duvar: ayna öbür duvar için |
| item.kilim_hanging | 1x1 | 96x64 | 48,32 | sw/ne | sol↔sağ | duvar: ayna öbür duvar için |
| item.large_mirror | 1x1 | 96x64 | 48,32 | sw/ne | sol↔sağ | duvar: ayna öbür duvar için |


## main ile birleştirme (20.11.2026)

Aşamalar ayrı bir oturumda yapıldı; o arada main'e görsel parti 3, belge 45 (ev bakımı) ve eski kayıt göçü geldi.
İki birleştirme commit'i (f1c37f1, 6ecfcd9), sonra main'e itildi.

- IsoArt: eşya görselleri yine items.json'dan (ItemFacing). Main'in ağaç ve ekin evreleri (Trees/Crops), yol karoları,
  ikinci renk maskesi korundu; `ItemAccent` artık eşyanın kendi görsel adını ItemFacing'ten alır (`<ad>_accent`).
- items.json: main'in sürümü alındı, `tools/layout/item_meta.py` yeniden çalıştırıldı (253 → 261 görselli eşya).
  Main'deki eski tablo görseli henüz depoya gelmemiş eşyaları bilerek eşlemişti ("dosya yoksa küp"); bunlar
  `art.pending: true` ile korunur, dosya gelince kendiliğinden çizilir. Bitki evreleri items.json'a yazılmaz.
- Eski kayıt genişletme (96/208 → 428): main'deki el koordinatlı `PlaceRegionItems` yerine
  `StarterWorld.PlaceFrom`: yeni bölgelerin eşyaları yeni oyunun şablon ve kurallarıyla, aynı tohumla; eski alana ve
  oyuncunun eşyalarına dokunulmaz (test: `EskiKayitGocu_YeniBolgeler_YeniOyunlaAyni_EskiAlanaDokunmaz`).
- Kayıt yükleme: `Regions.Apply` + `Places(map)` (dükkân sahipleri üst katta) + şehir planı.

Sonuç: 1011 test geçti, GameCheck derleniyor; rapor 77/77 şablon kusursuz, tohum 1, 2, 77, 12345'te ihlal 0.
