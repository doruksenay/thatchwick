---
id: "008b"
title: Duvar pencere (iki eksen) ve alçak duvar (iki eksen), inpaint ile
status: pending
approved_by: pending     # kullanıcı kararı 2026-11-08: yarın çalışacak; kullanıcı "user" yapınca işçi alır
family: yapi
style_gate: false
cost_est: 20
cost_max: 25
depends_on: []           # 008a (kit parçaları) sohbette kabul edildi, iş dosyası yok
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_window_sw_ne.png
  - Assets/Art/Iso/iso_wall_window_se_nw.png
  - Assets/Art/Iso/iso_wall_wall_low_sw_ne.png
  - Assets/Art/Iso/iso_wall_wall_low_se_nw.png
created: 2026-11-08
---
# 008b Duvar pencere ve alçak duvar

## Amaç
Yapı modunun duvar seti, 008a ile kabul edilen 007 kit parçalarından (`iso_wall_wall_*`, `iso_wall_door_*`, `iso_wall_post`) türetilerek tamamlanıyor. Eksik olanlar:
- iki eksen için **pencere** (oyunda `EdgeKind.Window`);
- iki eksen için **alçak duvar** (kesit modu, 10 px).

Alçak modda kapı ve pencere de düz alçak duvar olarak çizilir (`WallView`), bu yüzden alçak kapı/pencere yok.

Kaynak parçalar değişmez. Yeni dosyalar aynı 102x115 tuvalde, aynı pivotta.

## Tarif

### Araç
- `inpaint_image_pro_flash`, `output_method="Modify current layer"` (maske dışı baytlar aynen korunur), `no_background=true`, `seed=0`.
- Maliyet (araç açıklaması): kaynak tuvalin uzun kenarı ≤96 px ise 5 generation. Dört çağrı, toplam 20. Bu yüzden kaynak 102x115'ten **96x96 pencereye kırpılır**, çıktı aynı ofsetle 102x115 tuvale geri konur. Bu kayıpsız bir yerleştirmedir, boyama değildir.
- Kırpma pencereleri (sol-üst x, y; 96x96):

| Parça | Kaynak | Kırpma |
|---|---|---|
| window sw_ne | `iso_wall_wall_sw_ne.png` | (0, 4) |
| window se_nw | `iso_wall_wall_se_nw.png` | (6, 4) |
| wall_low sw_ne | alçak ön hazırlık (aşağıda) | (0, 19) |
| wall_low se_nw | alçak ön hazırlık | (6, 19) |

- Maske: kaynakla aynı boyutta PNG. Her pikselin RGB'si saf beyaz (üret) ya da saf siyah (koru), anti-alias yok.

