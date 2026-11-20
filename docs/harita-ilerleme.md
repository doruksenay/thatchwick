# Harita üretimi: ilerleme ve devam notu (belge 64, 64a)

Bağlam sıkışırsa buradan devam et. Özet ve karar bekleyenler: Project belgesi `64b-harita-uygulama-ozeti.md`.

## Yapılanlar (20.11.2026, parti 1)
- **A Önizleme:** `tools/map-preview` (C# döküm + `render.py`); `tools/map-preview/run.sh [bölge]` → `art/preview/<bölge>/`
  (`harita.png` kesit, `harita-dis.png` dış görünüş, `bina-<id>-kat<z>.png`). Görseli olmayan eşya mor kutu + adı. Çıktı git'e girmez.
- **B Taslak:** `Core/World/MapFiles.cs` (palet, `RegionMap`, `PngReader`, `MapFiles.Provider`). Oyun: `ContentLoader.InstallMaps`
  (Resources, okunabilir doku: `IsoImportPostprocessor`). Testler: `MapFilesSetup` (ModuleInitializer). Biçim: `Content/maps/README.md`.
  Yalnızca **köy yolu** taslakta; diğer bölgeler hâlâ `Regions.cs`/`RegionsWild.cs` kod çizimi.
- **C Araçlar:** `tools/maps/draw.py` (eğri yol, gürültülü dere, kenar bandı, kaya sırtı, 4 yönlü bağlantı düzeltmesi).
  `OutdoorBuilder.EdgeBand` bandı öbek/ağaçla, kaya sırtını iri kaya/kütükle doldurur; `Vegetation` banda girmez.
- **D Köy yolu:** 40x32 (kuzey ve güney 4'er karo büyüdü; Güney Yamaçları geçidi (107,16)); kıvrılan yol, dere ve köprü,
  sınır taşı (görsel 619 gelince konar), ulu ceviz yeri açık. Toplama düğümleri dereye uzaklıkla.
- **E Kitler (kısmen):** `layouts.json` `kits` + bina `kits`; `Core/Layout/BuildingKits.cs` cephe (duvar, kapı, pencere), zemin,
  çatı, iç kaplama ve eşya kaplaması (tek ahşap tonu, tek kumaş). Köylü evleri 4 kitten tohum + komşu kuralıyla.
- **G Maden:** `MineGen` v2 (bantlı kaya, yönlü damar, nem alanı, doğal kovuk, 6 hazır mekân). `MadenUretimiV2Tests`.
- **Görsel işleri:** 612-621.

## Yapılanlar (20.11.2026, parti 2)
- **Köy meydanı taslakta** (`maps/town`): eğri sokaklar, kenarı düzensiz taş meydan, kumlu kıyılı ırmak, kapıdan sokağa
  patikalar (kapı önünde iki karo düz çıkış), atölye kapılarında ve sokak kenarında aşınmış toprak, ince kenar bandı.
  Bina planı C#'ta kalır (`Regions.TownBuildings`); `draw.py town()` onu okur.
- **Dikdörtgen olmayan binalar:** `TownBuilding.WithAnnex` (ek yapı, ortak duvarda iç kapı). Meyhane mutfak kanadı, pazar
  ambarı, demirci kömürlüğü, marangoz kereste sundurması, değirmen un ambarı. Kit, döşeme (`annex_*` oda şablonları),
  izinsiz girme, bitki örtüsü ve avlu bölgeleri eki tanır.
- **Yaban bölgeleri ve çiftlik taslakta:** taban kod çiziminden (`dotnet run --project tools/map-preview -- out=art/preview/_base export`),
  `draw.py` patikaları yuvarlar ve kıvırır, kıyıyı girintiler ve kumlar, kenar bandı ve tepede kaya sırtı ekler; kıyıda kumsal
  kum zemin. `RegionsWild.Draft`: taslak varsa arazi oradan, eski koordinatlı eşya patikaya ya da suya düşerse konmaz.
  Çiftlikte yalnızca yol ve bant (ev, gölet, tarla kodda).
- **Maden:** kaya karoları 360/361 görsellerinin üst yüzüyle (`IsoArt.RockTop`, 620 çeşitleri b/c otomatik); hazır mekân ve
  kovuk süsleri (`MineMap.Props`, 7 eşya, 621 görselleri pending).
- **Zemin:** çayır, koyu çim ve çakıl öbekleri (612); dual-grid geçiş katmanı (613-617 gelince çizilir).
- **Eksik görsel işleri:** 730-734 (34 eşya + çalı + ahşap bodrum merdiveni); eşlemeleri `items.json`'da pending.

## Kalanlar (sıra)
1. Öbek (vignette) sistemi `layouts.json`; dükkânda başrol eşyanın kapıdan görünmesi.
2. Unity: editör "Harita önizlemesi al" komutu.
3. Simge yapı eşyaları (619 görselleri gelince markers.json items).
4. Kayıt: harita düzeni kimliği (eski bölgeli kayıt engeli).
5. Ulu ağaç görselleri (492a-c): tuval kararı kullanıcıda (492a Sorular); o zamana kadar büyütülmüş tür ağacı.
