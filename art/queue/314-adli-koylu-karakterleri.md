---
id: "314"
title: Adlı köylüler (Orhan, Hatice, Zeynep, Murat, Kerem, Ayşe; 56 px, 8 yön)
status: pending
approved_by: pending     # 012 'accepted' olmadan başlamaz; cost_max 25 altında
family: karakter
style_gate: false
cost_est: 10.2
cost_max: 14
depends_on: ["012"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_char_orhan_56_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_char_hatice_56_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_char_zeynep_56_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_char_murat_56_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_char_kerem_56_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_char_ayse_56_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-08
---
# 314 Adlı köylüler

## Amaç
`life.json` `villagers`: `VillagerService` altı adlı köylüyü köye yerleştirir (günlük programla evden işyerine meydana gider). Her biri mesleğini silüetten belli etmeli, birbirinden ve hanehalkı karakterlerinden ayırt edilmeli. Meslekler: Orhan demirci (52, E), Hatice değirmenci (44, K), Zeynep şifacı (38, K), Murat çoban (31, E), Kerem tüccar (36, E), Ayşe çömlekçi (29, K).

## Tarif
**012'deki yöntem, ayarlar, boy/tuval/dolgu kuralları, renk azaltma ve kayıt adımları aynen geçerlidir** (create_character v3, humanoid, high top-down, 8 yön, ortak açıklama eki, 60x60, ham çıktı `_aday/_ham/`, `reduce_colors` 32 renk). 012'nin kabul edilen temel tasarımı (`farmer_a2_56`) ve set renk/ölçek referansı olarak kullanılır. Çalışma tavanı 14 gen (6 × 1,7).

Karakterler (yaşa göre boy: 52 yaş `size=58`, diğerleri `size=60`):
1. **orhan:** `broad stocky blacksmith in his fifties, grey-streaked dark beard, soot-darkened leather apron over a rolled-sleeve grey tunic, heavy forearms, leather gloves at belt, medium skin`
2. **hatice:** `sturdy middle-aged miller woman, flour-dusted pale apron over a plain beige dress, grey-streaked hair in a headscarf, rolled sleeves, light skin`
3. **zeynep:** `slim herbalist woman in her late thirties, long muted green hooded cloak, braided dark hair, a leather satchel with dried herbs sticking out, olive skin`
4. **murat:** `young shepherd man, wide-brimmed felt hat, rough wool vest over a cream shirt, long wooden crook staff, boots, tanned skin`
5. **kerem:** `well-dressed travelling trader man, dark red-brown coat with a fur-trimmed collar (muted), wide leather belt with a coin pouch, a rolled bundle on the back, short neat beard, light skin`
6. **ayse:** `young potter woman, ochre clay-stained apron over a rust-brown dress, sleeves rolled, hair in a knotted cloth wrap, small clay pot held at hip, dark skin`

## Doğrulama
012'deki: boy ±3 px, taban hizası, renk sayısı ≤32, alfa uyumu. Ek: set önizlemesinde 012'nin 8 karakteri + 6 yeni köylü yan yana (14 kişilik sıra); hiçbir ikisi aynı baskın renkte olmamalı.

## Notlar
- Yön adlandırma 012 ile aynıdır. `Assets/Art/Iso/` köküne onaysız bir şey konmaz.
