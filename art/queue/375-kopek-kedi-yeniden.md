---
id: "375"
title: Köpek ve kedi yeniden (106, renk düzeltmeli)
status: running
claimed_at: 2026-11-18T13:16Z
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 ("yap bunu": yeniden deneme işleri)
family: hayvan
style_gate: false
cost_est: 17
cost_max: 24
depends_on: ["100"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_dog_{1,2,3,4}_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_cat_{1,2,3,4}_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-18
---
# 375 Köpek ve kedi yeniden

## Amaç
106 üretimi tamamladı, ama 32 dosyanın 29'u stil kapısında kaldı. Nedeni: K2 köylüsünün paletiyle zorlama ve v3 döndürmenin sonucu, hayvan ailesinin doygunluğunun 1,4-1,9 katı çıktı (`art/results/106-hayvan-kopek-kedi.md`). Biçimler kullanılabilirdi. Bu iş aynı adayları renk düzeltmesiyle yeniden üretir.

## Tarif
Tarif **106 ile birebir aynı** (aday tablosu, yol B, ortak kurallar, güney kontrolü, zemin kontrolü, masaüstü kopyası, doğrulama). Yalnızca şu farklar var:

1. **Palet zorlaması:** `color_image_base64` = `Assets/Art/Iso/_aday/iso_animal_sheep_1_cand_se.png`. Hayvan ailesinin referansı; K2 köylüsü değil.
2. **Açıklama eki**ne `earthy muted medieval palette`'ten sonra `, desaturated` eklenir.
3. **Tuvaller** (106 raporundaki doluluk ölçümüne göre):
   - köpek 1 ve 4: 42;
   - köpek 2: 38;
   - köpek 3: 34;
   - kedi: 32 (en küçük pixflux alanı).
4. **Renk düzeltmesi:** v3 döndürmesinden ve gerekirse `reduce_colors`'tan sonra her yönün dosyasına `python tools/art/style.py fit --family hayvan <girdi> <çıktı>` (ücretsiz). Ardından `check --family hayvan`. Hayvan ailesinin referansları artık koyun 1 ve domuz 2.
5. **Teslim kuralı:**
   - 8 yönün hepsi KALDI değilse aday teslim edilir.
   - Bir yön KALDI ise adayın tamamı teslim edilmez, raporda yazılır.

- Maliyet 106 ile aynı: 8 aday × 2 gen + renk azaltma ≈ 17. Güney kontrolünden dönen aday 1 gen. `cost_max` 24.

## Doğrulama
- 106 doğrulaması.
- Ek olarak her adayın SE yönü için `fit` öncesi ve sonrası doygunluk.
- Yan yana sayfası: koyun 1 ve domuz 2 ile aynı satırda.

## Notlar
- 106 `failed` durumunda kalır; bu iş onun yerine geçer. Aday dosya adları 106'daki gibi, 106 hiçbir dosya teslim etmedi.
- Dönem: çoban köpeği, bekçi köpeği, tazı ve çiftlik kedisi ortaçağ köyüne uygun.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
