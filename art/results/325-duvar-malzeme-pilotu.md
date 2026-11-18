unity-import: pending

# 325 Duvar malzemesi pilotu: kalas ve taş (bulut işçisi, şerit 1, 2026-11-18)

Yan yana sayfası: [325-yanyana.png](325-yanyana.png)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `output_method="Modify current layer"`, `no_background=true`, `seed=0` (008b yöntemi). 4 çağrı x 5 = **20 generation**; `get_balance` sonrası kalan **831,1** (paralel şeritler dahil). Tek deneme.
- Kaynak: `iso_wall_wall_sw_ne/se_nw.png`; 96x96 pencere sw_ne (0,4), se_nw (6,4); çıktı aynı ofsetle 102x115'e geri konuldu.
- Maske (betikle üretildi): ön yüz opak pikselleri; sol/sağ 3 px çerçeve ve üst kapak (üst kenar + 3 px) dışarıda. Maske dışı piksel değişimi 0.
- Yükleme: base64 aktarımında bozulma oldu (ilk kalas se_nw ve taş denemeleri "decode" hatası verdi, harcama yok), kaynaklar `get_upload_url` ile (`tools/art/upload_ref.py`) yüklendi. Hız sınırı hatası yok.
- Masaüstü kopyası: yerel (bulutta yapılmadı).

## Dosyalar
| Dosya | Boyut | Pivot (x, y ALTTAN) | Renk |
|---|---|---|---|
| Assets/Art/Iso/iso_wall_wall_plank_sw_ne.png | 102x115 | 32, 19 | 23 |
| Assets/Art/Iso/iso_wall_wall_plank_se_nw.png | 102x115 | 70, 19 | 33 |
| Assets/Art/Iso/iso_wall_wall_stone_sw_ne.png | 102x115 | 32, 19 | 23 |
| Assets/Art/Iso/iso_wall_wall_stone_se_nw.png | 102x115 | 70, 19 | 33 |

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: dördü geçti (yarı saydam 0). `style.py check --family yapi`:

| Dosya | Palete yakın | Ortalama uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| plank_sw_ne | %100 | 3.3 | x0.83 | GEÇTİ |
| plank_se_nw | %100 | 4.4 | x0.96 | GEÇTİ |
| stone_sw_ne | %100 | 3.3 | x0.89 | GEÇTİ |
| stone_se_nw | %100 | 6.0 | x0.75 | GEÇTİ |

## Sorunlar / görsel onay için notlar
- Kalas: iki eksen tutarlı yatay kalas ve çivi; taş: iki eksen moloz taş. Taş sw_ne'de kaynak çaprazın (koyu ahşap) bir kısmı kalmış, taş se_nw'de çapraz tamamen silinmiş: iki eksen arası tutarsızlık ⚠.
- Kalas ve taşın ışığı ve pişmiş gölge ile dikiş (varsayılan duvarla yan yana) yalnızca yan yana sayfadan gözle bakıldı; kenar çerçevesi (3 px) eski krem renkte kaldığı için varsayılan duvara dikiş yok ama malzemeyle renk farkı var ⚠. Kullanıcı onayına bırakıldı.
