unity-import: pending

# 741 Muhabbet pozları 2: tell_story, gossip, gasp, clap, scoff, think (84 px, erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün PixelLab döndürme görseli. Karakterler `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8) ve `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d). `end_frame` tek yön aldığı için 48 ayrı çağrı (6 anahtar x 2 karakter x 4 yön), 1 generation/çağrı = **48 gen** (tahmin 48, tavan 60). Tek deneme, hata yok.
- Yuva sınırı: iş slotu (10/10) diğer şeritlerce sürekli doluydu; "need 1 job slots" reddi ücretsiz, slot açılınca aynı çağrı yeniden gönderildi (çift üretim yok).
- `get_balance` sonrası kalan: 602,6 (diğer şeritler aynı anda harcıyor). Defter: art/ledger/741.md.
- İşleme (018e ile aynı): ham 84x112 (bazı klipte 88x116 / 84x124) -> 84x84; her karenin en alt opak satırı idle_<yön>_00 ile aynı y=81'e kaydırıldı, yatay ofset kare 0 ayak merkezinden (çoğu 0; man tell_story se/sw ve man scoff se -2). Ölçekleme yok, kırpılan opak piksel 0. Palet: cinsiyet başına 018d `cook_*_00` birleşimi 32 renk, en yakın RGB, alfa ikili. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı.
- Kapanış IoU eşiği altı (9 kare): man think se (0,934), man think sw (0,913), woman clap ne (0,937). Diğer 45 klip 8 kare.
- Yan yana sayfası: art/results/741-yanyana.png (4 referans + SE yönü örnek kareleri). GIF'ler (4 yön yan yana, 2x, 110 ms): art/results/741-<man|woman>-<anahtar>.gif.
- Masaüstü kopyası: yerel

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam 387 PNG (45 klip x 8 + 3 klip x 9).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (387 dosya) | 387/387 geçti |
| style.py check karakter | KALDI 0; GEÇTİ 274, UYARI 113. UYARI yalnızca erkek karelerinde "ailesinden doygun" (x1,25-1,38, palet yakınlığı %100); kadın hepsi GEÇTİ. Aile kilidi kurulmadı; `fit` uygulanmadı (018d/018e ile aynı). |
| Palet dışı oran | %0 |
| Ayak satırı | kare başına en alt opak satır y=81 |
| Tepe | woman gasp ne üst satır y=0 (kenara değiyor); diğerleri 1-5 |
| Ardışık IoU en düşük | man gossip ne 0,75; man gossip nw 0,77; woman gasp nw 0,77; man gossip se 0,79; diğerleri >= 0,80 |

## Sorunlar ve notlar
- Eylem okunurluğu yalnızca yan yana sayfası ve GIF üzerinden gözle bakıldı (kimlik ve giysi rengi korunmuş görünüyor); otomatik ölçüm yok. Küçük el/ağız hareketlerinin (gasp ağız açma, gossip el) okunurluğunu kullanıcı GIF'ten teyit etsin.
- Ayak satırı sabit kuralı yüzünden gasp'taki geriye yaslanma ve gossip'teki öne eğilme gövde hareketiyle kalır, dikey kayma yok.
- Ham çıktı art/tmp741 altında, git'e girmedi. İşleme betikleri art/tmp altında (git'te yok).
