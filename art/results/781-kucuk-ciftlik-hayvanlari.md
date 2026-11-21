unity-import: pending

# 781 Küçük çiftlik hayvanları ölçek seti: 2/10 gövde teslim (tavuk 5, kaz 1)

Sonuç: `status: review` (kısmi). 10 gövdeden **chicken_5 ve goose_1** 8 yönde ölçek aralığına girdi ve teslim edildi. Kalan 6 gövde döndürüldü ama en az bir yönü `scale_check` dışında kaldı: teslim edilmedi (`art/781-red/`). `goose_3` ve `pigeon` tek karede güney kontrolünü geçemedi, döndürülmedi. Masaüstü kopyası: yerel.
Yan yana sayfası (teslim edilen 2 + reddedilen 6 SE, hayvan referanslarıyla): `art/results/781-yanyana.png`. Not: 84 px yetişkin ve karo konturu sayfada yok (`style.py sheet` yalnızca verilen dosyaları dizer).

## Araç ve ayarlar
- 780 yöntemi: `create_map_object` 32x32, stil eşleme, özel maske. Kılavuz = eski SE görselinin hedef kutuya (tablodaki en x boy, alt kenardan 2 px yukarı, ortada) en yakın komşuyla küçültülmüş hâli gri zeminde; maske = aynı kutu. `high top-down`, `single color outline` (araçta black seçeneği yok), `medium detail`, `basic shading`; açıklama 780/106 ortak ekleriyle. Ördek kılavuzu `goose_1` SE'sinden (palet).
- Döndürme: `create_character` v3, 8 yön, referans = tek karenin indirme URL'si, size verilmedi. Kuşlar `humanoid` (yol B'), kedi `quadruped/cat`, tavşan `quadruped/dog`.
- goose_3 kılavuzunun base64'ü ilk çağrıda kesildi ("incomplete", ücretlendirilmedi); aynı kılavuz 16 renge indirilip yeniden gönderildi (istemci sorunu, harcama yok).
- Renk: `style.py fit --family hayvan` yalnızca teslim edilen 2 gövdeye (chicken_5 doygunluk x0,66-0,92 ile indirildi; goose_1 x1,00). Hız sınırı hatası olmadı.
- Kırpma: 780 kuralı (tuval 8 yönün birleşik kutusu, SE ayak orta noktasına göre simetrik, üstte 1 px, SE tabanı alttan 2. satır); içerik yeniden boyanmadı.

## Tek kare kontrolü
| Gövde | Boy x en (tek kare) | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|---|
| cat_2 | 14 x 21 | önden, ayakta | yok | döndürüldü |
| chicken_3 | 14 x 13 | önden, pembe tavuk (renk eski görselden farklı) | yok | döndürüldü |
| chicken_4 | 13 x 7 | önden, çok ince pembe | yok | döndürüldü |
| chicken_5 | 15 x 15 | 3/4 yandan, kahverengi horoz | yok | döndürüldü |
| goose | 16 x 18 | önden, beyaz | yok | döndürüldü |
| goose_1 | 20 x 10 | önden, uzun boyunlu | yok | döndürüldü |
| goose_3 | 21 x 18 | tam yandan (sola bakıyor) | yok | döndürülmedi |
| duck | 12 x 11 | önden, kahverengi kuş (ördekten çok tavuğa benziyor) | yok | döndürüldü |
| rabbit | 16 x 14 | önden, kulaksız kahverengi küme (tavşan okunmuyor) | yok | döndürüldü |
| pigeon | 11 x 11 | yandan/üçte dört | yok | döndürülmedi |

## Teslim (`Assets/Art/Iso/_aday/`, 16 PNG)
| Gövde | Tuval | Pivot x (tuval ortası) | Pivot y alttan | Boy (scale_check, 8 yön) | Stil |
|---|---|---|---|---|---|
| chicken_5 | 22x22 | 11 | 1 | 15-19 (hedef 15-22) | 8/8 GEÇTİ (doygunluk x1,14-1,16) |
| goose_1 | 16x26 | 8 | 1 | 20-22 (hedef 17-26) | 8/8 GEÇTİ (x0,92-1,04) |

