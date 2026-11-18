unity-import: pending

# 018e Etkileşim animasyonları: social, fun, care, love, fish (84 px, erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün PixelLab döndürme görseli (rotations/<yön>.png). `end_frame` tek yön kabul ettiği için her yön ayrı çağrı (40 çağrı, hepsi paralel gruplar hâlinde). Karakterler `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8) ve `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d); ikisi de `completed`, 8 rotasyon.
- Maliyet: çağrı başına 1 generation (makbuz), toplam 40 (tahmin 40, tavan 52). Tek deneme; hata yok. İş slotu sınırı (10/10) nedeniyle iki çağrı (kadın care NW, NE) reddedildi, ücret yok; slot açılınca bir kez yeniden gönderildi (çift üretim yok).
- `get_balance` sonrası kalan: 4041,7 (diğer şeritler aynı anda harcıyor, sayı yalnızca bu işe ait değil). Defter: art/ledger/018e.md.
- Ham çıktı 84x112, 9 kare (0 = başlangıç). İşleme (014/018d kuralları): yalnızca kaydırma. Her karenin en alt opak satırı durma karesinin (idle_<yön>) en alt satırına (y=81) çekildi; yatay ofset kare 0 ayak merkezinden hesaplandı (çoğu 0; man love nw -2, man love ne -6, woman fish ne -6). Ölçekleme yok. Palet: cinsiyet başına 018d teslimlerinin 32 rengi (cook_*_00 birleşimi), en yakın RGB, alfa ikili.
- Kırpılan opak piksel: yalnızca man fish se'de 9 (üst kenar, olta ucu); diğer 39 klipte 0.
- Kapanış: kare 0 ile son kare IoU > 0,95 ise son kare atıldı (8 kare); değilse 9 kare: man care nw (0,937), man love nw (0,853), man love ne (0,875), woman care nw (0,945), woman love ne (0,940), woman fish nw (0,930). Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: art/results/018e-yanyana.png (iki eski referans + 10 yeni kare, SE yönü). GIF'ler (4 yön yan yana, 2x, 110 ms): art/results/018e-<man|woman>-<anahtar>.gif.
- masaüstü kopyası: yerel

## Dosyalar
Yol: `Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam 326 PNG.

| Anahtar | Erkek (se/sw/nw/ne kare) | Kadın (se/sw/nw/ne kare) |
|---|---|---|
| social | 8/8/8/8 | 8/8/8/8 |
| fun | 8/8/8/8 | 8/8/8/8 |
| care | 8/8/9/8 | 8/8/9/8 |
| love | 8/8/9/9 | 8/8/8/9 |
| fish | 8/8/8/8 | 8/8/9/8 |

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (326 dosya) | 326/326 geçti |
| style.py check karakter | KALDI 0; GEÇTİ 256, UYARI 70. UYARI yalnızca erkek SE/SW karelerinde "ailesinden doygun" (x1,27-1,40, palet yakınlığı %100); kadın ve erkek NE/NW hepsi GEÇTİ. Aile kilidi kurulmadı, palet uyumu uyarı düzeyinde; `fit` uygulanmadı (018d ile aynı). |
| Tepe kırpılması | yalnızca man fish se (9 px, olta ucu); kadın fun se tepe y=0 (kenara değiyor), diğerleri 1-4 |
| Palet dışı oran | %0 |
| Ayak satırı | kare başına en alt opak satır y=81 (durma karesiyle aynı) |
| Ardışık IoU en düşük | man love ne 0,71; man love se 0,77; man fish se 0,77; man fun sw 0,78; man social sw 0,79; diğerleri >= 0,80 |

## Sorunlar ve notlar
- **Hareketsiz klipler:** woman fish se, woman fish sw, man fish sw ardışık kare IoU'su 1,00 (kareler birbirinin aynısı, hareket yok); woman care sw 1,00, woman care ne 0,99 (neredeyse hareketsiz). Bu klipler fiilen duran poz; kullanıcı GIF'ten karar versin, gerekirse yeniden üretim işi açılır.
- **Nesne okunurluğu (yan yana sayfasından gözle):** kadın care'de bebek bohçası okunuyor. Erkek care'de bebek/bohça belirgin değil (kollar önde duruyor). Erkek fish'te olta çubuğu okunmuyor (kare 04'te yalnızca küçük bir ucu var); kadın fish'te de olta küçük. Kullanıcı GIF'ten teyit etsin.
- fun hop: ayak satırı sabit kuralı gereği (her karede en alt satır y=81) zıplama yüksekliği silinir, kollar ve gövde hareketi kalır.
- Dokuz karelik klipler (care/love/fish'in bazı yönleri): kapanış IoU eşiği altında; döngüye girerken 08'den 00'a geçiş için kullanıcı GIF'e baksın.
- Kimlik (yüz, giysi rengi) yalnızca yan yana sayfasında gözle bakıldı: korunmuş görünüyor; otomatik ölçüm yok.
- Öneri: `tools/art` altında toplu `validate_iso.py` (çoklu dosya) ve klip işleme betiği olsaydı bulut şeridinde tekrar yazmak gerekmezdi; işleme betikleri art/tmp altında kaldı (git'e girmez).
