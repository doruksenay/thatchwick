---
id: "574"
title: "Peştamallı gövde pilotu: erkek ve kadın, 8 yön (stil kapısı)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 40
family: karakter
style_gate: true
cost_est: 16
cost_max: 32
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_char_{man,woman}_r84_pestamal_idle_{s,se,e,ne,n,nw,w,sw}.png
  - art/results/574-*.md
created: 2026-11-20
---
# 574 Peştamallı gövde pilotu: erkek ve kadın, 8 yön (stil kapısı)

## Amaç
Yıkanma zincirinde (Project belgeleri 60 §2.3, 60a karar Z1) soyunan karakter **peştamal** ile görünür; çıplak gövde hiç çizilmez. Bu iş stil pilotudur: 575 (yıkanma pozları) buna bağlı.

## Tarif
- `man_r84` (`c63fe39d-b168-4732-bf11-b476172c98e8`) ve `woman_r84` (`135cb0b1-bd46-4514-8c87-0834b96ef80d`) gövdelerinin peştamallı sürümü, 8 yön durma karesi.
- Peştamal: ince çizgili dokuma pamuklu örtü, krem zemin üstüne bordo ya da çivit mavisi çizgi. Erkekte belden dize; kadında koltuk altından dize (göğsü örter). Omuzlar, kollar ve baldırlar çıplak; ayaklar yalın.
- Yöntem: 500 (pelerin pilotu) ve 501 (kesim maskesi) ile aynı giysi katmanı yolu denenir; katman olarak ayrılamıyorsa gövdeyle birlikte tek karakter olarak üretilir ve rapora yazılır.
- Saç ve ten 012b paletinden; yüz ve vücut oranı durma karesiyle aynı kalmalı.
- Açıklama eki: `, wrapped in a striped cotton bath towel (peshtemal), modest, no nudity, bare shoulders and lower legs only`.

## Doğrulama
- 018a ölçütleri; 8 yönde peştamalın boyu ve çizgisi tutarlı.
- Mahremiyet: göğüs, kalça ve kasık her yönde örtülü. Tutmayan kare teslim edilmez.
- Yan yana sayfa: giyinik durma karesi ile peştamallı kare, 8 yön (`art/results/574-yanyana.png`).

## Notlar
Kod: `JobRunner.IsUndressed` adım bayrağından gelir; görünüm bu kareler kabul edilince bağlanır (kod oturumu).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
