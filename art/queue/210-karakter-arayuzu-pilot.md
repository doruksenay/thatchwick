---
id: "210"
title: Karakter arayüzü stil pilotu (Durum paneli; parşömen + ahşap)
status: pending
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: arayuz
style_gate: true
cost_est: 40
cost_max: 50
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_panel_durum_parsomen_1.png
  - Assets/Art/Iso/_aday/ui_panel_durum_ahsap_1.png
  - art/results/210-durum-parsomen-onizleme-x2.png
  - art/results/210-durum-ahsap-onizleme-x2.png
created: 2026-11-17
---
# 210 Karakter arayüzü stil pilotu

## Amaç
Moodlet'lerin ve karakter bilgilerinin göründüğü ekranlar PixelLab'le, oyunun piksel diliyle üretilecek. Kullanıcı HTML taslaklarını beğenmedi.

Bu pilot oyunun en yoğun ekranını, karakter panelinin **Durum** sekmesini, iki görsel yönde üretir:
- **parşömen:** el yazması ve deri;
- **ahşap:** meşe pano, demir bantlar.

Kullanıcının seçtiği yön bütün arayüz ailesinin stili olur: 211 (kalan ekranlar), 212 ve 315 (ikonlar), 200 (ana menü).

PixelLab yalnızca boş paneli çizer. Yazılar, portre, çubuk dolguları ve ikonlar oyunda koddan gelir. Kullanıcı sonucu oyundaki hâliyle görsün diye `tools/art/ui_mockup.py` bunları panelin üstüne yerleştirir. Yazı fontu Tiny5 (OFL, `tools/art/fonts/`).

**Ölçü:** ekran 844x390 (telefon, yatay). Panel 416x192 üretilir, oyunda 2 kat büyütülür (832x384).

Yerleşimin tek kaynağı `tools/art/ui_layouts.json` → `durum`:
- başlık şeridi: küçük portre çerçevesi, isim levhası, 4 sekme, kapat;
- solda ruh hali kartı ve 5'li gösterge çerçevesi;
- ortada 6 ikon yuvası ve ihtiyaç çubuğu çerçevesi;
- sağda moodlet listesi: 5 boş kart, kaydırma çubuğu.

## Tarif

### Araç ve ortak ayarlar (iki aday için aynı)
- **Araç:** `create_ui_asset`.
  - **Önce araç açıklamasını oku.**
  - 416x192 desteklenmiyorsa ya da tek çağrı 20 generation'ı geçiyorsa çağırma. `## Sorular` ekle, `blocked` yap. Desteklenen en büyük ölçüyü ve maliyeti yaz.
