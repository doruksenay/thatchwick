unity-import: pending

# 490 Tatlı su balıkları ve efsaneleri ikonları (11)

- Araç: `create_image_pixflux` 32x32, `no_background=true`, tarifteki açıklamalar; 11 çağrı x 1 gen = 11 gen. İlk turda 7 çağrı hız sınırı (10/10 iş, paylaşılan havuz) yüzünden reddedildi (ücret yok); işler bitince her biri bir kez yeniden gönderildi.
- `get_balance` sonrası: 2762,9 (paylaşılan havuz, diğer şeritler de harcıyor).
- Yan yana: `art/results/490-yanyana.png` (esya referansları). Masaüstü kopyası: yerel.
- Doygunluk düzeltmesi (`style.py fit --family hayvan`): altın sazan x0.48, kızılkanat x0.70 (ikisi de hayvan ailesinden çok doygundu). Ham hâller `art/results/490-red/`.

## Teslim (Assets/Art/Iso/_aday/), 32x32, pivot x ortada (16), pivot y alttan = alt boşluk
| Dosya | Pivot y (alttan) | validate | Stil denetimi (hayvan; esya ailesi renkli içerikte KALDI) |
|---|---|---|---|
| icon_fish_tench.png | 5 | geçti | UYARI (palet dışı, doygun x1.47) |
| icon_fish_bream.png | 7 | geçti | UYARI (palet dışı, doygun x1.43) |
| icon_fish_rudd.png | 8 | geçti | UYARI (fit sonrası: palet dışı) |
| icon_fish_catfish.png | 4 | geçti | UYARI (palete uzak) |
| icon_fish_perch.png | 8 | geçti | UYARI (palet dışı, doygun x1.48) |
| icon_fish_salmon.png | 8 | geçti | UYARI (palet dışı) |
| icon_fish_bullhead.png | 6 | geçti | UYARI (palete uzak) |
| icon_fish_carp_gold.png | 7 | geçti | GEÇTİ (fit sonrası) |
| icon_fish_catfish_giant.png | 1 | geçti | GEÇTİ |
| icon_fish_sturgeon_old.png | 5 | geçti | UYARI (palet dışı) |
| icon_fish_pike_old.png | 6 | geçti | UYARI (palet dışı, doygun x1.31) |

## Sorunlar
- Hayvan kilidi kurulmadı: palet uyumu yalnızca uyarı. Görsel kalite (tür tanınırlığı, efsanelerin iri/yaşlı görünümü) elle incelenmedi; kullanıcı yan yana sayfasına baksın.
- Efsanevi balıklar aynı 32x32 boyutta; "iri" görünümü yalnızca çizimden gelir.
- `validate_iso.py` tek dosya alıyor; her dosya ayrı çalıştırıldı (`--kind object`).
