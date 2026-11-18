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

PixelLab anahtarı yerel kapsamda tanımlıdır (`claude mcp add pixellab ...`), bu yüzden yerel işçi senin bilgisayarında çalışır. Bulut işçisi (GitHub Actions) aynı anahtarı repo secret'ı olarak kullanır; aşağıda "Otomatik işçiler".

## Durum akışı
`pending` → `running` → `review` → **`accepted`** (ya da `rejected`). Yan durumlar: `failed` (araç hatası ya da doğrulama tutmadı), `blocked` (bütçe, bağımlılık ya da soru var).

- İşçi `review`'a kadar götürür. `review` bağımlı işleri başlatmaya yeter (otomatik akış). `accepted` ya da `rejected` **senin** kararındır: iş dosyasının `status` satırını değiştir; reddedersen sebebini dosyanın sonuna yaz. Reddedilen işe bağlı, henüz başlamamış işler durur.
- `depends_on` içindeki işler `review` ya da `accepted` olmadan sıradaki iş başlamaz.
- `approved_by`: `user` (varsayılan, her iş) ya da `pending` (açık soru var, çalışmaz). Fren kuralları: `art/budget.md`.

## İş eklemek
`art/TEMPLATE.md`'yi `art/queue/NNN-ad.md` olarak kopyala ve doldur (planlama sohbeti bunu yapar, kod oturumu da yapabilir). `## Tarif` bölümü işçinin tek kaynağıdır: araç, ayarlar, açıklama metni, doğrulama ölçütü orada olmalı.

## İşçiyi çalıştırmak
1. **İlk kez elle ve izleyerek:** repo klasöründe Claude Code aç ve şunu yaz: `pixellab-artist ajanıyla art/queue'yu işle`. Gerçek maliyetleri ve izin istemlerini gör, gerekli izinleri bir kez onayla.
2. **Kod geliştirirken paralel:** Claude Code ana oturumu `pixellab-artist` ajanını arka planda başlatır (`CLAUDE.md` bunu söyler). Kod ve asset aynı anda ilerler.
3. **Otomatik tur:** `tools\art-worker.cmd` bir kez çalışır ve çıkar. Her 30 dakikada bir tekrar için komut satırında:
   `for /l %i in (0,0,1) do (call tools\art-worker.cmd & timeout /t 1800 /nobreak)`
   ya da Windows Görev Zamanlayıcı'dan aynı betiği zamanla. İşçi uygun iş yoksa hemen çıkar; kilit dosyası (`art/.worker.lock`) iki işçinin çakışmasını önler.

## Otomatik işçiler (2026-11-13)
Kod oturumu iş dosyasını push edince üretimin kendiliğinden başlaması için iki yol var. İkisi de yalnızca `art/budget.md` kurallarıyla uygun işleri alır (onay ve bağımlılık kuralı değişmedi). Uygunluğu `tools/art/queue_ready.py` belirler; uygun iş yoksa Claude hiç başlatılmaz.

| Yol | Nerede | Tetik | Kurulum |
|---|---|---|---|
| Bulut işçisi | GitHub Actions (`.github/workflows/art-worker.yml`) | `art/queue`, `art/budget.md` ya da `art/style` push'u; saatte bir; elle (Actions > Art worker > Run workflow) | Repo secret'ları: `CLAUDE_CODE_OAUTH_TOKEN` (bilgisayarında `claude setup-token`), `PIXELLAB_API_TOKEN` (pixellab.ai hesabı). Yoksa iş sessizce atlanır. |
| Yerel yoklayıcı | Senin bilgisayarın (yerel PixelLab MCP) | 15 dakikada bir `tools/art-poll.cmd` | Bir kez `tools\art-poll-install.cmd` |

- İkisi aynı anda çalışabilir: işe başlarken `status: running` (ve `claimed_at`) commit edilip push edilir (sahiplenme). Push tutmazsa iş başkasınındır, atlanır.
- Takılı sahiplenme: `running` olan iş dosyası 6 saattir değişmediyse (işçi çöktü) `queue_ready.py` onu yeniden uygun sayar.
- Bulut işçisi bilgisayar kapalıyken de çalışır ve Claude aboneliğinden harcar. İlk çalıştırmayı elle başlatıp Actions günlüğünden izle.
- Bulut işçisinde masaüstü kopyası adımı yoktur; sonuçlar repoya ve `art/results/`'a gelir.

## Bulut işçisinin güvenliği (2026-11-17)
Bulut işçisi açık repoda, Claude ve PixelLab anahtarlarıyla çalışır. Ajana bir iş dosyası ya da PixelLab çıktısı üzerinden talimat sızsa bile main'e kod ya da sır gitmesin diye:
- Çalıştırma başına tek iş. İşi iş akışı sahiplenir (`queue_ready.py --claim`); ajan yalnızca commit eder, push edemez (checkout kimlik bilgisi bırakmaz, git yalnızca okuma ve commit alt komutlarıyla açık).
- Push'tan önce `tools/art/cloud_guard.py`: değişen her yol `art/`, `Assets/Art/Iso/` ya da `tools/art/` altında; `.meta` yok; `Assets/Art/Iso/` altında var olan dosya değişmez ve silinmez; Claude, PixelLab ve GitHub anahtarlarının değeri değişikliklerde geçmez. Reddedilirse hiçbir şey push edilmez.
- `tools/art/` değiştiyse sonuç `art-worker/<çalıştırma>` dalına gider ve PR açılır: yerel yoklayıcı main'deki bu betikleri çalıştırdığı için kod değişikliği insan onayından geçer.
- Eylemler commit SHA'sıyla, Claude Code ve Pillow sürümle sabit. Yükseltmek için yml'deki satırları birlikte güncelle.
- Önerilen GitHub ayarı (elle): Settings > Branches'ta `main` için kural: zorla push ve silme kapalı.

## Stil kilidi
Tutarlılık: `art/style/README.md`. Her iş bir aileye aittir (`family`) ve ailenin stil pilotuna (`style_gate: true`) bağımlı yazılır. İşçi aile referanslarını kullanır, `tools/art/style.py check` ile palet ve doygunluk denetler (KALDI teslim edilmez) ve her rapora `art/results/NNN-yanyana.png` koyar.

## Otomatik akış (kullanıcı kararı, 2026-11-17)
Kuyruk insan onayı beklemeden akar. Kurallar `art/budget.md` "Otomatik akış" bölümünde:
- Her iş onaylı yazılır (`approved_by: user`).
- `review` bağımlılık için yeterlidir; pilotlar dahil.
- Plan işleri yeni iş dosyası yazabilir.
- Para frenleri aynen geçerli.

`queue_ready.py --strict` eski kuralı gösterir.

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
| `tools/art-poll.cmd`, `tools/art-poll-install.cmd` | Yerel yoklayıcı ve Görev Zamanlayıcı kurulumu |
| `tools/art/queue_ready.py` | Uygun işleri listeler (PixelLab'e dokunmaz); `--claim` ilk uygun işi sahiplenir |
| `tools/art/cloud_guard.py` | Bulut işçisinin commit'lerini push'tan önce denetler |
| `tools/art/style.py`, `art/style/` | Stil kilidi: referanslar, ana palet, denetim, yan yana sayfası |
| `.github/workflows/art-worker.yml` | Bulut işçisi |
| `art-brief*.md`, `art-brief-result.md` | Arşiv (tur 3-4 geçmişi) |
