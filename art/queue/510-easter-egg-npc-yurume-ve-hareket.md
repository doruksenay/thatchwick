---
id: "510"
title: "Easter egg NPC'leri: Johan Wick ve atsız şövalye yürüme, Johan sevme ve dağıtma"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet (easter egg sahneleri), Project belgeleri 55 ve 55d
priority: 50
family: karakter
style_gate: false
cost_est: 32
cost_max: 45
depends_on: ["496"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_johan_wick_r84_walk2_{e,ne,n,nw,w,sw,s,se}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_horseless_knight_r84_walk2_{e,ne,n,nw,w,sw,s,se}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_johan_wick_r84_pet_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/anim/iso_char_johan_wick_r84_wreck_{se,sw,nw,ne}_<NN>.png
  - art/results/510-*.md
created: 2026-11-20
---
# 510 Easter egg NPC'leri: yürüme, sevme, dağıtma

## Amaç
Johan Wick ve atsız şövalye artık haritada yürüyor (kod: `OddWalkers`, `AnimalViews.SyncWalkers`). Johan arsanın kenarından girer, köpeğe yürür, eğilip sever, ekinleri tek tek dağıtır ve çıkar; şövalye köy yolunu bir uçtan öbürüne, at biner gibi yürür. Şu an yalnızca durma kareleri var (496); yürürken kodla sallanıyorlar.

## Tarif
- Karakter kimlikleri (496 raporu): johan_wick `8d5fc676-e8ba-4988-ba86-d18df6ab56c8`, horseless_knight `f3a16e27-2afe-4bee-9157-9d33eb2be75b`; 84x84, 8 yön. `get_character` ile `completed` ve 8 rotasyon doğrulanır (ücretsiz).
- **Yürüme (`walk2`), iki karakter x 8 yön = 16 çağrı.**
  - Johan Wick, metin 018b2 ile aynen: `walking at a calm steady pace, a full stride cycle with the left foot then the right foot stepping equally, head and shoulders level, no bouncing, no limping`.
  - Atsız şövalye (at yok; görünmeyen bir ata biner gibi yürür): `walking with a bouncy trotting gait as if riding an invisible horse, both fists held forward gripping invisible reins, knees lifting high, left foot then right foot stepping equally`. Bu karakterde hafif sekme isteniyor; 018b2 sekme ölçütü uygulanmaz (ayak satırı yine sabit).
- **Johan Wick yerinde hareketler, 4 çapraz yön (`south-east`, `south-west`, `north-west`, `north-east`) x 2 = 8 çağrı:**
  - `pet`: `crouching down and gently petting a small dog in front of him with one hand, slow strokes, calm`
  - `wreck`: `stomping and kicking at low plants on the ground in front of him, sweeping one foot, cloak swinging`
- Toplam 24 gen (cost_est 32 pay dahil). Aynalama yok. Silah eklenmez; Johan'ın kılığı 496'daki gibi kalır.

## Ortak işleme (018e ile aynı)
- Yöntem: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` = ilgili yönün durma karesi (`get_upload_url` ile yükle). Yön başına tek çağrı, tek deneme. Bağımsız çağrılar paralel gönderilir. Çağrıdan önce maliyet satırı okunur; `cost_est`'ten çok farklıysa çağrılmaz.
- Tuval 84x84 (PixelLab başka tuval verirse maskeyle kırpma; ölçekleme yok). Ayak satırı sabit (durma karesiyle aynı), pivot x=42, y=2 (ALTTAN).
- Ücretsiz hizalama (018b2): her kare, yere basan ayağın en alt pikseli durma karesinin ayak satırına gelecek şekilde bütün olarak dikey kaydırılır.
- Palet: karakterin durma karelerinin renkleri (≤32), en yakın RGB, alfa ikili. Döngüde 00 ile son kare aynı pozsa son kare atılır.
- Doğrulama: `python tools/art/validate_iso.py --kind object --max-colors 32 <dosya>`, `python tools/art/style.py check --family karakter <dosya>` (KALDI teslim edilmez), klip başına GIF ve yan yana sayfası `art/results/` altına; kimlik kayması (yüz, giysi rengi) ve tepe kırpılması raporda yazılır.

## Doğrulama
Ortak işleme doğrulaması. Johan yürüyüşünde 018b2 sekme ölçütü (kafa farkı ≤ 3 px). `pet` karelerinde gövde belirgin biçimde eğilmeli; `wreck` karelerinde ayak hareketi okunmalı. Köpek ve ekin çizilmez (oyunda ayrı görseller).

## Notlar
- Kod kareleri adla bulur: `IsoArt.SetWalk` (`<takım>_walk2_<yön>_NN`), `IsoArt.SetAction` (`<takım>_<pet|wreck>_<çapraz yön>_NN`; yön yoksa `se`).
- Dönem: yürüyen yolcu ve zırhlı şövalye; at yok, silah yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
