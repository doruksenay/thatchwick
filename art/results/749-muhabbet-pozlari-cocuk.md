unity-import: pending

# 749 Muhabbet pozları, çocuk: laugh, giggle, listen_nod, gasp, clap, tell_story (48/48 klip)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = yönün durma karesi (`get_upload_url` ile yüklendi, 64x64). Metnin başına yön eklendi (`facing south-east: ...`). 48 çağrı (2 karakter x 6 anahtar x 4 çapraz yön), çağrı başına 1 generation (makbuz); toplam 48 (tahmin 48, tavan 60). Tek deneme, tüm klipler üretildi.
- Karakterler: boy_r64 `6940a34e-75c4-4cb6-b88d-8ef7e0e7719e`, girl_r64 `9d58ae77-0243-403e-84d3-33799a2f7e70`.
- `get_balance` sonrası: 467,6 (diğer şeritler eşzamanlı harcıyor, sayı yalnızca bu işe ait değil). Defter: art/ledger/749.md.
- Hız sınırı: iş yuvası "10/10" hatası ve bir kez "rate limit exceeded" geldi (ücret yok); yuva açılınca aynı çağrılar bir kez yeniden gönderildi, çift üretim yok.
- Masaüstü kopyası: yerel
- İndirme: karakter ZIP'i (`/mcp/characters/<id>/download`) Python ile `art/tmp749` altına açıldı (commit edilmedi).

## İşleme (588 ile aynı, bir fark)
- Ham klip 64x84-92 tuval, 9 kare. Kapanış IoU > 0,93 ise son kare atıldı (boy laugh ne, girl laugh ne, girl giggle se, girl clap se); diğer döngülerde 9. kare atıldı (00-07). `gasp` kısa/döngüsüz: 9 karenin hepsi (00-08) teslim.
- Yatay: klibin birleşik kutu ortası durma karesininkine. Dikey: yere basan (klip medyanına ±2 px) kareler durma karesinin ayak satırına (y=63) oturtuldu; havada kalan kareler (kıpırdama/zıplama: laugh, clap) klibin ortak kaymasını aldı, böylece zıplama silinmedi. (Tarifteki "her kareyi ayak satırına" kuralından bilinçli sapma; zıplama pozlarını korumak için.)
- 64x64'e maskeyle kırpma (ölçekleme yok); palet: yönün durma karesinin renkleri (<=32), en yakın RGB, alfa ikili.

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<boy|girl>_r64_<laugh|giggle|listen_nod|gasp|clap|tell_story>_<se|sw|nw|ne>_<NN>.png`, hepsi 64x64, pivot x=32, pivot y=2 (ALTTAN). 392 PNG: 40 klip x 8 kare (00-07) + 8 gasp klibi x 9 kare (00-08). 48 klibin hepsi teslim edildi.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (--kind object --max-colors 32) | 392/392 geçti |
| style.py check --family karakter | 392 GEÇTİ, 0 UYARI, 0 KALDI |
| Ayak satırı | yere basan kareler durma karesiyle aynı satırda (y=63) |
| Yan yana sayfası | art/results/749-yanyana.png (2 referans + 12 yeni kare, SE yönü, anahtar başına boy ve girl) |
| GIF | klip başına art/results/749-gif/<boy|girl>_<anahtar>_<yön>.gif (48 adet, 110 ms, 2x) |

## Kırpma ve kimlik
- Tuval dışına taşan opak piksel (silindi, gövde içeride): boy laugh se 22, laugh sw 15, giggle se 8, listen_nod se 2, clap se 9, clap sw 6, tell_story se 12, tell_story sw 6; girl clap ne 7; diğer klipler 0.
- Tepe: erkek SE/SW klipleri tuvale dayalı (en üst satır 0, kırpılma olabilir: yukarıdaki piksel sayıları), diğerleri 1-6 satır boşluk.
- Kimlik (yüz, saç, giysi rengi): kontak sayfasına gözle bakıldı, korunmuş (erkek kahverengi saç + keten tunik; kız kızıl örgü + zeytin elbise ve önlük). Otomatik ölçüm yok.
- Okunurluk: laugh (karına el, açık ağız), giggle (iki el ağızda), gasp (eller yanaklarda), clap, tell_story (geniş kol hareketi) net okunuyor. `listen_nod` ince: baş sallama küçük hareket, yüz ifadesi ve duruş değişimi az; kullanıcı Unity'de görerek karar vermeli. Clap'te el çırpma küçük (1-3 px) ama zıplama okunuyor.
- Kuzey (NW/NE) yönlerinde yüz görünmediği için ifade (kahkaha, kıkırdama) arkadan yalnızca omuz/kol hareketiyle okunuyor.

## Sorunlar
- `tools/art/save_png.py` kullanılmadı (ZIP'i Python ile `art/tmp749` altına açtım); yapı korunarak açma gerekti (588'deki öneri geçerli).
