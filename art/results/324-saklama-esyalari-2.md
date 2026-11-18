unity-import: pending

# 324 Saklama eşyaları 2 (rapor)

Yan yana sayfası: [324-yanyana.png](324-yanyana.png). Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 2 çağrı x 1 gen = 2 generation (cost_max 5). İkinci deneme gerekmedi. (Peynir presi ilk çağrısı base64 aktarım hatasıyla reddedildi, üretim başlamadı; kılavuz yeniden kodlanıp gönderildi, harcama yok.) `get_balance` iş sonu: 848,10 (paralel şeritler de harcıyor).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_smokehouse_2x1_se.png | 112x96 | 56 | 32 |
| iso_item_cheese_press_se.png | 72x64 | 40 | 24 |

## Doğrulama
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| tütsülük | x11-102, y8-88 | 43 | 0 | yok (7 satır boşluk) | geçti |
| peynir presi | x11-60, y8-56 | 33 | 0 | yok (7 satır boşluk) | geçti |

## Stil denetimi
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| peynir presi | %100 | 9.0 | x0.87 | GEÇTİ |
| tütsülük | %99 | 8.4 | x0.72 | GEÇTİ |

## Gözlem (görsel, ölçüm değil)
- Tütsülük: koyu ahşap kulübe, kil baca borusu ve tek açık gri duman izi; kapı SE yüzünde, yan yana sayfasına bakılarak teyit edilmeli. Et şeritleri küçük pencerede seçilemiyor olabilir; 312 fıçısıyla renk ailesi kıyası kullanıcıya bırakıldı.
- Peynir presi: koyu ahşap gövde, üstte soluk peynir tekerleği ve kol; taş ağırlık belirsiz. Boy 49 px (köylü ≈48 px sınırında).
- Dış mekân (çimen+toprak) üzerinde okunurluk ve ışık yönü ölçümü yapılmadı; kullanıcı sayfadan karar verir.
- Ayna: 2x1 tütsülük simetrik değil (kapı SE), diğer yönler sonraki iş; peynir presi asimetrik.
