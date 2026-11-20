unity-import: pending

# 588 Tarım animasyonları, çocuklar: bend, water, sow: kısmi teslim, 20/24 klip

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = yönün durma karesi (`get_upload_url` ile yüklendi, 64x64). Metnin başına yön eklendi (`facing south-east: ...`). 24 çağrı (2 karakter x 3 anahtar x 4 çapraz yön), çağrı başına 1 generation (makbuz); toplam 24 (tahmin 24, tavan 34). Tek deneme.
- Karakterler: boy_r64 `6940a34e-75c4-4cb6-b88d-8ef7e0e7719e`, girl_r64 `9d58ae77-0243-403e-84d3-33799a2f7e70`.
- `get_balance` sonrası: 1167,6 (diğer şeritler eşzamanlı harcıyor, sayı yalnızca bu işe ait değil). Defter: art/ledger/588.md.
- Hız sınırı: iş yuvası "10/10" hatası birkaç çağrıda geldi (ücret yok); yuva açılınca aynı çağrılar bir kez yeniden gönderildi, çift üretim yok.
- Masaüstü kopyası: yerel
- İşleme (512 ile aynı): her karenin en alt opak satırı durma karesinin ayak satırına çekildi; yatay: klibin birleşik kutu ortası durma karesininkine. 64x64'e maskeyle kırpma (ölçekleme yok). Palet: yönün durma karesinin renkleri, en yakın RGB, alfa ikili. Kapanış IoU > 0,93 ise son kare atıldı (23 klipte atıldı, 8 kare 00-07). Ham kareler art/tmp588 altında (commit edilmedi).

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<boy|girl>_r64_<bend|water|sow>_<se|sw|nw|ne>_<00..07>.png`, hepsi 64x64, pivot x=32, pivot y=2 (ALTTAN). Toplam 160 PNG (20 klip x 8 kare).

| Anahtar | Erkek | Kız |
|---|---|---|
| bend | se, sw, nw, ne | se, sw, nw, ne |
| water | nw, ne | se, nw, ne |
| sow | se, sw, nw, ne | se, sw, ne |

Kız bend ne: ham klip 9 kareydi (kapanış IoU 0,88); 08 kare yan kayık görünüyordu, atıldı, 00-07 teslim edildi.

## Teslim edilmeyenler (kareler art/results/588-red/)
| Klip | Neden |
|---|---|
| erkek water se, erkek water sw | Kova hiç çizilmedi, çocuk durma pozunda kalıyor (hareket yok) |
| kız water sw | Kova kalça hizasında küçük ve okunmuyor, su akışı belirsiz |
| kız sow nw | 5. kareden sonra kıyafetle karışan kırmızı-kahverengi bozuk nesne |
| kız bend ne 08 | Yan kayık son kare (yukarıda) |

Kodda bu yönlerde durma karesi + sallantı sürer. Öneri: yeniden deneme ayrı bir işte (ör. `direction_prompts` ile kovanın görünür olduğu bir metin, 588b).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (--kind object --max-colors 32) | 160/160 geçti |
| style.py check --family karakter | 160 GEÇTİ, 0 UYARI, 0 KALDI |
| Ayak satırı | kare başına en alt opak satır durma karesiyle aynı |
| Yan yana sayfası | art/results/588-yanyana.png (2 referans + 6 yeni kare, SE yönü hariç water: boy nw, girl se) |

## Kırpma ve kimlik
- Tuval dışına taşan opak piksel: erkek sow se 2, erkek sow sw 2 (silindi); kız water se 53 (ham 92x104 klip, su sıçraması taşan yan kısım silindi; kova ve çocuk içeride); diğer klipler 0.
- Tepe: kırpılma yok (en üst opak satır erkekte 0-3, kızda 4-21; erkek se water/sow'da 0 = tuvale dayalı, bend sırasında bükülünce iner).
- Kimlik (yüz, saç, giysi rengi): yan yana kontak sayfasına gözle bakıldı, korunmuş görünüyor (erkek kahverengi saç ve keten tunik, kız kızıl örgü, zeytin elbise ve önlük). Otomatik ölçüm yok.
- Okunurluk: bend iyi (eğilme ve toplama belli). Water: kova ve su okunuyor (erkek nw/ne, kız se/nw/ne), su damlası küçük. Sow: kız ve erkekte tohum çok küçük (1-2 piksel), hareket el sallama olarak okunuyor; kullanıcı Unity'de görerek karar vermeli.
- Kova küçük ve çocuk boyuna uygun.

## Sorunlar
- GIF üretilmedi (tarifte istenmişti); yalnızca yan yana sayfası ve ham kontak sayfası (commit edilmedi).
- 4 klip yukarıdaki nedenlerle teslim edilmedi; 588b önerilir.
- `tools/art/save_png.py --all` ZIP'i düz klasöre açtığı için karelerin üzerine yazmadan açamadı (kare adları çakışıyor); ZIP'i `art/tmp` altında Python ile açtım (yapı korunarak). Öneri: `--all` seçeneğine yol koruma.
