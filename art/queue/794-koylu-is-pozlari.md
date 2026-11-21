---
id: "794"
title: "Köylü iş pozları: öğüt, asaya yaslan, ölç, ağ onar, tezgâh sil, çalgı çal (48)"
status: running
claimed_at: 2026-11-21T09:21Z
approved_by: user
priority: 45
family: karakter
style_gate: false
cost_est: 48
cost_max: 56
depends_on: ["018a3", "571", "572"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_grind_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_lean_staff_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_measure_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_mend_net_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_wipe_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_play_music_{se,sw,nw,ne}_<NN>.png
  - art/results/794-*.md
created: 2026-11-21
---
# 794 Köylü iş pozları: öğüt, asaya yaslan, ölç, ağ onar, tezgâh sil, çalgı çal (48)

## Amaç
Yaşayan köy, parti 1 (Project belgeleri 66d Y1, 66f §C.11 ve §F.18): her meslek işyerinde 3-4 işi sırayla yapar (`life.json` → `villagerWork`). İşlerin çoğu var olan ya da kuyruktaki pozları kullanır (605-608: körük, daldır, havan, dikiş, testere, rende, yont, kazı, yaz, serp, çark). Bu iş yalnızca hiçbir yerde olmayan altı pozu üretir. Kod pozu adla arar (`GameRootVillage.VillagePoseFrame`); kare gelene kadar `villagerWork[].steps[].fallback` pozuna düşer. Kare gelince kod değişmeden oynar.

| Anahtar | Kim, nerede | Yedek poz (şimdi) |
|---|---|---|
| `grind` | değirmenci, el değirmeninin kolunu çevirir | `work` |
| `lean_staff` | çoban, sürüye bakarken asasına yaslanır | `idle` |
| `measure` | terzi, kumaşı kulaç ve arşınla ölçer | `craft_hand` |
| `mend_net` | balıkçı, ağı dizine serip onarır | `craft_hand` |
| `wipe` | hancı, tezgâhı bezle siler | `craft_hand` |
| `play_music` | Müzik becerisi olan köylü meyhanede akşam çalar | `fun` |

## Tarif
- 018c ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 6 anahtar x 8 = 48 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `grind`: `standing beside a waist-high stone hand mill, turning its wooden handle around in slow circles with one hand, loop`
  - `lean_staff`: `standing relaxed and leaning on a tall wooden shepherd's staff with both hands, looking out and shifting weight slowly, loop`
  - `measure`: `holding a length of cloth between both hands and stretching it out at arm's length to measure it, then folding it, loop`
  - `mend_net`: `sitting on a low stool with a fishing net over the knees, pulling a cord through the mesh with small tying motions, loop`
  - `wipe`: `wiping a waist-high wooden counter with a cloth in wide circular strokes, loop`
  - `play_music`: `standing and playing a small medieval lute held across the chest, gently strumming and swaying, loop`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit, karakterin giysisi ve saçı durma karesiyle aynı. Elde tutulan nesne (bez, asa, ud, ağ) okunur ama küçük olmalı. Dönem: ud ortaçağ çalgısı, metal ya da modern parça yok. Klip başına GIF `art/results/` altına.

## Notlar
- Giysi katmanları (43c, 509 planı): bu kareler gövde setidir; giysi ailelerine aktarım 509'daki toplu parti yöntemiyle ayrı iştir.
- El konumları: her klibin ilk karesindeki tutan elin konumu rapora yazılır (`hand_anchors.txt` için).
- Yaşlı ve çocuk kareleri istenmez (60a: yaşlı ve çocukta yalnızca yeme, oturma, uyuma, yıkanma).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
