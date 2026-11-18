---
id: "020"
title: Portre pilotu (64x64 göğüs portresi + katman denemesi)
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: karakter
style_gate: true
cost_est: 22
cost_max: 30
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/portrait_aldric_64.png
  - Assets/Art/Iso/_aday/portrait_maud_64.png
  - Assets/Art/Iso/_aday/portrait_agnes_64.png
  - Assets/Art/Iso/_aday/portrait_elder_m_64.png
  - Assets/Art/Iso/_aday/portrait_base_bald_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_short_brown_64.png
  - Assets/Art/Iso/_aday/portrait_layer_hair_long_red_64.png
  - Assets/Art/Iso/_aday/portrait_layer_beard_short_64.png
created: 2026-11-17
---
# 020 Portre pilotu

## Amaç
Karakter portreleri 1.0'a girdi (kullanıcı kararı 17.11.2026; tasarım: proje dokümanı `14-karakter-yaratma-arastirma-ve-tasarim.md` bölüm 2).

Portre 64x64, önden, baş ve omuzlar. Görüneceği yerler: HUD hane şeridi, karakter paneli, aile ağacı, bildirimler, yaratma ekranı.

Bu pilot iki soruyu cevaplar:
1. **Stil:** PixelLab, köylü sprite'larıyla aynı dünyada duran, okunaklı ve "profesyonel" görünen bir portre çizebiliyor mu?
2. **Katman:** Portre, yaratma ekranı için katmanlara bölünebilir mi? Kel taban + saç + sakal katmanı, maske dışını bozmadan yapılabiliyor mu?

## Tarif

### A. Dört tam portre (stil)
**Araç:** `create_image_pixflux`. Maliyet 1 gen/çağrı (100 raporunda ölçüldü). Çağrıdan önce araç açıklamasını oku.

**Ortak ayarlar**
- `width=64`, `height=64`, `no_background=true`, `direction="south"`.
- `detail="medium detail"`, `shading="basic shading"`.
- `color_image_base64` = `Assets/Art/Iso/_aday/iso_char_farmer_a2_56_cand_se.png`. Palet karakter ailesine bağlansın diye. Dosya yoksa `art/style/references.json` karakter ailesinin ilk referansı.
- Her açıklamanın sonuna şu ek:
  `, pixel art character portrait, head and shoulders bust, facing the viewer straight from the front, centered, face fills the upper two thirds, earthy muted medieval palette, flat simple shading, clean dark outline, light from top-left, no dithering, no noise, no background, no frame, no text`

**Portreler**
| Dosya | Açıklama (+ ortak ek) |
|---|---|
| portrait_aldric_64 | `adult medieval peasant man in his thirties, short messy brown hair, short brown beard, blue eyes, a small pale vertical scar through his right eyebrow, plain brown wool tunic` |
| portrait_maud_64 | `adult medieval peasant woman in her thirties, long wavy copper-red hair over one shoulder, green eyes, light freckles, olive green wool dress with a cream collar` |
| portrait_agnes_64 | `medieval village girl about eight years old, brown hair in two short braids, green eyes, round rosy cheeks, cream linen smock` |
| portrait_elder_m_64 | `elderly medieval villager man, thin grey hair, grey beard, wrinkles around the eyes, kind tired expression, dark brown hooded cloak` |

Portre başına en çok 2 aday (ilki doğrulamayı geçmezse ikincisi). Başarısız aday masaüstüne gider, Assets'e gitmez.

**Bütçe:** en çok 8 gen.

### B. Katman denemesi
1. **Taban.** `create_image_pixflux`, ayarlar A ile aynı. Açıklama: `adult medieval peasant, completely bald head, no hair, no beard, no eyebrows missing, neutral expression, plain undyed linen tunic` + ortak ek. Çıktı: `portrait_base_bald_64.png`. 1 gen.
2. **Katmanlar.** `inpaint_image_pro_flash`, taban üzerinde, `output_method="Modify current layer, only changes"` (009 raporu: yalnızca değişen pikselleri döndürür, yani katmanın kendisi). Görsel ≤96 px olduğu için 5 gen/çağrı. Çağrıdan önce araç açıklamasını oku.
   - **Maskeler:** betikle, geçici klasörde.
     - Saç maskesi: başın üst %45'i + kulak hizasına kadar yanlar. Uzun saç için omuzlara kadar yanlar.
     - Sakal maskesi: burnun altından çene ucuna.

| Çıktı | Maske | Açıklama |
|---|---|---|
| portrait_layer_hair_short_brown_64 | saç (kısa) | `short messy brown hair` |
| portrait_layer_hair_long_red_64 | saç (uzun) | `long wavy copper-red hair falling over the shoulders` |
| portrait_layer_beard_short_64 | sakal | `short brown beard and moustache` |

3. **Önizleme.** Üç birleşim yap (taban + kısa saç, taban + uzun saç, taban + kısa saç + sakal) ve 4 kat büyüt: `art/results/020-katman-birlesim-x4.png`. Birleşimde boşluk, kayma ya da çift dış hat var mı?

**Bütçe:** 1 + 15 = 16 gen. **Toplam (A + B):** 24 gen.

### Teslim
- Varlıklar: `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma.
- Rapor: `art/results/020-portre-pilotu.md`.
- Yan yana sayfası: `python tools/art/style.py sheet --family karakter --out art/results/020-yanyana.png <A dosyaları>`.
- Arayüz denemesi (yalnız bilgi, karar değil): 210 sonra bu portreleri kendi önizlemesinde kullanır.

### Masaüstü kopyası (yalnızca yerel işçi; kullanıcının genel izni)
Hiçbir şeyi silme, üzerine yazma.
- Klasör: `C:\Users\User\OneDrive\Desktop\art\portre\<yyyy-mm-dd>\`.
- İçerik: tüm adaylar (başarısızlar dahil, adına `_red` eki), `_onizleme_x4.png` (4 portre yan yana, `#333333` zemin), `020-katman-birlesim-x4.png`.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna aday başına bir satır.

## Doğrulama
- Her dosya: 64x64, saydam arka plan, yarı saydam piksel sayısı (0 beklenir), renk sayısı (≤ 32 beklenir; değilse `reduce_colors` 32, raporda yaz).
- `python tools/art/style.py check --family karakter <dosyalar>`: KALDI teslim edilmez.
- Görsel kontrol (4 kat):
  - yüz önden mi (yan profil değil);
  - gözler ve ağız okunuyor mu;
  - Aldric'in kaş yarası görünüyor mu;
  - yazı ya da çerçeve var mı (olmamalı);
  - dört portre aynı ölçekte mi (göz hizası ±3 px);
  - köylü sprite'ıyla aynı dünyadan mı.
- Katman: "only changes" çıktısında maske dışı piksel sayısı (0 beklenir). Birleşimde taban ile katman sınırında dikiş var mı?

## Notlar
- 012 karakter seti `review`'da. Bu pilot ona bağımlı değil; palet ve stil ondan referans alınır.
- Kabul edilirse sıradaki iş: portre parça seti (yüz şekli, göz, kaş, burun, ağız, saç, sakal). Doküman 14, bölüm 6: ~71 parça × 5 gen. Ayrıca 4 kat önizlemede neyin tuttuğuna göre tarif düzeltilir.
- Telif: bilinen oyun karakterlerine benzetme yok; açıklamalarda oyun adı geçmez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
