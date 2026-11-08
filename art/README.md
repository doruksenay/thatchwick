# Asset kuyruğu: nasıl çalışır

Sohbetler birbirine komut gönderemez; ortak olan tek şey bu repo. Bu yüzden iş, dosya olarak kuyruğa girer, işçi Claude Code oturumu uygular, sonuç dosya olarak döner.

```
Planlama sohbeti ──yazar──▶ art/queue/NNN-*.md
                                   │  (git pull)
Asset işçisi (Claude Code, senin bilgisayarın, PixelLab MCP)
   pixellab-artist ajanı: iş seç → bütçe kapısı → üret → doğrula → rapor → push
                                   │
                    Assets/Art/Iso/*.png + art/results/NNN-*.md
                                   │  (git pull)
Unity/geliştirme sohbeti ──okur──▶ `unity-import: pending` olanları import eder
```

PixelLab anahtarı yerel kapsamda tanımlıdır (`claude mcp add pixellab ...`), bu yüzden işçi her zaman senin bilgisayarında çalışır; bulut zamanlanmış görev PixelLab'e erişemez.

## Durum akışı
`pending` → `running` → `review` → **`accepted`** (ya da `rejected`). Yan durumlar: `failed` (araç hatası ya da doğrulama tutmadı), `blocked` (bütçe, bağımlılık ya da soru var).

- İşçi `review`'a kadar götürür. `accepted` ya da `rejected` **senin** kararındır (iş dosyasının `status` satırını değiştir; reddedersen sebebini dosyanın sonuna yaz).
- `depends_on` içindeki işler `accepted` olmadan sıradaki iş başlamaz. Böylece beğenmediğin çimen karosundan türeyen on karo üretilmez.
- `approved_by`: `auto` (en çok 25 generation'lık işler), `user` (daha pahalı ya da senin onayını isteyen), `pending` (karar bekleniyor, çalışmaz). Fren kuralları: `art/budget.md`.

## İş eklemek
`art/TEMPLATE.md`'yi `art/queue/NNN-ad.md` olarak kopyala ve doldur (planlama sohbeti bunu yapar, kod oturumu da yapabilir). `## Tarif` bölümü işçinin tek kaynağıdır: araç, ayarlar, açıklama metni, doğrulama ölçütü orada olmalı.

## İşçiyi çalıştırmak
1. **İlk kez elle ve izleyerek:** repo klasöründe Claude Code aç ve şunu yaz: `pixellab-artist ajanıyla art/queue'yu işle`. Gerçek maliyetleri ve izin istemlerini gör, gerekli izinleri bir kez onayla.
2. **Kod geliştirirken paralel:** Claude Code ana oturumu `pixellab-artist` ajanını arka planda başlatır (`CLAUDE.md` bunu söyler). Kod ve asset aynı anda ilerler.
3. **Otomatik tur:** `tools\art-worker.cmd` bir kez çalışır ve çıkar. Her 30 dakikada bir tekrar için komut satırında:
   `for /l %i in (0,0,1) do (call tools\art-worker.cmd & timeout /t 1800 /nobreak)`
   ya da Windows Görev Zamanlayıcı'dan aynı betiği zamanla. İşçi uygun iş yoksa hemen çıkar; kilit dosyası (`art/.worker.lock`) iki işçinin çakışmasını önler.

## Unity tarafı
İşçi `.meta` yazmaz. `git pull` sonrası Unity PNG'leri alır; PPU 32, Point, sıkıştırma yok ve pivot ayarı (rapordaki piksel değerinden) yapılır. `art/results/*.md` başında `unity-import: pending` olanlar bekleyen listesidir; import bitince `done` yap. Ad kuralı (repodaki mevcut dosyalar): `iso_item_<ad>_<yön>.png`, çok karolu eşyada uzun eksenin yönü (`iso_item_bed_straw_2x1_se_nw.png`, `_sw_ne`), durum eki yöndan önce (`hearth_stone_lit_se`).

## Dosyalar
| Yol | Ne |
|---|---|
| `art/queue/` | Bekleyen ve biten işler |
| `art/results/` | İş başına rapor (harcama, dosya, pivot, doğrulama, sorunlar) |
| `art/budget.md` | Fren kuralları ve harcama defteri |
| `art/TEMPLATE.md` | İş dosyası şablonu |
| `.claude/agents/pixellab-artist.md` | İşçi ajanın talimatı |
| `tools/art/validate_iso.py` | Boyut, saydamlık, renk, zemin 2:1 maske doğrulaması |
| `tools/art/footprint_guide.py` | Eşya ayak izi kılavuzu ve maskesi (stil eşleme modu) |
| `tools/art-worker.cmd` | İşçiyi tek tur çalıştırır |
| `art-brief*.md`, `art-brief-result.md` | Arşiv (tur 3-4 geçmişi) |
