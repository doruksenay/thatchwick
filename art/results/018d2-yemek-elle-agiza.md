unity-import: pending

# 018d2 Yemek: eliyle ağzına götürür (eat2, 84 px erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün rotasyon görseli (018d ile aynı). `action_description` tarifteki gibi ("head stays still and level, raises a piece of bread ... head does not lean forward"). Karakterler: man_r84 (c63fe39d-...), woman_r84 (135cb0b1-...). Tek deneme, hata yok.
- Maliyet: 8 çağrı x 1 = 8 generation (tahmin 10, tavan 20). `get_balance` sonrası: 4117,9 (diğer şeritler aynı anda harcıyor).
- Ham çıktı 84x112, 9 kare. İşleme 018d ile aynı: yalnızca kaydırma (ayak satırı y=81, yatay ofset 0 hepsinde), 32 renk palete (cand_se) en yakın RGB, alfa ikili; kırpılan piksel 0. Kapanış: son kare ile kare 0 IoU > 0,95 olduğundan 8 yönün hepsinde son kare atıldı (00-07).
- Yan yana sayfası: art/results/018d2-yanyana.png (2 referans + her yön için kare 00 ve 04). GIF'ler (4 yön yan yana, 2x): art/results/018d2-man-eat2.gif (SW reddedilen sürümle), art/results/018d2-woman-eat2.gif. 018d `eat` GIF'i ile karşılaştırma: art/results/018d-man-eat.gif, art/results/018d-woman-eat.gif.
- Elin ağız hizasına çıktığı kare: kare 04 (erkek SE/SW/NE/NW ve kadın SE/SW/NE/NW'de kare 03-05 civarı); yan yana sayfasında her yönün 04. karesi gösteriliyor.
- masaüstü kopyası: yerel

## Dosyalar
Yol: `Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_eat2_<se|sw|nw|ne>_<NN>.png`, 84x84, pivot x=42, pivot y=2 (alttan). Toplam 56 PNG (7 yön x 8 kare).

| Karakter | Yön | Kare | Teslim |
|---|---|---|---|
| man | se, nw, ne | 8 | evet |
| man | sw | 8 | HAYIR (baş ölçütü), art/results/018d2-red/ |
| woman | se, sw, nw, ne | 8 | evet |

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (56 dosya) | 56/56 geçti (84x84, <=32 renk, alt boşluk 2, yarı saydam 0) |
| style.py check karakter | KALDI 0; UYARI 7: yalnızca man SE kare 01-07 "ailesinden doygun" (x1,32-1,35, palet yakınlığı %100). Diğerleri GEÇTİ. Aile kilidi kurulmadı; `fit` uygulanmadı (018d ile aynı uyarı). |
| Ayak satırı | hepsi y=81 |
| Baş ölçütü (kareler arası tepe farkı, bağlayıcı <=2 px) | man se 2 (tepe 2-4), man nw 1, man ne 1, woman se 0, woman sw 2, woman nw 0, woman ne 0: GEÇTİ. **man sw 5 (tepe 1-6): KALDI** |
| Elin ağza çıktığı kare | var (kare 03-05; sayfada kare 04) |
| Tepe kırpılması | yok (en küçük tepe y: 0, woman sw kare 03-05; saç/baş üst kenara değiyor, kırpılma yok) |

## Sorunlar ve notlar
- **man sw teslim edilmedi:** el ağza çıkarken kare 05-07'de baş 4-5 px aşağı kayıyor (kare 04 tepe y=1, kare 05-06 tepe y=6); bağlayıcı baş ölçütü (<=2 px) tutmuyor. İşlenmiş kareler art/results/018d2-red/ altında. Tek deneme kuralı gereği yeniden üretilmedi; kullanıcı isterse yalnızca bu yön için yeni iş (1 gen) yazılabilir. Şimdilik oyunda man sw için 018d `eat` kullanılabilir (kullanıcı onayı beklenir) ya da yön boş kalır.
- Kimlik kayması (yüz, giysi) otomatik ölçülmedi; kareler gözle bakıldığında erkek ve kadında tutarlı, ekmek parçası küçük ama el ağız hizasında okunuyor. Kullanıcı GIF'ten teyit etsin.
- Man SE doygunluk uyarısı x1,3: 018d'deki ile aynı.
- Ham indirmede bir kare (man se 5) geçici HTTP 500 verdi, aynı URL yeniden indirildi (üretim tekrarı yok).
