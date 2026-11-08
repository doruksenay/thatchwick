unity-import: pending
# 010 Karakter boy ve detay testi (K1): sonuç

- İş: 010, durum `review`. Kullanıcı boyu seçecek; K2 (011) bu karara bağlı.
- Tarih: 2026-11-08.

## Araç ve ayarlar
`create_character` aracıyla üç çağrı yapıldı. Ortak ayarlar:
- `mode=v3`
- `body_type=humanoid`
- `view=high top-down`: tarifteki gibi; 2:1 izometriğin ~30°'sine en yakın seçenek ~35°.
- `outline=single color black outline`
- `detail=high detail`
- Açıklama metni tariftekiyle birebir aynı.

Çağrıdan önce okunan maliyet: aracın açıklamasında "v3 from scratch ... 2 for 16-90; 3 for 91-128" yazıyor. Buna göre 72 ve 84 için 2'şer, 96 için 3 generation; toplam 7, `cost_est` ile aynı. Eşzamanlı iş sınırına takılmadı; üç çağrı art arda kabul edildi.

| Boy etiketi | size | Character id | Generation |
|---|---|---|---|
| 48 | 72 | e0e23751-bc6f-4925-8cdd-17b1f6de29c3 | 2 |
| 56 | 84 | 4c545cbc-8ab7-4b21-8737-495ca0eb9681 | 2 |
| 64 | 96 | 0e892788-a35d-48ce-af39-5be45de822bb | 3 |
| **Toplam** | | | **7** |

`get_balance` iş öncesinde 1950, sonrasında 1928 gösterdi. Fark 22, bu işin payı 7. Kalan 15 generation, aynı anda çalışan zemin/eşya oturumunun işlerine ait: bekleme sırasında kuyrukta başka bir `generation` işi görüldü.

## Dosyalar
Hepsi `Assets/Art/Iso/_aday/` altında (`cp -n`, `.meta` yok). Yön eşlemesi: south=s, south-east=se, south-west=sw, west=w, north-west=nw, north=n, north-east=ne, east=e.

| Dosya | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_char_farmer_a_48_cand_{se,sw,nw,ne,s,w,n,e}.png | 72x72 | SE: 36.5 | SE: 1 |
| iso_char_farmer_a_56_cand_{se,sw,nw,ne,s,w,n,e}.png | 84x84 | SE: 45.0 | SE: 1 |
| iso_char_farmer_a_64_cand_{se,sw,nw,ne,s,w,n,e}.png | 96x96 | SE: 50.5 | SE: 4 |

Pivotun nasıl ölçüldüğü:
- **x:** en alttaki opak satırdan yukarı, gövde boyunun %10'u kadar satır alındı (48'de 7, 56'da 8, 64'te 9). Bu bant iki ayakkabıyı birlikte kapsıyor; x, bu banttaki opak sütunların ortası.
- **y:** tuval yüksekliği - 1 - en alt opak satır.

Diğer yönlerin pivotu için aşağıdaki 8 yön tablosunda ayak bandını gör. Yönler arasında taban 1-3 px kayıyor.

Masaüstü kopyası (kullanıcının açık istisnası; hiçbir dosya silinmedi ya da üzerine yazılmadı):
- `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_farmer_a_48\`, `..._56\`, `..._64\`: her birinde 8 yön PNG ve `_onizleme_x4.png` (8 yön yan yana, en yakın komşu ile 4 kat, #333333 zemin).
- `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_K1_toplu_x4.png`: üç boy, her biri bir satır. Hücreler 96x96, sprite'lar alta ve ortaya hizalı, 4 kat.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md`: sonuna 3 satır eklendi. O satırlardaki "38 gen." gibi ifadeler **genişlik** demek, generation değil. Generation sütunu ayrı (2/2/3).

## Doğrulama: SE ölçümü
| Boy (hedef) | Tuval | Gerçek gövde boyu (opak bbox yüksekliği) | Genişlik | Renk | Yarı saydam | Pivot x | Pivot y (alttan) |
|---|---|---|---|---|---|---|---|
| 48 | 72x72 | **70** | 38 | 63 | 0 | 36.5 | 1 |
| 56 | 84x84 | **81** | 42 | 47 | 0 | 45.0 | 1 |
| 64 | 96x96 | **86** | 43 | 64 | 0 | 50.5 | 4 |

`python tools/art/validate_iso.py <SE dosyası> --kind object` üç SE dosyasının hepsinde geçti. Yarı saydam piksel 0. Renk sayısı 63, 47 ve 64; validator sınır koymadığı için yalnızca bildirdi.

**Gövde boyları hedefin çok üstünde çıktı.** v3 tuvali neredeyse tamamen dolduruyor: gövde, size'ın %97 / %96 / %90'ı kadar. Hedef boy (48/56/64) değil, size boy belirliyor. Tarifteki "size=72 → gövde 48" varsayımı tutmadı. Her üç boy da 64x32 elmasın üzerinde mevcut köylüden (45 px) çok daha uzun görünecek: 70 px yaklaşık 2.2 karo yüksekliği. Elle küçültme ya da kırpma yapılmadı. Aşağıdaki Sorular bölümüne bak.

