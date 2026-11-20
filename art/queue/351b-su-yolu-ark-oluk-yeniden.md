---
id: "351b"
title: "Su yolu karoları yeniden: toprak ark, taş ark, ahşap oluk (kuru ve sulu)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-20: 351 reddedildi, yeniden üretim
priority: 20
family: zemin
style_gate: false
cost_est: 20
cost_max: 40
depends_on: ["340"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_conduit_{ditch,stone,flume}_{dry,wet}_center_v2.png
  - Assets/Art/Iso/_aday/iso_conduit_{ditch,stone,flume}_{dry,wet}_arm_ne_v2.png
  - Assets/Art/Iso/_aday/iso_conduit_{ditch,stone,flume}_{dry,wet}_arm_se_v2.png
created: 2026-11-20
---
# 351b Su yolu karoları yeniden: toprak ark, taş ark, ahşap oluk (kuru ve sulu)

## Neden yeniden
351 kullanıcı tarafından reddedildi (2026-11-20, sohbette toplu inceleme). Reddedilen çıktılar karşılaştırma için `art/results/351-yanyana.png` içinde.

## Tarif
**351 iş dosyasındaki tarifin aynısı** (amaç, ölçüler, yöntem, doğrulama). Yalnızca şu farklar geçerli:

- **Asıl sorun:** su yolu zemine kazılmış ya da zemine oturan bir kanal gibi okunmalı. 351'de toprak ark kütük gibi, ahşap oluk masa (bacaklı) gibi, taş ark ortada yuvarlak bir kapak gibi göründü.
- **Toprak ark:** zemine kazılmış sığ hendek. İçi koyu ve nemli toprak, iki kenarı hafif kabarık toprak sırtı. Kütük ya da çubuk olmamalı.
- **Taş ark:** iki yanı dizili taşlarla örülü açık kanal, tabanı taş. Yuvarlak kapak, menhol ya da daire olmamalı.
- **Ahşap oluk:** zemine yatırılmış, üstü açık U kesitli tahta kanal. Bacak ya da sehpa olmamalı, en çok iki küçük takoz olabilir.
- **Zemin:** karo zemini 002 toprağıyla aynı renkte olmalı; 351'deki turuncu zemin olmamalı.
- **Parçalar:** merkez parça karonun ortasından iki karşı kenara düz geçer. arm_ne ve arm_se merkezden yalnız o kenara uzanır, uçları karo kenarında kesilir ki komşu karoyla birleşsin.
- **Sulu hâller:** kanalın içinde 346 gölet suyunun rengi (`iso_ground_water_a`), ince bir parıltı çizgisiyle.

- Çıktı adları `_v2` ekiyle `_aday/` altına yazılır; var olan dosyaların üstüne yazılmaz. Kabul edilince kod eşlemesi `_v2` dosyalarına çevrilir.
- Raporda 351'in reddedilen çıktısıyla yan yana sayfası (`art/results/351b-yanyana.png`) olmalı.

## Notlar
