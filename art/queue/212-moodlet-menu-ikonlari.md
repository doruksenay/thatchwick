---
id: "212"
title: Moodlet, menü ve panel ikonları (16x16)
status: accepted
claimed_at: 2026-11-19T12:02Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: arayuz
style_gate: false
cost_est: 31
cost_max: 40
depends_on: ["210"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_moodlet_{good_meal,rested,clean,inspired,had_fun,nice_chat,newlywed,new_baby,grief,harmony,bright_room,good_harvest,proud,pet_love,animal_died,crop_frozen,sharp_tools,slaughtered,injured,gift,festival}.png
  - Assets/Art/Iso/_aday/ui_icon_menu_{build,family,village,market}.png
  - Assets/Art/Iso/_aday/ui_icon_ui_{close,play,diamond,lock,dice,undo}.png
created: 2026-11-17
---
# 212 Moodlet, menü ve panel ikonları

## Amaç
Karakter panelindeki moodlet kartlarının ikonları, HUD menü düğmeleri ve panel düğmeleri. 31 ikon, 16x16.

İhtiyaç, ruh hali, etkinlik ve hava ikonları ayrı iştedir (315). Koşul moodlet'leri (Aç, Yorgun, Üşüyor, Sıkılmış, Kirli, Yalnız) ihtiyaç ikonlarını kullanır.

Anahtarlar kodda `MoodCatalog` sabitleridir. `injured` (Yaralı) yeni moodlet'tir: doküman 14, bölüm 3F. `gift` ve `festival` olay moodlet'leridir.

## Tarif
- **Araç (kullanıcı kararı 2026-11-19, işçinin (a) önerisi):** her ikon **tek tek 32x32** üretilir (`create_map_object` ya da `create_image_pixen` 32x32; sayfa yok), sonra en yakın komşuyla **16x16'ya indirilir** ve `reduce_colors` ile en çok 12 renge düşürülür. 16x16 doğrudan üretim ve ikon sayfası denendi, tutmadı (Sorular).
  - İkon başına 1 gen + renk azaltma 0,1: 31 ikon ≈ 34 gen. `cost_max` 40. Okunmayan ikon için ikinci deneme yalnızca bütçe kalırsa.
  - 32x32 ham hâli de `_aday/` altına `_32` ekiyle konur (büyük arayüzde kullanılabilir).
- **Eski yöntem (tutmadı):** 315 ile aynı, `create_image_pixflux` ya da `create_map_object`; 5-8 ikonluk sayfalar hâlinde, 1 px boşlukla kesilir.
  - Kesim betiği 315'te yazılmışsa onu kullan, yoksa `tools/art/` altına ekle.
  - 16x16 desteklenmiyorsa 32x32 üret, en yakın komşuyla 16'ya indir.
- **Stil:** 210'da seçilen yön (`yon:` satırı; yoksa varsayılan `parsomen`). Kontur ve renk paneldeki gibi. Tek renk koyu kontur, düz gölgeleme, ışık sol-üstten, 3-5 renk. Anlam şekilden okunmalı.
- **İkonlar:**

| Anahtar | Şekil |
|---|---|
| good_meal | buharı tüten kâse |
| rested | yastık ve küçük "z" yerine ay |
| clean | su damlası ve parıltı |
| inspired | açık kitap ve küçük ışık |
| had_fun | zar |
| nice_chat | iki konuşma balonu |
| newlywed | iki halka |
| new_baby | kundak |
| grief | kırık kalp |
| harmony | dört küçük eş kare |
| bright_room | yanan kandil |
| good_harvest | buğday demeti |
| proud | çekiç ve yıldız |
| pet_love | pati ve kalp |
| animal_died | soluk tüy |
| crop_frozen | buz kristali ve filiz |
| sharp_tools | balta ve parıltı |
| slaughtered | bıçak |
| injured | bez sargı |
| gift | kurdeleli paket |
| festival | flama |
| menu: build | çekiç |
| menu: family | iki baş silueti |
| menu: village | çatılı ev |
| menu: market | kese |
| ui: close | çarpı |
| ui: play | üçgen |
| ui: diamond | elmas |
| ui: lock | asma kilit |
| ui: dice | zar |
| ui: undo | geri ok |

- **Maliyet:** ~31 ikon / sayfa başı 3 gen ≈ 12, yeniden deneme payıyla `cost_max` 20.

## Doğrulama
- 315 ile aynı: 16x16, yarı saydam yok, renk ≤ 12.
- 3 kat ölçekte koyu (#333333) ve açık (parşömen #EADBB8) zeminde okunuyor mu?
- Toplu önizleme x4: `art/results/212-ikonlar-x4.png`.
- `ui_mockup.py mockup durum --panel <210 paneli>` yeniden çalıştırılır. Moodlet kartlarında ikonlar görünmeli: `art/results/212-durum-onizleme-x2.png`.

## Notlar
- Ad kuralı `ui_icon_<grup>_<anahtar>`; kod eşlemesi 315'teki gibi.

## Sorular
- Üretilemedi, 2 gen harcandı (cost_max 20 içinde). Ham çıktılar PixelLab'de: job 3d6585b2-a5fb-43ea-8996-bee2c4d3fcd7 (sayfa), 75752e86-06b6-4d06-9d7c-19eeb3f0752c (zar).
  1. `create_image_pixen` 128x64 "ikon sayfası": ikonlar parşömen çerçeveli karolar, sıra 5+5 (istenen 4x2 değil), aralıklar eşit değil, her ikon ~12 px ve ayrıntılı; 16x16'ya temiz kesilemez.
  2. `create_image_pixen` 16x16 tek ikon (zar): 45 renk, şekil tanınmıyor (mor yumru). Renk ≤12 ve okunurluk ölçütü tutmaz.
- Tek ikon başına 1 gen = 31 gen; `cost_max` 20'yi ve 2x sapma sınırını (24) aşar, üstelik 16x16 kalitesi yetersiz.
- Öneri: (a) `cost_max`'ı 40'a çıkar ve 32x32 tek ikon üret (`create_image_pixen` 32x32 ya da `create_map_object`), 16'ya komşuyla indir + `reduce_colors`; ya da (b) ikonlar `pixellab: false` ile pixelart_workbench'te elle çizilsin. Karar kullanıcıda; 315 de aynı yönteme bağlı.

- 2026-11-19 (kod oturumu, kullanıcı kararı "önerdiklerini yap"): çözüldü. (a) seçildi: 32x32 tek ikon, 16'ya en yakın komşuyla indirme, `reduce_colors`; `cost_max` 40. İş `pending`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
