unity-import: pending

# 780 Hayvan ölçeği pilotu: kedi teslim, küçük köpek teslim edilmedi

Sonuç: `status: review` (kısmi). Kedi 8 yön teslim edildi, boy hedefte (18-23 px, hedef 16-24). Küçük köpeğin tek karesi güney kontrolünü geçemedi (yandan duruyor, tuvalde 8 px havada), döndürülmedi (tek deneme kuralı). Masaüstü kopyası: yerel.
Yan yana sayfası (eski kedi SE, yeni kedi SE ve S, eski köpek SE; hayvan ailesi referansları): `art/results/780-yanyana.png`. Not: tarifteki 84 px yetişkin ve karo konturu sayfaya eklenmedi (`style.py sheet` yalnızca verilen dosyaları dizer).

## Araç ve ayarlar
- `create_map_object` 32x32, stil eşleme, özel maske: kılavuz = eski SE görselinin hedef kutuya (kedi 22x20, köpek 24x22, alt kenardan 2 px yukarı) en yakın komşuyla küçültülmüş hâli gri zeminde; maske = aynı kutu. `high top-down`, `single color outline` (araçta "black" seçeneği yok), `medium detail`, `basic shading`, tarifteki açıklama + ortak ek + 375 eki.
- Kedi döndürme: `create_character` v3, `n_directions=8`, quadruped `cat`, `high top-down`, referans = tek karenin indirme URL'si (32x32, tuval küçültülmedi), size verilmedi. Karakter kimliği: **191be777-5901-4865-8226-00cd100e45eb** (`cat_1_v2`). Köpek karakteri üretilmedi.
- Boy ölçümü: v3 kediyi tuvali doldurmadı (SE 19 px), 40x40 ikinci deneme gerekmedi.
- Renk: ham çıktı doygunluk x1,58-1,64 ile KALDI; `style.py fit --family hayvan` (doygunluk x0,70-0,73) sonrası 8/8 GEÇTİ (x1,14-1,16). Ham hâli `art/tmp780/` altında (commit edilmez). Raporda: doygunluk x0,7.
- Hız sınırı hatası olmadı.

## Tek kare kontrolü
| Aday | Boy x en | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|---|
| kedi | 18 x 12 | önden, oturuyor (ayakta değil) | yok | döndürüldü |
| küçük köpek | 16 x 23 | yandan (sağa bakıyor) | yok; tuvalde 8 px havada | döndürülmedi; tek kare `art/780-red/` |

## Teslim (`Assets/Art/Iso/_aday/`, 8 PNG)
Tuval 26x25 (8 yönün birleşik kutusu, pivot sütununa göre simetrik, üstte 1 px, SE ayak tabanı alttan 2. satır). Pivot x 13 (tuval ortası), pivot y alttan 1. Animasyon işi (783) aynı 26x25 tuvali kullanır.

| Dosya | Boyut | Boy (scale_check) | Stil |
|---|---|---|---|
| iso_animal_cat_1_v2_s | 26x25 | 18 | GEÇTİ |
| iso_animal_cat_1_v2_se | 26x25 | 19 | GEÇTİ |
| iso_animal_cat_1_v2_sw | 26x25 | 19 | GEÇTİ |
| iso_animal_cat_1_v2_e | 26x25 | 21 | GEÇTİ |
| iso_animal_cat_1_v2_w | 26x25 | 21 | GEÇTİ |
| iso_animal_cat_1_v2_n | 26x25 | 23 | GEÇTİ |
| iso_animal_cat_1_v2_ne | 26x25 | 23 | GEÇTİ |
| iso_animal_cat_1_v2_nw | 26x25 | 23 | GEÇTİ |

`validate_iso.py --kind object` SE: geçti (opak 232, yarı saydam 0, renk 22). Kırpma yalnızca tuval sınırı (içerik yeniden boyanmadı).

## Sorunlar
- Kedi önden oturan pozda (ayakta istenmişti); döndürmede yatık ve oturan pozlar karışık. Ham renk turuncuya kaçtı (tekir kahve yerine), `fit` ile soldu; beyaz ayaklar zayıf. Kimlik eski kediyle yaklaşık aynı. Kullanıcı görsel kararı gerekir.
- Köpek: yeniden deneme iş kuralıyla yapılmadı. Öneri: simetrik ön görünüşlü kılavuz çizip "head facing the viewer, symmetrical" vurgusu ya da yeni iş (780b).
- Köpek eksik olduğundan 780 `supersedes` listesindeki köpek maddesi arşivlenmemeli.

## Generation
| Çağrı | Gen |
|---|---|
| create_map_object x2 | 2 |
| create_character v3 ref x1 | 1 |
| Toplam | 3 (cost_max 10) |

`get_balance` sonrası: 9588 (paralel şeritlerin harcaması dahil).
