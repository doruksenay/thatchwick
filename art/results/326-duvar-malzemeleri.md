unity-import: pending

# 326 Duvar malzemeleri: kerpiç, ahşap çatkı, kesme taş (bulut işçisi, şerit 1, 2026-11-18)

Yan yana sayfası: [326-yanyana.png](326-yanyana.png) (ilk üç görsel referans; sonra varsayılan duvar sw_ne/se_nw, ardından yeni 6 dosya)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0` (325 yöntemi). 6 çağrı x 5 = **30 generation**, aynı anda gönderildi. `get_balance` sonrası kalan **791,1** (paralel şeritler dahil). Tek deneme, hız sınırı hatası yok.
- Kaynak `iso_wall_wall_sw_ne/se_nw.png`, 96x96 pencere sw_ne (0,4), se_nw (6,4); 325 ile aynı maske (ön yüz, 3 px çerçeve ve üst kapak dışarıda); çıktı aynı ofsetle 102x115'e geri konuldu, maske dışı baytlar kaynaktan.
- Yükleme `get_upload_url` ile (Python urllib). Masaüstü kopyası: yerel.

## Dosyalar
| Dosya | Boyut | Pivot (x, y ALTTAN) |
|---|---|---|
| iso_wall_wall_cob_sw_ne.png | 102x115 | 32, 19 |
| iso_wall_wall_cob_se_nw.png | 102x115 | 70, 19 |
| iso_wall_wall_timber_sw_ne.png | 102x115 | 32, 19 |
| iso_wall_wall_timber_se_nw.png | 102x115 | 70, 19 |
| iso_wall_wall_ashlar_sw_ne.png | 102x115 | 32, 19 |
| iso_wall_wall_ashlar_se_nw.png | 102x115 | 70, 19 |

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: altısı geçti. `style.py check --family yapi`:

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| cob_sw_ne | %100 | 6.0 | x1.27 | UYARI (ailesinden doygun) |
| cob_se_nw | %100 | 5.0 | x1.16 | GEÇTİ |
| timber_sw_ne | %100 | 4.3 | x0.88 | GEÇTİ |
| timber_se_nw | %100 | 2.4 | x1.11 | GEÇTİ |
| ashlar_sw_ne | %100 | 11.7 | x0.11 | UYARI (palete uzak, soluk) |
| ashlar_se_nw | %100 | 7.8 | x0.11 | UYARI (soluk) |

## Sorunlar / görsel onay notları
- Kesme taş açık gri, neredeyse doygunsuz: stil uyarısı; tarif "light grey" dediği için bırakıldı.
- Ahşap çatkı kaynağın çatkısına çok yakın (kaynak zaten çatkılı); malzeme farkı az.
- Kerpiç sw_ne turuncuya yakın doygun.
- Kenar çerçevesi (3 px) kaynak renkte kaldığı için malzemeyle renk farkı olabilir (325'teki gibi). Dikiş ve ışık yalnızca yan yana sayfadan gözle bakıldı; döşeme testi duvar için uygulanmadı.
