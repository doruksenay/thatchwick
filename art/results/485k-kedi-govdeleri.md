unity-import: pending

# 485k Kedi gövdeleri: 1/6 teslim (orta uzun tüylü), 5 gövde güney kontrolünü geçemedi

Sonuç: `status: review` (kısmi). Altı tek kareden yalnızca `cat_medium_long` önden bakıyordu; döndürüldü ve 8 yön teslim edildi. Öteki beşi yandan (sola bakan yürüyüş pozu) çıktı, 780'deki küçük köpekle aynı sorun. Tek deneme kuralı gereği yeniden denenmedi ve döndürülmedi; tek kareler `art/485-red/` altında. Masaüstü kopyası: yerel.
Yan yana sayfası (yeni orta uzun SE ve S, kabul edilen `cat_1_v2` SE; hayvan ailesi referansları): `art/results/485k-yanyana.png`.

## Araç ve ayarlar
- `create_map_object` 32x32, stil eşleme, özel maske (780 yöntemi): kılavuz = `iso_animal_cat_1_cand_se` hedef kutuya en yakın komşuyla küçültülmüş, gri zeminde; maske aynı kutu (orta 22x20, iri 24x22, ince 20x20; alt kenardan 2 px yukarı). `high top-down`, `single color outline`, `medium detail`, `basic shading`; açıklama = tablo metni + "seen straight from the front facing the viewer, standing on all four legs" + 485k eki (nötr gri-kahve, desensiz) + 106 ortak eki + 375 eki.
- Döndürme (yalnızca orta uzun): `create_character` v3, 8 yön, quadruped `cat`, `high top-down`, referans = tek karenin indirme URL'si. Karakter kimliği: **a4756391-bdcd-4107-9184-3cf978e56e14** (`cat_medium_long_v2`).
- Renk: `style.py fit --family hayvan` doygunluğu x1,00 bıraktı (ham zaten uyumlu); `check` doygunluk x0,74-0,82, 8/8 GEÇTİ. Kırpma yalnızca tuval sınırı (içerik yeniden boyanmadı).
- Hız sınırı hatası olmadı. İki çağrı (kılavuz base64'ü) iletimde bozulduğu için istek reddedildi (kod çözme hatası), üretim başlamadı, ücret yazılmadı; kılavuz yeniden kodlanıp bir kez gönderildi.

## Tek kare kontrolü
| Gövde | Boy x en | Güney yüzü | Zemin lekesi | Karar |
|---|---|---|---|---|
| cat_medium | 18 x 12 | yandan (sağa bakıyor) | yok | döndürülmedi |
| cat_medium_long | 18 x 18 | önden, yüz görünüyor | yok | döndürüldü |
| cat_large | 24 x 20 | yandan (sola bakıyor) | yok | döndürülmedi |
| cat_large_long | 24 x 22 | gövde 3/4 yandan, baş önde; sınırda | yok | döndürülmedi (tek deneme, sınırda) |
| cat_slim | 20 x 15 | yandan | yok | döndürülmedi |
| cat_slim_long | 17 x 15 | yandan | yok | döndürülmedi |

## Teslim (`Assets/Art/Iso/_aday/`, 8 PNG)
Tuval 34x26 (8 yönün birleşik kutusu, pivot sütununa göre simetrik, üstte 1 px, SE ayak tabanı alttan 2. satır). **Pivot x 17 (tuval ortası), pivot y alttan 1.** Animasyon işi aynı 34x26 tuvali kullanmalı.

| Dosya | Boyut | Boy (scale_check) | Stil |
|---|---|---|---|
| iso_animal_cat_medium_long_v2_s | 34x26 | 18 | GEÇTİ (x0,82) |
| iso_animal_cat_medium_long_v2_se | 34x26 | 20 | GEÇTİ (x0,81) |
| iso_animal_cat_medium_long_v2_sw | 34x26 | 20 | GEÇTİ (x0,81) |
| iso_animal_cat_medium_long_v2_e | 34x26 | 22 | GEÇTİ (x0,79) |
| iso_animal_cat_medium_long_v2_w | 34x26 | 22 | GEÇTİ (x0,79) |
| iso_animal_cat_medium_long_v2_n | 34x26 | 24 | GEÇTİ (x0,74) |
| iso_animal_cat_medium_long_v2_ne | 34x26 | 23 | GEÇTİ (x0,75) |
| iso_animal_cat_medium_long_v2_nw | 34x26 | 23 | GEÇTİ (x0,75) |

`validate_iso.py --kind object` SE: geçti (opak 272, yarı saydam 0, renk 11, alt boşluk 1 satır). Gölge pişirilmemiş, ışık sol-üstten.

## Sorunlar
- Beş gövde teslim edilmedi: `create_map_object` kılavuzlu küçük kedide "yüz önden" tarifine rağmen yandan çiziyor (780 köpeğiyle aynı). Öneri (yeni iş): simetrik, önden bakan çizilmiş bir kılavuz (örn. kabul edilen `cat_1_v2` S yönünün ölçeklenmiş hâli) kullanmak ya da kabul edilen orta uzun gövdeden `transfer_outfit`/yeniden boyama ile boy farkı üretmek. Kısa tüylü ve ince/iri gövdeler bu yüzden eksik; belge 48 altı tipin hepsini ister.
- Orta uzun gövdede gözler iri ve kıvrık kuyruk tuvalin dışına çıkmadı ama bıyıksız; kimlik kullanıcı kararı gerekir.

## Generation
| Çağrı | Gen |
|---|---|
| create_map_object x6 | 6 |
| create_character v3 ref x1 | 1 |
| Toplam | 7 (cost_max 22) |

`get_balance` sonrası: 9440 (paralel şeritlerin harcaması dahil).
