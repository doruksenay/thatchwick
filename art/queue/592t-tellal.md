---
id: "592t"
title: "Tellal (84 px, 8 yön durma)"
status: pending
approved_by: user        # otomatik akış (CLAUDE.md, 2026-11-17); kullanıcı 20.11.2026 sohbet: tellal kararı "Pano + tellal", "assetleri de sıraya ekle"
priority: 40
family: karakter
style_gate: false
cost_est: 3
cost_max: 6
depends_on: ["012b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_crier_r84_idle_<yön>.png
  - art/results/592t-tellal.md
created: 2026-11-20
---
# 592t Tellal

## Amaç
Project belgesi 62 §4: tellal her sabah 08:00'de, yeni ilan varsa şehir meydanındaki ilan tahtasının yanında bir oyun saati durup ilanları bağırır. Kod hazır (`OddWalkerKind.Crier`, görsel takımı `iso_char_crier_r84`); görsel gelene kadar kırmızı bir yer tutucu çiziliyor. Meydanda köylülerden uzaktan ayırt edilmeli.

## Tarif
- **Araç:** 496 ile aynı (012b yöntemi): `create_character`, `mode=v3`, `body_type=humanoid`, `view=high top-down`, `outline=single color black outline`, `detail=high detail`, `size=84`, **8 yön**.
- **Oran:** "realistic adult proportions, about 7 heads tall, small head relative to body, long legs, not chibi, not cute, not big-headed, grounded medieval".
- **Karakter (crier):** "medieval town crier, stout middle-aged man with a short brown beard, burgundy wool tabard with a plain wheat-gold border over a cream linen shirt, soft dark felt cap with one feather, a brass handbell held in the right hand at chest height, a rolled parchment in the left hand, brown hose and plain leather shoes, mouth slightly open as if calling out"
- **Ortak ek:** 012b'deki ortak ek ve 32 renk `reduce_colors` aynen.
- **Kimlik referansı:** araç referans alıyorsa `Assets/Art/Iso/_aday/iso_char_man_r84_cand_se.png` (palet ve çizgi için).
- **Tuval ve pivot:** 84x84, ayaklar alttan 2 px, pivot x=42 y=2. Dosya adı `iso_char_crier_r84_idle_<yön>.png`, yönler e, ne, n, nw, w, sw, s, se.

## Doğrulama
- 415 doğrulaması: boy ±3 px, taban hizası, renk ≤32, alfa uyumu; `python tools/art/style.py check --family karakter <dosya> ...`.
- Yan yana sayfası: `man_r84`, `woman_r84`, 496'nın iki ziyaretçisi ve tellal aynı satırda; köylülerle aynı ölçekte, bordo cübbe ve çanla siluetten ayırt ediliyor.
- Dönem: çan, parşömen, keçe başlık ortaçağ. Borazan, davul, üniforma apoleti, üç köşeli şapka (18. yüzyıl tellalı) yok.

## Notlar
- Yalnızca durma kareleri. Çan sallama ("cry" hareketi) bu işte yok; kod durma karesini hafifçe sallıyor. İstenirse 510'daki gibi ayrı hareket işi açılır.
- `Assets/Art/Iso/` köküne onaysız bir şey konmaz.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
