unity-import: pending

# 575 Zincir pozları, yıkanma (bathe, dry_off): TESLİM EDİLMEDİ (failed)

**Özet:** 16 klip üretildi (16 gen) ama yanlış karakter durumuna uygulandı; hiçbiri teslim edilmedi. Tek deneme kuralı gereği yeniden üretim yapılmadı.

## Ne oldu (işçi hatası)
- Tarif "574'te üretilen peştamallı karakter" diyor. 574 çıktısı yeni karakter kimlikleridir: erkek `fc7d28c4-a86c-4565-88c1-cced1dd174b7`, kadın `445ff76c-f6ef-46c5-ab67-e072127bdc91` (durum adı Pestamal).
- Ben `animate_character` çağrılarını 573'teki gibi eski giyinik durum kimliklerine gönderdim: erkek `c63fe39d-...` ve kadın `135cb0b1-...` (grup içindeki `Idle` durumu). Bitiş karesi (`end_frame_url`) doğru, yani peştamallı rotasyon görüntüsüydü; başlangıç karesi ve kimlik ise giyinik karakterden geldi.
- İndirilen ZIP'te kare 0 giyinik köylü (kahverengi giysi, çizme). Sonuçta kareler giyinik karakterden peştamallı poza doğru kayıyor ya da giyinik kalıyor; peştamallı gövde seti değil. Gözle erkek SE `bathe` kare 0 kontrol edildi.
- Kare işleme, doğrulama ve teslim yapılmadı. `Assets/Art/Iso/` altına dosya konmadı.

## Maliyet
- 16 çağrı x 1 gen = 16 gen (`cost_est` ile aynı). Slot doluluğu hatası (10/10) alan çağrılar faturalanmadı, bekleyip yeniden gönderildi.
- `get_balance` sonrası: 1201,7 / 5000 (Tier 2). Paralel şeritler de harcadığı için bu sayı yalnızca bu işe ait değil.

## Yeniden üretim için gerekenler
- Çağrılar `character_id` = `fc7d28c4-a86c-4565-88c1-cced1dd174b7` (erkek) ve `445ff76c-f6ef-46c5-ab67-e072127bdc91` (kadın) ile yapılmalı (Pestamal durumu). `end_frame_url` aynı yönün rotasyon görüntüsü. Yeni deneme kullanıcı onayı ya da yeni iş (`cost_est` 16) ister.
- Yanlış üretilen klipler PixelLab hesabında duruyor (Idle durumları, adlar `bathe` ve `dry_off`); silinmedi. Ham ZIP kareleri `art/tmp575/` altında (commit edilmiyor).
- 573 raporundaki `end_frame` yöntemi `Idle` durumuna uygundu; peştamallı işlerde (575 gibi) durum kimliği değil, 574'ün yeni karakter kimliği kullanılmalı. İş dosyası tarifine "karakter kimlikleri: 574 raporundaki yeni kimlikler" satırı eklenmesi bunu önler.

## Doğrulama
Yapılmadı (teslim yok). Masaüstü kopyası: yerel.
