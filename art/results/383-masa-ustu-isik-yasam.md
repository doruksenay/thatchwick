unity-import: pending

# 383 Masa ve raf üstü: ışık, saksı otları, iplik ve sepetler (rapor)

Yan yana sayfası: [383-yanyana.png](383-yanyana.png) (9 teslim + 7 referans). Teslim edilmeyen: `art/results/383-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (301 yöntemi, kılavuz Python ile; taban elması alt kenardan 4 px yukarıda, 16x8 / 20x10 / 28x14, maske eşya yüksekliği + 1 px). `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki + takım eki. 12 çağrı x 1 gen = 12 generation (cost_max 18), ikinci deneme yok. #12 ilk gönderimde base64 bozulması (üretim başlamadı), yeniden gönderimde hız sınırı aldı; ~30 sn sonra bir kez yeniden gönderildi, tuttu. Tuvaller araç alt sınırı (32) yüzünden tarifteki tuvalden büyük (32x40, 32x32, 40x40). `get_balance` iş sonu: 4771,90 (paralel şeritler de harcıyor; bu işin payı 12).
Doygunluk düzeltmesi (`style.py fit`): kil kandil x0.68, biberiye saksısı x0.82, iplik ve iğ x0.88; ham hâller `383-red/ham_*` altında.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | iç yağı mumu | TESLİM EDİLMEDİ | `--light`: ışık payı %35 aşıldı (%48), palet %75 |
| 2 | balmumu mumu | TESLİM EDİLMEDİ | `--light`: ışık payı %35 aşıldı (%48); doygunluk x0.62 |
| 3 | kazıklı şamdan | teslim | UYARI: palete uzak (13.5) |
| 4 | katlanır şamdan | teslim | UYARI: ailesinden soluk (x0.53) |
| 5 | kil kandil | teslim (fit x0.68) | ham KALDI (x1.64); fit sonrası GEÇTİ |
| 6 | boynuz fener | teslim | UYARI: palete uzak (10.7) |
| 7 | fesleğen saksısı | TESLİM EDİLMEDİ | palet %60, uzaklık 21.8; `fit` palet sorununu çözmez |
| 8 | biberiye saksısı | teslim (fit x0.82) | ham KALDI; fit sonrası UYARI (palete uzak %84) |
| 9 | kekik saksısı | teslim | UYARI: palete uzak (13.9) |
| 10 | iplik ve iğ | teslim (fit x0.88) | ham KALDI; fit sonrası GEÇTİ |
| 11 | dikiş sepeti | teslim | GEÇTİ |
| 12 | asma sepet | teslim | UYARI: palete uzak (11.3) |

## Dosyalar (Assets/Art/Iso/_aday/)
Pivot: küçük taban elmasının merkezi (alttan 4 + elmas yüksekliği/2).
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_pricket_candlestick_se.png | 32x40 | 16 | 8 |
| iso_item_folding_candlestick_se.png | 32x40 | 16 | 9 |
| iso_item_clay_lamp_se.png | 32x32 | 16 | 9 |
| iso_item_horn_lantern_se.png | 32x40 | 16 | 9 (kanca eşyası da olabilir) |
| iso_item_herb_pot_rosemary_se.png | 32x40 | 16 | 9 |
| iso_item_herb_pot_thyme_se.png | 32x40 | 16 | 9 |
| iso_item_yarn_spindle_se.png | 32x32 | 16 | 9 |
| iso_item_sewing_basket_se.png | 40x40 | 20 | 11 |
| iso_item_hanging_basket_se.png | 32x40 | 16 | 10 (kanca eşyası; kanca noktası görselin üstü) |

## Doğrulama (validate_iso, `--max-colors 48`)
Hepsi geçti: yarı saydam 0, alta değme yok (alt boşluk 3-5 satır), renk sayısı 9-35 (en yüksek dikiş sepeti). Opak sınır kutuları: kazıklı x9-22 y4-36; katlanır x8-23 y5-36; kandil x6-25 y7-27; fener x6-25 y3-36; biberiye x6-25 y4-35; kekik x8-23 y5-35; iplik x6-25 y4-27; sepet x6-33 y4-35; asma x5-26 y5-35.

## Stil denetimi (`style.py check --family esya`, ışıklılarda `--light`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| kazıklı şamdan | %100 | 13.5 | x0.71 | UYARI |
| katlanır şamdan | %100 | 8.1 | x0.53 | UYARI |
| kil kandil (fit) | %100 | 8.3 | x1.11 | GEÇTİ |
| boynuz fener | %100 | 10.7 | x1.14 | UYARI |
| biberiye (fit) | %84 | 14.7 | x1.15 | UYARI |
| kekik | %85 | 13.9 | x1.03 | UYARI |
| iplik (fit) | %100 | 7.6 | x1.15 | GEÇTİ |
| dikiş sepeti | %93 | 8.1 | x0.97 | GEÇTİ |
| asma sepet | %100 | 11.3 | x1.14 | UYARI |
| iç yağı mumu (teslim değil) | %75 | 14.0 | x1.22 | KALDI |
| balmumu mumu (teslim değil) | %100 | 6.3 | x0.62 | KALDI |
| fesleğen (teslim değil) | %60 | 21.8 | x1.16 | KALDI |

## Sorunlar
- Fesleğen ve iki mum teslim edilmedi; bu işte tek deneme var. Yeni iş (383b) gerekir: mumlarda alevi küçük tutmak ("tiny flame") ışık payını düşürebilir; fesleğende "dark green leaves, terracotta pot" palet ipucu.
- Döşeme testi (zemin değil) ve ayna uygunluğu bu işte uygulanmadı.
- `footprint_guide.py` 32 px altı tuval ve küçük taban genişliği desteklemiyor: öneri, `--base-width` seçeneği.
