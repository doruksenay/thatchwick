unity-import: pending

# 447b Korkuluk varyantları B (bulut işçisi, şerit 0, 2026-11-19)

Yan yana sayfası: [447b-yanyana.png](447b-yanyana.png) (3 referans + 4 teslim edilen; kırmızı demir KALDI, sayfada yok)

## Araç ve ayarlar
- 332 ile aynı yöntem: `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`; araç açıklamasına göre 96 px kaynak için 5 gen/çağrı. 6 çağrı = **30 generation** (6 çağrı aynı anda gönderildi; hız sınırı hatası olmadı).
- Kaynak: `iso_wall_fence_wattle_*`, 102x115 tuvalden 96x96 kırpma (sw_ne ofset (0,19), se_nw (6,19)). Maske (betikle): çitin opak bölgesi + her sütunda üstünde 8 px. Çıktı aynı ofsetle 102x115 şeffaf tuvale geri konuldu. Yüklemeler Python (urllib) ile yapıldı.
- Açıklamalar: `carved walnut railing`, `low solid stone parapet`, `wrought iron railing painted red` (+ "simple balcony railing, light from top-left, no baked shadow"). Tarif aynen uygulandı.
- `get_balance` sonrası kalan: **3152.9** (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (hepsi 102x115; pivot x sw_ne 32, se_nw 70; pivot y ALTTAN 19)
Teslim edilen (`Assets/Art/Iso/`): `iso_wall_railing_carved_{sw_ne,se_nw}.png`, `iso_wall_railing_parapet_{sw_ne,se_nw}.png`.
Teslim EDİLMEYEN (stil denetimi KALDI): `iso_wall_railing_iron_red_{sw_ne,se_nw}.png`, `art/results/447b-red/` altında.

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: teslim edilen 4/4 geçti.

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| carved_se_nw | %100 | 4.3 | x1.17 | GEÇTİ |
| carved_sw_ne | %100 | 5.5 | x1.07 | GEÇTİ |
| parapet_se_nw | %100 | 8.3 | x0.30 | UYARI (soluk) |
| parapet_sw_ne | %100 | 6.9 | x0.44 | UYARI (soluk) |
| iron_red_se_nw | %14 | 33.3 | x2.41 | KALDI (palet dışı, çok doygun) |
| iron_red_sw_ne | %0 | 41.4 | x2.58 | KALDI (palet dışı, çok doygun) |

`style.py fit` ile kırmızı doygunluk x0.44/x0.48'e indirildi: sw_ne %100 yakın (UYARI), se_nw %79 yakın (KALDI, palet dışı). Bir eksen KALDI kaldığı için çift teslim edilmedi.

## Sorunlar
- 447a ile aynı sorun: parlak kırmızı boya yapı paleti dışında. Öneri (kullanıcı kararı): palete sönük kırmızı ton eklemek ya da tarifi "faded muted red" ile yeniden yazıp bu iki dosyayı yeniden kuyruğa almak.
- Oyma ceviz korkuluk balüster benzeri oyma dikmeler gösteriyor (ahşap, dönem uygun; taş torna balüster yok).
- Gölge pişirme ve dikiş gözle denetlenmedi (zemin işi değil, döşeme testi yapılmadı).
- Yardımcı betikler `art/tmp/` altında (git dışı).
