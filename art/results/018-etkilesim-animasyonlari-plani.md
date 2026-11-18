unity-import: pending

# 018 Animasyon planı, 84 px gerçekçi karakter

Durum: review. PixelLab çağrısı yok (0 gen). `get_balance`: 4275,9 kalan (Tier 2, plan `art/budget.md` ile aynı).

## Taban
- erkek `c63fe39d-b168-4732-bf11-b476172c98e8`, kadın `135cb0b1-bd46-4514-8c87-0834b96ef80d` (012b, 84x84, 8 yön PixelLab'de; yeni üretim yok).
- Yöntem: 013'te seçilen pixminimax (1 gen/yön, 8 kare, `keep_first_frame`, end_frame = durma karesi); 018a bunu 84 px'te doğrular.
- 013/014/015 (56 px köylü) bu planla geçersiz; yürüme 018a-b ile yeniden yapılır. Eski dosyalara dokunulmadı.

## Animasyon anahtarları (koddan)
| Kaynak | Anahtar | Animasyon | Tür | Kare | Paylaşım |
|---|---|---|---|---|---|
| ActivityDescriber | walk | walk | döngü | 8 | MoveTo, Mount |
| | idle | idle | döngü | 8 | boşta, hamile dinlenme |
| | chop (Woodcutting) | chop | döngü | 8 | |
| | farm (Farming, AnimalHusbandry; TileAction Till) | hoe | döngü | 8 | Till, SoilCare, Horticulture |
| | (TileAction Sow) | sow | döngü | 8 | |
| | (sulama, IsFetchingWater) | water | döngü | 8 | |
| | cook (Cooking, Herbalism) | cook | döngü | 8 | Baking, Brewing |
| | craft (Carpentry) | craft_hammer | döngü | 8 | Smithing, Masonry, Engineering |
| | craft (Leatherwork, Weaving) | craft_hand | döngü | 8 | Pottery, Chandlery |
| | work (varsayılan), Animal | work | döngü | 8 | Foraging, Literacy... |
| | eat | eat | döngü | 8 | |
| | social (Chat, TellStory) | social | döngü | 8 | Teaching, Music |
| | fun | fun | döngü | 8 | |
| | care (Feed, hamile) | care | döngü | 8 | Parenting, Medicine |
| | love (Propose, Child) | love | döngü | 8 | Charm |
| TileAction | Fish | fish | döngü | 8 | Hunting için de |
| TileAction | Dig, Support, Bail | 368: mine_dig, mine_support, mine_bucket | dig/support 6 tek seferlik, bucket 8 | | |
| | sleep, warm, dead | animasyon yok: yatak/ateş eşyasında poz ya da statik; kod oturumu karar verir | | | |

Tek seferlik (Till, Dig, Support) iş süresine göre döngülenir; kod oturumu karar verir.

## İş tablosu
| İş | İçerik | cost_est / cost_max | Bağımlılık |
|---|---|---|---|
| 018a | yürüme pilotu, erkek SE (style_gate) | 2 / 6 | 012b |
| 018b | yürüme, erkek+kadın, 8 yön | 16 / 24 | 018a |
| 018c | idle, chop, hoe, sow, water (x2 cinsiyet x4 yön) | 40 / 52 | 018a |
| 018d | cook, craft_hammer, craft_hand, work, eat | 40 / 52 | 018a |
| 018e | social, fun, care, love, fish | 40 / 52 | 018a |
| 368 | mine dig/bucket/support (84 px'e güncellendi, pending) | 24 / 36 | 018a |

Toplam tahmin: 162 gen (üst sınır 222), aylık 5000 bütçenin ~%3'ü. `review` bağımlılık için yeterli olduğundan 018a'dan sonra 018b-e ve 368 hemen sıraya girer; takvim günlük tavana (bugün sınırsız) göre bir iki günde biter.

## Notlar ve açık noktalar
- Çocuk ve yaşlı tabanı yok (sonraki karar); bu işler yalnızca yetişkin erkek ve kadın.
- 016/017 katmanlı karakter kararı: animasyon kareleri tek parça gövde üretir; katmanlı sisteme geçilirse katman başına yeniden üretim gerekir. Karar bekleniyor.
- 84 px karakter 56 px'in yerine geçer: `IsoArt` ve ölçek değişikliği kod oturumunun işi.
- 012b'deki oran sorunu (boy/kafa ~3,5) animasyonla düzelmez.
- 84 px'te tuval ve ücret farkı 018a'da görülecek.

## Doğrulama
`queue_ready.py --all` yeni işleri ayrıştırıyor (018a-e, 368), numara çakışması yok, hepsi `cost_max <= 60`.
