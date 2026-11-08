# Art Brief: Tur 4 (yapı modu, karakter yaratma, ev eşyaları)

Bu dosya Claude Code için iş emridir. tur 3 (art-brief.md, art-brief-result.md) hâlâ geçerlidir ve onunla çelişmez; bu dosya YENİ ihtiyaçları ekler. Bağlayıcı şartname `docs/gorsel-yonerge.md` (izometrik 2:1, 64x32 karo, PPU 32, pivot kuralı, adlandırma). Çelişkide yönerge geçerlidir, bana bildir.

## 0. Önce yap (harcama yapmadan)
1. `git pull --rebase`.
2. `docs/gorsel-yonerge.md`, `art-brief-result.md` ve bu dosyayı oku. Tur 3'te teslim edilenleri (köylü 8 yön, yatak 2x1, ocak taşı, tabure, çimen durumu) envanter olarak çıkar; bu turda tekrar üretme.
3. Aşağıdaki her kalem için araç seçimini, tahmini generation harcamasını ve yaklaşık toplamı yazan bir plan çıkar. **Planı bana göster, onay almadan PixelLab'de kredi harcama.** Hak bütçesi kuralları tur 3'teki gibidir (her çağrıdan önce maliyet, yetmiyorsa dur ve sor, hata olursa aynen bildir, her fazın sonunda dur).

## Oyundaki yeni ihtiyaçlar (neden?)
- **Karakter yaratma ekranı:** oyuncu saç stili, saç/göz/ten/kıyafet/başlık rengi, sakal, yüz işareti, vücut tipi seçer. Bu kombinasyonları her biri için ayrı karakter çizerek karşılamak imkânsız. **Katmanlı sprite + renk değişimi** gerekir.
- **Yapı modu (Sims tarzı):** oyuncu ev eşyası yerleştirir, taşır, döndürür; masanın üstüne küçük dekor koyar (kamera masaya yaklaşır). Eşya döndürme 4 yönlü; çok karolu eşyalar var.

## Faz A: Karakter katman sistemi (en kritik, önce fizibilite)
Kod tarafında kataloğ `Assets/Resources/Content/appearance.json`'dadır; kimlikler ve seçenekler orada. Bu kimliklerle birebir eşleşen katmanlar gerekir:
- **hairStyles:** bald (katman yok), crop, short, shaggy, long, ponytail, braid, bun
- **facialHair:** stubble, mustache, goatee, beard, full_beard
- **faceMarks:** freckles, scar, mole, rosy
- **outfits:** tunic, long_tunic, jerkin, apron, robe, dress
- **hats:** cap, hood, straw, kerchief
- **build:** Slim, Average, Sturdy (3 vücut tipi); **boy** kodda ölçekle ayarlanır, sprite gerekmez.

Teknik kurallar:
1. Her katman ayrı PNG, **aynı tuval boyutu (köylüyle aynı 68x68) ve aynı pivot**, aynı yön ve (ileride) aynı animasyon karesi. Katmanlar üst üste bindirilince hizalı olmalı.
2. **Renklendirilecek katmanlar (saç, sakal, kıyafet ana rengi, kenar rengi, başlık) gri tonlu, açık/koyu gölgeli üretilir**; oyun bunu çarparak renklendirir (tint). Ten ve göz için ayrı "ten katmanı" (gri tonlu gövde/yüz/eller) ve göz katmanı. Kıyafette iki bölge var (ana renk ve kenar/pantolon rengi): iki ayrı gri katman ya da ana katman + maske katmanı olarak ver, hangisini seçtiğini yaz.
3. Uzun saç, at kuyruğu, örgü, kapüşon gibi gövdenin **arkasında** kalan kısımlar için "arka" katman, öndekiler için "ön" katman ver.
4. Adlandırma: `iso_layer_<tür>_<kimlik>_<yön>.png`, ör. `iso_layer_hair_long_back_se.png`, `iso_layer_hair_long_front_se.png`, `iso_layer_outfit_tunic_main_se.png`, `iso_layer_outfit_tunic_trim_se.png`, `iso_layer_body_average_se.png`, `iso_layer_eyes_se.png`, `iso_layer_hat_cap_se.png`. `Assets/Art/Iso/Layers/` altına.
5. **Kombinasyon patlamasına dikkat:** 8 yön x 3 vücut x ~25 katman = yüzlerce görsel. PixelLab kredisiyle her birini üretme. Önce **fizibilite raporu** yaz: (a) mevcut köylüyü baz alıp katmanları **betikle** (Python/PIL: renk bölgelerinden ayırma, gri tonlamaya çevirme, elle piksel düzenleme) çıkarmak mümkün mü, (b) PixelLab'in hangi araçları (ör. varyant/edit) buna yarar, (c) hangi kısmı kredisiz ürettirebilirsin. Ucuz ve tutarlı yol hangisiyse onu öner.
6. **Pilot (onay sonrası):** yalnızca **SE yönü, Average vücut**, tüm kimlikler için katmanları üret; oyunda bindirilerek denenecek. Pilot beğenilirse diğer yönlere (S, SW, W, NW, N, NE, E) ve vücut tiplerine geç.
7. Yaş evreleri (bebek, çocuk, yaşlı) ayrı çizim ister; bu turda yapma, sadece "ayrı gövde katmanı mı, ölçek mi" önerini yaz (kod şu an bebek ve çocukta ölçek kullanıyor).

