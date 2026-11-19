unity-import: pending

# 441 Sergi rafı, sergi dolabı, giysi sırığı, kurdele (rapor): 2/4 teslim, 2 stil denetiminde KALDI, review

Yan yana sayfası: [441-yanyana.png](441-yanyana.png). Teslim edilmeyenler: `art/results/441-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object` stil eşleme (özel maske), `low detail`, `basic shading`, `single color outline`; duvar eşyaları `side`, dolap `high top-down`. 4 çağrı paralel, tek deneme, çağrı başı ~1 gen (toplam ~4; araç açıklaması fiyat vermiyor).
- Kılavuz: duvar eşyaları için "/" eğimli paralelkenar (rengi 184,160,122; 64 px aralık, 24 px kalınlık, 96x56 / 40x40), dolap için `footprint_guide.py --w 80 --h 104 --n 1 --height 64`. Betikler `art/tmp441/` (commit edilmez).
- Ek metin: 306/301 ortak eki; duvar eşyalarına duvar eki, raf ve sırığa Zanaatkâr takım eki.
- Maske dışı içerik yok. get_balance sonrası: 3442,9 (paralel şeritler de harcıyor).

## Dosyalar (pivot ALTTAN)
| Dosya | Boyut | Pivot x | Pivot y | Durum |
|---|---|---|---|---|
| iso_wall_item_display_shelf_2w_sw_ne.png | 96x56 | 48 | 28 | teslim (UYARI) |
| iso_item_display_cabinet_se.png | 80x104 | 40 | 24 | teslim |
| iso_wall_item_clothes_perch_2w_sw_ne.png | 96x56 | 48 | 28 | KALDI, `441-red/` |
| iso_wall_item_home_ribbon_sw_ne.png | 40x40 | 20 | 20 | KALDI, `441-red/` |

## Doğrulama
| Eşya | validate_iso | Renk | Stil denetimi |
|---|---|---|---|
| display_shelf_2w | geçti | 25 | UYARI (%100, x1,26 doygun) |
| display_cabinet | geçti (ayak izi 8,64,64,32) | 18 | GEÇTİ (%100, x0,73) |
| clothes_perch_2w | geçti | 5 | KALDI (x2,31 çok doygun) |
| home_ribbon | geçti | 23 | KALDI (%30 palet dışı, x2,22) |

## Sorunlar
- Giysi sırığı: şekil tanınıyor (iki braketli yatay sırık) ama kırmızımsı koyu ton ailesinden çok doygun; `esya` kilitli olduğundan `fit` uygulanmadı. Yeniden iş gerekir.
- Kurdele: kırmızı-altın rozet tanınıyor, ancak parlak kırmızı/altın palet dışı (ödül kurdelesi doğası gereği). Plaket okunmuyor. Kullanıcı kararı: gevşetilmiş denetim ya da yeniden deneme.
- Sergi rafı: gözler seçiliyor ama düz, çıta gibi; kullanıcı göz kararı.
- Işık "/" yüzü için sol-üstten. Döşeme testi geçerli değil.
