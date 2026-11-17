---
id: "017"
title: Karakter katman seti planı (iş dosyalarını yazar)
status: pending
approved_by: pending     # tools/art/approve_all.py --uygula ile açılır (otomatik akış, kullanıcı kararı 2026-11-17)
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["016"]
max_attempts: 1
pixellab: false
outputs:
  - art/queue/017a-*.md ... art/queue/017z-*.md
  - art/results/017-karakter-katman-seti-plani.md
created: 2026-11-17
---
# 017 Karakter katman seti planı

## Amaç
Plan işi: PixelLab çağrılmaz. 016'nın ölçtüğü yöntem ve maliyetle, karakter yaratmanın dünya sprite katmanlarının tamamı için **iş dosyası yazar** (`art/budget.md` "Otomatik akış": plan işleri).

## Tarif
1. `art/results/016-*.md` raporunu oku: önerilen yöntem, kimlik başına gen, yön sayısı.
2. Aşağıdaki listeyi parçalara böl ve iş dosyalarını yaz:
   - numaralar `017a`, `017b`, ...;
   - `art/TEMPLATE.md` biçimi;
   - `approved_by: user`, `family: karakter`, `style_gate: false`;
   - `depends_on: ["016"]`;
   - her iş `cost_max <= 60`.

   Her dosyanın `## Tarif` bölümü 016'daki yöntemi birebir tekrarlar: araç, maske kuralı, açıklama eki, çıktı adı, doğrulama.

**Liste** (proje dokümanı 14 bölüm 3; 1.0 kapsamı)
| Grup | Adet | Not |
|---|---|---|
| Taban gövde | 4 yapı (İnce, Orta, Gürbüz, Dolgun) × 3 çerçeve (erkeksi, ara, kadınsı) = 12 | Kel, iç gömlekli. Çocuk ve yaşlı için ayrı taban: 2 × 3 = 6. Boy kaldırıldı (karar 2). |
| Saç | 20 stil | Düz, dalgalı, kıvırcık doku. Gri tonlu çizilir, renk rampa takasıyla gelir (açıklamaya `neutral grey hair, will be recoloured` ekle). |
| Kâkül | 6 | Ayrı katman. |
| Bıyık | 5 | |
| Sakal | 6 | |
| Üst | 8 | Gri tonlu ana bölge + ayrı kenar maskesi. |
| Alt | 4 | |
| Ayakkabı | 4 | |
| Dış (pelerin, kürk) | 4 | |
| Başlık | 8 | |
| Sargı (taze yara) | 1 | Kol ya da el. Doküman 14 bölüm 3F. |

3. **Sıra:** önce taban gövdeler (diğerleri onlara göre maskelenir), sonra saç, sakal, kıyafet. Tabana bağlı işler ilgili taban işine `depends_on` ile bağlanır.
4. `art/results/017-karakter-katman-seti-plani.md`: yazılan işler tablosu (numara, içerik, `cost_max`) ve toplam. Toplam bu ayın kalan bakiyesini aşıyorsa yaz; işler frenlerle sonraki döneme taşar.

## Doğrulama
- `python tools/art/queue_ready.py --all` yeni işleri tanıyor mu (ayrıştırma hatası yok)?
- Her işte `cost_max <= 60`.
- Numara çakışması yok.

## Notlar
- Renk, yara izi, dövme, çil ve ben gibi küçük işaretler dünya sprite'ında betikle çizilir; bu listede yok.
- Portre katmanları ayrı plan: 021.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
