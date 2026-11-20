unity-import: pending

# 357t Eşya düzeltmeleri (rapor)

Yan yana sayfası: [357t-yanyana.png](357t-yanyana.png) (7 referans + 7 yeni; teslim edilenler ve `357t-red/` birlikte). Teslim edilmeyenler: `art/results/357t-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, 301 yöntemi (`footprint_guide.py` kılavuz + maske), `high top-down`, `low detail`, `basic shading`, `single color outline`, 357 ortak eki. Her parça 1 gen; 7 çağrı = **7 gen** (cost_max 20). Dovekot ilk çağrısı base64 bozulması yüzünden üretime girmeden reddedildi (ücret yok), yeniden kodlanıp bir kez gönderildi (hız/eşzamanlılık hatası değil, aktarım hatası). `get_balance` sonrası: 2401,9 (paralel şeritler de harcıyor). Kılavuz yükseklikleri: kompost 22, kaya 24, büyük kaya 32 (2x1), hela 40 (tuval 80x88), adaçayı 18, kiriş 8 (64x40), güvercinlik 50 (80x84).

## Sonuç
| # | Parça | Durum | Not |
|---|---|---|---|
| 1 | compost_heap | TESLİM EDİLMEDİ | renk 51 > 48 (validate BAŞARISIZ), stil UYARI (doygunluk x1.50). Görsel: yığınlı çıtalı bölme + dirgen, tarife uygun |
| 2 | terrain_boulder | teslim (UYARI) | yuvarlak, çatlaklı gri kaya, yosun; altında çimen elması var (tabana ot istenmişti ama zemin parçası gibi okunabilir, kullanıcı bakmalı) |
| 3 | boulder_large_2x1 | TESLİM EDİLMEDİ | stil KALDI (palet dışı %78). 3-4 yuvarlak kaya öbeği tanınıyor ama altında yol/çimen zemini var |
| 4 | flush_privy | TESLİM EDİLMEDİ | stil KALDI (palet dışı %78; mavi su). Görsel: ay oyuklu kapılı tahta kulübe, altında su oluğu, tarife uygun |
| 5 | forage_sage | TESLİM EDİLMEDİ | stil KALDI (palete %23, mor çiçek palet dışı). Görsel: gri-yeşil yapraklar, lila başaklar, tanınıyor; `fit` yalnızca doygunluğu çözer, palet sorunu kalır |
| 6 | painted_beam | TESLİM EDİLMEDİ | renk 49 > 48, stil KALDI (palet dışı %75; kırmızı/mavi/sarı boya) |
| 7 | dovecote_village | teslim | yuvarlak taş kule, konik çatı, delik sıraları, çatıda kuş |

## Dosyalar
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_terrain_boulder_se_v2.png | 80x72 | 40 | 24 |
| Assets/Art/Iso/_aday/iso_item_dovecote_village_se_v2.png | 80x84 | 40 | 24 |

## Doğrulama (validate_iso, max 48 renk) ve stil denetimi
| Dosya | Opak kutu | Renk | Yarı saydam | Alta değme | Palete yakın | Doygunluk | Stil |
|---|---|---|---|---|---|---|---|
| boulder | x10-69, y12-64 | 40 | 0 | yok | %82 | x0.39 | UYARI |
| dovecote | x24-55, y5-65 | 26 | 0 | yok | %92 | x0.94 | GEÇTİ |
| compost (red) | x16-70, y6-56 | 51 | 0 | yok | %94 | x1.50 | UYARI, renk sınırı BAŞARISIZ |
| boulder_large (red) | x8-103, y9-88 | 43 | 0 | yok | %78 | x1.04 | KALDI |
| privy (red) | x8-71, y12-80 | 28 | 0 | yok | %78 | x1.02 | KALDI |
| sage (red) | x11-68, y14-59 | 41 | 0 | yok | %23 | x1.27 | KALDI |
| beam (red) | x17-61, y6-24 | 49 | 0 | yok | %75 | x1.23 | KALDI, renk sınırı BAŞARISIZ |

## Sorunlar
- Tek deneme kuralı gereği 5 parça yeniden denenmedi. Compost ve kiriş yalnızca 1-3 renk fazlalıkla kaldı; `reduce_colors` ile 48'e indirme (0,1 gen) ya da palete lila/kırmızı/mavi eklenmesi kullanıcı kararı. Hela ve adaçayı görsel olarak tarife uyuyor, yalnızca palet kapısına takıldı.
- Boulder ve büyük kaya çıktıları taban elması çimen/yol zemini taşıyor ("no ground" ekine rağmen); zemin dışı kaya için maskeyi kayanın üst yarısına daraltan yeni deneme önerilir.
- Cloud modunda `art/tmp357t/` geçici dosyaları commit edilmez. Araç önerisi: kılavuz/maske base64'ünü elle aktarmak hata verdi (bir kez bozuldu); `upload:<id>` yolu için token gerekir.
- Ayna ve döşeme testi bu iş için geçerli değil (eşya).
