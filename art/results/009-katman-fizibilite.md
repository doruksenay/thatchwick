unity-import: pending
# 009 Karakter katman sistemi: fizibilite raporu

Not: bu iş içe aktarılacak varlık üretmedi, yalnızca rapor. `unity-import` satırı kuyruk kuralı gereği eklendi.

- İş: 009. Kredi harcanmadı (0 gen). PixelLab'de üretim aracı çağrılmadı; yalnızca araç açıklamaları (ücretsiz) okundu ve `get_balance` kontrol edildi.
- Kaynaklar: `art-brief-tur4.md` Faz A, `Assets/Resources/Content/appearance.json`, `Assets/Art/Iso/iso_char_villager_*.png` (8 yön, 68x68).
- Ölçüm: Python/PIL betiği geçici klasörde çalıştırıldı, repoya eklenmedi.

## 1. Katalog (appearance.json'dan sayıldı)
| Grup | Kimlikler | Katman gereken |
|---|---|---|
| hairStyles | bald, crop, short, shaggy, long, ponytail, braid, bun | 7 (bald katman istemez) |
| facialHair | none, stubble, mustache, goatee, beard, full_beard | 5 |
| faceMarks | none, freckles, scar, mole, rosy | 4 |
| outfits | tunic, long_tunic, jerkin, apron, robe, dress | 6, her biri 2 bölgeli (ana + kenar) |
| hats | none, cap, hood, straw, kerchief | 4 |
| build | Slim, Average, Sturdy | 3 gövde |
| Renkler | skinTones 8, hairColors 12, eyeColors 7, clothColors 14 | Hepsi tint ile verilir, sprite gerektirmez |

## 2. (a) Mevcut köylüden betikle katman çıkarma: ölçüm
Pikseller HLS eşikleriyle sınıflandırıldı:

- Çizgi: L < 0.06
- Saç: L < 0.25 ve S < 0.15, yalnızca bbox'ın üst 12 satırında
- Ten: L ≥ 0.45, H ≤ 0.12, S > 0.35
- Tunik: S < 0.25, L ≥ 0.3
- Kahve (kemer, çizme): L < 0.45, S > 0.3

Tablodaki sayılar ölçümdür:

| Yön | Gövde boyu | Opak | Renk | Çizgi (dış kenar/iç) | Ten | Tunik | Kahve | Saç | Göz | Toka | Koyu diğer | Sınıflanamayan |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| se | 45 | 585 | 32 | 202 (108/94) | 156 | 81 | 72 | 63 | 6 | 1 | 3 | 1 |
| sw | 45 | 594 | 32 | 209 (109/100) | 156 | 91 | 67 | 62 | 8 | 0 | 0 | 1 |
| nw | 45 | 578 | 32 | 197 (107/90) | 110 | 87 | 60 | 103 | 0 | 0 | 14 | 7 |
| ne | 45 | 579 | 30 | 198 (107/91) | 110 | 90 | 61 | 104 | 0 | 0 | 14 | 2 |
| s | 44 | 632 | 17 | 239 (124/115) | 162 | 106 | 54 | 53 | 7 | 0 | 5 | 6 |
| w | 44 | 464 | 29 | 184 (104/80) | 113 | 32 | 45 | 81 | 3 | 0 | 5 | 1 |
| n | 45 | 647 | 22 | 209 (132/77) | 125 | 111 | 60 | 112 | 0 | 0 | 27 | 3 |
| e | 44 | 457 | 32 | 175 (106/69) | 116 | 42 | 38 | 74 | 4 | 0 | 4 | 4 |

SE yönünde baş 11-26. satırlar arasında: 16 satır yüksek, en geniş yerinde 15 px.

**Sonuç: kısmen mümkün.**

