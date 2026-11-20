---
id: "497"
title: "Easter egg moodlet, başarım ve Tuhaflıklar ikonları (13)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet ("bu moodletler için de asset üretmemiz lazım", "yaz bunları ve sen ekle sıraya"), Project belgeleri 55 ve 55b
priority: 50
family: arayuz
style_gate: false
cost_est: 13
cost_max: 20
depends_on: ["212"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_moodlet_{animal_spoke,confidant,not_crazy,believed,family_tale,nobody_believes,odd_chat,thoughtful,let_down}.png
  - Assets/Art/Iso/_aday/ui_icon_achievement_{animal_spoke,runs_in_family,question_the_sword}.png
  - Assets/Art/Iso/_aday/ui_icon_tab_oddities.png
created: 2026-11-20
---
# 497 Easter egg moodlet, başarım ve Tuhaflıklar ikonları (13)

## Amaç
Project belgesi `55-easter-eggler.md`:
- **Hayvanla konuşma** (§3.3) ve **bulunan silah** (§4) moodlet'leri karakter panelindeki moodlet kartlarında görünür; kurbağayı öpme "Hayal kırıklığı" verir (§2).
- Üç **başarım** ikonu ve Doğa Defteri'ndeki gizli **Tuhaflıklar** sekmesinin ikonu.

Kod henüz yok; moodlet anahtarları `MoodCatalog`'a bu adlarla eklenecek (kod oturumu, belge 55b'deki prompt).

## Tarif
**212 ile aynı yöntem** (rapordaki ayarlar: `create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; ikon başına tek çağrı). 32x32 ham hâli alfa ikili yapılıp `_32` ekiyle konur; 16x16 hâli en yakın komşuyla indirilir ve yerel Pillow ile en çok 12 renge düşürülür. Başarım ikonları ve sekme ikonu yalnızca 32x32 teslim edilir (ad `_32` eksiz), en çok 16 renk.

Stil: 210 yönü (varsayılan `parsomen`), tek renk koyu kahve kontur, düz gölgeleme, ışık sol üstten, 3-5 renk. Palet: `earthy medieval palette: wood browns, linen cream, burgundy, wheat gold, iron grey, muted green`. Ortak ek: `, single game icon, one simple shape centered, thick readable silhouette, no frame, no background, no letters`.

**212'den ders:** 16x16'da iki küçük nesne okunmuyor (nice_chat, menu_family zayıf çıktı). Her ikon tek ana şekil; ikinci öğe varsa tek ve iri bir işaret.

| Dosya | Moodlet / kullanım | Açıklama (+ ortak ek) |
|---|---|---|
| ui_icon_moodlet_animal_spoke | Hayvan benimle konuştu! | `a brown cow head facing front with a white speech bubble above it` |
| ui_icon_moodlet_confidant | Dert ortağı | `a brass cowbell on a leather strap with a small red heart on it` |
| ui_icon_moodlet_not_crazy | Deli değilmişim | `two identical brass cowbells side by side tied with one burgundy ribbon` |
| ui_icon_moodlet_believed | İnanıldı | `a cream speech bubble with a big green check mark inside` |
| ui_icon_moodlet_family_tale | Aile hikâyesi doğruymuş | `a closed old leather-bound book with a big paw print on the cover` |
| ui_icon_moodlet_nobody_believes | Kimse inanmıyor | `a grey speech bubble crossed out by one thick dark red diagonal line` |
| ui_icon_moodlet_odd_chat | Tuhaf bir sohbet | `a cream speech bubble with a big question mark symbol inside` |
| ui_icon_moodlet_thoughtful | Düşünceli | `an old rusty sword stuck upright in a small mound of earth, one tiny white flower at its base` |
| ui_icon_moodlet_let_down | Hayal kırıklığı | `a small green frog sitting, front view, with a sad downturned mouth` |
| ui_icon_achievement_animal_spoke | Başarım: Dile geldi | `a brown cow head with a speech bubble, inside a round wheat-gold laurel border` |
| ui_icon_achievement_runs_in_family | Başarım: Bizde de oldu (gizli) | `a small tree branch with two leaves, a paw print on each leaf, inside a round wheat-gold laurel border` |
| ui_icon_achievement_question_the_sword | Başarım: Silahı bul ve sorgula | `a rusty sword stuck in the ground with a question mark symbol beside it, inside a round wheat-gold laurel border` |
| ui_icon_tab_oddities | Doğa Defteri, Tuhaflıklar sekmesi | `a small green frog wearing a tiny golden crown, front view` |

## Doğrulama
- Moodlet ikonları 16x16 (+ `_32`), yarı saydam 0, renk ≤ 12. Başarım ve sekme ikonları 32x32, yarı saydam 0, renk ≤ 16.
- `python tools/art/style.py check --family arayuz <dosya> ...`.
- Toplu önizleme x4, parşömen (#EADBB8) ve koyu (#333333) zeminde: `art/results/497-ikonlar-x4.png`, `art/results/497-ikonlar-x4-koyu.png`.
- Göz kontrolü 3 kat ölçekte:
  - üç balon (`believed`, `nobody_believes`, `odd_chat`) içindeki işaretten ayırt edilmeli;
  - `confidant` 212'deki `pet_love` (pati ve kalp) ile, `family_tale` `inspired` (açık kitap) ile, `let_down` `grief` (kırık kalp) ile karışmamalı;
  - `let_down` kurbağasında taç yok, `ui_icon_tab_oddities` kurbağasında taç var.
- Dönem: çıngırak, kitap, kılıç ortaçağ nesnesi. Kılıç paslı ve toprağa saplı; elde tutulan ya da kanlı silah yok (oyunda dövüş yok, belge 55 §4).

## Notlar
- Balonlardaki onay, çarpı ve soru işareti semboldür, yazı değildir; harf ya da kelime çıkarsa o ikon teslim edilmez.
- Kabul edilince dosyalar `Assets/Resources/UI/Icons/` altına da kopyalanır (oyun oradan yükler; ad aynı kalır).
- Kilitli başarım için ayrı çizim yok; kod gri gösterir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
