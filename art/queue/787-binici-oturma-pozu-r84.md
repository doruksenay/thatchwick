---
id: "787"
title: "Binici pozu: 84 px yetişkin erkek ve kadın at üstünde oturur (4 çapraz yön)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-21 sohbet: "At ... ekle" (binilmiş poz)
priority: 40
family: karakter
style_gate: false
cost_est: 8
cost_max: 16
depends_on: ["785"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_ride_{se,sw,nw,ne}_<NN>.png
  - art/results/787-*.md
created: 2026-11-21
---
# 787 Binici pozu (84 px yetişkin)

## Amaç
Ata binen köylü bugün ayakta duruş görseliyle atın üstünde çiziliyor (bacaklar atın içinden geçiyor). Kod artık binicide `ride` karesini arar (`GameRoot`: `IsoArt.CharacterAction(c, "ride", ...)`); kare varsa yürüme yerine o çizilir. At 785'in `ride` klibiyle (eyerli) altında yürür.

## Tarif
- Karakterler: 84 px gerçekçi taban `man_r84` (c63fe39d, 018a) ve `woman_r84` (kimlik 018 raporlarında ya da `list_characters`).
- `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true` (018a yöntemi), 4 çapraz yön; 2 gövde x 4 = 8 gen.
- Eylem metni: `sitting astride an invisible horse, legs apart and bent at the knees hanging down on both sides, both hands holding reins in front of the belly, upper body upright, gentle up and down bob of a walking horse, loop. the horse is not drawn`
- **Yerleştirme (ücretsiz, kodla aynı pivot):** kod binicinin pivotunu 84 tuvalin (42, 2) noktasında tutar ve atı binicinin ayak noktasına çizer. Bu yüzden kareler **84 en x 112 boy** tuvale konur (alta 28 px boş satır eklenir, tuvalin üstü kesilmez) ve binici, oturduğu yer (kalça alt satırı) 785'in eyerli at SE karesindeki eyer üst satırına gelecek kadar **yukarı kaydırılır**. Kaydırma miktarı her yönde aynı olur, rapora yazılır. Yeniden örnekleme yok.
- Dosya: `Assets/Art/Iso/_aday/anim/iso_char_<gövde>_r84_ride_<yön>_<NN>.png` (kütüphane anim/ altından yalnızca `_r84_` karelerini alır; bu ad uyar).
- Renk: karakter ailesi; saç ve kıyafet rengi oyunda `BaseSpriteTint` ile boyanır, taban tonları değiştirilmez.

## Doğrulama
- `validate_iso.py`, `style.py check --family karakter`.
- Bileşik önizleme (`art/results/787-binis-<yön>.png`, 4 kat): 785'in eyerli at karesi + binici karesi aynı pivotta üst üste; bacaklar atın iki yanında, kalça eyerde, at başı binicinin önünde (se ve sw) ya da arkasında (ne ve nw) okunuyor.
- Kimlik: yüz, saç, kıyafet durma karesiyle aynı.

## Notlar
- Katmanlı karakterlerde (belge 43) `ride` katman karesi yoksa taban r84 binici karesi çizilir.
- Çocuk ve yaşlı binmez (oyunda binicilik yetişkin işi); karesi yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
