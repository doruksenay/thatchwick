---
id: "018d"
title: Etkileşim animasyonları: pişirme, çekiçle zanaat, el işi, genel iş, yemek
status: accepted   # kullanıcı 18.11.2026: cook, craft_hammer, craft_hand, work onaylandı; eat 018d2 ile yeniden
claimed_at: 2026-11-18T18:54Z
approved_by: user        # otomatik akış (art/budget.md); plan işi 018
priority: 50
family: karakter
style_gate: false
cost_est: 40
cost_max: 52
depends_on: ["018a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_cook_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_craft_hammer_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_craft_hand_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_work_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_eat_{se,sw,nw,ne}_<NN>.png
  - art/results/018d-*.md
created: 2026-11-18
---
# 018d Etkileşim animasyonları: pişirme, çekiçle zanaat, el işi, genel iş, yemek

## Amaç
Etkileşim animasyonları (84 px erkek ve kadın). Anahtar eşlemesi: `art/results/018-etkilesim-animasyonlari-plani.md`.

## Tarif
- erkek: `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın: `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84, 8 yön. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 5 anahtar x 8 = 40 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `cook`: `stirring a pot with a wooden spoon in front, circular arm motion`
  - `craft_hammer`: `striking with a hammer on a workbench in front, repeated blows`
  - `craft_hand`: `both hands working a piece of cloth or leather at waist height, small stitching motions, standing`
  - `work`: `generic work: bending, picking something up from the ground, straightening, carrying it`
  - `eat`: `holding a piece of bread to the mouth, eating, small bites, standing`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.

## Ortak işleme (014 kuralları 84 px'e uyarlanmış)
- Yöntem: `animate_character`, `mode=pixminimax`, `keep_first_frame=true`, `end_frame` = ilgili yönün durma karesi (`get_upload_url` ile yükle). Tek deneme.
- Tuval 84x84 (PixelLab başka tuval verirse 84x84'e maskeyle kırpma; ölçekleme yok). Ayak satırı sabit (durma karesiyle aynı), tek pivot x=42, y=2 (ALTTAN).
- Palet: 012b dosyalarının 32 rengi (karakter başına); en yakın RGB, alfa ikili. Döngüde 00 ile son kare aynı poz ise son kare atılır.
- Dosya adı: `Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<yön>_<NN>.png`.
- Doğrulama: `validate_iso.py`, `style.py check --family karakter` (KALDI teslim edilmez), her anahtar için GIF/önizleme ve yan yana sayfası; kimlik kayması (yüz, giysi rengi) ve tepe kırpılması raporda yazılır.

## Doğrulama
018a ölçütleri her klip için; ek olarak elde tutulan nesnenin okunurluğu ve ayak satırı sabitliği. Klip başına GIF `art/results/` altına.


<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

<!-- 2026-11-18 kullanıcı: eat beğenilmedi ("direkt ağzına gitmesin, eliyle ağzına götürsün"); 018d2 ile yeniden. cook, craft_hammer, craft_hand, work için karar bekleniyor. -->
