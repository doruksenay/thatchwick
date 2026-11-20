---
id: "737"
title: "Kavga ve küslük pozları, yetişkin: tartış, kavgaya hazırlan, kazan, kaybet, küs, özür dile (48)"
status: pending
approved_by: user        # kullanıcı 20.11.2026 ("kavga: dediklerini yapalım", "şimdi alınabilir"); Project belgeleri 67, 67a, 67b
priority: 40
family: karakter
style_gate: false
cost_est: 48
cost_max: 60
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_argue_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_fight_ready_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_fight_win_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_fight_lose_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_sulk_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_apologize_{se,sw,nw,ne}_<NN>.png
  - art/results/737-*.md
created: 2026-11-20
---
# 737 Kavga ve küslük pozları, yetişkin (48)

## Amaç
İlişki sisteminin olumsuz tarafı (Project belgeleri 67a §5, 67b). Kavga Sims'teki gibi **toz bulutunda** oynar (bulut: iş 739); bu işin pozları bulutun öncesi ve sonrasıdır. Kod pozu adla arar (`life.json` → `chains[].steps[].pose`); kare gelene kadar en yakın poza düşer.

| Anahtar | Nerede oynar |
|---|---|
| `argue` | tartışma: el kol sallayarak bağırma (kavgadan önce ya da tek başına) |
| `fight_ready` | toz bulutundan hemen önce: yumruk sıkıp öne eğilme |
| `fight_win` | bulut dağılınca kazanan: ellerini silker, göğsünü kabartır |
| `fight_lose` | bulut dağılınca kaybeden: yerde oturmuş, başını ovuşturur |
| `sulk` | küs durumu: kollarını kavuşturup başını çevirme |
| `apologize` | özür ve barışma: başı eğik, eli göğsünde |

## Tarif
- 572 ile aynı karakterler ve yöntem: erkek `c63fe39d-b168-4732-bf11-b476172c98e8` (man_r84), kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (woman_r84); 84x84. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- Her anahtar için 2 karakter x 4 yön (`south-east`, `south-west`, `north-west`, `north-east`) = 8 `animate_character` çağrısı; `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = durma karesi. Toplam 6 anahtar x 8 = 48 gen. Aynalama yok.
- Eylem metinleri (`action_description`):
  - `argue`: `arguing angrily: waving one arm, pointing a finger forward, leaning in and shouting, loop`
  - `fight_ready`: `clenching both fists and raising them, leaning forward ready to scuffle, short`
  - `fight_win`: `brushing dust off both hands, then puffing out the chest proudly, standing`
  - `fight_lose`: `sitting on the ground with legs out, rubbing the back of the head with one hand, dazed, loop`
  - `sulk`: `standing with arms crossed, turning the head away and lifting the chin, offended, loop`
  - `apologize`: `head bowed, one hand placed on the chest, small humble nod, loop`
- Ton: çizgi film gibi, cozy. Kan, yara, silah yok.
- Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.

## Ortak işleme
018c'deki "Ortak işleme" bölümü aynen (tuval, ayak satırı sabit, pivot, 012b paleti). Dosya adı `Assets/Art/Iso/_aday/anim/iso_char_<karakter>_<anahtar>_<yön>_<NN>.png`.

## Doğrulama
018a ölçütleri her klip için; ayak satırı sabit (`fight_lose`'da oturma karesinde kalça satırı rapora yazılır), giysi ve saç durma karesiyle aynı. Klip başına GIF `art/results/` altına.

## Notlar
- Ergen itişmesi ayrı poz istemez: yetişkin bulutunun küçük boyu (739) ve bu pozlar kullanılır.
- Giysi katmanlarına aktarım 509 yöntemiyle ayrı iştir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
