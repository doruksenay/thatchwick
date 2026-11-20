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

## Kalanlar (sıra)
1. Köy meydanı taslağa: eğri yollar, bina biçimleri (L, sundurma), `TownBuildings` → `markers.json`.
2. Yaban haritaları ve çiftlik çevresi taslağa (`draw.py` bölge fonksiyonları), kum zemini, simge yapı eşyaları (619 sonrası).
3. Geçiş setlerinin (613-617) çizimi: `WorldView` dual-grid katmanı yol setindeki gibi.
4. Zemin yamaları ve aşınma haritası (görünüm; Core'da saf fonksiyon).
5. Öbek (vignette) sistemi `layouts.json`.
6. Unity: editör "Harita önizlemesi al" komutu; ekran görüntüsüyle kontrol.
7. Kaya varyantları (620) ve hazır mekân eşyaları (621) bağlama (maden M1/M4 ile).
