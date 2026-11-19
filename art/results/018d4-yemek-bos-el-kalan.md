unity-import: pending

# 018d4 Boş elle yeme, kalan 4 yön (eat3: man sw, woman se, ne, sw)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün rotasyon görseli (018d3 ile aynı). Karakterler man_r84 (c63fe39d-...), woman_r84 (135cb0b1-...).
- Tarif: 018d3 + "open empty hand, fingers slightly curled, the hand is completely empty, no bread, no food, no object" (kadın). Erkek ve kadın sw'de "head and shoulders stay perfectly still and level". Negatif tarif bu araçta yok.
- Maliyet: 5 çağrı x 1 = 5 generation (tahmin 10, tavan 20). Man sw ilk denemede baş farkı 3 (>2) olduğundan ikinci deneme yapıldı (yön başına en çok 2): "standing upright like a statue ... only the forearm moves"; baş farkı 2, geçti. Hız sınırı hatası olmadı. `get_balance` sonrası: 3559,5 (paralel şeritler dahil).
- İşleme 018d3 ile aynı: kare başına ayak satırı y=81'e kaydırma, 32 renk palete (iso_char_<ch>_r84_cand_se) en yakın RGB, alfa ikili; kırpılan piksel 0. Son kare ile kare 0 IoU > 0,95 (woman se 0,982, diğerleri 0,999-1,0), son kare atıldı (00-07). Ham çıktı 84x116.
- Yan yana sayfası: art/results/018d4-yanyana.png (2 referans + 4 yönün 04. karesi). El konumu sayfası: art/results/018d4-el-konumu.png (kırmızı nokta + `icon_item_apple` %35 ölçek; `b` karelerde elma gövdenin arkasında). GIF: art/results/018d4-man-eat3.gif (sw), art/results/018d4-woman-eat3.gif (se, ne, sw).
- Reddedilen ilk man sw denemesi (ham 84x116, 9 kare): art/results/018d4-red/.
- masaüstü kopyası: yerel

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_{man_r84_eat3_sw, woman_r84_eat3_se, woman_r84_eat3_ne, woman_r84_eat3_sw}_{00-07}.png`, 84x84, pivot x=42, pivot y=2 (alttan). 32 PNG, hepsi teslim.

| Karakter | Yön | Kare | Baş farkı | Elde nesne |
|---|---|---|---|---|
| man | sw | 8 | 2 (tepe 2-4) | yok |
| woman | se | 8 | 2 | yok |
| woman | ne | 8 | 1 | yok |
| woman | sw | 8 | 2 | yok |

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (32 dosya, --kind object) | 32/32 geçti |
| style.py check karakter | KALDI 0; UYARI 7 yalnız man sw kare 01-07 "ailesinden doygun" (x1,33-1,34, palet %100); diğer 25 GEÇTİ. Aile kilidi kurulmadı, `fit` uygulanmadı (018d3 ile aynı uyarı) |
| Baş farkı (≤2 px) | hepsi geçti (yukarıdaki tablo) |
| El ağız hizasında | evet, 4 yönde kare 03-05 (ne'de el saç/başın yanında, `b`) |
| Elde nesne | yok (görsel inceleme, ekmek/nesne çıkmadı) |
| Ayak satırı | hepsi y=81 |

## El konumu (`art/results/018d4-hand-anchors.txt`)
Biçim 018d3 ile aynı (önek, kare, x soldan, y alttan, isteğe bağlı `b`). 32 satır. Otomatik bulma yine güvenilir değil (ten rengi baş ile karışıyor); konumlar elle, ızgaralı bindirme sayfasında doğrulandı, hata payı ±2 px. `b`: woman ne kare 03-05. Kullanıcı el konumu sayfasına bakıp onaylasın.

## Sorunlar
- Man sw ilk deneme reddedildi (baş farkı 3), ikinci deneme geçti. Man sw doygunluk uyarısı 018d3'teki erkek yönlerle aynı.
- Kimlik kayması otomatik ölçülmedi; gözle tutarlı.
- Artık 8 yönün hepsi (018d3 + 018d4) `eat3` ve el konumuyla tamam.
