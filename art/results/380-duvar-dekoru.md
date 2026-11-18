unity-import: pending

# 380 Duvar dekoru (rapor): 5/7 teslim, 2 stil denetiminde KALDI, review

Yan yana sayfası: [380-yanyana.png](380-yanyana.png). Teslim edilmeyenler: `art/results/380-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object` stil eşleme (özel maske), `side`, `low detail`, `basic shading`, `single color outline`; tarifteki açıklama + duvar eki + takım eki + 301 ortak eki. 7 çağrı paralel, tek deneme. Çağrı başı 1 gen varsayıldı (araç açıklaması fiyat vermiyor; 306 ile aynı), toplam ~7.
- Kılavuz/maske: "/" eğimli paralelkenar (duvar rengi 184,160,122), betik `art/tmp380/` (commit edilmez). Nazarlık: araç en az 32 px genişlik istediği için 32x40 üretildi, eşya x4-27'ye sığdı, 24x40'a kırpıldı (içerik yeniden boyanmadı).
- Maske dışı piksel: 0 (hepsinde).
- get_balance sonrası: 4803,9 (paralel şeritler de harcıyor).

## Dosyalar (pivot = görsel merkezi, ALTTAN)
| Dosya (`_aday/`) | Boyut | Pivot x | Pivot y | Durum |
|---|---|---|---|---|
| iso_wall_item_peg_rail_sw_ne.png | 64x40 | 32 | 20 | teslim (UYARI) |
| iso_wall_item_plate_rack_2w_sw_ne.png | 96x56 | 48 | 28 | teslim |
| iso_wall_item_long_shelf_iron_2w_sw_ne.png | 96x48 | 48 | 24 | teslim |
| iso_wall_item_flower_wreath_sw_ne.png | 40x40 | 20 | 20 | teslim (UYARI) |
| iso_wall_item_nazar_charm_sw_ne.png | 24x40 | 12 | 20 | teslim (UYARI) |
| iso_wall_item_spoon_rack_sw_ne.png | 40x48 | 20 | 24 | KALDI, `380-red/` |
| iso_wall_item_bronze_mirror_wall_sw_ne.png | 40x48 | 20 | 24 | KALDI, `380-red/` |

## Doğrulama
| Eşya | validate_iso | Renk | Stil denetimi |
|---|---|---|---|
| peg_rail | geçti | 25 | UYARI (%100, x1,49 doygun) |
| plate_rack_2w | geçti | 14 | GEÇTİ (%100, x0,81) |
| long_shelf_iron_2w | geçti | 15 | GEÇTİ (%100, x1,09) |
| flower_wreath | geçti | 12 | UYARI (%95, x1,36 doygun) |
| nazar_charm | geçti | 32 | UYARI (%90, x1,37 doygun) |
| spoon_rack | geçti | 30 | KALDI (x1,51 çok doygun, %88) |
| bronze_mirror_wall | geçti | 7 | KALDI (%59 palet dışı, x1,91) |

## Sorunlar
- Tunç ayna: düz kahverengi/turuncu oval disk, çerçeve ve parlaklık okunmuyor, palet dışı. Tek deneme kullanıldı; yeniden iş gerekir.
- Kaşıklık: doygunluk x1,51. `esya` ailesi kilitli olduğu için `fit` uygulanmadı.
- Çelenk halka olarak okunuyor, çiçek/ot dokusu zayıf; kullanıcı göz kararı.
- Uzun raf ve tabak rafı ince bir çıta gibi; tabak yuvaları seçilmiyor (yan yana sayfasına bakın).
- Işık: "/" yüzü için sol-üstten; ayna "\" yüzünde sağ-üstten.
- Döşeme testi duvar eşyası için geçerli değil.
