unity-import: pending

# 103 Hayvanlar 3: kaz + koyun

Bulut şeridi 1. Sekiz adaydan **4'ü teslim edildi** (goose_1, goose_3, sheep_4, sheep_5), 4'ü teslim edilmedi. Yan yana sayfası: `art/results/103-yanyana.png`. Reddedilen tek kareler: `art/results/103-red/`. Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
- Kaz (yol B'): `create_image_pixen`, 32x32, `no_background`, south, `high top-down`, siyah tek renk dış hat, medium detail. Sonra v3 döndürme (`create_character`, `mode=v3`, `n_directions=8`, `body_type=humanoid`, referans = tek kare).
- Koyun (yol B): `create_image_pixflux`, 46x46, aynı ayarlar, `shading=basic shading`, palet için `color_image` = K2 köylüsü (`iso_char_farmer_a2_56_cand_se.png`). Sonra v3 döndürme (`body_type=quadruped`, `template=dog`). Döndürme çıktısı 48x48 geldi (araç tuvali 46'dan 48'e yuvarladı).
- **Kural T hesabı:** 101 raporundaki r ≈ 0,90 (tek kare ölçümü) alındı; 30 / 0,90 = 33,3, en yakın 4'ün katı **32**. Kural, "yoksa 0,81" der; 101 raporunda r olduğu için 0,90 kullanıldı.
- sheep_2 karşılaştırması (sheep_2 dosyası açılmadı; değer iş dosyası notundan: SE boy 38, doluluk 0,79): yeni koyunlar SE boy 38-39 (sheep_3 tek kare 38, sheep_4 39, sheep_5 38), yani sheep_2 ile aynı boyda; hedef 36'ya göre +%6-8. Stil olarak sheep_4 ve sheep_5 aynı pixflux çizgisinde (kalın yün, siyah dış hat; yan yana sayfasında görsel karşılaştırma); sheep_5'te yünün açık mavi gölge tonu belirgin.

## Çağrı başına generation
| Çağrı | Adet | Gen |
|---|---|---|
| pixen kaz tek kare | 4 | 4 |
| pixflux koyun tek kare | 4 | 4 |
| v3 döndürme (goose_1, goose_3, sheep_4, sheep_5) | 4 | 4 (araç çıktısı: 1 gen / çağrı) |
| reduce_colors (8 kare birlikte, 32 renk) | 4 | 0,4 |
| **Toplam** | | **12,4** (tahmin 17, cost_max 20) |

`get_balance`: iş başında 4904,30 (95,70 kullanılmış); iş sonunda 4848,90 (151,10 kullanılmış). Aradaki fark 55,4 > 12,4: bakiye diğer şeritlerle paylaşımlı, bu işin harcaması 12,4. 429/hız sınırı hatası olmadı.

## Güney kontrolü ve zemin kontrolü (tek kare)
| Aday | Tuval | Güney kontrolü | Gövde kayması (px) | Zemin/gölge | Sonuç |
|---|---|---|---|---|---|
| goose_1 | 32 | geçti, tam önden | 0,7 | yok | döndürüldü |
| goose_2 | 32 | kanatlar açık, gövde ortada | 0,5 | **⚠ zemin lekesi**: kanatların altında gömülü dama (sahte saydamlık) pikselleri, ayaklar tuvale dayalı (32x32 dolu) | döndürülmedi |
| goose_3 | 32 | geçti | 0,5 | yok | döndürüldü |
| goose_4 | 32 | önden ama tavuk/horoz gibi (ibik, kaz değil) | 0,5 | **⚠ zemin lekesi**: ayak altında opak teal zemin elması; tuval dolu | döndürülmedi |
| sheep_3 | 46 | geçti | 0,5 | **⚠ zemin lekesi**: ayak altında açık mavi leke | döndürülmedi |
| sheep_4 | 46 | geçti | 0,0 | yok | döndürüldü |
| sheep_5 | 46 | geçti (hafif 3/4) | 0,5 | yok (yünde açık mavi gölge tonu var, zemin değil) | döndürüldü |
| sheep_6 | 46 | **güney değil**: yandan/3/4 dönük, bbox kayması 3 px (ağırlık merkezi 2 px) | 3,0 | yok | döndürülmedi |

Zemin lekeli ve güney olmayan dört adayın tek karesi `art/results/103-red/` altında (ayrıca `tek_kareler_x6.png`). Yeniden deneme yapılmadı.

## Renk azaltma
| Aday | 8 yön ham toplam renk | Azaltılmış | Bozulma |
|---|---|---|---|
| goose_1 | 51 | 32 | yok |
| goose_3 | 52 | 32 | yok |
| sheep_4 | 39 | 31 | yok |
| sheep_5 | 45 | 31 | yok |

Ham SE ve azaltılmış SE 8 kat büyütülerek karşılaştırıldı (`art/results/103-red/azaltma_se_x4_ham_solda.png`, ham solda): göz, gaga, boynuz ve dış hat bozulmadı; ⚠ azaltma bozdu yok. Assets'e azaltılmış hâl gitti.

## Stil denetimi (SE; hayvan ailesinin kilidi kurulmadı, palet uyumu uyarıdır)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk / aile | Sonuç |
|---|---|---|---|---|
| goose_1 (fit sonrası) | %98 | 5,5 | x1,32 | UYARI ailesinden doygun |
| goose_3 | %88 | 9,2 | x0,88 | GEÇTİ |
| sheep_4 | %100 | 4,6 | x1,38 | UYARI ailesinden doygun |
| sheep_5 | %92 | 9,5 | x1,49 | UYARI ailesinden doygun |

goose_1 ham hâlinde x1,91 ile "ailesinden çok doygun" yüzünden KALDI (şekil tanınıyor), bu yüzden 8 yönün hepsine `style.py fit --family hayvan` uygulandı (doygunluk x0,60-x0,71); düzeltilmiş hâl teslim edildi, ham hâl `art/tmp103/` altında (commit edilmez). Yönler arası doygunluk hafif tutarsız olabilir.

## Dosyalar (SE tablosu; pivot ALTTAN, pivot x = ayak bandı ortalaması)
`Assets/Art/Iso/_aday/iso_animal_<ad>_cand_{se,sw,nw,ne,s,w,n,e}.png` (4 aday x 8 = 32 dosya, `.meta` yok).

| Dosya (SE) | Boyut | Boy x uzunluk | Renk (azaltılmış) | Yarı saydam | Pivot x | Pivot y (yön aralığı) | validate_iso |
|---|---|---|---|---|---|---|---|
| goose_1 | 32x32 | 28x21 | 30 | 0 | 17,1 | 2 (2-3) | geçti |
| goose_3 | 32x32 | 28x21 | 31 | 0 | 13,9 | 2 (2) | geçti |
| sheep_4 | 48x48 | 39x36 | 29 | 0 | 22,5 | 5 (4-5) | geçti |
| sheep_5 | 48x48 | 38x30 | 29 | 0 | 24,0 | 5 (4-5) | geçti |

Diğer yönlerde: goose_1 boy 27-28, goose_3 boy 28, sheep_4 boy 39-40, sheep_5 boy 38-39; yarı saydam piksel hep 0. Pivot x yönlere göre 13,6-17,4 (kaz) ve 21,1-25,9 (koyun) arasında oynuyor, bu ayak bandı ağırlığına bağlı normal bir salınım.

## Hedefe göre boy sapması ve doluluk (sonraki işler için)
| Aday | SE boy | Hedef | Sapma | Doluluk (SE boy / tuval) |
|---|---|---|---|---|
| goose_1 | 28 | 30 | -%7 | 28/32 = 0,88 |
| goose_3 | 28 | 30 | -%7 | 0,88 |
| sheep_4 | 39 | 36 | +%8 | 39/48 = 0,81 (tek kare 40/46 = 0,87) |
| sheep_5 | 38 | 36 | +%6 | 38/48 = 0,79 (tek kare 39/46 = 0,85) |

Öneri (tahmin): kaz için tuval 32 yeterli, hedef 30 için 36 ya da 32'de daha küçük gövde istemi gerekir; koyun için hedef 36 isteniyorsa tuval 42 (36 / 0,85). Yeniden örnekleme yapılmadı.

## Doğrulama notları
- Gölge pişirilmemiş, arka plan saydam, ışık sol-üstten (teslim edilen dört adayda).
- 8 yön tutarlı: aynı hayvan, doğru yönler. Humanoid iskelet kazı **insanlaştırmadı**; kuş siluetleri korundu.
- sheep_5'in açık mavi yün gölgesi sheep_2'den daha soğuk; kullanıcı seçim notu.
- Gözle: sheep_4 ve sheep_5 SE'de arka bacaklar kümelenmiş (6 bacak izlenimi) hafif; sheep_4 SE/SW'de bu belirgin.

## Sorunlar
- 4 aday teslim edilmedi (goose_2, goose_4, sheep_3, sheep_6); iş eksik, yeni iş gerekir: kaz için pixen tek karede zemin lekesi/dama üretti, kazlarda bu 2/4; koyun için 46 tuvalde hafif mavi leke ve yandan dönüş. Öneri (tahmin): açıklamaya "symmetrical front view, no ground, no floor tile" vurgusu, ya da tuvali 4-6 px küçültmek.
- Bulut kabuğu değişken/döngü/`rm` yasakladığı için yardımcı betikler `art/tmp/` altına yazıldı, repoya girmez.
- Masaüstü kopyası: yerel (bulutta yapılamaz).
