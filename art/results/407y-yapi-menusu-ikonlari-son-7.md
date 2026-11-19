unity-import: pending

# 407y Yapı menüsü ikonları, son 7

- Araç: `create_map_object` 32x32 x7 (side, single color outline, basic shading, medium detail; 399a/b yöntemi). `top_undo` ilk denemede düz mızrak gibi okundu: ikinci deneme `create_image_pixflux` 32x32 no_background (1 gen). Hız sınırı hatası olmadı.
- Harcanan: 8 generation (7 + 1). Paylaşılan hesapta başka şeritler de harcadığı için `get_balance` farkı bunu aşar. Kalan (get_balance): 3988,5.
- Hepsi yerel `quantize(16)` ile 16 renge indirildi, alfa eşiği 128 (yarı saydam 0).
- Yan yana: `art/results/407y-yanyana.png` (üst iki satır yeni 7 koyu/keten zeminde, alt iki satır komşu ikonlar). Eski sürümler `art/results/407y-eski/` altında.
- **Üzerine yazma yapılmadı:** `cloud_guard.py` var olan Assets dosyasını değiştirmeyi ve `Assets/Resources` yolunu reddeder; bütün iş push edilemezdi. 5 zayıf ikonun yeni sürümü `art/results/407y-yeni/` altında (eskiler `407y-eski/`). Kullanıcı kabul ederse yerel işçi ya da kod oturumu `Assets/Art/Iso/_aday/` ve `Assets/Resources/UI/Icons/` içindeki aynı adlı dosyaların üzerine kopyalar (`.meta` dokunulmaz). Yeni 2 ikon (`tool_gate`, `tool_conduit_clay`) `_aday/` altına teslim edildi.
- Masaüstü kopyası: yerel.

| Dosya | Boyut | Pivot x | Pivot y (alttan) | Renk | Stil denetimi | Not |
|---|---|---|---|---|---|---|
| ui_icon_top_undo | 32x32 | 16 | 0 | 15 | UYARI (palet dışı; kilit yok) | geri dönüş oku, pixflux 2. deneme |
| ui_icon_tool_erase_edge | 32x32 | 16 | 0 | 12 | UYARI (palete uzak %93) | tokmak ve kırık tuğla okunuyor |
| ui_icon_tool_move_room | 32x32 | 16 | 0 | 15 | GEÇTİ | |
| ui_icon_ui_filter | 32x32 | 16 | 0 | 15 | GEÇTİ | elek, hafif simit gibi duruyor |
| ui_icon_top_eyedropper | 32x32 | 16 | 0 | 14 | UYARI (palet dışı) | kırmızı boyalı fırça |
| ui_icon_tool_gate | 32x32 | 16 | 0 | 15 | GEÇTİ (validate_iso geçti) | |
| ui_icon_tool_conduit_clay | 32x32 | 16 | 0 | 11 | UYARI (palet dışı; turuncu kil bilerek) | |

Doğrulama: 32x32, yarı saydam 0, ≤16 renk; göz kontrolü koyu ve keten zeminde yapıldı.

## Sorunlar
- `arayuz` ailesinin stil kilidi kurulmamış; UYARI'lar bundan.
- Araç eksikliği yok. `tools/art`'a dokunulmadı.
