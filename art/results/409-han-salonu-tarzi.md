unity-import: pending

# 409 Han Salonu tarzı (rapor): 7/8 teslim, fıçı masa teslim edilmedi, review

Yan yana sayfası: [409-yanyana.png](409-yanyana.png). Teslim edilmeyen: `art/results/409-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + özel maske (`footprint_guide.py`; maske eşya yüksekliği kadar yukarı + 1 px), `low detail`, `basic shading`, `single color outline`; 301 ortak eki + takım eki (Zanaatkâr: dark oak wood with iron fittings; Köylü: rough light pine wood, rope lashing; Tüccar: painted wood, brass, pewter). Yer eşyaları `high top-down`; duvar eşyaları `side` + "mounted flat against a wall that slopes up to the right". 8 çağrı x 1 gen = 8 generation, paralel, tek deneme. İlk rafın çağrısı bozuk base64 yüzünden doğrulamada reddedildi (üretime girmedi, ücret yok), aynı girdiyle yeniden gönderildi.
Tuvaller: han masası 144x96 h30 (n=3, axis x), fıçı 80x72 h28, bank 112x80 h20 (n=2, axis x), kulp rafı 96x64 (duvar paralelkenarı 88 px, `/` eğimli, rengi 184,160,122), maşrapa ve sürahi 32x32 (12x6 taban elması, h12 ve h14), ilan tahtası 48x56 (paralelkenar 40 px), sahne 80x72 h24. Duvar kılavuzu betiği `art/tmp409/` altında (commit edilmez).
`get_balance` sonrası: 3980,5 (iş başı 3988,5; bu işin payı 8).

## Dosyalar (Assets/Art/Iso/_aday/)
| Dosya | Piksel | Pivot x | Pivot y (alttan) | Durum |
|---|---|---|---|---|
| iso_item_han_table_3x1_se.png | 144x96 | 72 | 40 | teslim (UYARI) |
| iso_item_han_bench_2x1_se.png | 112x80 | 56 | 32 | teslim |
| iso_wall_item_tankard_rack_2w_sw_ne.png | 96x64 | 48 | 32 (görsel merkezi) | teslim |
| iso_item_tankard_se.png | 32x32 | 16 | ~7 (taban elması merkezi) | teslim |
| iso_item_pewter_jug_se.png | 32x32 | 16 | ~7 | teslim |
| iso_wall_item_notice_board_sw_ne.png | 48x56 | 24 | 28 (görsel merkezi) | teslim (UYARI) |
| iso_item_singer_stand_se.png | 80x72 | 40 | 24 | teslim |
| iso_item_barrel_table_se.png | 80x72 | - | - | TESLİM EDİLMEDİ (`409-red/`) |

## Doğrulama (validate_iso, `--max-colors 48`) ve stil denetimi (`--family esya`)
| Dosya | validate_iso | Renk | Opak sınır | Palete yakın | Doygunluk | Stil |
|---|---|---|---|---|---|---|
| han_table | geçti | 7 | x20-128, y8-88 | %100 | x0,38 | UYARI (soluk) |
| barrel_table (red) | geçti | 38 | x9-70, y3-64 | %100 | x0,80 | GEÇTİ ama şekil tutmadı |
| han_bench | geçti | 11 | x10-102, y3-71 | %100 | x1,06 | GEÇTİ |
| tankard_rack | geçti | 15 | x7-88, y5-58 | %100 | x0,60 | GEÇTİ |
| tankard | geçti | 7 | x12-21, y10-23 | %100 | x1,25 | GEÇTİ |
| pewter_jug | geçti | 7 | x11-21, y7-25 | %100 | x1,17 | GEÇTİ |
| notice_board | geçti | 19 | x5-42, y7-51 | %99 | x1,35 | UYARI (doygun) |
| singer_stand | geçti | 19 | x8-71, y18-64 | %100 | x0,74 | GEÇTİ |
Yarı saydam piksel 0 (hepsinde), alta değme yok. Maske dışı taşma ayrıca ölçülmedi: kılavuz maskesi dışında üretim yapılmaz (araç maskeyle sınırlar).

## Sorunlar
- **Fıçı masa:** fıçı yerine üstü kapaklı, kafesli yan yüzlü kasa/sandık çıktı; fıçı olarak okunmuyor. Tek deneme, yeniden denenmedi. Yeni tarif gerekir (ör. yuvarlak taban elması ile "round barrel with curved staves and two iron hoops").
- **Han masası:** kılavuz 3x1 doldurdu, uzun masa/bank olarak okunuyor, ama mor-gri soluk (x0,38) ve ayaklar bank gibi; "ağır sehpa masa" etkisi zayıf. Bankla neredeyse aynı biçim.
- **Kulp rafı:** ince çıta; üstünde kulp yerine koyu yuvarlak noktalar ve sol uçta rastgele sarı "CX" benzeri işaret var. Kulpların okunurluğu zayıf.
- **Sürahi:** kalay grisi yerine bordo/kahverengi; kapaklı sürahi olarak okunuyor, metal hissi yok.
- **Maşrapa:** küçük, tanınır (kulplu ahşap kupa), boş.
- **İlan tahtası:** tanınır (iki parşömen), doygunluk x1,35.
- Alkol gösterilmedi: maşrapa boş, sürahi kapalı.
- Işık sol-üstten istendi; han masası ve bankta yüzler ayrışmıyor (düz). Döşeme testi eşya için geçerli değil.
- Raporda ışık parlaklık ölçümü yapılmadı.
