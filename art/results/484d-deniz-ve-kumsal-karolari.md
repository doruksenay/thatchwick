unity-import: pending

# 484d Deniz ve kumsal karoları: BAŞARISIZ (doğrulama)

- Araç: `create_tiles_pro`, stil modu (`style_images` = `iso_ground_grass_a`, `iso_ground_grass_b`, `iso_ground_dirt_a`, `iso_ground_water_a`; 64x32, satır içi base64), `outline_mode: segmentation` (zemin, ızgara çizgisi olmasın), tek çağrı. Maliyet çağrıdan önce okundu: "10-25 (tuval)", gerçek 10 (cost_max 15 içinde). Tile id: `7cb4cb5e-833a-4fea-b665-4d899cfe5d86`, 16 varyasyon (0-3 deniz dalga, 4-7 deniz köpük, 8-11 kumsal deniz kabuğu, 12-15 kumsal dalga izi; sütun düzeni, bkz. `art/results/484d-kontak-16.png`).
- Harcanan: 10 gen. `get_balance` sonrası: 9745 (ortak bakiye).
- Seçilenler: sea_a = t4, sea_b = t5, beach_a = t6, beach_b = t7.

## Doğrulama

| Dosya | Boyut | validate_iso | Stil denetimi | Dikiş oranı |
|---|---|---|---|---|
| iso_ground_sea_a | 64x32 | BAŞARISIZ (maske dışı 7, maske içi boş 21) | UYARI (palete uzak, x0,85) | 0,966 |
| iso_ground_sea_b | 64x32 | BAŞARISIZ (maske dışı 9, maske içi boş 15) | KALDI (palet dışı renkler, %16; `fit` düzeltmedi) | 0,891 |
| iso_ground_beach_a | 64x32 | BAŞARISIZ (maske dışı 1, maske içi boş 8) | UYARI (x0,97) | 1,005 |
| iso_ground_beach_b | 64x32 | BAŞARISIZ (maske dışı 1, maske içi boş 6) | UYARI (x0,92) | 1,023 |

Sorun: PixelLab elmasları 62 piksel genişlikte (x 1-62) ve kenarları tam 2:1 maskeye oturmayan biçimde çizdi. Maskeyle kırpmak yalnızca maske dışı pikselleri siler; maske içindeki 6-21 boş piksel kalır ve bunları doldurmak yeniden boyama olur (yasak). 16 varyasyonun hiçbiri (en az 5 boş piksel) geçmedi (`art/tmp484/scan.py` taraması). Hiçbir dosya `Assets/Art/Iso` altına teslim edilmedi. Yeniden deneme yok (bütçe kuralı); ham hâller `art/results/484d-red/`, 16'lı sayfa `art/results/484d-kontak-16.png`, yan yana sayfası `art/results/484d-yanyana.png`.

Görsel olarak deniz ve kumsal şekilleri tanınıyor (deniz koyu mavi-yeşil, kumsal açık sarı-gri, kabuk ve çakıl var); sorun yalnızca elmas maskesi. 346 su karolarında aynı yöntem geçmişti, bu çağrıda kenar boşlukları çıktı.

Masaüstü kopyası: yerel.

## Öneri
Kenar boşluklarını gideren bir araç (ör. maske içi boş pikseli en yakın komşu rengiyle dolduran `tools/art` betiği) yazılırsa bu 16 varyasyon yeniden harcama yapmadan teslim edilebilir; kullanıcı kararı gerekir (yeniden boyama kuralına istisna). Aksi hâlde yeni çağrı (10 gen).
