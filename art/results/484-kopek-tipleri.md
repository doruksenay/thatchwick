unity-import: pending

# 484 Köpek tipleri (484-kopek-tipleri): kısmen teslim (yalnızca lapdog)

Sonuç: `status: review`. 7 pixflux tek karesi üretildi; yalnızca kucak köpeği (lapdog) güney kontrolünü ve zemin kontrolünü geçti, döndürüldü ve teslim edildi. Diğer 6 tip teslim edilmedi. Masaüstü kopyası: yerel (bulut modu).
Not: kuyrukta aynı numaralı iki dosya var (`484-deniz-ve-kumsal-karolari.md` pending, `484-kopek-tipleri.md` running). Bu çalışma yalnızca köpek işini yaptı; deniz işine dokunulmadı. Numara çakışması çözülmeli (biri yeniden numaralanmalı).

Yan yana sayfası: `art/results/484-yanyana.png` (koyun 1, domuz 2 referans; lapdog 8 yön)

## Araç ve ayarlar
- 375 tarifi: `create_image_pixflux`, `view=high top-down`, black outline, `direction=south`, `no_background`, medium detail, basic shading; palet zorlaması `iso_animal_sheep_1_cand_se.png` (10 renk şeridi). Açıklama eki: 375 eki + `neutral grey-brown coat, plain coat without markings or spots` + `standing directly on transparent background, no ground disc, no floor shadow, no grey patch under the paws`.
- Tuvaller: çoban, tazı, mastıf 42; zağar, iz, su köpeği 38; lapdog 32.
- v3 döndürme (lapdog): `create_character mode=v3`, quadruped, `template=dog`, `high top-down`, reference = pixflux çıktı URL'si, 1 gen.
- iz (tracker) çağrısı ilk denemede hız sınırına takıldı (10/10 jobs, ücretlenmedi), bir kez yeniden denendi, geçti.
- `style.py fit --family hayvan` 8 yöne uygulandı (x1,00: değişmedi).

## Tek kare kontrolü
| Tip | Tuval | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|---|
| shepherd | 42 | yandan (güney değil) | yok | döndürülmedi |
| sighthound | 42 | yandan | var (gri leke) | döndürülmedi |
| scenthound | 38 | yandan | var | döndürülmedi |
| tracker | 38 | yandan | yok | döndürülmedi |
| mastiff | 42 | önden | var (geniş açık gri disk) | döndürülmedi |
| waterdog | 38 | yandan | var | döndürülmedi |
| lapdog | 32 | önden, oturan | yok | döndürüldü, teslim |

Reddedilen 6 tek kare: `art/results/484-red/`. Not: yandan çıkan kareler biçim olarak tipleri iyi ayırıyor (tazı çok uzun bacaklı, mastıf iri), ama v3 güney girdisi gerektirdiğinden döndürülemedi. Öneri (tahmin): `create_character mode=pro` ile bu kareleri `reference_image_url` olarak döndürmek (pro her yöne bakan sprite'ı döndürür; 10 gen/tip) ya da tarife "seen straight from the front" ve sağlam bir "no ground" vurgusu eklemek.

## Teslim edilen dosyalar (`Assets/Art/Iso/_aday/`, 8 PNG)
`iso_animal_dog_lapdog_cand_{se,sw,nw,ne,s,w,n,e}.png`. Hepsi 32x32.

| Yön | Opak bbox (x, y) | Renk | Yarı saydam | Pivot x | Pivot y alttan |
|---|---|---|---|---|---|
| se | 2-26, 1-30 | 13 | 0 | 14,5 | 1 |
| sw | 5-25 | 11 | 0 | 15,0 | 1 |
| nw | 5-30 | 10 | 0 | 16,7 | 1 |
| ne | 6-26 | 9 | 0 | 15,3 | 1 |
| s | 3-26 | 8 | 0 | 13,3 | 1 |
| w | 5-29 | 11 | 0 | 17,0 | 1 |
| n | 5-27 | 9 | 0 | 17,5 | 1 |
| e | 3-27 | 11 | 0 | 14,7 | 1 |

Boy ~30 px (oturan poz; kedi 375'teki gibi): küçüklük ölçütü (en küçük tip) sağlanır ama oturan poz yüksek çıktı. Renk azaltma gerekmedi (≤13). `validate_iso.py --kind object` SE: geçti. Gölge pişirilmemiş, ışık sol-üstten, 8 yön tutarlı.

### Stil denetimi (`style.py check --family hayvan`)
8 yönün hepsi UYARI, KALDI yok: "ailesinden soluk" (x0,26-0,27); palete yakın %100. Hayvan ailesinin kilidi kurulmadı. `fit` soluk yönde düzeltme yapmadı.

## Generation
| Çağrı | Gen |
|---|---|
| pixflux x7 | 7 |
| create_character v3 ref x1 | 1 |
| Toplam | 8 |

`get_balance` sonrası: 2812,9 (paralel şeritlerin harcaması dahil; iş öncesi 2836,9).

## Sorunlar
- 6/7 tip teslim edilmedi: 5 yandan duruş, 4'ünde (mastıf önden ama) zemin diski/lekesi; ayrıntı tabloda. Tek deneme kuralı gereği yeniden denenmedi. Yeni iş gerekir (öneri yukarıda).
- Kucak köpeği "soluk" (x0,26): beyaz ağızlıklı, gri-kahve nötr; oyundaki renk palet değişimiyle gelecek.
- Numara çakışması: iki iş dosyası `484`.
