unity-import: pending

# 580 Servis kapları 2/2: komposto kasesi 5/5 teslim, 5 birleşim teslim edilmedi

Yan yana sayfası: [580-yanyana.png](580-yanyana.png). Doluluk şeridi (komposto, 4→0): [580-serit.png](580-serit.png). Teslim edilmeyen ham kareler: `art/results/580-red/` (`ham-sayfa.png`: 6 birleşim × 5 kare, soldan sağa 4→0). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object`, stil eşleme, `high top-down`, `basic shading`, `low detail`, `single color outline`, tarif açıklaması + ortak ek. Tuval 32x32 (578 ile aynı ölçü). Kılavuz: tabanda 24x12 elmas (#786046); `_4` için tabanı kapsayan dikdörtgen maske.
- `_3…_0`: `_4` karesi arka plan, yalnızca kabın içini kapsayan elips maske (578 ile aynı yöntem). Araç maske dışını geri vermiyor; çıktı yalnızca iç parça. Kap `_4`'ten alındı, iç üretilen kareden elle kompozitlendi.
- Çağrılar: 6 dolu + 24 doluluk = 30 üretim. 6 base64 aktarım hatası ücretsiz (görüntü çözülemedi ya da kesildi), yeniden gönderildi. `cost_est` 30, cost_max 60.
- `get_balance` sonrası: 1861,7 (paralel şeritler de harcıyor; bu işin payı 30).

## Sonuç
| Birleşim | Durum | Not |
|---|---|---|
| bowl_compote `_4…_0` | teslim | kap 5 karede birebir aynı (alfa farkı 0 piksel); yemek 4→0 azalıyor, `_0` boş kil iç |
| board_bread | TESLİM EDİLMEDİ | `_0` boş değil, tek bir ekmek görünüyor; `_4` ile içerik silueti uyuşmuyor |
| board_sweet | TESLİM EDİLMEDİ | `_4` kare tezgâh benzeri siluet; iç kareler düz tahta, silüet tutmuyor |
| bowl_salad | TESLİM EDİLMEDİ | `_0` parlak yeşil kütle (boş değil, doygun) |
| plate_breakfast | TESLİM EDİLMEDİ | `_0` yemek dolu görünüyor |
| plate_fry | TESLİM EDİLMEDİ | iç maske dikdörtgen tabağa uymadı; `_0` dolu |

Tek deneme kuralı gereği yeniden üretilmedi.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_serve_bowl_compote_4.png | 32x32 | 16 | 10 |
| iso_serve_bowl_compote_3.png | 32x32 | 16 | 10 |
| iso_serve_bowl_compote_2.png | 32x32 | 16 | 10 |
| iso_serve_bowl_compote_1.png | 32x32 | 16 | 10 |
| iso_serve_bowl_compote_0.png | 32x32 | 16 | 10 |

Pivot = taban elmasının merkezi (fruit_basket ile aynı kural).

## Doğrulama
| Dosya | validate_iso | Renk | Stil: palete yakın / uzaklık / doygunluk | Sonuç |
|---|---|---|---|---|
| compote_4 | geçti | 21 | %82 / 14,7 / x1,22 | UYARI (palete uzak) |
| compote_3 | geçti | 22 | %89 / 13,1 / x1,19 | UYARI |
| compote_2 | geçti | 24 | %94 / 11,3 / x1,12 | UYARI |
| compote_1 | geçti | 17 | %94 / 11,5 / x1,08 | UYARI |
| compote_0 | geçti | 13 | %97 / 11,6 / x1,11 | UYARI |

Stil denetiminde KALDI yok. Kap dış çizgisi alfa farkı 0 piksel (tarif: en çok 1). Masa üstü eşyası olduğundan döşeme testi yok. Buhar, parlama, gölge ve el yok.

## Sorunlar
- ⚠ `_3` komposto koyu kırmızı kütle gibi, `_4`'teki turuncu meyveden renk olarak kopuk; kullanıcı `580-serit.png`'ye bakmalı.
- Düz tahta/tepsi kaplarda (ekmek, tatlı, kahvaltı, kızartma) ve salata kasesinde iç-maske yöntemi tutmadı (578 tepsisiyle aynı neden: model "boş" yerine dolu nesne çiziyor, kap silueti `_4` ile eşleşmiyor). Öneri: bu birleşimler için kabı tek sprite, yemeği ayrı overlay katmanları olarak çizmek, ya da `_4`'ten `inpaint_image_pro_flash` ile yemeği silip yeniden doldurmak. Karar kullanıcıda.
- Base64 aktarımında bazı çağrılar bozuldu ya da kesildi; renk sayısı düşürülmüş arka planla yeniden gönderildi (araç yüklemesi için `_url` değişkeni bu ortamda yok).
