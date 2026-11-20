unity-import: pending

# 617 Yaşlılar: etkileşim kareleri 3 (craft_hammer, craft_hand, love, fun, fish; 84 px)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, başlangıç ve bitiş karesi = ilgili yönün depodaki durma karesi (`get_upload_url` ile yüklendi; erkekte küreksiz sürüm: se `_v3`, sw/nw/ne `_v2`; kadında düz ad). Eylem metinleri tarifteki gibi, sonuna yaşlı duruşu eklendi. Çağrı başına 1 gen (makbuz `cost: 1 generation`, `cost_est` ile uyumlu).
- Toplam: 40 çağrı x 1 = **40 gen** (5 anahtar x 2 karakter x 4 yön). PixelLab'in 10 eş zamanlı iş sınırı diğer şeritlerle paylaşımlıydı; "10/10 used" ile reddedilen çağrılar ücretsizdi, yuva boşalınca aynı çağrı yeniden gönderildi (retry istisnası). Hiçbir klip yeniden üretilmedi.
- Karakterler: elder_m_r84 `8dcaede5-cc4d-4068-b56f-ab5e327fea1c`, elder_f_r84 `b7f31e8d-29f2-4f6a-9092-a02759611159`.
- `get_balance` sonrası: 918,6 kalan (diğer şeritler de harcıyor; bu işin payı 40).
- masaüstü kopyası: yerel
- İşleme: ham kare 84x108-112 (9 kare), ZIP'ten indirildi; tuval 84x84, ölçekleme yok, kırpma yok (tek klipte, erkek craft_hammer SW, 1 ham piksel tuval dışına düştü, kayda değmez). Ayak satırı ve yatay konum durma karesiyle aynı (kare 00 durma karesiyle birebir örtüşüyor, IoU 1,0). Palet: karakterin 4 durma karesinin 32 rengi, en yakın RGB, alfa ikili. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı (8 kare); yalnızca erkek love NW (IoU 0,865) 9 kare.
- Yan yana sayfası: `art/results/617-yanyana.png` (2 referans + 40 klibin 03. karesi). GIF: `art/results/617-elder_<m|f>-<anahtar>.gif` (4 yön yan yana, 2x).

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<elder_m|elder_f>_r84_<craft_hammer|craft_hand|love|fun|fish>_<se|sw|nw|ne>_<NN>.png`, 321 PNG (39 klip x 8 kare + 1 klip x 9), hepsi 84x84, pivot x=42, pivot y=2 (ALTTAN). Teslim edilmeyen yok. Not: dosya adlarındaki anahtar `elder_m`/`elder_f` (iş dosyasındaki yol kalıbı).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (`--kind object --max-colors 32`, 321 dosya) | 321/321 geçti |
| style.py check karakter | GEÇTİ 246, UYARI 75, KALDI 0 (UYARI: "ailesinden soluk", özellikle kadın craft_hammer/craft_hand NW doygunluk x0,42-0,58; aile kilidi kurulu değil) |
| Kare 00 = durma karesi | IoU 1,0 (örneklenen 6 klip) |
| Kare 0 ile son kare IoU | 0,865-1,0 |
| Tepe kırpılması | yok (en küçük tepe y: 0, kadın fun NE) |
| Kimlik | yüz, saç, giysi gözle durma karesiyle tutarlı (önizleme 3x); yaşlı erkekte kürek yok; duruş kambur kalıyor; gençleşme yok |

## Sorunlar ve notlar
- **Okunurluk zayıf klipler (görsel onayda dikkat):** erkek craft_hammer SE (çekiç görünmüyor, el hareketi belirsiz), erkek fish SE/NE (olta çok ince ya da yok), erkek love/fun ve kadın love poz değişimi küçük. Kadın craft_hammer SE'de çekiç okunuyor. Bu klipler yeniden üretilirse tek anahtar için 8 gen.
- **love**: sevgi sarılması partnersiz üretildiğinde kollar kendine dolanıyor gibi okunuyor (kadın SE). Kod partneri ayrı çiziyorsa sorun değil.
- `numpy` yok; işleme `art/tmp` betikleriyle yapıldı (`tools/art` değişmedi).
- Not: kuyrukta aynı numarayla iki iş dosyası var (`617-gecis-cim-su.md` ve bu). Bu iş `ART_JOB` ile yürütüldü; çakışmayı kullanıcı gidermeli.
