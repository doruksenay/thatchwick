unity-import: pending

# 399a Yapı menüsü ikonları yeniden (1/2)

Yan yana sayfası: `art/results/399a-yanyana.png` (21 yeni + 36 eski + eski 36'nın `_q16` hâli; referans kilidi olmadığı için "0 referans" yazıyor).

## Araç ve ayarlar
- `create_map_object` 32x32, `view: side`, `single color outline`, `basic shading`, `medium detail`, 21 çağrı. Araç `outline: single color black outline` ve `style_image` desteklemiyor, bu yüzden `single color outline` kullanıldı, stil referansı verilemedi. Hız sınırı (rate limit) yüzünden çağrılar 3 dalgada gönderildi; reddedilenler bekleyip aynen yeniden gönderildi.
- 2. deneme (`create_image_pixflux` 32x32, `no_background`, `single color black outline`): `tool_erase_edge`, `tool_room`, `top_undo`, `tool_move_room`.
- Renk azaltma: Pillow ile 16 renk (medyan kesim, dither yok), alfa 0/255. Teslim edilen 21 yeni ikon azaltılmış hâlde. 392-394'ten teslim edilen 36 ikonun `_q16.png` kopyaları yanlarında (orijinallere dokunulmadı).
- Maliyet: araç açıklamasında yazmıyor; ~1 gen/çağrı varsayıldı, toplam ~25 gen (`cost_max` 46 altında). `get_balance` sonrası: 4313,9.

## Dosyalar (hepsi `Assets/Art/Iso/_aday/`, 32x32, pivot x 16, pivot y 0 alttan merkez)
| Dosya | Deneme | Not |
|---|---|---|
| ui_icon_tab_build | 1 | çekiç, okunuyor |
| ui_icon_tab_items | 1 | sandalye, okunuyor |
| ui_icon_tab_inventory | 1 | sandık, okunuyor |
| ui_icon_top_level_basement | 1 | tahta kapak, okunuyor |
| ui_icon_top_level_upper | 1 | merdiven, okunuyor |
| ui_icon_top_walls_up | 1 | çerçeveli panel, orta okunurluk |
| ui_icon_top_walls_cut | 1 | çerçeveli panel, orta okunurluk ("yarım yükseklik" belirgin değil) |
| ui_icon_top_walls_down | 1 | taş sırası, okunuyor |
| ui_icon_top_roof | 1 | saz çatılı kulübe (çatı tek başına değil), UYARI palet |
| ui_icon_top_eyedropper | 1 | cam şişe, kırmızı tıkaç: damlalıktan çok iksir şişesi; UYARI palete uzak |
| ui_icon_top_sledgehammer | 1 | balyoz, ince sap, okunuyor |
| ui_icon_top_rooms | 1 | parşömen, kurdele yok; okunuyor |
| ui_icon_top_undo | 2 | 2. denemede ok SAĞA geldi; yatay çevrildi (yeniden boyama yok). Ok eğri değil, düz |
| ui_icon_ui_comfort | 1 | kırmızı yastık; UYARI palet (bordo ağırlıklı) |
| ui_icon_ui_filter | 1 | huni/çan, orta okunurluk |
| ui_icon_ui_lock | 1 | asma kilit, okunuyor |
| ui_icon_tool_room | 2 | izometrik oda kesiti, okunuyor |
| ui_icon_tool_wall | 1 | tuğla duvar parçası, okunuyor |
| ui_icon_tool_erase_edge | 2 | çatlak taş levha; "kırık duvar" zayıf ama tanınıyor (1. deneme gürültü çıktı) |
| ui_icon_tool_push_wall | 1 | panel ve beyaz ok, okunuyor |
| ui_icon_tool_move_room | 2 | küçük ev; okun olmaması gerekçesiyle "taşı" anlamı zayıf, 1. deneme ev + göz benzeri leke çıktı |

Teslim edilmeyen yok. 5 ikon zayıf okunurlukta (undo, erase_edge, move_room, filter, eyedropper); kullanıcı kararı gerek. Tüm 21 ikon yaratıksız/insansız.

## Doğrulama
- Boyut 32x32, yarı saydam 0 (alfa yalnızca 0/255), ≤16 renk: hepsi geçti.
- `validate_iso.py` ikon (tek dosya, izometrik değil) için uygulanamadı; atlandı.
- `style.py check --family arayuz` (kilit kurulmadı, palet uyumu uyarı): 18 GEÇTİ, 3 UYARI (top_roof %62, top_eyedropper palete uzak, ui_comfort %10 palet).
- Göz kontrolü yalnızca keten zeminde yapıldı (koyu zemin sayfası üretildi, incelenmedi).

## Sorunlar / öneri
- `create_map_object` stil referansı almıyor; `style_image` destekleyen araçla (ör. `create_image_pro_from_style`) 5 zayıf ikon yeniden denenebilir.
- `tools/art` değişikliği yapılmadı.
- Masaüstü kopyası: yerel.
