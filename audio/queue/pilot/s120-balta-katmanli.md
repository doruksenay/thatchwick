---
id: s120
title: Balta vuruşu, katmanlı (pilot; s121 ile karşılaştır)
kind: sfx
family: tools
freq: A
pilot: true
status: done
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
candidates: 12
passed: 12
credits_spent: 80
generated_at: '2026-11-20T09:53:50Z'
pick:
- impact_c1
- impact_c2
- crack_c1
- crack_c4
- chips_c3
- chips_c4
karar_zamani: '2026-11-20'
promoted_at: '2026-11-20T16:44:14Z'
---
Axe chopping wood, built from three layers that play together.
