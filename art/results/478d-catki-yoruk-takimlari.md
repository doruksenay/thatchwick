unity-import: pending

# 478d Beceri tamamlama: Çatkı ve Yörük takımları (rapor): 3/6 teslim, review

Yan yana sayfası: [478d-yanyana.png](478d-yanyana.png). Teslim edilmeyenler: `art/results/478d-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız `footprint_guide.py` kılavuzu + maske, `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki + takım eki (Çatkı: dark brown oak, pegged; Yörük: kilim, red and navy blue). Kılavuzlar: masa 112x80 `--n 2 --axis x` yükseklik 34; sandalye 80x88 yükseklik 44; sandık 80x72 yükseklik 28; Yörük masa 112x72 yükseklik 16; minder 80x56 yükseklik 12; Yörük sandık 80x72 yükseklik 34. Altısı paralel gönderildi.
Çağrılar: 6 üretim + 1 yeniden gönderim (çatkı sandalyesi: ilk çağrıda base64 aktarımı bozuldu, üretime girmeden reddedildi, ücret yok; kılavuz yeniden kodlanıp gönderildi) = **6 generation** (cost_max 14 içinde). Yeniden deneme yapılmadı: Yörük üçlüsü tanınıyor, ikinci deneme koşulu oluşmadı (sorun palet).
`get_balance` iş sonu: 2480,9 (paralel şeritler de harcıyor; bu işin payı 6).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | catki_table | teslim | GEÇTİ, düz plank üst, ayaklar çapraz |
| 2 | catki_chair | teslim (UYARI) | ailesinden soluk (x0,59), yalnız 9 renk; çapraz arka çıta okunuyor |
| 3 | catki_chest | teslim (UYARI) | palete uzak (10,9), soluk (x0,55); demir bantlı sandık, geçmeli birleşim seçilmiyor |
| 4 | yoruk_table | TESLİM EDİLMEDİ | stil KALDI (palete yakın %60); kilim üstünde yuvarlak tahta okunuyor ama kilim büyük, masa değil halı gibi |
| 5 | yoruk_chair | TESLİM EDİLMEDİ | stil KALDI (%78); minder kırmızı-lacivert desenli ama ahşap kutu üzerinde, yer minderi gibi değil |
| 6 | yoruk_chest | TESLİM EDİLMEDİ | stil KALDI (%41, x1,49); kilim yığını tanınıyor ama sandık görünmüyor; `fit` x0,77 sonrası da KALDI (%59, palet dışı) |

Neden: kırmızı ve lacivert Yörük deseni aile paletinin dışında; `fit` yalnız doygunluğu düzeltir, palet dışı renkleri değil. Soluk/toprak tonlu kilim ya da palete kırmızı-lacivert eklenmesi (kullanıcı kararı) gerek.

## Dosyalar (Assets/Art/Iso/_aday/; pivot x ve y ALTTAN)
| Dosya | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_catki_table_se.png | 112x80 | 56 | 32 |
| iso_item_catki_chair_se.png | 80x88 | 40 | 24 |
| iso_item_catki_chest_se.png | 80x72 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48 --footprint`; hepsi geçti)
| Dosya | Renk | Yarı saydam | Opak kutu | Sonuç |
|---|---|---|---|---|
| catki_table | 24 | 0 | x11-101 y7-69 | geçti |
| catki_chair | 9 | 0 | x13-66 y4-80 | geçti |
| catki_chest | 21 | 0 | x8-71 y3-64 | geçti |
| (red) yoruk_table | 39 | 0 | x8-103 y7-56 | geçti |
| (red) yoruk_chair | 37 | 0 | x8-71 y4-48 | geçti |
| (red) yoruk_chest | 27 | 0 | x8-71 y5-64 | geçti |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| catki_table | %100 | 5,9 | x1,05 | GEÇTİ |
| catki_chair | %100 | 6,1 | x0,59 | UYARI (soluk) |
| catki_chest | %100 | 10,9 | x0,55 | UYARI (palete uzak, soluk) |
| yoruk_table (teslim değil) | %60 | 19,8 | x0,72 | KALDI |
| yoruk_chair (teslim değil) | %78 | 12,3 | x1,06 | KALDI |
| yoruk_chest (teslim değil) | %41 | 25,5 | x1,49 | KALDI |
| yoruk_chest fit (teslim değil) | %59 | 19,9 | x1,14 | KALDI |

## Sorunlar
- Yörük takımı (3 eşya) stil kapısında düştü; yeni tarif ya da palet kararı kullanıcıda.
- Çatkı sandalye ve sandığı soluk; ışık sırası, ayna ve ahşap ton kıyası yan yana sayfasında gözle bakılmalı. Çatkı sandalyesinin yönü SE olarak kabul edildi (ölçülmedi).
- Döşeme testi eşyalar için geçerli değil.
- Araç önerisi: base64 aktarımı bozulursa kılavuzu `compress_level` değiştirerek yeniden kodlamak yetiyor.
