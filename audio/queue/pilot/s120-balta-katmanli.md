---
id: s120
title: Balta vuruşu, katmanlı (pilot; s121 ile karşılaştır)
kind: sfx
family: tools
freq: A
pilot: true
status: claimed
approved_by: auto
key: work.chop
note: '45a bölüm 6: katmanlı ile tek dosya karşılaştırması. İkisinden yalnız biri kabul edilir; diğeri ret.'
cost_max: 600
layers:
- name: impact
  prompt: The dull thud of an iron axe blade biting into a log, only the impact.
  duration_s: 0.5
  generate: 4
- name: crack
  prompt: Short dry wood crack and creak of wood fibres splitting, no impact.
  duration_s: 0.7
  generate: 4
  axes:
    wood:
    - seasoned oak
    - pine with knots
    - dry birch
    - wet log
- name: chips
  prompt: A few small wood chips and splinters falling onto grass and earth.
  duration_s: 0.8
  generate: 4
claimed_by: runnervm0hi8g
claimed_at: '2026-11-20T09:53:33Z'
---
Axe chopping wood, built from three layers that play together.
