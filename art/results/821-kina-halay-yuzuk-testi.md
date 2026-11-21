unity-import: pending

# 821 Kına oturuşu, halay_v2, put_ring_v2, throw_jar_v2 (84 px)

- Karakterler: man_r84 `c63fe39d-b168-4732-bf11-b476172c98e8`, woman_r84 `135cb0b1-bd46-4514-8c87-0834b96ef80d`.
- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = aynı yönün rotasyon görseli (`end_frame` tek yön kabul ettiği için çağrı başına 1 yön). 24 çağrı x 1 gen = **24 gen** (tahmin 24, sapma yok). Aynalama yok. En çok 3 eşzamanlı çağrı.
- Hız sınırı: 2 çağrı "need 1 job slots… (20/20)" hatası aldı (hesap genelinde yuvalar doluydu; iş başlamadı, harcama yok), yuva boşalınca aynen yeniden gönderildi.
- `get_balance` sonrası: 9513 (diğer şeritler de harcıyor; yalnız bu işe ait değil).
- İşleme (018c): ham kare 84x112 (bazı kliplerde 84-120 geniş, 112-160 yüksek); ham tuval 84 genişliğe ortalandı, en alt opak satır y=81; ölçekleme yok; palet `iso_char_<cinsiyet>_r84_cand_se.png`, alfa ikili. Kare 0 / son kare IoU > 0,95 ise son kare atıldı. Pivot x=42, y=2 (alttan).
- Yan yana sayfası: `art/results/821-yanyana.png` (2 referans + 12 yeni; her klipten ilk yön, kare 00 ve 03). 748 karşılaştırması bu turda yapılmadı. GIF: `art/results/821-<man|woman>-<klip>.gif`.
- masaüstü kopyası: yerel

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<yön>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). 

| Klip | Erkek | Kadın |
|---|---|---|
| halay_v2 | se 8, sw 8, nw 8, ne 8 | se 8, nw 8, ne 8 (sw YOK) |
| put_ring_v2 | (yok) | se 8, sw 8, nw 8, ne 8 |
| throw_jar_v2 | se 8, sw 9, ne 9 (nw YOK) | nw 9, ne 8 (se, sw YOK) |

## Teslim edilmeyenler
- **henna_sit (kadın, 4 yön): ölçüt tutmadı.** 8 karenin hepsinde karakter ayakta, tabure yok, öne açık avuç yok (el gövde yanında/diz hizasında). Kalça diz hizasında değil. Kareler `art/results/821-red/woman_henna_sit/`, GIF `art/results/821-woman-henna_sit.gif` (yalnız inceleme için). Tek deneme kuralı gereği yeniden üretilmedi. Öneri: `custom_start_frame` olarak oturan bir poz (572 `sit` kareleri) verip yeniden dene.
- **Tuvale (84x84) sığmayan 4 yön:** erkek throw_jar NW (ham 104x160, 21 px taşma), kadın halay SW (3 px), kadın throw_jar SE (1 px), kadın throw_jar SW (15 px). Kırpılmış hâller `art/results/821-red/<cinsiyet>_<klip>_<yön>/`. İş dosyasının "taşan yön teslim edilmez" kuralı gereği teslim edilmedi; 3 yön 1-3 px ile kaçtı, 96x96 tuval kararı bu yönleri kurtarır. Throw_jar için yön eksikleri 748'dekinden (3) farklı ama sayı aynı (4 yön eksik).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (163 dosya) | 163/163 geçti |
| style.py check karakter | KALDI 0, UYARI 22, GEÇTİ 141 (UYARI yalnızca "ailesinden doygun", ör. erkek halay SW x1,29; aile kilidi kurulmadı) |
| Ayak satırı | her karede en alt opak satır y=81 |
| Kare 0 / son kare IoU | 0,822 (erkek throw_jar SW, 9 kare), 0,876 (erkek throw_jar NE, 9 kare), 0,937 (kadın throw_jar NW, 9 kare); öteki 0,967-1,0. Döngü kapanışı 9 kareli yönlerde gözle bakılmalı |
| halay_v2 kollar omuz hizasında yanlara açık | SE karede erkek ve kadında iki kol da yanlara açık görünüyor (yan yana sayfası) |
| put_ring_v2 öne uzanan el | SE'de el öne uzanmış, parmaklar açık (görüldü). Altın yüzük: palette altın tonlu piksel sayısı 25-50 ama deri/saç tonlarıyla karışabilir; yüzük gözle okunmadı, **kullanıcı denetlemeli** |
| throw_jar_v2 4 yönün hepsi | HAYIR (3 yön taştı, bkz. üstte) |
| Tepe kırpılması | teslim edilenlerde yok |

## Sorunlar ve notlar
- İş dosyasının "Doğrulama" ölçütleri kısmen tutmadı: henna_sit tamamen, throw_jar_v2 4 yön eksik, halay kadın SW eksik, yüzük okunurluğu belirsiz. Bu yüzden iş `review`'da ama kabul edilmeden önce eksikler için yeni iş gerekir.
- Betikler `art/tmp/` altında (commit edilmez).
