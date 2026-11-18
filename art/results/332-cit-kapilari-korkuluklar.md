unity-import: pending

# 332 Çit kapıları ve korkuluklar (bulut işçisi, şerit 0, 2026-11-18)

Yan yana sayfası: [332-yanyana.png](332-yanyana.png) (3 referans + 10 yeni)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`; araç açıklamasında 96 px kaynak için 5 gen/çağrı (cost_est ile uyumlu). 10 çağrı = **50 generation**.
- Kaynak: 331'in `iso_wall_fence_wattle_*` parçası (örgü çit), 102x115 tuvalden 96x96 kırpma (sw_ne ofset (0,19), se_nw (6,19)). Maske (betikle): çitin opak bölgesi + her sütunda üstünde 8 px. Görseller `upload:` ile gönderildi. Çıktı aynı ofsetle 102x115 şeffaf tuvale geri konuldu.
- Kapılar için de tarifteki gibi örgü çit kaynak alındı; `iso_wall_door_*` kullanılmadı.
- Eşzamanlılık sınırı (8 iş): torna korkuluk iki çağrısı reddedildi (0 gen), slot açılınca bir kez yeniden denendi.
- `get_balance` sonrası kalan: **567.1** (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (hepsi 102x115; pivot x sw_ne 32, se_nw 70; pivot y ALTTAN 19)
`Assets/Art/Iso/` altında 10 dosya: iso_wall_gate_wood, gate_wicket, railing_wood, railing_turned, railing_iron (her biri _sw_ne ve _se_nw).

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: 10/10 geçti.

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| gate_wicket_se_nw | %100 | 4.9 | x1.10 | GEÇTİ |
| gate_wicket_sw_ne | %100 | 6.0 | x1.03 | GEÇTİ |
| gate_wood_se_nw | %100 | 4.6 | x1.16 | GEÇTİ |
| gate_wood_sw_ne | %100 | 6.0 | x1.06 | GEÇTİ |
| railing_iron_se_nw | %100 | 8.8 | x0.11 | UYARI (soluk, siyah demir) |
| railing_iron_sw_ne | %100 | 7.8 | x0.06 | UYARI (soluk, siyah demir) |
| railing_turned_se_nw | %100 | 4.5 | x1.13 | GEÇTİ |
| railing_turned_sw_ne | %100 | 5.8 | x1.07 | GEÇTİ |
| railing_wood_se_nw | %100 | 5.5 | x1.34 | UYARI (doygun) |
| railing_wood_sw_ne | %100 | 5.6 | x1.09 | GEÇTİ |

## Sorunlar
- Bahçe kapısı ile küçük kapı birbirine yakın görünüyor; payanda ve direk farkı yan yana sayfasından gözle denetlenmeli.
- Korkuluk üst kat içindir; duvar/döşeme dikişi, ışık ve pişmiş gölge gözle denetlenmedi (döşeme testi zemin işi olmadığı için yapılmadı).
- Yardımcı betikler `art/tmp/` altında (commit dışı).
