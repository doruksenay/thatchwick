unity-import: pending

# 393 Yapı menüsü ikonları: inşa araçları

Sonuç: 32 ikon üretildi, **16 teslim edildi, 16 okunmadığı için teslim edilmedi** (`art/results/393-red/`). 4 ikon (conduit_ditch, stone, flume, erase) `Assets/Art/Iso/_aday/` altında zaten vardı; üretilmedi, üzerine yazılmadı.

- Araç: `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, `seed=0`; tarifteki ortak ek ve palet. Her ikon ayrı çağrı, 1 gen. Maliyet `cost_est` ile aynı (araç açıklaması: 1 gen).
- Eşzamanlılık sınırı (10 iş, diğer şeritlerle ortak) nedeniyle çağrılar 4 dalgada gönderildi; reddedilenler (üretim yok, harcama yok) slot açılınca bir kez yeniden gönderildi.
- Toplam harcama: 32 gen. `get_balance` sonrası: 4429,9 (hesap genelinde, diğer şeritler dahil).
- Stil referansı `style_image` olarak verilmedi (pixen desteklemiyor); yalnızca açıklama eki.
- Yan yana sayfası: `art/results/393-yanyana.png` (referans yok: arayuz ailesinin kilidi kurulmadı).
- Masaüstü kopyası: yerel.

## Teslim edilen (16), 32x32, pivot x 16, pivot y 16 (ortalı ikon)
conduit_lead, door, finish, finish_room, floor_room, grass, halfwall, level_build, level_remove, plot, pond, railing, road_erase, window, zone_erase, zone_storage (hepsi `ui_icon_tool_<ad>.png`).

⚠ Hepsinde renk sayısı hedefin (≤12) çok üstünde (20-88; pixen gradyan/antialias). door ve level_build çerçeveli/dolu kare (şartta "çerçevesiz"). Kullanıcı kararı gerek.

## Teslim edilmeyen (16): okunmuyor / yanlış nesne
| İkon | Gördüğümüz |
|---|---|
| room | yaratık |
| wall | ot çatılı kulübe |
| erase_edge | taş blok üstünde kırmızı taş, çarpı yok |
| push_wall | kırmızı yuvarlak yaratık |
| move_room | yalnızca dört yönlü ok, oda yok |
| copy_room | hayvan |
| floor | kedi yüzü |
| roof | yüzlü torba |
| fence | çitte tilki |
| gate | küçük ev |
| road_path | ağaçlı ada |
| road_gravel | tilkili kaya |
| road_paved | yaratıklı kaplama |
| conduit_clay | boynuz/solucan |
| conduit_wood | yüzlü yaratık |
| zone_pasture | çerçeveli sahne |

Yeniden deneme yok (kural). Öneri: bu 16 için `create_image_pixflux` ya da referanslı yöntem, ya da elle yeniden tarif; ayrı iş.

## Doğrulama
| Denetim | Sonuç |
|---|---|
| Boyut 32x32, yarı saydam 0 | 32/32 geçti |
| `style.py check --family arayuz` | 0 KALDI; GEÇTİ: clay, lead, door, fence, floor_room, halfwall, level_remove, railing. Kalanı UYARI (palet dışı/uzak renk; kilit yok) |
| Göz kontrolü (şekil okunurluğu) | 16 geçti / 16 geçmedi (yukarıda) |
| `validate_iso.py` / döşeme testi | uygulanmaz (UI ikonu, zemin değil) |
