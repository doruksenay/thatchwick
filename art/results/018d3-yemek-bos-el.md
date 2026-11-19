unity-import: pending

# 018d3 Yemek: boş el + el konumu (eat3, 84 px erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün rotasyon görseli (018d2 ile aynı). Karakterler man_r84 (c63fe39d-...), woman_r84 (135cb0b1-...). `action_description` tarifteki gibi (boş elle, "nothing in the hand"). Tek deneme.
- Maliyet: 8 çağrı x 1 = 8 generation (tahmin 10, tavan 20). Üç çağrı iş yuvası sınırına (10/10) takıldı, para harcanmadı, slot açılınca bir kez yeniden gönderildi. `get_balance` sonrası: 3814,5 (diğer şeritler aynı anda harcıyor).
- İşleme 018d2 ile aynı: kaydırma (ayak satırı y=81, yatay ofset 0), 32 renk palete (cand_se) en yakın RGB, alfa ikili; kırpılan piksel 0. Son kare ile kare 0 IoU > 0,95 (woman nw 0,965, diğerleri 0,997-1,0), 8 yönde son kare atıldı (00-07).
- Yan yana sayfası: art/results/018d3-yanyana.png (2 referans + 4 teslim edilen yönün 00 ve 04. karesi). El konumu sayfası: art/results/018d3-el-konumu.png (her kare 3x3 kırmızı nokta + `item.apple` %35 ölçekte). GIF: art/results/018d3-man-eat3.gif (se, nw, ne), art/results/018d3-woman-eat3.gif (yalnız nw).
- masaüstü kopyası: yerel

## Teslim edilenler ve edilmeyenler
| Karakter | Yön | Teslim | Neden |
|---|---|---|---|
| man | se | evet | baş farkı 2 |
| man | nw | evet | baş farkı 2 |
| man | ne | evet | baş farkı 2 |
| man | sw | HAYIR | baş farkı 3 (>2); art/results/018d3-red/ |
| woman | nw | evet | baş farkı 1 |
| woman | se | HAYIR | karelerde elde kahverengi nesne (ekmek benzeri) var, "elde nesne yok" ölçütü tutmadı (baş farkı 2) |
| woman | ne | HAYIR | aynı: elde kahverengi nesne (baş farkı 1) |
| woman | sw | HAYIR | baş farkı 3 (>2) |

Dosyalar: `Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_eat3_{yön}_{00-07}.png`, 84x84, pivot x=42, pivot y=2 (alttan). 32 PNG (man se, nw, ne; woman nw; her biri 8 kare).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (32 dosya) | 32/32 geçti (84x84, <=32 renk, alt boşluk 2, yarı saydam 0) |
| style.py check karakter | KALDI 0; UYARI yalnızca man se kare 01-07 "ailesinden doygun" (x1,30-1,35, palet %100); diğerleri GEÇTİ. Aile kilidi kurulmadı, `fit` uygulanmadı (018d/018d2 ile aynı) |
| Baş farkı (tepe, <=2 px) | man se 2, nw 2, ne 2, woman nw 1: geçti. man sw 3, woman sw 3: KALDI |
| El ağız hizasında | evet, teslim edilen dört yönde kare 03-05 |
| Elde nesne | teslim edilenlerde yok (yakın görselle bakıldı: yumruk/avuç ten rengi). woman se ve ne'de var |
| Ayak satırı | hepsi y=81 |

## El konumu (`art/results/018d3-hand-anchors.txt`)
- Biçim tarifteki gibi: önek, kare, x (soldan), y (alttan, 83 - satır), gövdeye/başa yaslanan karelerde `b`.
- **Otomatik bulma çalışmadı:** ten rengi bileşenleri baş ve öteki elle karıştı (özellikle nw/ne ve kadın). Konumlar kare kare gözle, ızgaralı 4x büyütmeden işaretlendi (hata payı tahminen ±2 px). Elma bindirmesi sayfasında kontrol edildi; kullanıcı sayfaya bakıp onaylasın.
- Yalnızca teslim edilen 4 set için yazıldı, toplam 32 satır. `b` işareti: man nw ve woman nw kare 03-05 (el başa/saça yaslı). man ne'de el gövdenin önünde, `b` yok.
- Önerilen sonraki adım: eksik yönler (man sw, woman se, ne, sw) için yeni bir iş (4 gen). O zamana kadar oyun bu yönlerde `eat2` ya da ikonsuz geri düşebilir.

## Sorunlar
- 8 yönden 4'ü teslim edilemedi (tablo); tek deneme kuralı gereği yeniden üretilmedi. İşlenmiş kareler art/results/018d3-red/.
- Kimlik kayması otomatik ölçülmedi, gözle tutarlı.
- Öneri: `tools/art`'a el konumu aracı eklenebilir (ten rengi bileşen + izleme); bu işte `art/tmp/` altında denendi, güvenilir çıkmadığı için bırakıldı.
