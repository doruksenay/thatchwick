unity-import: pending

# 615 Yaşlılar: etkileşim kareleri 1 (eat2, eat3, cook, social, care; 84 px)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, başlangıç ve bitiş karesi = ilgili yönün depodaki durma karesi (`get_upload_url` ile yüklendi; erkekte küreksiz sürüm: se `_v3`, sw/nw/ne `_v2`; kadında düz ad). Eylem metinleri tarifteki gibi, sonuna yaşlı duruşu eklendi. Çağrı başına 1 gen (aracın makbuzu `cost: 1 generation`, `cost_est` ile uyumlu).
- Toplam: 40 çağrı x 1 = **40 gen** (5 anahtar x 2 karakter x 4 yön). PixelLab hesap başına 10 eş zamanlı işe izin veriyor (diğer şeritlerle paylaşımlı); "10/10 used" ile reddedilen çağrılar ücretsiz, `wait_for_jobs` sonrası aynı çağrı yeniden gönderildi (retry istisnası). Üretilen hiçbir klip yeniden üretilmedi.
- Karakterler: elder_m_r84 `8dcaede5-cc4d-4068-b56f-ab5e327fea1c`, elder_f_r84 `b7f31e8d-29f2-4f6a-9092-a02759611159` (completed).
- `get_balance` sonrası: 1058,6 kalan (diğer şeritler de harcıyor; bu işin payı 40).
- masaüstü kopyası: yerel
- İşleme: ham kare 84x108-112, tuval 84x84, ölçekleme yok, kırpma yok (hiçbir karede opak piksel tuval dışına taşmadı). Ayak satırı durma karesiyle aynı (y=81), klip başına tek yatay ofset (kare 0 kutu ortası = durma karesi kutu ortası), palet: karakterin 4 durma karesinin 32 rengi, en yakın RGB, alfa ikili. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 (kadın care NW, kadın eat2 NW, erkek cook SE: 9 kare). Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: `art/results/615-yanyana.png` (2 referans + 40 klibin 03. karesi). GIF: `art/results/615-elder_<m|f>-<anahtar>.gif` (4 yön yan yana, 2x). El konumu (eat3): `art/results/615-hand-anchors.txt`.

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<elder_m|elder_f>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, 323 PNG, hepsi 84x84, pivot x=42, pivot y=2 (alttan). 37 klip 8 kare, 3 klip 9 kare. Teslim edilmeyen yok.

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (`--kind object --max-colors 32`, 323 dosya) | 323/323 geçti |
| style.py check karakter | GEÇTİ 251, UYARI 72, KALDI 0 (UYARI doygunluk/palet; aile kilidi kurulu değil) |
| Ayak satırı | her karede en alt opak satır y=81 |
| Kare 0 ile son kare IoU | 0,927-1,0 |
| Tepe kırpılması | yok (en küçük tepe y: 1) |
| Kimlik | yüz, saç, giysi gözle durma karesiyle tutarlı; yaşlı erkekte kürek yok; duruş kambur kalıyor |

## Sorunlar ve notlar
- **care**: kucaktaki bebek/bohça pek okunmuyor; SE/NW karelerde poz idle'a yakın, hareket belirsiz. Görsel onayda dikkat edilmeli (reddedilirse tek anahtar için 8 gen).
- **cook**: kaşık görünüyor, kazan (tarifte yok) bazı karelerde belirsiz; erkek SE karelerinde kaşık ucunda dağınık birkaç piksel.
- **eat2/eat3**: baş sabit, el ağza çıkıyor; eat3'te eldeki nesne yok.
- **El konumu** (`615-hand-anchors.txt`, 64 satır): biçim `<kare öneki> <kare> <x soldan> <y alttan>`. **Otomatik ve doğrulanmamış**: durma karesine göre yeni ten rengi piksellerin merkezi; baş ve öteki elle karışabilir (018d3'te de otomatik yöntem güvenilmezdi). Kare 00 (durma pozu) `? ?` yazıldı (8 satır). Kullanıcı gözle kontrol etsin; gerekirse ayrı işle elle işaretlenir.
- `numpy` bulut ortamında yoktu; işleme `uv run --with pillow --with numpy` ile `art/tmp` betikleriyle yapıldı (`tools/art` değişmedi). Öneri: kare işleme (kaydırma, palet, IoU) için `tools/art`'a ortak araç eklenebilir.
