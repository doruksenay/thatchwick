---
id: "465"
title: "Manken siluet aileleri ve etkileşim kareleri planı (iş dosyalarını yazar)"
status: pending
approved_by: user        # plan işi, gen harcamaz; 460a, 460b ve 462 review olunca çalışır
family: karakter
style_gate: false
cost_est: 0
cost_max: 0
depends_on: ["460a", "460b", "462"]
max_attempts: 1
pixellab: false
outputs:
  - art/queue/465a-....md (ve devamı)
  - art/results/465-manken-aile-ve-etkilesim-plani.md
created: 2026-11-20
---
# 465 Manken siluet aileleri ve etkileşim kareleri planı

## Amaç
Plan işi: PixelLab çağrılmaz. Katmanlı karakterin her kareye çizilmesi gereken kısmı (belge 43 §4.2 katman C, §4.3) için iş dosyası yazar:
- **Siluet aileleri:** tunik + pantolon (460a/b'nin kendisi), uzun entari, iş kıyafeti (kolsuz yelek ya da sıvalı gömlek + önlük).
- **Etkileşim kareleri:** mankenin bugünkü 84 px etkileşim animasyonlarının eşi, her aile ve her gövde için.

## Tarif
1. **Yöntem:** 462'nin raporunda seçilen yöntem (transfer-outfit ya da yeniden üretim); seçilmemişse dur ve `## Sorular` ekle.
2. **Gövdeler:** m_orta, f_orta (beden 460c'nin sonucuna göre: B1 kabul edildiyse yalnız Orta; B2 ise Dolgun da).
3. **Animasyonlar:**
   - `idle` ve `walk2` (8 yön);
   - bugünkü 84 px etkileşim anahtarları, `IsoArt.CharacterAction`'ın kullandığı adlarla: `Assets/Art/Iso/_aday/anim/` altında `_r84_<anahtar>_` geçen her anahtar; çapraz yönler.
   - Etkileşim karelerinde hareket 018c-e işlerinin tarifleriyle aynı.
4. **Çıktı adları:** `Assets/Art/Iso/_ham/katman/lay_src_<gövde>_<aile>_<anim>_<yön>_<NN>.png`, işlenmiş `Assets/Art/Iso/katman/lay_body_...`.
   - Aile eki: tunik için yok; `entari`, `is`.
   - Kod tarafı aileyi henüz seçmiyor (LayeredCharacterArt yalnız gövde adına bakıyor); ad eki bu planla sabitlenir, kod sonra bağlanır.
5. **Bölme:** her iş en çok `cost_max: 60`; `approved_by: user`. İş kimlikleri 465a, 465b, ...
6. **Rapor:** `art/results/465-manken-aile-ve-etkilesim-plani.md`: iş tablosu, toplam gen, gövde × aile × anim matrisi.

## Doğrulama
- `queue_ready.py --all` okuyor; kimlik çakışması yok.
- Matris eksiksiz: her gövde × aile × animasyon × yön bir işe düşüyor.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
