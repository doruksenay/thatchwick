unity-import: pending

# 614 Yaşlılar: easter egg sahne kareleri (topaç, uluma, yerden alma)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `custom_start_frame_url` ve `end_frame_url` = ilgili yönün depodaki durma karesi (`get_upload_url` ile yüklendi; çağrı tek yön kabul ettiği için yön başına tek çağrı). Yaşlı erkek: küreksiz durma karesi (se `_v3`, sw/nw/ne `_v2`); yaşlı kadın: düz durma karesi. Karakterler `elder_m_r84` (8dcaede5-…) ve `elder_f_r84` (b7f31e8d-…), `get_character` ile `completed` doğrulandı. Eylem metinleri tarifteki gibi, sonuna `, an elderly person moving slowly and carefully, slightly stooped` eklendi.
- Maliyet: çağrı başına 1 generation (makbuz), toplam 24 (tahmin 24, tavan 34). Tek deneme, hata yok. İş slotu sınırı (10/10, şeritler paylaşıyor) yüzünden bazı çağrılar ücretsiz reddedildi, slot açılınca bir kez yeniden gönderildi (çift üretim yok).
- `get_balance` sonrası: 1058,60 (diğer şeritler aynı anda harcıyor). Defter: art/ledger/614.md.
- Ham çıktı 84x108/112, 9 kare. İşleme (511 ile aynı mantık): her karede en alt opak satır durma karesinin ayak satırına çekildi; yatay ofset kare 0 ayak merkezinden (tek tamsayı, klip başına). Ölçekleme yok, 84x84 tuval, kırpılan piksel 0. Palet: karakterin 4 durma karesinin 32 rengi, en yakın RGB, alfa ikili. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: art/results/614-yanyana.png (iki referans + SE yönü: erkek pickup, kadın pickup, erkek top, kadın howl). GIF'ler (4 yön yan yana, 2x, 110 ms): art/results/614-elder_(m|f)-(pickup|top|howl).gif.
- masaüstü kopyası: yerel

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_(elder_m|elder_f)_r84_(top|howl|pickup)_(se|sw|nw|ne)_NN.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam 194 PNG (8 ya da 9 kare).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso --kind object --max-colors 32 | 194/194 geçti |
| style.py check karakter | KALDI 0; GEÇTİ 166, UYARI 28 (yalnızca kadın NE/NW karelerinde "ailesinden soluk", x0,39-0,58). Aile kilidi kurulmadı; `fit` uygulanmadı |
| Ayak satırı | kare başına en alt opak satır = durma karesi ayak satırı |
| Eylem okunurluğu (yan yana sayfası SE, gözle) | pickup: eğilip nesneyi alıp doğrulma; top: eğik duruş, kamçı ipi; howl: eller ağızda, baş geride. Diğer yönler GIF'ten kullanıcı bakışına bırakıldı |
| Kimlik / kambur duruş | durma karesinin paleti ve giysisi korunuyor görünüyor, otomatik ölçüm yok; yaşlı erkekte kürek görünmüyor (SE'de bakıldı) |

### Klip tablosu
| Kişi | Anahtar | Yön | Kare | IoU(0,son) | Kapanış karesi atıldı | En düşük ardışık IoU | x ofset |
|---|---|---|---|---|---|---|---|
| f | howl | ne | 8 | 1.0 | evet | 0.88 | 0 |
| f | howl | nw | 9 | 0.896 | hayır | 0.77 | 0 |
| f | howl | se | 8 | 0.99 | evet | 0.79 | 0 |
| f | howl | sw | 8 | 1.0 | evet | 0.8 | 0 |
| f | pickup | ne | 8 | 1.0 | evet | 0.81 | 0 |
| f | pickup | nw | 8 | 1.0 | evet | 0.83 | 0 |
| f | pickup | se | 8 | 1.0 | evet | 0.71 | 0 |
| f | pickup | sw | 8 | 1.0 | evet | 0.78 | 0 |
| f | top | ne | 8 | 1.0 | evet | 0.82 | -4 |
| f | top | nw | 8 | 0.988 | evet | 0.96 | 0 |
| f | top | se | 8 | 1.0 | evet | 0.78 | -4 |
| f | top | sw | 8 | 1.0 | evet | 0.75 | -6 |
| m | howl | ne | 8 | 1.0 | evet | 0.67 | 0 |
| m | howl | nw | 9 | 0.743 | hayır | 0.7 | 0 |
| m | howl | se | 8 | 1.0 | evet | 0.72 | 0 |
| m | howl | sw | 8 | 0.999 | evet | 0.71 | 0 |
| m | pickup | ne | 8 | 0.998 | evet | 0.72 | 0 |
| m | pickup | nw | 8 | 1.0 | evet | 0.74 | 0 |
| m | pickup | se | 8 | 0.986 | evet | 0.69 | 0 |
| m | pickup | sw | 8 | 0.999 | evet | 0.73 | 0 |
| m | top | ne | 8 | 1.0 | evet | 0.79 | -6 |
| m | top | nw | 8 | 1.0 | evet | 0.89 | 0 |
| m | top | se | 8 | 0.963 | evet | 0.7 | -4 |
| m | top | sw | 8 | 0.998 | evet | 0.73 | 0 |

## Sorunlar ve notlar
- Dönüşlü iki klip 9 kareli (f howl nw, m howl nw): kapanış IoU eşiğinin altında; döngüye girerken son kareden 00'a geçiş sert olabilir (m howl nw 0,74).
- Topaç, eğilme ve nesne karelerinin tamamı yalnızca SE yönünde gözle bakıldı; diğer yönler GIF'e bırakıldı. "En az iki karede el yere yakın" ve "tepe kırpılmaz" ölçütleri otomatik ölçülmedi (kırpılan piksel 0, yani tepe kırpılmadı).
- Aynı ad üstüne bir sonraki 614 işi (geçiş çim-kum, aynı numara) ayrı dosyadır; bu rapor yalnızca yaşlı işine aittir.
- Öneri: `validate_iso.py` birden çok dosya kabul etsin; 194 dosya için tek tek çağrıldı.
