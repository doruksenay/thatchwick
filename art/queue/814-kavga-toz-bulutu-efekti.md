---
id: "814"
title: "Kavga toz bulutu efekti ve parçaları (Sims tarzı)"
status: running
claimed_at: 2026-11-21T00:42Z
approved_by: user        # kullanıcı 20.11.2026 ("simsteki gibi bir karmaşa animasyonu girer. animasyonu da ayarla", "şimdi alınabilir"); Project belgeleri 67a §5.2, 67b
priority: 40
family: esya
style_gate: false
cost_est: 24
cost_max: 40
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/fx/iso_fx_brawl_cloud_<NN>.png
  - Assets/Art/Iso/fx/iso_fx_brawl_bit_fist.png
  - Assets/Art/Iso/fx/iso_fx_brawl_bit_boot.png
  - Assets/Art/Iso/fx/iso_fx_brawl_bit_star.png
  - Assets/Art/Iso/fx/iso_fx_brawl_bit_dust.png
  - Assets/Art/Iso/fx/iso_fx_brawl_bit_hat.png
  - Assets/Art/Iso/fx/iso_fx_anger_puff.png
  - Assets/Art/Iso/fx/iso_fx_sweat_drop.png
  - art/results/814-*.md
created: 2026-11-20
---
# 814 Kavga toz bulutu efekti

## Amaç
Kavga eden iki karakter Sims'teki gibi bir **toz bulutunun içinde kaybolur**; bulut iki karonun ortasında kaynar, içinden yumruk, çizme, yıldız, toz, uçan şapka fırlar. Bulut dağılınca 813'deki `fight_win` / `fight_lose` pozları oynar. Ergen itişmesinde aynı bulut %70 ölçekle çizilir (kod).

## Tarif
- **Bulut döngüsü** `iso_fx_brawl_cloud_00..07`: 8 kare, 128x96 tuval (iki karoyu kaplar), alt kenarı zemine oturan kabarık, yuvarlak bej-gri toz bulutu; kenarları kareden kareye kaynar ve şişip iner. İçinde belli belirsiz iki silüet çizilmez. Stil: eşya ailesi, izometrik ışık (sol üst).
  - Yöntem: önce kare 00 `create_map_object` (eşya ailesi stil eşlemesi). Sonraki 7 kare aynı nesnenin varyasyonu olarak üretilir (aracın init/referans görseli seçeneği varsa kare 00 referans; yoksa `animate_character` bir "nesne karakteri" olarak denenmez, 4 kare üretilip ara kareler kodla aynalama + ölçek oynatmayla türetilir ve rapora yazılır).
- **Parçalar** (her biri 24x24, saydam, `create_map_object`): yumruk (`fist`, ten rengi, çizgi film), çizme (`boot`, kahve deri), yıldız (`star`, açık sarı), toz zerresi (`dust`, bej top), uçan şapka (`hat`, kahve keçe külah).
- **Öfke ve ter** (16x16): `anger_puff` (başın üstünde küçük kızgınlık bulutu, koyu gri, kıvrık), `sweat_drop` (açık mavi damla).
- Ton: cozy, çizgi film. Kan, silah, yara yok.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family esya <dosya>`. Bulut kareleri arasında taban satırı ve renk aynı kalmalı; 8 kare GIF olarak `art/results/` altına.

## Notlar
- Klasör `Assets/Art/Iso/fx/` yoksa oluşturulur; varsa oradaki adlandırma kuralı kullanılır ve rapora yazılır.
- Ekran sarsıntısı ve parça fırlatma kodla yapılır (efekt yoğunluğu ayarı, 57b).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
