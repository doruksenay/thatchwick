unity-import: pending

# 447a Korkuluk varyantları A (bulut işçisi, şerit 2, 2026-11-19)

Yan yana sayfası: [447a-yanyana.png](447a-yanyana.png) (3 referans + 2 teslim edilen; kırmızı ve yeşil KALDI, sayfada yok)

## Araç ve ayarlar
- 332 ile aynı yöntem: `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`; araç açıklamasına göre 96 px kaynak için 5 gen/çağrı (cost_est ile uyumlu). 6 çağrı = **30 generation**. Hız sınırı hatası olmadı.
- Kaynak: `iso_wall_fence_wattle_*` (332'de kullanılan örgü çit), 102x115 tuvalden 96x96 kırpma (sw_ne ofset (0,19), se_nw (6,19)). Maske (betikle): çitin opak bölgesi + her sütunda üstünde 8 px. Çıktı aynı ofsetle 102x115 şeffaf tuvale geri konuldu.
- Açıklamalar: `wooden railing painted red/green`, `blackened wooden railing` (+ "simple balcony railing, light from top-left, no baked shadow"). Balüster yok.
- Bir çağrı (se_nw, kırmızı) base64 aktarım hatasıyla reddedildi (0 gen); aynı çağrı yeniden kodlanmış PNG ile bir kez gönderildi.
- `get_balance` sonrası kalan: **3212.9** (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (hepsi 102x115; pivot x sw_ne 32, se_nw 70; pivot y ALTTAN 19)
Teslim edilen (`Assets/Art/Iso/`): `iso_wall_railing_black_sw_ne.png`, `iso_wall_railing_black_se_nw.png`.
Teslim EDİLMEYEN (stil denetimi KALDI): kırmızı ve yeşil, 4 dosya, `art/results/447a-red/` altında.

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: siyah 2/2 geçti (16 ve 23 renk).

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| black_se_nw | %100 | 6.1 | x0.17 | UYARI (soluk, siyah) |
| black_sw_ne | %100 | 6.1 | x0.02 | UYARI (soluk, siyah) |
| green_se_nw | %5 | 29.7 | x1.11 | KALDI (palet dışı) |
| green_sw_ne | %21 | 26.4 | x1.15 | KALDI (palet dışı) |
| red_se_nw | %0 | 41.4 | x2.29 | KALDI (palet dışı, çok doygun) |
| red_sw_ne | %0 | 61.2 | x2.91 | KALDI (palet dışı, çok doygun) |

`style.py fit` ile doygunluk indirildi (kırmızı x0.39/x0.50; yeşil zaten x1.00): kırmızı palete yakınlık %71-89'a çıktı, yeşil değişmedi; dördü de hâlâ KALDI (palet dışı renkler), bu yüzden teslim edilmedi.

## Sorunlar
- Parlak kırmızı ve yeşil boya, yapı ailesi paletinin dışında. Çözüm önerisi (kullanıcı kararı): ailenin paletine sönük boya tonları eklemek ya da tarifi "faded/muted red paint" gibi söndürülmüş tonlarla yeniden yazıp 447a'yı tekrar kuyruğa almak. 447b/447c de aynı sorunu yaşayabilir.
- Siyah korkuluk soluk (UYARI); gölge pişirme ve dikiş gözle denetlenmedi (zemin işi değil, döşeme testi yapılmadı).
- Yardımcı betikler `art/tmp447/` altında (commit dışı bırakılmalı).
