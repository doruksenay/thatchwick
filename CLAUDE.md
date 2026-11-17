# Thatchwick: Claude Code kuralları

Oyun: Thatchwick: Generations of a Village (Unity 6, 2D URP, izometrik 2:1). Tasarım belgeleri claude.ai Project'indedir; burada yalnızca kod ve asset kuralları var.

## Kod
- Mantık `Assets/Scripts/Core` (UnityEngine yok), Unity katmanı `Assets/Scripts/Game`. Çekirdek testleri: `dotnet test` (`core/`).
- `.meta` dosyalarını Unity üretir; Unity'siz ortamda `.meta` yazma.
- Görsel şartname bağlayıcıdır: `docs/gorsel-yonerge.md`.
- İçerik (eşya, hayvan, ekin, yemek, alet) eklerken dönem kurallarına uy: hayali bir ortaçağ yerleşimi; Yeni Dünya bitkisi/hayvanı yok (patates, domates, mısır, balkabağı, tütün, hindi), ortaçağ sonrası icat yok (gazyağı, balya, mibzer vb.). Gerçekçilik: mevsim, ömür oranı (1 gün ≈ yarım yıl), türün gerçek davranışı. Ayrıntı: Project belgesi `20-donem-tutarliligi-ve-gercekcilik.md`.

## Asset kuyruğu (PixelLab)
Asset üretimi `art/queue/` altındaki iş dosyalarıyla yürür. Akış ve tetikleme: `art/README.md`. İşçi ajan: `.claude/agents/pixellab-artist.md`. Bütçe kuralları: `art/budget.md` (bağlayıcı).

- Kod oturumu PixelLab'i kendisi çağırmaz. Asset gerekiyorsa `art/TEMPLATE.md`'den bir iş dosyası ekler; çalıştırmak için `pixellab-artist` ajanını arka planda başlatır (Agent aracı, `run_in_background`).
- `pixellab-artist` yalnızca `art/`, `Assets/Art/Iso/` ve `tools/art/` altına yazar. `Assets/Scripts`, `Packages`, `ProjectSettings` ve `Assets/Art/Test_34` yasaktır. Var olan PNG'nin üzerine yazılmaz.
- Bu düzen, eski `art-brief*.md` dosyalarındaki "commit ya da push yapma" kısıtının yerine geçer: işçi yalnızca kendi yollarını commit ve push eder (`git pull --rebase` ile). Bulut işçisinde ajan yalnızca commit eder; push'u iş akışı `tools/art/cloud_guard.py` denetiminden sonra yapar, `tools/art` değişikliği PR olur (`art/README.md`).
- `art-brief*.md` dosyaları arşivdir. `art-brief.md`'deki tur 4 planı `art/queue/001-008` işlerine taşındı; karakter katmanı ve yapı modu isteği (`art-brief-tur4.md`) 009 ile fizibiliteden başlar.
- Her görsel iş insan onayından geçer: işçi `status: review` bırakır, `accepted` yazmak kullanıcının işidir. Onaylanmamış işe bağlı iş başlamaz.
