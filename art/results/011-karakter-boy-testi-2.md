unity-import: pending
# 011 Karakter boy testi 2 (K1b): sonuç

- İş: 011, durum `review`. Kullanıcı boyu seçecek; K2 bu karara bağlı.
- Tarih: 2026-11-08.

## Araç ve ayarlar
**1. Üretim.** `create_character` aracıyla üç çağrı yapıldı. Ortak ayarlar:
- `mode=v3`, `body_type=humanoid`
- `view=high top-down`: 2:1 izometriğin ~30°'sine en yakın seçenek.
- `outline=single color black outline`, `detail=high detail`
- Açıklama metni 010 ile birebir aynı.
- Referans kullanılmadı; tarifte gerekçesi var.

Çağrıdan önce okunan maliyet: "v3 from scratch ... 2 for 16-90". Buna göre her çağrı 2 generation.

**2. Renk azaltma.** `reduce_colors` aracıyla her boy için bir çağrı yapıldı; o boyun 8 yönü birlikte verildi, böylece tek ortak palet çıktı.
- Ayarlar: `num_colors=32`, `dithering=none`.
- Maliyet: açıklamada "0.1 generations" yazıyor.
- Girdi: `get_upload_url` ile yüklenip `upload:<id>` olarak verildi, base64 kullanılmadı.
- Çıktı sırası girdi sırasıyla aynı. Bu, her karenin alfasının aynı adlı ham kareyle birebir eşleşmesiyle doğrulandı.

| Çağrı | size | Id | Generation |
|---|---|---|---|
| create_character farmer_a2_48 | 52 | 83ba590f-0b6e-4aa2-98e6-db689a3f3635 | 2 |
| create_character farmer_a2_56 | 60 | 37cebe59-b761-4dd8-b23c-b11f60f58294 | 2 |
| create_character farmer_a2_64 | 68 | fbc29cc9-7d14-453f-afe7-9005eca5429e | 2 |
| reduce_colors 48 (8 kare) | 52 | ebfbcbb6-eb75-4215-a985-b879142f3371 | 0,1 |
| reduce_colors 56 (8 kare) | 60 | 1aec7690-37a9-49e9-967f-de33ae95c65a | 0,1 |
| reduce_colors 64 (8 kare) | 68 | 309467ad-3870-4e20-9ada-6b00cbb4b123 | 0,1 |
| **Toplam** | | | **6,3** (cost_est 6,3, cost_max 8) |

Bakiye (`get_balance`):
- İş öncesi 1913, sonrası 1896,7. Fark 16,3.
- Bu işin payı 6,3. Kalan 10 generation aynı anda çalışan zemin/hayvan oturumuna ait; masaüstü `_indeks.md`'ye o oturumdan yeni satır düştü.
- Eşzamanlı iş sınırına takılmadı.

## Dosyalar
Hiçbir `.meta` yazılmadı.

**Proje (`cp -n`).** Yön eşlemesi: s, se, e, ne, n, nw, w, sw.
- Renk azaltılmış: `Assets/Art/Iso/_aday/iso_char_farmer_a2_{48,56,64}_cand_{s,se,e,ne,n,nw,w,sw}.png`, 24 dosya.
- Ham: `Assets/Art/Iso/_aday/_ham/iso_char_farmer_a2_{48,56,64}_cand_{yön}.png`, 24 dosya.

| Boy | Tuval | Pivot x (SE) | Pivot y (SE, ALTTAN) |
|---|---|---|---|
| 48 | 52x52 | 28.0 | 1 |
| 56 | 60x60 | 31.5 | 1 |
| 64 | 68x68 | 34.5 | 0 |

Pivot yöntemi 010 ile aynı:
- **y:** tuval yüksekliği - 1 - en alt opak satır.
- **x:** en alt opak satırdan yukarı, boyun %10'u kadar satırlık bant alınır; x, bu banttaki opak sütunların ortası.

**Betik:** `tools/art/char_preview.py`. Ölçüm yapar (`measure`) ve önizleme üretir (`sheet`, `row`). PixelLab'e dokunmaz.

**Masaüstü (kullanıcının istisnası; silme ya da üzerine yazma yok):**
- `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_farmer_a2_{48,56,64}\`. Her birinde:
  - 8 renk azaltılmış yön
  - `_ham\` altında 8 ham yön
  - `_onizleme_x4.png`. Üst satır: köylü SE, renk azaltılmış SE, ham SE. Alt satır: 8 yön. Hepsi kendi 64x32 elmas konturunun üstünde, pivot elmas merkezinde.
- `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_K1b_toplu_x4.png`: köylü ve üç boy, tek satırda.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md`: sonuna 3 satır eklendi. Generation sütunu 2.1, yani 2 + 0,1.

## Doğrulama: SE ölçümü
| Boy (hedef) | Dosya | Gerçek boy | Genişlik | Renk | Yarı saydam | Pivot x | Pivot y (alttan) | ±3 |
|---|---|---|---|---|---|---|---|---|
| 48 | ham | 49 | 26 | 46 | 0 | 28.0 | 1 | tuttu |
| 48 | renk azaltılmış | 49 | 26 | **32** | 0 | 28.0 | 1 | tuttu |
| 56 | ham | 58 | 32 | 57 | 0 | 31.5 | 1 | tuttu |
| 56 | renk azaltılmış | 58 | 32 | **32** | 0 | 31.5 | 1 | tuttu |
| 64 | ham | 66 | 35 | 60 | 0 | 34.5 | 0 | tuttu |
| 64 | renk azaltılmış | 66 | 35 | **32** | 0 | 34.5 | 0 | tuttu |
| köylü (ref.) | iso_char_villager_se | 45 | 17 | 32 | 0 | 34.5 | 12 | — |

