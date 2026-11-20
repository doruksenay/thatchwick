unity-import: pending

# 511 Easter egg sahne kareleri: topaç (top), uluma (howl), yerden alma (pickup), erkek ve kadın 84 px

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün PixelLab döndürme görseli (018e ile aynı; çağrı tek yön kabul ettiği için yön başına tek çağrı). Karakterler man_r84 (c63fe39d-b168-4732-bf11-b476172c98e8) ve woman_r84 (135cb0b1-bd46-4514-8c87-0834b96ef80d). Eylem metinleri tarifteki gibi.
- Maliyet: çağrı başına 1 generation (makbuz), toplam 24 (tahmin 24, tavan 34). Tek deneme, hata yok. İş slotu sınırı (10/10, şeritler paylaşıyor) yüzünden bazı çağrılar ücretsiz reddedildi ve slot açılınca bir kez yeniden gönderildi (çift üretim yok).
- `get_balance` sonrası: 2266,70 (diğer şeritler aynı anda harcıyor). Defter: art/ledger/511.md.
- Ham çıktı 84x112, 9 kare. İşleme (018e ile aynı): her kare bütün olarak dikey kaydırıldı, en alt opak satır durma karesinin (idle_<yön>) ayak satırına çekildi; yatay ofset kare 0 ayak merkezinden (man top sw -6, man top nw -10, diğerleri 0). Ölçekleme yok. Palet: cinsiyet başına cook_*_00 teslimlerinin 32 rengi, en yakın RGB, alfa ikili. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: art/results/511-yanyana.png (iki eski referans + SE yönü, her klibin 00/02/04/06 kareleri). GIF'ler (4 yön yan yana, 2x, 110 ms): art/results/511-(man|woman)-(top|howl|pickup).gif.
- masaüstü kopyası: yerel

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_(man|woman)_r84_(top|howl|pickup)_(se|sw|nw|ne)_NN.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam 199 PNG (8 ya da 9 kare; tabloya bak).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso --kind object --max-colors 32 | 199/199 geçti |
| style.py check karakter | KALDI 0; GEÇTİ 141, UYARI 58 (yalnızca erkek karelerinde "ailesinden doygun", x1,25-1,34; kadın hepsi GEÇTİ). Aile kilidi kurulmadı; `fit` uygulanmadı (018d/018e ile aynı) |
| Ayak satırı | kare başına en alt opak satır = durma karesi ayak satırı |
| Eylem okunurluğu (yan yana sayfası, SE, gözle) | pickup: gövde eğik, el yerde, nesne alınıp doğruluyor (erkek ve kadın); howl: eller ağızda, baş geride; top: topaç ayak hizasında yerde. Diğer yönler GIF'ten kullanıcı bakışına bırakıldı |
| Kimlik | yüz ve giysi rengi SE'de korunmuş görünüyor; otomatik ölçüm yok |

### Klip tablosu

| Kişi | Anahtar | Yön | Kare | IoU(0,son) | Kapanış karesi atıldı | En düşük ardışık IoU | Kırpılan piksel |
|---|---|---|---|---|---|---|---|
| man | top | se | 8 | 1.0 | evet | 0.57 | 0 |
| man | howl | se | 9 | 0.933 | hayır | 0.69 | 0 |
| man | pickup | se | 8 | 0.999 | evet | 0.55 | 78 |
| man | top | sw | 8 | 0.997 | evet | 0.57 | 0 |
| man | howl | sw | 9 | 0.84 | hayır | 0.7 | 0 |
| man | pickup | sw | 8 | 0.981 | evet | 0.67 | 0 |
| man | top | nw | 8 | 1.0 | evet | 0.81 | 0 |
| man | howl | nw | 9 | 0.707 | hayır | 0.71 | 0 |
| man | pickup | nw | 8 | 0.989 | evet | 0.74 | 0 |
| man | top | ne | 8 | 0.997 | evet | 0.7 | 0 |
| man | howl | ne | 9 | 0.722 | hayır | 0.72 | 0 |
| man | pickup | ne | 8 | 0.999 | evet | 0.61 | 0 |
| woman | top | se | 8 | 0.999 | evet | 0.68 | 0 |
| woman | howl | se | 8 | 0.999 | evet | 0.79 | 0 |
| woman | pickup | se | 9 | 0.949 | hayır | 0.63 | 0 |
| woman | top | sw | 9 | 0.783 | hayır | 0.76 | 0 |
| woman | howl | sw | 8 | 1.0 | evet | 0.79 | 0 |
| woman | pickup | sw | 8 | 1.0 | evet | 0.61 | 0 |
| woman | top | nw | 8 | 0.999 | evet | 0.76 | 0 |
| woman | howl | nw | 8 | 0.999 | evet | 0.88 | 0 |
| woman | pickup | nw | 8 | 1.0 | evet | 0.77 | 0 |
| woman | top | ne | 8 | 0.961 | evet | 0.74 | 0 |
| woman | howl | ne | 9 | 0.945 | hayır | 0.72 | 0 |
| woman | pickup | ne | 8 | 0.998 | evet | 0.69 | 0 |

## Sorunlar ve notlar
- man pickup se: ayak satırı sabit kuralı yüzünden 78 opak piksel tuval dışında kaldı; kullanıcı GIF'ten kırpılmaya baksın. Diğer 23 klipte kırpılan piksel 0.
- pickup'ta eğilirken karakter ayak satırına oturur; yerdeki nesne bazı karelerde ayak satırının altına inebilir (kırpılmadı).
- 9 kareli klipler (tabloda "hayır" olanlar) kapanış IoU eşiğinin altında: döngüye girerken son kareden 00'a geçiş sert olabilir; kullanıcı GIF'e baksın.
- En düşük ardışık IoU 0,55 (man pickup se); hareketsiz klip yok.
- Ham indirme: ZIP `save_png.py --all` ile düzleştiği için çakıştı; yapı korunarak art/tmp/dl511.py ile açıldı (git'e girmez). Öneri: save_png.py'ye `--keep-tree` seçeneği.