- `width=416`, `height=192`, `no_background=true`, `max_colors=48`, `seed=0`.
- `style_image_base64` = `Assets/Art/Iso/iso_char_villager_se.png` (200 ile aynı; palet ve dış hat dünyaya bağlansın).
- `color_palette` = `"earthy muted medieval browns, cream parchment, dark iron grey, muted moss green accent"`.
- `pieces`: iki adayda **aynı** şablon. Şu komutun çıktısı:
  `python tools/art/ui_mockup.py pieces durum --scale 2`
  - Koordinatlar çıktının 2 katı sanal tuvalde: 832x384 (200'deki 512 → 256 düzeniyle aynı).
  - Araç sanal tuvali başka tanımlıyorsa koordinatları orantılı ölçekle ve raporda yaz.
- `elements` verme.
- Her açıklamanın sonuna şu ek:
  `, pixel art game UI screen panel for a cozy medieval village life simulation, earthy muted medieval palette, flat simple shading, clean dark outline, light from top-left, crisp readable shapes, no dithering, no noise, no text, no letters, no numbers, no icons, no faces, all plaques, tabs, cards, slots and bars empty and blank`

### Adaylar
| Aday | Yön | `name` | Açıklama metni (+ ortak ek) | Gen |
|---|---|---|---|---|
| ui_panel_durum_parsomen_1 | parşömen ve deri | `thatchwick_panel_durum_parsomen_1` | `character status panel made of aged cream parchment with a dark brown ink border, a header strip of worn brown leather with stitched edges, a small empty carved wooden portrait frame on the left of the header, a blank dark leather name plaque, four blank parchment tabs where the first tab is lighter and joined to the panel body, a small blank square close button, inside the body a blank parchment card on the left with an empty thin gauge frame, six small empty square icon slots each next to an empty thin progress bar frame in the middle, and on the right a slightly darker inset parchment list area holding five blank rounded cards and a thin empty scrollbar track` | ≤20 |
| ui_panel_durum_ahsap_1 | ahşap pano, demir bantlar | `thatchwick_panel_durum_ahsap_1` | `character status panel made of dark oak planks with riveted dark iron bands on the outer edge and corners, a header beam of darker wood, a small empty wooden portrait frame with iron corners on the left of the header, a blank wooden name plaque, four blank wooden tabs where the first tab is lighter and joined to the body, a small blank square iron-studded close button, the body holds light cream parchment insets pinned onto the wood: a blank parchment card on the left with an empty thin gauge frame, six small empty square icon slots each next to an empty thin progress bar frame in the middle, and a parchment list area on the right with five blank rounded cards and a thin empty scrollbar track` | ≤20 |

### Önizleme (her aday için, PixelLab yok)
```
uv run --with pillow python tools/art/ui_mockup.py mockup durum --panel Assets/Art/Iso/_aday/ui_panel_durum_<yon>_1.png --out art/results/210-durum-<yon>-onizleme-x2.png
```
- Betik eksik portreyi ve ikonları yer tutucuyla çizer ve listesini basar. Listeyi rapora yaz.
- 020 bitmişse `portrait_aldric_64.png` kendiliğinden kullanılır.
- Panel 416x192 değilse betik uyarır, en yakın komşuyla sığdırır. Uyarıyı rapora yaz.

### Teslim
- PNG'ler: `Assets/Art/Iso/_aday/` altına `cp -n`; `.meta` yazma. İşçi `Assets/Art/UI/` altına yazmaz.
- Önizlemeler: `art/results/` altına.
- Ölçü ya da saydamlık beklenenden farklıysa yeniden örnekleme ya da boyama yok. Ölç, raporla, adayı "⚠" ile işaretleyip yine de teslim et.

### Masaüstü kopyası (yalnızca yerel işçi)
Hiçbir şeyi silme, üzerine yazma.
- `C:\Users\User\OneDrive\Desktop\art\arayuz\<yyyy-mm-dd>_durum_<yon>\`: panel PNG, `_onizleme_x2.png` (önizleme betiğinin çıktısı) ve panelin boş hâlinin 2 kat büyütülmüşü.
- `C:\Users\User\OneDrive\Desktop\art\arayuz\<yyyy-mm-dd>_durum_toplu.png`: iki önizleme alt alta, aralarında 16 px.
- `_indeks.md` sonuna aday başına bir satır.

## Doğrulama
- **Tablo:** piksel boyutu, opak renk sayısı, tam saydam / tam opak / yarı saydam piksel sayısı.
- **Ölçütler:** 416x192, renk ≤ 48, köşeler saydam, yarı saydam 0 (değilse ⚠).
- **Şablona uyum:** her parçanın (`ui_layouts.json` → `durum` → `pieces`) yerinde bir şekil var mı? Kutu başına "var / kaymış / yok" tablosu. Kayma 2 px'ten fazlaysa ⚠.
  - Önizlemede yazılar kutulara oturuyor mu?
  - Çubuk dolguları çerçevenin içinde mi?
- **Görsel kontrol (4 kat):**
  - yazıya ya da harfe benzer şekil (olmamalı);
  - yüz ya da ikon (olmamalı);
  - dithering ya da gürültü;
  - dış hat net ve koyu mu;
  - ışık sol-üstten mi;
  - aktif sekme ayırt ediliyor mu;
  - kartlar ve yuvalar boş mu.
- Bilinen bir oyunun arayüzüne, logosuna ya da çerçeve motifine benzerlik var mı?
- `style.py check --family arayuz`: aile referansı yok. Paletin genel uzaklığını yaz, KALDI uygulanmaz (pilot).

## Sorular
- (işçi, 2026-11-18) `get_balance`: Tier 2 (Pixel Artisan), 4999/5000 generation, yenilenme 2026-12-18. `art/budget.md` Tier 1 / 2000 / 2026-12-08 yazıyor. İşçi kuralı "plan Tier 1 değilse dur" olduğundan PixelLab çağrılmadı (0 generation). Kullanıcı `art/budget.md` planını güncelleyip işi `pending` yapmalı.
- Tarif düzeltmeleri (üretim başlarsa): `ui_mockup.py pieces` çıktısında `kind: rect` var; `create_ui_asset` yalnızca rounded_rect/circle/polygon kabul ediyor (rect -> rounded_rect radius 0). Araç sanal tuvali en uzun kenar 512 (416x192 için 512x236); 832 koordinatları 512/832 ile ölçeklenmeli. `art/tmp/pieces.json` bu dönüşümü içerir. Maliyet 10-25 gen/çağrı, 416x192 için kesin değil.

## Notlar
- Kullanıcı yön seçerken iş dosyasının altına `yon: parsomen` ya da `yon: ahsap` yazar. 211, 212, 213 ve 315 bu satırı okur. **Satır yoksa varsayılan `parsomen`** (otomatik akış: kuyruk kullanıcı seçimini beklemez). Kullanıcı sonra `ahsap` seçerse, henüz başlamamış işler ahşapla çalışır; bitmiş olanlar için yeniden üretim işi yazılır.
- Pilot kabul edilince `art/style/references.json` → `arayuz` referansına seçilen panel eklenir (kod oturumu yapar).
- Ruh hali, çubuk ve moodlet değer renkleri koddan gelir. Panel bu renkleri içermez, bilerek nötrdür.
- Telif: bilinen oyunların arayüzü kopyalanmaz; açıklamada oyun adı geçmez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz.
Kabulde seçilen yönü yaz: yon: parsomen | ahsap -->
- 2026-11-18 (tamir): `art/budget.md` planı Tier 2 olarak güncellendi, ajan kuralı "Tier 1 değilse dur" yerine "budget.md planıyla uyuşmuyorsa dur" oldu; iş yeniden `pending`.
