---
id: "423"
title: Yaşlı erkek 84 (415) elindeki küreği sil, 8 yön
status: pending
approved_by: user        # kullanıcı 19.11.2026 (sohbet): "bunu yap"
priority: 60
family: karakter
style_gate: false
cost_est: 8
cost_max: 16
depends_on: ["415"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_elder_m_r84_idle_<yön>_v2.png
  - art/results/423-yasli-erkek-kurek-sil.md
created: 2026-11-19
---
# 423 Yaşlı erkek: elindeki küreği sil

## Sorun
415'te tarif "holds no tool" dediği hâlde yaşlı erkek 8 yönün hepsinde bir elinde küçük gri kürek (bel/mala) tutuyor. Oyunda yürüme ve etkileşim kareleri alet taşımayacağı için bu el boş olmalı. Kadın (elder_f) temiz, dokunulmaz.

## Tarif
- **Araç:** `inpaint_image_pro_flash` (008b'deki gibi), her yön için küreği ve onu tutan yumruğun çevresini kapsayan maske. Maskeyi elle değil betikle üret: küreğin gri metal pikselleri (açık gri, düşük doygunluk) + 2 px genişletme; el ve kol pikselleri maskenin dışında kalsın.
- **İstem:** "empty relaxed hand hanging at side, no tool, no shovel, same skin tone and sleeve as the rest of the sprite, same pixel art style, black single color outline".
- Gerekirse inpaint için 96x96 kırpma (008b yöntemi), sonra 84x84 tuvale geri yapıştır; ayak tabanı ve pivot (42,2) değişmez.
- 32 renk sınırı ve 415 paleti korunur.
- **Var olan PNG'nin üzerine yazılmaz:** çıktı `_v2` son ekiyle. Kabul edilirse kod oturumu adları değiştirir ya da eskisini kaldırır.

## Doğrulama
- `validate_iso`, `style.py check --family karakter`.
- Önce/sonra sayfası (`423-oncesonra.png`): 8 yön, üst satır 415, alt satır v2, 4 kat.
- Maske dışında piksel değişimi 0 olmalı (fark tablosu yön başına).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
