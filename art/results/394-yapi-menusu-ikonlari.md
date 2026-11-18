unity-import: pending

# 394 Yapı menüsü ikonları: eşya kategorileri

**Sonuç: 18 ikondan 8'i teslim, 10'u okunmadığı için teslim edilmedi.** Yeniden deneme yok (tek deneme kuralı).

## Araç ve ayarlar
`create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, `seed=0`; tarifteki ortak ek ve marka paleti açıklamaya eklendi. İkon başına 1 gen, toplam 18. Stil referansı: araç `style_image` desteklemiyor, yalnızca açıklamaya ek yapıldı.
Hız sınırı: ilk dalgada 10/10 eşzamanlı iş sınırı (9 çağrı reddedildi, harcama yok); iş bitince reddedilen 9 çağrı bir kez yeniden gönderildi. Kalan bakiye: 4404,9 (paralel şeritler dahil ortak sayaç).
Masaüstü kopyası: yerel.

## Teslim edilenler (`Assets/Art/Iso/_aday/`, 32x32, pivot x=16, pivot y=0 alttan merkez varsayılan)
| Dosya | Renk | Yarı saydam | Stil denetimi |
|---|---|---|---|
| ui_icon_cat_seating | 34 | 0 | GEÇTİ |
| ui_icon_cat_sleep | 50 | 0 | GEÇTİ |
| ui_icon_cat_kitchen | 39 | 0 | UYARI (palet dışı, kilit yok) |
| ui_icon_cat_kids | 51 | 0 | UYARI |
| ui_icon_cat_heat_light | 17 | 0 | UYARI |
| ui_icon_cat_ceiling | 62 | 0 | UYARI |
| ui_icon_cat_garden | 29 | 0 | UYARI |
| ui_icon_cat_decor | 19 | 0 | UYARI (palete uzak; nesne küçük, 16x14 px, 24-26 hedefinin altında) |

⚠ Hepsi ≤12 renk hedefini aşıyor (17-62; Pixen yumuşak gölge üretiyor). Seçilmiş ikonlar yine de okunuyor; renk azaltma (`style.py fit`) denenmedi.

## Teslim edilmeyenler (`art/results/394-red/`)
| Dosya | Neden |
|---|---|
| animals | "beyaz tavuk" yerine rengârenk kuş, yanında yan nesneler |
| barn | kümes yerine kahverengi yaratık |
| decor_wall | tuval boydan boya dolu arka plan (şeffaf değil) |
| farm | tırmık yerine yaratık/şekilsiz |
| hygiene | okunmuyor (tekne ve sabun ayırt edilemiyor) |
| rug | rulo halı yerine kaplumbağa benzeri şekil |
| storage | sepet yerine pembe kubbe |
| structure | arka plan dolu, merdiven sahne gibi |
| surfaces | masa değil, şekilsiz kahverengi blok |
| work | çıkrık yerine karakter |

## Görseller
- Yan yana sayfası (18 ikon): `art/results/394-yanyana.png`
- Önizleme x3: `art/results/394-onizleme-dark.png`, `art/results/394-onizleme-linen.png` (18 ikon, teslim edilmeyenler dahil)

## Sorunlar ve öneri
- Pixen 32x32 ikonlarda nesne adını sık yanlış yorumluyor ("tavuk", "çıkrık", "kümes"). Eksik 10 ikon için `create_image_pro` ya da başka seed'lerle yeni iş yazılması önerilir (kullanıcı kararı).
- Önizleme sayfasındaki sıra alfabetik: animals, barn, ceiling, decor, decor_wall, farm / garden, heat_light, hygiene, kids, kitchen, rug / seating, sleep, storage, structure, surfaces, work.
