---
id: "021"
title: Portre parça seti planı (iş dosyalarını yazar)
status: review
claimed_at: 2026-11-18T08:03Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["020"]
max_attempts: 1
pixellab: false
outputs:
  - art/queue/021a-*.md ... art/queue/021z-*.md
  - art/results/021-portre-parca-seti-plani.md
created: 2026-11-17
---
# 021 Portre parça seti planı

## Amaç
Plan işi: PixelLab çağrılmaz. 020 portre pilotunun yöntemiyle karakter yaratmanın portre parçaları için **iş dosyası yazar** (proje dokümanı 14, bölüm 2, 3D-3F ve 6).

## Tarif
1. `art/results/020-*.md` raporunu oku: katman yöntemi tuttu mu, parça başına gen, maske kuralları.
2. **Yöntemin tutup tutmamasına göre:**
   - **Tuttuysa:** aşağıdaki listeyi `021a`, `021b`, ... işlerine böl.
     - `approved_by: user`, `family: karakter`, `depends_on: ["020"]`.
     - Her iş `cost_max <= 60`. 5 gen/parça ile iş başına en çok ~11 parça.
     - Yüz tabanına bağlı parçalar tabanların işine bağlanır.
   - **Tutmadıysa:** tam portre yöntemiyle (A) "hazır yüz" seti yaz: 5 yüz şekli × 4 yaş = 20 taban. Nedenini rapora yaz.

**Liste**
| Grup | Adet | Not |
|---|---|---|
| Yüz şekli tabanı | 5 (oval, yuvarlak, köşeli, uzun, kalp) × 4 yaş evresi (çocuk, genç, yetişkin, yaşlı) | Kel, nötr ifade, yaka sade. Ten gri tonlu çizilir, rampa takasıyla boyanır. |
| Göz şekli | 8 | Göz akı ve iris ayrı maske (iris renklenir). |
| Kaş | 6 | |
| Burun | 6 | |
| Ağız | 6 | |
| Kulak | 3 | |
| Saç (ön + arka) | 20 | 017'deki stillerle aynı adlar. |
| Kâkül | 6 | |
| Bıyık / sakal | 5 / 6 | |
| Yara izi | 8 bölge × 4 desen (yarık, çizik, yanık, ısırık), yalnızca yüz ve boyun bölgeleri | Doküman 14 bölüm 3F. |
| Dövme | 8 desen (Kelt düğümü, çivit sarmalı, rün, haç, kuş, yılan, başak, hanedan arması) × yüz/boyun | Kol dövmeleri dünya sprite'ında betikle çizilir. |
| Takı | küpe 4, burun halkası 1, ip kolye 1 | |
| Makyaj | sürme, allık, dudak boyası, kına | Gri tonlu, opaklıkla boyanır. |
| Kırışıklık, eksik diş, sargı | 3 | Yaşla ya da olayla kendiliğinden gelir. |

3. **Rapor:** `art/results/021-portre-parca-seti-plani.md`. İş tablosu ve toplam gen. Ay bütçesini aşan kısım frenlerle sonraki döneme kalır.

## Doğrulama
- `queue_ready.py --all` yeni işleri ayrıştırıyor mu?
- `cost_max <= 60`.
- Numara çakışması yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