### Geometri (102x115 tuval, piksel sütun x, satır y, sol-üstten)
- `sw_ne` ("/") ön yüz taban satırı: `b(x) = 95 - ceil((x-32)/2)`, x = 32..63. Kalınlık arkaya, **sol-yukarı** 12 px yatay / 6 px dikey.
- `se_nw` ("\") ön yüz taban satırı: `b(x) = 80 + floor((x-38)/2)`, x = 38..69. Kalınlık arkaya, **sağ-yukarı** 12 px yatay / 6 px dikey.
- Tam duvar yüksekliği 64 px. Pivot (ALTTAN): sw_ne (32, 19), se_nw (70, 19); kaynaklarla aynı.

### 1) Pencere (iki eksen)
- **Maske:** ön yüzde pencere açıklığı.
  - Sütunlar: sw_ne x = 41..54, se_nw x = 47..60 (14 px, yüzün ortası).
  - Her sütunda satırlar `b(x)-44` … `b(x)-24` (taban üstünde 24–44 px).
  - Bu alan duvar kalınlığı boyunca arkaya süpürülür (sw_ne: sol-yukarı 12/6, se_nw: sağ-yukarı 12/6), içerideki pervaz da çizilebilsin diye. Sonra 1 px genişletilir.
- **Açıklama:** `an open window hole through the timber-frame plaster wall with a simple dark wooden frame around it, no glass, no shutters, the hole itself is empty and fully transparent so you can see through the wall, same colours, outline and pixel style as the surrounding wall, light from top-left, no shadow`

### 2) Alçak duvar (iki eksen)
- **Ön hazırlık (betik, yalnızca silme):** Tam duvardan şunlar kalır:
  - ön yüz sütunlarında `y >= b(x) - 9` olan pikseller (10 px yükseklik);
  - kesim yüzü paralelkenarının altında kalan arka döşeme pikselleri.

  Geri kalan her şey (üst kısım, kapak çıkıntısı) silinir. Hiçbir piksel boyanmaz.
- **Maske:** kesim yüzü, yani alçak duvarın üst yüzü. Bu, ön üst kenar çizgisi (`b(x) - 10`) ile bu çizginin kalınlık yönünde 12 px yatay / 6 px dikey kaydırılmışı arasındaki paralelkenar (sw_ne sol-yukarı, se_nw sağ-yukarı). 1 px genişletilir.
- **Açıklama:** `the flat top cut surface of a low cut-away timber-frame wall seen from above: cream plaster section with dark wooden beam ends, same colours and pixel style as the wall, clean dark outline, light from top-left, no shadow`

### Teslim
- Çıktıyı 102x115 tuvale kırpma ofsetiyle geri koy. `Assets/Art/Iso/` altına `cp -n` ile teslim et; `.meta` yazma, var olan dosyanın üzerine yazma.
- Her parça tek deneme. Bir parça tutmazsa tekrar deneme; o parçayı teslim etme, nedenini yaz, diğer parçalara devam et.

### Masaüstü kopyası (kullanıcının açık izni, yalnızca bu iş için)
Hiçbir şeyi silme, üzerine yazma. Klasör tarihi üretim günüdür.
- `C:\Users\User\OneDrive\Desktop\art\duvar\<yyyy-mm-dd>_008b\`: dört PNG (aynı adlar) ve önizlemeler.
- **Önizlemeler:** 008a ile aynı 3x3 oda, 2 kat, `#333333` zemin. Oda oyunun kenar modeliyle kurulur:
  - Köşe noktası `(gx, gy)` → `X = 32*(gx-gy) + OX`, `Y = OY - 16*(gx+gy)`.
  - "/" = `Horizontal` kenar, pivot (32, 96 sol-üstten). "\" = `Vertical` kenar, pivot (70, 96). Direk (51, 96).
  - Sıralama kenar orta noktasının dünya y'sine göre, büyük olan önce çizilir.
  - Zemin `iso_ground_dirt_set_a/b` (içeride) ve `iso_ground_grass_set_a/b/c` (dışarıda), ortada `iso_char_villager_se.png`.
  - `008b_oda_tam_x2.png`: arka iki duvarda birer pencere, ön duvarda bir kapı.
  - `008b_oda_kesit_x2.png`: ön duvarlar yeni alçak parçalarla. Ön köşedeki direk alçak duvar yüksekliğine kırpılır (yalnızca önizleme).
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna her parça için bir satır: `tarih | duvar | duvar/<yyyy-mm-dd>_008b | inpaint_image_pro_flash | <gen> | aday | <renk sayısı, not>`.
- Python'da Pillow yok: `uv run --with pillow ...`. Betikleri geçici klasörde tut, repoya ekleme.

## Doğrulama
- `uv run --with pillow python tools/art/validate_iso.py <dosya> --kind object --max-colors 48` (her dosya). 102x115, yarı saydam 0, renk ≤ 48.
- **Maske dışı değişmezlik:** çıktıyı kaynakla (alçakta ön hazırlıkla) karşılaştır; maske dışında değişen piksel **0** olmalı.
- **Pencere:** açıklığın içinde saydam piksel sayısı. Delik saydam değil de koyuya boyanmışsa ⚠ ile işaretle ve yaz, düzeltme yok. Çerçeve ahşap mı, cam ya da kepenk var mı?
- **Alçak:** en üst opak satır, tabanın en çok 10 + 6 px üstünde mi? Kesim yüzü okunuyor mu?
- Işık (sol yüz orta, sağ yüz en koyu), pişmiş gölge yok. Önizlemede parçaların komşularla dikişsiz birleşmesi.
- Rapor: her dosya için piksel boyutu, renk sayısı, yarı saydam sayısı, pivot (ALTTAN) ve maske dışı değişen piksel sayısı.

## Notlar
- Ad kuralı (kullanıcı onaylı): `iso_wall_<tür>_<eksen>.png`. "/" = `sw_ne`, "\" = `se_nw` (kod eksenine göre; `Iso.GridToWorld`, +x ekranda sağ-yukarı).
- Pivotları kullanıcı `IsoPivots.txt`'e ekler; işçi `.meta` yazmaz.
- Alçak direk (`iso_wall_post_low`) bu işte yok; gerekirse ayrı iş.
- 008 (eski plan, `depends_on: ["007"]`, 007 reddedildi) bu iş ve 008a ile karşılandı; 008'in durumunu kullanıcı belirler.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
