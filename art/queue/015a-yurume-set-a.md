---
id: "015a"
title: Yürüme seti, yaşlı erkek ve kadın (84 px, walk2, 8 yön)
status: running
claimed_at: 2026-11-19T12:54Z
approved_by: user        # kullanıcı 19.11.2026: "Kodu yaz" (görsel parti 3; 014/015 yeni karaktere göre yeniden yazıldı)
priority: 20
family: karakter
style_gate: false
cost_est: 24
cost_max: 40
depends_on: ["018b2", "415", "423"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_{elder_m,elder_f}_r84_walk2_<yön>_<NN>.png
  - art/results/015a-yurume-yasli-84.md
created: 2026-11-19
---
# 015a Yürüme seti: yaşlı erkek ve kadın (84 px)

## Amaç
Eski 015a (K2 56 px seti) 012'nin reddiyle geçersiz kaldı. Yaşlılar 415'te 84 px gerçekçi tabana geçti (kabul). Oyunda yaşlılar şu an yürürken durma karesi + kodla sallantı gösteriyor. Bu iş onlara yetişkinlerle aynı yöntemle yürüme verir.

## Tarif
- **Yöntem ve metin 018b2 ile aynı** (018a3 aday d, kullanıcı seçimi):
  - `animate_character`, `pixminimax`, `frame_count=8`, `keep_first_frame=true`;
  - `end_frame` = ilgili yönün durma karesi;
  - metin aynen: `walking at a calm steady pace, a full stride cycle with the left foot then the right foot stepping equally, head and shoulders level, no bouncing, no limping`.
  - Yaşlı için metne yalnızca şu eklenir: `, slightly slower shorter steps of an old person`.
- **Karakterler (415):**
  - elder_m `8dcaede5-cc4d-4068-b56f-ab5e327fea1c`
  - elder_f `b7f31e8d-29f2-4f6a-9092-a02759611159`
  - Her biri 8 yön; çağrılar bağımsız, paralel.
- **Yaşlı erkekte kürek (423):** 415 rotasyonları elde küçük kürek taşıyor. 423 bunu 7 yönde sildi (`iso_char_elder_m_r84_idle_<yön>_v2`, se teslim edilmedi).
  - Başlangıç ve bitiş karesi olarak bu `_v2` durma kareleri yüklenebiliyorsa onlar kullanılır.
  - Araç yalnızca karakter rotasyonunu kabul ediyorsa ilk yön üretilir ve bakılır. Yürüme karelerinde kürek görünüyorsa elder_m durdurulur (elder_f sürer), `## Sorular`a yazılır.
  - se yönü 423'te kürekli kaldığı için elder_m se, `_v2` se gelene kadar üretilmez; raporda belirtilir.
- **Ücretsiz düzeltme (zorunlu, 018b2 ile aynı):**
  - Her kare, yere basan ayağın en alt pikseli durma karesinin ayak satırına gelecek şekilde bütün olarak dikey kaydırılır.
  - Döngüyü bozan ya da 00'a çok yakın son kare atılır.
- Tuval 84x84, pivot (42, 2 alttan), 018b2 işleme kuralları ve `walk2` adı aynen.
- Bir yön ölçütü geçmezse bütçe izin verirse bir kez daha denenir (yön başına en çok 2 deneme).
- Yine geçmeyen yön teslim edilmez; kareleri `art/results/015a-red/` altına, nedeni sonuç dosyasına yazılır.

## Doğrulama
- **Sekme ölçütü (bağlayıcı):** kareler arası kafa (opak bbox tepesi) farkı ≤ 3 px, ayak satırı sabit; yön × karakter tablosu.
- Kürek ya da başka alet hiçbir karede görünmez (elder_m için yön yön yazılır).
- `validate_iso.py --kind object --max-colors 32`, pivot (42, 2).
- İki GIF (elder_m, elder_f; 8 yön ızgara) ve yetişkin erkekle yan yana bir GIF: hız ve boy farkı görünsün.

## Notlar
- Kod hazır: `IsoArt.CharacterWalk` yaşlıda da `_walk2_<yön>_NN` karelerini arar. Kare yoksa durma karesi + kod sallantısı sürer.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
