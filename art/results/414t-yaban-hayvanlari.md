unity-import: pending

# 414t Yaban hayvanları tamamlama: 5/5 tür, 40 PNG teslim (review)

Yan yana sayfası (her türün SE yönü, koyun ve köpek referanslı): [414t-yanyana.png](414t-yanyana.png). Teslim edilmeyen ilk tilki karesi: `art/results/414t-red/`. Masaüstü kopyası: yerel (bulut modu).

## Araç ve ayarlar
- 414/375'ten fark: **palet zorlaması yok** (koyun paleti 414'te tilkiyi griye çevirmiş ve zemin lekesi getirmişti). `create_image_pixflux`, `high top-down`, black outline, `direction=south`, `no_background`, medium detail, basic shading; açıklama: "seen straight from the front, head-on, no ground shadow, no floor, earthy muted medieval palette, desaturated". Tuvaller: tilki 40, karaca 48, kurt 44, domuz 44, ayı 56.
- Döndürme: `create_character` mode v3 + referans, quadruped, `high top-down`. Şablonlar: tilki dog, karaca horse, kurt dog, domuz bear, ayı bear. 1 gen/tür.
- Zemin gölgesi: kurt, domuz ve ayının tek karesinde gri zemin lekesi vardı. Yalnızca maske ile temizlendi (alt 10 satırda lekenin tek rengi şeffaf yapıldı: kurt 74, domuz 72, ayı 89 px; hayvan içeriği yeniden boyanmadı). Domuz referansı base64 boyutu için 12 renge nicemlendi (ilk iki çağrı kesik base64 hatasıyla reddedildi, ücret yok).
- Tilki: ilk kare oturuyordu; "standing" vurgulu ikinci pixflux yine oturan çıktı (max_attempts 2 doldu). Oturan tilki teslim edildi (414'teki gibi), kızıl renk tamam.
- `style.py fit --family hayvan`: tilki x0,66-0,78 ve karaca x0,74-0,79 (ham doygunluk x1,5-1,75 ile KALDI'ydı); kurt, domuz, ayı ham teslim.
- Kurt ve domuz yönleri v3 yön kaymasından dolayı ham görüntüde "3/4 yandan" referanstan türedi: bkz. Sorunlar.

## Tek kare kontrolü
| Tür | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|
| fox (v2) | önden, oturan (2 deneme) | yok | teslim |
| roe_deer | önden/3-4, ayakta | yok | teslim |
| wolf | 3/4 yandan | var (temizlendi) | teslim |
| boar | 3/4 | var (temizlendi) | teslim |
| bear | 3/4 önden | var (temizlendi) | teslim |

## Stil denetimi (`check --family hayvan`, teslim edilen 40 dosya)
Hayvan ailesi kilidi kurulmadı. Sonuç: 0 KALDI, 22 UYARI, 18 GEÇTİ.
| Tür | Sonuç (8 yön) | Doygunluk / aile |
|---|---|---|
| fox_v2 | 8 UYARI (palet dışı renkler / palete uzak: kızıl, aile gri-kahve) | x1,13-1,17 (fit sonrası) |
| roe_deer | 8 GEÇTİ | x1,14-1,17 (fit sonrası) |
| wolf | 6 UYARI (palete uzak), 2 GEÇTİ | x0,61-0,68 (soluk) |
| boar | 8 UYARI (ailesinden soluk) | x0,44-0,46 |
| bear | 8 GEÇTİ | x1,04-1,10 |

`validate_iso.py --kind object` SE: 5 türün hepsi geçti. Yarı saydam piksel 0. Işık sol-üstten, gölge pişirilmemiş.

## Dosyalar (`Assets/Art/Iso/_aday/iso_wild_<tür>_cand_{se,sw,nw,ne,s,w,n,e}.png`, 40 PNG)
| Tür | Boyut | SE opak kutu | Pivot x (SE) | Pivot y (alttan) |
|---|---|---|---|---|
| fox_v2 | 40x40 | x11-28, y4-35 | 18,0 | 4 |
| roe_deer | 48x48 | x13-33, y2-45 | 19,8 | 2 |
| wolf | 44x44 | x12-32, y3-41 | 18,4 | 2 |
| boar | 44x44 | x11-35, y4-39 | 19,5 | 4 |
| bear | 56x56 | x11-45, y2-52 | 28,5 | 3 |
Diğer yönlerin pivotları aynı alt-ortalama kuralıyla hesaplandı (pivot y yönler arası 2-5 oynar). Ayı boyu 51 px, köylünün (≈56) altında görünür; tuval sınırı 56 olduğundan ölçek uyumu kullanıcı kararıdır.

## Generation
| Çağrı | Gen |
|---|---|
| pixflux x6 (5 tür + tilki ikinci deneme) | 6 |
| create_character v3 ref x5 | 5 |
| Toplam | 11 (cost_max 60) |

`get_balance` sonrası: 2663,9 (paralel şeritler dahil).

## Sorunlar
- Tilki oturuyor (v3 şablon dog); ayakta istenmişti, 2 denemede tutmadı.
- Kurt, domuz, ayı referansı tam önden değil (3/4) olduğundan v3 yönleri biraz kayık olabilir (yan yana tabloda SE yönü önden görünüyor: v3'te `south` referansın kendisi). Kod eşlemesi `IsoArt.cs` yön adlandırmasını kontrol etmeli.
- Kurt ve domuz aileden soluk (UYARI); tilki palet dışı (kızıl, gri-kahve aile paletinde yok): tasarım gereği.
- Ayı köylüden iki kat iri değil (56 px tuval sınırı); daha büyük tuval gerekirse yeni iş.
