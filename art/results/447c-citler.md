unity-import: pending

# 447c Çit varyantları (bulut işçisi, şerit 1, 2026-11-19)

Yan yana sayfası: [447c-yanyana.png](447c-yanyana.png) (3 referans + 6 yeni; sırayla cob, iron, wattle_hurdle; her biri se_nw ve sw_ne)

## Araç ve ayarlar
- 331 ile aynı yöntem: `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`; 96 px kaynak için 5 gen/çağrı, 6 çağrı paralel = **30 generation** (cost_est 30). Hız sınırı hatası olmadı.
- Kaynak: `iso_wall_fence_wattle_*` 96x96 kırpma (sw_ne ofset (0,19), se_nw (6,19)); maske = çitin opak bölgesi + üstünde 8 px. Çıktı aynı ofsetle 102x115 şeffaf tuvale geri konuldu. Görseller `upload:` ile gönderildi.
- Açıklamalar tarifteki gibi; sonuna "muted tones, light from top-left, no baked shadow" eklendi (447a'daki parlak boya sorunu nedeniyle).
- `get_balance` sonrası kalan: **3122.9** (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (hepsi 102x115; pivot x sw_ne 32, se_nw 70; pivot y ALTTAN 19)
`iso_wall_fence_{cob,wattle_hurdle,iron}_{sw_ne,se_nw}.png` (6 dosya, `Assets/Art/Iso/`).

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: altısı geçti (renk 13-26).

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| cob_se_nw | %100 | 5.6 | x1.06 | GEÇTİ |
| cob_sw_ne | %100 | 4.8 | x1.23 | GEÇTİ |
| iron_se_nw | %100 | 4.7 | x0.41 | UYARI (soluk) |
| iron_sw_ne | %100 | 6.1 | x0.45 | UYARI (soluk) |
| wattle_hurdle_se_nw | %100 | 4.3 | x1.07 | GEÇTİ |
| wattle_hurdle_sw_ne | %100 | 5.9 | x1.05 | GEÇTİ |

## Sorunlar
- Cob çiti ahşap tahta görünümüne yakın (kaynak çit etkisi); sazlı üst var ama kerpiç doku zayıf. Kullanıcı gözle karar vermeli.
- wattle_hurdle, 331'in wattle çitine çok benziyor (aynı kaynak); varyant farkı az.
- Demir çit ince ve soluk (UYARI), ama tanınıyor.
- Döşeme testi yapılmadı (zemin işi değil); gölge pişmemiş, ışık sol-üstten gözle uygun.
