---
id: s233
title: Kepçeyle kaba alma
kind: sfx
family: foley
freq: B
status: claimed
approved_by: user
depends_on: []
note: 'Project belgesi 60: izlenen eylem zinciri adım sesi (life.json chains[].steps[].sfx).'
key: kitchen.ladle
generate: 8
keep: 3
cost_max: 700
pilot_onayi_bekliyordu:
- s110
not_bagimlilik: '20.11.2026: kullanıcı pilotları onaylanmış saydı; pilot kararı sonra verilecek (Project 45b)'
inceleme_notlari:
- tarih: '2026-11-20'
  karar: yeniden
  not: sonlarında bozuk para sesi gibi bir ses var
- tarih: '2026-11-20'
  karar: yeniden
  not: yemek sesi yok, katı bir şey koymak gibi
- tarih: '2026-11-20'
  karar: yeniden
  not: hiç sıvı sesi yok
- tarih: '2026-11-20'
  karar: yeniden
  not: bunlar da aşırı sıvı, kepçe sesi yok
yeniden_sayisi: 4
prompt_influence: 0.7
layers:
- name: ladle
  generate: 6
  duration_s: 0.8
  prompt: A wooden ladle gently knocking the rim of a clay cooking pot and dipping in with a soft wet scoop. Close, cozy kitchen, short.
- name: pour
  generate: 6
  duration_s: 0.8
  prompt: 'A small ladleful of thick stew poured into a clay bowl: one short soft wet glug. Brief, not a long stream, no splashing.'
claimed_by: ip-10-0-138-220
claimed_at: '2026-11-20T18:31:37Z'
---
Kepçe iki katman: kepçenin kazana değip alması ve kâseye kısa dökülüş. Oyunda birlikte çalar (5. tur; 3. tur katı, 4. tur fazla sıvıydı).
