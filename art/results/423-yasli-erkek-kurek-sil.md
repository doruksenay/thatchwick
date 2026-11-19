unity-import: pending

# 423 Yaşlı erkek: elindeki küreği sil

Sonuç: 8 yönün 7'sinde kürek silindi ve teslim edildi; **se** yönünde kürek kaldı, teslim edilmedi (`art/423-red/se_kurek_kaldi.png`). Tek deneme kuralı gereği yeniden denenmedi.

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`; 5 gen/çağrı x 8 yön = **40 gen** (`cost_max` 48 içinde, çağrı öncesi maliyet okundu: 5).
- Kaynak 84x84 tuval 96x96'ya ortalanmadan (6,6) ofsetle genişletildi (96 px = 5 gen eşiği), çıktı aynı ofsetle 84x84'e kırpıldı. Yüklemeler `get_upload_url` ile (base64 yok). 8 çağrı paralel gönderildi.
- Maske: elle belirlenmiş kutu (kürek + sap + tutan el çevresi; betikle gri piksel maskesi yerine kutu, çünkü sap kahverengi). Kutular (x0,y0,x1,y1): e 38,62,58,82; n 55,52,75,74; ne 54,54,73,77; nw 52,52,66,68; s 10,56,28,82; se 26,62,42,84; sw 10,50,28,74; w 21,52,36,68.
- İstem: tarifteki gibi ("empty relaxed hand hanging at side, no tool, no shovel, ...").
- Palet: inpaint 35-42 renk üretti (sınır 32). Maske içindeki yeni renkler, orijinal 415 paletindeki en yakın renge eşlendi (deterministik, yapay zekâ dışı; yön başına 11-75 piksel). Sonuç 31-32 renk.

## Harcama
8 x 5 = 40 gen. `get_balance` sonrası kalan: 3616,5.

## Dosyalar (`Assets/Art/Iso/_aday/anim/`, 84x84, pivot x 42, pivot y 2 alttan, değişmedi)
| Dosya | Boyut | Pivot |
|---|---|---|
| iso_char_elder_m_r84_idle_e_v2.png | 84x84 | (42,2) |
| iso_char_elder_m_r84_idle_n_v2.png | 84x84 | (42,2) |
| iso_char_elder_m_r84_idle_ne_v2.png | 84x84 | (42,2) |
| iso_char_elder_m_r84_idle_nw_v2.png | 84x84 | (42,2) |
| iso_char_elder_m_r84_idle_s_v2.png | 84x84 | (42,2) |
| iso_char_elder_m_r84_idle_sw_v2.png | 84x84 | (42,2) |
| iso_char_elder_m_r84_idle_w_v2.png | 84x84 | (42,2) |
| se | teslim edilmedi | |

## Doğrulama
- `validate_iso --kind object --max-colors 32`: 7/7 geçti (snap sonrası). Yarı saydam piksel 0.
- Maske dışı piksel değişimi (ham çıktıda, snap öncesi): her yönde 0 (maske içi değişim dışında hiçbir bayt oynamadı).
- `style.py check --family karakter`: 7 GEÇTİ, KALDI/UYARI yok (kilit kurulmadı; palet uyumu yalnızca uyarı).

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| e | %93 | 8.2 | x0.71 | GEÇTİ |
| n | %96 | 6.6 | x0.68 | GEÇTİ |
| ne | %94 | 7.0 | x0.69 | GEÇTİ |
| nw | %94 | 7.1 | x0.70 | GEÇTİ |
| s | %95 | 7.0 | x0.78 | GEÇTİ |
| sw | %95 | 7.3 | x0.80 | GEÇTİ |
| w | %93 | 8.0 | x0.68 | GEÇTİ |

- Yan yana sayfası: `art/results/423-yanyana.png`. Önce/sonra (4 kat; satır 1 ve 3 = 415, satır 2 ve 4 = v2, sıra e n ne nw / s se sw w): `art/results/423-oncesonra.png`. Not: se için gösterilen v2 karesi kürekli reddedilen çıktıdır.

## Sorunlar
- **se**: kutu maske (26,62,42,84) küreğin bıçağını tam kapsamadı ya da model onu yeniden çizdi; kürek kaldı. Öneri: ayrı küçük iş (5 gen), maske kutusunu aşağı/sağa genişlet (y 60-84, x 24-46) ve sınırlı alt boşluk nedeniyle tuvali alttan genişlet.
- Elleri çevreleyen kutular bazı yönlerde bot/bacak/el piksellerini de yeniden çizdi; görsel kontrol kullanıcıda (önce/sonra sayfası).
- Eski dosyalar değişmedi; kabulde adlar değiştirilir (`_v2` kaldırılır) ya da eskisi kaldırılır.
- Masaüstü kopyası: yerel.
