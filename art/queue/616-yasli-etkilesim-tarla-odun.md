---
id: "616"
title: "Yaşlılar: etkileşim kareleri 2 (çapa, ekim, sulama, odun, genel iş)"
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
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_hoe_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_sow_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_water_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_chop_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_work_{se,sw,nw,ne}_<NN>.png
  - art/results/616-*.md
created: 2026-11-20
---
# 616 Yaşlılar: etkileşim kareleri 2 (tarla ve odun)

## Amaç
Yaşlı erkek ve kadının yalnızca durma ve yürüme karesi var; iş yaparken, yerken, konuşurken durma görseliyle kalıyorlar (oyun genelinde). Kullanıcı kararı (20.11.2026): erkek ve kadın setindeki etkileşim anahtarlarının (018c-e, 018d2-d3, 368) yaşlı eşi yapılsın. Dört parça: 615 gündelik, 616 tarla ve odun, 617 zanaat ve keyif, 618 maden. Kod yaşlı setini kendiliğinden arar (`IsoArt.AdultR84` → `iso_char_elder_<m|f>_r84_<anahtar>_<yön>_NN`); kare gelince kod değişmeden oynar. Zincir pozlarının yaşlı eşi ayrı iştir (576).

## Tarif
- Karakterler (415 raporu): yaşlı erkek `8dcaede5-cc4d-4068-b56f-ab5e327fea1c` (elder_m_r84), yaşlı kadın `b7f31e8d-29f2-4f6a-9092-a02759611159` (elder_f_r84); 84x84. `get_character` ile `completed` doğrulanır (ücretsiz).
- Yöntem 015a ile aynı: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `custom_start_frame_url` ve `end_frame_url` = ilgili yönün **depodaki** durma karesi (`get_upload_url` ile yüklenir). Yaşlı erkekte küreksiz sürüm kullanılır: `iso_char_elder_m_r84_idle_<yön>_v3` varsa o, yoksa `_v2`, yoksa düz ad (423, 423b). PixelLab'deki döndürme görseli kullanılmaz (kürekli).
- Her anahtar için 2 karakter x 4 çapraz yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 çağrı. 5 anahtar x 8 = 40 gen. Aynalama yok. Bağımsız çağrılar paralel gönderilir; çağrıdan önce maliyet satırı okunur, `cost_est`'ten çok farklıysa çağrılmaz.
- Her metnin sonuna yaşlı duruşu eklenir: `, an elderly person moving slowly and carefully, slightly stooped`.
- Eylem metinleri (`action_description`; erkek ve kadın setindeki metinlerle aynı):
  - `hoe`: `tilling soil with a long-handled hoe, rhythmic chopping pull motion`
  - `sow`: `walking slowly and scattering seed from one hand, sweeping arm`
  - `water`: `pouring water from a wooden bucket onto the ground in front, tilting bucket`
  - `chop`: `swinging a woodcutting axe down at a log in front, overhead strike, loop`
  - `work`: `generic work: bending, picking something up from the ground, straightening, carrying it`

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (84x84 tuval, ölçekleme yok, ayak satırı durma karesiyle aynı, palet karakterin durma karelerinden, 00 ile son kare aynı pozsa son kare atılır) ve 018b2'deki ücretsiz dikey hizalama. Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<elder_m|elder_f>_r84_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
`python tools/art/validate_iso.py --kind object --max-colors 32 <dosya>`, `python tools/art/style.py check --family karakter <dosya>` (KALDI teslim edilmez). Klip başına GIF ve yan yana sayfası (durma karesi + 00/02/04/06). Kimlik: yüz, saç ve giysi durma karesiyle aynı; yaşlı erkekte kürek görünmez. Yaşlı gençleşmez: duruş hafif kambur kalır.

## Notlar
- Alet (çapa, balta, kova) okunur ama küçük kalır; karenin dışına taşmaz.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
