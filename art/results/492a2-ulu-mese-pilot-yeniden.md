unity-import: pending

# 492a2 Ulu meşe (yeniden): ENGELLİ, teslim yok

Masaüstü kopyası: yerel. Yan yana sayfası üretilmedi (teslim edilecek dosya yok).

## Araç ve ayarlar
`create_map_object` stil eşleme, özel 1-bit maske (160x224; gövde-taç bandı x 4-155, y 10-190 + 2x2 elmas ayak izi, merkez (80,188), pivot (80,36)), high top-down, low detail, basic shading, single color outline. Açıklama tarifteki (platform yok). 1 çağrı = 1 gen. İlk çağrı maske base64'ü aktarımda bozulduğu için doğrulamada reddedildi (ücret yok); küçük maskeyle bir kez yeniden gönderildi.
Mevsimler: `edit_image_pro_flash` x3 başlatıldı, araç 9 gen/görsel gösterdi (cost_est 6 aşılıyordu); üçü de hemen iptal edildi.

## Sonuç
| Dosya | Piksel | Pivot x | Pivot y (alttan) | Durum |
|---|---|---|---|---|
| iso_greattree_oak_summer_se.png | 160x224 | 80 | 36 | teslim edilmedi (`492a2-red/`) |

validate_iso: geçti (35 renk, opak x 6-153 y 12-204, alt boşluk 19). style.py check bitki: KALDI (doygunluk x1.66, palet %100, uzaklık 8.2). Platform/ev yok.

get_balance (iş sonu): 9923 (paralel şeritler dahil; bu işin payı 1 + iptal edilen 3 çağrı için belirsiz).

## Sorunlar
Bkz. iş dosyası `## Sorular`: edit maliyeti 9 gen/görsel, cost_max 12 yetmiyor.
