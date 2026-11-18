---
id: "351"
title: Su yolu karoları: toprak ark, taş ark, ahşap oluk (kuru ve sulu)
status: running
claimed_at: 2026-11-18T11:56Z
approved_by: user        # otomatik akış (2026-11-17); su şebekesi PR (feat/su-sebekesi)
priority: 20
family: zemin
style_gate: false
cost_est: 20
cost_max: 40
depends_on: ["340"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_conduit_{ditch,stone,flume}_{dry,wet}_center.png
  - Assets/Art/Iso/_aday/iso_conduit_{ditch,stone,flume}_{dry,wet}_arm_ne.png
  - Assets/Art/Iso/_aday/iso_conduit_{ditch,stone,flume}_{dry,wet}_arm_se.png
created: 2026-11-17
---
# 351 Su yolu karoları: ark ve oluk

## Amaç
Yeni su yolu katmanı (Project belgesi 20): göletten ve dereden her yöne **ark** kazılır, suyu tarlaya, su çarkına, sarnıca ve eve götürür. Açık su yolları her zaman görünür; kışın donar (motor buz rengini kendisi karıştırır, ayrı buz görseli gerekmez). Bugün motor bunları renkli elmasla çiziyor.

- **Toprak ark** (ditch): kazılmış dar hendek, çamurlu kenarlar.
- **Taş ark** (stone): iki yanı taş döşeli dar kanal.
- **Ahşap oluk** (flume): ayaklar (kısa sehpa) üstünde yükseltilmiş tahta oluk; yolun üstünden geçer.

## Tarif
- Yöntem **340 ile aynı** (`create_tiles_pro`, stil modu, `style_images` = kabul edilmiş zemin/yol karoları), 64x32 elmas, gölgesiz. Çağrıdan önce maliyeti yaz.
- Parçalı çizim (otomatik birleşim): her tür için 3 parça × 2 durum.
  - `center`: karonun ortasında küçük yuvarlak çukur/havuzcuk (hiç komşusu yoksa tek başına görünür).
  - `arm_ne`: ortadan karonun kuzeydoğu kenarının ortasına uzanan yarım kol; `arm_se`: güneydoğuya. Motor NW ve SW kollarını bunların yatay aynası olarak çizer; komşu karodaki kolla kenar ortasında dikişsiz birleşmeli.
  - `dry`: su yok (ark kuru toprak/taş; oluk boş tahta). `wet`: dar akan su şeridi, açık mavi parıltı (gölet suyuyla aynı ton).
- Parçalar saydam zemin üstünde; alttaki çim/toprak karosu motor tarafından ayrıca çizilir.

## Doğrulama
`validate_iso.py`, `style.py check --family zemin`; 3x3 birleşim testi: düz hat, L dönüş, T ve artı kavşak (merkez + kollar yan yana) dikişsiz mi; kuru ve sulu yan yana ayırt ediliyor mu.

## Notlar
Motor bağlantısı `WorldView` su yolu çiziminde (şimdilik renk); kabulden sonra kod oturumu parçaları bağlar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
