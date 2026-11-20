unity-import: pending

# 731 Zanaat dükkânı eşyaları (rapor)

Yan yana sayfası: [731-yanyana.png](731-yanyana.png) (teslim edilen 3 eşya + esya referansları). Teslim edilmeyenler: `art/results/731-red/` (6 dosya).
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`, `--axis x`; yükseklikler: kovası 30, demir yığını 16, demirci standı 64, yarım dolap 60, çömlek standı 56, elbise askısı 70, yün sepeti 24, tezgâh 40, kil yalağı 22). `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki açıklama + ortak ek. 9 başarılı çağrı = 9 generation (cost_max 24 içinde), hepsi paralel. Yün sepeti ve tezgâhın ilk çağrısı base64 aktarım hatasıyla doğrulamada reddedildi (üretim yok, ücret yok); kılavuz/maske yeniden kodlanıp bir kez gönderildi. Yeniden üretim yapılmadı (tek deneme).
Elbise askısı: 64 px tuvale 64 px'lik elmas kılavuz sığmadığı için 80x104 üretildi (teslim edilmedi).
`get_balance` iş sonu: **1031,6** (paralel şeritler de harcıyor; bu işin payı 9).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | su kovası (quench_tub) | TESLİM EDİLMEDİ | stil KALDI (doygunluk x1,78); `fit` yalnızca kilitsiz aile için |
| 2 | demir yığını | TESLİM EDİLMEDİ | yığın değil, duvarlı küçük oda/kutu çıktı (kılavuz elmasını oda olarak çizdi); sayısal doğrulama geçti |
| 3 | demirci standı | TESLİM EDİLMEDİ | 52 renk (>48); ayrıca askıda halka/zincir gibi, aletler (nal, orak) net değil |
| 4 | yarım dolap | teslim | GEÇTİ; kapısız, ham tahta, yanında tahta var |
| 5 | çömlek standı | TESLİM EDİLMEDİ | 63 renk (>48) |
| 6 | elbise askısı | TESLİM EDİLMEDİ | 61 renk (>48), stil UYARI (palete uzak %87) |
| 7 | yün sepeti | TESLİM EDİLMEDİ | 61 renk, stil KALDI (palet dışı); sepet yerine renkli top tepsisi |
| 8 | dükkân tezgâhı | teslim | GEÇTİ; arkasında raf/şişe var, ölçeklerin tanınırlığı kontrol edilmeli |
| 9 | kil yalağı | teslim (UYARI) | ailesinden soluk (x0,40) |

Renk sınırı (48) aşan ve stil KALDI olanlarda `reduce_colors`/`fit` yapılmadı (esya ailesinin kilidi kurulu; 301 kuralı).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_unfinished_furniture_se.png | 80x96 | 40 | 24 |
| iso_item_shop_counter_2x1_se_nw.png | 112x88 | 56 | 32 |
| iso_item_clay_trough_se.png | 80x64 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`; maske dışı opak piksel hepsinde 0)
| Dosya | Opak sınır | Sonuç |
|---|---|---|
| yarım dolap | x10-69, y10-88 | geçti |
| tezgâh | x16-98, y7-80 | geçti |
| kil yalağı | x10-69, y9-56 | geçti |
| (red) kovası | x10-69, y8-62 | geçti (stil KALDI) |
| (red) demir yığını | x9-70, y7-56 | geçti (şekil yanlış) |
| (red) demirci standı | 80x96 | BAŞARISIZ (52 renk) |
| (red) çömlek standı | 80x96 | BAŞARISIZ (63 renk) |
| (red) elbise askısı | 80x104 | BAŞARISIZ (61 renk) |
| (red) yün sepeti | 80x64 | BAŞARISIZ (61 renk) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| yarım dolap | %100 | 6.7 | x1.18 | GEÇTİ |
| tezgâh | %100 | 5.7 | x0.78 | GEÇTİ |
| kil yalağı | %100 | 6.6 | x0.40 | UYARI |
| (red) kovası | %100 | 8.2 | x1.78 | KALDI |
| (red) demir yığını | %100 | 5.3 | x0.68 | GEÇTİ |
| (red) demirci standı | %94 | 8.7 | x0.96 | GEÇTİ |
| (red) çömlek standı | %97 | 7.9 | x0.86 | GEÇTİ |
| (red) elbise askısı | %87 | 13.7 | x1.24 | UYARI |
| (red) yün sepeti | %79 | 11.7 | x0.99 | KALDI |

## Yapılmayanlar / sorunlar
- Teslim edilmeyen 6 eşya için öneri: ayrı bir iş (çömlek standı, demirci standı, elbise askısı için `reduce_colors` izni ya da renk sınırı gevşetmesi; demir yığını ve yün sepeti ile kovası yeniden üretimi, demir yığınında kılavuz yüksekliği çok düşük (16) olduğu için maske oda gibi okunmuş olabilir, yüksekliği ~28 yap).
- Ölçülmedi: ışık sırası, ayna uygunluğu; zemin değil, döşeme testi yok. Gölge pişirilmemiş görünüyor (gözle).
- `items.json` `pending` alanını kod oturumu kaldırır; yalnızca 3 eşya geldi: `item.unfinished_furniture`, `item.shop_counter`, `item.clay_trough`.
