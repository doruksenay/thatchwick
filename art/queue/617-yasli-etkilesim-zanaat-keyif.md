---
id: "617"
title: "Yaşlılar: etkileşim kareleri 3 (zanaat, sevgi, eğlence, balık)"
status: running
claimed_at: 2026-11-20T17:52Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet ("yaşlıların eylem kareleri: evet"), Project belgesi 55d
priority: 50
family: karakter
style_gate: false
cost_est: 40
cost_max: 52
depends_on: ["415"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_craft_hammer_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_craft_hand_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_love_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_fun_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_fish_{se,sw,nw,ne}_<NN>.png
  - art/results/617-*.md
created: 2026-11-20
---
# 617 Yaşlılar: etkileşim kareleri 3 (zanaat ve keyif)

## Amaç
Yaşlı erkek ve kadının yalnızca durma ve yürüme karesi var; iş yaparken, yerken, konuşurken durma görseliyle kalıyorlar (oyun genelinde). Kullanıcı kararı (20.11.2026): erkek ve kadın setindeki etkileşim anahtarlarının (018c-e, 018d2-d3, 368) yaşlı eşi yapılsın. Dört parça: 615 gündelik, 616 tarla ve odun, 617 zanaat ve keyif, 618 maden. Kod yaşlı setini kendiliğinden arar (`IsoArt.AdultR84` → `iso_char_elder_<m|f>_r84_<anahtar>_<yön>_NN`); kare gelince kod değişmeden oynar. Zincir pozlarının yaşlı eşi ayrı iştir (576).

## Tarif
- Karakterler (415 raporu): yaşlı erkek `8dcaede5-cc4d-4068-b56f-ab5e327fea1c` (elder_m_r84), yaşlı kadın `b7f31e8d-29f2-4f6a-9092-a02759611159` (elder_f_r84); 84x84. `get_character` ile `completed` doğrulanır (ücretsiz).
- Yöntem 015a ile aynı: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `custom_start_frame_url` ve `end_frame_url` = ilgili yönün **depodaki** durma karesi (`get_upload_url` ile yüklenir). Yaşlı erkekte küreksiz sürüm kullanılır: `iso_char_elder_m_r84_idle_<yön>_v3` varsa o, yoksa `_v2`, yoksa düz ad (423, 423b). PixelLab'deki döndürme görseli kullanılmaz (kürekli).
- Her anahtar için 2 karakter x 4 çapraz yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 çağrı. 5 anahtar x 8 = 40 gen. Aynalama yok. Bağımsız çağrılar paralel gönderilir; çağrıdan önce maliyet satırı okunur, `cost_est`'ten çok farklıysa çağrılmaz.
- Her metnin sonuna yaşlı duruşu eklenir: `, an elderly person moving slowly and carefully, slightly stooped`.
- Eylem metinleri (`action_description`; erkek ve kadın setindeki metinlerle aynı):
  - `craft_hammer`: `striking with a hammer on a workbench in front, repeated blows`
  - `craft_hand`: `both hands working a piece of cloth or leather at waist height, small stitching motions, standing`
  - `love`: `embracing a partner: arms opening and closing in a hug, warm gesture`
  - `fun`: `playing: a small joyful dance or hop, arms swinging`
  - `fish`: `casting and holding a fishing rod toward the water in front, slight tug`

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (84x84 tuval, ölçekleme yok, ayak satırı durma karesiyle aynı, palet karakterin durma karelerinden, 00 ile son kare aynı pozsa son kare atılır) ve 018b2'deki ücretsiz dikey hizalama. Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<elder_m|elder_f>_r84_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
`python tools/art/validate_iso.py --kind object --max-colors 32 <dosya>`, `python tools/art/style.py check --family karakter <dosya>` (KALDI teslim edilmez). Klip başına GIF ve yan yana sayfası (durma karesi + 00/02/04/06). Kimlik: yüz, saç ve giysi durma karesiyle aynı; yaşlı erkekte kürek görünmez. Yaşlı gençleşmez: duruş hafif kambur kalır.

## Notlar
- `fun`: yaşlıya yakışan küçük, keyifli bir sallanma; zıplama abartılmaz.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
