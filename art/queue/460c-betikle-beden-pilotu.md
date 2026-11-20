---
id: "460c"
title: "P1b betikle beden: Orta mankenden İnce, Gürbüz, Dolgun (0 gen)"
status: pending
approved_by: user        # gen harcamaz; 460a/460b review olunca çalışabilir
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["460a", "460b"]
max_attempts: 1
pixellab: false
outputs:
  - Assets/Art/Iso/katman/lay_body_{m,f}_{ince,gurbuz,dolgun}_{idle,walk2}_<yön>_<NN>.png
  - art/results/460c-betikle-beden-pilotu.md
created: 2026-11-20
---
# 460c P1b betikle beden

## Amaç
Kullanıcı kararı (19.11.2026, belge 43 §7.1): beden yapısı (İnce, Orta, Gürbüz, Dolgun) dünyada da görünür. Önce en ucuz yol B1 denenir: yalnız Orta çizilir, diğer üçü bölge haritasından betikle türetilir (0 gen). Bu iş, kararın maliyetini belirler: B1 tutarsa ×1, tutmazsa B2 (×2) ya da B3 (×4).

## Tarif
- Her Orta kare için: `python tools/art/manken.py build Assets/Art/Iso/katman/lay_body_<m|f>_orta_*.png --build <ince|gurbuz|dolgun> --out-dir Assets/Art/Iso/katman`
- Önizleme: aynı yön ve kare için dört beden yan yana, `manken.py preview` ile; G, GD, D yönleri ve yürümenin 00, 02, 04, 06 kareleri.

## Doğrulama
- Dört beden yan yana bakınca ayırt ediliyor; gövde kenarı ve kontur kırık değil; yürürken genişlik titremiyor.
- Baş satırları değişmiyor (çapa aynı kalıyor): `manken.py anchors` her bedende Orta ile aynı baş çapasını verir.
- Rapor: kullanıcıya "B1 yeterli mi" sorusu; değilse önerilen yol (B2: Dolgun manken çizilir).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
