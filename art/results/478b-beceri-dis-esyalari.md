unity-import: pending

# 478b Beceri tamamlama: dış mekân eşyaları (rapor)

Yan yana sayfası: [478b-yanyana.png](478b-yanyana.png) (teslim edilen 4 eşya + referanslar). Teslim edilmeyen: `art/results/478b-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuzu + maske (referanssız), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301'deki ortak ek ("dark brown wood" yalnızca çan çerçevesinde bırakıldı; çiçek tarhı, sepet, tuzak ve mezar taşında çıkarıldı). Çağrı başı 1 gen, toplam 5 gen (cost_max 12 içinde). Maske base64 aktarımı bir çağrıda bozulduğu için (üretime girmedi, ücret yok) kuluçka sepeti maskesi 1 bit palet PNG ile yeniden gönderildi. İkinci deneme yapılmadı.
`get_balance` iş sonu: 2512,9 (paralel şeritler de harcıyor; bu işin payı 5).
Doygunluk düzeltmesi (`style.py fit`, ücretsiz): kuluçka sepeti x0,78, kerevit tuzağı x0,51. Ham hâller `478b-red/*_ham.png`.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | flower_bed | TESLİM EDİLMEDİ | stil KALDI: palet dışı renkler (%58; fit sonrası %61). Parlak çiçekler palet dışı; şekil tanınıyor (taş çerçeveli iki bölmeli tarh). Kullanıcı `478b-red/` içine bakabilir |
| 2 | storm_bell | teslim | ⚠ çanın üstünde haç biçimli direk çıktı; kullanıcı bakmalı |
| 3 | brood_basket | teslim (UYARI, fit x0,78) | palete uzak; hasır sepet, saman, yumurta tanınıyor |
| 4 | crayfish_trap | teslim (fit x0,51) | ⚠ huni ağzı belirgin değil, üstü açık sepet gibi |
| 5 | gravestone | teslim (UYARI) | ⚠ aileden soluk; üzerinde okunur harfler var (çizgi yerine yazı) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_storm_bell_se.png | 80x88 | 40 | 24 |
| iso_item_brood_basket_se.png | 80x56 | 40 | 24 |
| iso_item_crayfish_trap_se.png | 80x64 | 40 | 24 |
| iso_item_gravestone_se.png | 80x72 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`; hepsi geçti)
| Dosya | Opak sınır kutusu | Renk | Alta değme | Alt boşluk |
|---|---|---|---|---|
| storm_bell | x8-71, y7-80 | 35 | yok | 7 |
| brood_basket | x18-61, y12-46 | 39 | yok | 9 |
| crayfish_trap | x18-61, y4-52 | 25 | yok | 11 |
| gravestone | x12-68, y4-64 | 25 | yok | 7 |

Sepet ve tuzak ayak izinden (x8-71) dar (44 px genişlik).

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| storm_bell | %97 | 6,8 | x1,01 | GEÇTİ |
| gravestone | %89 | 13,2 | x0,54 | UYARI |
| brood_basket (fit) | %82 | 13,1 | x1,17 | UYARI |
| crayfish_trap (fit) | %100 | 6,7 | x1,25 | GEÇTİ |
| flower_bed (teslim edilmedi) | %58 | 25,5 | x1,30 | KALDI |
| (ham) brood_basket | %65 | 17,2 | x1,48 | KALDI |
| (ham) crayfish_trap | %78 | 12,2 | x2,24 | KALDI |

## Sorunlar
- Çiçek tarhı palet nedeniyle teslim edilmedi; renkli çiçek eşyası aile paletine doğal olarak uymuyor (öneri: kullanıcı onayı ya da palet gevşetmesi).
- Işık sırası, ayna uygunluğu ölçülmedi (betik yok); yan yana sayfasına bakılmalı.
- `validate_iso.py` tek dosya alıyor (öneri: çoklu dosya).