`validate_iso.py <SE> --kind object --max-colors 32` sonuçları:
- Üç renk azaltılmış SE dosyası: geçti.
- Ham SE'ler: yalnızca renk sınırında kaldı (46/57/60 > 32). Bu beklenen sonuç; ham dosyalar renk ölçütü için teslim değil.

**Başarı ölçütü üç boyda da tuttu:** boy hedefin +1/+2/+2 px içinde, renk 32 ve altında, yarı saydam piksel 0. v3'te gövde boyu size'ın yaklaşık %94-97'si çıkıyor.

## Doğrulama: 8 yön (renk azaltılmış)
Her hücre: renk / yarı saydam / pivot y (alttan).

| Boy | s | se | e | ne | n | nw | w | sw |
|---|---|---|---|---|---|---|---|---|
| 48 | 30/0/2 | 32/0/1 | 32/0/2 | 31/0/2 | 29/0/3 | 30/0/2 | 32/0/2 | 32/0/1 |
| 56 | 32/0/1 | 32/0/1 | 31/0/1 | 30/0/2 | 29/0/1 | 29/0/2 | 32/0/1 | 32/0/1 |
| 64 | 31/0/1 | 32/0/0 | 31/0/1 | 30/0/2 | 30/0/2 | 30/0/2 | 32/0/1 | 32/0/0 |

- Her boyun 8 yönü birlikte tek palet kullanıyor; ortak palet tam 32 renk.
- Renk azaltma alfayı bozmadı: 24 karenin hepsinde alfa kanalı hamla bayt bayt aynı.
- Değişen opak piksel sayısı kare başına: 48'de 155-291, 56'da 176-353, 64'te 374-770.
- Boylar da yönler arasında aynı kaldı: 48 için 47-49, 56 için 57-58, 64 için 65-66.

Pivot y yöne göre 0-3 px değişiyor; arka yönler (n, ne, nw) 1-2 px yukarıda bitiyor. 010'daki gibi yön başına pivot önerilir.

## Boylar arası detay farkı
SE yönü 8 kat büyütülerek bakıldı.
- **Yüz:**
  - 48: kaş ve göz çizgisi, sakal okunuyor; burun ve ağız sakal kütlesine karışıyor.
  - 56: kaş, burun ve sakal ayrı okunuyor; ağız çizgisi zayıf.
  - 64: kaş, burun, bıyık, sakal ve ağız ayrı ayrı okunuyor. En okunur yüz bu.
- **Eller:**
  - 48: yumruk lekesi.
  - 56: 1 px parmak çizgisi.
  - 64: parmak boğumları seçiliyor.
- **Kemer, toka, kese:**
  - Üçünde de kemer, gri metal toka ve çapraz omuz kayışı var.
  - Kese 56'da sağ kalçada belirgin, 64'te önden (S) belirgin, 48'de küçük.
  - 64'te kahve deri bileklik (kol manşeti) da var.
- **Bacak sargısı:**
  - 48: açık bej bantlar.
  - 56: iki bant net.
  - 64: bant ve gölge net.
- **Ayakkabı bağı:** üç boyda da seçilmiyor. Düz kahve deri ayakkabı/çizme.
- **Oran:**
  - 48 tıknaz (baş/gövde oranı büyük).
  - 56 en dengeli.
  - 64 iri ve kalın gövdeli.
  - Mevcut köylü (45 px, çocuksu oranlar) yanında üçü de yetişkin ve daha gerçekçi duruyor.

**Renk azaltmanın etkisi:**
- 8 kat büyütmede ham ile azaltılmış arasında yüz ve aksesuar okunurluğunda gözle görülür kayıp yok.
- Değişim, birbirine çok yakın ton çiftlerinin birleşmesinden ibaret (tunik kıvrım ara tonları, ten geçişleri).
- Toka gri kaldı, göz ve kaş koyu kaldı, kayış ve kese kontrastı korundu.
- Dithering yok, banding görülmedi.

## Üç boy arası tasarım tutarlılığı
- **Ortak olanlar:**
  - Kahve kısa saç, sakal
  - Hardal/okra yün tunik
  - Kemer, toka, omuz kayışı
  - Bej bacak sargısı, kahve ayakkabı
- **Farklar:** v3 seed almıyor, üç ayrı çizim yaptı.
  - 48: tunik kolları açık krem (alttaki gömlek görünüyor), tunik sarı-kahve.
  - 56: tunik turuncu-okra, kol manşetleri koyu kahve.
  - 64: tunik zeytin-okra, en koyu ton. Saç ve sakal daha dolgun, kol manşeti deri.
- 010'a göre tutarlılık daha iyi: omuz kayışı bu kez üçünde de var. Yine de tunik tonu ve kol rengi bire bir değil.
- K2'de tek bir boyun karakteri temel alınırsa bu fark ortadan kalkar.
- Gölge pişirilmemiş, zemin saydam. Işık sol-üstten: sol omuz ve tunik açık, sağ yan koyu.

## Sorunlar ve sorular
1. **64'ün SE ve SW yönleri tuvalin son satırına değiyor** (pivot y 0). Görsel kontrolde ayakkabılar kesik görünmüyor, siyah kontur tamam. Yine de Unity'de taban için 0 px pay var.
2. **Pivot y yöne göre 0-3 px oynuyor.** Yön başına pivot önerilir; 010'daki öneri geçerli.
3. **Boylar arası tasarım tam aynı değil** (tunik tonu, kol rengi). Seçilen boydan K2 üretilirken tutarlılık için ek yöntem gerekebilir, ör. `mode=pro` + `style_character_id`. Karar kullanıcıda.
4. Bu çalışmada yeniden deneme yapılmadı; her çağrı tek seferde başarılı oldu.
