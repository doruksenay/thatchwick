unity-import: pending

# 415 Yaşlı karakter 84 px (erkek ve kadın, 8 yön durma)

Durum: review. 16/16 dosya teslim edildi (`Assets/Art/Iso/_aday/anim/`). Yan yana: [415-yanyana.png](415-yanyana.png) (yetişkin erkek ve kadın SE yanında iki yaşlı SE).

## Araç ve ayarlar
- `create_character` v3, humanoid, `high top-down`, siyah tek renk dış çizgi, high detail, size 84, 8 yön (012b ile aynı). Tarifteki yaşlı oran metni, kıyafet ve 012b ortak eki aynen. Araç stil referansı almadığı için kimlik referansı kullanılmadı. Çağrılar paralel gönderildi.
  - elder_m: 8dcaede5-cc4d-4068-b56f-ab5e327fea1c (2 gen)
  - elder_f: b7f31e8d-29f2-4f6a-9092-a02759611159 (2 gen)
- `reduce_colors` 32 renk, dithering none, karakter başına 8 yön tek çağrıda (ortak palet): 2 x 0,1 gen.
- Toplam harcanan: 4,2 gen. `get_balance` sonrası: 4088,7 (paralel şeritlerin harcamaları dahil).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/anim/`, 84x84, pivot x=42, y=2 alttan hedefi)
`iso_char_elder_{m,f}_r84_idle_{e,ne,n,nw,w,sw,s,se}.png`

| Yön | m opak (g x y) | m alt boşluk | f opak (g x y) | f alt boşluk |
|---|---|---|---|---|
| e | 29x76 | 3 | 35x78 | 3 |
| ne | 52x77 | 4 | 39x78 | 4 |
| n | 57x76 | 5 | 50x77 | 5 |
| nw | 41x77 | 4 | 39x78 | 4 |
| w | 34x76 | 3 | 35x78 | 3 |
| sw | 53x77 | 3 | 43x78 | 2 |
| s | 56x76 | 4 | 51x78 | 3 |
| se | 44x78 | 2 | 43x78 | 2 |

Tüm dosyalar: yarı saydam piksel 0, renk 31-32.

## Doğrulama
- `validate_iso`: 16/16 geçti.
- `style.py check --family karakter`: 13 GEÇTİ, 3 UYARI (f n/ne/nw "ailesinden soluk", x0.48-0.58; kuzey yönleri). KALDI yok. Aile kilidi kurulmadı; palet uyumu yalnızca uyarı.
- Opak boy oranı (SE): yaşlı erkek 78 / yetişkin erkek 78 = %100; yaşlı kadın 78 / yetişkin kadın 80 = %97,5. Hedef %92-97: erkek tutmadı (kambur boyu kısaltmadı), kadın sınırda.

## Sorunlar
- Alt boşluk yönlere göre 2-5 px; tarifteki "ayaklar alttan 2 px" kuralı için kareler yeniden konumlandırılmadı (içerik kaydırılmadı). Unity tarafında pivot ayak tabanına göre ayarlanmalı ya da ayrı bir hizalama işi gerekir.
- Yaşlı erkek boyu yetişkinle aynı; hafif kambur görünüm sınırlı. Kullanıcı yan yana sayfasına bakıp karar verir.
- Yürüme kareleri bu işte yok (018a2 sonrası).