- **Renk bölgeleri temiz ayrılıyor.** Sınıflanamayan piksel yön başına 1-7 (opak pikselin %1.2'sinden az). Ten, tunik ve saç ("short") bölgeleri betikle ayrı PNG'lere bölünüp gri tona çevrilebilir. Tint için ton basamağı yeterli: SE'de tunik 3, ten 10, kahve 7, saç 3 renk.
- **İç çizgi sorunu.** Yön başına 69-115 piksel iç siyah çizgi var (opak alanın %13-18'i). Bunların hangi bölgeye ait olduğu renkten anlaşılmıyor. Saç da neredeyse siyah (#1e1a1b, #100d0d), bu yüzden saç ile çizgi birbirine karışıyor. Komşuluk kuralı (çizgiyi bitişik bölgeye verme) yaklaşık çözüm sağlar; yine de katman başına 5-15 pikselin elle düzeltilmesi gerekir (tahmin).
- **Örtülen alanlar çizilmemiş.** Köylü kolsuz tunik giyiyor, kolları ve bacakları çıplak, saçı kısa. Saçın altındaki kafa derisi (SE'de 63 piksellik saç alanı) ve tuniğin altındaki gövde hiç yok. Kel baş ve çıplak gövde katmanı ancak betikle doldurularak taslak kalitede yapılabilir.
- **Mevcut çizimden yalnızca iki katman çıkar:** 1 saç (short) ve 1 kıyafet (tunic). Kalan 6 saç, 5 sakal, 5 kıyafet ve 4 şapka yeni çizim gerektirir. 16x15 piksellik bir başa betikle uzun saç, örgü ya da kapüşon çizmek mümkün, ama sonuç AI çizimiyle aynı stil ailesinde durmaz.
- **Betikle kolay olanlar:** yüz işaretleri (çil, ben, al yanak, yara izi) ve göz katmanı birkaç piksellik iştir.
- **Zamanlama riski.** 010 (K1) daha büyük yeni bir karakter boyunu deniyor: gövde 48/56/64, tuval 72-96. K1 sonucunda taban karakter değişirse 68x68 köylüden çıkarılan katmanlar boşa gider. **Öneri:** katman pilotu, K1'de boy seçilip K2'nin taban karakteri belli olduktan sonra o karakter üzerinde başlasın.

## 3. (b) İşe yarayan PixelLab araçları ve maliyetleri
Maliyetler bugün araç açıklamalarından okundu; toplamlar tahmindir.

| Araç | Ne işe yarar | Maliyet (açıklamadan) |
|---|---|---|
| `inpaint_image_pro_flash` | Maskeli bölgeyi yeniden çizer, maske dışını bayt bayt korur. `output_method="Modify current layer, only changes"` yalnızca değişen pikselleri döndürür, yani katmanın kendisini verir | Görselin en uzun kenarı ≤96 px ise 5 gen (tek görsel, tek yön) |
| `edit_image` | Metinle düzenler; aynı düzenlemeyi birden çok kareye (8 yön) tutarlı uygular | 256x256'ya kadar 10 gen. Kare sınırı: ≤64 px'te 16, ≤80 px'te 9, ≤128 px'te 4 kare. 68x68'de 8 yön tek çağrıda 10 gen; 96x96'da 2 çağrı, 20 gen. Maske dışını korumaz, bu yüzden katman fark (diff) ile çıkarılırsa gürültü taşır |
| `create_character_state` | Bir karakterin 8 yönüne aynı değişikliği uygular, kimliği korur | Durum başına 10 gen (çizim ≤70 px ise). Önce köylü `create_character` v3'e tüm yönleri verilerek içe aktarılmalı (0.25 gen) |
| `create_character` v3 + referans | Güneye bakan sprite'ı 8 yöne döndürür | ceil(w*h*8/65536); 68x68 için 1 gen |
| `transfer_outfit` | Animasyon karelerine kıyafet giydirir (ileride yürüyüş için) | 10-25 gen |

**Önerilen yol (tahmin):** pilot için `inpaint_image_pro_flash`. Maske dışı sabit kaldığı için katman temiz ve hizalı çıkar; maliyet kimlik ve yön başına 5 gen.

8 yöne yaymanın iki seçeneği var:

| Seçenek | Kimlik başına maliyet | Not |
|---|---|---|
| Aynı inpaint aracıyla | 8 x 5 = 40 gen | Temiz katman |
| `edit_image` toplu, ardından diff ve elle temizlik | 10-20 gen | Ucuz ama gürültülü |

Hangisinin kullanılacağına pilot sonucuna göre karar verilmeli.

## 4. (c) Kredisiz yapılabilecekler
- **Tint testi:** köylünün ten, tunik ve saç bölgeleri gri tona çevrilip `skinTones`, `hairColors` ve `clothColors` ile çarpılarak (multiply) oyunda denenir. Betikle, yön başına 8 bölge.
- **Küçük katmanlar:** göz (yön başına 0-8 px), yüz işaretleri (freckles, mole, rosy, scar) ve kıyafet kenar maskesi (trim) betikle yapılır.
- **Taslak taban:** kel baş ve çıplak gövde, saç ve tunik alanı ten tonu ve dış çizgiyle doldurularak taslak kalitede çıkarılır.
- **AI çıktısının işlenmesi:** inpaint "only changes" çıktısının hizalanması, saçın ön/arka bölmesi, gri tonlama ve adlandırma betikle yapılır.

## 5. Toplam görsel sayısı
Kimlikler appearance.json'dan; sayılar hesaptır.

| Katman grubu | Yön başına PNG |
|---|---|
| Vücut tipine bağlı: gövde 1 + kıyafet 6 x 2 (ana + kenar maskesi) | 13 (3 vücut tipiyle 39) |
| Vücuttan bağımsız (baş aynı yerde kalırsa): saç 7 + arka katman 4 (long, ponytail, braid, bun), sakal 5, yüz işareti 4, şapka 4 + kapüşon arka 1, göz 1 | 26 |
| **Toplam** | **65** |

- 8 yönde toplam **520 PNG**.
- Baş da vücut tipine bağlı olursa 8 x 3 x (13 + 26) = **936 PNG**.

Kredi tahmini (8 yön, tahmin):

| Yol | Hesap | Toplam |
|---|---|---|
| `edit_image` toplu | saç 7 + sakal 5 + şapka 4 = 16, gövde 3, kıyafet 6 x 3 = 18; toplam 37 çağrı x 10 | **~370 gen** (68x68 tuvalde; 96x96 tuvalde ~740) |
| Her yöne ayrı `inpaint_image_pro_flash` | 37 x 8 x 5 | **~1480 gen**. Aylık 2000'lik bütçeyle uygun değil |
| Karma (önerilen): pilotta inpaint, yaygınlaştırmada `edit_image` toplu + diff + elle temizlik | | ~400-500 gen |

## 6. Yaş evreleri: ayrı gövde mi, ölçek mi?
**Öneri:** bebek ve çocuk için ayrı, küçük bir taban gövde; yaşlı için ölçek değil, durum katmanı.

- **Neden ölçek değil:** pikselart tamsayı olmayan oranda ölçeklenince pikseller düzensizleşir ve çizgi kalınlığı değişir. Örneğin 0.7 kat ölçekte 45 px'lik gövde ~31 px olur; 1 px'lik çizgiler ya kaybolur ya kalınlaşır. Kodun şimdiki ölçek çözümü geçici olarak kalabilir.
- **Çocuk:** ayrı taban (tahmin: ~30 px gövde) ve ona ait küçük bir katman seti.
- **Bebek:** tek sprite yeter (kucakta ya da beşikte); kıyafet ve saç katmanı gerekmez.
- **Yaşlı:** aynı gövde, gri saç tint ile verilir. Kambur duruş istenirse `create_character_state` ile 8 yönlü ayrı bir durum (10 gen, tahmin).

## 7. Pilot önerisi (SE yönü, Average vücut)
1. **Kredisiz ön adım:** tint testi, göz ve yüz işareti katmanları (bkz. bölüm 4).
2. **Taban karakter:** K1/K2 kararından sonra seçilir (bkz. bölüm 2'deki zamanlama riski).
3. **Kredili pilot:** 4 temsilci kimlik, SE yönü, `inpaint_image_pro_flash` ile (aşağıda Taslak A).

## Kredi gerektiren iş taslakları
Bunlar yalnızca öneri metni; kuyruğa eklenmedi. Karar planlama sohbetinde.

**Taslak A: Katman pilotu, 4 temsilci kimlik (SE, Average)**
```
title: Katman pilotu (SE, Average, 4 kimlik)
approved_by: user
cost_est: 20   # 4 x inpaint_image_pro_flash (5 gen, <=96 px)
cost_max: 25
depends_on: [K2 taban karakter işi]
pixellab: true
Tarif: taban karakterin SE sprite'ı üzerinde inpaint_image_pro_flash,
  output_method="Modify current layer, only changes". Kimlikler:
  hair long (baş + sırt maskesi), facialHair full_beard (çene maskesi),
  outfit robe (gövde maskesi), hat hood (baş maskesi). Çıktılar betikle
  gri tona çevrilip iso_layer_<tür>_<kimlik>_se.png adıyla Assets/Art/Iso/Layers/ altına.
Doğrulama: tabana bindirince hizalı, yarı saydam 0, ön/arka bölme.
```

**Taslak B: Kalan kimlikler (SE, Average)**
```
title: Katman pilotu tamamlama (SE, Average)
approved_by: user
cost_est: 90   # 18 kimlik x 5 (inpaint_pro_flash), tahmin
cost_max: 100
depends_on: [Taslak A accepted]
```

**Taslak C: 8 yöne yayma (Average)**
```
title: Katmanlar 8 yön (Average)
approved_by: user
cost_est: 220  # 22 kimlik x edit_image 8 yön toplu (10 gen/çağrı, <=80 px tuval), tahmin; 96 px tuvalde ~440
depends_on: [Taslak B accepted]
Not: çalışma başına 60 gen tavanı nedeniyle birden çok işe bölünmeli.
```

**Taslak D: Slim ve Sturdy gövde + kıyafetler**
```
cost_est: ~140  # 2 gövde + 12 kıyafet çağrısı x 10, tahmin
depends_on: [Taslak C accepted]
```

**Taslak E (isteğe bağlı): Yaşlı durumu**
```
create_character_state, 8 yön, 10 gen.
```
