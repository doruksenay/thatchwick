---
id: "009"
title: Karakter katman sistemi fizibilite raporu
status: pending
approved_by: auto
cost_est: 0
cost_max: 0
depends_on: []
max_attempts: 1
pixellab: false
outputs:
  - art/results/009-katman-fizibilite.md
created: 2026-11-08
---
# 009 Karakter katman sistemi fizibilite raporu

## Amaç
Karakter yaratma ekranı katmanlı sprite + renk değişimi ister. `art-brief-tur4.md` Faz A'nın 0. ve 5. maddeleri: kombinasyon patlaması yüzünden önce rapor. Bu iş **kredi harcamaz** ve PixelLab'e dokunmaz.

## Tarif
`art-brief-tur4.md` Faz A'yı ve `Assets/Resources/Content/appearance.json`'ı oku. Şunları yaz:
- (a) Mevcut köylüyü (`iso_char_villager_*.png`, 68x68) baz alıp katmanları Python/PIL ile çıkarmak (renk bölgesinden ayırma, gri tonlama, elle piksel düzenleme) mümkün mü? Köylünün palet bölgeleri üzerinden somut ölçüm yap (saç/ten/tunik bölgeleri ayrılabiliyor mu, kaç piksel).
- (b) PixelLab'in hangi araçları (varyant/düzenleme) buna yarar ve yaklaşık generation maliyeti.
- (c) Kredisiz üretilebilecek kısım.
- Toplam görsel sayısı tahmini: 8 yön x 3 vücut tipi x katman sayısı (appearance.json'daki kimliklerle), ucuz ve tutarlı yol önerisi.
- Yaş evreleri için ayrı gövde katmanı mı ölçek mi önerisi (kod şu an bebek ve çocukta ölçek kullanıyor).
- Pilot önerisi: SE yönü, Average vücut. Kredi gerektiren her adımı ayrı iş dosyası taslağı olarak sona ekle (yazmak yerine metin olarak öner; kuyruğa sen ekleme).

## Doğrulama
Raporda sayılar ölçülmüş olmalı (piksel sayıları, bölge sayıları), tahmin ise "tahmin" diye belirtilmeli.

## Notlar
Çıktı yalnızca `art/results/009-katman-fizibilite.md`. `Assets/` altına dosya koyma.
