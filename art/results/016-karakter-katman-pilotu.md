unity-import: pending
# 016 Karakter katman pilotu: BLOCKED (kısmi)

- Yapılan: tek çağrı, adım 1 (kel taban SE), `inpaint_image_pro_flash`, 60x60 kaynak, `Modify current layer`, maske = baş (saç, sakal, dış hat) + gövde önü, 1 px genişletilmiş (HLS sınıflandırması). Maliyet 5 gen (tahminle aynı). Job `4db28afb-32a5-499e-ae76-0874dcec4384`, image `3f59b7f4-c710-5fb5-b1d6-95f73756736f`.
- Sonuç görsel olarak makul (kel, keten gömlek), ama **PNG diske alınamadı**: bulut oturumunda MCP çıktısı yalnızca korumalı `~/.claude/.../tool-results/` altına düşüyor (kopyalama izni yok) ve `curl` ile indirme onaylanmadı. Doğrulama, teslim ve sonraki adımlar (4 katman, 8 yön) yapılamadı. Boşa harcama olmasın diye durdum.
- Teslim edilen dosya yok. Doğrulama ve stil denetimi yapılmadı.
- Öneri: işçi izinlerine `api.pixellab.ai` indirmesi (curl) ya da tool-results yolundan `cp` eklenmeli; sonra iş `pending` yapılıp yeniden çalıştırılmalı. Kel taban image_id yukarıda; `source_image_id` ile yeniden kullanılabilir.
- Not: maske yüzü de kapsadı (baş dış hattı dilatasyonu); sonraki denemede yüz çekirdeği maskeden çıkarılmalı.
