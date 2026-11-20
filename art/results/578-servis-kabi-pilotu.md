unity-import: pending

# 578 Servis kabı pilotu (rapor): güveç 5/5 teslim, tepsi teslim edilmedi

Yan yana sayfası: [578-yanyana.png](578-yanyana.png) (5 güveç karesi + 7 referans). Doluluk şeridi: [578-serit.png](578-serit.png) (4→0). Teslim edilmeyen tepsi ve ham güveçler: `art/results/578-red/` (`tray-serit.png`). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object`, stil eşleme, `high top-down`, `basic shading`, `low detail`, `single color outline`, açıklama + tarifteki ortak ek. Tuval 32x32 (meyve sepetiyle aynı ölçü), kılavuz: tabanda 24x12 elmas (#786046), tabanın altı tuval altından 4 px yukarıda; maske = tabanı kapsayan dikdörtgen.
- Önce `_4` (2 çağrı; pilav çağrısında hız sınırı hatası, bir kez yeniden denendi, hatalı çağrıdan ücret yok). Sonra `_3…_0`: `_4` karesi arka plan, yalnızca kabın içi (elips) maske, 8 çağrı. Araç maske dışını dondurup geri vermiyor; çıktı yalnızca maskeli nesne. Bu yüzden kap `_4`'ten alındı, iç üretilen kareden elle kompozitlendi (tarifteki "kabı elle eşitle" yolu).
- Güveç: doygunluk ham x1.7 (KALDI), `style.py fit` ile x0.64-0.68 indirildi (aile hedefi 0.467); kap `fit_4`'ten, iç her karenin kendi fit'inden. Ham hâller `578-red/*_ham.png`.
- Toplam 10 generation (cost_est 10, cost_max 20). `get_balance` sonrası: 1932,7 (paralel şeritler de harcıyor).

## Sonuç
| Birleşim | Durum | Not |
|---|---|---|
| güveç `_4…_0` | teslim (fit) | kap 5 karede birebir aynı (alfa ve kap rengi fark 0 piksel); yemek 4→0 alçalıyor, `_0` boş koyu iç |
| tepsi `_4…_0` | TESLİM EDİLMEDİ | pilav her doluluk düzeyinde tepsiyi dolduruyor; `_1` tepsi içinde küçük kase gibi (yanlış nesne), `_0` boş değil, dolu görünüyor |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_serve_pot_stew_4.png | 32x32 | 16 | 10 |
| iso_serve_pot_stew_3.png | 32x32 | 16 | 10 |
| iso_serve_pot_stew_2.png | 32x32 | 16 | 10 |
| iso_serve_pot_stew_1.png | 32x32 | 16 | 10 |
| iso_serve_pot_stew_0.png | 32x32 | 16 | 10 |

Pivot = taban elmasının merkezi (fruit_basket ile aynı kural).

## Doğrulama
| Dosya | validate_iso (--max-colors 48) | Opak sınır | Stil: palete yakın / uzaklık / doygunluk | Sonuç |
|---|---|---|---|---|
| stew_4 | geçti | x3-28, y3-27 | %100 / 7,1 / x1,16 | GEÇTİ |
| stew_3 | geçti | x3-28, y3-27 | %100 / 6,7 / x1,14 | GEÇTİ |
| stew_2 | geçti | x3-28, y3-27 | %100 / 6,9 / x1,18 | GEÇTİ |
| stew_1 | geçti | x3-28, y3-27 | %100 / 6,7 / x1,17 | GEÇTİ |
| stew_0 | geçti | x3-28, y3-27 | %100 / 7,0 / x1,20 | GEÇTİ |

Kap dış çizgisi: 5 karede alfa farkı 0 piksel (tarif: en çok 1 px). Zemin döşeme testi gerekmiyor (masa üstü eşyası). Buhar, parlama, gölge ve el yok. Kap simetrik, tek sprite.

## Sorunlar
- ⚠ Güveç kabı kulplu, yüksek gövdeli bir kap görünümünde; iç yüzey elips. Doluluk farkı yalnızca üst yüzeyde okunuyor; `_1` ve `_0` birbirine yakın koyu. Kullanıcı `578-serit.png`'ye bakmalı.
- Tepsi: mask içi üretim yöntemi yuvarlak düz kapta tutmadı. Öneri: içi maskeyi küçültmek ve "heap" yerine "flat layer" tarifiyle yeni iş açmak ya da tepsiyi derin kap siluetine çevirmek. Karar kullanıcıda.
- Araçta `seed` yok, maske dışı dondurma yok (çıktı yalnızca nesne).
