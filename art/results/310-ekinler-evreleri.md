unity-import: pending

# 310 Ekin büyüme evreleri (rapor)

Sonuç: **TESLİM YOK**. Üretilen 14 görselin tamamı `style.py check --family bitki` denetiminde KALDI; pırasa 1 ayrıca iki denemede de zemin karosu çıktı. Yan yana sayfası: [310-yanyana.png](310-yanyana.png). Adaylar `art/results/310-red/` altında (Assets'e konmadı). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`, referanssız), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ortak ek ("muted green and earthy colours").
Tuval tarifteki 64x64 yerine **80x72** (bezelye evre 3 hariç) ve **80x80** (bezelye evre 3, yükseklik 40): 1x1 elması 64 px genişlik ve 8 px kenar boşluğu ister, 64 px tuval elmasi kırpıyordu. Pivot: x=40, y(alttan)=24.
Çağrılar: 15 ilk + 4 ikinci deneme (şalgam 2, lahana 1, pırasa 1, pırasa 2) = 19 generation (cost_max 24). Base64 aktarım hatasıyla reddedilen 3 çağrı üretime girmedi, ücret yok. `get_balance` iş sonu: 957,70.

## Sonuç
| Parça | Durum | Not |
|---|---|---|
| şalgam 1 | KALDI | küçük toprak höyüğü çizilmiş ("no soil" tutmadı) |
| şalgam 2 (2. deneme) | KALDI | yaprak halkası ortasında şalgam tepesi, tanınır; palet dışı |
| şalgam 3 | KALDI | tanınır, doygun |
| lahana 1 (2. deneme) | KALDI | ilk deneme kahverengi kabuk; ikincide fazladan kütük nesnesi vardı, bağlı bileşen olarak silindi (yalnız silme) |
| lahana 2, 3 | KALDI | çok doygun (x2,1) |
| pırasa 1 | TESLİM EDİLEMEZ | iki denemede de çim karosu, bitki değil |
| pırasa 2 (2. deneme) | KALDI | koyu bıçaklar, toprak yok |
| pırasa 3 | KALDI | mavi-yeşil, tanınır |
| bezelye 1, 2, 3 | KALDI | 2'de küçük toprak elması; 3 tanınır |
| kabak 1, 2, 3 | KALDI | 3 şişe kabağı tanınır; 2 yalnız çiçekli dal |

## Stil denetimi (`style.py check --family bitki`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| cabbage_1 | %51 | 15.6 | x1.00 | KALDI |
| cabbage_2 | %20 | 40.0 | x2.13 | KALDI |
| cabbage_3 | %69 | 20.4 | x2.08 | KALDI |
| leek_2 | %76 | 13.0 | x1.16 | KALDI |
| leek_3 | %52 | 23.3 | x1.42 | KALDI |
| pea_1 | %59 | 14.9 | x1.05 | KALDI |
| pea_2 | %54 | 23.0 | x2.02 | KALDI |
| pea_3 | %47 | 23.9 | x1.89 | KALDI |
| pumpkin_1 | %26 | 27.1 | x2.12 | KALDI |
| pumpkin_2 | %42 | 24.0 | x1.52 | KALDI |
| pumpkin_3 | %24 | 26.5 | x1.73 | KALDI |
| turnip_1 | %56 | 23.5 | x1.46 | KALDI |
| turnip_2 | %9 | 31.2 | x1.69 | KALDI |
| turnip_3 | %41 | 27.1 | x1.39 | KALDI |

`validate_iso.py` yalnızca turnip_1 ve leek_3 için ayrıca çalıştırıldı (geçti; yarı saydam 0, 25-37 renk, alta değme yok).

## Sorunlar ve öneri
- Bitki ailesinin referansı yalnızca 2 görsel; palet kapısı doygun yeşillerin hepsini eliyor. Kullanıcı yan yana sayfasına bakıp kapıyı gevşetmeli ya da referans eklemeli; sonra görseller `310-red/`'den teslim edilebilir (pırasa 1 hariç).
- Araçta `seed` yok; ikinci denemeler yalnızca yeniden gönderim.
- Evre boyu, döşeme testi ve olgun ürün ikonu kıyası (316) teslim olmadığı için yapılmadı.
