# PixelLab bütçesi

Bu dosya işçinin freni ve harcama defteridir. İşçi her iş öncesi okur, her iş sonrası günceller.

## Plan
- Plan: **Tier 3 (Pixel Architect), aylık 10.000 generation, aynı anda 20 iş** (2026-11-21'de yükseltildi; kullanıcı, sohbet: "pixellabde tier atladık"). Bakiye için `get_balance` geçerlidir; bu dosyadaki dönem satırları bir sonraki okumada güncellenir.
- Önceki: Tier 2 (Pixel Artisan), aylık 5000 (2026-11-18'de yükseltildi; kullanıcı, sohbet: "yeni pakete geçtik ve 5000 yeni generation geldi").
- Dönem: 2026-11-18 → yenilenme 2026-12-18.
- Başlangıç bakiyesi (2026-11-18, işçinin `get_balance` okuması): 4999 / 5000.
- Önceki dönem: Tier 1 (Pixel Apprentice), 2000/ay, 2026-11-08 başlangıç 1999; yükseltme öncesi defter bakiyesi 625,1.

## Kurallar
| Kural | Değer |
|---|---|
| Tek iş tavanı (`cost_max`) | 60 generation (çalışma tavanı). Daha büyük iş parçalara bölünür. Onay için ayrıca bir tavan yok (aşağıda "Otomatik akış") |
| Çalışma başına tavan | Şerit sayısı kadar iş (6), her biri kendi `cost_max` tavanıyla |
| Günlük tavan | **Yok** (kullanıcı kararı 2026-11-21: "sınırı kaldır sadece her işlemden sonra agent beni bilgilendirsin"). Bir güne sayı konmak istenirse "Günlük tavan istisnaları" bölümüne yazılır. Önceki: 150 |
| Bildirim | Bulut işçisi her işten sonra "art-bildirim" etiketli konuya yorum yazar ve depo sahibini anar (`tools/art/notify.py`): durum, harcanan, kalan bakiye, sonuç dosyaları, varsa sorular |
| Rezerv | Kalan bakiye (`get_balance`), işten sonra 200'ün altına düşecekse işçi durur |
| Maliyet sapması | Gerçek maliyet `cost_est`'in 2 katını aşarsa işçi durur, `status: failed` yazar |
| Yeniden deneme | Yok. Hata, Tier kısıtı ya da ayak izi/2:1 doğrulaması tutmazsa durur, hatayı aynen yazar |
| Doğruluk kaynağı | Bakiye için her zaman `get_balance`. Bu dosyadaki "kalan" sütunu yalnızca defterdir; çelişirse `get_balance` geçerlidir |

Kuralları gevşetmek ya da sıkılaştırmak kullanıcının kararıdır; işçi bu tabloyu değiştirmez.

## Günlük tavan istisnaları
Bu listedeki gün için günlük tavan yerine buradaki değer geçerlidir (`tools/art/queue_ready.py` ve işçi okur). Biçim: `- YYYY-AA-GG: sınırsız` ya da `- YYYY-AA-GG: <sayı>`. Diğer frenler (çalışma başına 60 gen / 3 iş, rezerv 200, maliyet sapması) istisna günlerinde de aynen geçerlidir.

- 2026-11-18: sınırsız (kullanıcı kararı, sohbet: "bugün sınırsız çalışsın")
- 2026-11-19: sınırsız (aynı karar; bulut işçisi UTC tarihiyle yazar, İstanbul gece yarısı ile UTC 21:00 arasını kapsar)
- 2026-11-20: sınırsız (kullanıcı kararı, sohbet 21.11 02:07: "limiti kaldır"; aynı gün önce 2500, 1500, 1000; rezerv 200 aynen)

## Otomatik akış (kullanıcı kararı, 2026-11-17)
Kullanıcı: "Bunu otomatize et. Bu projede istenen tüm asset'ler PixelLab'e üretilmeli ve kuyruğa atılmalı."

- **Onay:** kuyruğa giren her iş onaylıdır. Yeni iş `approved_by: user` ile yazılır. `pending` yalnızca işin açık bir sorusu varsa kullanılır.
- **Bağımlılık:** `review` durumundaki iş, stil pilotları dahil, karşılanmış sayılır. Pilot türevleri kullanıcının kabulünü beklemez.
  - Bekletenler: `pending`, `running`, `blocked`, `failed`, `rejected`.
  - Kullanıcı bir pilotu `rejected` yaparsa henüz başlamamış türevler durur.
- **Kullanıcı seçimi gereken yerler:** kullanıcı seçimi yazmamışsa iş dosyasındaki varsayılan kullanılır. Örnek: 210 arayüz yönü, varsayılan `parsomen`.
- **Plan işleri:** `pixellab: false` olan ve tarifi "iş dosyası yaz" diyen işlerde işçi yeni iş dosyaları yazar. Bu işler `approved_by: user`, en çok 60 `cost_max` olur ve `art/TEMPLATE.md`'ye uyar.
- **Yeni ihtiyaç:** asset gerektiren her yeni özellik ya da karar için, planlama ve kod oturumları aynı turda iş dosyası yazar ve `main`'e push eder.
- **Frenler:** tek iş 60 gen, rezerv 200, maliyet sapması (günlük tavan 2026-11-21'de kaldırıldı). Kuyruk ayı aşarsa işler bir sonraki dönemde kendiliğinden devam eder.

## Paralel üretim (kullanıcı kararı 2026-11-18)
Kullanıcı: "Asset üretimini hızlandıramaz mıyız kaliteden taviz vermeden? Paralel bir agent vs."
- **6 şerit (2026-11-21; önce 3, kısa süre 10):** bulut işçisi her turda 6 işi aynı anda yürütür (`art-worker.yml` matrisi). Her şerit sıradaki uygun işi sahiplenir; iki şerit aynı işe denk gelirse push reddedilir, kaybeden şerit sahiplenmesini geri alıp sonraki işi dener.
- **İş içi paralellik:** bir işteki bağımsız PixelLab çağrıları aynı anda gönderilir, ama **en çok 3 eşzamanlı çağrı** (6 şerit x 3 = 18, PixelLab'in 20 eşzamanlı iş sınırının altında); tarif "sırayla" diyorsa sırayla.
- **Hız sınırı:** PixelLab eşzamanlılık ya da hız sınırı hatası verirse (429 / "too many") o çağrı bekleyip bir kez yeniden denenir. "Yeniden deneme: yok" kuralının tek istisnası budur.
- **Defter:** paralel işler aynı tabloya yazıp çakışmasın diye her iş defter satırını `art/ledger/NNN.md` dosyasına yazar (aynı tablo biçimi). Aşağıdaki tablo 2026-11-18'e kadarki geçmiştir; `queue_ready.py` ikisini birlikte okur.
- Kalite kapıları değişmedi: stil kilidi, `validate_iso.py`, yan yana sayfası ve kullanıcının `accepted`/`rejected` kararı aynen geçerli.
- Not: her şerit ayrı bir Claude Code oturumudur; Claude aboneliğinin kullanım sınırı şerit sayısı kadar (6 kat) hızlı dolar.
- **Claude kullanım sınırı (kullanıcı kararı 2026-11-21):** şeritteki oturum sınıra takılırsa işçi yarım çıktıyı atar, işi hemen `pending`'e geri koyar ve `art/claude-sinir.md`'ye zamanı yazar. Bu andan sonra 60 dakika hiçbir işçi yeni iş almaz (`queue_ready.py`); zincir de o şeritten yeni tur başlatmaz. Bildirimde "Claude kullanım sınırı, iş sıraya geri kondu" yazar. Sınırdan önce harcanan gen boşa gitmiş olabilir.

## Öncelik (kullanıcı kararı 2026-11-18)
Kullanıcı: portre işleri (021o-r ve sonrası) duraklatılsın; kalan bütçe önce oyunun dünyasına gitsin (toprak yol, zemin, duvar malzemeleri, kapı ve pencere, eşya dalgası, ekinler). Portreler bir sonraki dönemde ya da paket yükseltilince tamamlanır.
- İş dosyasında `priority:` alanı (küçük sayı önce). Yoksa 50. `queue_ready.py` uygun işleri önce önceliğe, sonra numaraya göre sıralar; bulut şeritleri bu sırayla sahiplenir.
- 10: oyun dünyası (eşya pilotu 301 ve dalgası, duvar pilotu 325 ve malzemeleri, zemin, yol, kapı, pencere, çatı, halı, çit, merdiven, ekin, ağaç, toplama, su karoları).
- 20: kalan dünya ve zanaat eşyaları, adlı köylüler.
- 50 (varsayılan): karakter animasyonları, hayvanlar, arayüz ve ikonlar.
- Ertelenen: 021r (portre makyaj, kırışıklık...), `approved_by: pending`. Yeni portre işleri de aynı şekilde yazılır.

## Günlük defter
| Tarih | İş | Araç | Harcanan | Kalan (get_balance) |
|---|---|---|---|---|
| 2026-11-08 | (devir) tur 3-4 Faz A | — | 1 | 1999 |
| 2026-11-08 | 007 duvar malzeme | create_building_kit (materials, 64) | 10 | 1950 |
| 2026-11-08 | 009 katman fizibilite | — (rapor, PixelLab yok) | 0 | 1950 |
| 2026-11-08 | 010 karakter boy testi | create_character v3 (72/84/96) | 7 | 1928 |
| 2026-11-08 | 011 karakter boy testi 2 | create_character v3 (52/60/68) + reduce_colors (32 renk, 3 çağrı) | 6,3 | 1896,7 |
| 2026-11-08 | 100 hayvan pilotu | create_character standard + create_image_pixflux x2 + create_image_pixen + create_character v3 ref x2 + reduce_colors (32 renk, 1 çağrı) | 6,1 | 1890,6 |
| 2026-11-08 | 012 karakter seti K2 | create_character v3 x8 (60/56/40) + reduce_colors (32 renk, 8 çağrı) | 16,8 | 1873,8 |
| 2026-11-08 | 013 yürüme pilotu | animate_character SE: template 1 + v3 1 + skeleton-v3 3 + pixminimax 1 | 6 | 1867,8 |
| 2026-11-18 | 008b duvar pencere/alçak | inpaint_image_pro_flash x4 (96x96 kırpma) | 20 | 1823,8 |
| 2026-11-08 | 300 eşya grup 1 | create_map_object stil eşleme x14 (8 tabureli + 6 kılavuzlu ikinci deneme); 6/8 teslim | 14 | 1853,8 |
