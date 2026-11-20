unity-import: pending

# 832 Erkek güneybatı yoğurma, ikinci deneme: teslim (review)

- Araç: `animate_character`, `mode=pixminimax` + `direction_prompts` (tarifteki metin), `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = man_r84 south-west rotasyonu, yön `south-west`. Karakter `c63fe39d-b168-4732-bf11-b476172c98e8`, grup `545da334-7c56-4d89-af63-c8cc66cd0a53`, job `69ce62c2-5852-4ff2-9f2e-db610978e119`, animasyon `knead_sw2`. Maliyet 1 gen (tahmin 3, tavan 6). İkinci yol (v3) gerekmedi.
- `get_balance` sonrası: 9849 (Tier 3, plan uyumlu; diğer şeritler de harcıyor).
- İşleme: ham 84x112, 9 kare; yalnızca kaydırma (x sabit, kare 0'ın merkezine göre; ayak satırı y=81), palet `iso_char_man_r84_cand_se` 32 renk, alfa ikili, kırpılan piksel 0. Kare 0-8 IoU 0,999 olduğundan son kare atıldı: 8 kare (00-07). Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: `art/results/832-yanyana.png`. GIF (4 yön): `art/results/832-man-knead.gif`.
- masaüstü kopyası: yerel

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_man_r84_knead_sw_00..07.png`, 84x84, pivot (42, 2 alttan).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (8) | 8/8 geçti |
| style check | GEÇTİ 1 (00), UYARI 7 ("ailesinden doygun", x1,33-1,36; palet yakınlığı %100), KALDI 0 |
| Hareket (kare 0'dan farklı piksel, kare 1-7) | 1076, 1384, 1564, 1567, 1569, 1359, 1036 (ölçüt: bir kare ≥60, üç kare ≥30: GEÇTİ). Kare 8 (atıldı): 465 |
| Ayak satırı | y=81 |

## Notlar
- Hareket gözle kontrol edildi: kollar öne gelip katlanıyor, gövde sallanıyor; 796'daki donuk klip değil. "Dirsekler gövdeye yakın" kısmı tam tutmuyor olabilir (GIF'ten bakılmalı). Kimlik tutarlı.
- Hamur/tekne görünmüyor, el konumları ölçülmedi.
