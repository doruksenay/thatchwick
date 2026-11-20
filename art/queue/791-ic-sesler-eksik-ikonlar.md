---
id: "791"
title: "İç Sesler kartı: eksik iç ses ikonları, ruh hali yüzleri, Tuvalet ve genel yedekler (42)"
status: pending
approved_by: pending     # 21.11.2026: nakış yaması dili (820 pilotu) seçildi; 820 kabul edilince bu iş o dile göre yeniden yazılır (belge 68a §6)
priority: 50
family: arayuz
style_gate: false
cost_est: 42
cost_max: 55
depends_on: ["212"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_moodlet_{accident,catharsis,cave_in_survived,cold_water,complimented,composed,doctor_visit,dusty,feast,festival_speech,first_steps,got_letter,hopeful,hot_bath,lulled,master_work,midwife_care,new_foal,new_song,persuasive,played_tavern,potion_calm,potion_focus,potion_vigor,relieved,same_meal,skill_showcase,swim,unburdened,understood,varied_table,vein_found,well_cared,wet}.png
  - Assets/Art/Iso/_aday/ui_icon_mood_{miserable,unhappy,neutral,good,happy}.png
  - Assets/Art/Iso/_aday/ui_icon_need_bladder.png
  - Assets/Art/Iso/_aday/ui_icon_moodlet_{joy,gloom}.png
created: 2026-11-21
---
# 791 İç Sesler kartı: eksik ikonlar (42)

## Amaç
Alttaki karakter kartı Sims gibi oldu (Project belgesi 68, kod dalı `ic-sesler-karakter-karti`): ruh hali yüz ikonu ve sözcükle, iç sesler çerçeveli ikonla gösterilir, sayı görünmez. Kod ikonları `InnerVoiceView.IconKeys` sırasıyla arar (eş, kendi ikonu, aile, genel yedek). Bugün 47 iç sesin ikonu var (212, 497); `MoodCatalog`'daki 34 iç sesin, beş ruh hali basamağının, Tuvalet ihtiyacının ve iki genel yedeğin ikonu yok. Bu iş onları üretir. İkon gelene dek kod genel yedeği, o da yoksa boş çerçeveyi çizer.

Aile ikonu paylaşanlar bu işte **yok** (kodda eşlendi): ihtiyaç iç sesleri ihtiyaç ikonunu, `injured_*` "Yaralı"yı, `meal_*` "Doyurucu yemek"i, `harmony_*` "Uyumlu ev"i, `ashamed` "Utandı"yı, `chilled` Isınma ikonunu kullanır.

## Tarif
**212 ve 497 ile aynı yöntem:** `create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; ikon başına tek çağrı. 32x32 ham hâli alfa ikili yapılıp `_32` ekiyle konur; 16x16 hâli en yakın komşuyla indirilir ve yerel Pillow ile en çok 12 renge düşürülür.

Stil: tek renk koyu kahve kontur, düz gölgeleme, ışık sol üstten, 3-5 renk. Palet: `earthy medieval palette: wood browns, linen cream, burgundy, wheat gold, iron grey, muted green`. Ortak ek: `, single game icon, one simple shape centered, thick readable silhouette, no frame, no background, no letters`. Çerçeveyi kod çizer (keyif yosun yeşili, kasvet bordo); ikonda çerçeve olmaz.

**212'den ders:** 16x16'da iki küçük nesne okunmuyor; her ikon tek ana şekil, ikinci öğe varsa tek ve iri bir işaret.

### A. Ruh hali yüzleri (5)
Beşi aynı aile: aynı yuvarlak keten rengi yüz, aynı kontur, yalnızca ağız ve kaş değişir. Renk kodu kartın levhasında; yüzün kendisi keten kalır, yanaklar basamağa göre.

| Dosya | Basamak | Açıklama (+ ortak ek) |
|---|---|---|
| ui_icon_mood_happy | Mutlu | `a round linen-cream face with closed happy eyes, a wide open smile and rosy cheeks` |
| ui_icon_mood_good | İyi | `a round linen-cream face with open eyes and a gentle closed smile` |
| ui_icon_mood_neutral | Nötr | `a round linen-cream face with open eyes and a flat straight mouth` |
| ui_icon_mood_unhappy | Mutsuz | `a round linen-cream face with worried eyebrows and a small downturned mouth` |
| ui_icon_mood_miserable | Perişan | `a round linen-cream face with tightly shut eyes, a deep frown and one big tear drop` |

### B. Genel yedekler ve Tuvalet (3)
| Dosya | Kullanım | Açıklama |
|---|---|---|
| ui_icon_moodlet_joy | kendi ikonu olmayan keyif | `a small green sprout with two round leaves and a tiny sun spark above` |
| ui_icon_moodlet_gloom | kendi ikonu olmayan kasvet | `a small dark grey rain cloud with three rain drops` |
| ui_icon_need_bladder | Tuvalet ihtiyacı (315 setinde yoktu) | `a small wooden outhouse door with a crescent moon cut-out` |

### C. İç sesler (34)
| Dosya (`ui_icon_moodlet_…`) | İç ses | Açıklama |
|---|---|---|
| accident | Kaza (Tuvalet) | `a small brown puddle beside a worn leather shoe` |
| catharsis | İçini döktü | `an open hand releasing one small white bird` |
| cave_in_survived | Göçükten sağ çıktı | `a miner's iron helmet with a small dent, a few pebbles below` |
| cold_water | Soğuk su | `a wooden bucket of water with ice shards and a blue shiver mark` |
| complimented | İltifat aldı | `a single pink rose` |
| composed | Yeni ezgi | `a wooden lute with three floating music notes` |
| doctor_visit | Hekim baktı | `a brown leather physician's bag with a green leaf mark` |
| dusty | Tozlu | `a grey dust cloud puff with small specks` |
| feast | Ziyafet | `a big roast on a round wooden platter with a bread loaf` |
| festival_speech | Şenlikte söz aldık | `a wooden lectern with a burgundy cloth banner` |
| first_steps | İlk adımlar | `two tiny baby footprints` |
| got_letter | Mektup aldı | `a folded parchment letter with a red wax seal` |
| hopeful | Umutlu | `a small lit candle with a warm halo` |
| hot_bath | Sıcak banyo | `a wooden bath tub with three steam curls` |
| lulled | Ninniyle uyudu | `a wooden cradle with a crescent moon above` |
| master_work | Ustalık eseri | `a carpenter's hammer crossed with a gold star` |
| midwife_care | Ebe gözetiminde | `a folded cream swaddling cloth with a sprig of herbs` |
| new_foal | Tay doğdu | `a small brown foal head, front view` |
| new_song | Yeni türkü | `an open songbook with two music notes` |
| persuasive | İkna edici | `a pair of brass merchant scales tipped to one side` |
| played_tavern | Meyhanede çaldı | `a wooden frame drum with two crossed beaters` |
| potion_calm | Sakinlik iksiri | `a small round glass bottle with soft blue liquid and a cork` |
| potion_focus | Odak iksiri | `a small round glass bottle with violet liquid and a cork` |
| potion_vigor | Dinçlik iksiri | `a small round glass bottle with bright red liquid and a cork` |
| relieved | Rahatladı | `a wooden outhouse door slightly open with a small sparkle` |
| same_meal | Yine aynı yemek | `a clay bowl of grey porridge with a spoon stuck upright` |
| skill_showcase | Ustayı seyretti | `a single wide-open eye with a small gold sparkle` |
| swim | Yüzdü | `blue water waves with a single splash` |
| unburdened | Dertleşti | `a heavy sack slipping off, a feather floating above` |
| understood | Anlaşıldı | `a big ear with a small heart beside it` |
| varied_table | Çeşitli sofra | `a wooden board with an apple, cheese wedge and bread` |
| vein_found | Damar buldu | `a grey rock chunk with a shining gold vein` |
| well_cared | Bakımlı | `a wooden comb with a small heart` |
| wet | Islak | `a single big blue water drop with a smaller drop beside it` |

## Doğrulama
- 16x16 (+ `_32`), yarı saydam 0, renk ≤ 12. `python tools/art/style.py check --family arayuz <dosya> ...`. `hopeful` ışık içerir: `--light`.
- Toplu önizleme x4, keten (#F3EAD6, kartın kutucuk zemini) ve koyu (#333333) zeminde: `art/results/791-ikonlar-x4.png`, `art/results/791-ikonlar-x4-koyu.png`.
- Beş yüz yan yana `art/results/791-yuzler-x4.png`: aynı aile görünmeli, basamak 16x16'da ağızdan okunmalı.
- Karışma denetimi 3 kat ölçekte: `relieved` ile `need_bladder` (ikisi de kapı; `relieved` aralık ve parıltılı), üç iksir (yalnızca sıvı rengi farklı, bilerek), `wet` ile `cold_water`, `dusty` ile `moodlet_gloom` (toz bej-gri, bulut koyu ve yağmurlu).
- Dönem: lavta, def, kantar, mum, beşik ortaçağ nesnesi; cam şişe Camcı zinciriyle var (belge 27).

## Notlar
- Harf, rakam, yazı çıkan ikon teslim edilmez.
- Kabul edilince kod kütüphaneden (`Assets/Art/Iso/_aday`) okumaya devam eder; ayrıca kopya gerekmez. `IconKeys` önce `_32` sürümünü 16 birimde çizer (1080p'de tam 2x).
- 59 kod içi iç sesin (`cook.*`, `mood.*`, `eff_*`, oyuncak, düğün) çoğu kodda var olan ikonlara eşlendi; eşlenmeyenler genel yedeği çizer. Onlar için ikinci iş, bu iş kabul edildikten sonra yazılır (belge 68 §6).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
