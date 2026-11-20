# Yapılacaklar

Son güncelleme: 2026-11-20. Bu oturumun kod işleri bitti; kalanlar evden yapılacak işler.

## Unity'de dene
- [ ] Elde yiyecek: elma, yumurta, sepetten armut ve erik (iki karakter, 4 yön).
- [ ] Zıplamasız yürüme (walk2, 16 yön).
- [ ] Sohbet, bakım, sevgi, keyif ve balık animasyonları: karakter hedefe varıp durunca oynamalı.
- [ ] Madencilik: kazma, destek kurma, kovayla su boşaltma.
- [ ] Yıkanma ve tuvalette keyif animasyonunun oynamadığını kontrol et.

## Yayın altyapısı (20.11.2026; ayrıntı `docs/yayin-altyapisi.md`)
- [ ] Unity'de dene: başarım penceresi, hata raporu kopyalama, tam ekran/pencere ve çözünürlük, `Esc` menüsü (belgenin 7. bölümü).
- [ ] Unity'de projeyi aç, görsel kütüphanesini yenile, bütün `.meta` dosyalarını commit et.
- [ ] GitHub > Settings > Rules > Rulesets > Import: `.github/rulesets/main-gecmis.json` (hemen açılabilir).
- [ ] Unity'de aç: Package Manager dört paketi kaldırmış olmalı (Visual Scripting, Timeline, Multiplayer Center, Version Control); derleme hatası çıkarsa `Packages/manifest.json`'a geri ekle.
- [ ] Unity lisans secret'ları (`UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`): eklenince Unity testleri ve derleme kendiliğinden çalışır.
- [x] Repo gizliliği: açık kalıyor (kullanıcı kararı 20.11.2026; gizli repoda Actions dakikası kotalı). Haklar `LICENSE.md`'de.
- [ ] Steamworks hesabı açılınca: `PlatformConfig` kimlikleri, başarım tablosu, Auto-Cloud (belgenin 3. bölümü).

## İçerik (karar kullanıcıda)
- [x] `item.crop_pumpkin`: görünen adlar zaten su kabağı (TR) ve bottle gourd (EN); yalnızca kimlikte `pumpkin` kaldı. Kimlik kayıtlarda geçtiği için değiştirilmedi.

## Sanat kararları (görselleri sohbette göster, karar kullanıcıda)
- [ ] 018b4: yürüme, son 5 yön (review; kod zaten kullanıyor, reddedilirse bağlantı geri alınır)
- [ ] 018d4: boş elle yeme, kalan 4 yön (review; kod zaten kullanıyor)
- [ ] 395e: kömür yığını (review)
- [ ] 018c: boşta, odun, çapa, ekim, sulama. 20.11'de kuyruğa yeniden alındı; reddedilen 018a'ya bağlı olduğu için takılıydı. Kod bağlantısı hazır (chop, hoe, sow, water), kareler gelince kendiliğinden oynar.

## Karar bekleyen öneri
- [ ] Elde tutulan aletleri de el konumu yöntemiyle göstermek (balta, çapa, kazma…). Evet denirse yeni sanat işi + kod.

## Biten (19-20.11.2026)
- [x] Elde yenen yiyecek: #60, #73, #77
- [x] walk2 yürüme, 018e animasyonları, madencilik üçlüsü oyuna bağlandı (#73, #77)
- [x] Eşyasız işlerde (sosyal, hayvan, karo) eylem karesi oynuyor; 018c bağlantısı hazır (#79)
- [x] Yerelleştirme altyapısı ve içerik metinlerinin İngilizcesi (#70)
