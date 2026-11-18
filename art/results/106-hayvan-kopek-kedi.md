unity-import: pending

# 106 Hayvanlar 6: köpek + kedi: stil kapısı KALDI, teslim yok

Sonuç: `status: failed`. Üretim tamamlandı ama `style.py check --family hayvan` 4 rotasyon setinin 29/32 dosyasında KALDI verdi (ailesinden çok doygun, bazılarında palete uzak). Yönerge gereği KALDI dosya teslim edilmez; `Assets/Art/Iso/` altına hiçbir şey konmadı. Masaüstü kopyası: yerel (bulut modu, yapılmadı).

Yan yana sayfası (yalnızca SE yönleri, referanslarla): `art/results/106-yanyana.png`

## Araç ve ayarlar
- Yol B: `create_image_pixflux`, `view=high top-down`, `outline=single color black outline`, `direction=south`, `no_background=true`, `detail=medium`, `shading=basic`. Palet zorlaması: `iso_char_farmer_a2_56_cand_se.png`'nin 32 rengi (32x1 şerit PNG olarak inline; yalnızca renkler okunur).
- Tuval: köpek 38, kedi 32. Tarifteki açıklama metni ve ek aynen.
- v3 döndürme (yalnızca güney kontrolünü ve zemin kontrolünü geçen 4 aday): `create_character mode=v3`, `n_directions=8`, `body_type=quadruped`, `template=dog|cat`, `reference_image_url` = pixflux çıktısının indirme URL'si. Maliyet 1 gen/aday (araç çıktısı: "1 generations").
- Hız sınırı: cat_2 ve cat_4 çağrıları "rate limit exceeded (8/8 jobs)" döndü (iş yok sayıldı, ücretlendirilmedi). Tarifteki istisna gereği bir kez yeniden denendi, ikisi de geçti.

## Güney kontrolü ve zemin kontrolü (tek kare)
| Aday | Tuval | SE/tek kare boy x en | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|---|---|
| dog_1 | 38 | 27 x 22 | önden, tamam | yok | döndürüldü |
| dog_2 | 38 | 26 x 22 | önden, tamam (görünüm koyuna benziyor, köpek okunmuyor) | yok | döndürüldü |
| dog_3 | 38 | 33 x 24 | önden, tamam | yok | döndürüldü |
| dog_4 | 38 | 33 x 24 | önden | var (açık mavi zemin lekesi ayak altında) | döndürülmedi, 1 gen |
| cat_1 | 32 | 30 x 22 | önden, tamam | yok | döndürüldü (döndürmede oturan poz çıktı) |
| cat_2 | 32 | 27 x 25 | 3/4 yandan, gövde kayık | var (mavi leke) | döndürülmedi |
| cat_3 | 32 | 23 x 25 | tam yandan | var (toprak lekesi) | döndürülmedi |
| cat_4 | 32 | 26 x 19 | önden | var (mavi leke) | döndürülmedi |

Döndürülmeyen 4 adayın tek kareleri teslim edilmedi (cloud modunda masaüstü `_ham` yok).

## Döndürülen adaylar (8 yön)
Tuval: köpek döndürmeden 40x40 çıktı (38 değil); kedi 32x32. Pivot = ayak bandı (en alt 3 opak satır) x ortalaması, y alttan.

| Aday | SE boy x en | Hedef boy / sapma | Doluluk (SE boy / tuval) | Ham renk (8 yön birleşik) | Azaltılmış | Yarı saydam | Pivot x (SE) | Pivot y alttan (8 yön) |
|---|---|---|---|---|---|---|---|---|
| dog_1 | 27 x 22 | 30 / -10% | 0,68 | 36 | 32 (reduce_colors, gerekti) | 0 | 18,9 | 5 (hepsi aynı) |
| dog_2 | 26 x 23 | 30 / -13% | 0,65 | 18 | gerekmedi | 0 | 17,6 | 7 (hepsi aynı) |
| dog_3 | 33 x 28 | 30 / +10% | 0,83 | 47 | 32 (reduce_colors, gerekti) | 0 | 21,4 | 3 (hepsi aynı) |
| cat_1 | 30 x 18 | 21 / +43% | 0,94 | 29 | gerekmedi | 0 | 15,2 | 1-2 (1 oynama) |

Notlar:
- Kedi: pixflux'ın en küçük tuvali 32 ve çıktı 30 px boyunda (hedef 21). Gelecek için tahmin: `create_image_pixen` 28 tuvaliyle denenmeli ya da kedi küçük bir tuvale sığacak şekilde (oturan poz yerine ayakta) yeniden tarif edilmeli. Kedi döndürmede oturan poz çıktı, bu yüzden boy yüksek.
- Köpek tuvali 38 iken döndürme 40 çıktı; doluluk 0,65-0,83, hedefe tuval önerisi: dog_1 için ~42, dog_3 için ~34 (tahmin).
- dog_2 koyuna benziyor (kıvırcık krem yün + koyu yüz); "sheepdog" tarifi koyun görünümü verdi. Kullanıcı seçmeyecekse zaten dışarıda.
- dog_1 s yönünde sırt/yan yönlerde gövde pozu tutarlı; dog_3 8 yönde aynı hayvan, tutarlı. cat_1 yönler tutarlı.
- Azaltma bozulma denetimi: dog_1 ve dog_3 SE ham/azaltılmış 8 kat karşılaştırması yapılmadı (görsel inceleme sayfasında göz ve dış hat okunur durumda); ⚠ işareti konmadı ama kesin denetim eksik.
- Tam 2:1 gerektiren zemin işi değil, döşeme testi uygulanmadı. Gölge pişirilmemiş (tek kare ve 8 yönde ayak altı temiz), ışık sol-üstten.

## Doğrulama
`validate_iso.py --kind object` dog_1 SE: geçti (opak 390, yarı saydam 0, renk 27, alt boşluk 5). Diğer SE dosyaları için ayrıca çalıştırılmadı (stil kapısı zaten KALDI).

### Stil denetimi (`style.py check --family hayvan`)
| Aday | KALDI | UYARI | Not |
|---|---|---|---|
| dog_1 | 5 (e, n, ne, nw, w) | 3 (s, se, sw) | doygunluk x1,40-1,54; palete uzak |
| dog_2 | 8 | 0 | doygunluk x1,59-1,78 |
| dog_3 | 8 | 0 | doygunluk x1,63-1,89 |
| cat_1 | 8 | 0 | doygunluk x1,66-1,74; n, ne, se palete uzak |

Ana neden: palet zorlaması (köylü paleti) + v3 döndürme, ailenin (hayvan) doygunluk düzeyinin 1,4-1,9 katı çıktı.

## Generation
| Çağrı | Gen |
|---|---|
| pixflux x8 (38/32) | 8 |
| create_character v3 ref x4 | 4 |
| reduce_colors x2 (dog_1, dog_3; 8 kare birlikte) | 0,2 |
| Toplam | 12,2 |

`get_balance` sonrası: 1725,2 (öncesi 1760,4; paralel şeritlerin harcamasını da içerir).

## Sorunlar / öneri
- Stil kapısı: hayvan ailesinin doygunluk düzeyine göre sonuç çok doygun. Öneri: palet zorlaması yerine hayvan ailesinin `references.json` görselinden `reduce_colors palette_image` ile palet eşleme ya da daha az doygun açıklama ("muted desaturated"), sonra yeniden iş. Yeniden deneme bu işte yapılmadı (tek deneme kuralı).
- Hiçbir PNG `Assets/Art/Iso/`'ya konmadı.
