---
id: "423"
title: Yaşlı erkek 84 (415) elindeki küreği sil, 8 yön
status: pending
claimed_at: 2026-11-19T07:55Z
approved_by: user        # kullanıcı 19.11.2026 (sohbet): "bunu yap"
priority: 60
family: karakter
style_gate: false
cost_est: 40
cost_max: 48   # 19.11: işçi tahmini 40 gen (8 yön inpaint), kullanıcı onayı "bunu yap"
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

## Sorular
- (işçi, 2026-11-19) `inpaint_image_pro_flash` maliyeti kaynak tuvalin uzun kenarına göre: 96 px'e kadar çağrı başına 5 gen. Yön başına 1 çağrı x 8 yön = 40 gen. Bu, `cost_est: 8` ve `cost_max: 16` değerlerinin 5 katı; kurala göre çağrı yapılmadı, harcama 0.
- Seçenekler: (a) `cost_max`'ı 40'a çıkar (60 çalışma tavanının altında, 008b ile aynı yöntem); (b) birkaç yönü tek tuvalde birleştirip çağrı sayısını düşür (kopya/maske betiği gerekir). Önerim (a).
- Not: küreğin gri metali saçla aynı renk ailesinde, maske bel altıyla sınırlanmalı; sap kahverengi olduğundan renkle ayrılamaz, kol/el çevresine kutu maske gerekir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
