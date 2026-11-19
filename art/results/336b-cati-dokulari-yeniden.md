unity-import: pending

# 336b Çatı dokuları yeniden: sonuç (bulut işçisi, şerit 1, 2026-11-19)

Durum: **review (kısmi)**. 8 karodan 3 eğim karosu teslim edildi; 5'i teslim edilmedi.

- Yan yana sayfası (3 teslim + referanslar): [336b-yanyana.png](336b-yanyana.png)
- Ham 8 karo (4x): [336b-ham-8-karo.png](336b-ham-8-karo.png); reddedilenler `336b-red/`
- Döşeme testleri (8x8): [saz](336b-doseme-thatch.png), [şindıl](336b-doseme-shingle.png), [kiremit](336b-doseme-tile.png)

## Araç ve ayarlar
- `create_isometric_tile` x8, tek tek: `size 64`, `tile_shape thin tile`, `outline selective outline`, diğerleri varsayılan. Tarifteki açıklama ekleri aynen kullanıldı.
- **Sapma:** araçta `style_images` / `style_options` parametresi yok; stil referansı verilemedi. Palet yakınlığı `style.py fit` ile sonradan sağlandı.
- Son (8.) çağrı ilk denemede hız sınırı hatası verdi (10/10 iş, diğer şeritler); bir slot açılınca bir kez yeniden denendi, geçti.
- Araç 64x64 **kalın döşeme** (üst elmas + yan duvar) döndürdü. Üst yüz 64x32 elmas maskesiyle kırpıldı (yalnızca maske, içerik yeniden boyanmadı; yarı saydam pikseller opak yapıldı). Maske içi boş: 0-2 piksel (elmas içi, kıpırdamadan en yakın komşuyla dolduruldu). Her karoda ~550-600 maske dışı piksel (yan duvar) silindi.
- Harcama: araç maliyeti çağrıdan önce okunamadı (açıklamada fiyat yok); `cost_est` 12 sınırında kabul edildi. Paralel şeritler yüzünden `get_balance` farkı bu işe ait ayrıştırılamadı. Çağrı öncesi 3559,5, sonrası 3518,9 (tüm şeritler). Tahmini bu iş: ≤12.
- `get_balance` sonrası kalan: **3518,9**. Masaüstü kopyası: yerel.

## Dosyalar (teslim)
| Dosya | Boyut | Pivot x, y (alttan) |
|---|---|---|
| Assets/Art/Iso/iso_roof_thatch_a.png | 64x32 | 32, 16 |
| Assets/Art/Iso/iso_roof_shingle_a.png | 64x32 | 32, 16 |
| Assets/Art/Iso/iso_roof_tile_a.png | 64x32 | 32, 16 |

## Doğrulama
| Karo | validate_iso ground | Stil (yapi) | Doygunluk | Dikiş oranı (hedef 0,95-1,05) | Karar |
|---|---|---|---|---|---|
| saz eğim | geçti | UYARI (palete uzak) | x1,35 | 0,86 | teslim |
| şindıl eğim | geçti | UYARI | x1,29 | 0,64 | teslim |
| kiremit eğim | geçti | UYARI (fit sonrası; ham KALDI x1,49) | x1,15 (fit doygunluk x0,77) | 0,74 | teslim (fit uygulandı) |
| arduvaz eğim | geçti | KALDI: palet dışı %40 (fit sonrası da) | x0,88 | 1,15 | teslim edilmedi |
| saz sırt | - | KALDI (x1,72) | | | sırt yok, saz eğimle aynı; teslim edilmedi |
| şindıl sırt | - | UYARI | x1,09 | | piramit çıktı; teslim edilmedi |
| kiremit sırt | - | KALDI (x1,69, %35) | | | piramit çıktı; teslim edilmedi |
| arduvaz sırt | - | KALDI (%18) | | | sırt yok, blok doku; teslim edilmedi |

Dikiş oranı: kenar bandı (elmas kenarına yakın) / iç parlaklık; koyu dış çizgi yüzünden üç teslimde hedefin altında. Karolar dikişsiz döşeniyor ama kenarlar koyu çizgili izlenebilir. Kullanıcı görsel kararı versin.

## Sorunlar / kullanıcı kararı
- **Düz eğim sorunu çözüldü:** 4/4 eğim karosu düz doku geldi (piramit yok); yedek yöntem tetiklenmedi.
- **Sırt karoları başarısız:** 4 sırt karosunun ikisi piramit (şindıl, kiremit), ikisi sırtsız (saz, arduvaz). `create_isometric_tile` elmas içinde yatay sırt örtüsü vermiyor. Öneri: sırtı eğim karosundan türetmek (kod tarafında orta hatta ince koyu bant) ya da `create_building_kit` (`roof_profile`).
- **Arduvaz eğimi:** palet dışı renkler (mavi-gri, yapı paletinde yok); `fit` yalnızca doygunluğu düzeltir. Ham/fit hâli `336b-red/`de. Ailede mavi-gri ton gerekecekse kullanıcı karar versin.
- **Stil referansı:** araç `style_images` almıyor; `create_tiles_pro` stil modu (336) bunu destekliyordu ama piramit sorunu orada çıkmıştı.
- Öneri (araç): `create_isometric_tile` açıklamasında maliyet yok; işçi çağrı öncesi okuyamadı.
