---
id: "008c"
title: Pencere kenarındaki boşluğun onarımı (008b)
status: running
claimed_at: 2026-11-18T12:16Z
approved_by: user        # kullanıcı kararı 2026-11-18 (sohbet): 008b kabul, boşluk onarılsın
family: yapi
style_gate: false
cost_est: 10
cost_max: 15
depends_on: ["008b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_wall_window_sw_ne_v2.png
  - Assets/Art/Iso/iso_wall_window_se_nw_v2.png
created: 2026-11-18
---
# 008c Pencere kenar onarımı

## Amaç
008b kabul edildi. Kullanıcı (2026-11-18): pencere "fena değil ama kenarda boşluk kalmış". Çerçeve ile duvar dolgusu arasında, özellikle sağ ve alt kenarda, koyu ya da saydam 1-2 px'lik bir şerit kalıyor. Yalnızca bu şerit kapatılır; duvar ve pencerenin geri kalanı aynı kalır.

## Tarif
- Kaynak: `Assets/Art/Iso/iso_wall_window_sw_ne.png` ve `iso_wall_window_se_nw.png` (008b, 102x115).
- Önce betikle şeridi bul: pencere açıklığının çevresinde, çerçeve (koyu kahve kiriş) ile duvar sıvası arasında kalan saydam ya da çok koyu pikseller. Pencere açıklığının kendisi (camsız delik) şerit değildir, açık kalır. Bulduğun şeridi raporda piksel sayısıyla yaz.
- Maske: yalnızca şerit + 1 px genişleme. Araç: `inpaint_image_pro_flash`, `output_method="Modify current layer"`, `no_background=true`, `seed=0`, 008b'deki gibi 96x96 kırpma ve aynı ofsetle geri koyma. Açıklama: "fill the gap between the wooden window frame and the plaster wall, continue the plaster and timber texture seamlessly, no gap, no outline change".
- Şerit betikle bulunamazsa ya da 3 px'ten genişse PixelLab çağırma: `## Sorular` ekle, `blocked` yap.
- Çıktılar `_v2` adıyla yazılır (var olan PNG'nin üzerine yazılmaz). Kullanıcı kabul edince Unity tarafında `_v2` kullanılır.

## Doğrulama
- `validate_iso.py --kind object --max-colors 48`, `style.py check --family yapi`.
- Maske dışında değişen piksel 0.
- Şerit bölgesinde kalan saydam/koyu piksel sayısı (önce/sonra).
- Yan yana sayfası: 008b orijinali ve `_v2`, 4 kat büyütülmüş, pencere bölgesi yakın plan.

## Notlar
Pivotlar 008b ile aynı (sw_ne 32,19; se_nw 70,19).


## Sorular
- 2026-11-18 (bulut şerit 1): `get_balance` plan olarak Tier 2 (Pixel Artisan), 4999/5000 generation, yenilenme 2026-12-18 gösteriyor. `art/budget.md` ise Tier 1, 2000 generation, yenilenme 2026-12-08 yazıyor. Kural "Plan Tier 1 değilse dur" olduğu için PixelLab çağrılmadı, harcama 0. Kullanıcı `art/budget.md` planını güncelleyip (Tier 2 kabulü) işi `pending` yapınca iş tekrar alınabilir.
- 2026-11-18 (tamir): `art/budget.md` planı Tier 2 olarak güncellendi, ajan kuralı "Tier 1 değilse dur" yerine "budget.md planıyla uyuşmuyorsa dur" oldu; iş yeniden `pending`.
