---
id: "213"
title: Arayüz ekranları planı (kalan ekranlar için yerleşim + iş dosyası yazar)
status: running
claimed_at: 2026-11-18T12:45Z
approved_by: user        # otomatik akış: kullanıcı onayı (tools/art/approve_all.py)
family: arayuz
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["210"]
max_attempts: 1
pixellab: false
outputs:
  - tools/art/ui_layouts.json (yeni ekran anahtarları)
  - art/queue/213a-*.md ... art/queue/213z-*.md
  - art/results/213-arayuz-ekranlari-plani.md
created: 2026-11-17
---
# 213 Arayüz ekranları planı

## Amaç
Plan işi: PixelLab çağrılmaz. 210'da seçilen yönle oyunun karakter dışı ekranları için yerleşim ve **iş dosyası yazar**. 200 ana menüyü, 211 karakter ekranlarını, 212 ve 315 ikonları kapsıyor.

## Tarif
1. **Ekranlar:**
   - Unity'deki geçici IMGUI panellerine bak (okumak serbest, yazmak yasak): `Assets/Scripts/Game/` → `StatusHud`, `VillagePanel`, `FamilyPanel`, `BuildMode` ve pazar paneli.
   - Proje belgelerine göre liste:

| Ekran | Kısa içerik |
|---|---|
| yapi | Yapı modu: araç şeridi, kategori sekmeleri, katalog kartları (5-6), kat seçici, ev değeri |
| esya_karti | Eşya bilgi kartı: ad, fiyat, takım, konfor, açıklama |
| pazar | Alım-satım listesi, fiyat, teklif, akçe |
| koy | Köy paneli (V): köylüler, ilişki çubukları, festival, projeler |
| aile | Aile ağacı (F): portre düğümleri, hanedan geçmişi listesi |
| depo | Envanter ve depo: ızgara, kapasite |
| etkilesim | Eşyaya dokununca açılan etkileşim menüsü (uzun listeler kaydırılır) |
| gun_sonu | Gün sonu özeti ve kış göstergesi kartı |
| ayarlar | Ayarlar, duraklat; kaza ayarı (Kapalı/Seyrek/Normal) dahil |
| teklif | Faz 5 evlilik ve çocuk kartı (portre + alel gizli) |
| ayna | Ayna ekranı (görünüm değiştirme, elmas fiyatı yuvası) |

2. **Yerleşim:** her ekran için `tools/art/ui_layouts.json`'a `durum` biçiminde bir anahtar ekle.
   - Ölçü 416x192 (yatay telefon, 2 kat).
   - `pieces`: tüm kutular boş ve etiketli. `content`: örnek metinler.
   - Önizleme betiği değişmeden çalışmalı. Kontrol: `ui_mockup.py mockup <ekran> --placeholder`.
3. **İş dosyaları:** `213a`, `213b`, ...
   - `approved_by: user`, `family: arayuz`, `depends_on: ["210"]`.
   - Ekran başına bir `create_ui_asset` çağrısı (≤ 20 gen); iş başına en çok 3 ekran, `cost_max <= 60`.
   - Tarif 211'deki gibi: `style_image` = 210'un seçilen paneli; yön satırı yoksa varsayılan `parsomen`.
   - Her ekran için `ui_mockup.py` önizlemesi.
4. **Rapor:** `art/results/213-arayuz-ekranlari-plani.md` (ekran ve iş tablosu, toplam gen).

## Doğrulama
- Her yeni ekran için `ui_mockup.py pieces <ekran>` ve `mockup <ekran> --placeholder` hatasız çalışıyor mu? Placeholder önizlemeleri `art/results/213-<ekran>-yerlesim-x2.png`.
- `queue_ready.py --all` ayrıştırıyor mu? `cost_max <= 60`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