Dosyalar: `iso_animal_chicken_5_v2_{se,sw,nw,ne,s,w,n,e}.png`, `iso_animal_goose_1_v2_{se,sw,nw,ne,s,w,n,e}.png`. `validate_iso.py --kind object` SE: ikisi de geçti (chicken_5 opak 167 / 16 renk; goose_1 opak 138 / 17 renk; alt boşluk 1 satır). Animasyon işi bu tuval boylarını kullanmalı.

## Teslim edilmeyenler (`art/781-red/`, ham, `fit` yapılmadı)
| Gövde | Tuval | Yön boyları (px) | Hedef | Neden |
|---|---|---|---|---|
| cat_2 | 26x20 | e18 n17 ne18 nw16 se16 / s14 sw15 w14 | 16-24 | 3 yön 1-2 px düşük |
| chicken_3 | 14x22 | 14-16 (s14, se14 düşük) | 15-22 | 2 yön 1 px düşük |
| chicken_4 | 18x21 | 13-17 (s13, se14, sw14 düşük) | 15-22 | 3 yön 1-2 px düşük |
| goose | 20x19 | 14-17 (s16, se16, sw14, w16 düşük) | 17-26 | 4 yön 1-3 px düşük |
| duck | 14x17 | 11-15 | 17-26 | 8 yön de düşük (kuş maskeden küçük çizildi) |
| rabbit | 20x26 | 14-24 (e, n, ne, nw, se yüksek) | 11-17 | 5 yön yüksek (v3 hayvanı büyüttü; tavşan okunmuyor) |

goose_3 ve pigeon: yalnızca tek kare (`art/781-red/goose_3_tek_kare_yandan.png`, `pigeon_tek_kare.png`).

## Sorunlar ve öneri
- Tavuk 3, tavuk 4 ve ördek eski görselin rengini taşımıyor (pembe/kahve); tavuk 5 ayakta bir horozdan çok 3/4 poz. Kimlik eski tavuklarla ancak yaklaşık. Kullanıcı görsel kararı gerekir.
- PixelLab kuşu maskeden küçük çiziyor (ördek, tavuk 3/4). Öneri 781b: yalnızca reddedilen 8 gövde, maskeyi hedefin üst sınırına yakın büyüt (kaz 18x24, ördek 20x21, tavuk 20x20), tek kare kapısını sıkı tut (yandan ya da kulaksız çıkanı ele).
- Kedi ve tavuklarda güney tarafı yönleri (s, se, sw, w) 1-2 px kısa: v3 öne bakan hayvanı kısa çiziyor.
- `goose_3` yandan, `pigeon` belirsiz çıktı: tek deneme kuralıyla yeniden denenmedi.
- Kısmi teslim: oyun `_v2` yoksa eski görseli çizer (kod hazır). `supersedes` arşivi yalnızca chicken_5 ve goose_1 için uygulanmalı.

## Generation
| Çağrı | Gen |
|---|---|
| create_map_object x10 | 10 |
| create_character v3 ref x8 | 8 |
| Toplam | 18 (cost_max 34, cost_est 20) |

`get_balance` sonrası: 9143 (paralel şeritlerin harcaması dahil).

Karakter kimlikleri (animasyon için): cat_2 eb4183e2-aaa7-4617-adc8-6d71f7fc9231, chicken_3 0fdd8256-82cd-4898-a59b-8f1ea535213c, chicken_4 795aaf79-e455-4c1f-966a-1617bda1afa0, chicken_5 78f1c000-6f2e-4988-b7ca-239daa357261, goose 7373a526-650b-4fff-9c8b-ceb86db25777, goose_1 0f511ebf-c453-4804-a25d-534547b7c95f, duck 1ff2e65d-1c7a-46b5-859f-8e8776a7d9d4, rabbit e4f6c5a9-45cd-48a7-95e9-a4fd92306108.
