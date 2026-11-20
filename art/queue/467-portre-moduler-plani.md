---
id: "467"
title: "Portre parçaları 43b planı (iş dosyalarını yazar)"
status: pending
approved_by: user        # plan işi, gen harcamaz; 463 review olunca çalışır
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["463"]
max_attempts: 1
pixellab: false
outputs:
  - art/queue/467a-....md (ve devamı)
  - art/results/467-portre-moduler-plani.md
created: 2026-11-20
---
# 467 Portre parçaları 43b planı

## Amaç
Belge 43b §3'ün portre kısmı. 021 serisi (64 px, review) 14'ün eski sayılarıyla ve tam saç stilleriyle üretildi. Yeni sayılar: göz 20, kaş 16, kirpik 4, yüz 8, burun 10, ağız 10, kulak 5. Saç portrede de modüler: kütle, ön, eklenti, süs. Sakal: bıyık, sakal, favori. Başlık 16.

## Tarif
1. **Boyut:** 463'ün kararı (64 ya da 96).
   - 64 seçildiyse 021'in kabul edilen parçaları yeniden kullanılır; yalnız eksikler yazılır.
   - 96 seçildiyse hepsi yeniden.
2. **Eksik listesi:** `appearance.json` kimlikleri ile `Assets/Art/Iso/_aday/portrait_layer_*` karşılaştırılır. 021'deki tam stil saçlar (`hair_ponytail` vb.) modüllere bölünemiyorsa yeniden üretilir.
3. **Adlar** (kod tarafı henüz yok, ad bu planla sabitlenir):
   - parçalar: `portrait_<boyut>_<yuva>_<kimlik>.png` (yuva: face, eyes, brows, lashes, nose, mouth, ears, mass, front, extra, orn, must, beard, side, hat);
   - arka parça: `_bk` eki.
4. **Renk:** saç ve sakal referans saç rampasında, başlık ve süs gri rampada (`tools/art/ramps.py` REFERENCE); ten 020/021 gibi.
5. **Bölme:** her iş en çok `cost_max: 60`, `approved_by: user`. İş kimlikleri 467a, 467b, ...
6. **Rapor:** `art/results/467-portre-moduler-plani.md`: eksik tablosu, yeniden kullanılanlar, toplam gen.

## Doğrulama
- `queue_ready.py --all` okuyor; kimlik çakışması yok.
- Her `appearance.json` kimliğinin portre parçası ya bir işte ya da yeniden kullanılanlar listesinde.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

- 2026-11-20 (kullanıcı kararı, sohbette toplu inceleme): 021a–021k kabul edildi, yeniden kullanılır. 021l–021r (yara izi, dövme, makyaj, kırışıklık, sargı) reddedildi: kaba ve bloklu. Bu plan onları eksik sayıp yeniden yazmalı.
