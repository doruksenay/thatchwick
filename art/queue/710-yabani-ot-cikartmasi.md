---
id: "710"
title: Yabani ot çıkartması (iki değişke)
status: accepted
claimed_at: 2026-11-20T18:13Z
approved_by: user
family: zemin
style_gate: false
cost_est: 15
cost_max: 30
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_decal_weeds_a.png
  - Assets/Art/Iso/iso_decal_weeds_b.png
created: 2026-11-20
---
# 710 Yabani ot çıkartması

## Amaç
Belge 59 §8 ve 57g: tarlada yolunmamış yabani ot. Kod hazır: otlu karoya zemin çıkartma katmanında (`WorldView.RefreshDecals`) bu görsel çizilir; görsel yokken ot yalnızca "Otunu yol" etkileşimiyle anlaşılır.

## Tarif
Zemin ailesi stil eşlemesi; tek karoya sığan, kenarları saydam, zemine yatık çıkartma. Ekinin dibinde birkaç dağınık yabani ot tutamı (kısa, düzensiz yeşil, bir iki sarı çiçek); ekinin kendisini kapatmayacak kadar alçak ve karonun kenarlarına doğru. İki değişke farklı dağılımda.

## Doğrulama
`python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family zemin <dosya>`.

## Notlar
Karo ortasına oturur (`IsoArt.Decal`, `GetCentered`). 709 ile aynı katman.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->

Kullanıcı kararı (20.11.2026): `iso_decal_weeds_a` kabul, `iso_decal_weeds_b` ret (ekini kapatacak kadar uzun karahindiba; kenarlar bulanık, stile uymuyor). Kod yalnızca `a`'yı kullanır.
