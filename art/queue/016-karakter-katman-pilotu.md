---
id: "016"
title: Karakter katman pilotu (kel taban + saç, sakal, kıyafet katmanı; 8 yön denemesi)
status: running
claimed_at: 2026-11-18T07:23Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: karakter
style_gate: true
cost_est: 45
cost_max: 55
depends_on: ["012"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/layers/iso_char_base_m_56_bald_se.png
  - Assets/Art/Iso/_aday/layers/iso_char_layer_hair_short_brown_se.png
  - Assets/Art/Iso/_aday/layers/iso_char_layer_hair_long_braid_se.png
  - Assets/Art/Iso/_aday/layers/iso_char_layer_beard_short_se.png
  - Assets/Art/Iso/_aday/layers/iso_char_layer_outfit_apron_se.png
  - Assets/Art/Iso/_aday/layers/iso_char_base_m_56_bald_<yön>.png
  - Assets/Art/Iso/_aday/layers/iso_char_layer_hair_short_brown_<yön>.png
created: 2026-11-17
---
# 016 Karakter katman pilotu

## Amaç
Detaylı karakter yaratma (proje dokümanı 14, bölüm 3 ve 5) dünya sprite'ında katman ister: taban gövde + saç + kâkül + sakal + kıyafet parçaları. 009 fizibilite raporu "kısmen mümkün" dedi ve katman pilotunun, boy seçilip (012: 56 px) taban karakter belli olduktan sonra başlamasını önerdi.

Bu pilot şunları ölçer:
- katmanlar maske dışını bozmadan çıkıyor mu;
- 8 yöne nasıl yayılıyor;
- kimlik başına gerçek maliyet ne.

017 (plan işi) setin kalanını bu sonuca göre yazar.

## Tarif
- **Kaynak:** `Assets/Art/Iso/_aday/iso_char_farmer_a2_56_cand_<yön>.png` (012, 56 px). Yoksa 012 raporundaki yetişkin erkek adayı.
- **Ortak açıklama eki:** `, pixel art, same character, same proportions, same outline and palette, earthy muted medieval palette, light from top-left, no dithering`.

1. **Kel taban (SE).** `inpaint_image_pro_flash`; normal çıktı, değişikliklerle birlikte tam görsel.
   - Maske: saç bölgesi. Kafanın üstü ve yanları, kulak hizasına kadar. 009'daki HLS sınıflandırmasıyla bul, 1 px genişlet.
   - Açıklama: `completely bald head, same skin tone, plain undyed linen shirt instead of the tunic`. Tunik bölgesi de maskeye eklenir.
   - Çıktı: `iso_char_base_m_56_bald_se.png`. 5 gen.
2. **Katmanlar (SE).** `inpaint_image_pro_flash`, taban üzerinde, `output_method="Modify current layer, only changes"`. Her biri 5 gen, toplam 20.

| Çıktı | Maske | Açıklama |
|---|---|---|
| hair_short_brown | kafa üstü + yanlar | `short messy brown hair` |
| hair_long_braid | kafa üstü + sırt (ense → bel) | `long brown hair in a single braid down the back` |
| beard_short | çene ve yanak altı | `short brown beard` |
| outfit_apron | gövde önü | `leather work apron over the shirt` |

3. **8 yön.** İki yol denenir, raporda karşılaştırılır:
   - (a) **Taban:** `create_character_state` ile kel taban durumu 8 yöne yayılır. Gerekirse önce kaynak karakter `create_character` v3 ile tüm yönleri verilerek içe aktarılır (009: 0,25 gen). Durum başına 10 gen.
   - (b) **Kısa saç:** `edit_image` ile 8 yönlü tabanın tümüne aynı düzenleme (`add short messy brown hair`), sonra tabana göre fark alınıp katman çıkarılır. 10 gen.
   - Çıktılar `_<yön>` ekiyle. Toplam yaklaşık 20 gen.
4. **Birleşim önizlemesi:** `art/results/016-birlesim-x4.png`.
   - SE'de taban + her katman tek tek ve hepsi birlikte.
   - 8 yönde taban + kısa saç.
   - Dikiş, kayma, çift dış hat var mı?

## Doğrulama
- Boyut kaynakla aynı. Alfa: taban kaynağın dış hattını korumuş mu?
- "Only changes" çıktısında maske dışı piksel 0 olmalı.
- 8 yönde katman ile taban arasında 1 px'ten fazla kayma var mı?
- `style.py check --family karakter`. Yan yana sayfası.
- **Raporda öneri (017 bunu kullanır):**
  - en iyi yöntem;
  - kimlik başına gen (SE ve 8 yön);
  - elle düzeltme ihtiyacı;
  - 4 yön ile 8 yön maliyet farkı.

## Notlar
- Yön adları: s, se, e, ne, n, nw, w, sw (012 ile aynı).
- Telif yok; açıklamada oyun adı geçmez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
