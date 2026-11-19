---
id: "350"
title: Soy Mührü ve yadigâr ikonları (arayüz ve dünya işareti)
status: failed
claimed_at: 2026-11-18T13:05Z
approved_by: user        # otomatik akış (2026-11-17); miras sistemi PR #11
family: arayuz
style_gate: false
cost_est: 8
cost_max: 15
depends_on: ["210"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_legacy_{seal,heirloom,descendants,married_out}.png
  - Assets/Art/Iso/_aday/ui_legacy_seal_large.png
  - Assets/Art/Iso/_aday/iso_fx_heirloom_glint.png
created: 2026-11-17
---
# 350 Soy Mührü ve yadigâr ikonları

## Amaç
Miras sistemi (PR #11, Project belgesi 19) için arayüz ikonları ve dünyada yadigâr eşyayı belli eden küçük bir işaret. Bugün arayüzde yerlerinde metin ve "✦" duruyor.

| Dosya | Boyut | Nerede |
|---|---|---|
| `ui_icon_legacy_seal.png` | 16x16 | üst çubukta Soy Mührü sayısı, Soy sekmesindeki defter satırları |
| `ui_icon_legacy_heirloom.png` | 16x16 | yadigâr listesi, yapı modunda "Yadigâr yap" düğmesi |
| `ui_icon_legacy_descendants.png` | 16x16 | Soy sekmesi "Köydeki soy" başlığı |
| `ui_icon_legacy_married_out.png` | 16x16 | Aile panelinde "Evlendir" düğmesi, komşu haneye gidenler listesi |
| `ui_legacy_seal_large.png` | 64x64 | Soy sekmesi başlığı, mühür kazanılınca bildirim kartı |
| `iso_fx_heirloom_glint.png` | 12x12 | dünyada yadigâr eşyanın üstünde süzülen küçük parıltı |

## Tarif
- **Araç:** 212 ve 315 ile aynı yöntem: `create_image_pixflux` ya da `create_map_object`, 5-6 ikonluk tek sayfa, 1 px boşlukla kesilir. 212'nin kesim betiği varsa onu kullan.
- **Stil:** 210 arayüz pilotunun paleti ve kontur kalınlığı; arayüz ailesinin referansları (`art/style/references.json`).
- **Açıklamalar:**
  - Soy Mührü: kırmızı balmumu mühür; içinde saz çatı ve kökleriyle bir ağaç (marka belgesi 17'deki elenen logo adayı, artık oyun içi eşya). 16'lıkta yalnızca siluet ve bordo-kırmızı ton okunmalı; 64'lükte çatı ve kök çizgileri seçilsin.
  - Yadigâr: eski, koyu ahşap bir sandık ya da balta sapı üzerinde küçük altın bir yıldız. Kalıtım hissi: yıpranmış, sıcak.
  - Köydeki soy: üç dallı küçük bir soy ağacı; uçlarda üç küçük yuvarlak (kişiler).
  - Evlendirme: iki iç içe halka ya da iki küçük ev arasında bir kalp ipi; ortaçağ köyüne uygun, parlak değil.
  - Parıltı: 4 kollu, açık altın, ortası beyaz; 12x12, saydam zemin; 2-3 px kalınlıkta kollar. Animasyon değil, tek kare.
- 16x16 ikonlarda en çok 8 renk; 64x64'te en çok 16 renk.

## Doğrulama
- `python tools/art/validate_iso.py` (boyut, saydamlık, renk sayısı).
- `python tools/art/style.py check --family arayuz <dosyalar>`: KALDI teslim edilmez.
- 16'lık ikonlar 2 kat büyütülmüş olarak ve 212'nin moodlet ikonlarıyla yan yana (`style.py sheet`) birbirinden ayırt edilebilmeli.

## Notlar
- Kod tarafı bu dosya adlarını henüz okumuyor; ikonlar teslim edilince arayüz bağlantısı ayrı bir kod işidir (UI Toolkit geçişi, Adım 27, ile birlikte).
- Parıltı `iso_fx_*` adıyla dünya katmanındadır, eşya sprite'ı değildir.
- 2026-11-19 (kod oturumu, kullanıcı kararı "önerdiklerini yap"): yeniden deneme işi 350b yazıldı.
