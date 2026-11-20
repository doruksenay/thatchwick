unity-import: pending

# 618 Yaşlılar: etkileşim kareleri 4 (mine_dig, mine_bucket, mine_support; 84 px)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, başlangıç ve bitiş karesi = ilgili yönün depodaki durma karesi (`get_upload_url` ile yüklendi; erkekte küreksiz sürüm: se `_v3`, sw/nw/ne `_v2`; kadında düz ad). Eylem metinleri tarifteki gibi, sonuna yaşlı duruşu eklendi. Çağrı başına 1 gen (makbuz `cost: 1 generation`, `cost_est` ile uyumlu).
- Toplam: 24 çağrı x 1 = **24 gen**. 10 eş zamanlı iş sınırı nedeniyle 6 `mine_support` çağrısı "10/10 used" ile reddedildi (ücretsiz), yuva boşalınca aynen yeniden gönderildi (retry istisnası). Hiçbir klip yeniden üretilmedi.
- Karakterler: elder_m_r84 `8dcaede5-cc4d-4068-b56f-ab5e327fea1c`, elder_f_r84 `b7f31e8d-29f2-4f6a-9092-a02759611159`.
- `get_balance` sonrası: 848,6 kalan (diğer şeritler de harcıyor; bu işin payı 24).
- masaüstü kopyası: yerel
- İşleme: ham kare 84x108 ile 120x144 arası; tuval 84x84, ölçekleme yok. Her klipte ham kare 0, durma karesiyle piksel piksel örtüştürüldü (hata 0), aynı kaydırma tüm karelere uygulandı (ayak satırı ve yatay konum durma karesiyle aynı). Palet: karakterin 4 durma karesinin renkleri, en yakın RGB, alfa ikili. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı (8 kare); 5 klip 9 kare (IoU 0,775-0,93).
- Yan yana sayfası: `art/results/618-yanyana.png` (2 referans + 24 klibin 03. karesi). GIF: `art/results/618-elder_<m|f>-<anahtar>.gif` (4 yön yan yana, 2x).

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<elder_m|elder_f>_r84_<mine_dig|mine_bucket|mine_support>_<se|sw|nw|ne>_<NN>.png`, 197 PNG (19 klip x 8 + 5 klip x 9), hepsi 84x84, pivot x=42, pivot y=2 (ALTTAN). Teslim edilmeyen yok.

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (`--kind object --max-colors 32`, 197 dosya) | 197/197 geçti |
| style.py check karakter | GEÇTİ 135, UYARI 62, KALDI 0 (UYARI: "ailesinden soluk", doygunluk x0,4-0,6; aile kilidi kurulu değil) |
| Kare 00 = durma karesi | piksel hatası 0 (24/24 klip) |
| Kimlik | yan yana sayfasında yüz, saç, giysi durma karesiyle tutarlı; yaşlı erkekte kürek yok; kambur duruş korunuyor |

## Sorunlar ve notlar
- **Tuval taşması (görsel onayda dikkat):** 84x84 tuvalin dışına düşen ham piksel: kadın mine_dig SW 650 piksel (9 karede toplam; kazma ucu kırpılıyor olabilir), erkek mine_support NW 76, kadın mine_support SW 22 ve NE 9. Kadın mine_support (ne/se/sw) ve ilgili klipler tepede y=0'a değiyor, üstten kırpılmış olabilir; kırpma yalnızca maskeyle yapıldı, içerik yeniden boyanmadı. Gerekirse tek klip 1 gen ile yeniden üretilir.
- Kuyrukta aynı numarayla iki iş dosyası var (`618-kaya-sirti-ve-kenar.md`, `pending`, ve bu). Bu iş `ART_JOB` ile yürütüldü; kaya sırtı işine dokunulmadı, numara çakışmasını kullanıcı gidermeli.
- `numpy` yok; işleme `art/tmp` betikleriyle yapıldı (`tools/art` değişmedi).
