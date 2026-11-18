unity-import: pending

# 407 Öküz, örme çit ve bekçi ateşi (kısmen teslim: öküz, ateş x2; örme çit teslim edilmedi)

İş dosyası: `art/queue/411-okuz-ve-atlama-ates.md` (id 407). Yan yana: [öküz](407-yanyana.png), [ateş](407-yanyana-esya.png). Masaüstü kopyası: yerel (bulut modu).

## Araç ve ayarlar
- Öküz: `create_image_pixflux` 48x48, `direction=south`, `high top-down`, siyah dış hat, palet = koyun 1 SE'nin 10 rengi, açıklamaya `desaturated`; sonra `create_character` v3 + referans, `quadruped`, şablon `horse` (öküz şablonu yok; en yakın gövde), 8 yön. `style.py fit --family hayvan` her yöne (x1,00, değişmedi).
- Çit ve ateş: `create_map_object` stil eşleme, `footprint_guide.py` 80x72 (çit, yükseklik 24) ve 80x64 (ateş, yükseklik 20) kılavuz + maske, high top-down, low detail, basic shading. Tek deneme.

## Generation
pixflux 1 + v3 1 + map_object 3 = 5. `get_balance` sonrası: 4117,9 (paralel şeritler dahil).

## Dosyalar (`Assets/Art/Iso/_aday/`)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_animal_ox_cand_{se,sw,nw,ne,s,w,n,e}.png | 48x48 | ~24 | ~2 (en alt opak satır y=45) |
| iso_item_bonfire_se.png | 80x64 | 40 | 24 |
| iso_item_bonfire_lit_se.png | 80x64 | 40 | 24 |

## Doğrulama
| Dosya | Renk | Yarı saydam | Sonuç | Stil |
|---|---|---|---|---|
| ox 8 yön | 9-30 | 0 | validate geçti | 8 UYARI (ailesinden soluk, x0,26-0,31); kilit kurulmadı, KALDI yok |
| bonfire_se | 25 | 0 | validate geçti (footprint 8,24,64,32) | UYARI soluk x0,47 |
| bonfire_lit_se | **49** (sınır 48, 1 fazla) | 0 | validate renk sayısı BAŞARISIZ (yalnızca 1 renk) | UYARI palete uzak; `--light` GEÇTİ |

- Öküz: SE boy 43 px, doluluk 0,90; önden, zemin lekesi yok, ışık sol-üstten, gölge yok. Şablon `horse` olduğundan yönler kısmen at gibi olabilir: kullanıcı göz kontrolü gerekli (yalnız SE ve S tek tek incelendi).
- Ateş: sönük kare taş halka gibi okunuyor ama **istiflenmiş odun yok** (içi boş altıgen halka). Yanan kare iyi (alev + odun yığını). Sönük hâl tarife tam uymadığı için zayıf.

## Teslim edilmeyen
- **Örme çit** (`407-red/iso_item_wattle_hurdle_se.png`): model hurdle yerine iki duvarlı, yeşil zeminli küçük bir oda köşesi çizdi; tanınmıyor. `max_attempts: 1` nedeniyle yeniden denenmedi.

## Sorunlar / öneri
- Çit için yeni iş: `single flat woven wicker fence panel, wooden stakes, horizontal woven twigs` ve elmas kılavuz yerine ince panel maskesi (ince nesnede elmas kılavuz oda üretiyor).
- Sönük ateş için: `a pile of firewood logs stacked in a stone ring` ile yeniden.
- bonfire_lit 49 renk: `reduce_colors` bu işte yapılmadı.
