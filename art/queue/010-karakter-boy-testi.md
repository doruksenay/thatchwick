---
id: "010"
title: Karakter boy ve detay testi (K1)
status: running
approved_by: user        # K0 planı 2026-11-08 onaylandı
cost_est: 7
cost_max: 9
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_char_farmer_a_48_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_farmer_a_56_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_farmer_a_64_cand_se.png
created: 2026-11-08
---
# 010 Karakter boy ve detay testi (K1)

## Amaç
`art-brief-karakter.md` Faz K1. Mevcut köylü (`iso_char_villager_*`, 68x68, boy ~45) yeterince detaylı değil. Yeni bir karakter üç boyda üretilir. Kullanıcı Unity'de 64x32 elmasın üzerinde boyu seçer, K2 (8 karakterlik set) bu karara bağlıdır. Plan ve araç karşılaştırması: `art-brief-karakter-result.md` Faz K0.

## Tarif
- Araç: `create_character`, `mode=v3`, `body_type=humanoid`, `view=high top-down` (~35°; 2:1 için ~30° hedefine en yakın seçenek), `outline=single color black outline`, `detail=high detail`, `name=farmer_a_<boy>`.
- Üç çağrı: `size=72` (hedef gövde 48), `size=84` (hedef 56), `size=96` (hedef 64). Maliyet 2 + 2 + 3 = 7. Çağrılar sırayla yapılır. Eşzamanlı iş sınırı çıkarsa (çağrı reddedilir, ücret yazılmaz) 30 saniye bekleyip bir kez daha dene, ret sürerse dur.
- Açıklama (üçünde aynı): "adult male medieval peasant villager, standing idle, yellow-brown wool tunic with collar, folds and cuffs, leather belt with buckle and small pouch, wrapped cloth leg bindings, laced leather shoes, dark brown short hair with visible strands, light stubble beard, fair skin, readable face with brows nose and mouth line, earthy muted palette, clean pixel art, crisp black outline, no dithering, no noise, no cast shadow, transparent background".
- 8 yönün hepsini `Assets/Art/Iso/_aday/` altına `iso_char_farmer_a_<boy>_cand_<yön>.png` adıyla kaydet. Yönler: se, sw, nw, ne, s, w, n, e; PixelLab south = s, south-east = se ... Boy etiketi hedef boydur (48, 56, 64).
- Mevcut `iso_char_villager_*` dosyalarına dokunma.
- **Masaüstü kopyası (kullanıcının brief'indeki açık istisna, yalnızca bu iş için):** `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_farmer_a_<boy>\` altına 8 yön PNG'si (aynı adlarla) ve `_onizleme_x4.png`. Önizleme 8 yön yan yana, en yakın komşu ile 4 kat, koyu gri (#333333) zemin. Faz toplusu: `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_K1_toplu_x4.png` (üç boy, her biri bir satır). `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna her boy için bir satır ekle: `2026-11-08 | karakter | karakter/2026-11-08_farmer_a_<boy> | create_character v3 | <gen> | aday | <boy ölçümü>`. Masaüstünde hiçbir şeyi silme ve üzerine yazma.

## Doğrulama
- Her boy için SE yönünde ölç: gerçek gövde boyu (opak bbox yüksekliği), genişlik, renk sayısı (hedef ≤32), yarı saydam piksel sayısı (hedef 0), pivot (ayak izi merkezi: ayakların altındaki opak sütunların ortası x, en alt opak satır y ALTTAN).
- 8 yönde renk sayısı ve yarı saydam piksel tablosu.
- `python tools/art/validate_iso.py <dosya> --kind object` her SE dosyası için.
- Boylar arası detay farkını yaz: yüz (kaş, burun, ağız), eller, kemer/toka/kese, bacak sargısı, ayakkabı bağı seçiliyor mu.
- 8 yön tutarlılığı: yüz, kıyafet ve aksesuar her yönde aynı mı?
- Renk 32'yi aşarsa elle azaltma yapma, ölçümü yaz.

## Notlar
- v3 seed almadığından üç boyda üç ayrı çizim çıkar; tasarım birebir aynı olmayabilir. Raporda belirt.
- Rapor: `art/results/010-karakter-boy-testi.md` (başta `unity-import: pending`). Kullanıcının brief'i ayrıca `art-brief-karakter-result.md`'ye "Faz K1" bölümü eklenmesini istiyor; işçi yalnızca `art/` altına yazdığı için bu bölümü kod oturumu rapordan kopyalar.
- Onaylanan boy, K2 iş dosyasına (011) yazılacak.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