## Doğrulama: 8 yön renk ve yarı saydam
| Boy | se | sw | nw | ne | s | w | n | e |
|---|---|---|---|---|---|---|---|---|
| 48 renk / yarı saydam | 63/0 | 64/0 | 52/0 | 55/0 | 48/0 | 55/0 | 48/0 | 59/0 |
| 56 renk / yarı saydam | 47/0 | 48/0 | 47/0 | 48/0 | 32/0 | 48/0 | 40/0 | 48/0 |
| 64 renk / yarı saydam | 64/0 | 64/0 | 64/0 | 64/0 | 57/0 | 61/0 | 53/0 | 64/0 |

32 renk hedefi yalnızca 56'nın S yönünde tutuyor (32). Diğer 23 dosyada aşılıyor. Tarif gereği renk azaltma yapılmadı.

8 yön opak bbox ölçümleri (yükseklik x genişlik):
| Boy | se | sw | nw | ne | s | w | n | e |
|---|---|---|---|---|---|---|---|---|
| 48 (72) | 70x38 | 70x37 | 67x33 | 67x34 | 68x45 | 69x23 | 67x42 | 69x23 |
| 56 (84) | 81x42 | 81x41 | 81x38 | 81x38 | 80x46 | 81x24 | 79x44 | 81x25 |
| 64 (96) | 86x43 | 86x40 | 85x40 | 85x37 | 84x48 | 86x26 | 84x46 | 86x27 |

Pivot y (alttan) yöne göre değişiyor: 48'de 1-4, 56'da 1-4, 64'te 4-7. Arkadan bakan yönler (n, nw, ne) 2-3 px daha yukarıda bitiyor. Tek bir sabit pivot kullanılırsa arka yönler 2-3 px havada durur.

## Boylar arası detay farkı
SE yönü 8 kat büyütülerek bakıldı.
- **Yüz (kaş, burun, ağız):**
  - 48: kaş ve göz okunuyor, burun yalnızca gölgeyle belli. Ağız sakalın içinde kayboluyor.
  - 56: kaş, burun ve sakal içinde ağız çizgisi seçiliyor.
  - 64: göz bebeği (açık mavi), kaş, burun ve ağız/sakal ayrı ayrı okunuyor. En okunur yüz bu.
- **Eller:**
  - 48: yumruk biçiminde leke, parmak yok.
  - 56: parmak ipucu var (1-2 px'lik çizgi).
  - 64: 2-3 parmak seçiliyor.
- **Kemer, toka, kese:**
  - Üçünde de kemer, gri metal toka ve kalçada kese var.
  - 56 ve 64'te ayrıca çapraz omuz kayışı var. 64'te kahve deri bileklik de var.
  - 48'de omuz kayışı yok; tasarım farkı.
- **Bacak sargısı:**
  - 48: açık bej bantlar halinde, zayıf okunuyor.
  - 56: belirgin sargı.
  - 64: belirgin sargı, şerit dokusu da görünüyor.
- **Ayakkabı bağı:** üç boyda da seçilmiyor. Ayakkabılar düz kahve deri, bağ çizilmemiş.
- **Genel:**
  - 48 iri başlı ve tıknaz (chibi'ye yakın) oranlarda.
  - 56 ve 64 daha gerçekçi oranlarda.
  - 64'ün tuniği daha sarı-turuncu, saçı daha uzun ve kızıl-kahve.

## 8 yön tutarlılığı
- Her boy kendi içinde tutarlı: yüz, sakal, tunik, kemer, kese ve sargı 8 yönde aynı. 56 ve 64'te omuz kayışı arkadan da doğru görünüyor.
- Yönler doğru: SE sağ-aşağı, SW sol-aşağı, NW ve NE arkadan, W ve E profilden bakıyor.
- **Boylar arasında tasarım birebir aynı değil**, çünkü v3 seed almıyor ve üç ayrı çizim yapıyor:
  - 48'de omuz kayışı yok.
  - 64'ün saçı daha uzun ve renk tonu daha sıcak.
  - Tunik rengi 48'de sarı-kahve, 56'da zeytin-kahve, 64'te turuncu-sarı.
- Gölge pişirilmemiş, zemin saydam. Işık genel olarak sol-üstten geliyor (sol omuz ve tunik açık, sağ taraf koyu).

## Sorunlar ve sorular
1. **Boy hedefi tutmadı.** Gerçek gövde boyu 70/81/86, hedef 48/56/64'tü. Hedefe ulaşmak için size yaklaşık 50/58/68 olmalı (tahmin; ölçülen size/boy oranı ~0.9-0.97). Kullanıcı Unity'de mevcut adayları denesin. Hedef boylarda yeniden üretim istenirse bu yeni bir iş olmalı (tahmini maliyet 3 x 2 = 6 gen). Bu çalışmada yeniden deneme yapılmadı.
2. **Renk sayısı 32'nin üstünde** (47-64). K2'de 32 renk şartı korunacaksa ya `mode=pro` + `max_colors` (pahalı) ya da `reduce_colors` gibi bir son işlem adımı gerekir. Karar kullanıcıda.
3. **Pivot y yöne göre 1-3 px değişiyor.** Unity'de tek pivot kullanılırsa arka yönler hafif havada durur. Yön başına pivot önerilir.
4. **Paralel oturumun dosyaları.** Bu çalışma sırasında `Assets/Art/Iso/iso_item_bed_straw_2x1_sw_ne.png` ve `iso_item_hearth_stone_unlit_se.png` (ve `.meta`'ları) paralel oturumca eklendi; masaüstü `_indeks.md`'de kaydı var. Bunlara dokunulmadı ve commit edilmedi.
