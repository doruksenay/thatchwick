---
id: "738"
title: "Yaşlı pozları: bastonla tehdit, tartış, küs, gül, anlat, başını salla (48)"
status: pending
approved_by: user        # kullanıcı 20.11.2026 ("şimdi alınabilir"); Project belgeleri 67a, 67b
priority: 40
family: karakter
style_gate: false
cost_est: 48
cost_max: 60
depends_on: ["015a"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_cane_threat_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_argue_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_sulk_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_laugh_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_tell_story_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_listen_nod_{se,sw,nw,ne}_<NN>.png
  - art/results/738-*.md
created: 2026-11-20
---
# 738 Yaşlı pozları: kavga ve toplu sohbet (48)

## Amaç
Yaşlılar kavgaya girmez, **bastonla tehdit eder** (Project belgesi 67a §5.1). Toplu sohbette (muhabbet) yaşlılar ayrı gövdeyle gülüp anlatır. Kod pozu adla arar; kare gelene kadar en yakın poza düşer.

| Anahtar | Nerede oynar |
|---|---|
| `cane_threat` | kavga yerine: bastonu havaya kaldırıp sallama (komik) |
| `argue` | tartışma |
| `sulk` | küs |
| `laugh` | muhabbette gülme |
| `tell_story` | muhabbette masal ya da eski hikâye anlatma |
| `listen_nod` | dinlerken başını sallama |

## Tarif
- Karakterler: 015a işindeki yaşlı erkek ve yaşlı kadın (`elder_m_r84`, `elder_f_r84`; kimlikler 015a raporunda); 84x84. Yaşlı erkek küreksiz sürümdür (423). `cane_threat` için elde baston çizilir; yaşlı kadın da bastonlu.
- Her anahtar için 2 karakter x 4 yön = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 6 x 8 = 48 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `cane_threat`: `raising a wooden walking cane overhead and shaking it angrily, grumbling, slightly hunched, loop`
  - `argue`: `arguing, wagging a finger, slightly hunched, loop`
  - `sulk`: `arms crossed, turning the head away, offended, slightly hunched, loop`
  - `laugh`: `laughing warmly with shoulders shaking, one hand on the belly, slightly hunched, loop`
  - `tell_story`: `telling an old story with slow wide hand gestures, slightly hunched, loop`
  - `listen_nod`: `listening and nodding slowly, hands folded in front, slightly hunched, loop`
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen.

## Doğrulama
018a ölçütleri her klip için; baston okunur ama ince. Klip başına GIF `art/results/` altına.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
