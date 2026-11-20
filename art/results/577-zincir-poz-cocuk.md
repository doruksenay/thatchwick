unity-import: pending

# 577 Zincir pozları, çocuklar: KISMİ teslim (yalnızca `wash`, 8/32 klip)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = yönün durma karesi (karakterin `rotations/<yön>.png` https adresi), tarifteki metinler aynen. 32 çağrı x 1 generation (tahmin 32, tavan 44). Yön başına ayrı çağrı ve ayrı `animation_name` (`sit_se` vb.); eşleme ("already queued") sorunu çıkmadı.
- Hız sınırı: yuvalar (10/10) başka şeritlerce doluydu; reddedilen çağrılar yuva açılınca bir kez yeniden gönderildi (hepsi geçti).
- Harcanan: 32 generation. `get_balance` sonrası: 1201,7 (diğer şeritlerin eşzamanlı harcaması dahil). Defter: art/ledger/577.md.
- Karakterler: boy `6940a34e-75c4-4cb6-b88d-8ef7e0e7719e`, girl `9d58ae77-0243-403e-84d3-33799a2f7e70`. Ham klipler PixelLab'de (`<anahtar>_<yön>` adlı).
- Masaüstü kopyası: yerel

## Sonuç özeti
| Anahtar | Karar | Neden |
|---|---|---|
| `wash` | TESLİM (8 klip, 64 kare) | Poz okunuyor: eller yüze, sıçrama. |
| `sit` | RED | Hiçbir yönde oturmuyor; karakter ayakta duruyor, tabure yok. |
| `lie_sleep` | RED | Yatmıyor; ayakta, yalnızca gözler kapanıyor. |
| `sit_eat` | RED | Yalnızca erkek SE tabureli oturuyor; ötekiler ayakta kasenin başında (kız dahil), kare 0/1/7 durma pozu. |

Kök neden: bitiş karesi olarak ayakta durma pozunun verilmesi ve başlangıç karesinin ayakta olması, pixminimax'i oturma ya da yatma pozuna götürmüyor. Tek deneme kuralı gereği yeniden denenmedi. Reddedilenlerin örnekleri: `art/results/577-red-sit-lie-eat.png` (ham kareler teslim edilmedi, PixelLab'de duruyor).

Öneri (yeni iş, `## Sorular`): `sit`, `sit_eat`, `lie_sleep` için önce oturan/yatan duruş karelerini `create_object_state`/`edit_image_pro_flash` ya da `custom_start_frame` (oturan başlangıç karesi, 8 yönden her biri için) ile üretmek, animasyonu bu başlangıç karesinden yürütmek (`end_frame` vermeden ya da aynı oturan kareyle). Tabure ve yatak ayrı nesne olarak çizilir, yoksa karakterle bütünleşmiş tabure palete girer.

## İşleme (ücretsiz)
1. Ham klip 9 kare (0 = durma karesi, 1-8 hareket). Kare 0 atıldı; kareler 1-8 `00..07` olarak adlandırıldı (kare 0 durma pozu olduğu için döngüye alınmadı).
2. Her yön için kare 0'ın alt-opak satırı ve yatay kutu ortası, karakterin durma karesine (rotation) oturtuldu; aynı kaydırma bütün karelere uygulandı, 64x64'e maskeyle kırpma (ölçekleme yok; kırpılan piksel 0).
3. Palet: karakterin sekiz yön durma karelerinden en sık kullanılan 32 renk, en yakın RGB; alfa ikili. Beyaz su sıçraması bu yüzden bej lekeye dönüştü (renk kaybı); okunabilir ama boyalı görünüyor.
4. Döngü kapanışı: kare 07 (ham 8) ile 00 aynı poz değil, kare 07 durma pozuna dönüş kareleri (yani 07 -> 00 geçişi pürüzsüz değil, ham 8 hedef durma karesine yakın, 00 ise ham 1'in hareket başı). Kodda döngü yerine ileri-geri ya da bir kez oynatma önerilir.

## Dosyalar (Assets/Art/Iso/_aday/anim/, 64x64)
Pivot x = 32. Pivot y alttan: en alt opak satır erkekte y=62 (alttan 1 boş satır), kızda y=60 (alttan 3 boş satır, kız SE/SW/NW) ya da y=59 (NE; 4 boş satır), yani karakterin durma karesinin ayak satırıyla aynı; pivot y = durma karesiyle aynı alt boşluk (015c'de verilen y=2 ile uyumlu: oyun yüklemesi durma karesindeki pivotu kullanmalı).
| Yol | Kare |
|---|---|
| iso_char_boy_r64_wash_{se,sw,nw,ne}_00..07.png | 4 yön x 8 |
| iso_char_girl_r64_wash_{se,sw,nw,ne}_00..07.png | 4 yön x 8 |

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (--kind object --max-colors 32) | 64/64 geçti |
| style.py check --family karakter | 64 GEÇTİ, 0 UYARI, 0 KALDI |
| Ayak satırı sabit | Evet (durma karesiyle aynı kaydırma tüm karelerde) |
| Kimlik (giysi, saç) | Durma karesiyle aynı; yüz ayrıntısı kareler arasında hafif oynuyor |
| Tepe kırpılması | Yok |

## El konumları (`hand_anchors.txt` için)
Ayrıca ölçülmedi: `wash` klibinde eller yüzde, taşınan nesne yok. Oturma klipleri reddedildiği için sofra eli anchor'ı verilmedi.

## Görseller
- Yan yana: art/results/577-yanyana.png
- Tüm `wash` kareleri (8 yön x 8 kare): art/results/577-wash.png
- Reddedilenler: art/results/577-red-sit-lie-eat.png
- GIF üretilmedi (ortamda hazır GIF aracı yok; kare şeritleri yukarıda).

## Sorunlar
- 3/4 anahtar (24 klip) tarifin yöntemiyle çalışmadı; yeni iş gerekir (yukarıdaki öneri). 24 generation boşa gitti.
- Su sıçraması paletle bej lekeye dönüştü.
- Kare 0 (durma) kırpıldığı için 8 kare = ham 1-8; döngü kapanışı pürüzlü.
