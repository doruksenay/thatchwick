---
id: "740"
title: "Muhabbet (toplu sohbet) pozları 1: gülümse, kahkaha, kıkırda, başını salla, kollar kavuşuk dinle, omuz silk (48)"
status: accepted
claimed_at: 2026-11-20T20:44Z
approved_by: user        # kullanıcı 20.11.2026 ("toplu sohbeti onaylıyorum", "herkes aynı gülmesin baş sallamasın çeşit olsun", "şimdi alınabilir"); Project belgeleri 67a §6, 67b
priority: 40
family: karakter
style_gate: false
cost_est: 48
cost_max: 60
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_laugh_smile_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_laugh_big_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_giggle_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_listen_nod_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_listen_arms_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_shrug_{se,sw,nw,ne}_<NN>.png
  - art/results/740-*.md
created: 2026-11-20
---
# 740 Muhabbet pozları 1 (48)

## Amaç
Toplu sohbette (muhabbet halkası, Project belgesi 67a §6) dinleyenlerin tepkileri. Kullanıcı: herkes aynı gülmesin, aynı baş sallamasın. Kod her dinleyiciye huyuna ve ilişkisine göre farklı poz seçer (Neşeli → `laugh_big`, Utangaç → `giggle`, Gergin ilişki → `listen_arms`), ayrıca aynı halkada aynı pozu iki kişiye aynı anda vermez.

| Anahtar | Ne |
|---|---|
| `laugh_smile` | hafif gülümseme, omuzlar hafif sarsılır |
| `laugh_big` | kahkaha: geriye yaslanıp elini karnına koyar |
| `giggle` | elini ağzına kapatıp kıkırdama |
| `listen_nod` | başını sallayarak dinleme |
| `listen_arms` | kollar kavuşuk, şüpheci dinleme |
| `shrug` | omuz silkme, "bilmem" |

## Tarif
- 572 ile aynı karakterler: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84.
- Her anahtar için 2 karakter x 4 yön = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 6 x 8 = 48 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `laugh_smile`: `smiling and chuckling softly, shoulders shaking a little, standing, loop`
  - `laugh_big`: `laughing loudly, leaning back with one hand on the belly, standing, loop`
  - `giggle`: `giggling with one hand covering the mouth, shoulders raised, standing, loop`
  - `listen_nod`: `listening attentively and nodding the head, hands relaxed at the sides, standing, loop`
  - `listen_arms`: `listening with arms crossed and head tilted, skeptical, standing, loop`
  - `shrug`: `shrugging both shoulders with palms turned up, standing, short`
- Bağımsız çağrılar paralel gönderilir.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen.

## Doğrulama
018a ölçütleri her klip için; giysi ve saç durma karesiyle aynı. Klip başına GIF `art/results/` altına.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
