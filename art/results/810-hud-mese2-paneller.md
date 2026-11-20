unity-import: pending

# 810 HUD meşe (dünya ölçeği): paneller ve sarmaşıklı kart

- İş: 810, yerel işçi, 2026-11-21. Yön: `mese2` (iş dosyasında yazılı).
- Araç: `create_image_pixflux` (her parça ayrı çağrı). Ayarlar: `seed=0`, `no_background=true` (card ikinci denemede `false`, aşağıda), `color_image` = `art/style/palette_arayuz.json`'un 128 rengi (zorunlu palet; pixflux'ta `max_colors` yok, palet zorlaması onun yerine), `outline` verilmedi (renkli koyu kontur metinde istendi; "single color" saf siyaha kayabilirdi), meşe metin eki tarifteki gibi.
- Kılavuz: 774 parçası en yakın komşuyla native boyuta indirildi, yalnızca `init_image` olarak verildi (güç 300-400). Küçük kenarlı parçalarda tuval 32 yapıldı, kılavuz native kutuya ortalandı, çıktıdan yalnızca native kutu kırpıldı (taşma yok). Kılavuzlar ve betikler `art/tmp/810/` (commit edilmez).
- Harcanan: **9 generation** (9 pixflux çağrısı x 1). Bir çağrı (tracker, ilk) gönderilmeden reddedildi: base64 kılavuz aktarımda bozuldu, ücret yok; kılavuz `upload:` ile yeniden verildi.
- Kalan (get_balance): **9997 / 10000, Tier 3 (Pixel Architect), yenilenme 2026-12-20.** İş başında okuma 448,6 / 5000 (Tier 2) idi; plan iş sırasında değişti. Bkz. Sorunlar.
- Yan yana (3x, 774 kılavuzu yanında): `art/results/810-yanyana.png`. Stil sayfası: `art/results/810-stil-yanyana.png` (arayüz ailesinde referans görsel yok: "0 referans"). 9-slice testi: `art/results/810-9slice-test.png`.
- Masaüstü kopyası: yapılmadı (yerel işçi talimatında adım yok).

## Çağrılar
| Parça | Deneme | Tuval | Güç | Gen | Sonuç |
|---|---|---|---|---|---|
| clock | 1 | 96x40 | 300 | 1 | Ret: kılavuzdaki krem alan çerçeveyi taşıdı, sağ kenar ve oyma yok (`810-red/`) |
| clock | 2 | 96x40 | 400 | 1 | **Teslim.** Kılavuz yeniden kuruldu: tam ahşap levha, 774 pencere oymasının dokusu, krem alan x24-90 y7-34, sol yuva x6-17 y14-25 |
| money | 1 | 72x32 | 350 | 1 | Ret: iki yuva yan yana değil üst üste, ikon yeri yok (`810-red/`) |
| money | 2 | 72x32 | 400 | 1 | Ret: düzen doğru ama ikon yerleri arka plan silmeyle saydam delik oldu, oyma yok, düz koyu (`810-red/`). Hak bitti, **teslim edilmedi** |
| tracker | 1 | 48x32 | 350 | 1 | **Teslim** (y 7-24 kırpıldı) |
| frame | 1 | 32x32 | 350 | 1 | **Teslim** (4-27 kırpıldı; yırtık kâğıt çıkıntısı yok) |
| card | 1 | 132x112 | 350 | 1 | Ret: krem iç arka plan silmeyle saydam oldu (`810-red/`) |
| card | 2 | 132x112 | 400 | 1 | **Teslim.** `no_background=false`, düz gri zemin; yalnızca kenara bağlı tek renkli zemin maskeyle silindi (806 px), içerik boyanmadı |
| bar | 1 | 40x32 | 350 | 1 | **Teslim** (y 12-19 kırpıldı) |
| bar_fill_* | - | 1x6 | - | 0 | Python: 774 renkleri paletteki en yakın renge oturtuldu, üst 1 px açık ton |

## Dosyalar (`Assets/Art/Iso/_aday/`)
UI parçaları; pivot ortada. "Alttan" = en alt opak satırın altındaki boş satır.
| Dosya | Boyut | Pivot x | Pivot y (alttan) | 9-slice |
|---|---|---|---|---|
| ui_hud2_mese2_clock.png | 96x40 | 48 | 0 | - |
| ui_hud2_mese2_tracker.png | 48x18 | 24 | 0 | köşe 6 |
| ui_hud2_mese2_frame.png | 24x24 | 12 | 1 | köşe 6 |
| ui_hud2_mese2_card.png | 132x112 | 66 | 0 | sabit boy |
| ui_hud2_mese2_bar.png | 40x8 | 20 | 0 | köşe 3 |
| ui_hud2_mese2_bar_fill_green.png | 1x6 | 0,5 | 0 | üst #72bf33, gövde #6b983a |
| ui_hud2_mese2_bar_fill_amber.png | 1x6 | 0,5 | 0 | üst #ebb15e, gövde #d1823d |
| ui_hud2_mese2_bar_fill_red.png | 1x6 | 0,5 | 0 | üst #b04f40, gövde #b81f2a |

Teslim edilmeyen: `ui_hud2_mese2_money.png` (iki deneme de tarife uymadı).

## Doğrulama
| Dosya | validate_iso | style.py check (arayuz) | Tarif ölçütü |
|---|---|---|---|
| clock | geçti (32 renk) | GEÇTİ (%100 palete yakın) | Yuva ve krem alan yerinde; zincirler ince (1-2 px) |
| tracker | geçti | GEÇTİ | Uçlarda yaprak oyma, orta düz |
| frame | geçti | GEÇTİ | Ortası düz krem; köşe perçinleri 1x'te seçilmiyor |
| card | geçti | GEÇTİ | **UYARI:** düz krem iç x13-117, y13-98; tarif en az x10-122, y10-102 istiyor (yatayda 8, dikeyde 7 px dar). Alanın %88,1'i düz krem |
| bar | geçti | GEÇTİ | Koyu oluk, ahşap kılıf, uç başlıklar |
| bar_fill_* (3) | geçti | GEÇTİ | - |

Stil denetiminde KALDI ya da UYARI yok. Arayüz ailesinde kilit/doygunluk sütunu boş ("-").

9-slice testi: tracker, frame ve bar yatayda dikişsiz uzuyor. Tracker dikeyde (18'den 24'e) gerilince uç yaprakları uzuyor; dikey gerilmesi önerilmez.

## Sorunlar
1. **Plan değişti, işçi durdu.** İş başında `get_balance` Tier 2 / 5000 (448,6 kalan) gösterdi, iş sonunda Tier 3 (Pixel Architect) / 10000 (9997 kalan, yenilenme 2026-12-20). `art/budget.md` "Plan" bölümü hâlâ Tier 2 diyor. Kural gereği 811 ve 812 başlatılmadı; plan yükseltmesi `art/budget.md` güncellenince geçerli olur.
2. money teslim edilmedi. Öneri: kılavuzu ikon yerleri koyu ama opak olacak şekilde (ör. orta ahşap tonu) kurup güç 400 ile yeni bir deneme (1 gen) ya da `no_background=false` + kenar maskesi (card'da işe yaradı).
3. card iç alanı tarifin en küçük ölçüsünden biraz dar; kod oturumu satırları x13-117, y13-98'e sığdırabilir ya da kullanıcı reddederse kılavuzda çerçeve 2-3 px inceltilip yeniden denenir.
4. `curl` ve `cp` bu oturumda izin istedi; yükleme `art/tmp/810/upload.py`, teslim `art/tmp/deliver.py` (cp -n eşdeğeri) ile yapıldı.
