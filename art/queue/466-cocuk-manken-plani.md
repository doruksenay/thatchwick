---
id: "466"
title: "Çocuk mankeni ve çocuk saç modülleri planı (iş dosyalarını yazar)"
status: pending
approved_by: user        # plan işi, gen harcamaz; 461b review olunca çalışır
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["460a", "460b", "461b"]
max_attempts: 1
pixellab: false
outputs:
  - art/queue/466a-....md (ve devamı)
  - art/results/466-cocuk-manken-plani.md
created: 2026-11-20
---
# 466 Çocuk mankeni ve çocuk saç modülleri planı

## Amaç
Kullanıcı kararı (19.11.2026, belge 43 §7.2): çocuk ayrı küçük set. Bebek katmansız kalır; ergen ve yaşlı yetişkin mankenini kullanır. Bu plan çocuk için iş dosyalarını yazar (belge 43b §3: 8 kütle, 6 ön, 6 eklenti).

## Tarif
1. **Çocuk mankeni:** kız ve erkek (`cm`, `cf`), bugünkü çocuk tabanının (64 px, `ChildR64`) oran ve tuvaliyle, 460a'nın anahtar renk tarifiyle.
   - Beden yalnız İnce/Orta; İnce betikle türetilir.
   - Animasyonlar: `idle`, `walk2` ve çocuk etkileşimleri (oyun, oyuncak).
2. **Çocuk saç modülleri:** 461b yöntemiyle, çocuk mankeni üzerinde. Kimlikler yetişkin kimlikleri; dosya adı `lay_<yuva>_<kimlik>_c_<yön>`.
   - Kütle (8): crop, short, curls, ear_length, nape, shoulder, shoulder_straight, long.
   - Ön (6): straight, side, wispy, curly, parted, side_part.
   - Eklenti (6): ponytail, low_tail, braid, twin_braids, bun, twin_buns.
   - Kodun `_c_` önekini okuması ayrı iş; plan raporuna not düşülür.
3. **Bölme:** her iş en çok `cost_max: 60`, `approved_by: user`. İş kimlikleri 466a, 466b, ...
4. **Rapor:** `art/results/466-cocuk-manken-plani.md`.

## Doğrulama
- `queue_ready.py --all` okuyor; kimlik çakışması yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