## Faz B: Yapı modu eşyaları (yer eşyaları, 4 yönlü)
Her eşya 4 yönde gerekir; kural: SE ve SW çizilir, NW ve NE aynaya çevrilerek kodda üretilebiliyorsa (ışık yönü bozulmayacaksa) onu öner ve yaz. Çok karolu eşyada tabanın kapladığı karo sayısını (ör. `_2x1`) yönün her iki ekseni için ver.
| Eşya | Karo | Not |
|---|---|---|
| Ahşap masa (`item.table_wood`) | 2x1 | **Üst yüzü düz ve boş**, üstüne 3 dekor yuvası konacak. Üst yüzün elmas içindeki yaklaşık konumlarını (yuva noktaları) piksel olarak ver |
| Sandalye / tabure çeşidi | 1x1 | tabure teslim edildi; arkalıklı sandalye ekle |
| Ahşap sandık (`item.chest_wood`) | 1x1 | depolama |
| Raf (`item.shelf_wood`) | 1x1 | üst yüzünde 2 dekor yuvası |
| Hasır kilim | 2x1 (yerde, geçilebilir olabilir) | zemin dekoru; ayrı karar: yürünebilir mi, bana sor |
Mevcut `item.bed_straw` (2x1), `item.hearth_stone`, `item.cooking_pot` için de 4 yön gerekir; tur 3'te hangi yönler teslim edildiyse eksikleri listele.

## Faz C: Yüzey dekorları (küçük, 1 yuvaya sığar)
Masa ve rafın üstüne konan minik eşyalar. Boyut yaklaşık 12-20 px geniş, taban 1 yuva noktasında (pivot: tabanın merkezi). Her biri yalnızca **tek görünümle** (yön bağımsız ya da SE) yetinebilir; 4 yönlü şart değil, ama ışık sol-üstten olmalı.
- mum (`item.candle`), saksı çiçeği (`item.flower_pot`), kil bardak (`item.clay_cup`), kâse/ekmek (`item.bread_bowl`), kitap (`item.book`)
Bunlar ucuz üretilebilir olmalı (düşük generation); nesne araçlarının pahalı olması durumunda betikle çiz.

## Faz D: Yapı modu arayüz görselleri (kredisiz, betikle)
- Yerleştirme önizlemesi için elmas ızgara vurguları: yeşil (geçerli) ve kırmızı (geçersiz) 64x32 elmas, yarı saydam (bu tek istisna: bu iki dosyada yarı saydamlık serbest).
- Yuva işaretçisi (dekor yuvası): küçük sarı elmas/halka, 16x8 civarı.
- Dosyalar: `iso_ui_tile_ok.png`, `iso_ui_tile_bad.png`, `iso_ui_slot.png`.

## Teslimat ve rapor
- Her faz sonunda dur ve rapor ver: dosya listesi, piksel boyutları, pivotlar, harcanan generation, kalan bakiye.
- Zemin/duvar için tur 3'teki doğrulama kuralları (2:1 kenar, saydamlık, ışık yönü) aynen geçerli. Katman PNG'lerinde ek olarak: tüm katmanların boyut ve pivotunun aynı olduğunu bir betikle doğrula ve raporla.
- `Assets/Scripts` altına dokunma.
