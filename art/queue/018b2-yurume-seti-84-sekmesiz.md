---
id: "018b2"
title: Yürüme seti yeniden: erkek ve kadın 84 px, 8 yön, sekmesiz
status: running
claimed_at: 2026-11-18T19:55Z
approved_by: user        # kullanıcı 18.11.2026 (sohbet)
priority: 10
family: karakter
style_gate: false
cost_est: 40
cost_max: 60
depends_on: ["018a3"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_walk2_<yön>_<NN>.png
  - art/results/018b2-yurume-seti-84-sekmesiz.md
created: 2026-11-18
---
# 018b2 Yürüme seti yeniden (sekmesiz)

## Amaç
018b reddedildi: yürürken zıplıyor (kafa 4-8 px oynuyor). Kullanıcı yalnızca bozuk yönleri değil, yürümeyi bütünüyle zıplamalı buldu; set baştan yapılır.

## Tarif
- **Yöntem: 018a3 aday d (kullanıcı seçimi).** `animate_character`, `pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame` ilgili yönün durma karesi, metin aynen: `walking at a calm steady pace, a full stride cycle with the left foot then the right foot stepping equally, head and shoulders level, no bouncing, no limping`.
- **Ücretsiz düzeltme (zorunlu):** 018a3 d'de son kareler 2-3 px yere gömüldü ve döngü kapanmadı. Her yönde: (1) her kareyi, yere basan ayağın en alt pikseli durma karesinin ayak satırına gelecek şekilde bütün olarak dikey kaydır; (2) son kare 00 ile neredeyse aynı pozsa ya da döngüyü bozuyorsa at. Kafa ölçütü düzeltmeden sonra yeniden ölçülür.
- 018b'nin karakter kimlikleri, 8 yön, ortak işleme kuralları aynen; dosya adı `walk2` (karar: 018a3 d).
- Çağrılar bağımsız: paralel.

## Doğrulama
- **Sekme ölçütü (bağlayıcı):** her yönde kareler arası kafa (opak bbox tepesi) farkı ≤ 3 px, ayak satırı sabit. Geçmeyen yön teslim edilmez; raporda yön × karakter tablosu.
- 018b doğrulamaları, iki GIF (erkek, kadın; 8 yön ızgara) ve yan yana.

## Sorular
- 018a3 raporunda "kazanan yöntem" yok: kullanıcı seçimi yazılmamış (iş dosyasında karar notu yok). Rapor yalnızca a (şablon 8 kare) ile c (skeleton-v3 8 kare) arasında öneri veriyor; ikisinde de hafif topallama riski, b ve d elenmiş. Hangi harf (a, c ya da başka)? Seçim yazılınca (örn. bu dosyaya `yöntem: c`) iş `pending` yapılıp yeniden alınabilir. Harcama yapılmadı (0 gen).
- **Yanıt (kullanıcı, 18.11.2026):** yöntem: **d** (pixminimax, simetrik adım metni). Tarif bölümü buna göre güncellendi; son karelerin gömülmesi ücretsiz hizalamayla düzeltilir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
